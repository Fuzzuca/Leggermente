using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leggermente.Translator;
using System.IO;

namespace Leggermente
{
    class Program
    {
        static void Main(string[] args)
        {
            //Set console param
            Console.SetBufferSize(300, 180);
            Console.SetWindowSize(100, 30);
            Console.SetWindowPosition(0, 0);

            //Translator work
            Translator.Translator traduttore = new Translator.Translator();

            string file = File.ReadAllText("../../../../Examples/massimo3.lm", Encoding.UTF8);
            string[] package = new string[2];
            package[1] = "../../../../Examples/MATE.lmp";
            package[0] = "../../../../Examples/LEGGERMENTE.lmp";



            ResultCode result = traduttore.Translate(CodeType.Program, file, package, "");
            if (traduttore.ErrorManager.WithOutError) Console.Write(result.ToString());
            else for (int i = 0; i < traduttore.ErrorManager.LogList.Length; i++) Console.WriteLine(traduttore.ErrorManager.LogList[i]);

            Directory.Delete("./temp", true);
            Console.ReadKey();
        }
    }
}
