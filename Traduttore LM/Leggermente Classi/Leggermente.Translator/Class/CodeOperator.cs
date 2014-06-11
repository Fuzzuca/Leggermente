using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Leggermente.Translator
{
    public static class CodeCleaner
    {
        /// <summary>
        /// Elimina tutti i commenti dal codice e segnala la presenza di eventuali errori sintattici
        /// </summary>
        /// <param name="RawCode">Testo grezzo, con possibili commenti</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>restituisce il testo senza commenti o in caso di errore NULL</returns>
        public static string CommentDelete(string RawCode, LogManager lm)
        {
            string ret;
            try
            {
                ret = Regex.Replace(RawCode, @"\|.*?\|", string.Empty, RegexOptions.Singleline);    //Rimuove tutti i commenti
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the comments in the Code"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the comments but something gone wrong"); return null; }

            Match _m = Regex.Match(ret, @"^.*?\|", RegexOptions.Singleline); //Se trova un simbolo di inizio commento
            if (_m.Success)
            {
                lm.Add("The translator cannot close the '|' symbol", _m.Value.Split('\n').Length + 1); return null;
            }
            else return ret;
        }
        /// <summary>
        /// Funzione che permette di rimuovere le tabulazioni
        /// </summary>
        /// <param name="RawCode">Testo con tabulazioni</param>
        /// <param name="lm">Gestore dei Messaggi</param>
        /// <returns>Restituisce un testo senza tabulazioni o, in caso di errore, NULL</returns>
        public static string RemoveTabulation(string RawCode, LogManager lm)
        {
            try
            {
                return Regex.Replace(RawCode, @"\t", string.Empty, RegexOptions.Singleline);
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }
        }
        /// <summary>
        /// Unficica i ritorni a capo in stile UNIX
        /// </summary>
        /// <param name="RawCode">Codice grezzo</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Codice unificato</returns>
        public static string TextReturnUnified(string RawCode, LogManager lm)
        {
            try
            {
                return Regex.Replace(RawCode, "\r\n", "\n", RegexOptions.Singleline); //unifica i ritorni a capo
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to unified the text return"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to unified the text return but something gone wrong"); return null; }
        }
        /// <summary>
        /// Unifica gli spazi in tabulazioni all'inizio di una riga di codice
        /// </summary>
        /// <param name="RawCode">Codice grezzo</param>
        /// <param name="EqualSpace">Numero di spazi da unificare</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Codice unificato</returns>
        public static string StartLineUnified(string RawCode, int EqualSpace, LogManager lm)
        {
            string header;
            string spaces = "";
            int decrescita = 0;
            int sup;
            MatchCollection ms;

            for (int i = 0; i < EqualSpace; i++) spaces += " ";

            try
            {
                ms = Regex.Matches(RawCode, @"^[\t ]+", RegexOptions.Multiline);
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to find all the white space at the line start"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to find all the white space at the line start but something gone wrong"); return null; }

            foreach (Match m in ms)
            {
                header = m.Value;
                RawCode = RawCode.Remove(m.Index - decrescita, m.Length);

                try
                {
                    header = Regex.Replace(header, @"\t", spaces, RegexOptions.Singleline);
                    header = Regex.Replace(header, @" {" + EqualSpace.ToString() + "}", "\t", RegexOptions.Singleline);
                }
                //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to  replace white spaces with tabulation"); return null; }
                catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to replace white spaces with tabulation but something gone wrong"); return null; }

                sup = header.Length;
                header = header.TrimEnd(' ');
                if (sup != header.Length) lm.Add("There are a useless white space in the tabulation", RawCode.Substring(0, m.Index).Split('\n').Length, LogType.Warning);

                RawCode = RawCode.Insert(m.Index - decrescita, header);
                decrescita += (m.Length > header.Length) ? m.Length - header.Length : header.Length - m.Length; //Delta fra i due elementi
            }
            return RawCode;
        }
        /// <summary>
        /// Ricude gli spazi multipli in spazi singoli
        /// </summary>
        /// <param name="RawCode">Codice grezzo</param>
        /// <param name="lm">Gestore di messagi</param>
        /// <returns>Codice Elaborato</returns>
        public static string ReduceWitheSpaces(string RawCode, LogManager lm)
        {
            try
            {
                return Regex.Replace(RawCode, @" +", " ", RegexOptions.Singleline); //La tabulazione diventa spazio
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }
        }
        /// <summary>
        /// Dato un nome di una variabile, di una funzione o di un parametro controllo la sua integrità
        /// </summary>
        /// <param name="Name">Nome da controllare</param>
        /// <returns>Pertinenza con il controllo</returns>
        public static bool CheckNameIntegrity(string Name)
        {
            return Regex.Match(Name, @"^([_a-z][\w]*)?", RegexOptions.IgnoreCase).Success;
        }
        /// <summary>
        /// Rimpiazza le parole chiave sinonimo
        /// </summary>
        /// <param name="RawCode">Codice da elaborare</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Codice elaborato</returns>
        public static string RemoveSynonyms(string RawCode, LogManager lm)
        {
            try
            {
                RawCode = Regex.Replace(RawCode, @" di ", " ", RegexOptions.IgnoreCase);    //Rimuove tutti i sinonimo
                RawCode = Regex.Replace(RawCode, @" con ", " ", RegexOptions.IgnoreCase);    //Rimuove tutti i sinonimo
                RawCode = Regex.Replace(RawCode, @" a ", " ", RegexOptions.IgnoreCase);    //Rimuove tutti i sinonimo
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the Synonym in the Code"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the Synonyms but something gone wrong"); return null; }
            return RawCode;
        }
        /// <summary>
        /// Riduce gli spazi multipli, toglie le tabulazioni e elimina i carattere alle estremità della stringa
        /// </summary>
        /// <param name="RawCode">Codice da elaborare</param>
        /// <param name="lm">Gestore dei messaggi</param>
        /// <returns>Testo ripulito</returns>
        public static string RemoveWhiteSpace(string RawCode, LogManager lm)
        {
            try
            {
                return Regex.Replace(RawCode, @"\s", string.Empty, RegexOptions.Singleline);
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }
        }
        /// <summary>
        /// Rimuove la firma della funzione da un pezzo di codice
        /// </summary>
        /// <param name="RawCode">Codice grezzo</param>
        /// <param name="lm">Gesore di nessaggi</param>
        /// <returns>Codice senza firma</returns>
        public static string RemoveFunctionSign(string RawCode, LogManager lm)
        {
            Match m;
            try
            {
                m = Regex.Match(RawCode, @"#(.*)\[", RegexOptions.None);
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }

            if (m.Success)
            {
                RawCode = RawCode.Remove(m.Index, m.Length);
                return RawCode.Remove(RawCode.IndexOf(']'), 1);
            }

            return RawCode;
        }
        
    }

    public static class CodeElaborator
    {
        #region Costanti
        //Regex.Replace(RawCode, @"@#(?:[^#]+|##)*#|#(?:[^#\\]+|\\.)*#".Replace('#', '\"'), "#", RegexOptions.None)

        static public VariableCollection ExtractConstant(ref string RawCode, LogManager lm)
        {
            VariableCollection ret = new VariableCollection();
            Match m;
            int indexr = 0;

            do
            {
                m = Regex.Match(RawCode, @"@#(?:[^#]+|##)*#|#(?:[^#\\]+|\\.)*#".Replace('#', '\"'), RegexOptions.Singleline);
                if (m.Success && m.Length > 0)
                {
                    ret.Add(new Variable("txtConst" + indexr.ToString(), m.Value));
                    RawCode = RawCode.Remove(m.Index , m.Length);
                    RawCode = RawCode.Insert(m.Index , "txtConst" + indexr.ToString());
                    indexr++;
                }
            } while (m.Success && m.Length > 0);

            indexr = 0;

            do
            {
                m = Regex.Match(RawCode, @"[ ,]\d+([.]?\d+)?(\^?\d+)?", RegexOptions.Singleline);
                if (m.Success && m.Length > 0)
                {
                    ret.Add(new Variable("numConst" + indexr.ToString(), m.Value.TrimStart(' ', ',')));
                    RawCode = RawCode.Remove(m.Index + 1, m.Length - 1);
                    RawCode = RawCode.Insert(m.Index + 1, "numConst" + indexr.ToString());
                    indexr++;
                }
            } while (m.Success && m.Length > 0);

            return ret;
        }


        #endregion

        #region Pacchetti
        public static string PackegeNameExtractor(string RawCode, LogManager lm)
        {
            Match _match = Regex.Match(RawCode, @"\spacchetto [\w\d]+\s", RegexOptions.IgnoreCase);
            if (_match.Success)
            {
                RawCode = _match.Value.Remove(0, " pacchetto ".Length);
                RawCode = RawCode.Remove(RawCode.Length - 1, 1);
                return RawCode;
            }
            return null;
        }

        public static string RemovePackageName(string RawCode, LogManager lm)
        {
            return Regex.Replace(RawCode, @"pacchetto [\w\d]+", string.Empty, RegexOptions.IgnoreCase);
        }
        #endregion

        #region Include
        /// <summary>
        /// Trova e estrapola tutti i pacchetti da includere nel codice
        /// </summary>
        /// <param name="RawCode">Codice da analizzare</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Lista di pacchetti inclusi</returns>
        public static string[] IncludeFinder(string RawCode, LogManager lm)
        {
            MatchCollection ms;

            try
            {
                ms = Regex.Matches(RawCode, @"\s?aggiungi [a-zA-Z0-9]+\s?", RegexOptions.IgnoreCase); //La tabulazione diventa spazio
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }

            string[] names = new string[ms.Count];

            for (int i = ms.Count - 1; i >= 0; i--)
            {
                names[i] = ms[i].Value.Remove(0, "aggiungi ".Length).Trim('\n');
            }

            return names;
        }
        /// <summary>
        /// Rimuove tutte le inclusioni di pacchetti dal codice
        /// </summary>
        /// <param name="RawCode">Codice grezzo</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Codice elaborato</returns>
        public static string RemoveInclude(string RawCode, LogManager lm)
        {
            MatchCollection ms;

            try
            {
                ms = Regex.Matches(RawCode, @"aggiungi [a-zA-Z0-9]+", RegexOptions.Singleline); //La tabulazione diventa spazio
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }

            for (int i = ms.Count - 1; i >= 0; i--)
            {
                RawCode = RawCode.Remove(ms[i].Index, ms[i].Length);
            }

            return RawCode;
        }
        #endregion

        /// <summary>
        /// Dato un codice e un tipo di compilazione controlla la presenza di tutti gli elementi necessari
        /// </summary>
        /// <param name="RawCode">Codice da controllare</param>
        /// <param name="type">Tipo di compilazione</param>
        /// <param name="lm">Gestore di Messaggi</param>
        /// <returns>Restituisce la correttezza degli elementi nel codice</returns>
        public static bool CheckIfCorrectType(string RawCode, CodeType type, LogManager lm)
        {
            if (CodeType.Package == type)
            {
                if (Regex.Match(RawCode, @"pacchetto (.?)*\b", RegexOptions.IgnoreCase).Success)
                {
                    if (!Regex.Match(RawCode, @"#programma", RegexOptions.IgnoreCase).Success) return true;
                    else return lm.Add("The package contains a '#programma' definitions");
                }
                return lm.Add("The package not cointains the package name");
            }
            if (CodeType.Program == type)
            {
                if (Regex.Match(RawCode, @"#programma", RegexOptions.IgnoreCase).Success)
                {
                    if (!Regex.Match(RawCode, @"pacchetto (.?)*\b", RegexOptions.IgnoreCase).Success)
                    {
                        if (Regex.Matches(RawCode, @"#programma", RegexOptions.IgnoreCase).Count == 1) return true;
                        else return lm.Add("The program more than one '#programma' definitions");
                    }
                    else return lm.Add("The program contains a 'pacchetto' definitions");
                }
                return lm.Add("The program not cointains the '#programma' sign functions");
            }
            return lm.Add("Was specified a type of data not aknologed");
        }

        #region Funzioni
        /// <summary>
        /// Trova nel codice gli indici delle funzioni
        /// </summary>
        /// <param name="RawCode">Codice da analizzare</param>
        /// <param name="lm">Gestore di messaggi</param>
        /// <returns>Indice della funzione</returns>
        public static CodeIndex[] FindFunctionPosition(string RawCode, LogManager lm)
        {
            MatchCollection ms;
            CodeIndex[] ci;
            try
            {
                ms = Regex.Matches(RawCode, @"#\s?\w+\s?(\([\w,\s]+\))?\s?\[[\s\S]*\]", RegexOptions.Singleline);
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to find all the white space at the line start"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to find all the white space at the line start but something gone wrong"); return null; }

            ci = new CodeIndex[ms.Count];

            for (int i = 0; i < ms.Count; i++)
            {
                string prova = ms[i].Value;
                ci[i] = new CodeIndex(ms[i].Index, ms[i].Length);
            }

            return ci;
        }
        /// <summary>
        /// Rimuove la firma della funzione
        /// </summary>
        /// <param name="RawCode">Codice da lavorare</param>
        /// <param name="lm">Gestore dei Messaggi</param>
        /// <returns>Testo elaborato</returns>
        public static string RemoveFunctionSign(string RawCode, LogManager lm)
        {
            int linesInSign;
            try
            {
                linesInSign = Regex.Match(RawCode, @"#(.*)\s*\[", RegexOptions.None).Value.Split('\n').Length-1;
                RawCode = Regex.Replace(RawCode, @"#(.*)\s*\[", string.Empty, RegexOptions.None);
                RawCode = Regex.Replace(RawCode, @"\s*\]$", string.Empty, RegexOptions.None);
            }
            //catch (RegexMatchTimeoutException) { lm.Add("The translator was unable to remove all the tabulation"); return null; }
            catch (ArgumentOutOfRangeException) { lm.Add("The translator had try to remove all the tabulation but something gone wrong"); return null; }

            for (int i = 0; i < linesInSign; i++) RawCode = RawCode.Insert(0, "\n");
            return RawCode;
        }
        #endregion
    }
}