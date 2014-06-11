using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leggermente.lib;

namespace Leggermente.lib.MATE
{
    public class MATE
    {
        private static Random rnd = new Random();

        public static Variable random(Variable num1, Variable num2)
        {
            if (num1 != null && num2 != null && num1.Type == ValueType.Number && num2.Type == ValueType.Number)
            {
                int int1, int2;
                if (int.TryParse(num1.Value, out int1) && int.TryParse(num2.Value, out int2))
                {
                    return new Variable("_", (rnd.Next(int1, int2 + 1)).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }

        public static Variable resto(Variable num1, Variable num2)
        {
            if (num1 != null && num2 != null && num1.Type == ValueType.Number && num2.Type == ValueType.Number)
            {
                int int1, int2;
                if (int.TryParse(num1.Value, out int1) && int.TryParse(num2.Value, out int2))
                {
                    return new Variable("_", (int1 % int2).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }

        public static Variable exp(Variable num1, Variable num2)
        {
            if (num1 != null && num2 != null && num1.Type == ValueType.Number && num2.Type == ValueType.Number)
            {
                int int1, int2;
                if (int.TryParse(num1.Value, out int1) && int.TryParse(num2.Value, out int2))
                {
                    return new Variable("_", (int1^int2).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }

        public static Variable sqrt(Variable num1, Variable num2)
        {
            if (num1 != null && num2 != null && num1.Type == ValueType.Number && num2.Type == ValueType.Number)
            {
                int int1, int2;
                if (int.TryParse(num1.Value, out int1) && int.TryParse(num2.Value, out int2))
                {
                    return new Variable("_", (int1 ^ (1 / int2)).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }
    }
}
