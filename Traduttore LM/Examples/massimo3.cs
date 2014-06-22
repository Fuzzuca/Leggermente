using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeggerMente.MATE;

namespace TreNumeri 
{
    class Programma 
    {
        static void Main(string[] args)
        {
            var num1 = MATE.random(0, 100);
            var num2 = MATE.random(0, 100);
            var num3 = MATE.random(0, 100);
            Console.WriteLine(num1+"|"+num2+"|"+num3);

            if (num1 > num2)
            {
                if (num1 > num3)
                {
                    Console.Write("Il numero massimo è " + num1.ToString());
                }
                else
                {
                    Console.Write("Il numero massimo è " + num3.ToString());
                }
            }
            else
            {
                if (num2 > num3)
                {
                    Console.Write("Il numero massimo è " + num2.ToString());
                }
                else
                {
                    Console.Write("Il numero massimo è " + num3.ToString());
                }
            }

            /*DEFAULT*/
            Console.ReadKey();
        }
    }
}














namespace LeggerMente.MATE
{
    public static class MATE
    {
        static public Random rm = new Random();

        public static double random(int basso, int alto)
        {
            return rm.Next(basso, alto + 1);
        }
    }
}
