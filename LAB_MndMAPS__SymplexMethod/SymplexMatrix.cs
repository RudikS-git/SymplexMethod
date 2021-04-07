using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_MndMAPS__SymplexMethod
{
    public class SymplexMatrix : Matrix
    {
        public SymplexMatrix(ObjectiveFunction objectiveFunction, double[,] coefs, double[] basis, Sign[] signs)
            : base(coefs, basis, signs)
        {
            ObjectiveFunction = objectiveFunction;
            Variables = new Coef[Coefs.GetLength(0)][];

            ValidateBasis();
            IntroduceBalanceVariable();
            IntroduceArtificialVariable();
        }

        public Coef[][] Variables { get; set; }

        public ObjectiveFunction ObjectiveFunction { get; set; }

        public int GetCountBalance()
        {
            int sum = 0;

            for (int i = 0; i < ObjectiveFunction.Coefs.Count; i++)
            {
                if (ObjectiveFunction.Coefs[i].TypeCoef == TypeCoef.Balance)
                {
                    sum++;
                }
            }

            return sum;
        }

        public int GetCountArtificial()
        {
            int sum = 0;

            for (int i = 0; i < ObjectiveFunction.Coefs.Count; i++)
            {
                if (ObjectiveFunction.Coefs[i].TypeCoef == TypeCoef.Art)
                {
                    sum++;
                }
            }

            return sum;
        }

        private void IntroduceBalanceVariable()
        {
            for (int i = 0; i < Basis.Length; i++)
            {
                Sign sign = GetSign(i);

                if (sign == Sign.LessOrEqual)
                {
                    ObjectiveFunction.Coefs.Add(new Coef()
                    {
                        Value = 1,
                        TypeCoef = TypeCoef.Balance,
                        Row = i
                    });
                }
                else if (sign == Sign.MoreOrEqual)
                {
                    ObjectiveFunction.Coefs.Add(new Coef()
                    {
                        Value = -1,
                        TypeCoef = TypeCoef.Balance,
                        Row = i
                    });
                }
            }
        }

        private void IntroduceArtificialVariable()
        {
            for (int i = 0; i < Basis.Length; i++)
            {
                Sign sign = GetSign(i);

                if (sign == Sign.Equal || sign == Sign.MoreOrEqual)
                {
                    ObjectiveFunction.Coefs.Add(new Coef()
                    {
                        Value = 1,
                        TypeCoef = TypeCoef.Art,
                        Row = i
                    });
                }
            }
        }

        private void ValidateBasis()
        {
            for (int i = 0; i < Basis.Length; i++)
            {
                if (Basis[i] < 0)
                {
                    MultiplyRow(i, -1);
                }
            }
        }
    }
}
