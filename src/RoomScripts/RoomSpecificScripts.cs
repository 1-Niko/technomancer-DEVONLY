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
                    player.controller ??= new Player.NullController();
                    player.SuperHardSetPosition(new Vector2(566f, 165f));
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