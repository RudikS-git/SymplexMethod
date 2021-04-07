using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_MndMAPS__SymplexMethod
{
    public enum Sign
    {
        Equal,
        LessOrEqual,
        MoreOrEqual
    }

    public class Matrix
    {
        public double[,] Coefs { get; set; }
        public double[] Basis { get; set; }
        public Sign[] Signs { get; set; }

        public Matrix(double[,] coefs, double[] basis, Sign[] signs)
        {
            Coefs = coefs;
            Basis = basis;
            Signs = signs;
        }

        public Sign GetSign(int row)
        {
            if (Coefs == null || Basis == null)
            {
                throw new ArgumentException("Не задан базис или коэффициенты при переменных");
            }

            if (row < 0 || row >= Coefs.GetLength(0))
            {
                throw new ArgumentException("Строка должна существовать");
            }

            return Signs[row];
        }

        public void SetSign(int row, Sign sign)
        {
            if (Coefs == null || Basis == null)
            {
                throw new ArgumentException("Не задан базис или коэффициенты при переменных");
            }

            if (row < 0 || row >= Coefs.GetLength(0))
            {
                throw new ArgumentException("Строка должна существовать");
            }

            Signs[row] = sign;
        }

        public void MultiplyRow(int row, double number)
        {
            Basis[row] *= number;

            for (int j = 0; j < Coefs.GetLength(1); j++)
            {
                Coefs[row, j] *= number;

                Sign sign = GetSign(row);

                if (sign == Sign.LessOrEqual)
                {
                    SetSign(row, Sign.MoreOrEqual);
                }
                else if (sign == Sign.MoreOrEqual)
                {
                    SetSign(row, Sign.LessOrEqual);
                }
            }
        }
    }
}
