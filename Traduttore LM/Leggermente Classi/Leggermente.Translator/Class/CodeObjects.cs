using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Leggermente.Translator
{
    #region Funzioni
    /// <summary>
    /// Classe che descrive una funzione con i suoi parametri
    /// </summary>
    [Serializable]
    public class Function
    {
        //Campi
        private string name;
        private string[] param;
        private int startPos;

        //Costruttore
        /// <summary>
        /// Classe che descrive una funzione con i suoi parametri
        /// </summary>
        /// <param name="Name">Nome della funzione</param>
        /// <param name="Parameters">Nomi dei parametri</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <param name="StartPosition">Posizione di partenza nel codice, oppure -1</param>
        private Function(string Name, string[] Parameters, int StartPosition, LogManager lm)
        {
            bool check = true;
            startPos = (StartPosition < 0) ? -1 : StartPosition;

            Name = CodeCleaner.RemoveWhiteSpace(Name, lm);

            if (CodeCleaner.CheckNameIntegrity(Name))
            {
                name = Name;

                for (int i = 0; i < Parameters.Length; i++)
                {
                    Parameters[i] = CodeCleaner.RemoveWhiteSpace(Parameters[i], lm);
                    check = (CodeCleaner.CheckNameIntegrity(Parameters[i])) ? check : false;
                }
                if (check) param = Parameters;
                else lm.Add("The functions '" + Name + "' once or more incorrect parameter");
            }
            else lm.Add("The function name '" + Name + "' was incorrect");
        }
        /// <summary>
        /// Classe che descrive una funzione con i suoi parametri
        /// </summary>
        /// <param name="Name">Nome della funzione</param>
        /// <param name="Parameters">Nomi dei parametri</param>
        /// <param name="lm">Gestore di messaggi</param>
        private Function(string Name, string[] Parameters, LogManager lm) : this(Name, Parameters, -1, lm) { }
        /// <summary>
        /// Classe che descrive una funzione con i suoi parametri
        /// </summary>
        /// <param name="Name">Nome della funzione</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <param name="StartPosition">Posizione di partenza nel codice, oppure -1</param>
        private Function(string Name, int StartPosition, LogManager lm) : this(Name, new string[0], StartPosition, lm) { }
        /// <summary>
        /// Classe che descrive una funzione con i suoi parametri
        /// </summary>
        /// <param name="Name">Nome della funzione</param>
        /// <param name="lm">Gestore di messaggi</param>
        private Function(string Name, LogManager lm) : this(Name, new string[0], -1, lm) { }
        /// <summary>
        /// Classe che descrive una funzione con i suoi parametri
        /// </summary>
        private Function() : this(string.Empty, new string[0], -1, new LogManager()) { }

        //Proprietà
        /// <summary>
        /// Nome della funzione
        /// </summary>
        public string Name
        {
            get { return name; }
        }
        /// <summary>
        /// Nomi dei parametri
        /// </summary>
        public string[] Parameters
        {
            get { return param; }
        }
        /// <summary>
        /// Numero di parametri
        /// </summary>
        public int ParamNum
        {
            get { return param.Length; }
        }

        //Metodi override
        /// <summary>
        /// Confronta la funzione con un altra per individuarne le differenze
        /// </summary>
        /// <param name="item">Funzione da confrontare</param>
        /// <returns>Risultato del confronto</returns>
        public bool Equals(Function item)
        {
            return (this.name == item.name && this.param == item.param);
        }
        /// <summary>
        /// Permette un esportazione in stringa della classe
        /// </summary>
        /// <returns>Classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";

            if (!string.IsNullOrWhiteSpace(name))
            {
                ret += name;  //Se il nome esiste e non è nullo
                if (param.Length > 0)
                {
                    ret += "(";

                    foreach (string s in param)
                    {
                        if (!string.IsNullOrWhiteSpace(s)) ret += " " + s + ",";  //Se il parametro esiste lo aggiunge seguito da virgola
                    }

                    ret = ret.Remove(ret.Length - 1);   //Elimina la virgola dopo l'ultimo parametro
                    ret += " )";
                }
            }
            return ret;
        }

        //Metodi statici
        /// <summary>
        /// Metodo Factory, istanzia le funzioni
        /// </summary>
        /// <param name="sign">Firma di una funzione</param>
        /// <param name="lm">Gestore dei messaggi</param>
        /// <param name="lineNumber">Numero di linea del documento</param>
        /// <param name="checkSign">Controlla l'integrita della firma inserita</param>
        /// <returns>Funzione estratta dalla firma, oppure null</returns>
        static public Function GetFunctionBySign(string sign, LogManager lm, int lineNumber = -1, bool checkSign = true)
        {
            if (Regex.Match(sign, @"#(.*)\s*\[", RegexOptions.None).Success)   //Se è una firma in un formato riconosciuto
            {
                //Situazione:    #programma(sicuro,massimo)[
                sign = sign.Trim('#', '['); //Rimuove i simboli

                //Situazione:   programma(sicuro,massimo)
                Match _match = Regex.Match(sign, @"\((.*)\)\s*", RegexOptions.None);   //Inidividua singolarmente i parametri
                if (_match.Success) return new Function(sign.Substring(0, _match.Index), CodeCleaner.RemoveWhiteSpace(_match.Value, lm).Trim('(', ')').Split(','), lineNumber, lm);   //Genera funzione con parametri
                else return new Function(sign, lineNumber, lm); //Genera funzione senza parametri
            }
            else
            {
                lm.Add("The string \'" + sign + "\' is not a functions string", lineNumber);
                return null;
            }
        }
        /// <summary>
        /// Metodo Factory, istanzia le funzioni da codice
        /// </summary>
        /// <param name="code">Frammento di commento</param>
        /// <param name="lm">Gestore dei messaggi</param>
        /// <param name="lineNumber">Numero di linea</param>
        /// <returns>Funzione estratta dal codice, oppure null</returns>
        static public Function GetFunctionByCode(string code, LogManager lm, int lineNumber)
        {
            Match m = Regex.Match(code, @"#(.*)\s*\[", RegexOptions.None);
            if (m.Success)
            {
                return GetFunctionBySign(code.Substring(m.Index, m.Length), lm, lineNumber, false);
            }
            else
            {
                lm.Add("The code that start at the line " + lineNumber.ToString() + "not contain functions sign", lineNumber);
                return null;
            }
        }
    }

    /// <summary>
    /// Rappresenta un insieme di funzioni
    /// </summary>
    [Serializable]
    public class FunctionCollection
    {
        //Campi
        private List<Function> _fncts;

        //Costruttore
        /// <summary>
        /// Rappresenta un insieme di funzioni
        /// </summary>
        public FunctionCollection()
        {
            _fncts = new List<Function>();
        }

        //Proprità
        /// <summary>
        /// Numero di elementi nella raccolta
        /// </summary>
        public int Count
        {
            get { return _fncts.Count; }
        }
        /// <summary>
        /// Permette di selezionare una funzione presente nella raccolta
        /// </summary>
        /// <param name="index">indice della funzione</param>
        /// <returns>Funzione indicata</returns>
        public Function this[int index]
        {
            get { return _fncts[index]; }
        }

        //Metodi override
        /// <summary>
        /// Converte la classe in una stringa visualizzabile
        /// </summary>
        /// <returns>classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < this.Count; i++)
            {
                ret += this[i].ToString() + "\n";
            }
            return ret;
        }

        //Metodi
        /// <summary>
        /// Aggiunge una funzione alla raccolta
        /// </summary>
        /// <param name="item">Funzione</param>
        /// <returns>Retituisce false in caso di funzione non valida</returns>
        public bool Add(Function item)
        {
            if (item == null) return false;
            _fncts.Add(item);
            return true;
        }
        /// <summary>
        /// Inserisce una funzione nella raccolta alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        /// <param name="item">Funzione</param>
        /// <returns>Retituisce false in caso di funzione non valida</returns>
        public bool Insert(int index, Function item)
        {
            if (item == null) return false;
            _fncts.Insert(index, item);
            return true;
        }
        /// <summary>
        /// Rimuove il primo elemento presente nella raccolta
        /// </summary>
        /// <param name="item">Funzione</param>
        public void Remove(Function item)
        {
            RemoveAt(FirstIndexOf(item));
        }
        /// <summary>
        /// Rimuove tutti gli elementi presenti nella lista
        /// </summary>
        /// <param name="item">Funzione</param>
        public void RemoveAll(Function item)
        {
            while (Exist(item)) Remove(item);
        }
        /// <summary>
        /// Rimuove l'elemento alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        public void RemoveAt(int index)
        {
            _fncts.RemoveAt(index);
        }
        /// <summary>
        /// Indica il numero di elementi uguali che la raccolta contiene
        /// </summary>
        /// <param name="item">Funzione</param>
        /// <returns>Numero di ricorrenze nella collezione</returns>
        public int Contains(Function item)
        {
            int count = 0;
            int sup = 0;

            do
            {
                sup = FirstIndexOf(sup, item);
                if (sup != -1) count++;
            }
            while (sup != -1);

            return count;
        }
        /// <summary>
        /// Controlla l'esistenza dell'elemento nella raccolta
        /// </summary>
        /// <param name="item">Funzione</param>
        /// <returns>Esistenza dell'elemento</returns>
        public bool Exist(Function item)
        {
            return (FirstIndexOf(item) != -1);
        }
        /// <summary>
        /// Individua la prima ricorrenza dell'oggetto nella raccolta
        /// </summary>
        /// <param name="item">Funzione</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(Function item)
        {
            return FirstIndexOf(0, item);
        }
        /// <summary>
        /// Infividua la prima ricorrenza dell'oggerro nella raccolta a partire dall'indice
        /// </summary>
        /// <param name="start">Indice si partenza</param>
        /// <param name="item">Funzione</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(int start, Function item)
        {
            for (int i = start; i < _fncts.Count; i++) if (_fncts[i].Equals(item)) return i;
            return -1;
        }

        //Metodi statici
        /// <summary>
        /// Dato un codice ne estrae tutte le funzioni
        /// </summary>
        /// <param name="RawCode">Codice con funzioni</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Restituisce una collezione di Funzioni</returns>
        static public FunctionCollection FindFunctionInCode(string RawCode, LogManager lm)
        {
            //FunctionCollection funcs = new FunctionCollection();    //Crea una collezione di funzioni
            //Function sup;
            //int lineNumber;

            //MatchCollection matcs = Regex.Matches(RawCode, @"#(.*)\[", RegexOptions.Singleline);    //Estrae tutte le firme

            //foreach (Match m in matcs)  //Per ogni firma
            //{
            //    lineNumber = RawCode.Substring(0, RawCode.IndexOf(m.Value)).Split('\n').Length + 1;    //Estrae il testo prima della firma e calcola il numero di linee
            //    sup = Function.GetFunctionBySign(CodeCleaner.RemoveSpaceAndTab(m.Value, lm), lm, lineNumber);   //Estrae la funzione

            //    if (sup != null && !funcs.Exist(sup)) funcs.Add(sup);  //Se la firma era valida e la funzione non è gia presente
            //    else if (sup != null)   //Se la funzioen è già presente
            //    {
            //        lm.Add("The Function '" + sup.ToString() + "' was alredy read in the code", lineNumber);
            //        return null;
            //    }
            //}

            //return funcs;
            return null;
        }
    }
    #endregion

    #region Linee di Codice
    /// <summary>
    /// Linea di codice pronto per l'interpretazione
    /// </summary>
    public class CodeLine
    {
        //Campi
        private string _code;
        private int _originalNumberLine;
        private int _indentLeve;

        //Costruttore
        /// <summary>
        /// Linea di codice pronto per l'interpretazione
        /// </summary>
        /// <param name="RawLine">Linea di codice</param>
        /// <param name="LineNumber">Numero della linea</param>
        /// <param name="Level">Livello dell'indentazione</param>
        private CodeLine(string RawLine, int LineNumber, int Level)
        {
            _code = RawLine;
            _originalNumberLine = LineNumber;
            _indentLeve = Level;
        }

        //Proprità
        /// <summary>
        /// Testo del codice
        /// </summary>
        public string Code
        {
            get { return _code; }
            set { if (value != null)_code = value; }
        }
        /// <summary>
        /// Numero della linea del codice
        /// </summary>
        public int LineNumber
        {
            get { return _originalNumberLine; }
        }
        /// <summary>
        /// Livello di indentazione a cui si trovava il codice
        /// </summary>
        public int IndentLevel
        {
            get { return _indentLeve; }
        }

        //Metodi override
        /// <summary>
        /// Confronta la funzione con un altra per individuarne le differenze
        /// </summary>
        /// <param name="item">Funzione da confrontare</param>
        /// <returns>Risultato del confronto</returns>
        public bool Equals(CodeLine item)
        {
            return (this._code == item._code && this._indentLeve == item._indentLeve && this._originalNumberLine == item._originalNumberLine);
        }
        /// <summary>
        /// Permette un esportazione in stringa della classe
        /// </summary>
        /// <returns>Classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            ret += ((this._originalNumberLine < 0) ? "0" : this._originalNumberLine.ToString()) + ":";
            ret += "[" + ((this._indentLeve < 0) ? "" : this._indentLeve.ToString()) + "]\t";
            ret += (!string.IsNullOrWhiteSpace(this._code)) ? this._code : "";
            return ret;
        }

        //Metodi statici
        /// <summary>
        /// Permette l'estrazione da una stringa del codice per l'interpretazione
        /// </summary>
        /// <param name="RawLine">Codice grezzo</param>
        /// <param name="LineNumber">Numero della linea</param>
        /// <param name="lm">Gestore dei messaggi</param>
        /// <returns>Restituisce una linea di codice composta</returns>
        public static CodeLine LineExtractor(string RawLine, int LineNumber, LogManager lm)
        {
            if (string.IsNullOrWhiteSpace(RawLine))
            {
                if (RawLine == null)
                {
                    lm.Add("The translator cannot work on the that line", LineNumber);
                }
                return null;
            }

            int level = CountIntendation(RawLine);
            RawLine = CodeCleaner.RemoveTabulation(RawLine, lm);
            RawLine = CodeCleaner.ReduceWitheSpaces(RawLine, lm);

            if (level == -1)
            {
                lm.Add("The translator cannot found the level of the indentation on that line", LineNumber);
                return null;
            }
            else return new CodeLine(RawLine, LineNumber, level);
        }

        //Metodi privati
        /// <summary>
        /// Calcola il livello di indentazione di una linea di codice
        /// </summary>
        /// <param name="RawCode">Codice grezzo</param>
        /// <returns>Restituisce il livello dell'indentazione</returns>
        private static int CountIntendation(string RawCode)
        {
            try
            {
                return Regex.Match(RawCode, @"^\t+", RegexOptions.Singleline).Length;
            }
            //catch (RegexMatchTimeoutException) { return -1; }
            catch (ArgumentOutOfRangeException) { return -1; }
        }
    }

    /// <summary>
    /// Collezione di linee di codice
    /// </summary>
    public class CodeLineCollection
    {
        //Campi
        private List<CodeLine> _cdlns;
        int _index;

        //Costruttore
        /// <summary>
        /// Collezione di linee di codice
        /// </summary>
        private CodeLineCollection()
        {
            _cdlns = new List<CodeLine>();
            _index = 0;
        }

        //Proprietà
        /// <summary>
        /// Numero di elementi nella collezione
        /// </summary>
        public int Count
        {
            get { return _cdlns.Count; }
        }
        /// <summary>
        /// Indice interno alla collezione
        /// </summary>
        public int Index
        {
            get { return 0; }
            set
            {
                if (value < 0) _index = 0;
                else if (value >= this.Count) _index = this.Count - 1;
                else _index = value;
            }
        }
        /// <summary>
        /// Lista di elementi interna alla collezione
        /// </summary>
        /// <param name="index">Indice</param>
        /// <returns>oggetto indicato</returns>
        public CodeLine this[int index]
        {
            get { return _cdlns[index]; }
        }

        //Metodi override
        public override string ToString()
        {
            string ret = "";
            ret += (_cdlns.Count > 0) ? "{" : "";
            for (int i = 0; i < _cdlns.Count; i++)
            {
                ret += _cdlns[i].ToString() + "\n";
            }
            ret += (_cdlns.Count > 0) ? "}" : "";
            return ret;
        }

        //Metodi
        /// <summary>
        /// Aggiunge una Linea di codice alla raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Retituisce false in caso di Linea di codice non valida</returns>
        public bool Add(CodeLine item)
        {
            if (item == null) return false;
            _cdlns.Add(item);
            return true;
        }
        /// <summary>
        /// Inserisce una linea di codice nella raccolta alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        /// <param name="item">Linea di codice</param>
        /// <returns>Retituisce false in caso di Linea di codice non valida</returns>
        public bool Insert(int index, CodeLine item)
        {
            if (item == null) return false;
            _cdlns.Insert(index, item);
            return true;
        }
        /// <summary>
        /// Rimuove il primo elemento presente nella raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        public void Remove(CodeLine item)
        {
            RemoveAt(FirstIndexOf(item));
        }
        /// <summary>
        /// Rimuove tutti gli elementi presenti nella lista
        /// </summary>
        /// <param name="item">Linea di codice</param>
        public void RemoveAll(CodeLine item)
        {
            while (Exist(item)) Remove(item);
        }
        /// <summary>
        /// Rimuove l'elemento alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        public void RemoveAt(int index)
        {
            _cdlns.RemoveAt(index);
        }
        /// <summary>
        /// Indica il numero di elementi uguali che la raccolta contiene
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Numero di ricorrenze nella collezione</returns>
        public int Contains(CodeLine item)
        {
            int count = 0;
            int sup = 0;

            do
            {
                sup = FirstIndexOf(sup, item);
                if (sup != -1) count++;
            }
            while (sup != -1);

            return count;
        }
        /// <summary>
        /// Controlla l'esistenza dell'elemento nella raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Esistenza dell'elemento</returns>
        public bool Exist(CodeLine item)
        {
            return (FirstIndexOf(item) != -1);
        }
        /// <summary>
        /// Individua la prima ricorrenza dell'oggetto nella raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(CodeLine item)
        {
            return FirstIndexOf(0, item);
        }
        /// <summary>
        /// Infividua la prima ricorrenza dell'oggetto nella raccolta a partire dall'indice
        /// </summary>
        /// <param name="start">Indice si partenza</param>
        /// <param name="item">Linea di codice</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(int start, CodeLine item)
        {
            for (int i = start; i < _cdlns.Count; i++) if (_cdlns[i].Equals(item)) return i;
            return -1;
        }
        /// <summary>
        /// Individua l'indice della linea richiesta
        /// </summary>
        /// <param name="LineNumber">Numero della linea richiesta</param>
        /// <returns>Indice della linea</returns>
        public int FindLine(int LineNumber)
        {
            for (int i = 0; i < this._cdlns.Count; i++)
            {
                if (this[i].LineNumber == LineNumber) return i;
            }
            return -1;
        }

        //Metodi di estrazione
        /// <summary>
        /// Permette di estrarre una sottocollezione dalla collezione stessa
        /// </summary>
        /// <param name="start">indice di partenza</param>
        /// <param name="lenght">numero di elementi da estrarre</param>
        /// <returns>Collezione estratta</returns>
        public CodeLineCollection Extractor(int start, int lenght)
        {
            if (start > 0 && lenght > 0 && start + lenght <= this.Count)
            {
                CodeLineCollection cc = new CodeLineCollection();
                for (int i = start; i < start + lenght; i++)
                {
                    cc.Add(this[i]);
                }
                return cc;
            }
            else return null;
        }
        /// <summary>
        /// Permette di estrarre una sottocollezione dalla collezione stessa
        /// </summary>
        /// <param name="start">indice di partenza</param>
        /// <param name="lenght">numero di elementi da estrarre</param>
        /// <returns>Collezione estratta</returns>
        public CodeLineCollection Extractor(int start)
        {
            return Extractor(start, this.Count - start);
        }
        /// <summary>
        /// Estrae tutte le linee con indentazione maggiore a quella indicata
        /// </summary>
        /// <param name="LineNumber">Numero di linea da cui partire (non compresa)</param>
        /// <returns>Lista a indentazione maggiore</returns>
        public CodeLineCollection ExtractSubIndentation(int LineNumber)
        {
            int indx = FindLine(LineNumber), i = 0;

            if (indx > -1)
            {
                CodeLineCollection clc = new CodeLineCollection();
                i = indx + 1;
                while (i < this.Count && this[indx].IndentLevel < this[i].IndentLevel)
                {
                    clc.Add(this[i]);
                    i++;
                }
                return clc;
            }
            else return null;
        }

        //Metodi statici
        public static CodeLineCollection ExtractCollection(string RawCode, int LineNumber, LogManager lm)
        {
            CodeLineCollection cc = new CodeLineCollection();
            string[] line = RawCode.Split('\n');
            for (int i = 0; i < line.Length; i++) cc.Add(CodeLine.LineExtractor(line[i], LineNumber + i, lm));
            return cc;
        }
    }
    #endregion

    /// <summary>
    /// Indice in un codice
    /// </summary>
    public struct CodeIndex
    {
        private int _index;
        private int _lenght;

        /// <summary>
        /// Indice in un codice
        /// </summary>
        /// <param name="Index">Indice</param>
        /// <param name="Lenght">Lunghezza</param>
        public CodeIndex(int Index, int Lenght)
        {
            this._index = Index;
            this._lenght = Lenght;
        }

        /// <summary>
        /// Indice
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = (value >= 0) ? value : _index; }
        }

        /// <summary>
        /// Lunghezza
        /// </summary>
        public int Lenght
        {
            get { return _lenght; }
            set { _lenght = (value >= 0) ? value : _lenght; }
        }
    }

    #region Variables
    /// <summary>
    /// Variabile del linguaggio leggermente
    /// </summary>
    public class Variable
    {
        //Campi
        private string name;
        private string value;
        private ValueType type;
        private Variable[] vect;

        //Costruttore
        /// <summary>
        /// Variabile del linguaggio leggermente
        /// </summary>
        /// <param name="Name">Nome della variabile</param>
        /// <param name="Value">Valore della variabile</param>
        /// <param name="VectorNumber">Numero di elementi nel vettore</param>
        private Variable(string Name, string Value, int VectorNumber = -1)
        {
            if (VectorNumber >= 0) CreateVector(VectorNumber);
            else ChangeValue(Value);
            name = (Name != null) ? Name : "";
            CriticalError += new OperationErrorHandler(Variable_CriticalError);
            DifferentTypeError += new DifferentTypeErrorHandler(Variable_DifferentTypeError);
        }
        private Variable() : this("", "", -1) { }

        /// <summary>
        /// Variabile del linguaggio leggermente
        /// </summary>
        /// <param name="Name">Nome della variabile</param>
        /// <param name="Value">Valore della variabile</param>
        public Variable(string Name, string Value) : this(Name, Value, -1) { }
        /// <summary>
        /// Variabile del linguaggio leggermente
        /// </summary>
        /// <param name="Name">Nome della variabile</param>
        /// <param name="VectorNumber">Numero di elementi nel vettore</param>
        public Variable(string Name, int VectorNumber) : this(Name, "", VectorNumber) { }
        /// <summary>
        /// Variabile del linguaggio leggermente
        /// </summary>
        /// <param name="Name">Nome della variabile</param>
        public Variable(string Name) : this(Name, "", -1) { }

        //Proprietà
        /// <summary>
        /// Valore della variabile
        /// </summary>
        public string Value
        {
            get { return value; }
        }
        /// <summary>
        /// Tipo della variabile
        /// </summary>
        public ValueType Type
        {
            get { return type; }
        }
        /// <summary>
        /// Vettore di variabili
        /// </summary>
        /// <param name="index">Indice</param>
        /// <returns>Variabile nell'array</returns>
        public Variable this[int index]
        {
            get
            {
                if (index > vect.Length) index = vect.Length;
                return vect[index];
            }
        }
        /// <summary>
        /// Nome della variabile
        /// </summary>
        public string Name
        {
            get { return name; }
        }
        public int VectLenght
        {
            get
            {
                if (this.Type == ValueType.Vector) return vect.Length;
                return -1;
            }
        }

        //Metodi pubblici
        /// <summary>
        /// Cambia il valore della variabile
        /// </summary>
        /// <param name="value">Valore</param>
        public void ChangeValue(string value)
        {
            switch (Variable.GetValueType(value))
            {
                case ValueType.Boolean:
                    type = ValueType.Boolean;
                    this.value = value;
                    break;
                case ValueType.Number:
                    type = ValueType.Number;
                    this.value = value;
                    break;
                case ValueType.String:
                    type = ValueType.String;
                    this.value = value;
                    break;
                default:
                    type = ValueType.Null;
                    this.value = "";
                    break;
            }
        }
        /// <summary>
        /// Converte la variabile in vettore di variabili
        /// </summary>
        /// <param name="ElementsNumber">Numero di elementi del vettore</param>
        public void CreateVector(int ElementsNumber)
        {
            vect = new Variable[ElementsNumber];
            for (int i = 0; i < ElementsNumber; i++) vect[i] = new Variable("");

            this.ChangeValue("");
            this.type = ValueType.Vector;
        }

        //Metodi privati (Evitare eccezzioni per la mancata implementazione di eventuali errori)
        private void Variable_DifferentTypeError(string operation, Variable Left, Variable Right, string Message, DateTime When) { }
        private void Variable_CriticalError(Variable sender, string operation, Variable[] operatedVariable, DateTime When) { }

        //Evento
        public event OperationErrorHandler CriticalError;
        public event DifferentTypeErrorHandler DifferentTypeError;

        //Metodi override
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((Variable)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(Variable obj)
        {
            return (this.type == obj.type && this.name == obj.name && this.value == obj.value && this.vect == obj.vect);
        }
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret;
            if (type == ValueType.Vector)
            {
                ret = "array[";
                for (int i = 0; i < vect.Length; i++) ret += " " + i.ToString() + ": " + vect[i].ToString() + ",";
                ret = ret.Remove(ret.Length);
                return ret + " ]";
            }
            else ret = name + ": " + value;
            return ret;
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //Metodo override operatori
        public static Variable operator +(Variable left, Variable right)
        {
            if (left.Type == ValueType.Number && right.Type == ValueType.Number)
            {
                double nLeft, nRight;
                if (double.TryParse(left.value, out nLeft) && double.TryParse(right.value, out nRight))
                {
                    return new Variable((nLeft + nRight).ToString());
                }
                else
                {
                    Variable[] v = new Variable[2];
                    v[0] = left;
                    v[1] = right;
                    left.CriticalError(left, "+", v, DateTime.Now);
                    return new Variable();
                }
            }
            else if (left.Type == ValueType.String && right.Type == ValueType.String)
            {
                return new Variable(left.value + right.value);
            }
            else left.DifferentTypeError("+", left, right, "There aren't numeric or string", DateTime.Now);
            return new Variable();
        }
        public static Variable operator -(Variable left, Variable right)
        {
            if (left.Type == ValueType.Number && right.Type == ValueType.Number)
            {
                double nLeft, nRight;
                if (double.TryParse(left.value, out nLeft) && double.TryParse(right.value, out nRight))
                {
                    return new Variable((nLeft - nRight).ToString());
                }
                else
                {
                    Variable[] v = new Variable[2];
                    v[0] = left;
                    v[1] = right;
                    left.CriticalError(left, "-", v, DateTime.Now);
                    return new Variable();
                }
            }
            else left.DifferentTypeError("-", left, right, "There aren't numeric", DateTime.Now);
            return new Variable();
        }
        public static Variable operator *(Variable left, Variable right)
        {
            if (left.Type == ValueType.Number && right.Type == ValueType.Number)
            {
                double nLeft, nRight;
                if (double.TryParse(left.value, out nLeft) && double.TryParse(right.value, out nRight))
                {
                    return new Variable((nLeft * nRight).ToString());
                }
                else
                {
                    Variable[] v = new Variable[2];
                    v[0] = left;
                    v[1] = right;
                    left.CriticalError(left, "*", v, DateTime.Now);
                    return new Variable();
                }
            }
            else left.DifferentTypeError("*", left, right, "There aren't numeric", DateTime.Now);
            return new Variable();
        }
        public static Variable operator /(Variable left, Variable right)
        {
            if (left.Type == ValueType.Number && right.Type == ValueType.Number)
            {
                double nLeft, nRight;
                if (double.TryParse(left.value, out nLeft) && double.TryParse(right.value, out nRight))
                {
                    if (nRight == 0)
                    {
                        return new Variable((nLeft / nRight).ToString());
                    }
                    else
                    {
                        Variable[] v = new Variable[2];
                        v[0] = left;
                        v[1] = right;
                        left.CriticalError(left, "/", v, DateTime.Now);
                        return new Variable();
                    }
                }
                else
                {
                    Variable[] v = new Variable[2];
                    v[0] = left;
                    v[1] = right;
                    left.CriticalError(left, "/", v, DateTime.Now);
                    return new Variable();
                }
            }
            else left.DifferentTypeError("/", left, right, "There aren't numeric", DateTime.Now);
            return new Variable();
        }
        public static bool operator >(Variable left, Variable right)
        {
            if (left.Type == ValueType.Number && right.Type == ValueType.Number)
            {
                double nLeft, nRight;
                if (double.TryParse(left.value, out nLeft) && double.TryParse(right.value, out nRight))
                {
                    return (nLeft > nRight);
                }
                else
                {
                    Variable[] v = new Variable[2];
                    v[0] = left;
                    v[1] = right;
                    left.CriticalError(left, "maggiore", v, DateTime.Now);
                    return false;
                }
            }
            else if (left.Type == ValueType.String && right.Type == ValueType.String)
            {
                return (left.value.Length > right.value.Length);
            }
            else left.DifferentTypeError(">", left, right, "There aren't numeric or string", DateTime.Now);
            return false;
        }
        public static bool operator <(Variable left, Variable right)
        {
            if (left.Type == ValueType.Number && right.Type == ValueType.Number)
            {
                double nLeft, nRight;
                if (double.TryParse(left.value, out nLeft) && double.TryParse(right.value, out nRight))
                {
                    return (nLeft < nRight);
                }
                else
                {
                    Variable[] v = new Variable[2];
                    v[0] = left;
                    v[1] = right;
                    left.CriticalError(left, "minore", v, DateTime.Now);
                    return false;
                }
            }
            else if (left.Type == ValueType.String && right.Type == ValueType.String)
            {
                return (left.value.Length < right.value.Length);
            }
            else left.DifferentTypeError("<", left, right, "There aren't numeric or string", DateTime.Now);
            return false;
        }
        public static bool operator >=(Variable left, Variable right)
        {
            return (left == right || left > right);
        }
        public static bool operator <=(Variable left, Variable right)
        {
            return (left == right || left < right);
        }
        public static bool operator ==(Variable left, Variable right)
        {
            return (left.type == right.type && left.value == right.value && left.vect == right.vect);
        }
        public static bool operator !=(Variable left, Variable right)
        {
            return !(left.type == right.type && left.value == right.value && left.vect == right.vect);
        }
        public static bool operator !(Variable var)
        {
            if (var.type == ValueType.Boolean)
            {
                return (var.value == "true");
            }
            var.DifferentTypeError("!", var, null, "The translator has try to negate the variable not bool", DateTime.Now);
            return false;
        }

        // Metodi statici pubblici
        /// <summary>
        /// Dato il valore individua il tipo
        /// </summary>
        /// <param name="Value">valore</param>
        /// <returns>Tipo affine</returns>
        public static ValueType GetValueType(string Value)
        {
            if (string.IsNullOrEmpty(Value)) return ValueType.Null;
            else
            {
                Match m = Regex.Match(Value, @"(true)?(false)?", RegexOptions.None);
                if (m.Success && m.Length > 0) return ValueType.Boolean;
                else if (Regex.IsMatch(Value, @"\d+([.]?\d+)?(\^?\d+)?", RegexOptions.None)) return ValueType.Number;
                else if (Regex.IsMatch(Value, @"[\S\s\D]+", RegexOptions.None)) return ValueType.String;
                else return ValueType.Null;
            }
        }
    }

    /// <summary>
    /// Rappresenta un insieme di variabili
    /// </summary>
    public class VariableCollection
    {
        //Campi
        private List<Variable> _vbcts;

        //Costruttore
        /// <summary>
        /// Rappresenta un insieme di variabili
        /// </summary>
        public VariableCollection()
        {
            _vbcts = new List<Variable>();
        }

        //Proprità
        /// <summary>
        /// Numero di elementi nella raccolta
        /// </summary>
        public int Count
        {
            get { return _vbcts.Count; }
        }
        /// <summary>
        /// Permette di selezionare una variabile presente nella raccolta
        /// </summary>
        /// <param name="index">indice della variabile</param>
        /// <returns>variabile indicata</returns>
        public Variable this[int index]
        {
            get { return _vbcts[index]; }
        }
        /// <summary>
        /// Permette la selezione di una variabile in base al nome
        /// </summary>
        /// <param name="name">Nome</param>
        /// <returns>variabile indicata</returns>
        public Variable this[string name]
        {
            get
            {
                int i = GetIndexOfName(name);
                if (i >= 0) return this[i];
                else return null;
            }
        }

        //Metodi override
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((VariableCollection)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(VariableCollection obj)
        {
            return (this._vbcts == obj._vbcts);
        }
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < _vbcts.Count; i++)
            {
                ret += _vbcts.ToString() + ", \n";
            }
            return ret;
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //Metodi
        /// <summary>
        /// Aggiunge una variabile alla raccolta
        /// </summary>
        /// <param name="item">variabile</param>
        /// <returns>Retituisce false in caso di variabile non valida</returns>
        public bool Add(Variable item)
        {
            _vbcts.Add(item);
            return true;
        }
        /// <summary>
        /// Inserisce una variabile nella raccolta alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        /// <param name="item">variabile</param>
        /// <returns>Retituisce false in caso di variabile non valida</returns>
        public bool Insert(int index, Variable item)
        {
            if (item == new Variable("NULL")) return false;
            _vbcts.Insert(index, item);
            return true;
        }
        /// <summary>
        /// Rimuove il primo elemento presente nella raccolta
        /// </summary>
        /// <param name="item">variabile</param>
        public void Remove(Variable item)
        {
            RemoveAt(FirstIndexOf(item));
        }
        /// <summary>
        /// Rimuove tutti gli elementi presenti nella lista
        /// </summary>
        /// <param name="item">variabile</param>
        public void RemoveAll(Variable item)
        {
            while (Exist(item)) Remove(item);
        }
        /// <summary>
        /// Rimuove l'elemento alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        public void RemoveAt(int index)
        {
            _vbcts.RemoveAt(index);
        }
        /// <summary>
        /// Indica il numero di elementi uguali che la raccolta contiene
        /// </summary>
        /// <param name="item">variabile</param>
        /// <returns>Numero di ricorrenze nella collezione</returns>
        public int Contains(Variable item)
        {
            int count = 0;
            int sup = 0;

            do
            {
                sup = FirstIndexOf(sup, item);
                if (sup != -1) count++;
            }
            while (sup != -1);

            return count;
        }
        /// <summary>
        /// Controlla l'esistenza dell'elemento nella raccolta
        /// </summary>
        /// <param name="item">variabile</param>
        /// <returns>Esistenza dell'elemento</returns>
        public bool Exist(Variable item)
        {
            return (FirstIndexOf(item) != -1);
        }
        /// <summary>
        /// Individua la prima ricorrenza dell'oggetto nella raccolta
        /// </summary>
        /// <param name="item">variabile</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(Variable item)
        {
            return FirstIndexOf(0, item);
        }
        /// <summary>
        /// Individua la prima ricorrenza dell'oggetto nella raccolta a partire dall'indice
        /// </summary>
        /// <param name="start">Indice si partenza</param>
        /// <param name="item">variabile</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(int start, Variable item)
        {
            for (int i = start; i < _vbcts.Count; i++) if (_vbcts[i].Name == item.Name) return i;
            return -1;
        }
        /// <summary>
        /// Individua la prima occorrenza del nome di una variabile
        /// </summary>
        /// <param name="VarName">Nome della variabile</param>
        /// <returns>Relativo indice o -1</returns>
        public int GetIndexOfName(string VarName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name == VarName) return i;
            }
            return -1;
        }
    }

    /// <summary>
    /// Tipi di valori che può prendere la variabile in leggermente
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        /// Tipo Booleano
        /// </summary>
        Boolean,
        /// <summary>
        /// Tipo Stringa
        /// </summary>
        String,
        /// <summary>
        /// Tipo Numerico
        /// </summary>
        Number,
        /// <summary>
        /// Tipo Vettore
        /// </summary>
        Vector,
        /// <summary>
        /// Null
        /// </summary>
        Null
    }

    /// <summary>
    /// Errore richiamato da un operazione errata
    /// </summary>
    /// <param name="sender">Variabile soggetta all'errore</param>
    /// <param name="operation">Operazione eseguita</param>
    /// <param name="operatedVariable">Parametri operati</param>
    /// <param name="When">Quando</param>
    public delegate void OperationErrorHandler(Variable sender, string operation, Variable[] operatedVariable, DateTime When);
    /// <summary>
    /// Tipo di variabile non possibile con la seguente operazione
    /// </summary>
    /// <param name="operation">Operazione eseguita</param>
    /// <param name="Left">Variabile di destra</param>
    /// <param name="Right">Variabile di sinistra</param>
    /// <param name="Message">Messaggio</param>
    /// <param name="When">Quandos</param>
    public delegate void DifferentTypeErrorHandler(string operation, Variable Left, Variable Right, string Message, DateTime When);
    #endregion
}
