using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Leggermente.Translator
{
    /// <summary>
    /// Frammento di funzione nel codice
    /// </summary>
    public class FunctionSection
    {
        //Campi
        private Function funct;
        private CodeLineCollection codes;

        //Costruttore
        /// <summary>
        /// Frammento di funzione nel codice
        /// </summary>
        /// <param name="Functions">Funzione</param>
        /// <param name="CodeRows">Linee di codice</param>
        private FunctionSection(Function Function, CodeLineCollection CodeRows)
        {
            if (Function != null && CodeRows != null)
            {
                funct = Function;
                codes = CodeRows;
            }
        }

        //Proprietà
        /// <summary>
        /// Funzione
        /// </summary>
        public Function Function
        {
            get { return funct; }
        }
        /// <summary>
        /// Linee di codice
        /// </summary>
        public CodeLineCollection FunctionCodes
        {
            get { return codes; }
        }

        //Metodi override
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            if (funct != null)
            {
                ret += funct.ToString() + " | ";
                ret += codes.ToString();
            }
            return ret;
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((FunctionSection)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(FunctionSection obj)
        {
            return (this.funct == obj.funct && this.codes == obj.codes);
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //Metodi statici
        public static FunctionSection ExtractSection(string RawCode, int LineNumber, LogManager lm)
        {
            Function f = Function.GetFunctionByCode(RawCode, lm, LineNumber);
            RawCode = CodeElaborator.RemoveFunctionSign(RawCode, lm);
            CodeLineCollection cc = CodeLineCollection.ExtractCollection(RawCode, LineNumber, lm);
            return new FunctionSection(f, cc);
        }
    }

    /// <summary>
    /// Collezione di sezioni di codice
    /// </summary>
    public class SectionCollection
    {
        //Campi
        private List<FunctionSection> _scls;
        private int _index;
        
        //Costruttore
        /// <summary>
        /// Collezione di sezioni di codice
        /// </summary>
        private SectionCollection()
        {
            _scls = new List<FunctionSection>();
            _index = 0;
        }

        //Proprietà
        /// <summary>
        /// Numero di elementi nella collezione
        /// </summary>
        public int Count
        {
            get { return _scls.Count; }
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
        public FunctionSection this[int index]
        {
            get { return _scls[index]; }
        }

        //Metodi override
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            ret += (_scls.Count > 0) ? "{" : "";
            for (int i = 0; i < _scls.Count; i++)
            {
                ret += "[" + _scls[i].ToString() + "]\n";
            }
            ret += (_scls.Count > 0) ? "}" : "";
            return ret;
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((SectionCollection)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(SectionCollection obj)
        {
            return (this._scls == obj._scls);
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
        /// Aggiunge una Linea di codice alla raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Retituisce false in caso di Linea di codice non valida</returns>
        public bool Add(FunctionSection item)
        {
            if (item == null) return false;
            _scls.Add(item);
            return true;
        }
        /// <summary>
        /// Inserisce una linea di codice nella raccolta alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        /// <param name="item">Linea di codice</param>
        /// <returns>Retituisce false in caso di Linea di codice non valida</returns>
        public bool Insert(int index, FunctionSection item)
        {
            if (item == null) return false;
            _scls.Insert(index, item);
            return true;
        }
        /// <summary>
        /// Rimuove il primo elemento presente nella raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        public void Remove(FunctionSection item)
        {
            RemoveAt(FirstIndexOf(item));
        }
        /// <summary>
        /// Rimuove tutti gli elementi presenti nella lista
        /// </summary>
        /// <param name="item">Linea di codice</param>
        public void RemoveAll(FunctionSection item)
        {
            while (Exist(item)) Remove(item);
        }
        /// <summary>
        /// Rimuove l'elemento alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        public void RemoveAt(int index)
        {
            _scls.RemoveAt(index);
        }
        /// <summary>
        /// Indica il numero di elementi uguali che la raccolta contiene
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Numero di ricorrenze nella collezione</returns>
        public int Contains(FunctionSection item)
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
        public bool Exist(FunctionSection item)
        {
            return (FirstIndexOf(item) != -1);
        }
        /// <summary>
        /// Individua la prima ricorrenza dell'oggetto nella raccolta
        /// </summary>
        /// <param name="item">Linea di codice</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(FunctionSection item)
        {
            return FirstIndexOf(0, item);
        }
        /// <summary>
        /// Infividua la prima ricorrenza dell'oggetto nella raccolta a partire dall'indice
        /// </summary>
        /// <param name="start">Indice si partenza</param>
        /// <param name="item">Linea di codice</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(int start, FunctionSection item)
        {
            for (int i = start; i < _scls.Count; i++) if (_scls[i].Equals(item)) return i;
            return -1;
        }

        //Metodi statici
        public static SectionCollection ExtractCollection(string RawCode, LogManager lm)
        {
            SectionCollection sc = new SectionCollection();
            CodeIndex[] ci = CodeElaborator.FindFunctionPosition(RawCode, lm);

            for (int i = 0; i < ci.Length; i++) sc.Add(FunctionSection.ExtractSection(RawCode.Substring(ci[i].Index, ci[i].Lenght), RawCode.Substring(0, ci[i].Index).Split('\n').Length, lm));

            return sc;
        }
    }

    /// <summary>
    /// Rappresenta un codice
    /// </summary>
    public class CodeImage
    {
        //Campi
        private VariableCollection _const;
        private SectionCollection _sc;  
        private PackageCollection _pc;
        private CodeType _type;

        //Costruttore
        /// <summary>
        /// Rappresenta un codice
        /// </summary>
        /// <param name="sc">Sezioni di codice</param>
        /// <param name="pc">Pacchetti richiamati</param>
        /// <param name="type">Tipo di codice</param>
        private CodeImage(SectionCollection sc, PackageCollection pc, VariableCollection vc, CodeType type)
        {
            this._const = vc;
            this._sc = sc;
            this._pc = pc;
            this._type = type;
        }

        //Proprietà
        /// <summary>
        /// Costanti del codice
        /// </summary>
        public VariableCollection Constant
        {
            get { return _const; }
        }
        /// <summary>
        /// Sezioni di codice
        /// </summary>
        public SectionCollection Section
        {
            get { return _sc; }
        }
        /// <summary>
        /// Pacchetti inclusi
        /// </summary>
        public PackageCollection Package
        {
            get { return _pc; }
        }
        /// <summary>
        /// Tipo di codice
        /// </summary>
        public CodeType Type
        {
            get { return _type; }
        }

        //Metodo statico
        /// <summary>
        /// Crea un immagine del codice con tuttte le sezioni ripulite ed esplicitate 
        /// </summary>
        /// <param name="RawCode">Codice da analizzare</param>
        /// <param name="PackageAdd">Pacchetti aggiunti nel sistema</param>
        /// <param name="Type">Tipo di codice</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Immagine del codice</returns>
        public static CodeImage CreateCodeImage(string RawCode, PackageCollection PackageAdd, CodeType Type, LogManager lm)
        {
            //Pachet name
            string[] packageName;
            SectionCollection sc;
            VariableCollection vc;//Costanti 
            PackageCollection pc; //Pacchetti inclusi
            bool exactType = false;

            //Pulizia
            RawCode = CodeCleaner.TextReturnUnified(RawCode, lm);
            vc = CodeElaborator.ExtractConstant(ref RawCode, lm);
            RawCode = CodeCleaner.CommentDelete(RawCode, lm);
            RawCode = CodeCleaner.RemoveSynonyms(RawCode, lm);
            RawCode = CodeCleaner.StartLineUnified(RawCode, 4, lm);
            RawCode = CodeCleaner.ReduceWitheSpaces(RawCode, lm);

            //Analisi Include
            packageName = CodeElaborator.IncludeFinder(RawCode, lm);
            RawCode = CodeElaborator.RemoveInclude(RawCode, lm);

            //Tipo
            exactType = CodeElaborator.CheckIfCorrectType(RawCode, Type, lm);
            if (!exactType) return null;

            //Estrazione codice
            sc = SectionCollection.ExtractCollection(RawCode, lm);

            //Selezione pacchetti
            pc = PackageAdd.IncludedPackage(packageName, lm);

            //Creazione aggiunta per pacchetti
            if (Type == CodeType.Package)
            {
                string nome = CodeElaborator.PackegeNameExtractor(RawCode, lm);
                RawCode = CodeElaborator.RemovePackageName(RawCode, lm);
            }

            return new CodeImage(sc, pc, vc, Type);
        }
    }

    /// <summary>
    /// Codice risultante della traduzione
    /// </summary>
    public class ResultCode
    {
        //Campi
        private List<string> codes;
        private string filename;

        //Costruttore
        /// <summary>
        /// Codice risultante della traduzione
        /// </summary>
        public ResultCode()
        {
            codes = new List<string>();
            filename = "";
            IncludesStandard();
        }

        //Metodi Privati
        /// <summary>
        /// Include tutte le librerie standard richieste dal sistema
        /// </summary>
        private void IncludesStandard()
        {
            AddGenericPack("System", null);
            AddGenericPack("System.Collections.Generic", null);
            AddGenericPack("System.Linq", null);
            AddGenericPack("System.Text", null);
            AddGenericPack("Leggermente.lib", null);
        }
        /// <summary>
        /// Aggiungiunge una libreria al codice
        /// </summary>
        /// <param name="NamePackage">Nome della libreria</param>
        /// <param name="lm">Gestore dei messaggi</param>
        private void AddGenericPack(string NamePackage, LogManager lm)
        {
            if (!string.IsNullOrWhiteSpace(NamePackage)) codes.Add("include " + NamePackage + ";");
            else lm.Add("The Translator cannot insert a package in the code");
        }

        //Metodi Pubblici
        /// <summary>
        /// Aggiunge un pacchetto
        /// </summary>
        /// <param name="NamePackage">Nome del pacchetto</param>
        /// <param name="lm">Gestore dei messaggi</param>
        public void AddInclude(string NamePackage, LogManager lm)
        {
            AddGenericPack("Leggermente.lib." + NamePackage, lm);
        }
        /// <summary>
        /// Aggiunge un linea al codice tradotto
        /// </summary>
        /// <param name="Line">Linea di testo</param>
        /// <param name="lm">Gestore dei messaggi</param>
        public void AddLine(string Line, LogManager lm)
        {
            if (Line != null) codes.Add(Line);
            else lm.Add("The translator has try to add a null line in the code-");
        }
        /// <summary>
        /// Aggiunge una linea vuota
        /// </summary>
        public void AddBlankLine()
        {
            AddLine("", null);
        }

        //Metodi override
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < codes.Count; i++)
            {
                ret += codes[i] + "\n";
            }
            return ret;
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((SectionCollection)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(ResultCode obj)
        {
            return (this.filename == obj.filename && this.codes == obj.codes);
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //Proprietà
        /// <summary>
        /// Lista di linee di codice
        /// </summary>
        public List<string> Codes
        {
            get { return codes; }
        }
        /// <summary>
        /// Percorso depositario del file
        /// </summary>
        public string FileName
        {
            get { return filename; }
        }
    }
}
