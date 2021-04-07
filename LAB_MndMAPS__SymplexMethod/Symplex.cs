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
                else if(Table[i, leadingColumn] > 0 && Table[i + 1, leadingColumn] < 0)
                {
                    leadingRow = Table[i, 0] / Table[i, leadingColumn];
                    leadingRowIndex = i;
                }
                else if (Table[i, leadingColumn] < 0 && Table[i + 1, leadingColumn] > 0)
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
                for (int i = 0; i < SymplexMatrix.ObjectiveFunction.Coefs.Count - countArt - 1; i++)
                {
                    if(symplexDifference[i, 1] < 0 && symplexDifference[i + 1, 1] < 0)
                    {
                        if (symplexDifference[i, 1] > symplexDifference[i + 1, 1])
                        {
                            index = i + 1;
                        }
                    }
                    else if (symplexDifference[i, 1] < 0 && symplexDifference[i + 1, 1] > 0)
                    {
                        index = i;
                    }
                    else if (symplexDifference[i, 1] > 0 && symplexDifference[i + 1, 1] < 0)
                    {
                        index = i + 1;
                    }
                }

            }
            else
            {
                for (int i = 0; i < SymplexMatrix.ObjectiveFunction.Coefs.Count - countArt - 1; i++)
                {
                    if (symplexDifference[i, 0] < 0 && symplexDifference[i + 1, 0] < 0)
                    {
                        if (symplexDifference[i, 0] > symplexDifference[i + 1, 0])
                        {
                            index = i + 1;
                        }
                    }
                    else if (symplexDifference[i, 0] < 0 && symplexDifference[i + 1, 0] > 0)
                    {
                        index = i;
                    }
                    else if (symplexDifference[i, 0] > 0 && symplexDifference[i + 1, 0] < 0)
                    {
                        index = i + 1;
                    }
                }
            }
 
            return index;
        }

        private void MultiplyRow(double[,] table, int row, double number)
        {
            for(int i = 0; i < table.GetLength(1); i++)
            {
                table[row, i] *= number;
            }    
        }
        
        public double[] GetSolution()
        {
            while (true)
            {
                double[] z = new double[SymplexMatrix.ObjectiveFunction.Coefs.Count];

                z[0] = ScalarVectorB(); // скалярное произведение
                var symplexDfference = ScalarVectorsA();
                int leadingColumn = GetLeadingColumn(symplexDfference);

                if (leadingColumn == -1) // в сумме нет отриц элементов, значит решение является оптимальным
                {
                    // вставить дополнительную проверку на наличие в базисе искусственных переменных
                    // если в базисе присутствуют, то решения нет
                    // завершаем
                    break;
                }

                int leadingRow = GetLeadingRow(leadingColumn);

                // рассчитываем новую таблицу
                CalculateNewTable(leadingRow, leadingColumn + 1);

                // заносим в базис другой элемент, который относится к ведущему столбцу
                ChangeBasis(leadingRow, SymplexMatrix.ObjectiveFunction.Coefs[leadingColumn]);
            }


            return new double[5];
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
                        symplexDifference[i, 1] += Table[j, i + 1] * -1;
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

    public static class Symplex
    {
        public static void GetSolution(SymplexMatrix symplexMatrix)
        {
            if (symplexMatrix.ObjectiveFunction.Striving == Striving.Min)
            {
                symplexMatrix.ObjectiveFunction.SwapStriving();
            }

            SymplexTable symplexTable = new SymplexTable(symplexMatrix);
            symplexTable.GetSolution();
        }

    }
}
