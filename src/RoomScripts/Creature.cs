namespace Slugpack;

internal static class CreatureHooks
{
    internal static void Apply()
    {
        // On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        On.Vulture.Update += Vulture_Update;
        On.AbstractCreatureAI.RandomMoveToOtherRoom += AbstractCreatureAI_RandomMoveToOtherRoom;
    }

    private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, RWCustom.IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player && (self as Player).IsTechy(out var scanline) && (scanline.generatedIcons || scanline.roomController != null))
        {
            // Prevention of an NRE
            self.enteringShortCut = null;
            self.inShortcut = false;
            return;
        }
        else
        {
            if (self != null && self.room != null)
            {
                var shortcutLocation = Utilities.DetermineObjectFromPosition(self.room.MiddleOfTile(entrancePos), self.room).nearestShortcut;
                if (Constants.DamagedShortcuts.TryGetValue(self.room.game, out var ShortcutTable))
                {
                    for (int i = 0; i < ShortcutTable.locks.Count; i++)
                    {
                        for (int r = 0; r < 2; r++)
                        {
                            if (ShortcutTable.locks[i].Shortcuts[r].Equals(shortcutLocation))
                            {
                                self.enteringShortCut = null;
                                self.inShortcut = false;
                                return;
                            }
                        }
                    }
                }
            }
        }
        orig(self, entrancePos, carriedByOther);
    }

    private static void Vulture_Update(On.Vulture.orig_Update orig, Vulture self, bool eu)
    {
        orig(self, eu);

        if (Constants.VultureStuff.TryGetValue(self, out var vulturestuff))
        {
            if (vulturestuff.timer > 0)
            {
                if (vulturestuff.thruster == -1)
                {
                    vulturestuff.thruster = Random.Range(0, 5);
                }

                self.stun = 45;
                self.landingBrake = vulturestuff.timer;

                if (self.IsKing && Random.Range(0, 20) == 0)
                {
                    //self.kingTusks.TryToShoot();

                    int tusk = Random.Range(0, 2);

                    if (self.kingTusks.tusks.ElementAtOrDefault(tusk) != null && self.kingTusks.tusks[tusk] != null && self.kingTusks.tusks[tusk].mode == KingTusks.Tusk.Mode.Attached)
                    {
                        Vector2 vector = RWCustom.Custom.DirVec(self.kingTusks.tusks[tusk].vulture.neck.tChunks[self.kingTusks.tusks[tusk].vulture.neck.tChunks.Length - 1].pos, self.kingTusks.tusks[tusk].vulture.bodyChunks[4].pos);
                        Vector2 a = RWCustom.Custom.PerpendicularVector(vector);
                        Vector2 vector2 = self.kingTusks.tusks[tusk].vulture.bodyChunks[4].pos + (vector * -5f);
                        vector2 += a * self.kingTusks.tusks[tusk].zRot.x * 15f;
                        vector2 += a * self.kingTusks.tusks[tusk].zRot.y * ((self.kingTusks.tusks[tusk].side == 0) ? -1f : 1f) * 7f;

                        self.kingTusks.tusks[tusk].Shoot(vector2);
                    }
                }

                /*foreach (var chunk in self.bodyChunks)
                 {
                     chunk.vel += RWCustom.Custom.RNV() * 1.5f;
                 }*/

                if (self.thrusters.ElementAtOrDefault(vulturestuff.thruster) != null)
                {
                    self.thrusters[vulturestuff.thruster].Activate(20);
                    for (int i = 0; i < 4; i++)
                    {
                        if (i != vulturestuff.thruster)
                        {
                            self.thrusters[i].thrust = 0;
                            self.thrusters[i].smoke = null;
                        }
                    }

                    self.mainBodyChunk.vel *= 3f;
                }

                vulturestuff.timer--;
            }
            else
            {
                vulturestuff.thruster = -1;
            }
        }
    }

    private static void AbstractCreatureAI_RandomMoveToOtherRoom(On.AbstractCreatureAI.orig_RandomMoveToOtherRoom orig, AbstractCreatureAI self, int maxRoamDistance)
    {
        if (self.world.GetAbstractRoom(self.parent.pos).connections.Length < 1)
        {
            return;
        }
        WorldCoordinate worldCoordinate = QuickConnectivity.DefineNodeOfLocalCoordinate(self.parent.pos, self.world, self.parent.creatureTemplate);
        List<WorldCoordinate> list = [];
        int num = 0;
        for (int i = 0; i < 100; i++)
        {
            AbstractRoom abstractRoom = self.world.GetAbstractRoom(worldCoordinate.room);
            List<WorldCoordinate> list2 = [];
            for (int j = 0; j < abstractRoom.connections.Length; j++)
            {
                if (abstractRoom.connections[j] > -1)
                {
                    bool flag = self.CanRoamThroughRoom(abstractRoom.connections[j]);
                    int num2 = 0;
                    while (num2 < list.Count && flag)
                    {
                        if (list[num2].room == abstractRoom.connections[j])
                        {
                            flag = false;
                        }
                        num2++;
                    }
                    if (flag)
                    {
                        WorldCoordinate worldCoordinate2 = new(abstractRoom.connections[j], -1, -1, self.world.GetAbstractRoom(abstractRoom.connections[j]).ExitIndex(abstractRoom.index));

                        if (self.world.GetAbstractRoom(worldCoordinate2.room).nodes.Length < worldCoordinate2.abstractNode && abstractRoom.ConnectionAndBackPossible(worldCoordinate.abstractNode, j, self.parent.creatureTemplate) && self.parent.creatureTemplate.AbstractSubmersionLegal(self.world.GetNode(worldCoordinate2).submerged))
                        {
                            list2.Add(worldCoordinate2);
                        }
                    }
                }
            }
            if (list2.Count <= 0)
            {
                break;
            }
            WorldCoordinate worldCoordinate3 = list2[Random.Range(0, list2.Count)];
            float num3 = 0f;
            for (int k = 0; k < list2.Count; k++)
            {
                num3 += self.world.GetAbstractRoom(list2[k]).SizeDependentAttractionValueForCreature(self.parent.creatureTemplate.type);
            }
            float num4 = Random.value * num3;
            for (int l = 0; l < list2.Count; l++)
            {
                float num5 = self.world.GetAbstractRoom(list2[l]).SizeDependentAttractionValueForCreature(self.parent.creatureTemplate.type);
                if (num4 < num5)
                {
                    worldCoordinate3 = list2[l];
                    break;
                }
                num4 -= num5;
            }
            list.Insert(0, new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.ExitIndex(worldCoordinate3.room)));
            list.Insert(0, worldCoordinate3);
            num += (worldCoordinate.abstractNode == abstractRoom.ExitIndex(worldCoordinate3.room)) ? 0 : abstractRoom.nodes[worldCoordinate.abstractNode].ConnectionLength(abstractRoom.ExitIndex(worldCoordinate3.room), self.parent.creatureTemplate);
            if (num > maxRoamDistance)
            {
                break;
            }
            worldCoordinate = worldCoordinate3;
        }
        if (list.Count > 0 && self.world.GetAbstractRoom(list[0]).SizeDependentAttractionValueForCreature(self.parent.creatureTemplate.type) < self.world.GetAbstractRoom(self.parent.pos).SizeDependentAttractionValueForCreature(self.parent.creatureTemplate.type))
        {
            while (list.Count > 1 && (list[0].room == list[1].room || self.world.GetAbstractRoom(list[0]).SizeDependentAttractionValueForCreature(self.parent.creatureTemplate.type) < self.world.GetAbstractRoom(list[1]).SizeDependentAttractionValueForCreature(self.parent.creatureTemplate.type)))
            {
                list.RemoveAt(0);
            }
        }
        if (list.Count > 0)
        {
            self.path = list;
            self.InternalSetDestination(list[0]);
        }
    }
}