using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleActionHandler
{
    public class Params<Tkey, TValue> : Dictionary<Tkey, TValue>
    {
        new public TValue this[Tkey key]
        {
            get
            {
                if (base.ContainsKey(key))
                    return base[key];
                else
                    return default(TValue);
            }
            set
            {
                base[key] = value;
            }
        }

        public int GetInt(Tkey key)
        {
            if (this[key] == null)
                return default(int);
            else
                return int.Parse(this[key].ToString()); 
        }

        public int GetIntOr(Tkey key, int defaultValue)
        {
            if (GetInt(key) == default(int))
                return defaultValue;
            else
                return GetInt(key);
        }

        public string GetString(Tkey key)
        {
            if (this[key] == null)
                return default(string);
            else
                return this[key].ToString();
        }

        public string GetStringOr(Tkey key, string defaultValue)
        {
            if (GetString(key) == default(string))
                return defaultValue;
            else
                return GetString(key);
        }

        public T[] GetArray<T>(Tkey key)
        {
            if (this[key] == null)
                return new T[] { };
            else
                return ParseToArray<T>(this[key].ToString());
        }

        private T[] ParseToArray<T>(string value)
        {
            var splited = value.Split(new char[] { ',', ' ' });
            var trimmed = splited.Select(s => s.Trim());
            var casted = trimmed.Select(s => s.Convert<T>());
            var array = casted.ToArray();
            return array;
        }

        public T[] GetArrayOr<T>(Tkey key, T[] defaultValue)
        {
            if (this[key] == null)
                return defaultValue;
            else
                return GetArray<T>(key);
        }

        public bool GetBoolean(Tkey key)
        {
            if (this[key] == null)
                return false;

            var value = this[key].ToString();

            switch (value)
            {
                case "true":
                case "1":
                    return true;
                case "false":
                case "0":
                default:
                    return false;
            }
        }

        public void SetIfNull(Tkey key, TValue value)
        {
            if (this[key] == null)
                this[key] = value;
        }

    }
}
