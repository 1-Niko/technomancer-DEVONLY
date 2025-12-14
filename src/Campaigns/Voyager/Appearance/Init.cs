// Campaigns/Voyager/Appearance/Init.cs
/* Controls Voyager's appearance */

/*
    There are three separate areas to Both slug's appearance:
    Head, Body, and Tail.

    Head and body are easy, it's just replacing the sprites.

    Tail is a pain in the ass, since it's purely a mesh it's
    much more difficult to anchor objects to. It is possible,
    but it is not easy.
*/

namespace Slugpack;

public static partial class Voyager
{
    public partial class Appearance
    {

        public static void Apply()
        {
            Log.Info("Initializing Techy's Appearance...");
            Head.Apply();
            Body.Apply();
            Tail.Apply();
        }
    }
}