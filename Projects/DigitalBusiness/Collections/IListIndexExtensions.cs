using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Collections
{
    public static class IListIndexExtensions
    {
        extension<T>(IList<T> list) 
        {
            public int LastIndexOf(Func<T, bool> predicate)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (predicate(list[i]))
                        return i;
                }
                return -1;
            }
        }
    }
}
