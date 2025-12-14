// Campaigns/Technomancer/Init.cs
/* Initializes all Technomancer objects */

namespace Slugpack;

public static partial class Technomancer
{
    public static void Init()
    {
        Log.Info("Initializing Technomancer...");
        Appearance.Apply();
    }
}