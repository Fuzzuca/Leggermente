using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leggermente.Translator
{
    /// <summary>
    /// Maniglia per la gestione dell'evento ERRORE CRITICO implementato nelle varie classi
    /// </summary>
    /// <param name="sender">Oggetto che richiama la seguente funzione</param>
    /// <param name="when">Quando l'errore viene richiamato</param>
    /// <param name="message">Messaggio generato</param>
    public delegate void CriticalErrorHandler(object sender, DateTime when, string message);

    /// <summary>
    /// Classe per la gestione dei messaggi del compilatore
    /// </summary>
    public class LogManager : ILogError
    {
        //Campi
        private bool _errorFree;    //Indica la presenza di almeno un errore nel programma
        private List<string> _msgList;  //Lista delle dei messaggi presenti nel sistema

        //Costruttore
        /// <summary>
        /// Classe per la gestione dei messaggi del compilatore
        /// </summary>
        public LogManager()
        {
            _errorFree = true;
            _msgList = new List<string>();
        }

        //Proprietà
        /// <summary>
        /// Indica se la classe non contiene alcun messaggio di errore
        /// </summary>
        public bool WithOutError
        {
            get { return _errorFree; }
        }
        /// <summary>
        /// Lista di messaggi presenti nel sistema
        /// </summary>
        public string[] LogList
        {
            get { return listToArray(_msgList); }   //Converte la lista in array
        }

        //Eventi
        /// <summary>
        /// Errore critico del gestore di messaggi
        /// </summary>
        public event CriticalErrorHandler CriticalError;

        //Metodi privati
        /// <summary>
        /// Converte una lista in un Array
        /// </summary>
        /// <param name="lista">Lista di stringhe da convertire</param>
        /// <returns>Array di messaggi</returns>
        private string[] listToArray(List<string> lista)
        {
            if (lista == null) CriticalError(this, DateTime.Now, "There list of log to convert is set NULL");   //Richiamo l'errore

            string[] ret = new string[lista.Count]; //Creo una stringa di lunghezza pari alla lista
            for (int i = 0; i < lista.Count; i++) ret[i] = lista[i];    //Copio elemento per elemento
            return ret;
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio [valore non presente = -1]</param>
        /// <param name="charNumber">Numero di carattere a cui fà riferimento il messaggio [valore non presente = -1]</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="data">Possibilità di indicare un ora differente di quella attuale [ora attuale = null]</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        private bool add(string message, int lineNumber, int charNumber, bool addData, DateTime? data, LogType type)   //Il ? dà la possibilità di mettere la data a null
        {
            string head = "";
            bool ret = true;

            //Testa il tipo e aggiunge l'intestazione del messaggio oppure genera un errore
            if (type == LogType.Log) head += "LOG: ";
            else if (type == LogType.Warning) head += "WARNING: ";
            else if (type == LogType.Error) { head += "ERROR: "; ret = false; _errorFree = false; }
            else CriticalError(this, DateTime.Now, "There are a LogType not know");

            if (addData)    //Se è richiesta la data
                head += ((data == null) ? DateTime.Now.ToShortTimeString() : ((DateTime)data).ToShortTimeString()) + " "; //se data è a null mette l'ora attuale, altrimenti quella passata

            if (lineNumber > 0) //Se il numero è impostato sopra a -1 esiste una linea da indicare
            {
                head += lineNumber.ToString();

                if (charNumber > 0) //È inutile inserire il numero di carattere di una linea di cui non si conosce l'identificatore
                    head += ":" + charNumber.ToString();
            }

            _msgList.Add(head + ": " + message); //Aggiunta del messaggio con relativa intestazione

            return ret;
        }

        //Metodi pubblici
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, LogType type = LogType.Error)
        {
            return add(message, -1, -1, false, null, type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, int lineNumber,LogType type = LogType.Error)
        {
            return add(message,lineNumber,-1,false,null,type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio</param>
        /// <param name="charNumber">Numero di carattere a cui fà riferimento il messaggio</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message,int lineNumber,int charNumber, LogType type = LogType.Error)
        {
            return add(message,lineNumber,charNumber,false,null,type = LogType.Error);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message,bool addData, LogType type = LogType.Error)
        {
            return add(message, -1, -1, addData, null, type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, int lineNumber, bool addData, LogType type = LogType.Error)
        {
            return add(message, lineNumber, -1, addData, null, type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio</param>
        /// <param name="charNumber">Numero di carattere a cui fà riferimento il messaggio</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, int lineNumber, int charNumber, bool addData, LogType type = LogType.Error)
        {
            return add(message, lineNumber, charNumber, addData, null, type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="data">Possibilità di indicare un ora differente di quella attuale</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, bool addData, DateTime data, LogType type = LogType.Error)
        {
            return add(message, -1, -1, addData, data, type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="data">Possibilità di indicare un ora differente di quella attuale</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, int lineNumber, bool addData, DateTime data, LogType type = LogType.Error)
        {
            return add(message, lineNumber, -1, addData, data, type);
        }
        /// <summary>
        /// Funzione per l'aggiunta di un messaggio alla lista
        /// </summary>
        /// <param name="message">Mesaggio</param>
        /// <param name="lineNumber">Numero di linea a cui fà riferimento il messaggio</param>
        /// <param name="charNumber">Numero di carattere a cui fà riferimento il messaggio</param>
        /// <param name="addData">Indica se inserire oppure no la data e l'ora del messaggio</param>
        /// <param name="data">Possibilità di indicare un ora differente di quella attuale</param>
        /// <param name="type">Tipo di messaggio</param>
        /// <returns>Restituisce 'false' all'aggiunta di errori</returns>
        public bool Add(string message, int lineNumber, int charNumber, bool addData, DateTime data, LogType type = LogType.Error)
        {
            return add(message, lineNumber, charNumber, addData, data, type);
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
        public bool Equals(LogManager obj)
        {
            return (this._errorFree == obj._errorFree && this._msgList == obj._msgList);
        }
        /// <summary>
        /// Converte la classe in stringa
        /// </summary>
        /// <returns>Rappresentazione della classe in stringa</returns>
        public override string ToString()
        {
            string ret = "[LogManager: ErrorFree: " + this._errorFree + " Message: \n";
            string[] list = this.LogList;
            for (int i = 0; i < list.Length; i++) { ret += list[i] + ", \n"; }
            return ret.Remove(ret.Length - 3) + " ]";
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
    /// Tipi di Messaggio
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Messaggio di Log
        /// </summary>
        Log,
        /// <summary>
        /// Messaggio di Avvertimento
        /// </summary>
        Warning,
        /// <summary>
        /// Messaggio d'Errore
        /// </summary>
        Error,
    }
}