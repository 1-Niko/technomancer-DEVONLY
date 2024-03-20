using System.Diagnostics.CodeAnalysis;

namespace Slugpack
{
    public static class CreatureTemplateType
    {
        [AllowNull] public static CreatureTemplate.Type PastGreen = new(nameof(PastGreen), true);

        public static void UnregisterValues()
        {
            if (PastGreen != null)
            {
                PastGreen.Unregister();
                PastGreen = null;
            }
        }
    }

    public static class SandboxUnlockID
    {
        [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID PastGreen = new(nameof(PastGreen), true);

        public static void UnregisterValues()
        {
            if (PastGreen != null)
            {
                PastGreen.Unregister();
                PastGreen = null;
            }
        }
    }
}