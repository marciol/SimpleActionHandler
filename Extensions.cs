using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
    }
}
