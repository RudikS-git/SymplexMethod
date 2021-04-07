using System;
using System.Linq;

namespace LAB_MndMAPS__SymplexMethod
{
    public static class ConsoleSymplex
    {
        public static Coef[] GetSolution(SymplexMatrix symplexMatrix)
        {
            /*if (symplexMatrix.ObjectiveFunction.Striving == Striving.Min)
            {
                symplexMatrix.ObjectiveFunction.SwapStriving();
            }*/

            SymplexTable symplexTable = new SymplexTable(symplexMatrix);
            symplexTable.EventTableChanges += SymplexTableEventChanges;

            Coef[] coefs = symplexTable.GetSolution();

            Console.WriteLine("Решение");
            ShowVector(coefs);
            Console.WriteLine(
                $"Значение функции: {GetValueFun(symplexMatrix.ObjectiveFunction.Coefs.ToArray(), coefs)}");

            var dualCoefs = symplexTable.GetDualSolution();

            Console.WriteLine("Решение двойственной");
            ShowVector(dualCoefs);

            return coefs;
        }

        private static void SymplexTableEventChanges(int iteration, Coef[] coefsFunction, Coef[] coefs)
        {
            ShowVector(iteration, coefs);
            Console.WriteLine($"Значение функции: {GetValueFun(coefsFunction, coefs)}");
        }

        private static void ShowVector(int iteration, Coef[] coefs)
        {
            for (int i = 0; i < coefs.Length; i++)
            {
                Console.Write($"{iteration}) x{coefs[i].Row + 1} - {Math.Round(coefs[i].Value, 2)} ");
            }

            Console.WriteLine(" ");
        }

        private static void ShowVector(Coef[] coefs)
        {
            for (int i = 0; i < coefs.Length; i++)
            {
                Console.Write($"x{coefs[i].Row + 1} - {Math.Round(coefs[i].Value, 2)} ");
            }

            Console.WriteLine(" ");
        }

        private static double GetValueFun(Coef[] coefsFunction, Coef[] coefs)
        {
            double value = 0;
            
             for (int i = 0; i < coefs.Length; i++)
             {
                 if (coefs[i].TypeCoef == TypeCoef.Art)
                 {
                     break;
                 }
                 else if (coefs[i].TypeCoef == TypeCoef.Default)
                 {
                     value += coefsFunction[coefs[i].Row].Value * coefs[i].Value;
                 }
             }
            
            return value;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            double[,] coefs =
            {
                {-2, 1, 1, 0, 1, 0},
                {-1, -2, 0, 1, 3, 0},
                {3, -1, 0, 0, -12, 1}
            };

            double[] basis = new double[] {20, 24, 18};
            Sign[] signs = new Sign[] {Sign.Equal, Sign.Equal, Sign.Equal};

            double[] objFunctionCoefs = new double[] {1, -4, 1, 1, 1, 1};

            Coef[] ansCoefs = null;
            try
            {
                SymplexMatrix symplexMatrix = new SymplexMatrix(new ObjectiveFunction(objFunctionCoefs, Striving.Min),
                    coefs, basis, signs);

                ConsoleSymplex.GetSolution(symplexMatrix);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
