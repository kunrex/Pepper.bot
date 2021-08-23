using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class TupleBag<TKey, TValue> 
    {
        private List<CustomTuple<TKey, TValue>> Values { get; set; }

        public TupleBag()
        {
            Values = new List<CustomTuple<TKey, TValue>>();
        }

        public TupleBag(List<(TKey, TValue)> values)
        {
            Values = values.Select(x => new CustomTuple<TKey, TValue>(x.Item1, x.Item2)).ToList();
        }

        public TupleBag(List<CustomTuple<TKey, TValue>> values)
        {
            Values = values;
        }

        public CustomTuple<TKey, TValue> this[int index] { get => Values[index]; }

        public int Count
        {
            get => Values.Count;
        }

        public void Add(TKey key, TValue value) => Values.Add(new CustomTuple<TKey, TValue>(key, value));

        public void Clear() => Values = new List<CustomTuple<TKey, TValue>>();

        public void Insert(int index, TKey key, TValue value) => Values.Insert(index, new CustomTuple<TKey, TValue>(key, value));

        public void RemoveAt(int index) => Values.RemoveAt(index);
    }
}
