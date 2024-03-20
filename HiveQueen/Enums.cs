using System.Diagnostics.CodeAnalysis;

namespace HiveQueen;

public static class CreatureTemplateType
{
    public static CreatureTemplate.Type HiveQueen = new CreatureTemplate.Type("HiveQueen", true);

    public static void UnregisterValues()
    {
        if (HiveQueen != null)
        {
            HiveQueen.Unregister();
            HiveQueen = null;
        }
    }
}

public static class SandboxUnlockID
{
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID HiveQueen = new(nameof(HiveQueen), true);

    public static void UnregisterValues()
    {
        if (HiveQueen != null)
        {
            HiveQueen.Unregister();
            HiveQueen = null;
        }
    }
}