namespace Slugpack;

public static class Null
{
    public static bool Check(object check, int method)
    {
        if (check == null || method == 0)
            return false;

        if (method == 1)
        {
            return check is Player checkObj && checkObj.room != null && checkObj.room.game != null;
        }
        if (method == 2)
        {
            return check is Player checkObj && checkObj.room != null && checkObj.room.updateList != null && checkObj.room.updateList.Count > 0;
        }
        if (method == 3)
        {
            return check is Creature creature && creature != null && creature.room != null && creature.room.world != null && creature.room.world.game != null;
        }
        if (method == 4)
        {
            return check is RoomCamera camera && camera != null && camera.room != null && camera.room.world != null && camera.room.world.game != null;
        }

        return false;
    }
}