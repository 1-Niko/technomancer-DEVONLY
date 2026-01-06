// Campaigns/Technomancer/Ability/Controller/GetNearestObjectInDirection.cs
/* Identifies the closest object within a list of manipulatable objects given a specific direction */

using System.Collections.Generic;
using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static ManipulatableObject GetNearestObjectInDirection(List<ManipulatableObject> objects, Vector2 pos, Direction dir)
        {
            float lowest_dist_sq = float.MaxValue;
            ManipulatableObject nearest = null;

            for (int i = 0; i < objects.Count; i++) {
                Vector2 delta = objects[i].pos - pos;

                if (delta == Vector2.zero) continue;

                bool isInDirection = false;

                switch (dir) {
                    case Direction.Up:
                        if (delta.y > 0 && delta.y > Mathf.Abs(delta.x))
                            isInDirection = true;
                        break;

                    case Direction.Down:
                        if (delta.y < 0 && Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                            isInDirection = true;
                        break;

                    case Direction.Left:
                        if (delta.x < 0 && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                            isInDirection = true;
                        break;

                    case Direction.Right:
                        if (delta.x > 0 && delta.x > Mathf.Abs(delta.y))
                            isInDirection = true;
                        break;
                }

                if (isInDirection) {
                    float dist_sq = delta.sqrMagnitude;

                    if (dist_sq < lowest_dist_sq) {
                        lowest_dist_sq = dist_sq;
                        nearest = objects[i];
                    }
                }
            }
            return nearest;
        }
    }
}