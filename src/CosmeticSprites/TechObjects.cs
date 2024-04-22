namespace Slugpack;

public class RoomController : CosmeticSprite
{
    public List<HighlightSprite> Nodes { get; }
    public bool ScheduledRegenerateConnections { get; set; }
    public Color Colour { get; set; }
    public SlugArrow Arrow { get; }
    public int PreviousNodeCount { get; set; }

    public RoomController(Room room, Color colour, SlugArrow arrow)
    {
        this.room = room;
        Colour = colour;
        Arrow = arrow;
        Nodes = [];
        PreviousNodeCount = 0;
        GenerateEmptyNodes();

    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        GenerateEmptyNodes();
        // if (this.nodes.Count == 0 || this.nodes.Count != this.previousNodeCount)
        ForceUninitiatedNodesToGenerateConnections();
        PreviousNodeCount = Nodes.Count;
        if (ScheduledRegenerateConnections)
        {
            RegenerateConnectionsForAllNodes();
            ScheduledRegenerateConnections = false;
        }
        PurgeMarkedNodes();
    }

    public override void Destroy()
    {
        for (int i = 0; i < Nodes.Count; i++)
            Nodes[i].Destroy();
        room.RemoveObject(this);
        base.Destroy();
    }

    public void PurgeMarkedNodes()
    {
        int purged_nodes = 0;
        List<HighlightSprite> markedNodes = [];
        for (int i = 0; i < Nodes.Count; i++)
            if (Nodes[i].MarkedForDeletion)
            {
                Nodes[i].Destroy();
                markedNodes.Add(Nodes[i]);
                purged_nodes++;
            }
        if (purged_nodes > 0)
        {
            for (int i = 0; i < markedNodes.Count; i++)
                _ = Nodes.Remove(markedNodes[i]);
            RegenerateConnectionsForAllNodes();
        }
    }

    // THINGS TO SOLVE:
    // 1. Nodes are continuously generated when a moving node (creature) is in the room!
    // 2. Connections continuously flicker
    public void GenerateEmptyNodes()
    {
        List<Node> nodeData = Utilities.GetAllNodeInformation(room);

        int addedNodes = 0;

        for (int i = 0; i < nodeData.Count; i++)
        {
            if (!Nodes.Any(obj => obj.pos.Equals(nodeData[i].Position)) && (nodeData[i].Anchor == null || !Nodes.Any(obj => obj.Anchor == nodeData[i].Anchor)) && (room.TileHeight * 20f) > nodeData[i].Position.y && (room.TileWidth * 20f) > nodeData[i].Position.x && 0 < nodeData[i].Position.x && 0 < nodeData[i].Position.y)
            {
                // going to have to make something to allow for nodes to track their parent object
                // at least the functionality for the connections is already in place
                HighlightSprite node = new(nodeData[i].Position, nodeData[i].Level, nodeData[i].Protection, room, this, nodeData[i].Anchor);
                Nodes.Add(node);
                room.AddObject(node);
                addedNodes++;
            }
        }
        if (addedNodes > 0)
            RegenerateConnectionsForAllNodes();
    }

    public void RegenerateConnectionsForAllNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].RegenerateConnections();
        }
    }

    public void ForceUninitiatedNodesToGenerateConnections()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i].Connections.Count == 0)
            {
                Nodes[i].RegenerateConnections();
            }
        }
    }
}

public class HighlightSprite : CosmeticSprite
{
    public float Padding { get; set; }
    public List<ConnectingLine> Connections { get; set; }
    public float Age { get; }
    public int ConnectionCount { get; set; }
    public PhysicalObject Anchor { get; }
    public bool MarkedForDeletion { get; set; }
    public RoomController Owner { get; }
    public Dictionary<HighlightSprite, ConnectingLine> ConnectionTable { get; }
    public bool IsSmall { get; set; }
    public bool Selected { get; set; }

    public HighlightSprite(Vector2 pos, int nodeLevel, int protectionLevels, Room room, RoomController owner, PhysicalObject anchor)
    {
        Age = Utilities.Timestamp();
        MarkedForDeletion = false;
        IsSmall = false;
        ConnectionCount = 0;
        this.pos = pos;
        lastPos = pos;
        this.nodeLevel = nodeLevel;
        this.protectionLevels = protectionLevels;
        this.room = room;
        Anchor = anchor;
        Owner = owner;
        Connections = [];
        ConnectionTable = [];
        RegenerateConnections();
    }

