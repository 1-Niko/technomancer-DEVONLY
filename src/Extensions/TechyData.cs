namespace Slugpack;

public class TechyData
{
    public readonly bool IsTechy;

    public readonly Player player;

    //ScanLine Data

    public bool queueThrw;
    public bool queueJmp;
    public bool isQueued;

    public bool isHeld;
    public int holdTime;
    public Vector2 position;
    public SlugArrow arrow;

    public bool heldup;
    public bool helddown;
    public bool heldleft;
    public bool heldright;

    public int x;
    public int y;

    public bool jmp;
    public bool thrw;

    public bool pckp;

    public bool inputHoldThrw;
    public bool inputHoldJmp;

    public float debugTimer;

    public Vector2 tail_0_c;
    public Vector2 tail_0_p;

    public Vector2 tail_1_c;
    public Vector2 tail_1_p;

    public Vector2 tail_2_c;
    public Vector2 tail_2_p;

    public int murdered_neurons;

    public float wetness;

    public float danger;

    public Vector2[] slugVector;
    public int correctVelocity;
    public Vector2 pointTo;
    public float pointPower;

    public int train_sound_timer;

    public bool generatedIcons;

    public List<HighlightSprite> TechIcons;
    public List<ConnectingLine> TechConnections;

    public Vector2 screenPosition;

    public float padding;

    public bool roomControllerGenerated;
    public RoomController roomController;

    public int stunImmune;

    // Debug Variables

    public float xOffset;
    public float yOffset;
    public float rOffset;

    public float sOffset;

    public bool debugbool = true;

    public TechyData(Player player)
    {
        IsTechy = player.slugcatStats.name == TnEnums.Technomancer;
        this.player = player;

        if (!IsTechy) return;
        //Add all the values or data for Techy below

        holdTime = 0;
        position = Vector2.zero;
        arrow = null;
        heldup = false;
        helddown = false;
        heldleft = false;
        heldright = false;
        inputHoldThrw = false;
        inputHoldJmp = false;
        debugTimer = 0f;
        tail_0_c = Vector2.zero;
        tail_0_p = Vector2.zero;
        tail_1_c = Vector2.zero;
        tail_1_p = Vector2.zero;
        tail_2_c = Vector2.zero;
        tail_2_p = Vector2.zero;
        murdered_neurons = 0;
        wetness = 0f;
        danger = 0f;
        correctVelocity = 0;
        pointPower = 0;
        train_sound_timer = 0;
        generatedIcons = false;
        TechIcons = [];
        TechConnections = [];
        screenPosition = Vector2.zero;
        padding = 30f;
        roomControllerGenerated = false;
        stunImmune = 0;
        xOffset = 0f;
        yOffset = 0f;
        rOffset = 0f;
        sOffset = 0f;
        debugbool = true;
    }
}