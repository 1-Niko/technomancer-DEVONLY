using IL.MoreSlugcats;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Slugpack
{
    public class Utilities
    {
        public static float BinaryToFloat(int minValue, string chunk, List<float> values)
        {
            float result = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != -1)
                    result += values[i] * int.Parse(chunk[i + minValue].ToString());
            }

            if (values[0] == -1)
                result *= (-2 * int.Parse(chunk[minValue].ToString()) + 1);
            
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
            List<string> chunks = new();
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
            return (1 - wetness) * (danger * maxRotation + (1 - danger) * (defaultRotation - (2 * breathingFactor))) + wetness * minRotation;
        }

        public static float FluffWax(float A, float B, float period, float x)
        {
            return (A - B) / 2f * (float)Math.Sin(2f * (float)Math.PI / period * x) + ((A + B) / 2f);
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
            string text = "";

            foreach (var path in AssetManager.ListDirectory("World", true, false)
                .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
                .Where(File.Exists)
                .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
            {
                var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
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
            Vector2 pointC = pointA + direction * distance;

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
            float x = (offset.x - center.x) * cos - (offset.y - center.y) * sin + center.x;
            float y = (offset.x - center.x) * sin + (offset.y - center.y) * cos + center.y;
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
                .Where(element => (element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1) || element.shortCutType == ShortcutData.Type.Normal)
                .Where(element => room.ViewedByAnyCamera(room.MiddleOfTile(element.StartTile), 0f))
                .Select(element => room.MiddleOfTile(element.StartTile))
                .ToList();

            var creatures = room.physicalObjects[1]
                .Where(element => (element as Creature) is MirosBird || (element as Creature) is VultureGrub || (element as Creature) is Vulture || (element as Creature) is MoreSlugcats.Inspector || (element as Creature) is Overseer)
                .Where(element => room.ViewedByAnyCamera((element as Creature).mainBodyChunk.pos, 0f))
                .Where(element => !(element as Creature).dead)
                .ToList();

            var items = room.physicalObjects[2]
                .Where(element => (element as PlayerCarryableItem) is DataPearl || (element as PlayerCarryableItem) is OverseerCarcass)
                .Where(element => room.ViewedByAnyCamera((element as PlayerCarryableItem).firstChunk.pos, 0f))
                .ToList();

            items.AddRange(room.physicalObjects[0]
                .Where(element => (element is SSOracleSwarmer))
                .Where(element => room.ViewedByAnyCamera(element.firstChunk.pos, 0f))
                .ToList());

            var objects = room.roomSettings.placedObjects
                .Where(element => element.type.ToString() == "TrackHologram" || element.type.ToString() == "TrainBell")
                .Where(element => room.ViewedByAnyCamera(element.pos, 0f))
                .ToList();

            return (positions, creatures, items, objects);
        }

        public static (string nearestObjectType, Creature nearestCreature, PhysicalObject nearestItem, ShortcutData nearestShortcut, PlacedObject nearestObject, Vector2 nearestPosition) DetermineObjectFromPosition(Vector2 position, Room room)
        {
            var (positions, creatures, items, objects) = GetEverything(room);

            creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
            items.ForEach(item => positions.Add(item.firstChunk.pos));

            foreach (var shortcut in room.shortcuts)
            {
                //Debug.Log($"LOOK HERE : FUNCTION CALL : SHORTCUT : {room.MiddleOfTile(shortcut.StartTile)} : {position}");
                if (room.MiddleOfTile(shortcut.StartTile) == position)
                {
                    return ("shortcut", null, null, shortcut, null, position);
                }
            }

            foreach (var creature in creatures)
            {
                //Debug.Log($"LOOK HERE : FUNCTION CALL : CREATURE : {(creature as Creature).mainBodyChunk.pos} : {position}");
                if ((creature as Creature).mainBodyChunk.pos == position)
                {
                    return ("creature", (creature as Creature), null, new ShortcutData(), null, position);
                }
            }

            foreach (var item in items)
            {
                //Debug.Log($"LOOK HERE : FUNCTION CALL : ITEM : {(item as PlayerCarryableItem).firstChunk.pos} : {position}");
                if (item.firstChunk.pos == position)
                {
                    return ("item", null, item, new ShortcutData(), null, position);
                }
            }

            foreach (var _object in objects)
            {
                //Debug.Log($"LOOK HERE : FUNCTION CALL : ITEM : {(item as PlayerCarryableItem).firstChunk.pos} : {position}");
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

                //Debug.Log($"LOOK HERE : FUNCTIONCALL : {direction} : {searchPosition} : {position}");

                if ((angle <= 45f || angle >= 315f)) // within 45 degrees either way of the direction
                {
                    result.Add(searchPosition);
                }
            }

            return result;
        }

        public static (Vector2 up, Vector2 left, Vector2 right, Vector2 down) GetNearestPointsInAllDirections(Vector2 position, List<Vector2> searchPositions, Room room)
        {
            Vector2 up = Vector2.zero;
            Vector2 left = Vector2.zero;
            Vector2 right = Vector2.zero;
            Vector2 down = Vector2.zero;

            up = Utilities.FindNearest(position, Utilities.GetPointsInDirection(position, Vector2.up, searchPositions), room);
            left = Utilities.FindNearest(position, Utilities.GetPointsInDirection(position, Vector2.left, searchPositions), room);
            right = Utilities.FindNearest(position, Utilities.GetPointsInDirection(position, Vector2.right, searchPositions), room);
            down = Utilities.FindNearest(position, Utilities.GetPointsInDirection(position, Vector2.down, searchPositions), room);

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

                //Debug.Log($"LOOK HERE : FUNCTIONCALL : {direction} : {searchPosition} : {position}");

                if ((angle <= 45f || angle >= 315f)) // within 45 degrees either way of the direction
                {
                    result.Add(searchPosition);
                }
            }

            return result;
        }

        public static HighlightSprite FindNearestNode(HighlightSprite input, List<HighlightSprite> vectors, Room room, List<HighlightSprite> nodesToIgnore = null)
        {
            HighlightSprite nearest = null;
            float distance = Mathf.Infinity;
            nodesToIgnore ??= new List<HighlightSprite>(); // Set to empty list if null

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
            HighlightSprite up = Utilities.FindNearestNode(self, Utilities.GetNodesInDirection(self, Vector2.up, searchPositions), room);
            HighlightSprite left = Utilities.FindNearestNode(self, Utilities.GetNodesInDirection(self, Vector2.left, searchPositions), room);
            HighlightSprite right = Utilities.FindNearestNode(self, Utilities.GetNodesInDirection(self, Vector2.right, searchPositions), room);
            HighlightSprite down = Utilities.FindNearestNode(self, Utilities.GetNodesInDirection(self, Vector2.down, searchPositions), room);

            return (up, left, right, down);
        }

        public static float CalculateAngleBetweenVectorsForLineSegment(Vector2 vector1, Vector2 vector2)
        {
            // Calculate the average point between the two vectors
            float averageX = (vector1.x + vector2.x) / 2.0f;
            float averageY = (vector1.y + vector2.y) / 2.0f;

            // Calculate the angle of the line segment passing through the average point
            float angleRadians = (float)Math.Atan2(vector2.y - vector1.y, vector2.x - vector1.x);

            // Convert the angle from radians to degrees
            float angleDegrees = angleRadians * (180.0f / (float)Math.PI);

            return -angleDegrees;
        }

        public static List<DataStructures.Node> GetAllNodeInformation(Room room)
        {
            var (positions, creatures, items, objects) = GetEverything(room);

            // positions, karma level, protection level

            creatures.ForEach(creature => positions.Add((creature as Creature).mainBodyChunk.pos));
            items.ForEach(item => positions.Add(item.firstChunk.pos));
            objects.ForEach(_object => positions.Add(_object.pos));

            List<DataStructures.Node> NodeInfo = new List<DataStructures.Node>();

            // "element.destNode != -1" seems to be enough to distinguish from pipe entrances and everything else
            // foreach (var shortcut in room.shortcuts.Where(element => element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1).ToList())
            // foreach (var shortcut in room.shortcuts.Where(element => element.LeadingSomewhere).ToList()) // Filters out the wack a mole holes and spawnpoints, but also selects exit pipes with no connection set
            foreach (var shortcut in room.shortcuts.Where(element => (element.destNode != -1 && element.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[element.destNode] != -1) || element.shortCutType == ShortcutData.Type.Normal).ToList())
            {
                NodeInfo.Add(new DataStructures.Node(room.MiddleOfTile(shortcut.StartTile), 1, 0, null));
                    //room.MiddleOfTile(shortcut.StartTile), 1, 0, null));
            }

            foreach (var creature in creatures)
            {
                if (((creature as Creature) is MirosBird || (creature as Creature) is VultureGrub || (creature as Creature) is Vulture ||
                    (creature as Creature) is MoreSlugcats.Inspector || (creature as Creature) is Overseer) && (room.ViewedByAnyCamera((creature as Creature).mainBodyChunk.pos, 0f)) &&
                    (!(creature as Creature).dead))
                {
                    NodeInfo.Add(new DataStructures.Node((creature as Creature).mainBodyChunk.pos, 2, 0, creature));
                }
            }

            foreach (var item in items)
            {
                break;
            }

            foreach (var _object in objects)
            {
                break;
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
                if ((shortcut.destNode != -1 && shortcut.destNode < room.abstractRoom.connections.Length && room.abstractRoom.connections[shortcut.destNode] != -1) || shortcut.shortCutType == ShortcutData.Type.Normal)
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
            vectorsToIgnore ??= new List<Vector2>(); // Set to empty list if null

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
        public static AssetBundle LoadFromEmbeddedResource(string fullyQualifiedPath)
        {
            Debug.Log($"Loading embedded asset bundle: {fullyQualifiedPath}");
            using (MemoryStream mstr = new MemoryStream())
            {
                Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullyQualifiedPath);
                str.CopyTo(mstr);
                str.Flush();
                str.Close();
                Debug.Log("Bundle loaded into memory as byte[], processing with Unity...");
                AssetBundle bundle = AssetBundle.LoadFromMemory(mstr.ToArray());
                Debug.Log("Unity has successfully loaded this asset bundle from memory.");
                return bundle;
            }
        }

        public static FShader CreateFromAsset(AssetBundle bundle, string shortName)
        {
            Debug.Log($"Loading shader \"{shortName}\"...");
            Shader target = bundle.LoadAsset<Shader>($"assets/{shortName}.shader");
            Debug.Log($"Implementing shader \"{shortName}\" into Futile...");
            return FShader.CreateShader(shortName, target);
        }

        public static Vector4 Lerp(Vector4 start, Vector4 end, float t)
        {
            t = Mathf.Clamp(t, 0f, 1f); // Ensure t is within the range [0, 1]
            return start + (end - start) * t;
        }

        public static Vector4 ColourFade(Vector4 vector1, Vector4 vector2, float t)
        {
            return Utilities.Lerp(vector1, vector2, t);
        }
        public static List<int> GenerateNumbers(int N)
        {
            List<int> numbers = new List<int>();
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
                int k = UnityEngine.Random.Range(0, n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
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
            float normalizedValue = (oldValue - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;

            // Make sure the normalized value doesn't exceed the new range
            normalizedValue = Math.Max(newMin, Math.Min(newMax, normalizedValue));

            return normalizedValue;
        }

        public static UnityEngine.Color ColourLerp(UnityEngine.Color ColourA, UnityEngine.Color ColourB, float N)
        {
            float R = Mathf.Lerp(ColourA.r, ColourB.r, N);
            float G = Mathf.Lerp(ColourA.g, ColourB.g, N);
            float B = Mathf.Lerp(ColourA.b, ColourB.b, N);

            return new UnityEngine.Color(R, G, B);
        }

        public static float Timestamp()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            float unixTimestamp = (float)(now.ToUnixTimeSeconds() + (now.Millisecond / 1000.0));
            return unixTimestamp;
        }
    }
}