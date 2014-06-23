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
        private LogManager lm;

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
            ResultCode result = new ResultCode(ExoprtPath);

            if (lm.WithOutError)
            {
                for (int i = 0; i < code.Package.Count; i++) result.AddInclude(code.Package[i].Name, lm);

                int general = pc.FirstIndexOf("GENERAL");
                if (general < 0) lm.Add("Cannot find the main package for the program execution");
                else code.Package.Add(pc[general]);

                result.AddBlankLine();

                if (code.Type == CodeType.Program) result.AddLine("namespace Leggermente.Programma{ class Program{", lm);
                else result.AddLine("namespace Leggermente.lib." + code.PackageName + "{ class " + code.PackageName + "{", lm);

                WriteConstant(code, result);
                if (lm.WithOutError) Parsing(code, result);

                result.AddBlankLine();
                result.AddLine("} }", lm);

                WriteCsDocumets(result);
            }
            return result;
        }

        //Metodi privati
        public void Parsing(CodeImage ci, ResultCode res)
        {
            FunctionCollection fc = new FunctionCollection();
            for (int i = 0; i < ci.Section.Count; i++) fc.Add(ci.Section[i].Function);

            ParserFunction parser = new ParserFunction(ci.Constant, fc, ci.Package, res, lm);
            if (ci.Type == CodeType.Package) WritePackageFile(ci.PackageName, fc, res.FileName);

            for (int i = 0; i < ci.Section.Count; i++) parser.AnalyzeFunction(ci.Section[i]);
        }

        public void WriteConstant(CodeImage ci, ResultCode res)
        {
            VariableCollection vc = ci.Constant;

            res.AddBlankLine();
            for (int i = 0; i < vc.Count; i++)
            {
                res.AddLine("static const int " + vc[i].Name + " = new Variable(" + vc[i].Name + ", " + vc[i].Value + ");", lm);
            }
            res.AddBlankLine();
        }

        public void WritePackageFile(string Name, FunctionCollection fc, string Path)
        {
            int lindx = Path.LastIndexOf(@"\");
            if (lindx < 0) lindx = Path.LastIndexOf("/");

            Package p = new Package(Name, fc);
            if (!Package.SalvaFile(p, Path.Substring(0, lindx + 1) + "lib.bin"))
            {
                lm.Add("Cannot write the package descriptor file");
            }
        }

        public void WriteCsDocumets(ResultCode rc)
        {
            try
            {
                File.WriteAllLines(rc.FileName, rc.Codes);
            }
            catch (Exception) { lm.Add("Cannot write the output file"); }
        }
    }
}
 