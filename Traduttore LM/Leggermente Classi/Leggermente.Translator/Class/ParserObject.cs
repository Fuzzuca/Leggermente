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
        PackageCollection pc;
        VariableCollection def;
        VariableCollection vc;
        LogManager lm;
        ResultCode res;

        //Costruttore
        public ParserFunction(VariableCollection Constant, PackageCollection Package, ResultCode Code, LogManager Message)
        {
            def = Constant;
            res = Code;
            lm = Message;
            pc = Package;
        }

        //Metodi pubblici
        public string[] AnalyzeFunction(FunctionSection fs)
        {
            vc = new VariableCollection();
            this.fs = fs;

            res.AddLine(GenerateSign(fs.Function), lm);

            AnalyzeCodeline(fs.FunctionCodes);
            Variable v = new Variable("NULL",null);
            if (!CheckIfReturn(fs.FunctionCodes)) res.AddLine("\treturn new Variable(\"NULL\",null);", lm);
            res.AddLine("}", lm);
            return null;
        }

        private void AnalyzeCodeline(CodeLineCollection cls)
        {
            if (cls != null && cls.Count > 0)
            {
                if (Regex.IsMatch(cls[0].Code, @"crea (vettore )?[a-z][\w]* [\w\W]+", RegexOptions.IgnoreCase))
                {
                    res.AddLine(GetIndent(cls[0].IndentLevel) + CreateVariable(cls[0]), lm);
                    AnalyzeCodeline(cls.Extractor(1));
                }
                else if (Regex.IsMatch(cls[0].Code, @"cambia [a-z][\w]* [\w\W]+", RegexOptions.IgnoreCase))
                {
                    res.AddLine(GetIndent(cls[0].IndentLevel) + ChangeVariable(cls[0]), lm);
                    AnalyzeCodeline(cls.Extractor(1));
                }
                else if (Regex.IsMatch(cls[0].Code, @"se [\W\w]+", RegexOptions.IgnoreCase))
                {
                    int LineCount = 0;
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "if(" + CodeOperation(cls[0].Code.Remove(0, cls[0].Code.Split(' ')[0].Length).Trim(), cls[0].LineNumber) + "){", lm);
                    CodeLineCollection clSe = cls.ExtractSubIndentation(cls[0].LineNumber);
                    LineCount += clSe.Count + 1;
                    AnalyzeCodeline(clSe);

                    if (Regex.IsMatch(cls[LineCount].Code, @"altrimenti", RegexOptions.IgnoreCase))
                    {
                        res.AddLine(GetIndent(cls[LineCount].IndentLevel) + "}else{", lm);
                        CodeLineCollection clElse = cls.ExtractSubIndentation(cls[LineCount].LineNumber);
                        LineCount += clElse.Count + 1;
                        AnalyzeCodeline(clElse);
                    }
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "}", lm);
                    AnalyzeCodeline(cls.Extractor(LineCount));
                }
                else if (Regex.IsMatch(cls[0].Code, @"controlla [\W\w]+", RegexOptions.IgnoreCase))
                {
                    CodeLineCollection clCont = cls.ExtractSubIndentation(cls[0].LineNumber);
                    string varName = cls[0].Code.Remove(0, cls[0].Code.Split(' ')[0].Length).Trim();
                    ControllaOperation(clCont, varName);
                }
                else if (Regex.IsMatch(cls[0].Code, @"ripeti quando [\w\W]+", RegexOptions.IgnoreCase)) //while
                {
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "while(" + CodeOperation(cls[0].Code.Remove(0, cls[0].Code.Split(' ')[0].Length + cls[0].Code.Split(' ')[1].Length + 1).Trim(),cls[0].LineNumber) + "){", lm);
                    CodeLineCollection clr = cls.ExtractSubIndentation(cls[0].LineNumber);
                    AnalyzeCodeline(clr);
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "}", lm);
                    cls = cls.Extractor(clr.Count + 1);
                    AnalyzeCodeline(cls);
                }
                else if (Regex.IsMatch(cls[0].Code, @"ripeti [\w\W]+ volte", RegexOptions.IgnoreCase))  //for
                {
                    string code="";
                    string name="indice";
                    int iStart = cls[0].Code.Split(' ')[0].Length;
                    int iName = cls[0].Code.IndexOf("volte");

                    code = cls[0].Code.Substring(iStart, iName-iStart).Trim();
                    if (Regex.IsMatch(cls[0].Code, @"ripeti [\w\W]+ volte [_a-zA-Z][\w]*", RegexOptions.IgnoreCase))
                    {

                        name = cls[0].Code.Substring(iName + "volte".Length, cls[0].Code.Length - (iName + "volte".Length)).Trim();
                    }
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "for(int " + name + "=0 " + "; " + name + "<" + CodeOperation(code, cls[0].LineNumber) + " ;" + name + "++ ){", lm);
                    CodeLineCollection clr = cls.ExtractSubIndentation(cls[0].LineNumber);
                    AnalyzeCodeline(clr);
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "}", lm);
                    cls = cls.Extractor(clr.Count + 1);
                    AnalyzeCodeline(cls);
                }
                else if (Regex.IsMatch(cls[0].Code, @"ripeti", RegexOptions.IgnoreCase))    //do-while
                {
                    res.AddLine(GetIndent(cls[0].IndentLevel)+"do{", lm);
                    CodeLineCollection clr = cls.ExtractSubIndentation(cls[0].LineNumber);
                    AnalyzeCodeline(clr);
                    cls = cls.Extractor(clr.Count + 1);
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "}while(" + CodeOperation(cls[0].Code.Remove(0, cls[0].Code.Split(' ')[0].Length).Trim(), cls[0].LineNumber) + ");", lm);
                    cls = cls.Extractor(1);
                    AnalyzeCodeline(cls);
                }
                else if (Regex.IsMatch(cls[0].Code, @"ritorna [\w\W]+", RegexOptions.IgnoreCase))
                {
                    res.AddLine(GetIndent(cls[0].IndentLevel) + "return " + CodeOperation(cls[0].Code.Remove(0, cls[0].Code.Split(' ')[0].Length).Trim(), cls[0].LineNumber) + ";", lm);
                }
                /*else if (Regex.IsMatch(cls[0].Code, @"[\w\W]+ ", RegexOptions.IgnoreCase))
                {
                }*/
                else
                {
                    //rattoppo VAR e IO
                    res.AddLine(GetIndent(cls[0].IndentLevel) + cls[0].Code.Split(' ')[0] + CodeOperation(cls[0].Code.Remove(0, cls[0].Code.Split(' ')[0].Length).Trim(),cls[0].LineNumber) + ";", lm);
                    AnalyzeCodeline(cls.Extractor(1));
                }
            }
        }

        private void SingleFunctionCall(CodeLine cl)
        {
            string[] txt = cl.Code.Split(' ');
        }

        private void ControllaOperation(CodeLineCollection cl, string varName)
        {
            if (cl != null && cl.Count > 0)
            {
                if (Regex.IsMatch(cl[0].Code, @"se [\W\w]+", RegexOptions.IgnoreCase))
                {
                    res.AddLine(GetIndent(cl[0].IndentLevel) + "if(" + CodeOperation(varName + " " + cl[0].Code.Remove(0, cl[0].Code.Split(' ')[0].Length).Trim(), cl[0].LineNumber) + "){", lm);
                    CodeLineCollection clExt = cl.ExtractSubIndentation(cl[0].LineNumber);
                    AnalyzeCodeline(clExt);

                    res.AddLine(GetIndent(cl[0].IndentLevel) + "}else ", lm);
                    cl = cl.Extractor(clExt.Count + 1);

                    ControllaOperation(cl, varName);
                }
                else if (Regex.IsMatch(cl[0].Code, @"altrimenti", RegexOptions.IgnoreCase))
                {
                    res.AddLine(GetIndent(cl[0].IndentLevel)+"{", lm);
                    CodeLineCollection clExt = cl.ExtractSubIndentation(cl[0].LineNumber);
                    AnalyzeCodeline(clExt);

                    res.AddLine(GetIndent(cl[0].IndentLevel) + "}", lm);
                    cl = cl.Extractor(clExt.Count + 1);
                } 
            }
        }

        private string GenerateSign(Function fs)
        {
            if (string.IsNullOrWhiteSpace(fs.Name)) lm.Add("There are a function whitout name");
            else
            {
                string nome = (fs.Name.ToLower() == "programma") ? "main" : fs.Name;
                string functionSign = "public static Variable " + nome + "(";
                for (int i = 0; i < fs.Parameters.Length; i++) functionSign += "Variable " + fs.Parameters[i] + ((i + 1 != fs.Parameters.Length) ? ", " : "");
                return functionSign += "){";
            }
            return "";
        }

        private string CodeOperation(string code, int lineNum = -1)
        {
            string ret = "";
            string extTxt = ExtractBrackets(ref code);
            extTxt = (extTxt != null) ? CodeOperation(extTxt, lineNum) : "";

            ret += AnalyzeOperator(code, lineNum);
            return "(" + ret.Replace("|", extTxt) + ")";
        }
        #region CodeOperation
        private string AnalyzeOperator(string code, int lineNumber)
        {
            Match m, n;
            bool exit;
            string codice = code, ret = "";
            int precIndex = 0;

            do
            {
                exit = true;
                m = Regex.Match(codice, @"[\+\-\*\/]", RegexOptions.None);
                if (m.Success)
                {
                    ret += FunctionOrVariable(codice.Substring(precIndex, m.Index), lineNumber);
                    ret += m.Value;
                    codice = codice.Substring(m.Index + m.Length, codice.Length - (m.Index + m.Length));
                    exit = false;
                }
                m = Regex.Match(codice, @" maggiore ", RegexOptions.None);
                if (m.Success)
                {
                    n = Regex.Match(codice, @" maggiore uguale ", RegexOptions.None);
                    if (n.Success)
                    {
                        ret += FunctionOrVariable(codice.Substring(precIndex, n.Index), lineNumber);
                        ret += ">=";
                        codice = codice.Substring(n.Index + n.Length, codice.Length - (n.Index + n.Length));
                        exit = false;
                    }
                    else
                    {
                        ret += FunctionOrVariable(codice.Substring(precIndex, m.Index), lineNumber);
                        ret += ">";
                        codice = codice.Substring(m.Index + m.Length, codice.Length - (m.Index + m.Length));
                        exit = false;
                    }
                }
                m = Regex.Match(codice, @" minore ", RegexOptions.None);
                if (m.Success)
                {
                    n = Regex.Match(codice, @" minore  uguale ", RegexOptions.None);
                    if (n.Success)
                    {
                        ret += FunctionOrVariable(codice.Substring(precIndex, n.Index), lineNumber);
                        ret += "<=";
                        codice = codice.Substring(n.Index + n.Length, codice.Length - (n.Index + n.Length));
                        exit = false;
                    }
                    else
                    {
                        ret += FunctionOrVariable(codice.Substring(precIndex, m.Index), lineNumber);
                        ret += "<";
                        codice = codice.Substring(m.Index + m.Length, codice.Length - (m.Index + m.Length));
                        exit = false;
                    }
                }
                m = Regex.Match(codice, @" uguale ", RegexOptions.None);
                if (m.Success)
                {
                    ret += FunctionOrVariable(codice.Substring(precIndex, m.Index), lineNumber);
                    ret += "==";
                    codice = codice.Substring(m.Index + m.Length, codice.Length - (m.Index + m.Length));
                    exit = false;
                }
                m = Regex.Match(codice, @" diverso ", RegexOptions.None);
                if (m.Success)
                {
                    ret += FunctionOrVariable(codice.Substring(precIndex, m.Index), lineNumber);
                    ret += "!=";
                    codice = codice.Substring(m.Index + m.Length, codice.Length - (m.Index + m.Length));
                    exit = false;
                }
                m = Regex.Match(codice, @" non ", RegexOptions.None);
                if (m.Success)
                {
                    ret += "!";
                    ret += FunctionOrVariable(codice.Substring(precIndex, m.Index), lineNumber);
                    codice = codice.Substring(m.Index + m.Length, codice.Length - (m.Index + m.Length));
                    exit = false;
                }
            } while (!exit);

            ret += FunctionOrVariable(codice.Substring(0, codice.Length), lineNumber);

            return ret;
        }
        private string ExtractBrackets(ref string code)
        {
            int indx = code.IndexOf('(');
            int lght = code.LastIndexOf(')') - indx;

            if (indx >= 0 && lght >= 0)
            {
                string ret = code.Substring(indx + 1, lght - 1);
                code = code.Replace("(" + ret + ")", "|");
                return ret;
            }

            return null;
        }
        private string FunctionOrVariable(string name, int lineNumber)
        {
            Match m = Regex.Match(name, @"[\w]+\.[\w]+ [\w]*(, ?[\w]+)*", RegexOptions.None);
            if (m.Success && m.Length > 0)
            {
                return GenerateCSharpSign(name, lineNumber);
            }
            if (name == "|") return "|";
            VariableType type = CheckVariableName(name);
            if (type == VariableType.Variable) return name;
            else if (type == VariableType.Constant) return name;
            else
            {
                lm.Add("The variable name '" + name + "' not exist", lineNumber);
                return "";
            }
        }
        private VariableType CheckVariableName(string name)
        {
            for (int i = 0; i < def.Count; i++)
            {
                if (def[i].Name == name)
                {
                    return VariableType.Constant;
                }
            }
            for (int i = 0; i < vc.Count; i++)
            {
                if (vc[i].Name == name)
                {
                    return VariableType.Variable;
                }
            }
            return VariableType.Null;
        }
        private bool CheckFunctionName(string[] nameSplitted, int lineNumber)
        {
            for (int i = 0; i < pc.Count; i++)
            {
                if (pc[i].Name == nameSplitted[0])
                {
                    for (int j = 0; j < pc[i].Functions.Count; j++)
                    {
                        if (pc[i].Functions[j].Name == nameSplitted[1])
                        {
                            return (pc[i].Functions[j].ParamNum == nameSplitted.Length - 2);
                        }
                    }
                    lm.Add("Function named '" + nameSplitted[1] + "' not found", lineNumber);
                    return false;
                }
            }
            lm.Add("Package named '" + nameSplitted[0] + "' not found", lineNumber);
            return false;
        }
        private string GenerateCSharpSign(string code, int lineNuber)
        {
            string[] part = code.Split('.', ' ', ',');
            if (CheckFunctionName(part, lineNuber))
            {
                string ret = "Leggermente.lib." + part[0] + "." + part[0] + "." + part[1] + "(";
                for (int i = 2; i < part.Length; i++) ret += FunctionOrVariable(part[i], lineNuber) + ",";
                return ret.Remove(ret.Length - 1, 1) + ")";
            }
            else
            {
                lm.Add("The function name '" + part[0] + "." + part[1] + "' not exist", lineNuber);
                return "";
            }
        }
        #endregion

        private string CreateVariable(CodeLine cl)
        {
            string[] ret;
            string code = cl.Code;
            if (Regex.IsMatch(cl.Code, @"crea vettore [_a-z][\w]+ [\w\W]+", RegexOptions.IgnoreCase))
            {
                ret = cl.Code.Split(' ');
                vc.Add(new Variable(ret[2], 0));
                code = code.Remove(0, ret[0].Length + ret[1].Length + ret[2].Length + 2).Trim();
                return "Variable " + ret[2] + " = new Variable(\"" + ret[2] + "\"," + CodeOperation(code, cl.LineNumber) + ");";
            }
            else if (Regex.IsMatch(cl.Code, @"crea [_a-z][\w]+ [\w\W]+", RegexOptions.IgnoreCase))
            {
                ret = cl.Code.Split(' ');
                vc.Add(new Variable(ret[1]));
                code = code.Remove(0, ret[0].Length + ret[1].Length + 1).Trim();
                return "Variable " + ret[1] + " = new Variable(\"" + ret[1] + "\"," + CodeOperation(code, cl.LineNumber) + ");";
            }
            else
            {
                lm.Add("The creation of the variable at the line gone wrong", cl.LineNumber);
                return "";
            }
        }

        private string ChangeVariable(CodeLine cl)
        {
            string[] ret;
            ret = cl.Code.Split(' ');
            if (VariableType.Null != CheckVariableName(ret[1]))
            {
                string code = cl.Code.Remove(0, ret[0].Length + ret[1].Length + 1).Trim();
                return ret[1] + ".ChangeValue(" + CodeOperation(code, cl.LineNumber) + ".ToString());";
            }
            else
            {
                lm.Add("The variable '" + ret[1] + "' not exist", cl.LineNumber);
                return "";
            }
        }

        private string GetIndent(int Tabulation)
        {
            string ret = "";
            for (int i = 0; i < Tabulation; i++) ret += "\t";
            return ret;
        }

        private bool CheckIfReturn(CodeLineCollection cc)
        {
            for (int i = 0; i < cc.Count; i++)
            {
                if (Regex.IsMatch(cc[i].Code, "ritorna", RegexOptions.IgnoreCase)) return true;
            }
            return false;
        }


    }

    public enum VariableType
    {
        Variable,
        Constant,
        Null
    }
}
