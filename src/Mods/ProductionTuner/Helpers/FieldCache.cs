using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Milex.GMS1.Mods.ProductionTuner.Helpers
{
    /// <summary>
    /// Thread-safe reflection member cache to eliminate repetitive reflection overhead in runtime loops.
    /// </summary>
    public static class FieldCache
    {
        private static readonly Dictionary<(Type, string), FieldInfo> FieldMap = new Dictionary<(Type, string), FieldInfo>();
        private static readonly Dictionary<(Type, string), PropertyInfo> PropertyMap = new Dictionary<(Type, string), PropertyInfo>();
        private static readonly Dictionary<(Type, string), MethodInfo> MethodMap = new Dictionary<(Type, string), MethodInfo>();
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Retrieves and caches a field from a given type.
        /// </summary>
        public static FieldInfo GetField(Type type, string fieldName)
        {
            if (type == null || string.IsNullOrEmpty(fieldName)) return null;

            var key = (type, fieldName);
            lock (SyncRoot)
            {
                if (FieldMap.TryGetValue(key, out var field)) return field;

                field = AccessTools.Field(type, fieldName);
                FieldMap[key] = field;
                return field;
            }
        }

        /// <summary>
        /// Retrieves and caches a property from a given type.
        /// </summary>
        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            if (type == null || string.IsNullOrEmpty(propertyName)) return null;

            var key = (type, propertyName);
            lock (SyncRoot)
            {
                if (PropertyMap.TryGetValue(key, out var property)) return property;

                property = AccessTools.Property(type, propertyName);
                PropertyMap[key] = property;
                return property;
            }
        }

        /// <summary>
        /// Retrieves and caches a parameterless or primary method from a given type.
        /// </summary>
        public static MethodInfo GetMethod(Type type, string methodName)
        {
            if (type == null || string.IsNullOrEmpty(methodName)) return null;

            var key = (type, methodName);
            lock (SyncRoot)
            {
                if (MethodMap.TryGetValue(key, out var method)) return method;

                method = AccessTools.Method(type, methodName);
                MethodMap[key] = method;
                return method;
            }
        }
    }
}
