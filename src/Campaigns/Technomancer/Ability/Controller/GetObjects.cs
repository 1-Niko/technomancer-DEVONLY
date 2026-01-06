// Campaigns/Technomancer/Ability/Controller/GetObjects.cs
/* Will control the visual aspect of the technomancy hud */

using System.Collections.Generic;
using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public static List<ManipulatableObject> GetObjectsOnScreen(Room room)
        {
            List<ManipulatableObject> objects = new List<ManipulatableObject>();

            for (int i = 0; i < room.shortcuts.Length; i++) {
                ShortcutData candidate = room.shortcuts[i];
                if (ObjectRegistry.Identify(candidate) == OwnerType.Error)
                    continue;

                Vector2 position = room.MiddleOfTile(candidate.StartTile);

                if (room.ViewedByAnyCamera(position, 0f))
                    objects.Add(new ManipulatableObject(candidate, room));
            }

            for (int j = 0; j < 3; j++) {
                for (int i = 0; i < room.physicalObjects[j].Count; i++) {
                    PhysicalObject candidate = room.physicalObjects[j][i];
                    if (ObjectRegistry.Identify(candidate) == OwnerType.Error)
                        continue;

                    if (candidate is Creature && !(candidate as Creature).dead && room.ViewedByAnyCamera((candidate as Creature).mainBodyChunk.pos, 0f)) {
                        objects.Add(new ManipulatableObject(candidate, room));
                    }
                    else if (room.ViewedByAnyCamera(candidate.firstChunk.pos, 0f)) {
                        objects.Add(new ManipulatableObject(candidate, room));
                    }
                }
            }

            for (int i = 0; i < room.roomSettings.placedObjects.Count; i++) {
                PlacedObject candidate = room.roomSettings.placedObjects[i];
                if (ObjectRegistry.Identify(candidate) == OwnerType.Error)
                    continue;

                if (room.ViewedByAnyCamera(candidate.pos, 0f))
                    objects.Add(new ManipulatableObject(candidate, room));
            }

            return objects;
        }
    }
}