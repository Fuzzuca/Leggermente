﻿using System;
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
            string[] package = new string[1];
            package[0] = "../../../../Examples/MATE.lmp";



            ResultCode result = traduttore.Translate(CodeType.Program, file, package, "");

            Directory.Delete("./temp", true);
            Console.Write(result.ToString());
            Console.ReadKey();
        }
    }
}