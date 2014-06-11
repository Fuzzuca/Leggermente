using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Leggermente.Translator
{
    public class Translator : ITraslator
    {
        //Campi 
        public LogManager lm;

        //Costruttore
        public Translator(LogManager Log)
        {
            if (Log == null) ErroreCritico(this, DateTime.Now, "The LogManager passed to created the Translator was set to NULL");
            else
            {
                lm = Log;
            }
        }
        public Translator() : this(new LogManager()) { }

        //Distruttore
        ~Translator()
        {
            try
            {
                Directory.Delete("./temp", true);
            }
            catch (Exception) { }
        }

        //Proprità
        public LogManager ErrorManager
        {
            get { return lm; }
            set
            {
                if (value == null) ErroreCritico(this, DateTime.Now, "The LogManager passed to created the Translator was set to NULL");
                else lm = value;
            }
        }

        //Eventi
        public event CriticalErrorHandler ErroreCritico;

        //Metodi pubblici
        public ResultCode Translate(CodeType type, string Code, string[] PackagesPath, string ExoprtPath)
        {
            PackageCollection pc = PackageCollection.LoadPackageByFile(PackagesPath, lm);
            CodeImage code = CodeImage.CreateCodeImage(Code, pc, type, lm);
            ResultCode result = new ResultCode();

            for (int i = 0; i < pc.Count; i++) result.AddInclude(pc[i].Name, lm);
            result.AddBlankLine();
            Parsing(code, result);

            return result;
        }

        //Metodi privati
        public void Parsing(CodeImage ci, ResultCode res)
        {
            ParserFunction parser = new ParserFunction(ci.Constant, ci.Package, res, lm);

            for (int i = 0; i < ci.Section.Count; i++) parser.AnalyzeFunction(ci.Section[i]);
        }
    }
}
 