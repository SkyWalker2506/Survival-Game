using System;
using System.Reflection;

namespace PolymindGamesEditor
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Returns the FieldInfo matching 'name' from either type 't' itself or its most-derived 
        /// base type (unlike 'System.Type.GetField'). Returns null if no match is found.
        /// </summary>
        public static FieldInfo GetPrivateField(this Type t, string name)
        {
            const BindingFlags FLAGS = BindingFlags.Instance |
                                       BindingFlags.NonPublic |
                                       BindingFlags.DeclaredOnly;

            int iterations = 0;

            FieldInfo fi;
            while ((fi = t.GetField(name, FLAGS)) == null && (t = t.BaseType) != null && iterations < 12)
                iterations++;
            return fi;
        }

        public static T GetPrivateFieldValue<T>(this object obj, string fieldName)
        {
            Type type = obj.GetType();
            return (T)type.GetPrivateField(fieldName).GetValue(obj);
        }

        public static void SetFieldValue(this object source, string fieldName, object value)
        {
            Type sourceType = source.GetType();
            FieldInfo field = sourceType.GetPrivateField(fieldName);
            field.SetValue(source, value);
        }
    }
}