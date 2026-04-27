using System;
using System.Collections.Generic;

namespace VarelaAloisio.Core
{
    public static class Service
    {
        public static Dictionary<Type, object> Services { get; } = new();
        public static void Add(Type type, object service,
                               bool overrideIfFound = false)
        {
            if (!Services.TryAdd(type, service) && overrideIfFound)
                Services[type] = service;
        }
        public static void Add<T>(T service, bool overrideIfFound = false)
        {
            if (!Services.TryAdd(typeof(T), service) && overrideIfFound)
                Services[typeof(T)] = service;
        }

        public static bool Remove(Type type)
            => Services.Remove(type);

        public static bool Remove<T>()
            => Services.Remove(typeof(T));

        public static object Get(Type type)
            => Services.GetValueOrDefault(type);

        public static bool TryGet<T>(out T service) where T : class
        {
            if (Services.TryGetValue(typeof(T), out var serviceObject)
                && serviceObject is T tService)
            {
                service = tService;
                return true;
            }

            service = null;
            return false;
        }
    }
}