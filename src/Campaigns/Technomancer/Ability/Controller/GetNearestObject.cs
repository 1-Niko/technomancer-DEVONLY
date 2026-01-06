// Campaigns/Technomancer/Ability/Controller/GetNearestObject.cs
/* Identifies the closest object within a list of manipulatable objects */

using System.Collections.Generic;
using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static ManipulatableObject GetNearestObject(List<ManipulatableObject> objects, Vector2 pos)
        {
            float lowest_dist = float.MaxValue;
            ManipulatableObject nearest = null;
            for (int i = 0; i < objects.Count; i++) {
                float candidate = Vector2.Distance(pos, objects[i].pos);
                if (lowest_dist > candidate) {
                    lowest_dist = candidate;
                    nearest = objects[i];
                }
            }
            return nearest;
        }
    }
}