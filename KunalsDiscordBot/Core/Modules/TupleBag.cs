using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules
{
    public class TupleBag<Item1, Item2> 
    {
        private List<CustomTuple<Item1, Item2>> Values { get; set; }

        public TupleBag()
        {
            Values = new List<CustomTuple<Item1, Item2>>();
        }

        public TupleBag(List<(Item1, Item2)> values)
        {
            Values = values.Select(x => new CustomTuple<Item1, Item2>(x.Item1, x.Item2)).ToList();
        }

        public TupleBag(List<CustomTuple<Item1, Item2>> values)
        {
            Values = values;
        }

        public CustomTuple<Item1, Item2> this[int index] { get => Values[index]; }

        public int Count
        {
            get => Values.Count;
        }

        public void Add(Item1 key, Item2 value) => Values.Add(new CustomTuple<Item1, Item2>(key, value));

        public void Clear() => Values = new List<CustomTuple<Item1, Item2>>();

        public void Insert(int index, Item1 key, Item2 value) => Values.Insert(index, new CustomTuple<Item1, Item2>(key, value));

        public void RemoveAt(int index) => Values.RemoveAt(index);
    }
}
