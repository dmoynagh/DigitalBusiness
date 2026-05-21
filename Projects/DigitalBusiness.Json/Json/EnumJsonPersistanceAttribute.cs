using DigitalBusiness.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Json
{
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public class EnumJsonPersistanceAttribute : Attribute
    {
        public EnumJsonPersistanceAttribute() { }
    
        public static bool PersistAsNumber<T>() => typeof(T).HasAttribute<EnumJsonPersistanceAttribute>();

        public static bool PersistEnumAsNumber(Type enumType) => enumType.HasAttribute<EnumJsonPersistanceAttribute>();
    }
}
