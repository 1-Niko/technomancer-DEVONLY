// Campaigns/Technomancer/Ability/Controller/NodeSpawner.cs
/* Handles spawning in the nodes, pruning off invalid ones, and adding new ones as applicable */

using System.Collections.Generic;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public class NodeHandler : UpdatableAndDeletable
        {
            public Player player;
            public List<ManipulatableObject> loadedObjects;

            public NodeHandler(Player player)
            {
                this.player = player;
                loadedObjects = new List<ManipulatableObject>();
            }

            public ManipulatableObject NearestToPlayer()
            {
                if (loadedObjects.Count == 0)
                    return null;

                return GetNearestObject(loadedObjects, player.mainBodyChunk.pos);
            }

            public bool ObjectInsideList(List<ManipulatableObject> list, object data)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i].Equals(data))
                        return true;

                return false;
            }

            public HackNode[] RetrieveSprites(List<ManipulatableObject> list)
            {
                HackNode[] data = new HackNode[list.Count];

                for (int i = 0; i < list.Count; i++)
                    data[i] = list[i].sprite;

                return data;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                List<ManipulatableObject> objects = GetObjectsOnScreen(this.room);

                for (int i = 0; i < objects.Count; i++) {
                    if (!ObjectInsideList(loadedObjects, objects[i])) {
                        HackNode node = new HackNode(objects[i]);
                        objects[i].sprite = node;
                        node.pos = objects[i].pos;
                        node.lastPos = node.pos;
                        this.room.AddObject(node);
                        loadedObjects.Add(objects[i]);
                    }
                }
                List<ManipulatableObject> pruneList = new List<ManipulatableObject>();
                for (int i = 0; i < loadedObjects.Count; i++) {
                    if (!ObjectInsideList(objects, loadedObjects[i])) {
                        loadedObjects[i].Destroy();
                        pruneList.Add(loadedObjects[i]);
                    }
                }
                for (int i = 0; i < pruneList.Count; i++)
                    loadedObjects.Remove(pruneList[i]);

                this.room.game.GetNodeUI().nodes = this.RetrieveSprites(this.loadedObjects);
            }

            public override void Destroy()
            {
                base.Destroy();

                for (int i = 0; i < loadedObjects.Count; i++)
                    loadedObjects[i].Destroy();
            }
        }
    }
}