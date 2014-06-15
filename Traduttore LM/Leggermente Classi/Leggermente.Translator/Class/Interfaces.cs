using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leggermente.Translator 
{
    /// <summary>
    /// Interfaccia che rappresenta l'oggetto traduttore
    /// </summary>
    public interface ITraslator
    {
        //Proprietà
        /// <summary>
        /// Lista dei messaggi di Log
        /// </summary>
        LogManager ErrorManager
        {
            set;
        }

        //Metodi
        /// <summary>
        /// Funzione per la traduzione del codice in eseguibile
        /// </summary>
        /// <param name="type">Tipo di output ottenuto</param>
        /// <param name="Code">Codice da tradurre</param>
        /// <param name="PackagePath">Lista di pacchetti d'aggiungere</param>
        /// <param name="ExportPath">Percorso di uscita del file</param>
        /// <returns>Restituisce il risultato della traduzione</returns>
        ResultCode Translate(CodeType type, string Code, string[] PackagePath, string ExportPath);
    }

    /// <summary>
    /// Tipo di Codice da tradurre
    /// </summary>
    public enum CodeType
    {
        Program,
        Package
    }

    /// <summary>
    /// Lista di messaggi di Log
    /// </summary>
    public interface ILogError
    {
        //Proprietà
        /// <summary>
        /// Indica se la classe non contiene alcun messaggio di errore
        /// </summary>
        bool WithOutError
        {
            get;
        }
        /// <summary>
        /// Lista di messaggi presenti nel sistema
        /// </summary>
        string[] LogList
        {
            get;
        }
    }
}
