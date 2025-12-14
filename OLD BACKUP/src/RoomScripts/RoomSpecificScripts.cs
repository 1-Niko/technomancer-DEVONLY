namespace Slugpack;

public static class RoomScripts
{
    public static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        GATE_TL_OE_IntroScript.alreadyRun = false;
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        if (room.game.session is StoryGameSession story
            && Utilities.IsTechnomancerOrVoyager(story.game.StoryCharacter)
            && story.saveState.denPosition == "GATE_TL_OE"
            && room.abstractRoom.name == "GATE_TL_OE")
        {
            room.AddObject(new GATE_TL_OE_IntroScript(room));
        }
    }
}

public class GATE_TL_OE_IntroScript : UpdatableAndDeletable
{
    internal static bool alreadyRun; //sighs. but hey it fixes the problem
    private Player RealizedPlayer => room.game.Players.Count > 0 ? room.game.Players[0].realizedCreature as Player : null;

    public GATE_TL_OE_IntroScript(Room room)
    {
        this.room = room;
        //DebugLog($"The Technomancer: Created new {nameof(GATE_TL_OE_IntroScript)} in room {room.abstractRoom.name}");
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (null == RealizedPlayer)
        {
            return;
        }

        if (alreadyRun)
        {
            GiveAllPlayersControllersBack();
            Destroy();
            return;
        }
        else
        {
            foreach (var abstrCrit in room.game.session.Players)
            {
                if (abstrCrit != null)
                {
                    var player = abstrCrit.realizedCreature as Player;
                    player.AddFood(5);
                    player.standing = true;
                    player.controller ??= new Player.NullController();
                    player.SuperHardSetPosition(new Vector2(406f, 165f));

                    (player.graphicsModule as PlayerGraphics).owner.bodyChunks[0].pos = new Vector2(390.1f, 165.6f);
                    (player.graphicsModule as PlayerGraphics).owner.bodyChunks[1].pos = new Vector2(390.1f, 148f);
                    (player.graphicsModule as PlayerGraphics).head.pos = new Vector2(390.1f, 170.1f);
                    (player.graphicsModule as PlayerGraphics).tail[0].pos = new Vector2(384.3f, 146.1f);
                    (player.graphicsModule as PlayerGraphics).tail[1].pos = new Vector2(377.2f, 143.5f);
                    (player.graphicsModule as PlayerGraphics).tail[2].pos = new Vector2(370.2f, 142.2f);
                    (player.graphicsModule as PlayerGraphics).tail[3].pos = new Vector2(363.4f, 141f);
                }
            }

            if (room.regionGate != null)
            {
                room.regionGate.letThroughDir = true;

                room.regionGate.mode = RegionGate.Mode.ClosingAirLock;

                room.regionGate.goalDoorPositions[0] = 1f;
                room.regionGate.goalDoorPositions[1] = 1f;
                room.regionGate.doors[1].closedFac = 1f;
            }

            Constants.DamagedShortcuts.TryGetValue(room.game, out var ShortcutTable);
            int newRoom = room.abstractRoom.connections[0];
            if (newRoom > -1)
            {
                AbstractRoom abstractRoom = room.world.GetAbstractRoom(newRoom);
                while (abstractRoom.realizedRoom == null)
                {
                    abstractRoom.RealizeRoom(room.world, room.game);
                }
                var shortcutList = abstractRoom?.realizedRoom?.shortcuts?
                    .Where(element => element.destNode != -1 && element.destNode < abstractRoom.connections?.Length && abstractRoom.connections[element.destNode] != -1)
                    .ToList() ?? [];

                if (shortcutList.Count > 0)
                {
                    var exitIndex = abstractRoom.ExitIndex(room.abstractRoom.index);
                    if (exitIndex >= 0 && exitIndex < shortcutList.Count)
                    {
                        var shortcut = shortcutList[exitIndex];

                        List<ShortcutData> inQuestion = room.shortcuts.Where(element => (element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1) && element.shortCutType == ShortcutData.Type.RoomExit).ToList();

                        ShortcutData[] shortcutDataArray = [inQuestion[0], shortcut];
                        Room[] roomArray = [room, abstractRoom.realizedRoom];

                        int lockTime = 10 * 40;

                        LockHologram[] hologramArray = [new(room.MiddleOfTile(inQuestion[0].StartTile), (room.game.session.Players[0].realizedCreature as Player).ShortCutColor(), lockTime), new LockHologram(abstractRoom.realizedRoom.MiddleOfTile(shortcut.StartTile), (room.game.session.Players[0].realizedCreature as Player).ShortCutColor(), lockTime)];

                        ShortcutTable.locks.Add(new Lock(shortcutDataArray, roomArray, lockTime, hologramArray));

                        room.AddObject(hologramArray[0]);
                        abstractRoom.realizedRoom.AddObject(hologramArray[1]);
                    }
                }
            }

            alreadyRun = true;
            GiveAllPlayersControllersBack();
            return;
        }
    }

    public void GiveAllPlayersControllersBack()
    {
        foreach (var abstrCrit in room.game.session.Players)
        {
            if (abstrCrit?.realizedCreature is Player player)
                player.controller = null;
        }
    }
}