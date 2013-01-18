using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Reflection;

namespace SimpleActionHandler
{
    public static class Extensions
    {
        public static void CopyTo(this Stream input, Stream output)
        {
            byte[] buffer = new byte[32768]; // Fairly arbitrary size
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        public static T CastTo<T>(this object value, T targetType)
        {
            return (T)value;
        }

        public static T Cast<T>(object value)
        {
            return (T)value;
        }

        public static T Convert<T>(this string self)
        {
            if (self == string.Empty)
                return default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                return default(T);
            else
                return (T)converter.ConvertFromString(self);
        }

        public static T GetOrElse<T>(this T getValue, T orElseValue)
        {
            return getValue == null ? orElseValue : getValue;
        }

        public static T GetOrElse<T>(this object getValue) where T : new()
        {
            return getValue == null ? new T() : (T)getValue;
        }

        public static T GetOrElse<T>(this object getValue, Func<T> elseFunc)
        {
            return getValue == null ? elseFunc.Invoke() : (T)getValue;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToPairs(this System.Collections.Specialized.NameValueCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            return collection.Cast<string>().Select(key => new KeyValuePair<string, string>(key, collection[key]));
        }

        public static void Rethrow(this Exception ex)
        {
            typeof(Exception).GetMethod("PrepForRemoting",
                BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(ex, new object[0]);
            throw ex;
        }

    }
}
