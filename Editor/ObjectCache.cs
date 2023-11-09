using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace gomoru.su.LightController
{
    internal sealed class ObjectCache : IDisposable
    {
        public readonly Object Container;
        private List<Item> _items = new List<Item>();

        public ObjectCache(Object assetContainer)
        {
            Container = assetContainer;
        }

        public void Add(Object item, bool forceFlush = false)
        {
            if (Container == null)
                return;

            foreach (ref readonly var x in _items.AsSpan())
            {
                if (x.Object == item)
                    return;
            }
            if (forceFlush)
                AssetDatabase.AddObjectToAsset(item, Container);
            _items.Add(new Item(item, forceFlush));
        }

        public void Flush()
        {
            if (Container == null)
                return;

            bool flushed = false;
            foreach (ref var item in _items.AsSpan())
            {
                if (item.Flushed)
                    continue;
                AssetDatabase.AddObjectToAsset(item.Object, Container);
                item.Flushed = true;
                flushed = true;
            }
            if (flushed)
                AssetDatabase.SaveAssets();
        }

        public void Dispose()
        {
            Flush();
        }

        private struct Item
        {
            public Object Object;
            public bool Flushed;

            public Item(Object item, bool flushed)
            {
                Object = item;
                Flushed = flushed;
            }

            public override int GetHashCode() => Object.GetHashCode();
        }

        private sealed class ItemComparer : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y) => x.Object == y.Object;

            public int GetHashCode(Item obj) => obj.GetHashCode();
        }
    }
}