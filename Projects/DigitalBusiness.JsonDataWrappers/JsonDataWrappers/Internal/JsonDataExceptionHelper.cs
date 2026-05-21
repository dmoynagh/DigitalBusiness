using DigitalBusiness.JsonDataWrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Internal
{
    public static class JsonDataExceptionHelper
    {
        public static Exception ReadOnlyException()
           => new InvalidOperationException("Cannot modify read-only JsonData.");

        public static Exception NullException() => new InvalidOperationException("JsonData is null.");

        public static Exception GetTypedValueException<T>(JsonData jsonData)
        {
            if(jsonData.IsNull)  return NullException();
            return new InvalidOperationException($"Cannot get value of type {typeof(T).FullName} from JsonData with ValueKind {jsonData.ValueKind}.");
        }

        public static Exception GetTypedPropertyException<T>(string propertyName, JsonData objectJsonData)
        {
            if (objectJsonData.IsNull) return NullException();
            if(objectJsonData.ContainsProperty(propertyName)) return JsonDataPropertyNotExist(propertyName);
            return new InvalidOperationException($"Cannot get value of type {typeof(T).FullName} from property '{propertyName}' with ValueKind {objectJsonData[propertyName]?.ValueKind.ToString() ?? "null"}.");
        }

        public static Exception GetTypedIndexException<T>(int index, JsonData arrayJsonData)
        {
            if (arrayJsonData.IsNull) return NullException();
            if (arrayJsonData.IsArray) return JsonDataArrayExpectedException();

            if (index < 0 || index > arrayJsonData.Count) return JsonDataArrayIndexOutOfRangeException(index);

            return new InvalidOperationException($"Cannot get value of type {typeof(T).FullName} from index '{index}' with ValueKind {arrayJsonData[index]?.ValueKind.ToString() ?? "null"}.");
        }

        public static Exception JsonDataArrayIndexOutOfRangeException(int index) => new IndexOutOfRangeException($"Index '{index}' is out of range. .");

        public static Exception JsonDataObjectExpectedException()=> new InvalidOperationException("JsonData is not an object.");
        public static Exception JsonDataArrayExpectedException() => new InvalidOperationException("JsonData is not an object.");


        public static Exception PropertyExistsAndNotArrayException(string propertyName)=> new InvalidOperationException($"Property '{propertyName}' exists but is not an array.");
        public static Exception PropertyExistsAndNotObjectException(string propertyName)=> new InvalidOperationException($"Property '{propertyName}' exists but is not an object.");

        public static Exception IndexExistsAndNotArrayException(int index) => new InvalidOperationException($"Index '{index}' exists but is not an array.");
        public static Exception IndexExistsAndNotObjectException(int index) => new InvalidOperationException($"Index '{index}' exists but is not an object.");


        public static Exception JsonDataPropertyNotExist(string propertyName)
        {
            return new InvalidOperationException($"Property '{propertyName}' does not exist.");
        }






        //public static Exception ArrayRequiredException(bool isNull) => 
        //    isNull ? new InvalidOperationException("Expected a JSON array but the JNode is null or value IsNull.")
        //    : new InvalidOperationException("Expected a JSON array but the JNode is not an array.");

        //public static Exception ObjectRequiredException(bool isNull) =>
        //    isNull ? new InvalidOperationException("Expected a JSON object but the JNode is null or value IsNull.")
        //    : new InvalidOperationException("Expected a JSON object but the JNode is not an object.");


        //public static Exception GetNodeValueException<T>(JsonData node) => new InvalidOperationException($"Cannot get value of type ${typeof(T).FullName} from Node type ${node.Source?.GetType().FullName ?? "null"}");


        public static Exception GetNodeValueException(Type type, JsonData node) => new InvalidOperationException($"Cannot get value of type ${type.FullName} from Node type ${node.Source?.GetType().FullName ?? "null"}");

        public static Exception GetRequiredValueException(bool allowNullNodes)
            => new InvalidOperationException($"Required node is null{(!allowNullNodes ? " or is a Null JNode." : "")}.");

    }
}
