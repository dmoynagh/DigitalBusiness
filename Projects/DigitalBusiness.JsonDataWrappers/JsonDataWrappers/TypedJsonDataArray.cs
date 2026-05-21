using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    public readonly struct JsonDataArray<T> : IJsonDataWrapper, IEnumerable<T>
    {
        public JsonData Json
        {
            get;
            init { if (value.IsArray) field = value; else throw new ArgumentException("Value must be an array."); }
        }

        public bool ReadOnly => Json.ReadOnly;
        public int Count => Json.Count;

        public T? this[int index]
        {
            get => Json.TryGet<T>(index);
            set => Json.Set(index, value);
        }

        public void Add(T item)
        {
            Json.Add<T>(item);
        }

        public void Insert(int index, T item)
        {
            Json.Insert(index, item);
        }

        public void Clear()
        {
            Json.Clear();
        }

        public void RemoveAt(int index)
        {
            Json.RemoveAt(index);
        }

        private IEnumerable<T> Items => Json.Items.Select(jsonDataItem => jsonDataItem.Get<T>());


        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }

}
