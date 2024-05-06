namespace Slugpack;

internal static class WeakTables
{
    public class ShortcutLockImmunity
    {
        public bool immune;
    }

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

    //public class ScanLine
    //{
    //    public int holdTime = 0;
    //    public Vector2 position = Vector2.zero;
    //    public SlugArrow arrow = null;

    //    public bool heldup = false;
    //    public bool helddown = false;
    //    public bool heldleft = false;
    //    public bool heldright = false;

    //    public int x;
    //    public int y;

    //    public bool jmp;
    //    public bool thrw;

    //    public bool pckp;

    //    public bool inputHoldThrw = false;
    //    public bool inputHoldJmp = false;

    //    public float debugTimer = 0f;

    //    public Vector2 tail_0_c = Vector2.zero;
    //    public Vector2 tail_0_p = Vector2.zero;

    //    public Vector2 tail_1_c = Vector2.zero;
    //    public Vector2 tail_1_p = Vector2.zero;

    //    public Vector2 tail_2_c = Vector2.zero;
    //    public Vector2 tail_2_p = Vector2.zero;

    //    public int murdered_neurons = 0;

    //    public float wetness = 0f;

    //    public float danger = 0f;

    //    public Vector2[] slugVector;
    //    public int correctVelocity = 0;
    //    public Vector2 pointTo;
    //    public float pointPower = 0;

    //    public int train_sound_timer = 0;

    //    public bool generatedIcons = false;

    //    public List<HighlightSprite> TechIcons = [];
    //    public List<ConnectingLine> TechConnections = [];

    //    public Vector2 screenPosition = Vector2.zero;

    //    public float padding = 30f;

    //    public bool roomControllerGenerated = false;
    //    public RoomController roomController;

    //    public int stunImmune = 0;

    //    // Debug Variables

    //    public float xOffset = 0f;
    //    public float yOffset = 0f;
    //    public float rOffset = 0f;

    //    public float sOffset = 0f;

    //    public bool debugbool = true;
    //}

    public class VultureStuff
    {
        public int timer = 0;
        public int thruster = -1;
    }

    public class ShortcutList
    {
        public List<Lock> locks = [];
        public int camPosition = 0;
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