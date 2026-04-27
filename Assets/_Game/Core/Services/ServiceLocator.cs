using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Core.Services
{
    public static class ServiceLocator
    {
        private const string TAG = "<color=red>[ServiceLocator]</color>";

        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (!_services.ContainsKey(type))
            {
                _services.Add(type, service);
            }
            else
            {
                Debug.LogWarning($" {TAG} O serviço do tipo {type} já está registrado.");
            }
        }

        public static void Clear()
        {
            _services.Clear();
        }

        public static void Unregister<T>()
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            Debug.LogError($" {TAG} Serviço do tipo {type} não foi encontrado.");
            return default;
        }
    }
}