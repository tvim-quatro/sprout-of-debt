using System;
using System.Collections.Generic;
using UnityEngine;

namespace DrugCrafter.Laboratory
{
    public class TempInventory : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public ScriptableObject item;
            public int count;
        }

        public List<Entry> items = new List<Entry>();

        public int GetCount(ScriptableObject item)
        {
            Entry entry = items.Find(e => e.item == item);
            if (entry == null)
            {
                return 0;
            }
            return entry.count;
        }

        public void Add(ScriptableObject item, int amount)
        {
            Entry entry = items.Find(e => e.item == item);
            if (entry == null)
            {
                entry = new Entry();
                entry.item = item;
                items.Add(entry);
            }
            entry.count += amount;
        }

        public bool Remove(ScriptableObject item, int amount)
        {
            Entry entry = items.Find(e => e.item == item);
            if (entry == null || entry.count < amount)
            {
                return false;
            }
            entry.count -= amount;
            return true;
        }
    }
}
