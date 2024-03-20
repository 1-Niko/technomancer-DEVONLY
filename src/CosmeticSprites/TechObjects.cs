namespace Slugpack;

public class RoomController : CosmeticSprite
{
    public List<HighlightSprite> nodes { get; }
    public bool scheduledRegenerateConnections { get; set; }
    public Color colour { get; set; }
    public SlugArrow arrow { get; }

    public RoomController(Room room, Color colour, SlugArrow arrow)
    {
        this.room = room;
        this.colour = colour;
        this.arrow = arrow;
        this.nodes = new List<HighlightSprite>();
        this.previousNodeCount = 0;
        this.generateEmptyNodes();
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        this.generateEmptyNodes();
        // if (this.nodes.Count == 0 || this.nodes.Count != this.previousNodeCount)
        this.forceUninitiatedNodesToGenerateConnections();
        this.previousNodeCount = this.nodes.Count;
        if (this.scheduledRegenerateConnections)
        {
            this.regenerateConnectionsForAllNodes();
            this.scheduledRegenerateConnections = false;
        }
        // Debug.Log(this.nodes.Count);
        this.purgeMarkedNodes();
    }

    public override void Destroy()
    {
        for (int i = 0; i < this.nodes.Count; i++)
            this.nodes[i].Destroy();
        this.room.RemoveObject(this);
        base.Destroy();
    }

    public void purgeMarkedNodes()
    {
        int purged_nodes = 0;
        List<HighlightSprite> markedNodes = new List<HighlightSprite>();
        for (int i = 0; i < this.nodes.Count; i++)
            if (this.nodes[i].markedForDeletion)
            {
                this.nodes[i].Destroy();
                markedNodes.Add(this.nodes[i]);
                purged_nodes++;
            }
        if (purged_nodes > 0)
        {
            for (int i = 0; i < markedNodes.Count; i++)
                this.nodes.Remove(markedNodes[i]);
            this.regenerateConnectionsForAllNodes();
        }
    }

    // THINGS TO SOLVE:
    // 1. Nodes are continuously generated when a moving node (creature) is in the room!
    // 2. Connections continuously flicker
    public void generateEmptyNodes()
    {
        List<DataStructures.Node> nodeData = Utilities.GetAllNodeInformation(this.room);

        int addedNodes = 0;

        for (int i = 0; i < nodeData.Count; i++)
        {
            if (!nodes.Any(obj => obj.pos.Equals(nodeData[i].position)) && (nodeData[i].anchor == null || !nodes.Any(obj => obj.anchor == nodeData[i].anchor)))
            {
                // Added so it doesn't immediately get marked for deletion
                if (((this.room.TileHeight * 20f) > nodeData[i].position.y && (this.room.TileWidth * 20f) > nodeData[i].position.x && 0 < nodeData[i].position.x && 0 < nodeData[i].position.y))
                {
                    // going to have to make something to allow for nodes to track their parent object
                    // at least the functionality for the connections is already in place
                    HighlightSprite node = new HighlightSprite(nodeData[i].position, nodeData[i].level, nodeData[i].protection, this.room, this, nodeData[i].anchor);
                    this.nodes.Add(node);
                    this.room.AddObject(node);
                    addedNodes++;
                }
            }
        }
        if (addedNodes > 0)
            this.regenerateConnectionsForAllNodes();
    }

    public void regenerateConnectionsForAllNodes()
    {
        for (int i = 0; i < this.nodes.Count; i++)
        {
            this.nodes[i].regenerateConnections();
        }
    }

    public void forceUninitiatedNodesToGenerateConnections()
    {
        for (int i = 0; i < this.nodes.Count; i++)
        {
            if (this.nodes[i].connections.Count == 0)
            {
                this.nodes[i].regenerateConnections();
            }
        }
    }

    Room room;

    int previousNodeCount;
}

public class HighlightSprite : CosmeticSprite
{
    public float padding { get; set; }
    public List<ConnectingLine> connections { get; set; }
    public float age { get; }
    public int connectionCount { get; set; }
    public PhysicalObject anchor { get; }
    public bool markedForDeletion { get; set; }
    public RoomController owner { get; }
    public Dictionary<HighlightSprite, ConnectingLine> connectionTable { get; }
    public bool isSmall { get; set; }
    public bool selected { get; set; }
    public HighlightSprite(Vector2 pos, int nodeLevel, int protectionLevels, Room room, RoomController owner, PhysicalObject anchor)
    {
        this.age = Utilities.Timestamp();
        this.markedForDeletion = false;
        this.isSmall = false;
        this.connectionCount = 0;
        this.pos = pos;
        this.lastPos = pos;
        this.nodeLevel = nodeLevel;
        this.protectionLevels = protectionLevels;
        this.room = room;
        this.anchor = anchor;
        this.owner = owner;
        this.connections = new List<ConnectingLine>();
        this.connectionTable = new Dictionary<HighlightSprite, ConnectingLine>();
        this.regenerateConnections();
    }

