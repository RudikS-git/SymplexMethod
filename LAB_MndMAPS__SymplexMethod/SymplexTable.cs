using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LAB_MndMAPS__SymplexMethod
{
    public class SymplexTable
    {
        private SymplexMatrix SymplexMatrix { get; set; }

        private double[,] Table { get; set; }
        private Coef[] CurrentBasis { get; set; } // индексы коэффициентов
        private double[,] LastsymplexDifference { get; set; }

        public delegate void TableChanges(int iteration, Coef[] coefsFunction, Coef[] coefs);
        public event TableChanges EventTableChanges;
        
        public Func<double, double, bool> isValid = null;
        public Func<double[,], int, int> getFirst = null;

        public SymplexTable(SymplexMatrix symplexMatrix)
        {
            SymplexMatrix = symplexMatrix;

            Table = new double[symplexMatrix.Basis.Length,
                                SymplexMatrix.Coefs.GetLength(1) +
                                SymplexMatrix.GetCountBalance() +
                                SymplexMatrix.GetCountArtificial() +
                                1];

            CurrentBasis = new Coef[SymplexMatrix.Basis.Length];

            InitTable();
        }

        /// <summary>
        /// Заполняет таблицу начальными значениями
        /// </summary>
        private void InitTable()
        {
            GetCurrentBasis(); // берет текущий базис

            for (int i = 0; i < SymplexMatrix.Coefs.GetLength(0); i++)
            {
                Table[i, 0] = SymplexMatrix.Basis[i]; // вектор b
                int j = 1;
                
                // A1 ... Ak-1, где A - является стандартной переменной
                for (; j <= SymplexMatrix.Coefs.GetLength(1); j++)
                {
                    Table[i, j] = SymplexMatrix.Coefs[i, j - 1];
                }

                /* Ak ... An, где A является балансовой или искусственной переменной*/
                for (; j < Table.GetLength(1); j++)
                {
                    if (SymplexMatrix.ObjectiveFunction.Coefs[j - 1] == CurrentBasis[i])
                    {
                        Table[i, j] = CurrentBasis[i].Value;
                    }
                    else if (SymplexMatrix.ObjectiveFunction.Coefs[j - 1].Row == i)
                    {
                        Table[i, j] = SymplexMatrix.ObjectiveFunction.Coefs[j - 1].Value;
                    }
                    else
                    {
                        Table[i, j] = 0;
                    }
                    
                }
            }
        }

        public void GetCurrentBasis()
        {
            for (int i = 0; i < CurrentBasis.Length; i++)
            {
                CurrentBasis[i] = SymplexMatrix.ObjectiveFunction.GetIndexBasisElement(i);
            }
        }
        
        private void ChangeBasis(int row, Coef coef)
        {
            CurrentBasis[row] = coef;
        }

        private int GetLeadingRow(int leadingColumn)
        {
            leadingColumn++; // т.к в 0 - вектор b
            
            int leadingRowIndex = -1;
            double leadingRow = Table[0, 0] / Table[0, leadingColumn];

            for (int i = 0; i < CurrentBasis.Length - 1; i++)
            {
                if (Table[i, leadingColumn] > 0 && Table[i+1, leadingColumn] > 0)
                {
                    double nextLeadingRow = Table[i + 1, 0] / Table[i + 1, leadingColumn];

                    if (leadingRow > nextLeadingRow)
                    {
                        leadingRow = nextLeadingRow;
                        leadingRowIndex = i;
                    }
                }
                else if(Table[i, leadingColumn] > 0 && Table[i + 1, leadingColumn] <= 0)
                {
                    leadingRow = Table[i, 0] / Table[i, leadingColumn];
                    leadingRowIndex = i;
                }
                else if (Table[i, leadingColumn] <= 0 && Table[i + 1, leadingColumn] > 0)
                {
                    leadingRow = Table[i + 1, 0] / Table[i + 1, leadingColumn];
                    leadingRowIndex = i + 1;
                }

            }

            return leadingRowIndex;
        }

        private bool IsBasisHasArt()
        {
            for (int i = 0; i < CurrentBasis.Length; i++)
            {
                if (CurrentBasis[i].TypeCoef == TypeCoef.Art)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetLeadingColumn(double [,] symplexDifference)
        {
            // наименьший отрицательный
            // если нету, то завершаем

            int countArt = SymplexMatrix.ObjectiveFunction.GetCountArt();
            int index = -1;

            if (IsBasisHasArt()) // в базисе присутствуют искусственные элементы
            {
                index = getFirst(symplexDifference, 1);
                
                for (int i = 0; i < SymplexMatrix.ObjectiveFunction.Coefs.Count - countArt - 1; i++)
                {
                    if(isValid(symplexDifference[i, 1], symplexDifference[i + 1, 1]))
                    {
                        if (symplexDifference[i, 1] > symplexDifference[i + 1, 1])
                        {
                            index = i + 1;
                        }
                    }
                }

            }
            else
            {
                index = getFirst(symplexDifference, 0);
                
                for (int i = 0; i < SymplexMatrix.ObjectiveFunction.Coefs.Count - countArt - 1; i++)
                {
                    if (isValid(symplexDifference[i, 0], symplexDifference[i + 1, 0]))
                    {
                        if (symplexDifference[i, 0] > symplexDifference[i + 1, 0])
                        {
                            index = i + 1;
                        }
                    }
                }
            }
 
            return index;
        }

        private int GetFirstNegValueIndex(double [,] array, int col)
        {
            for(int i = 0; i < array.Length; i++)
            {
                if (array[i, col] < 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetFirstPosValueIndex(double[,] array, int col)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i, col] > 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private void MultiplyRow(double[,] table, int row, double number)
        {
            for(int i = 0; i < table.GetLength(1); i++)
            {
                table[row, i] *= number;
            }    
        }

        public Coef[] GetDualSolution()
        {
            Coef[] coefs = new Coef[SymplexMatrix.GetCountBalance()];
            
            for (int i = SymplexMatrix.ObjectiveFunction.Coefs.Count - SymplexMatrix.GetCountArtificial() - SymplexMatrix.GetCountBalance(), j = 0; i < SymplexMatrix.GetCountBalance(); i++, j++)
            {
                coefs[j].Value = LastsymplexDifference[i, 0];
                coefs[j].TypeCoef = TypeCoef.Balance;
            }

            return coefs;
        }
        
        public Coef[] GetSolution()
        {
            if (SymplexMatrix.ObjectiveFunction.Striving == Striving.Max)
            {
                isValid = (x, y) => (x < 0 && y < 0);
                getFirst = GetFirstNegValueIndex;
            }
            else
            {
                isValid = (x, y) => (x > 0 && y > 0);
                getFirst = GetFirstPosValueIndex;
            }
            
            int iteration = 0;
            double[,] symplexDifference = null;
            
            while (true)
            {
                double[] z = new double[SymplexMatrix.ObjectiveFunction.Coefs.Count];

                z[0] = ScalarVectorB(); // скалярное произведение
                symplexDifference = ScalarVectorsA();
                int leadingColumn = GetLeadingColumn(symplexDifference);

                if (leadingColumn == -1) // в сумме нет отриц элементов, значит решение является оптимальным
                {
                    // вставить дополнительную проверку на наличие в базисе искусственных переменных
                    // если в базисе присутствуют, то решения нет
                    // завершаем

                    if (IsBasisHasArt())
                    {
                        throw new ArgumentException(
                            "В строке симплексных разностей отсутсвуют отрицательные значения, но в базисе все еще есть искусственные переменные\n" +
                            "Решения не существует!");
                    }
                    
                    break;
                }

                int leadingRow = GetLeadingRow(leadingColumn);

                if (leadingRow == -1)
                {
                    throw new ArgumentException("Отсутствуют положительные элементы в столбце. Решения не существует!");
                }

                // рассчитываем новую таблицу
                CalculateNewTable(leadingRow, leadingColumn + 1);

                // заносим в базис другой элемент, который относится к ведущему столбцу
                ChangeBasis(leadingRow, SymplexMatrix.ObjectiveFunction.Coefs[leadingColumn]);
                iteration++;

                EventTableChanges?.Invoke(iteration, SymplexMatrix.ObjectiveFunction.Coefs.ToArray(), ShapeOutputCoefs());
            }

            LastsymplexDifference = symplexDifference;

            return ShapeOutputCoefs();
        }

        private Coef[] ShapeOutputCoefs()
        {
            Coef[] coef = new Coef[CurrentBasis.Length];

            for (int i = 0; i < CurrentBasis.Length; i++)
            {
                coef[i] = new Coef()
                {
                    Row = GetIndexCoef(CurrentBasis[i]),
                    TypeCoef = CurrentBasis[i].TypeCoef,
                    Value = Table[i, 0]
                };
            }

            return coef;
        }

        private int GetIndexCoef(Coef coef)
        {
            for (int i = 0; i < SymplexMatrix.ObjectiveFunction.Coefs.Count; i++)
            {
                if (coef == SymplexMatrix.ObjectiveFunction.Coefs[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private void CalculateNewTable(int leadingRow, int leadingColumn)
        {
            double[,] newTable = new double[Table.GetLength(0), Table.GetLength(1)];
            double v = Table[leadingRow, leadingColumn]; // ведущий элемент

            for (int i = 0; i < Table.GetLength(0); i++)
            {
                if (leadingRow == i)
                {
                    // копируем строку из старой таблицы
                    for (int j = 0; j < Table.GetLength(1); j++)
                    {
                        newTable[i, j] = Table[i, j];
                    }

                    if (newTable[leadingRow, leadingColumn] != 1)
                    {
                        // делим на ведущий элемент всю строку, если он не равен единице
                        MultiplyRow(newTable, leadingRow, 1 / newTable[leadingRow, leadingColumn]);
                    }
                    
                    continue;
                }
                
                for (int j = 0; j < Table.GetLength(1); j++)
                {
                    newTable[i, j] = Table[i, j] - (Table[leadingRow, j] * Table[i, leadingColumn]) / v;
                }
            }

            Table = newTable;
        }    

        private double ScalarVectorB()
        {
            double answer = 0;

            for (int i = 0; i < CurrentBasis.Length; i++)
            {
                double k = 0;
                if (CurrentBasis[i].TypeCoef == TypeCoef.Art)
                {
                    k = double.NegativeInfinity;
                }
                else
                {
                    k = CurrentBasis[i].Value;
                }

                answer += k * Table[i, 0];
            }
            
            return answer;
        }

        private double[,] ScalarVectorsA()
        {
            // 0 - свободный, 1 - коэффициент при -M
            double [,] symplexDifference = new double [SymplexMatrix.ObjectiveFunction.Coefs.Count, 2];

            for (int i = 0; i < SymplexMatrix.ObjectiveFunction.Coefs.Count; i++)
            {
                for (int j = 0; j < CurrentBasis.Length; j++)
                {
                    if (CurrentBasis[j].TypeCoef == TypeCoef.Art)
                    {
                        if (SymplexMatrix.ObjectiveFunction.Striving == Striving.Max)
                        {
                            symplexDifference[i, 1] += Table[j, i + 1] * -1;
                        }
                        else
                        {
                            symplexDifference[i, 1] += Table[j, i + 1];
                        }
                    }
                    else if (CurrentBasis[j].TypeCoef == TypeCoef.Balance)
                    {
                        symplexDifference[i, 1] += Table[j, i + 1] * 0;
                    }
                    else
                    { 
                        symplexDifference[i, 0] += Table[j, i + 1] * CurrentBasis[j].Value;
                    }
                }

                if (SymplexMatrix.ObjectiveFunction.Coefs[i].TypeCoef == TypeCoef.Art)
                {
                    symplexDifference[i, 1] -= -1;
                }
                else if (SymplexMatrix.ObjectiveFunction.Coefs[i].TypeCoef == TypeCoef.Default)
                {
                    symplexDifference[i, 0] -= SymplexMatrix.ObjectiveFunction.Coefs[i].Value;
                }

            }
            
            return symplexDifference;
        }
    }
}
