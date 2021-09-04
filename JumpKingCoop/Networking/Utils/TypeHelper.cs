using JumpKingCoop.Networking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Utils
{
    public static class TypeHelper
    {
        public static Type GetNetworkEntityStateType(string entityType)
        {
            Type requestedType = Assembly.GetExecutingAssembly().GetType(entityType);

            if (requestedType != null)
            {
                foreach (var method in requestedType.GetMethods())
                {
                    if (method.Name.Equals("GetEntityStateType") && method.IsStatic && method.ReturnType == typeof(Type))
                    {
                        var result = method.Invoke(null, null) as Type;
                        if (InheritsAbstractClass(result, typeof(EntityState)))
                            return result;
                    }
                }
            }
            return null;
        }

        public static bool InheritsInterface(string entityType, Type interfaceType)
        {
            Type requestedType = Assembly.GetExecutingAssembly().GetType(entityType);
            return InheritsInterface(requestedType, interfaceType);
        }

        public static bool InheritsInterface(Type requestedType, Type interfaceType)
        {
            if(requestedType != null)
                return requestedType.GetInterfaces().Any(t => t.Equals(interfaceType));
            return false;
        }

        public static bool InheritsAbstractClass(string entityType, Type abstractClass)
        {
            Type requestedType = Assembly.GetExecutingAssembly().GetType(entityType);
            return InheritsAbstractClass(requestedType, abstractClass);
        }

        public static bool InheritsAbstractClass(Type requestedType, Type abstractClass)
        {
            if (requestedType != null)
                return requestedType.IsSubclassOf(abstractClass);
            return false;
        }

        public static T InstantiateNetworkEntity<T>(string type) where T : INetworkEntity
        {
            Type requestedType = Assembly.GetExecutingAssembly().GetType(type);
            if(requestedType != null)
            {
                return (T)Activator.CreateInstance(requestedType);
            }
            return default(T);
        }
    }
}