    public override void Destroy()
    {
        // this.owner.scheduledRegenerateConnections = true;
        // this.owner.nodes.Remove(this);
        for (int i = 0; i < Connections.Count; i++)
            Connections[i].Destroy();
        for (int i = 0; i < spriteLeaser.sprites.Length; i++)
            spriteLeaser.sprites[i].RemoveFromContainer();
        room.RemoveObject(this);
        base.Destroy();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastStepTimer = stepTimer;
        stepTimer++;

        if (Anchor is Creature)
        {
            pos = (Anchor as Creature).mainBodyChunk.pos;
            if ((Anchor as Creature) == null || (Anchor as Creature).slatedForDeletetion || (Anchor as Creature).dead)
            {
                MarkedForDeletion = true;
                RegenerateConnections();
            }
        }

        else if (Anchor is SSOracleSwarmer)
        {
            pos = (Anchor as PhysicalObject).firstChunk.pos;
            if ((Anchor as SSOracleSwarmer) == null || (Anchor as SSOracleSwarmer).slatedForDeletetion)
            {
                MarkedForDeletion = true;
                RegenerateConnections();
            }
        }
        else if (Anchor is PlayerCarryableItem)
        {
            pos = (Anchor as PlayerCarryableItem).firstChunk.pos;
            if ((Anchor as PlayerCarryableItem) == null || (Anchor as PlayerCarryableItem).slatedForDeletetion)
            {
                MarkedForDeletion = true;
                RegenerateConnections();
            }
        }

        if (Connections.Count == 0)
            RegenerateConnections();
        CalculateConnections();

        EnableValid();
        CalculateSmallness();

        Selected = RWCustom.Custom.Dist(Owner.Arrow.pos, pos) < 1;

        if ((room.TileHeight * 20f) < pos.y || (room.TileWidth * 20f) < pos.x || 0 > pos.x || 0 > pos.y)
            MarkedForDeletion = true;
    }

    public void RegenerateConnections()
    {
        for (int i = 0; i < Connections.Count; i++)
            room.RemoveObject(Connections[i]);

        Connections.Clear();
        ConnectionTable.Clear();

        for (int i = 0; i < Owner.Nodes.Count; i++)
        {
            ConnectingLine connection = new((pos + Owner.Nodes[i].pos) / 2, Utilities.CalculateAngleBetweenVectorsForLineSegment(pos, Owner.Nodes[i].pos),
                                                     RWCustom.Custom.Dist(pos, Owner.Nodes[i].pos) - (Padding * 2), Padding, this, Owner.Nodes[i]);
            Connections.Add(connection);
            room.AddObject(Connections[i]);
            if (!ConnectionTable.ContainsKey(Owner.Nodes[i]))
                ConnectionTable.Add(Owner.Nodes[i], connection);
        }

        EnableValid();
    }

    public void CalculateConnections()
    {
        ConnectionCount = 0;
        for (int i = 0; i < Connections.Count; i++)
            if (Connections[i].ShouldBeEnabled)
                ConnectionCount++;
    }

    public bool ReturnValid(HighlightSprite node)
    {
        return node != null && node.ConnectionTable != null && (!node.ConnectionTable.ContainsKey(this) || !node.ConnectionTable[this].Enabled || (node.ConnectionTable[this].Enabled && Age > node.Age));
    }

    public void CalculateSmallness()
    {
        IsSmall = false;
        for (int i = 0; i < Owner.Nodes.Count; i++)
            if (Owner.Nodes[i] != this && RWCustom.Custom.Dist(pos, Owner.Nodes[i].pos) < 45)
                IsSmall = true;
    }

    // enableValid isn't working as expected, will work on after i get the lerping problem with connections sorted
    // sorted and solved :)
    public void EnableValid()
    {
        for (int i = 0; i < Owner.Nodes.Count; i++)
            ConnectionTable[Owner.Nodes[i]].Enabled = false;

        //if (Utilities.CheckIfOnScreen(this.pos, this.room))
        //{
        // At least one is bound to be valid
        var (up, left, right, down) = Utilities.GetNearestNodesInAllDirections(this, Owner.Nodes, room);

        // Must also check to see if partner has theirs enabled
        if (ReturnValid(up)) { ConnectionTable[up].Enabled = true; }
        if (ReturnValid(left)) { ConnectionTable[left].Enabled = true; }
        if (ReturnValid(right)) { ConnectionTable[right].Enabled = true; }
        if (ReturnValid(down)) { ConnectionTable[down].Enabled = true; }
        if (up != null && up.ConnectionTable != null) { ConnectionTable[up].ShouldBeEnabled = true; }
        if (left != null && left.ConnectionTable != null) { ConnectionTable[left].ShouldBeEnabled = true; }
        if (right != null && right.ConnectionTable != null) { ConnectionTable[right].ShouldBeEnabled = true; }
        if (down != null && down.ConnectionTable != null) { ConnectionTable[down].ShouldBeEnabled = true; }
        //}
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        string sVar = IsSmall ? "A" : "B";

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"karma_{sVar}{nodeLevel}");
        sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("protection_1");
        sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("protection_2");
        sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("protection_3");

