using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Ionic.Zip;

namespace Leggermente.Translator
{
    /// <summary>
    /// Gestisce la configurazione su un file INI
    /// </summary>
    [Serializable]
    public class FileINI
    {
        //Campi
        private string path;    //Percorso del file di configurazione INI

        //Riferimenti esterni al "kernel32.dll" di sistema
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);  //Write on a INI file
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath); //Read on a INI file

        //Costruttore
        /// <summary>
        /// Gestisce la configurazione su un file INI
        /// </summary>
        /// <PARAM name="FilePath">Percorso del file di configurazione</PARAM>
        public FileINI(string FilePath)
        {
            path = FilePath;
        }

        //Proprità
        /// <summary>
        /// Percorso del file
        /// </summary>
        public string Path
        {
            get { return path; }
        }

        //Metodi pubblici
        /// <summary>
        /// Scrive i parametri del file di configurazione
        /// </summary>
        /// <PARAM name="Section">Sezione</PARAM>
        /// <PARAM name="Key">Nome</PARAM>
        /// <PARAM name="Value">Valore</PARAM>
        public void WriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);  //Richiamo la API esterna con il path della classe
        }
        /// <summary>
        /// Legge i parametri dal file di configurazione
        /// </summary>
        /// <PARAM name="Section">Sezione</PARAM>
        /// <PARAM name="Key">Nome</PARAM>
        /// <returns>Parametro letto</returns>
        public string ReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(511);    //Creo una stringa
            int i = GetPrivateProfileString(Section, Key, "", temp, 511, this.path);    //Richiamo la API di lettura per un parametro di massimo 511
            return (temp.Length > 0) ? temp.ToString() : null;  //Se non vi sono caratteri letti imposta il risultato a null
        }

        //Metodi override
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((FileINI)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(FileINI obj)
        {
            return (this.path == obj.path);
        }
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            return "[FileINI: " + path + " ]";
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Ereditandola permette la serializzazione di una classe
    /// </summary>
    [Serializable]
    public class Serializzator
    {
        //Metodi protected
        /// <summary>
        /// Serializza un oggetto salvandolo su file
        /// </summary>
        /// <param name="obj">Oggetto da serializzare</param>
        /// <param name="path">Percorso su cui serializzare</param>
        /// <returns>Risultato della serializzazione</returns>
        static protected bool Serialize(object obj, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);  //Creo il percorso e apro uno stream
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                bf.Serialize(fs, obj);  //Serializzo
            }
            catch (SerializationException) { return false; }    //Se va in eccezione ritorno falso
            catch (System.Security.SecurityException) { return false; }
            finally //In ogni caso
            {
                fs.Close(); //Chiudo la connessione
            }
            return true;
        }
        /// <summary>
        /// Deserializzo un oggetto leggendolo da file 
        /// </summary>
        /// <param name="path">Percorso del file</param>
        /// <returns>Oggetto letto</returns>
        static protected object Deserialize(string path)
        {
            if (File.Exists(path))  //Se il percorso esiste
            {
                FileStream fs = new FileStream(path, FileMode.Open);    //Creo uno stream al percorso
                BinaryFormatter bf = new BinaryFormatter();

                object obj;
                try
                {
                    obj = bf.Deserialize(fs);   //Deserializzo
                }
                catch (SerializationException) { return false; }    //Se va in eccezione ritorno falso
                catch (System.Security.SecurityException) { return false; }
                finally //In ogni caso
                {
                    fs.Close(); //Chiudo la connessione
                }

                return obj; //Restituisco l'oggetto
            }
            return null;
        }
    }

    /// <summary>
    /// Gestore che permette la composizione e la decomposizione di file ZIP
    /// La seguente classe implementa "Ionic.zip.dll"
    /// </summary>
    public class ZipManager
    {
        //Campi
        private string _tempFolder; //Percorso della cartella temporanea

        //Costruttore
        /// <summary>
        /// Gestore che permette la composizione e la decomposizione di file ZIP
        /// </summary>
        /// <param name="PathTempFolder">Percorso della cartella temporanea</param>
        public ZipManager(string PathTempFolder)
        {
            this.PathTempFolder = PathTempFolder;
        }

        //Proprietà
        /// <summary>
        /// Percorso della cartella temporanea
        /// </summary>
        public string PathTempFolder
        {
            get { return _tempFolder; }
            set
            {
                if (!Directory.Exists(value))   //Se la cartella non esiste
                {
                    try
                    {
                        Directory.CreateDirectory(value);   //Provo a crearla
                    }
                    catch (PathTooLongException) { }
                    catch (DirectoryNotFoundException) { }
                    catch (NotSupportedException) { }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }

                }
                _tempFolder = value;    //In ogni caso salvo il path
            }
        }

        //Metodi pubblici
        /// <summary>
        /// Permette la scompattazione di un pacchetto
        /// </summary>
        /// <param name="PathOfPackage">Percorso del file</param>
        /// <param name="lm">Gestore dei messaggi</param>
        /// <returns>Una lista di oggetti decompressi</returns>
        public UnzipObject OpenPackage(string PathOfPackage, LogManager lm)
        {
            ZipFile file = new ZipFile(PathOfPackage);  //Imposto il percorso del file da aprire
            file.ExtractAll(_tempFolder, ExtractExistingFileAction.OverwriteSilently);  //Estraggo il file sovrascrivendo nella cartella temporanea
            return UnzipObject.CreateUnzipObject(_tempFolder);  //Creo un oggetto UnzipObject
        }
        //TO DO: public bool CreatePackage(){}

        //Metodi override
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((ZipManager)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(ZipManager obj)
        {
            return (this._tempFolder == obj._tempFolder);
        }
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            return "[ZipManager: " + _tempFolder + " ]";
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Insieme di file che viene generato all'apertura di una cartella compressa
    /// </summary>
    public class UnzipObject
    {
        //Campi
        List<string> _files;    //Lista dei file contenuti della cartella (primo livello)
        string _folderPath;     //Percorso della cartella

        //Costruttore
        /// <summary>
        /// Insieme di file che viene generato all'apertura di una cartella compressa
        /// </summary>
        /// <param name="PathOfUnzippedFolder">Percorso della cartella contenente i file precedentemente compressi</param>
        /// <param name="Files">Nome di tutti i file presenti nel sistema</param>
        private UnzipObject(string PathOfUnzippedFolder,string[] Files)
        {
            _files = new List<string>();    //Riassegno la lista dei file
            for (int i = 0; i < Files.Length; i++) _files.Add(Files[i]);    //Aggiungo file
            _folderPath = PathOfUnzippedFolder; //Imposto il percorso della certella
        }

        //Proprità
        /// <summary>
        /// Percorso della cartella contenente i file precedentemente compressi
        /// </summary>
        public string UnzipFolderPath
        {
            get { return (Directory.Exists(_folderPath)) ? _folderPath : null; }    //Se la cartella esiste ancora ritorna il percorso, altrimenti NULL
        }
        /// <summary>
        /// Numero di file presenti nella cartella
        /// </summary>
        public int Count
        {
            get { return _files.Count; }
        }
        /// <summary>
        /// Insieme di file precedentemente compressi
        /// </summary>
        /// <param name="index">Indice</param>
        /// <returns>Percorso del file compresso</returns>
        public string this[int index]
        {
            get
            {
                string ret;

                if (this.UnzipFolderPath != null)   //Se la cartella esiste ancora
                {
                    ret = this.UnzipFolderPath + "/" + _files[index];   //Aggiungo alla cartella il file selezionato
                    return (File.Exists(ret)) ? ret : null;   //Se il file esiste ancora ritorno il path, altrimenti null
                }
                else return null;
            }
        }
        /// <summary>
        /// Insieme di file precedentemente compressi
        /// </summary>
        /// <param name="name">Nome del file</param>
        /// <returns>Percorso del file compresso</returns>
        public string this[string name]
        {
            get {
                for (int i = 0; i < this.Count; i++)    //Fra tutti gli elementi
                {
                    if (_files[i] == name) return this[i];  //Se trovo un file con lo stesso nome richiamo la proprietà con indice
                }
                return null;
            }
        }

        //Medoti pubblici
        /// <summary>
        /// Permette l'eliminazione di ogni elemento
        /// </summary>
        public void EraseObject()
        {
            try
            {
                Directory.Delete(_folderPath, true);    //Elimina la cartella ricorsivamente
            }
            catch (PathTooLongException) { }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (ArgumentNullException) { }
            catch (ArgumentException) { }
            finally
            {
                _files = null;
                _folderPath = null;
            }
        }

        //Metodi statici
        /// <summary>
        /// Data una cartella la esplora nel suo primo livello individuando i file e creando un UnzipObject
        /// </summary>
        /// <param name="PathOfUnzippedFolder">Percorso della cartella</param>
        /// <returns>Oggetti decompressi</returns>
        public static UnzipObject CreateUnzipObject(string PathOfUnzippedFolder)
        {
            if (Directory.Exists(PathOfUnzippedFolder)) //Se la cartella esiste
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(PathOfUnzippedFolder); //Creo una lista di files
                }
                catch (IOException) { return null; }
                catch (UnauthorizedAccessException) { return null; }
                catch (ArgumentNullException) { return null; }
                catch (ArgumentException) { return null; }

                string[] name = new string[files.Length];   //Creo la lista dei nomi

                //Per ogni file presente nella lista elimina la cartella di appartenenza individuandone il singolo nome
                for (int i = 0; i < files.Length; i++) name[i] = files[i].Substring(PathOfUnzippedFolder.Length + 1, files[i].Length - PathOfUnzippedFolder.Length - 1);

                return new UnzipObject(PathOfUnzippedFolder, name);
            }
            else return null;
        }

        //Metodi override
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public override bool Equals(object obj)
        {
            return (this.GetType() == obj.GetType() && Equals((UnzipObject)obj));
        }
        /// <summary>
        /// Permette la comparazione di due istanze della classe
        /// </summary>
        /// <param name="obj">Oggetto da confrontare</param>
        /// <returns>Risultato della comparazione</returns>
        public bool Equals(UnzipObject obj)
        {
            return (this._folderPath == obj._folderPath && this._files == obj._files);
        }
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret = "[UnzipObject: path: " + this.UnzipFolderPath + " files: ";
            for (int i = 0; i < this.Count; i++) { ret += _files[i] + ", "; }
            return ret.Remove(ret.Length-2) + " ]";
        }
        /// <summary>
        /// Funge da funzione hash per un determinato tipo
        /// </summary>
        /// <returns>Codice Hash</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
