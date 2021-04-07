using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LAB_MndMAPS__SymplexMethod
{
    public enum Striving
    {
        Min,
        Max
    };

    public class ObjectiveFunction
    {
        public List<Coef> Coefs { get; set; }
        public Striving Striving { get; set; }

        public ObjectiveFunction(double[] coefs, Striving striving)
        {
            Coefs = new List<Coef>(AddCoefs(coefs, TypeCoef.Default));
            Striving = striving;
        }

        public Coef[] AddCoefs(double[] arrayCoefs, TypeCoef typeCoefs)
        {
            Coef[] coefs = new Coef[arrayCoefs.Length];
            for (int i = 0; i < coefs.Length; i++)
            {
                coefs[i] = new Coef() { TypeCoef = typeCoefs, Value = arrayCoefs[i], Row = i };
            }

            return coefs;
        }

        public Coef GetIndexBasisElement(int row)
        {
            var coefs = Coefs.Where(it => it.Row == row && it.TypeCoef != TypeCoef.Default).ToArray();

            if (coefs.Length == 0)
                return null;

            switch (coefs.Length)
            {
                case 0:
                    return null;
                
                case 1:
                    return coefs[0];

                case 2:
                    return coefs[1];

                default:
                    throw new ArgumentException("Нестандартных переменных не может быть больше 2!");
            }
        }

        public Coef GetBalanse(int row)
        {
            return Coefs.Where(it => it.Row == row && it.TypeCoef == TypeCoef.Balance).FirstOrDefault();
        }
        
        public Coef GetArt(int row)
        {
            return Coefs.Where(it => it.Row == row && it.TypeCoef == TypeCoef.Balance).FirstOrDefault();
        }

        public int GetCountArt()
        {
            return Coefs.Where(it => it.TypeCoef == TypeCoef.Art).Count();
        }

        public void SwapStriving()
        {
            if (Striving == Striving.Max)
                Striving = Striving.Min;
            else Striving = Striving.Max;

            for (int i = 0; i < Coefs.Count; i++)
            {
                Coefs[i].Value *= -1;
            }
        }
    }
}
