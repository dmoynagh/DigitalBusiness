using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Reflection
{
    public static class AttributeExtensions
    {
        public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetCustomAttributes(typeof(TAttribute), inherit: false).Length > 0;
        }

        public static TAttribute? GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(TAttribute), inherit: false);
            if (attributes.Length > 0)
            {
                return attributes[0] as TAttribute;
            }
            return null;
        }
    }
}
