using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
namespace System
{
    public static class Extensions
    {
        public static IDictionary<TKey, TValue> Clone<TKey, TValue>
           (this IDictionary<TKey, TValue> original) where TValue : ICloneable
        {
            IDictionary<TKey, TValue> ret = (IDictionary<TKey, TValue>)Activator.CreateInstance(original.GetType());
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> mergedict)
        {
            foreach (var kv in mergedict)
            {
                var key = kv.Key;
                var value = kv.Value;
                if (!dict.ContainsKey(key)) { dict[key] = value; }
            }
        }
        public static IList<T> Clone<T>(this IList<T> listToClone)
                 where T : ICloneable
        {
            var list = (IList<T>)Activator.CreateInstance(listToClone.GetType());
            foreach (var item in listToClone)
            {
                list.Add((T)item.Clone());
            }
            return list;
        }
        public static T Cloneobj<T>(this T obj)
        {
            T objResult;
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);

                ms.Position = 0;
                objResult = (T)bf.Deserialize(ms);
            }
            return objResult;
        }
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            return source.RandomElement(1).Single();
        }

        public static IEnumerable<T> RandomElement<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }


        public static IEnumerable<int> Enumerate(this int inp)
        {
            return Enumerable.Range(1, inp);
        }
    }
}