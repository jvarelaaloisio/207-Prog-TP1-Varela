using System.Collections.Generic;
using UnityEngine;

namespace Core.Extensions
{
    public static class TransformExtensions
    {
        public static List<Transform> FetchChildrenWithTag(this Transform parent, string tag)
        {
            var result = new List<Transform>();

            foreach (Transform child in parent)
            {
                if (child.CompareTag(tag))
                    result.Add(child);

                result.AddRange(child.FetchChildrenWithTag(tag));
            }

            return result;
        }
    }
}