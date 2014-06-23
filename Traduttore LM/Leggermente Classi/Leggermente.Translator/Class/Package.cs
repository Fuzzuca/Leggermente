using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Leggermente.Translator 
{
    /// <summary>
    /// Classe che gestisce i pacchetti e la relativa esportazione
    /// </summary>
    [Serializable]
    public class Package : Serializzator
    {
        //Campi
        private string path;
        private string name;
        private FunctionCollection function;

        //Costruttore
        /// <summary>
        /// Classe che gestisce i pacchetti e la relativa esportazione
        /// </summary>
        /// <param name="Path">Percorso del file</param>
        /// <param name="Name">Nome del pacchetto</param>
        /// <param name="Function">Lista delle funzioni</param>
        private Package(string Path, string Name, FunctionCollection Function)
        {
            path = Path;
            name = Name;
            function = Function;
        }
        /// <summary>
        /// Classe che gestisce i pacchetti e la relativa esportazione
        /// </summary>
        /// <param name="pack">Pacchetto da copiare</param>
        public Package(Package pack) : this(pack.Path, pack.Name, pack.Functions) { }
        /// <summary>
        /// Classe che gestisce i pacchetti e la relativa esportazione
        /// </summary>
        /// <param name="Name">Nome del pacchetto</param>
        /// <param name="Function">Lista delle funzioni</param>
        public Package(string Name, FunctionCollection Function) : this("", Name, Function) { }
        /// <summary>
        /// Classe che gestisce i pacchetti e la relativa esportazione
        /// </summary>
        /// <param name="Path">Percorso del file</param>
        public Package(string Path) : this(Package.LeggiFile(Path)) { }
        /// <summary>
        /// Classe che gestisce i pacchetti e la relativa esportazione
        /// </summary>
        public Package() : this("", "", null) { }

        //Proprietà
        /// <summary>
        /// Percorso della dll
        /// </summary>
        public string Path
        {
            get { return path; }
        }
        /// <summary>
        /// Nome del pacchetto
        /// </summary>
        public string Name
        {
            get { return name; }
        }
        /// <summary>
        /// Lista delle funzioni
        /// </summary>
        public FunctionCollection Functions
        {
            get { return function; }
        }

        //Metodi override
        /// <summary>
        /// Confronta un pacchetto con un altro per individuarne le differenze
        /// </summary>
        /// <param name="item">Pacchetto da confrontare</param>
        /// <returns>Risultato del confronto</returns>
        public bool Equals(Package item)
        {
            return (this.function == item.function && this.name == item.name);
        }
        /// <summary>
        /// Permette un esportazione in stringa della classe
        /// </summary>
        /// <returns>Classe in stringa</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "[Path: " + ((!string.IsNullOrWhiteSpace(path)) ? this.Path : "") + " ]\n";
            ret += "[Name: " + ((!string.IsNullOrWhiteSpace(name)) ? this.Name : "") + " ]\n";
            ret += "[Function: " + this.Functions.ToString().TrimEnd('\n') + "]\n";
            return ret;
        }

        //Metodo pubblico
        /// <summary>
        /// Permette di impostare il nuovo percorso del file
        /// </summary>
        /// <param name="Path">Percorso del file</param>
        /// <returns>Indica il risultato del controllo d'integrità</returns>
        public bool ChangePath(string Path)
        {
            if (!string.IsNullOrWhiteSpace(Path))
            {
                path = Path;
                return true;
            }
            else return false;
        }

        //Metodo statico
        /// <summary>
        /// Salva l'oggetto su un File
        /// </summary>
        /// <param name="pack">Pacchetto da salvare</param>
        /// <param name="path">Percorso del file</param>
        /// <returns>Successo del corretto salvataggio</returns>
        static public bool SalvaFile(Package pack,string path)
        {
            return Serialize(pack, path);
        }
        /// <summary>
        /// Legge l'oggetto su un File
        /// </summary>
        /// <param name="path">Percorso da leggere</param>
        /// <returns>Successo del corretta lettura</returns>
        static public Package LeggiFile(string path)
        {
            object obj = Deserialize(path);

            if(obj.GetType() == (new Package()).GetType() && obj!=null) {
                Package p = (Package)obj;
                return p;
            }

            return null;
        }
    }

    /// <summary>
    /// Rappresenta un insieme di pacchetti
    /// </summary>
    public class PackageCollection
    {
        //Campi
        private List<Package> _pcks;

        //Costruttore
        /// <summary>
        /// Rappresenta un insieme di pacchetti
        /// </summary>
        public PackageCollection()
        {
            _pcks = new List<Package>();
        }

        //Proprità
        /// <summary>
        /// Numero di elementi nella raccolta
        /// </summary>
        public int Count
        {
            get { return _pcks.Count; }
        }
        /// <summary>
        /// Permette di selezionare una Pacchetto presente nella raccolta
        /// </summary>
        /// <param name="index">indice della Pacchetto</param>
        /// <returns>Pacchetto indicata</returns>
        public Package this[int index]
        {
            get { return _pcks[index]; }
        }

        //Metodi Override
        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < _pcks.Count; i++)
            {
                ret += _pcks[i].ToString() + ((i + 1 < _pcks.Count) ? "; " : "");
            }
            return ret.Remove(ret.Length - 1);
        }

        //Metodi
        /// <summary>
        /// Aggiunge una Pacchetto alla raccolta
        /// </summary>
        /// <param name="item">Pacchetto</param>
        /// <returns>Retituisce false in caso di Pacchetto non valida</returns>
        public bool Add(Package item)
        {
            if (item == null) return false;
            _pcks.Add(item);
            return true;
        }
        /// <summary>
        /// Inserisce una Pacchetto nella raccolta alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        /// <param name="item">Pacchetto</param>
        /// <returns>Retituisce false in caso di Pacchetto non valida</returns>
        public bool Insert(int index, Package item)
        {
            if (item == null) return false;
            _pcks.Insert(index, item);
            return true;
        }
        /// <summary>
        /// Rimuove il primo elemento presente nella raccolta
        /// </summary>
        /// <param name="item">Pacchetto</param>
        public void Remove(Package item)
        {
            RemoveAt(FirstIndexOf(item));
        }
        /// <summary>
        /// Rimuove tutti gli elementi presenti nella lista
        /// </summary>
        /// <param name="item">Pacchetto</param>
        public void RemoveAll(Package item)
        {
            while (Exist(item)) Remove(item);
        }
        /// <summary>
        /// Rimuove l'elemento alla posizione indicata
        /// </summary>
        /// <param name="index">Posizione</param>
        public void RemoveAt(int index)
        {
            _pcks.RemoveAt(index);
        }
        /// <summary>
        /// Indica il numero di elementi uguali che la raccolta contiene
        /// </summary>
        /// <param name="item">Pacchetto</param>
        /// <returns>Numero di ricorrenze nella collezione</returns>
        public int Contains(Package item)
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

        public bool Exist(Package item)
        {
            return (FirstIndexOf(item) != -1);
        }

        public int FirstIndexOf(int i, Package item)
        {
            return FirstIndexOf(i, item.Name);
        }
        public int FirstIndexOf(Package item)
        {
            return FirstIndexOf(0, item.Name);
        }
        /// <summary>
        /// Individua la prima ricorrenza dell'oggetto nella raccolta
        /// </summary>
        /// <param name="item">Pacchetto</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(string itemName)
        {
            return FirstIndexOf(0, itemName);
        }
        /// <summary>
        /// Infividua la prima ricorrenza dell'oggerro nella raccolta a partire dall'indice
        /// <param name="start">Indice si partenza</param>
        /// <param name="item">Pacchetto</param>
        /// <returns>Indice della prima ricorrenza</returns>
        public int FirstIndexOf(int start, string itemName)
        {
            for (int i = start; i < _pcks.Count; i++) if (_pcks[i].Name == itemName) return i;
            return -1;
        }
        /// <summary>
        /// Data la lista dei pacchetti aggiunti filtra quelli attualmente nel codice 
        /// </summary>
        /// <param name="ListInclude">Lista di pacchetti inclusi</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Pacchetti inclusi nel codice</returns>
        public PackageCollection IncludedPackage(string[] ListInclude, LogManager lm)
        {
            PackageCollection pc = new PackageCollection();
            bool find = false;

            for (int i = 0; i < ListInclude.Length; i++)
            {
                find = false;
                for (int j = 0; j < this.Count; j++)
                {
                    if (this[j].Name.Trim() == ListInclude[i].Trim())
                    {
                        find = true;
                        if (pc.Exist(this[j])) lm.Add("The package '" + ListInclude[i] + "' was alredy added", LogType.Warning);
                        else pc.Add(this[j]);
                    }
                }

                if (!find) lm.Add("The translator has try do add the '" + ListInclude[i] + "' package but he hasn't find on the disk");
            }
            return pc;
        }

        //Metodi statici
        public static PackageCollection LoadPackageByFile(string[] PackagePath,LogManager lm)
        {
            PackageCollection pc = new PackageCollection();
            Random rm = new Random(DateTime.Now.Second);
            ZipManager zip = new ZipManager(@"./temp");
            UnzipObject obj;
            Package pack;

            for (int i = 0; i < PackagePath.Length; i++)
            {
                if (File.Exists(PackagePath[i]))
                {
                    zip.PathTempFolder = @"./temp"+@"/"+rm.Next().ToString();
                    obj = zip.OpenPackage(PackagePath[i], lm);
                    if (obj != null)
                    {
                        pack = Package.LeggiFile(obj["lib.bin"]);
                        pack.ChangePath(obj["pack.dll"]);
                        if (pack != null)
                        {
                            pc.Add(pack);
                        }
                    }
                }
            }
            return pc;
        }
    }
}
