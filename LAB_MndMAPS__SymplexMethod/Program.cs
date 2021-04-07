using System;

namespace LAB_MndMAPS__SymplexMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            double[,] coefs =
            {
                { 3, -4 },
                { 5, 6 },
                { -7, 8 }
            };

            double[] basis = new double[] { 1, 2, 3 };
            Sign[] signs = new Sign[] {Sign.Equal, Sign.LessOrEqual, Sign.MoreOrEqual };

            double[] objFunctionCoefs = new double[] { 1, 2 };

            Symplex.GetSolution(new SymplexMatrix(new ObjectiveFunction(objFunctionCoefs, Striving.Max), coefs, basis, signs));
        }
    }
}
