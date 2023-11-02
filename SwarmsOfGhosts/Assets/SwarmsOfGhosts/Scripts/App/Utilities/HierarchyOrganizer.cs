using System.Collections.Generic;
using UnityEngine;

namespace SwarmsOfGhosts.App.Utilities
{
    public interface IHierarchyOrganizer
    {
        public void AddChild(Transform parent, Transform child, int order);
    }

    public class HierarchyOrganizer : IHierarchyOrganizer
    {
        private class OrderComparer : IComparer<(int Order, Transform)>
        {
            public int Compare((int Order, Transform) x, (int Order, Transform) y) => x.Order.CompareTo(y.Order);
        }

        private readonly Dictionary<Transform, List<(int, Transform Child)>> _hierarchy =
            new Dictionary<Transform, List<(int, Transform)>>();

        private readonly OrderComparer _orderComparer = new OrderComparer();

        public void AddChild(Transform parent, Transform child, int order)
        {
            if (_hierarchy.TryGetValue(parent, out var value))
            {
                value.Add((order, child));
                value.Sort(_orderComparer);

                for (var i = 0; i < value.Count; i++)
                    value[i].Child.SetSiblingIndex(i);
            }
            else
            {
                _hierarchy.Add(parent, new List<(int, Transform)>() { (order, child) });
            }
        }
    }
}