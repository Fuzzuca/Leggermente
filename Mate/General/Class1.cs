using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Leggermente.lib
{
    #region Variabili
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

        public string GetValue(){
            return value.ToString();
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
                    Variable[] err = new Variable[2];
                    err[0] = left;
                    err[1] = right;
                    left.CriticalError(left, "+", err, DateTime.Now);
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
                    Variable[] err = new Variable[2];
                    err[0] = left;
                    err[1] = right;
                    left.CriticalError(left, "-", err, DateTime.Now);
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
                    Variable[] err = new Variable[2];
                    err[0] = left;
                    err[1] = right;
                    left.CriticalError(left, "*", err, DateTime.Now);
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
                        Variable[] err = new Variable[2];
                        err[0] = left;
                        err[1] = right;
                        left.CriticalError(left, "/", err, DateTime.Now);
                        return new Variable();
                    }
                }
                else
                {
                    Variable[] err = new Variable[2];
                    err[0] = left;
                    err[1] = right;
                    left.CriticalError(left, "/", err, DateTime.Now);
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
                    Variable[] err = new Variable[2];
                    err[0] = left;
                    err[1] = right;
                    left.CriticalError(left, ">", err, DateTime.Now);
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
                    Variable[] err = new Variable[2];
                    err[0] = left;
                    err[1] = right;
                    left.CriticalError(left, "<", err, DateTime.Now);
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
            return (this.GetType() == obj.GetType() && Equals((Variable)obj));
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

    public static class IO
    {
        public static Variable schermo()
        {
            Console.Clear();
            return new Variable("NULL", null);
        }

        public static Variable schermo() { return null; }

        /*public static Variable resto(Variable var1, Variable var2)
        {
            if (var1.Type == ValueType.Number && var2.Type == ValueType.Number)
            {
                int num1, num2;
                if (int.TryParse(var1.Value, out num1) && int.TryParse(var2.Value, out num2))
                {
                    return new Variable("return", (num1 % num2).ToString());
                }
                else return new Variable("NULL", null);
            }
            else return new Variable("NULL", null);
        }*/
    }

    public static class VAR
    {
    }
}
