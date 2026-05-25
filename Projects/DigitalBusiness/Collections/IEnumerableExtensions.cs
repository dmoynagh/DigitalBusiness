using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBusiness.Collections
{
    public static class IEnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            int index = 0;
            foreach(var item in items)
            {
                if (predicate(item))
                {
                    return index;
                }
                index++;
            }   
            return -1;

        }       
            
    }
}
