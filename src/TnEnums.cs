namespace Slugpack;

public static class TnEnums
{
    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(CreatureType).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SandboxUnlock).TypeHandle);
    }

    public static void Unregister()
    {
        Utilities.UnregisterEnums(typeof(CreatureType));
        Utilities.UnregisterEnums(typeof(SandboxUnlock));
    }

    public static class CreatureType
    {
        public static CreatureTemplate.Type PastGreen = new(nameof(PastGreen), true);
        public static CreatureTemplate.Type HiveQueen = new(nameof(HiveQueen), true);

        public static CreatureTemplate.Type CaveLeech = new(nameof(CaveLeech), true);
    }

    public static class SandboxUnlock
    {
        public static MultiplayerUnlocks.SandboxUnlockID PastGreen = new(nameof(PastGreen), true);
        public static MultiplayerUnlocks.SandboxUnlockID HiveQueen = new(nameof(HiveQueen), true);

        public static MultiplayerUnlocks.SandboxUnlockID CaveLeech = new(nameof(CaveLeech), true);
    }
}