    public override void Destroy()
    {
        // this.owner.scheduledRegenerateConnections = true;
        // this.owner.nodes.Remove(this);
        for (int i = 0; i < this.connections.Count; i++)
            this.connections[i].Destroy();
        for (int i = 0; i < this.spriteLeaser.sprites.Length; i++)
            this.spriteLeaser.sprites[i].RemoveFromContainer();
        this.room.RemoveObject(this);
        base.Destroy();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        this.lastStepTimer = this.stepTimer;
        this.stepTimer++;

        if (this.anchor != null)
            if (this.anchor is Creature)
                this.pos = (this.anchor as Creature).mainBodyChunk.pos;

        if (this.connections.Count == 0)
            this.regenerateConnections();
        this.calculateConnections();

        this.enableValid();
        this.calculateSmallness();

        this.selected = RWCustom.Custom.Dist(this.owner.arrow.pos, this.pos) < 1;

        // Debug.Log($"({this.room.TileHeight * 20f}, {this.room.TileWidth * 20f}) ({this.pos.x - 300f}, {this.pos.y})");
        if ((this.room.TileHeight * 20f) < this.pos.y || (this.room.TileWidth * 20f) < this.pos.x || 0 > this.pos.x || 0 > this.pos.y)
            this.markedForDeletion = true;
    }

    public void regenerateConnections()
    {
        for (int i = 0; i < this.connections.Count; i++)
            this.room.RemoveObject(this.connections[i]);

        this.connections.Clear();
        this.connectionTable.Clear();

        for (int i = 0; i < this.owner.nodes.Count; i++)
        {
            ConnectingLine connection = new ConnectingLine((this.pos + this.owner.nodes[i].pos) / 2, Utilities.CalculateAngleBetweenVectorsForLineSegment(this.pos, this.owner.nodes[i].pos),
                                                     RWCustom.Custom.Dist(this.pos, this.owner.nodes[i].pos) - (this.padding * 2), this.padding, this, this.owner.nodes[i]);
            this.connections.Add(connection);
            this.room.AddObject(this.connections[i]);
            if (!this.connectionTable.ContainsKey(this.owner.nodes[i]))
                this.connectionTable.Add(this.owner.nodes[i], connection);
        }

        this.enableValid();
    }

    public void calculateConnections()
    {
        this.connectionCount = 0;
        for (int i = 0; i < this.connections.Count; i++)
            if (this.connections[i].shouldBeEnabled)
                this.connectionCount++;
    }

    public bool returnValid(HighlightSprite node)
    {
        return node != null && (node.connectionTable != null && (!node.connectionTable.ContainsKey(this) || !node.connectionTable[this].enabled || (node.connectionTable[this].enabled && this.age > node.age)));
    }

    public void calculateSmallness()
    {
        this.isSmall = false;
        for (int i = 0; i < this.owner.nodes.Count; i++)
            if (this.owner.nodes[i] != this && RWCustom.Custom.Dist(this.pos, this.owner.nodes[i].pos) < 45)
                this.isSmall = true;
    }

    // enableValid isn't working as expected, will work on after i get the lerping problem with connections sorted
    // sorted and solved :)
    public void enableValid()
    {
        for (int i = 0; i < this.owner.nodes.Count; i++)
            this.connectionTable[this.owner.nodes[i]].enabled = false;

        //if (Utilities.CheckIfOnScreen(this.pos, this.room))
        //{
        // At least one is bound to be valid
        var (up, left, right, down) = Utilities.GetNearestNodesInAllDirections(this, this.owner.nodes, this.room);

        // Must also check to see if partner has theirs enabled
        if (returnValid(up)) { this.connectionTable[up].enabled = true; }
        if (returnValid(left)) { this.connectionTable[left].enabled = true; }
        if (returnValid(right)) { this.connectionTable[right].enabled = true; }
        if (returnValid(down)) { this.connectionTable[down].enabled = true; }
        if (up != null && up.connectionTable != null) { this.connectionTable[up].shouldBeEnabled = true; }
        if (left != null && left.connectionTable != null) { this.connectionTable[left].shouldBeEnabled = true; }
        if (right != null && right.connectionTable != null) { this.connectionTable[right].shouldBeEnabled = true; }
        if (down != null && down.connectionTable != null) { this.connectionTable[down].shouldBeEnabled = true; }
        //}
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        string sVar = (this.isSmall) ? "A" : "B";

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"karma_{sVar}{this.nodeLevel}");
        sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("protection_1");
        sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName("protection_2");
        sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("protection_3");

