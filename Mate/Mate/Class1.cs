using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leggermente.lib;

namespace Leggermente.lib.MATE
{
    public static class MATE
    {
        private static Random rm = new Random();

        public static Variable resto(Variable var1, Variable var2)
        {
            if (var1.Type == ValueType.Number && var2.Type == ValueType.Number)
            {
                int num1, num2;
                if (int.TryParse(var1.Value, out num1) && int.TryParse(var2.Value, out num2))
                {
                    return new Variable("return", (num1 % num2).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }

        public static Variable random(Variable var1, Variable var2)
        {
            if (var1.Type == ValueType.Number && var2.Type == ValueType.Number)
            {
                int num1, num2;
                if (int.TryParse(var1.Value, out num1) && int.TryParse(var2.Value, out num2))
                {
                    return new Variable("return", (rm.Next(num1, num2 + 1)).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }

        public static Variable sqrt(Variable var1, Variable var2)
        {
            if (var1.Type == ValueType.Number && var2.Type == ValueType.Number)
            {
                int num1, num2;
                if (int.TryParse(var1.Value, out num1) && int.TryParse(var2.Value, out num2))
                {
                    return new Variable("return", (num1 ^ (1 / num2)).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }

        public static Variable exp(Variable var1, Variable var2)
        {
            if (var1.Type == ValueType.Number && var2.Type == ValueType.Number)
            {
                int num1, num2;
                if (int.TryParse(var1.Value, out num1) && int.TryParse(var2.Value, out num2))
                {
                    return new Variable("return", (num1 ^ (num2)).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }
    }
}
