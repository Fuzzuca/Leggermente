using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Leggermente.Translator
{
    public class ParserFunction
    {
        //Campi
        FunctionSection fs;
        FunctionCollection fc;
        PackageCollection pc;
        VariableCollection def;
        VariableCollection vc;
        LogManager lm;
        ResultCode res;

        //Costruttore
        public ParserFunction(VariableCollection Constant, FunctionCollection Functions, PackageCollection Package, ResultCode Code, LogManager Message)
        {
            def = Constant;
            res = Code;
            lm = Message;
            pc = Package;
            fc = Functions;
        }

        public void AnalyzeFunction(FunctionSection fs)
        {
            vc = new VariableCollection();
            this.fs = fs;
            bool findReturn = false;

            res.AddLine(GenerateSign(fs.Function), lm);

            AnalyzeCodeLines(fs.FunctionCodes);

            for (int i = 0; i < fs.FunctionCodes.Count && findReturn == false; i++)
            {
                if (Regex.IsMatch(fs.FunctionCodes[i].Code, @"ritorna", RegexOptions.IgnoreCase)) findReturn = true;
            }
            if (!findReturn && fs.Function.Name.ToLower() != "programma") res.AddLine("\treturn new Variable(\"NULL\",null);", lm);
            

            res.AddLine("}", lm);
        }

        //Metodi privati
        private string GenerateSign(Function f)
        {
            if (f.Name.ToLower() == "programma")
            {
                return "static void Main(string[] args){";
            }
            else
            {
                string r = "static Variable " + f.Name;
                if (f.ParamNum > 0)
                {
                    r += "(";
                    for (int i = 0; i < f.ParamNum; i++) r += "Variable " + f.Parameters[i] + ",";
                    r = r.Remove(r.Length - 1, 1) + ")";
                }
                else r += "()";
                return r + "{";
            }
        }

        private void AnalyzeCodeLines(CodeLineCollection cc)
        {
            if (cc != null && cc.Count > 0)
            {
                if (Regex.IsMatch(cc[0].Code, @"crea (vettore )?[a-z][\w]* [\w\W]+", RegexOptions.IgnoreCase))
                {
                    CreateVariable(cc[0]);
                    AnalyzeCodeLines(cc.Extractor(1));
                }
                else if (Regex.IsMatch(cc[0].Code, @"cambia [a-z][\w]* [\w\W]+", RegexOptions.IgnoreCase))
                {
                    ChangeVariable(cc[0]);
                    AnalyzeCodeLines(cc.Extractor(1));
                }
                else if (Regex.IsMatch(cc[0].Code, @"se [\W\w]+", RegexOptions.IgnoreCase))
                {
                    cc = cc.Extractor(IfAction(cc.ExtractSubIndentation(cc[0].LineNumber, true)));
                    if (Regex.IsMatch(cc[0].Code, @"altrimenti", RegexOptions.IgnoreCase))
                    {
                        AnalyzeCodeLines(cc.Extractor(ElseAction(cc.ExtractSubIndentation(cc[0].LineNumber), cc[0].IndentLevel)));
                    }
                }
                else if (Regex.IsMatch(cc[0].Code, @"controlla [\W\w]+", RegexOptions.IgnoreCase))
                {
					AnalyzeCodeLines(cc.Extractor(SwitchAction(cc.ExtractSubIndentation(cc[0].LineNumber,true))));
                }
                else if (Regex.IsMatch(cc[0].Code, @"ripeti quando [\w\W]+", RegexOptions.IgnoreCase))
                {
                    AnalyzeCodeLines(cc.Extractor(WhileAction(cc.ExtractSubIndentation(cc[0].LineNumber, true))));
                }
                else if (Regex.IsMatch(cc[0].Code, @"ripeti [\w\W]+ volte", RegexOptions.IgnoreCase))
                {
                    AnalyzeCodeLines(cc.Extractor(ForAction(cc.ExtractSubIndentation(cc[0].LineNumber, true))));


                    /*if (Regex.IsMatch(cc[0].Code, @"ripeti [\w\W]+ volte [a-zA-Z][\w]*", RegexOptions.IgnoreCase))
                    {
                    }*/
                }
                else if (Regex.IsMatch(cc[0].Code, @"ripeti", RegexOptions.IgnoreCase))
                {
                    /*if (Regex.IsMatch(cc[0].Code, @"quando [\w\W]+", RegexOptions.IgnoreCase))
                    {
                    }*/
                }
                else if (Regex.IsMatch(cc[0].Code, @"ritorna [\w\W]+", RegexOptions.IgnoreCase))
                {
                    ReturnAction(cc[0]);
                    AnalyzeCodeLines(cc.Extractor(1));
                }
                else if (Regex.IsMatch(cc[0].Code, @"[\w\W]+", RegexOptions.IgnoreCase))
                {
                    FunctionAction(cc[0]);
                    AnalyzeCodeLines(cc.Extractor(1));
                }
                else lm.Add("Nothing is possible to translate at this line", cc[0].LineNumber);
            }
        }
        #region AnalyzeCodeLines
        private void CreateVariable(CodeLine cl)
        {
            cl.Code = cl.Code.Trim();
            string[] ret = cl.Code.Split(' ');
            string code = cl.Code;

            if (Regex.IsMatch(cl.Code, @"crea vettore [a-z][\w]+ [\w\W]+", RegexOptions.IgnoreCase))
            {
                vc.Add(new Variable(ret[2], 0));

                code = code.Remove(0, ret[0].Length + ret[1].Length + ret[2].Length + 2).Trim();
                res.AddLine(Tabul(cl.IndentLevel) + "Variable " + ret[2] + " = new Variable(\"" + ret[2] + "\"," + AnalyzeCode(code, cl.LineNumber) + ");", lm);
            }
            else if (Regex.IsMatch(cl.Code, @"crea [a-z][\w]+ [\w\W]+", RegexOptions.IgnoreCase))
            {
                vc.Add(new Variable(ret[1]));

                code = code.Remove(0, ret[0].Length + ret[1].Length + 1).Trim();
                res.AddLine(Tabul(cl.IndentLevel) + "Variable " + ret[1] + " = new Variable(\"" + ret[1] + "\"," + AnalyzeCode(code, cl.LineNumber) + ");", lm);
            }
            else
            {
                lm.Add("The creation of the variable at the line gone wrong", cl.LineNumber);
            }
        }
        private void ChangeVariable(CodeLine cl)
        {
            cl.Code = cl.Code.Trim();
            string[] sp = cl.Code.Split(' ');
            if (CheckVariable(sp[1]) != null)
            {
                res.AddLine(Tabul(cl.IndentLevel) + sp[1] + ".ChangeValue(" + AnalyzeCode(cl.Code.Substring(sp[0].Length + 1 + sp[1].Length, cl.Code.Length - (sp[0].Length + 1 + sp[1].Length)).Trim(), cl.LineNumber) + ".ToString());", lm);
            }
            else lm.Add("The variable '" + sp[1] + "' not exist", cl.LineNumber);
        }
        private int IfAction(CodeLineCollection cc)
        {
            cc[0].Code = cc[0].Code.Trim();
            int indx = cc[0].Code.IndexOf(' ') + 1;

            res.AddLine(Tabul(cc[0].IndentLevel) + "if(" + AnalyzeCode(cc[0].Code.Substring(indx, cc[0].Code.Length - indx).Trim(), cc[0].LineNumber) + "){", lm);

            AnalyzeCodeLines(cc.Extractor(1));

            res.AddLine(Tabul(cc[0].IndentLevel) + "}", lm);
            return cc.Count;
        }
        private int ElseAction(CodeLineCollection cc, int IndentLevel)
        {
            res.AddLine(Tabul(IndentLevel) + "else{", lm);
            AnalyzeCodeLines(cc);
            res.AddLine(Tabul(IndentLevel) + "}", lm);
            return cc.Count+1;
        }
        private void FunctionAction(CodeLine cl)
        {
            string read = CheckFunction(cl.Code.Trim(), cl.LineNumber);
            if (read != null)
            {
                res.AddLine(Tabul(cl.IndentLevel) + read + ";", lm);
            }
            else lm.Add("Nothing is possible to translate at this line", cl.LineNumber);
        }
        private int SwitchAction(CodeLineCollection cc)
        {
            cc[0].Code = cc[0].Code.Trim();
			int indx = cc[0].Code.IndexOf(' ')+1;
			string name = cc[0].Code.Substring(indx,cc[0].Code.Length-indx);

			CaseAction(cc.Extractor(1),name);

            return cc.Count;
        }
		private void CaseAction(CodeLineCollection cc, string varName)
		{
            if (cc != null && cc.Count > 0)
            {
                cc[0].Code = cc[0].Code.Trim();
                if (Regex.IsMatch(cc[0].Code, @"se [\W\w]+", RegexOptions.IgnoreCase))
                {
                    int indx = cc[0].Code.IndexOf(' ') + 1;

                    res.AddLine(Tabul(cc[0].IndentLevel) + "if(" + AnalyzeCode(varName + " " + cc[0].Code.Substring(indx, cc[0].Code.Length - indx).Trim(),cc[0].LineNumber) + "){", lm);

                    CodeLineCollection sup = cc.ExtractSubIndentation(cc[0].LineNumber);
                    AnalyzeCodeLines(sup);

                    res.AddLine(Tabul(cc[0].IndentLevel) + "}else ", lm);

                    sup = cc.Extractor(sup.Count + 1);
                    if (sup == null || sup.Count <= 0) lm.Add("The translator cannot find the 'Altrimenti' tag");
                    CaseAction(sup, varName);
                }
                else if (Regex.IsMatch(cc[0].Code, @"altrimenti", RegexOptions.IgnoreCase))
                {
                    int indx = cc[0].Code.IndexOf(' ') + 1;

                    res.AddLine(Tabul(cc[0].IndentLevel) + "{", lm);

                    CodeLineCollection sup = cc.ExtractSubIndentation(cc[0].LineNumber);
                    AnalyzeCodeLines(sup);

                    res.AddLine(Tabul(cc[0].IndentLevel) + "}", lm);
                    if (cc.Extractor(sup.Count + 1).Count > 0)
                    {
                        lm.Add("The 'altrimenti' must be the last tag in the switch");
                    }
                }
                else lm.Add("Cannot translate the line in the switch", cc[0].LineNumber);
            }
		}
        private void ReturnAction(CodeLine cl)
        {
            cl.Code = cl.Code.Trim();
            int indx = cl.Code.IndexOf(' ');
            res.AddLine(Tabul(cl.IndentLevel) + "return " + AnalyzeCode(cl.Code.Substring(indx, cl.Code.Length - indx).Trim(), cl.LineNumber) + ";", lm);
        }
        private int WhileAction(CodeLineCollection cc)
        {
            cc[0].Code = cc[0].Code.Trim();
            int indx = Regex.Match(cc[0].Code, "ripeti quando ", RegexOptions.IgnoreCase).Length;

            res.AddLine(Tabul(cc[0].IndentLevel) + "while(" + AnalyzeCode(cc[0].Code.Substring(indx, cc[0].Code.Length - indx), cc[0].LineNumber) + "){", lm);

            AnalyzeCodeLines(cc.Extractor(1));

            res.AddLine(Tabul(cc[0].IndentLevel) + "}", lm);

            return cc.Count;
        }
        private int ForAction(CodeLineCollection cc)
        {
            return 0;
        }
        #endregion

        private string AnalyzeCode(string code, int lineNumber = -1)
        {
            string txt = BracketExtractors(ref code);
            txt = (txt == null) ? "" : AnalyzeCode(txt, lineNumber);

            return "(" + ExpandOperator(code, lineNumber).Replace("(_)", txt) + ")";
        }
        #region AnalyzeCode
        private string BracketExtractors(ref string code)
        {
            int first, last;
            first = code.IndexOf('(') + 1;
            last = code.LastIndexOf(')') + 1;
            if (first > 0 && last > 0)
            {
                string ret = code.Substring(first, last - first - 1);
                code = code.Substring(0, first) + "_" + code.Substring(last - 1, code.Length - last + 1);
                return ret.Trim();
            }
            else return null;
        }
        private string ExpandOperator(string code, int lineNumber = -1)
        {
            Match m = Regex.Match(code, @"[\+\-\/\*]", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index).Trim()) + m.Value + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)).Trim(), lineNumber);
            }
            m = Regex.Match(code, @" maggiore uguale ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index)) + ">=" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            m = Regex.Match(code, @" minore uguale ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index)) + "<=" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            m = Regex.Match(code, @" maggiore ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index)) + ">" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            m = Regex.Match(code, @" minore ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index)) + "<" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            m = Regex.Match(code, @" uguale ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index)) + "==" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            m = Regex.Match(code, @" diverso ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return ExpandOperator(code.Substring(0, m.Index)) + "!=" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            m = Regex.Match(code, @" non ", RegexOptions.IgnoreCase);
            if (m.Success && m.Length > 0)
            {
                return "!" + ExpandOperator(code.Substring(m.Index + m.Value.Length, code.Length - (m.Index + m.Value.Length)), lineNumber);
            }
            return CheckName(code);
        }
        #endregion

        public string CheckName(string Name, int lineNumber = -1)
        {
            string read = CheckVariable(Name);
            if (read != null) return read;
            else
            {
                read = CheckFunction(Name);
                if (read != null) return read;
            }
            lm.Add("The name cannot be resolved", lineNumber);
            return "";
        }
        #region checkName
        public string CheckVariable(string Name)
        {
            if (Name == "(_)") return Name;
            for (int i = 0; i < def.Count; i++) if (def[i].Name == Name) return Name;
            for (int i = 0; i < vc.Count; i++) if (vc[i].Name == Name) return Name;
            /*Futuro controllo Vettori*/
            return null;
        }
        public string CheckFunction(string name, int lineNumber = -1)
        {
            string[] sp = name.Trim().Split(' ', ',');
            string[] nome = sp[0].Trim().Split('.');
            if (nome.Length > 1)
            {
                for (int i = 0; i < pc.Count; i++)
                {
                    if (pc[i].Name == nome[0])
                    {
                        for (int j = 0; j < pc[i].Functions.Count; j++)
                        {
                            if (pc[i].Functions[j].Name == nome[1])
                            {
                                if (pc[i].Functions[j].ParamNum == sp.Length - 1)
                                {
                                    string r = nome[0] + "." + nome[1] + "(";
                                    for (int x = 1; x < sp.Length; x++) r += AnalyzeCode(sp[x]) + ",";
                                    return ((pc[i].Functions[j].ParamNum > 0) ? r.Remove(r.Length - 1, 1) : r) + ")";
                                }
                            }
                        }
                    }
                }
                lm.Add("Cannot find the function " + nome[1] + " in the package " + nome[0], lineNumber);
                return null;
            }
            else
            {
                for (int i = 0; i < fc.Count; i++)
                {
                    if (fc[i].Name == nome[0])
                    {
                        if (fc[i].ParamNum == sp.Length - 1)
                        {
                            string r = nome[0] + "(";
                            for (int x = 1; x < sp.Length; x++) r += AnalyzeCode(sp[x]) + ",";
                            return ((fc[i].ParamNum > 0) ? r.Remove(r.Length - 1, 1) : r) + ")";
                        }
                    }
                }

                int inx = pc.FirstIndexOf("GENERAL");
                for (int i = 0; i < pc[inx].Functions.Count; i++)
                {
                    if (pc[inx].Functions[i].Name == nome[0])
                    {
                        if (pc[inx].Functions[i].ParamNum == sp.Length - 1)
                        {
                            string r = nome[0] + "(";
                            for (int x = 1; x < sp.Length; x++) r += AnalyzeCode(sp[x]) + ",";
                            return ((pc[inx].Functions[i].ParamNum > 0) ? r.Remove(r.Length - 1, 1) : r) + ")";
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        private string Tabul(int number)
        {
            string r = "";
            for (int i = 0; i < number; i++) r += "\t";
            return r;
        }
    }
}