        // This is required to not get any strange artifacts. No idea why, but it's 100% required

        sLeaser.sprites[0].scale = 1.0002f;
        sLeaser.sprites[1].scale = 1.0002f;
        sLeaser.sprites[2].scale = 1.0002f;
        sLeaser.sprites[3].scale = 1.0002f;

        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[1].isVisible = this.protectionLevels > 0;
        sLeaser.sprites[2].isVisible = this.protectionLevels > 1;
        sLeaser.sprites[3].isVisible = this.protectionLevels > 2;

        Vector2 currentPos = Vector2.Lerp(lastPos, pos, timeStacker);

        sLeaser.sprites[0].SetPosition(currentPos - rCam.pos);
        sLeaser.sprites[1].SetPosition(currentPos - rCam.pos);
        sLeaser.sprites[2].SetPosition(currentPos - rCam.pos);
        sLeaser.sprites[3].SetPosition(currentPos - rCam.pos);

        sLeaser.sprites[0].color = new Color(0, 0, (this.selected) ? 1 : 0, 0);
        sLeaser.sprites[1].color = new Color(0, 0, (this.selected) ? 1 : 0, 0);
        sLeaser.sprites[2].color = new Color(0, 0, (this.selected) ? 1 : 0, 0);
        sLeaser.sprites[3].color = new Color(0, 0, (this.selected) ? 1 : 0, 0);


        if (Constants.shaders_enabled)
            if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
            {
                sLeaser.sprites[0].shader = Shaders.SelectionShader;
                sLeaser.sprites[1].shader = Shaders.SelectionShader;
                sLeaser.sprites[2].shader = Shaders.SelectionShader;
                sLeaser.sprites[3].shader = Shaders.SelectionShader;

                sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(this.lastStepTimer, this.stepTimer, timeStacker));
                sLeaser.sprites[1]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(this.lastStepTimer, this.stepTimer, timeStacker));
                sLeaser.sprites[2]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(this.lastStepTimer, this.stepTimer, timeStacker));
                sLeaser.sprites[3]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(this.lastStepTimer, this.stepTimer, timeStacker));

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

        string sVar = (this.isSmall) ? "A" : "B";

        sLeaser.sprites[0] = new FSprite($"karma_{sVar}{this.nodeLevel}", true);
        sLeaser.sprites[1] = new FSprite("protection_1", true);
        sLeaser.sprites[2] = new FSprite("protection_2", true);
        sLeaser.sprites[3] = new FSprite("protection_3", true);

        sLeaser.sprites[0].isVisible = true;
        sLeaser.sprites[1].isVisible = this.protectionLevels > 0;
        sLeaser.sprites[2].isVisible = this.protectionLevels > 1;
        sLeaser.sprites[3].isVisible = this.protectionLevels > 2;

        sLeaser.sprites[0].SetPosition(this.pos - rCam.pos);
        sLeaser.sprites[1].SetPosition(this.pos - rCam.pos);
        sLeaser.sprites[2].SetPosition(this.pos - rCam.pos);
        sLeaser.sprites[3].SetPosition(this.pos - rCam.pos);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        // Same here, gotta solve an NRE
        this.spriteLeaser ??= sLeaser;
        newContatiner ??= rCam.ReturnFContainer("HUD");
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContatiner.AddChild(fsprite);
        }
    }

    int nodeLevel;

    int protectionLevels;

    float stepTimer;

    float lastStepTimer;

    Room room;

    RoomCamera.SpriteLeaser spriteLeaser;
}

public class ConnectingLine : CosmeticSprite
{
    public Color colour { get; set; }
    public Vector2 pos { get; set; }
    public float rot { get; set; }
    public float length { get; set; }
    public float padding { get; set; }
    public bool enabled { get; set; }
    public bool shouldBeEnabled { get; set; }
    public HighlightSprite mother { get; }
    public HighlightSprite father { get; }