        // This is required to not get any strange artifacts. No idea why, but it's 100% required

        sLeaser.sprites[0].scale = 1.0002f;
        sLeaser.sprites[1].scale = 1.0002f;
        sLeaser.sprites[2].scale = 1.0002f;
        sLeaser.sprites[3].scale = 1.0002f;

        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[1].isVisible = protectionLevels > 0;
        sLeaser.sprites[2].isVisible = protectionLevels > 1;
        sLeaser.sprites[3].isVisible = protectionLevels > 2;

        Vector2 currentPos = Vector2.Lerp(lastPos, pos, timeStacker);

        sLeaser.sprites[0].SetPosition(currentPos - rCam.pos);
        sLeaser.sprites[1].SetPosition(currentPos - rCam.pos);
        sLeaser.sprites[2].SetPosition(currentPos - rCam.pos);
        sLeaser.sprites[3].SetPosition(currentPos - rCam.pos);

        sLeaser.sprites[0].color = new Color(0, 0, Selected ? 1 : 0, 0);
        sLeaser.sprites[1].color = new Color(0, 0, Selected ? 1 : 0, 0);
        sLeaser.sprites[2].color = new Color(0, 0, Selected ? 1 : 0, 0);
        sLeaser.sprites[3].color = new Color(0, 0, Selected ? 1 : 0, 0);

        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
        {
            sLeaser.sprites[0].shader = Shaders.SelectionShader;
            sLeaser.sprites[1].shader = Shaders.SelectionShader;
            sLeaser.sprites[2].shader = Shaders.SelectionShader;
            sLeaser.sprites[3].shader = Shaders.SelectionShader;

            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(lastStepTimer, stepTimer, timeStacker));
            sLeaser.sprites[1]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(lastStepTimer, stepTimer, timeStacker));
            sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(lastStepTimer, stepTimer, timeStacker));
            sLeaser.sprites[3]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(lastStepTimer, stepTimer, timeStacker));

            // if (!Constants.ScanLineMemory.TryGetValue(this.technomancer, out var scanline)) Constants.ScanLineMemory.Add(this.technomancer, scanline = new WeakTables.ScanLine());

            sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PlayerPosition", new Vector4(Shaders.position.x, Shaders.position.y, 0, 0));
            sLeaser.sprites[1]._renderLayer?._material?.SetVector("_PlayerPosition", new Vector4(Shaders.position.x, Shaders.position.y, 0, 0));
            sLeaser.sprites[2]._renderLayer?._material?.SetVector("_PlayerPosition", new Vector4(Shaders.position.x, Shaders.position.y, 0, 0));
            sLeaser.sprites[3]._renderLayer?._material?.SetVector("_PlayerPosition", new Vector4(Shaders.position.x, Shaders.position.y, 0, 0));
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[4];

        string sVar = IsSmall ? "A" : "B";

        sLeaser.sprites[0] = new FSprite($"karma_{sVar}{nodeLevel}", true);
        sLeaser.sprites[1] = new FSprite("protection_1", true);
        sLeaser.sprites[2] = new FSprite("protection_2", true);
        sLeaser.sprites[3] = new FSprite("protection_3", true);

        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[1].isVisible = protectionLevels > 0;
        sLeaser.sprites[2].isVisible = protectionLevels > 1;
        sLeaser.sprites[3].isVisible = protectionLevels > 2;

        sLeaser.sprites[0].SetPosition(pos - rCam.pos);
        sLeaser.sprites[1].SetPosition(pos - rCam.pos);
        sLeaser.sprites[2].SetPosition(pos - rCam.pos);
        sLeaser.sprites[3].SetPosition(pos - rCam.pos);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        // Same here, gotta solve an NRE
        spriteLeaser ??= sLeaser;
        newContatiner ??= rCam.ReturnFContainer("HUD");
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContatiner.AddChild(fsprite);
        }
    }

    private readonly int nodeLevel;

    private readonly int protectionLevels;

    private float stepTimer;

    private float lastStepTimer;

    private RoomCamera.SpriteLeaser spriteLeaser;
}

public class ConnectingLine : CosmeticSprite
{
    public Color Colour { get; set; }
    public Vector2 Pos { get; set; }
    public float Rot { get; set; }
    public float Length { get; set; }
    public float Padding { get; set; }
    public bool Enabled { get; set; }
    public bool ShouldBeEnabled { get; set; }
    public HighlightSprite Mother { get; }
    public HighlightSprite Father { get; }

