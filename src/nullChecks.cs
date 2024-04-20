using MoreSlugcats;
using static Sony.NP.Matching;

namespace Slugpack;

public static class Null
{
    public static bool Check(object check, int method)
    {
        if (check == null || method == 0)
            return false;

        if (method == 1)
        {
            Player checkObj = check as Player;

            return checkObj != null && checkObj.room != null && checkObj.room.game != null;
        }

        return false;
    }
}