    public ConnectingLine(Vector2 pos, float rot, float len, float padding, HighlightSprite mother, HighlightSprite father)
    {
        this.enabled = false;
        this.shouldBeEnabled = false;
        this.pos = pos;
        this.rot = rot;
        this.length = length;
        this.padding = 30;

        this.lastPos = pos;
        this.lastRot = rot;
        this.lastLen = len;
        this.lastPadding = padding;

        this.mother = mother;
        this.father = father;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        this.lastStepTimer = this.stepTimer;
        this.stepTimer++;

        this.lastPos = this.pos; // shouldnt have to add this but it's being a bitch
        this.lastLen = this.length;
        this.lastRot = this.rot;
        this.lastPadding = this.padding;

        this.pos = (this.mother.pos + this.father.pos) / 2;
        this.rot = Utilities.CalculateAngleBetweenVectorsForLineSegment(this.mother.pos, this.father.pos);
        this.length = RWCustom.Custom.Dist(this.mother.pos, this.father.pos);// - (this.padding * 2);
        this.padding = this.padding;
    }

    public override void Destroy()
    {
        try
        {
            for (int i = 0; i < this.spriteLeaser.sprites.Length; i++)
                this.spriteLeaser.sprites[i].RemoveFromContainer();
            this.room.RemoveObject(this);
        }
        catch (System.NullReferenceException ex)
        {
            // ignore the problem :)
        }
        base.Destroy();
    }

    public bool determineWhoGetsToExist(bool isEnabled)
    {
        if (!isEnabled)
            return false;
        //else if (!getParentConnectionState(this.father) && this.mother.age < this.father.age)
        else if ((!getParentConnectionState(this.father)) || (getParentConnectionState(this.father) && this.mother.age < this.father.age))
            return true;
        return false;
    }

    public bool getParentConnectionState(HighlightSprite to)
    {
        for (int i = 0; i < to.connections.Count; i++)
            if (to.connections[i] == this)
                return to.connections[i].enabled;
        return false;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("connecting_line");

        Vector2 currentPos = Vector2.Lerp(this.lastPos, this.pos, timeStacker);

        float currentRot = Mathf.Lerp(this.lastRot, this.rot, timeStacker);
        float leniency = 1.5f;
        if ((currentRot > this.lastRot + leniency || currentRot < this.lastRot - leniency) && (currentRot > this.rot + leniency || currentRot < this.rot - leniency))
            currentRot = this.rot;
        float currentLength = Mathf.Lerp(this.lastLen, this.length, timeStacker);
        float currentPadding = Mathf.Lerp(this.lastPadding, this.padding, timeStacker) * 2;

        // This is required to not get any strange artifacts. No idea why, but it's 100% required

        sLeaser.sprites[0].scaleX = (1.0002f / 1024f * (currentLength - currentPadding));
        sLeaser.sprites[0].scaleY = 1.0002f; // 1.0002f;

        sLeaser.sprites[0].SetPosition(currentPos - rCam.pos);

        sLeaser.sprites[0].alpha = (this.mother.connectionCount / 8f) % 1;

        bool allowedToExist = (currentLength - currentPadding) > 0;

        sLeaser.sprites[0].isVisible = (allowedToExist && determineWhoGetsToExist(allowedToExist)) ? this.enabled : false;

        // Disable nodes which are offscreen to attempt to save memory? Maybe? It probably won't do anything, but it will give me peace of mind.
        // Nevermind this breaks the game
        // Theoretical memory leaks be damned
        // if (!Utilities.CheckIfOnScreen(this.mother.pos, this.room) && !Utilities.CheckIfOnScreen(this.father.pos, this.room))
        //     sLeaser.sprites[0].isVisible = false;

        sLeaser.sprites[0].rotation = currentRot;

        if (Constants.shaders_enabled)
            if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var Shaders))
            {
                sLeaser.sprites[0].shader = Shaders.ConnectingLine;

                sLeaser.sprites[0]._renderLayer?._material?.SetFloat("_RandomOffset", Mathf.Lerp(this.lastStepTimer, this.stepTimer, 1));// timeStacker));

                sLeaser.sprites[0]._renderLayer?._material?.SetVector("_PlayerPosition", new Vector4(Shaders.position.x, Shaders.position.y, 0, 0));

                // sLeaser.sprites[0]._renderLayer?._material?.SetVector("_Colour", new Vector4(this.mother.owner.colour.r, this.mother.owner.colour.g, this.mother.owner.colour.b, 0));
            }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("connecting_line", true);

        sLeaser.sprites[0].SetPosition(this.pos - rCam.pos);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        // I know it looks bad, but I need to clear these on Destroy()
        this.spriteLeaser ??= sLeaser;
        newContatiner ??= rCam.ReturnFContainer("HUD");
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContatiner.AddChild(fsprite);
        }
    }

    float lastLen;

    float lastRot;

    float stepTimer;

    float lastPadding;

    float lastStepTimer;

    RoomCamera.SpriteLeaser spriteLeaser;
}