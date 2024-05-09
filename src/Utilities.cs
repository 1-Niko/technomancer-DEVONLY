using MoreSlugcats;

namespace Slugpack;

public static class Utilities
{
    public static bool pipeIsLocked(RainWorldGame game, RWCustom.IntVector2 pos)
    {
        Constants.DamagedShortcuts.TryGetValue(game, out var ShortcutTable);

        for (int i = 0; i < ShortcutTable.locks.Count; i++)
        {
            for (int j = 0; j < ShortcutTable.locks[i].Shortcuts.Length; j++)
            {
                if ((ShortcutTable.locks[i].Shortcuts[j].StartTile == pos || ShortcutTable.locks[i].Shortcuts[j].DestTile == pos) && Constants.isLocked.ContainsKey(ShortcutTable.locks[i].Shortcuts[j]) && Constants.isLocked[ShortcutTable.locks[i].Shortcuts[j]])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void Spark(Room room, int spark_count, Vector2 position, float lerpA, float lerpB, int lifetime)
    {
        for (int j = 0; j < spark_count; j++)
        {
            Vector2 a = RWCustom.Custom.RNV();
            room.AddObject(new Spark(position + a * Random.value * 40f, a * Mathf.Lerp(lerpA, lerpB, Random.value), new Color(0.9f, 0.9f, 1f), null, lifetime, 18));
        }
    }

    public static float closestTrainPosition(Vector2 position, Room room)
    {
        List<TrainObject> trainPositions = [];

        for (int i = 0; i < room.updateList.Count; i++)
        {
            if (room.updateList[i] is TrainObject)
            {
                trainPositions.Add((room.updateList[i] as TrainObject));
            }
        }

        float minimumTrainDistance = float.MaxValue;
        Vector2 closestTrainPos = Vector2.zero;
        if (trainPositions.Count > 0)
        {
            for (int i = 0; i < trainPositions.Count; i++)
            {
                if (trainPositions[i].velocity > 50)
                {
                    float checkingDistance = RWCustom.Custom.Dist(trainPositions[i].pos, position);
                    if (checkingDistance < minimumTrainDistance)
                    {
                        minimumTrainDistance = checkingDistance;
                        closestTrainPos = trainPositions[i].pos;
                    }
                }
            }
        }
        return minimumTrainDistance;
    }

    public static (float minimumDistance, float nearestHeight) closestTrainPosition(Player player)
    {
        List<TrainObject> trainPositions = [];

        for (int i = 0; i < player.room.updateList.Count; i++)
        {
            if (player.room.updateList[i] is TrainObject)
            {
                trainPositions.Add((player.room.updateList[i] as TrainObject));
            }
        }

        float minimumTrainDistance = float.MaxValue;
        Vector2 closestTrainPos = Vector2.zero;
        if (trainPositions.Count > 0)
        {
            for (int i = 0; i < trainPositions.Count; i++)
            {
                if (trainPositions[i].velocity > 50)
                {
                    float checkingDistance = RWCustom.Custom.Dist(trainPositions[i].pos, player.mainBodyChunk.pos);
                    if (checkingDistance < minimumTrainDistance)
                    {
                        minimumTrainDistance = checkingDistance;
                        closestTrainPos = trainPositions[i].pos;
                    }
                }
            }
        }
        return (minimumTrainDistance, closestTrainPos.y);
    }

    public static int Identify(SlugArrow arrow, bool thrw, bool jmp, bool inputHoldThrw, bool inputHoldJmp)
    {
        /*
0000 *****      0-63    ERROR
          	   
0001 00000     64,65    Yellow Lizard              CREATURE // Don't know how to detect specific lizard types yet, if you can solve that that would be very helpful but I'm not dealing with it right now.
0001 00001     66,67    Vultures                   CREATURE
0001 00010     68,69    Brother Long Legs          CREATURE
0001 00011     70,71    Leviathans                 CREATURE
0001 00100     72,73    Miros Birds                CREATURE
0001 00101     74,75    Daddy Long Legs            CREATURE
0001 00110     76,77    Vulture Grubs              CREATURE
0001 00111     78,79    Looks To The Moon          ?
0001 01000     80,81    Five Pebbles               ?
0001 01001     82,83    Cyan Lizards               CREATURE
0001 01010     84,85    King Vultures              CREATURE
0001 01011     86,87    Red Centipedes             CREATURE
0001 01100     88,89    Overseer                   CREATURE
0001 01101     90,91    Neuron                     CREATURE
          	   
              92-127    ERROR                      
          	   
0010 00000   128,129    Shortcuts                  SHORTCUT
0010 00001   130,131    Gate                       ?
          	   
                        ERROR                      
          	   
0011 00000   192,193    Miros Vultures             CREATURE
0011 00001   194,195    Mother Long Legs           CREATURE
0011 00010   196,197    Inspectors                 CREATURE
          	   
                        ERROR                      
          	   
0100 00000   256,257    Pearl                      ITEM
0100 00001   258,259    Overseer Eye               ITEM
          	   
                        ERROR                      
          	   
0101 00000   320,321    Singularity Bomb           ITEM // Deal with later
0101 00001   322,323    Rarefaction Cell           ITEM
0101 00010   324,325    Inspector Eye              ITEM
          	   
                        ERROR                      
          	   
0111 00000   448,449    Train Warnings             OBJECT
0111 00001   450,451    Hologram Advertisements    OBJECT
0111 00010   452,453    Holograms (In General)     OBJECT
0111 00011   454,455    Transformers               OBJECT
          	   
                        ERROR                      
          	   
1000 00000   512,513    Dronemaster's Drones       ?
        */

        bool isCreature = false;
        bool isObject = false;
        bool isItem = false;
        bool isShortcut = false;

        string nearestObjectType = null;
        Creature nearestCreature = null;
        PhysicalObject nearestItem = null;
        ShortcutData nearestShortcut = new();
        PlacedObject nearestObject = null;
        Vector2 nearestPosition = Vector2.zero;

        if (thrw && inputHoldThrw) return 0;
        if (jmp && inputHoldJmp) return 0;

        if (arrow != null && arrow.room != null)
        {
            (nearestObjectType, nearestCreature, nearestItem, nearestShortcut, nearestObject, nearestPosition) = DetermineObjectFromPosition(arrow.pos, arrow.room);

            isCreature = nearestCreature != null;
            isObject = nearestObject != null && arrow._object != null;
            isItem = nearestItem != null;
            isShortcut = nearestObjectType == "shortcut";


            if (nearestItem != null && nearestItem is SSOracleSwarmer)
                return (thrw) ? 90 : (jmp) ? 91 : 0;

            if (isCreature)
            {
                if (arrow.creature.stun != 0)
                    return 0;

                // Adding in the error condition here because it might be an option to add a third thing (if both inputs are held at the same time) if we want to later, for now though it's just going to throw the error condition
                if (nearestCreature is Lizard && nearestCreature.Template.type == CreatureType.YellowLizard)
                    return (thrw) ? 64 : (jmp) ? 65 : 0;
                else if (nearestCreature is Vulture && nearestCreature.Template.type == CreatureType.Vulture)
                    return (thrw) ? 66 : (jmp) ? 67 : 0;
                else if (nearestCreature is DaddyLongLegs && nearestCreature.Template.type == CreatureType.BrotherLongLegs)
                    return (thrw) ? 68 : (jmp) ? 69 : 0;
                else if (nearestCreature is BigEel)
                    return (thrw) ? 70 : (jmp) ? 71 : 0;
                else if (nearestCreature is MirosBird)
                    return (thrw) ? 72 : (jmp) ? 73 : 0;
                else if (nearestCreature is DaddyLongLegs)
                    return (thrw) ? 74 : (jmp) ? 75 : 0;
                else if (nearestCreature is VultureGrub && (arrow.creature as VultureGrub).signalWaitCounter == 0 && (arrow.creature as VultureGrub).singalCounter == 0)
                    return (thrw) ? 76 : (jmp) ? 77 : 0;

                else if (nearestCreature is Lizard && nearestCreature.Template.type == CreatureType.CyanLizard)
                    return (thrw) ? 82 : (jmp) ? 83 : 0;
                else if (nearestCreature is Vulture && nearestCreature.Template.type == CreatureType.KingVulture)
                    return (thrw) ? 84 : (jmp) ? 85 : 0;
                else if (nearestCreature is Centipede && nearestCreature.Template.type == CreatureType.RedCentipede)
                    return (thrw) ? 86 : (jmp) ? 87 : 0;
                else if (nearestCreature is Overseer)
                    return (thrw) ? 88 : (jmp) ? 89 : 0;

                else if (nearestCreature is Vulture && nearestCreature.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
                    return (thrw) ? 192 : (jmp) ? 193 : 0;
                else if (nearestCreature is DaddyLongLegs && nearestCreature.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
                    return (thrw) ? 194 : (jmp) ? 195 : 0;
                else if (nearestCreature is Inspector)
                    return (thrw) ? 196 : (jmp) ? 197 : 0;

            }
            else if (isObject)
            {
                if (arrow._object.type.ToString() == "TrainBell")
                    return (thrw) ? 448 : (jmp) ? 449 : 0;
                else if (arrow._object.type.ToString() == "TrackHologram")
                    return (thrw) ? 450 : (jmp) ? 451 : 0;
            }
            else if (isItem && nearestItem is PlayerCarryableItem)
            {
                if (nearestItem is DataPearl)
                    return (thrw) ? 256 : (jmp) ? 257 : 0;
                else if (nearestItem is OverseerCarcass)
                    return (thrw) ? 258 : (jmp) ? 259 : 0;
                else if (nearestItem is SingularityBomb)
                    return (thrw) ? 320 : (jmp) ? 321 : 0;
                else if (nearestItem is OverseerCarcass && (nearestItem.abstractPhysicalObject as OverseerCarcass.AbstractOverseerCarcass).InspectorMode)
                    return (thrw) ? 324 : (jmp) ? 325 : 0;
            }
            else if (isShortcut)
                return (thrw) ? 128 : (jmp) ? 129 : 0;
        }

        return 0;
    }

    public static bool InRange(int x, int low, int high)
    {
        return x >= low && x <= high;
    }

    public static float BinaryToFloat(int minValue, string chunk, List<float> values)
    {
        float result = 0;
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] != -1)
                result += values[i] * int.Parse(chunk[i + minValue].ToString());
        }

        if (values[0] == -1)
            result *= (-2 * int.Parse(chunk[minValue].ToString())) + 1;

        return result;
    }

    public static string ConvertNumbersToBinaryString(ulong[] numbers, int length)
    {
        string binaryString = "";
        foreach (ulong number in numbers)
        {
            binaryString += Convert.ToString((long)number, 2).PadLeft(length, '0');
        }
        return binaryString;
    }

    public static List<string> SplitStringIntoChunks(string input, int chunkSize)
    {
        List<string> chunks = [];
        int length = input.Length;

        for (int i = 0; i < length; i += chunkSize)
        {
            if (i + chunkSize <= length)
            {
                string chunk = input.Substring(i, chunkSize);
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    public static float CalculateFurRotation(float defaultRotation, float minRotation, float maxRotation, float wetness, float danger, float breathingFactor)
    {
        return ((1 - wetness) * ((danger * maxRotation) + ((1 - danger) * (defaultRotation - (2 * breathingFactor))))) + (wetness * minRotation);
    }

    public static float FluffWax(float A, float B, float period, float x)
    {
        return ((A - B) / 2f * (float)Math.Sin(2f * (float)Math.PI / period * x)) + ((A + B) / 2f);
    }

    public static int MaxScreen(string roomName)
    {
        for (int i = 0; i < 21; i++)
        {
            if (!File.Exists(AssetManager.ResolveFilePath($"screens/{roomName}_{i + 1}.png")))
            {
                return i;
            }
        }
        throw new NullReferenceException("No screens detected!");
    }

    public static void ShowMessage(RoomCamera roomCamera, string messageContents, int messageTime)
    {
        HUD.DialogBox dialogue = roomCamera.hud.InitDialogBox();
        dialogue.NewMessage(messageContents, messageTime);
    }

    public static string GetEquivalence(string inputText, string baseAcronym, SlugcatStats.Name character)
    {
        if (inputText is null)
        {
            throw new ArgumentNullException(nameof(inputText));
        }

        string text = "";

        foreach (var path in AssetManager.ListDirectory("World", true, false)
            .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
            .Where(File.Exists)
            .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
        {
            var parts = path.Contains("-") ? path.Split('-') : [path];
            if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], StringComparison.OrdinalIgnoreCase)))
            {
                text = Path.GetFileName(path).ToUpper();
                break;
            }
        }

        return text;
    }

    public static Vector2 GetPointC(Vector2 pointA, Vector2 pointB)
    {
        // Calculate the direction vector from A to B
        Vector2 direction = pointB - pointA;

        // Normalize the direction vector to get a unit vector
        direction.Normalize();

        // Calculate the distance between A and C
        float distance = Vector2.Distance(pointA, pointB);

        // Calculate the position of point C along the same line in the direction from A to B
        Vector2 pointC = pointA + (direction * distance);

        return pointC;
    }

    public static Vector2 MultiplyVector2ByFloats(Vector2 vector2, float x, float y)
    {
        return new Vector2(vector2.x * x, vector2.y * y);
    }

    public static float AngleBetween(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 ab = b - a;
        Vector2 bc = c - b;

        float angle = Vector2.Angle(ab, bc);

        // Determine the direction of the angle
        Vector3 cross = Vector3.Cross(ab, bc);
        if (cross.z > 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }

    public static Vector2 RotateAroundPoint(Vector2 center, Vector2 offset, float degrees)
    {
        offset += center;
        float radians = (float)(degrees * Math.PI / 180.0);
        float cos = (float)Math.Cos(radians);
        float sin = (float)Math.Sin(radians);
        float x = ((offset.x - center.x) * cos) - ((offset.y - center.y) * sin) + center.x;
        float y = ((offset.x - center.x) * sin) + ((offset.y - center.y) * cos) + center.y;
        return new Vector2(x, y);
    }

    public static float NegativeAbs(float x, float n, float X, float Y)
    {
        return -Mathf.Abs(n * (X - x)) + Y;
    }

    public static bool IsBelowFunction(Vector2 point, Vector2 center)
    {
        // Calculate the y-coordinate of the function at the x-coordinate of the point
        float functionY = -Mathf.Abs(point.x - center.x);

        // Check if the y-coordinate of the point is below the y-coordinate of the function
        return point.y < center.y + functionY;
    }

    public static (List<Vector2> positions, List<PhysicalObject> creatures, List<PhysicalObject> items, List<PlacedObject> objects) GetEverything(Room room)
    {
        var positions = room.shortcuts
            .Where(element => (element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1) && element.shortCutType == ShortcutData.Type.RoomExit)
            .Where(element => room.ViewedByAnyCamera(room.MiddleOfTile(element.StartTile), 0f))
            .Select(element => room.MiddleOfTile(element.StartTile))
            .ToList();

        var creatures = room.physicalObjects[1]
            .Where(element => (element as Creature is MirosBird or VultureGrub or Vulture or Inspector or Overseer) || (element is Lizard && (((element as Creature).Template.type == CreatureType.YellowLizard) || ((element as Creature).Template.type == CreatureType.CyanLizard))))
            .Where(element => room.ViewedByAnyCamera((element as Creature).mainBodyChunk.pos, 0f))
            .Where(element => !(element as Creature).dead)
            .ToList();

        var items = room.physicalObjects[2]
            .Where(element => element as PlayerCarryableItem is DataPearl or OverseerCarcass)
            .Where(element => room.ViewedByAnyCamera((element as PlayerCarryableItem).firstChunk.pos, 0f))
            .ToList();

        items.AddRange(room.physicalObjects[0]
            .Where(element => element is SSOracleSwarmer && element != null && !(element as SSOracleSwarmer).slatedForDeletetion)
            .Where(element => room.ViewedByAnyCamera(element.firstChunk.pos, 0f))
            .ToList());

        /*var objects = room.roomSettings.placedObjects
            .Where(element => element.type.ToString() is "TrackHologram")
            .Where(element => room.ViewedByAnyCamera((element.data as TrackHologramData).handle[1], 0f))
            .ToList();*/

        var objects = new List<PlacedObject>();
        foreach (var element in room.roomSettings.placedObjects)
        {
            if (element.type.ToString() == "TrackHologram")
            {
                var hologramData = element.data as TrackHologramData;

                if (hologramData != null && room.ViewedByAnyCamera(element.pos + hologramData.handle[1], 0f))
                {
                    objects.Add(element);
                }
            }
        }

        return (positions, creatures, items, objects);
    }

    public static (string nearestObjectType, Creature nearestCreature, PhysicalObject nearestItem, ShortcutData nearestShortcut, PlacedObject nearestObject, Vector2 nearestPosition) DetermineObjectFromPosition(Vector2 position, Room room)
    {
        var (positions, creatures, items, objects) = GetEverything(room);

        creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
        items.ForEach(item => positions.Add(item.firstChunk.pos));

        foreach (var shortcut in room.shortcuts)
        {
            if (room.MiddleOfTile(shortcut.StartTile) == position)
            {
                return ("shortcut", null, null, shortcut, null, position);
            }
        }

        foreach (var creature in creatures)
        {
            if ((creature as Creature).mainBodyChunk.pos == position)
            {
                return ("creature", creature as Creature, null, new ShortcutData(), null, position);
            }
        }

        foreach (var item in items)
        {
            if (item.firstChunk.pos == position)
            {
                return ("item", null, item, new ShortcutData(), null, position);
            }
        }

        foreach (var _object in objects)
        {
            if (_object.pos == position)
            {
                return ("object", null, null, new ShortcutData(), _object, position);
            }
        }

        return ("none", null, null, new ShortcutData(), null, Vector2.zero);
    }

    public static List<Vector2> GetPointsInDirection(Vector2 position, Vector2 direction, List<Vector2> searchPositions)
    {
        var result = new List<Vector2>();

        foreach (var searchPosition in searchPositions)
        {
            if (searchPosition == position) continue; // skip the position itself

            var delta = searchPosition - position;
            var angle = Vector2.Angle(delta, direction);

            if (angle is <= 45f or >= 315f) // within 45 degrees either way of the direction
            {
                result.Add(searchPosition);
            }
        }

        return result;
    }

    public static (Vector2 up, Vector2 left, Vector2 right, Vector2 down) GetNearestPointsInAllDirections(Vector2 position, List<Vector2> searchPositions, Room room)
    {
        _ = Vector2.zero;
        _ = Vector2.zero;
        _ = Vector2.zero;
        _ = Vector2.zero;

        Vector2 up = FindNearest(position, GetPointsInDirection(position, Vector2.up, searchPositions), room);
        Vector2 left = FindNearest(position, GetPointsInDirection(position, Vector2.left, searchPositions), room);
        Vector2 right = FindNearest(position, GetPointsInDirection(position, Vector2.right, searchPositions), room);
        Vector2 down = FindNearest(position, GetPointsInDirection(position, Vector2.down, searchPositions), room);

        return (up, left, right, down);
    }

    public static List<HighlightSprite> GetNodesInDirection(HighlightSprite position, Vector2 direction, List<HighlightSprite> searchPositions)
    {
        var result = new List<HighlightSprite>();

        foreach (var searchPosition in searchPositions)
        {
            if (searchPosition.pos == position.pos) continue; // skip the position itself

            var delta = searchPosition.pos - position.pos;
            var angle = Vector2.Angle(delta, direction);

            if (angle is <= 45f or >= 315f) // within 45 degrees either way of the direction
            {
                result.Add(searchPosition);
            }
        }

        return result;
    }

    public static HighlightSprite FindNearestNode(HighlightSprite input, List<HighlightSprite> vectors, Room room, List<HighlightSprite> nodesToIgnore = null)
    {
        if (room is null)
        {
            throw new ArgumentNullException(nameof(room));
        }

        HighlightSprite nearest = null;
        float distance = Mathf.Infinity;
        nodesToIgnore ??= []; // Set to empty list if null

        foreach (var vector in vectors.Where(v => !nodesToIgnore.Contains(v)))//.Where(p => room.ViewedByAnyCamera(p.pos, 0f)))
        {
            var newDistance = Vector2.Distance(input.pos, vector.pos);
            if (newDistance < distance)
                (nearest, distance) = (vector, newDistance);
        }

        return nearest;
    }

    public static (HighlightSprite up, HighlightSprite left, HighlightSprite right, HighlightSprite down) GetNearestNodesInAllDirections(HighlightSprite self, List<HighlightSprite> searchPositions, Room room)
    {
        HighlightSprite up = FindNearestNode(self, GetNodesInDirection(self, Vector2.up, searchPositions), room);
        HighlightSprite left = FindNearestNode(self, GetNodesInDirection(self, Vector2.left, searchPositions), room);
        HighlightSprite right = FindNearestNode(self, GetNodesInDirection(self, Vector2.right, searchPositions), room);
        HighlightSprite down = FindNearestNode(self, GetNodesInDirection(self, Vector2.down, searchPositions), room);

        return (up, left, right, down);
    }

    public static float CalculateAngleBetweenVectorsForLineSegment(Vector2 vector1, Vector2 vector2)
    {
        // Calculate the average point between the two vectors
        _ = (vector1.x + vector2.x) / 2.0f;
        _ = (vector1.y + vector2.y) / 2.0f;

        // Calculate the angle of the line segment passing through the average point
        float angleRadians = (float)Math.Atan2(vector2.y - vector1.y, vector2.x - vector1.x);

        // Convert the angle from radians to degrees
        float angleDegrees = angleRadians * (180.0f / (float)Math.PI);

        return -angleDegrees;
    }

    public static List<Node> GetAllNodeInformation(Room room)
    {
        var (positions, creatures, items, objects) = GetEverything(room);

        // positions, karma level, protection level

        creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
        items.ForEach(item => positions.Add(item.firstChunk.pos));
        objects.ForEach(_object => positions.Add(_object.pos));

        List<Node> NodeInfo = [];

        // "element.destNode != -1" seems to be enough to distinguish from pipe entrances and everything else
        // foreach (var shortcut in room.shortcuts.Where(element => element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1).ToList())
        // foreach (var shortcut in room.shortcuts.Where(element => element.LeadingSomewhere).ToList()) // Filters out the wack a mole holes and spawnpoints, but also selects exit pipes with no connection set

        // Use this one when shortcut support is added
        foreach (var shortcut in room.shortcuts.Where(element => (element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1) && element.shortCutType == ShortcutData.Type.RoomExit).ToList())

        //foreach (var shortcut in room.shortcuts.Where(element => element.destNode != -1).ToList())
        {
            NodeInfo.Add(new Node(room.MiddleOfTile(shortcut.StartTile), 1, 0, null, null));
            //room.MiddleOfTile(shortcut.StartTile), 1, 0, null));
        }

        foreach (var creature in creatures)
        {
            if ((creature as Creature is MirosBird || creature as Creature is VultureGrub || creature as Creature is Vulture ||
                creature as Creature is Inspector || creature as Creature is Overseer) && room.ViewedByAnyCamera((creature as Creature).mainBodyChunk.pos, 0f) &&
                (!(creature as Creature).dead) || (creature is Lizard && (((creature as Creature).Template.type == CreatureType.YellowLizard) || ((creature as Creature).Template.type == CreatureType.CyanLizard))))
            {
                NodeInfo.Add(new Node((creature as Creature).mainBodyChunk.pos, 2, 0, creature, null));
            }
        }
        NodeInfo.AddRange(from item in items
                          where item as PlayerCarryableItem is DataPearl or OverseerCarcass
                          select new Node((item as PlayerCarryableItem).firstChunk.pos, 1, 0, item, null));

        NodeInfo.AddRange(from _object in room.physicalObjects[0]
                          where _object is SSOracleSwarmer && _object != null && !(_object as SSOracleSwarmer).slatedForDeletetion
                          select new Node(_object.firstChunk.pos, 1, 0, _object as PhysicalObject, null));

        // There's a bug with these at the moment
        foreach (var _object in objects.Where(element => element.type.ToString() == "TrackHologram"))// || element.type.ToString() == "TrainBell"))
        {
            NodeInfo.Add(new Node(_object.pos + (_object.data as TrackHologramData).handle[1], 3, 0, null, _object));
        }

        return NodeInfo;
    }

    public static (List<Vector2> positions, string nearestObjectType, Creature nearestCreature, PhysicalObject nearestItem, PlacedObject nearestPlacedObject, Vector2 nearestPosition) GetPositions(Room room, Vector2 searchPosition, bool positionsOnly)
    {
        var (positions, creatures, items, objects) = GetEverything(room);

        creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
        items.ForEach(item => positions.Add(item.firstChunk.pos));
        objects.ForEach(_object => positions.Add(_object.pos));

        if (positionsOnly)
        {
            return (positions, null, null, null, null, Vector2.zero);
        }

        string nearestObjectType = "";
        Creature nearestCreature = null;
        PhysicalObject nearestItem = null;
        PlacedObject nearestPlacedObject = null;

        Vector2 nearestPosition = Vector2.zero;
        float nearestDistance = Mathf.Infinity;

        foreach (var shortcut in room.shortcuts)
        {
            if ((shortcut.destNode != -1 && shortcut.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[shortcut.destNode] != -1) && shortcut.shortCutType == ShortcutData.Type.RoomExit)
            {
                Vector2 position = room.MiddleOfTile(shortcut.StartTile);
                float distance = RWCustom.Custom.Dist(position, searchPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPosition = position;
                    nearestObjectType = "shortcut";
                }
            }
        }

        foreach (var creature in creatures)
        {
            Vector2 position = (creature as Creature).mainBodyChunk.pos;
            float distance = RWCustom.Custom.Dist(position, searchPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPosition = position;
                nearestObjectType = "creature";
                nearestCreature = creature as Creature;
            }
        }

        foreach (var item in items)
        {
            Vector2 position = item.firstChunk.pos;
            float distance = RWCustom.Custom.Dist(position, searchPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPosition = position;
                nearestObjectType = "item";
                nearestItem = item;
            }
        }

        foreach (var _object in objects)
        {
            Vector2 position = _object.pos;
            float distance = RWCustom.Custom.Dist(position, searchPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPosition = position;
                nearestObjectType = "object";
                nearestPlacedObject = _object;
            }
        }

        return (positions, nearestObjectType, nearestCreature, nearestItem, nearestPlacedObject, nearestPosition);
    }

    public static bool CheckIfOnScreen(Vector2 position, Room room)
    {
        return room.ViewedByAnyCamera(position, 0f);
    }

    public static Vector2 FindNearest(Vector2 input, List<Vector2> vectors, Room room, List<Vector2> vectorsToIgnore = null)
    {
        var nearest = Vector2.zero;
        var distance = Mathf.Infinity;
        vectorsToIgnore ??= []; // Set to empty list if null

        foreach (var vector in vectors.Where(v => !vectorsToIgnore.Contains(v)).Where(p => room.ViewedByAnyCamera(p, 0f)))
        {
            var newDistance = Vector2.Distance(input, vector);
            if (newDistance < distance)
                (nearest, distance) = (vector, newDistance);
        }

        return nearest;
    }

    public static Vector2 CalculateVector(Vector2 StartFrom, Vector2 PointTo, Vector2 Vector, float Power)
    {
        // Calculate the direction vector from StartFrom to PointTo
        Vector2 direction = PointTo - StartFrom;

        // Calculate the magnitude of the resulting vector based on Power
        float magnitude = Mathf.Lerp(Vector.magnitude, direction.magnitude, Power);

        // Normalize the direction vector and scale it by the magnitude
        Vector2 newVector = direction.normalized * magnitude;

        // Return the new vector
        return newVector;
    }

    /// <summary>
    /// Given the name of a file marked as "Embedded Resource" in the VS solution, this will load it as a unity <see cref="AssetBundle"/>.
    /// </summary>
    /// <param name="fullyQualifiedPath"></param>
    /// <returns></returns>
    public static AssetBundle LoadFromEmbeddedResource_OLD(string fullyQualifiedPath)
    {
        //DebugLog($"Loading embedded asset bundle: {fullyQualifiedPath}");
        using MemoryStream mstr = new();
        Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullyQualifiedPath);
        str.CopyTo(mstr);
        str.Flush();
        str.Close();
        //DebugLog("Bundle loaded into memory as byte[], processing with Unity...");
        AssetBundle bundle = AssetBundle.LoadFromMemory(mstr.ToArray());
        //DebugLog("Unity has successfully loaded this asset bundle from memory.");
        return bundle;
    }

    public static AssetBundle LoadFromEmbeddedResource(string fullyQualifiedPath)
    {
        return AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(fullyQualifiedPath));
    }

    public static FShader CreateFromAsset(AssetBundle bundle, string shortName)
    {
        //DebugLog($"Loading shader \"{shortName}\"...");
        Shader target = bundle.LoadAsset<Shader>($"assets/{shortName}.shader");
        //DebugLog($"Implementing shader \"{shortName}\" into Futile...");
        return FShader.CreateShader(shortName, target);
    }

    public static Vector2 Lerp(Vector2 start, Vector2 end, float t)
    {
        t = Mathf.Clamp(t, 0f, 1f); // Ensure t is within the range [0, 1]
        return start + ((end - start) * t);
    }

    public static Vector4 Lerp(Vector4 start, Vector4 end, float t)
    {
        t = Mathf.Clamp(t, 0f, 1f); // Ensure t is within the range [0, 1]
        return start + ((end - start) * t);
    }

    public static Vector4 ColourFade(Vector4 vector1, Vector4 vector2, float t)
    {
        return Lerp(vector1, vector2, t);
    }

    public static List<int> GenerateNumbers(int N)
    {
        List<int> numbers = [];
        for (int i = 0; i <= N; i++)
        {
            numbers.Add(i);
        }
        return numbers;
    }

    public static void Shuffle(List<int> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static float Normalize(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        // Check for division by zero or invalid ranges
        if (oldMax - oldMin == 0)
        {
            throw new ArgumentException("Old range is invalid (min and max are the same).");
        }

        // Normalize the value
        float normalizedValue = ((oldValue - oldMin) / (oldMax - oldMin) * (newMax - newMin)) + newMin;

        // Make sure the normalized value doesn't exceed the new range
        normalizedValue = Math.Max(newMin, Math.Min(newMax, normalizedValue));

        return normalizedValue;
    }

    public static Color ColourLerp(Color ColourA, Color ColourB, float N)
    {
        float R = Mathf.Lerp(ColourA.r, ColourB.r, N);
        float G = Mathf.Lerp(ColourA.g, ColourB.g, N);
        float B = Mathf.Lerp(ColourA.b, ColourB.b, N);

        return new Color(R, G, B);
    }

    public static float Timestamp()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        float unixTimestamp = (float)(now.ToUnixTimeSeconds() + (now.Millisecond / 1000.0));
        return unixTimestamp;
    }

    public static float EncodeBools(bool a, bool b)
    {
        return !a && !b ? 0.25f : a && !b ? 0.5f : !a && b ? 0.75f : a && b ? 1f : 0f;
    }

    public static int BinStep(int N)
    {
        return N >= 0 ? 1 : 0;
    }

    public static List<float> IntFloatListBinaryEncoding(int N)
    {
        // All outputs will have 4 bits, so there's no need to generalize
        return [BinStep((N % 32) - 16), BinStep((N % 16) - 8), BinStep((N % 8) - 4), BinStep((N % 4) - 2), BinStep((N % 2) - 1)];
    }

    public static float FloatListBinaryEncoding(List<float> a, List<float> b, List<float> c, List<float> d)
    {
        return (a[0] * Mathf.Pow(2, 19)) + (a[1] * Mathf.Pow(2, 18)) + (a[2] * Mathf.Pow(2, 17)) + (a[3] * Mathf.Pow(2, 16)) + (a[4] * Mathf.Pow(2, 15)) +
                       (b[0] * Mathf.Pow(2, 14)) + (b[1] * Mathf.Pow(2, 13)) + (b[2] * Mathf.Pow(2, 12)) + (b[3] * Mathf.Pow(2, 11)) + (b[4] * Mathf.Pow(2, 10)) +
                       (c[0] * Mathf.Pow(2, 9)) + (c[1] * Mathf.Pow(2, 8)) + (c[2] * Mathf.Pow(2, 7)) + (c[3] * Mathf.Pow(2, 6)) + (c[4] * Mathf.Pow(2, 5)) +
                       (d[0] * Mathf.Pow(2, 4)) + (d[1] * Mathf.Pow(2, 3)) + (d[2] * Mathf.Pow(2, 2)) + (d[3] * Mathf.Pow(2, 1)) + (d[4] * Mathf.Pow(2, 0));
    }

    public static int ConvertFloat(float N)
    {
        return (int)Mathf.Round(N * 31f);
    }

    public static float EncodeFloats(float a, float b, float c, float d)
    {
        return FloatListBinaryEncoding(IntFloatListBinaryEncoding(ConvertFloat(a)),
                                       IntFloatListBinaryEncoding(ConvertFloat(b)),
                                       IntFloatListBinaryEncoding(ConvertFloat(c)),
                                       IntFloatListBinaryEncoding(ConvertFloat(d)));
    }

    public static float FloatListBinaryEncoding(List<float> b, List<float> c, List<float> d)
    {
        return (b[0] * Mathf.Pow(2, 14)) + (b[1] * Mathf.Pow(2, 13)) + (b[2] * Mathf.Pow(2, 12)) + (b[3] * Mathf.Pow(2, 11)) + (b[4] * Mathf.Pow(2, 10)) +
                       (c[0] * Mathf.Pow(2, 9)) + (c[1] * Mathf.Pow(2, 8)) + (c[2] * Mathf.Pow(2, 7)) + (c[3] * Mathf.Pow(2, 6)) + (c[4] * Mathf.Pow(2, 5)) +
                       (d[0] * Mathf.Pow(2, 4)) + (d[1] * Mathf.Pow(2, 3)) + (d[2] * Mathf.Pow(2, 2)) + (d[3] * Mathf.Pow(2, 1)) + (d[4] * Mathf.Pow(2, 0));
    }

    public static float EncodeFloats(float a, float b, float c)
    {
        return FloatListBinaryEncoding(IntFloatListBinaryEncoding(ConvertFloat(a)), IntFloatListBinaryEncoding(ConvertFloat(b)), IntFloatListBinaryEncoding(ConvertFloat(c)));
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.TrimStart('#'); // Remove the '#' if it's included in the input
        if (hex.Length != 6)
        {
            throw new ArgumentException("Invalid HEX code. It should be 6 characters long.");
        }

        int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static bool IsTechnomancerOrVoyager(SlugcatStats.Name slugName)
    {
        return null != slugName && (slugName.ToString() == Constants.Technomancer || slugName.ToString() == Constants.Voyager);
    }

    public static bool IsTechnomancerOrVoyager(Creature crit)
    {
        return crit is Player player && IsTechnomancerOrVoyager(player.slugcatStats.name);
    }

    public static void InPlaceTryCatch<T>(ref T variableToSet, T defaultValue, string errorMessage, [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            variableToSet = defaultValue;
        }
        catch (Exception ex)
        {
            Plugin.DebugError(errorMessage.Replace("%ln", $"{lineNumber}"));
            Debug.LogException(ex);
        }
    }

    /*public static Texture2D SpriteToTexture2D(FSprite sprite)
    {
        var texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        );
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public static UnityEngine.Color SampleTexture(Texture2D texture, float u, float v)
    {
        // Ensure the UVs are clamped between 0 and 1
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);

        // Sample the color using bilinear filtering
        UnityEngine.Color color = texture.GetPixelBilinear(u, v);

        return color;
    }*/

    public static float Floor(float number)
    {
        return (int)number;
    }

    public static void UnregisterEnums(Type type)
    {
        var extEnums = type.GetFields(BindingFlags.Static | BindingFlags.Public).Where(x => x.FieldType.IsSubclassOf(typeof(ExtEnumBase)));

        foreach (var extEnum in extEnums)
        {
            var obj = extEnum.GetValue(null);
            if (obj != null)
            {
                _ = obj.GetType().GetMethod("Unregister")!.Invoke(obj, null);
                extEnum.SetValue(null, null);
            }
        }
    }
}