namespace Slugpack;

internal static class WeakTables
{
    public class TrackHologramMessenger
    {
        public bool playerInteracted = false;
        public bool onCooldown = false;
    }

    public class WaterKillBoxCounter
    {
        public bool contact = false;
        public int count = -1;
    }

    public class Shaders
    {
        public FShader Redify = null;

        // public FShader Layer_0 = null;
        public FShader ShadowMask = null;

        public FShader HologramA = null;
        public FShader HologramB = null;
        public FShader Distances = null;
        public FShader ProjectionLinesA = null;
        public FShader ProjectionLinesB = null;
        public FShader ProjectionLinesC = null;
        public FShader ProjectionLinesD = null;
        public FShader ColourChangerShader = null;
        public FShader SelectionShader = null;
        public FShader ConnectingLine = null;
        public FShader ModifiedLightBeamShader = null;
        public FShader DynamicTrain = null;
        public FShader SpinningFan = null;

        public AssetBundle SlugShaders = null;
        public Texture _shadowMask = null;
        public Texture _effectMask = null;
        public Texture _RGB2HSL = null;
        public Texture _HSL2RGB = null;

        public Vector2 position = Vector2.zero;
    }

    public class VultureStuff
    {
        public int timer = 0;
        public int thruster = -1;
    }

    public class ShortcutList
    {
        public List<Lock> locks = [];
        public int camPosition = 0;
        public string room;
    }

    public class GraphicsData
    {
        public int firstIndex = 0;

        public FSprite[] sprites = null;

        public TailSegment[] tail = null;
    }

    public class OracleData
    {
        public bool unlockShortcuts = false;

        public bool endMain = false;

        public int overstayTimer = -400;

        public bool setTimer = false;

        public int timerOffset = 0;

        public bool seenPlayer = false;

        public int timer = 0;
    }
}