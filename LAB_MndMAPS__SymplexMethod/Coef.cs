using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_MndMAPS__SymplexMethod
{
    public enum TypeCoef
    {
        Default,
        Balance,
        Art
    }

    public class Coef : ICloneable
    {
        public TypeCoef TypeCoef { get; set; }
        public double Value { get; set; }
        public int Row { get; set; }

        public static bool operator !=(Coef coef1, Coef coef2)
        {
            return !(coef1 == coef2);
        }

        public static bool operator == (Coef coef1, Coef coef2)
        {
            if ((object)coef1 == null && (object)coef2 == null)
                return true;

            if ((object) coef1 != null && (object) coef2 != null)
            {
                if (coef1.Row == coef2.Row &&
                    coef1.TypeCoef == coef2.TypeCoef &&
                    coef1.Value == coef2.Value)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public object Clone()
        {
            return new Coef() {TypeCoef = TypeCoef, Value = Value, Row = Row};
        }
    }
}