    public ConnectingLine(Vector2 pos, float rot, float len, float padding, HighlightSprite mother, HighlightSprite father)
    {
        Enabled = false;
        ShouldBeEnabled = false;
        Pos = pos;
        Rot = rot;
        Padding = 30;

        lastPos = pos;
        lastRot = rot;
        lastLen = len;
        lastPadding = padding;

        Mother = mother;
        Father = father;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastStepTimer = stepTimer;
        stepTimer++;

        lastPos = Pos; // shouldnt have to add this but it's being a bitch
        lastLen = Length;
        lastRot = Rot;
        lastPadding = Padding;

        Pos = (Mother.pos + Father.pos) / 2;
        Rot = Utilities.CalculateAngleBetweenVectorsForLineSegment(Mother.pos, Father.pos);
        Length = RWCustom.Custom.Dist(Mother.pos, Father.pos);// - (this.padding * 2);
    }

    public override void Destroy()
    {
        try
        {
            for (int i = 0; i < spriteLeaser.sprites.Length; i++)
                spriteLeaser.sprites[i].RemoveFromContainer();
            room.RemoveObject(this);
        }
        catch
        {
            // ignore the problem :)
        }
        base.Destroy();
    }

    public bool DetermineWhoGetsToExist(bool isEnabled)
    {
        if (!isEnabled)
            return false;
        //else if (!getParentConnectionState(this.father) && this.mother.age < this.father.age)
        else if ((!GetParentConnectionState(Father)) || (GetParentConnectionState(Father) && Mother.Age < Father.Age))
            return true;
        return false;
    }

    public bool GetParentConnectionState(HighlightSprite to)
    {
        for (int i = 0; i < to.Connections.Count; i++)
            if (to.Connections[i] == this)
                return to.Connections[i].Enabled;
        return false;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("connecting_line");

        Vector2 currentPos = Vector2.Lerp(lastPos, Pos, timeStacker);

        float currentRot = Mathf.Lerp(lastRot, Rot, timeStacker);
        float leniency = 1.5f;
        if ((currentRot > lastRot + leniency || currentRot < lastRot - leniency) && (currentRot > Rot + leniency || currentRot < Rot - leniency))
            currentRot = Rot;
        float currentLength = Mathf.Lerp(lastLen, Length, timeStacker);
        float currentPadding = Mathf.Lerp(lastPadding, Padding, timeStacker) * 2;

        // This is required to not get any strange artifacts. No idea why, but it's 100% required

        sLeaser.sprites[0].scaleX = 1.0002f / 1024f * (currentLength - currentPadding);
        sLeaser.sprites[0].scaleY = 1.0002f; // 1.0002f;

        sLeaser.sprites[0].SetPosition(currentPos - rCam.pos);

        sLeaser.sprites[0].alpha = Mother.ConnectionCount / 8f % 1;

        bool allowedToExist = (currentLength - currentPadding) > 0;

        sLeaser.sprites[0].isVisible = allowedToExist && DetermineWhoGetsToExist(allowedToExist) && Enabled;

        // Disable nodes which are offscreen to attempt to save memory? Maybe? It probably won't do anything, but it will give me peace of mind.
        // Nevermind this breaks the game
        // Theoretical memory leaks be damned
        // if (!Utilities.CheckIfOnScreen(this.mother.pos, this.room) && !Utilities.CheckIfOnScreen(this.father.pos, this.room))
        //     sLeaser.sprites[0].isVisible = false;

        sLeaser.sprites[0].rotation = currentRot;

        if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
        {
            sLeaser.sprites[0].shader = Shaders.ConnectingLine;

            sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(lastStepTimer, stepTimer, 1));// timeStacker));

            sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PlayerPosition", new Vector4(Shaders.position.x, Shaders.position.y, 0, 0));

            // sLeaser.sprites[0]._renderLayer?._material?.SetVector("_Colour", new Vector4(this.mother.owner.colour.r, this.mother.owner.colour.g, this.mother.owner.colour.b, 0));
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("connecting_line", true);

        sLeaser.sprites[0].SetPosition(Pos - rCam.pos);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        // I know it looks bad, but I need to clear these on Destroy()
        spriteLeaser ??= sLeaser;
        newContatiner ??= rCam.ReturnFContainer("HUD");
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContatiner.AddChild(fsprite);
        }
    }

    private float lastLen;

    private float lastRot;

    private float stepTimer;

    private float lastPadding;

    private float lastStepTimer;

    private RoomCamera.SpriteLeaser spriteLeaser;
}