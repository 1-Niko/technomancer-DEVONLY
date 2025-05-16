using JetBrains.Annotations;
using TMPro;

namespace Slugpack;

public class Karma10Projection : BackgroundScene.BackgroundSceneElement
{
    private AboveCloudsView AboveCloudsScene { get { return this.scene as AboveCloudsView; } }

    private readonly int maxSegmentsPerOriginalEdge = 10;
    private readonly float targetSegmentModelLength = 0.05f;
    private readonly float lineWidth = 1f;

    private struct SegmentInfo
    {
        public Vector2 midPoint;
        public float rotation;
        public float length;
        public float depth; 
        public float visualDepth;
        
        public SegmentInfo(Vector2 midPoint, float rotation, float length, float depth, float visualDepth)
        {
            this.midPoint = midPoint;
            this.rotation = rotation;
            this.length = length;
            this.depth = depth;
            this.visualDepth = visualDepth;
        }
    }

    private List<SegmentInfo> segmentInfos;

    private Vector2[] pMesh;
    private float[] pDepth;

    private float extents = 0f;
    private float scale = 140f;
    private Vector3 rotation = Vector3.zero;

    private float TEMP_X = 0f;
    private float TEMP_Y = 0f;
    private float TEMP_Z = 0f;

    private int maxSplitIndex = 0;
    private int fadeSprites;

    public Karma10Projection(AboveCloudsView aboveCloudsScene, Vector2 pos, float depth, float atmosphericalDepthAdd) : base(aboveCloudsScene, pos, depth)
    {
        pMesh = new Vector2[mesh.Length];
        pDepth = new float[mesh.Length];

        segmentInfos = new List<SegmentInfo>(connections.Length * maxSegmentsPerOriginalEdge);

        for (int i = 0; i < mesh.Length; i++)
            extents = Mathf.Max(extents, mesh[i].magnitude);

        fadeSprites = (int)Mathf.Ceil(extents * scale);

        this.rotation = new Vector3(TEMP_X, TEMP_Y, TEMP_Z);
        UpdatePoints();
    }

    public override void Update(bool eu)
    {
        TEMP_X += (7f / 10f);
        TEMP_Y += (7f / 25f);
        TEMP_Z += (7f / 18f);
        this.rotation = new Vector3(TEMP_X, TEMP_Y, TEMP_Z);

        UpdatePoints();
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        maxSplitIndex = connections.Length * maxSegmentsPerOriginalEdge;
        sLeaser.sprites = new FSprite[maxSplitIndex + (fadeSprites * 2)];
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i] = new FSprite("pixel", true);
            sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["Background"];
            sLeaser.sprites[i].anchorY = 0.5f;
        }
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);
        segmentInfos.Clear();

        for (int i = 0; i < connections.Length; i++)
        {
            int vIndex1 = connections[i][0]; int vIndex2 = connections[i][1];

            Vector3 model_p1_3D = mesh[vIndex1]; Vector3 model_p2_3D = mesh[vIndex2];
            float originalEdgeModelLength = Vector3.Distance(model_p1_3D, model_p2_3D);

            int numSubSegmentsForThisEdge;
            if (targetSegmentModelLength <= 0.0001f) { numSubSegmentsForThisEdge = 1; }
            else { numSubSegmentsForThisEdge = Mathf.Max(1, Mathf.RoundToInt(originalEdgeModelLength / targetSegmentModelLength)); }
            numSubSegmentsForThisEdge = Mathf.Min(numSubSegmentsForThisEdge, maxSegmentsPerOriginalEdge);

            Vector2 p1_2D_interp = this.pMesh[vIndex1];
            Vector2 p2_2D_interp = this.pMesh[vIndex2];
            float depth1_interp = this.pDepth[vIndex1];
            float depth2_interp = this.pDepth[vIndex2];

            Vector2 edgeDirection_2D_interp = p2_2D_interp - p1_2D_interp;

            for (int j = 0; j < numSubSegmentsForThisEdge; j++)
            {
                float t0 = (float)j / numSubSegmentsForThisEdge; float t1 = (float)(j + 1) / numSubSegmentsForThisEdge;
                Vector2 segmentStart_2D = p1_2D_interp + edgeDirection_2D_interp * t0;
                Vector2 segmentEnd_2D = p1_2D_interp + edgeDirection_2D_interp * t1;
                Vector2 segmentMidPoint_2D = (segmentStart_2D + segmentEnd_2D) / 2f;
                float segmentScreenLength = Custom.Dist(segmentStart_2D, segmentEnd_2D);
                float segmentRotation = 0f;
                if (segmentScreenLength > 0.001f) { segmentRotation = Custom.AimFromOneVectorToAnother(segmentStart_2D, segmentEnd_2D); }

                float segmentDepthStart = Mathf.Lerp(depth1_interp, depth2_interp, t0);
                float segmentDepthEnd = Mathf.Lerp(depth1_interp, depth2_interp, t1);
                float averageSegmentDepth = (segmentDepthStart + segmentDepthEnd) / 2.0f;

                segmentInfos.Add(new SegmentInfo(segmentMidPoint_2D, segmentRotation, segmentScreenLength, averageSegmentDepth, averageSegmentDepth));
            }
        }

        segmentInfos.Sort((a, b) => a.depth.CompareTo(b.depth));
        for (int k = 0; k < segmentInfos.Count; k++)
        {
            FSprite currentSprite = sLeaser.sprites[k]; SegmentInfo info = segmentInfos[k];
            currentSprite.x = vector.x + info.midPoint.x; currentSprite.y = vector.y + info.midPoint.y;
            currentSprite.scaleX = lineWidth; currentSprite.scaleY = info.length; currentSprite.rotation = info.rotation;
            currentSprite.color = Color.Lerp(Color.black, Color.red, info.visualDepth);
            currentSprite.isVisible = info.length > 0.001f;
        }
        for (int k = segmentInfos.Count; k < maxSplitIndex; k++)
        {
            sLeaser.sprites[k].isVisible = false;
        }

        int firstFadeSpriteIndex = maxSplitIndex;
        int lastFadeSpriteIndex = sLeaser.sprites.Length - 1;
        int numFadeSprites = sLeaser.sprites.Length - firstFadeSpriteIndex;

        if (numFadeSprites > 0 && pMesh != null && pMesh.Length > 0)
        {
            float[] screenSpaceHeights = new float[numFadeSprites];
            float[] relativeHeightsForCheck = new float[numFadeSprites];

            float screenYTop = vector.y + fadeSprites;
            float screenYBottom = vector.y - fadeSprites;
            float screenYSpan = screenYTop - screenYBottom;

            float pixelStep = screenYSpan / numFadeSprites;

            for (int fadeIdx = 0; fadeIdx < numFadeSprites; fadeIdx++)
            {
                float rowTopY = screenYTop - (fadeIdx * pixelStep);
                screenSpaceHeights[fadeIdx] = rowTopY - (pixelStep * 0.5f);

                relativeHeightsForCheck[fadeIdx] = screenSpaceHeights[fadeIdx] - vector.y;
            }

            Vector2[][] formattedPMesh = FormatPMesh(pMesh);

            Dictionary<float, List<Vector2[]>> buckets = SegmentBucketSort(relativeHeightsForCheck, formattedPMesh);

            bool[] intersections = ProjectionIntersect(buckets, relativeHeightsForCheck);

            Vector2[] relativeBounds = MinMaxIntersect(buckets, relativeHeightsForCheck, formattedPMesh);

            for (int i = firstFadeSpriteIndex; i <= lastFadeSpriteIndex; i++)
            {
                int fadeSpriteIndex = i - firstFadeSpriteIndex;
                FSprite fadeSprite = sLeaser.sprites[i];

                fadeSprite.y = screenSpaceHeights[fadeSpriteIndex];

                fadeSprite.isVisible = false;// intersections[fadeSpriteIndex];

                if (fadeSprite.isVisible)
                {
                    float minX = relativeBounds[fadeSpriteIndex].x;
                    float maxX = relativeBounds[fadeSpriteIndex].y;
                    float width = Mathf.Abs(maxX - minX);

                    if (width > 0.001f)
                    {
                        fadeSprite.x = vector.x + (minX + maxX) / 2f;
                        fadeSprite.scaleX = width;
                        fadeSprite.scaleY = 1f;
                        fadeSprite.rotation = 0f;
                        fadeSprite.anchorY = 0.5f;
                        fadeSprite.anchorX = 0.5f;
                        fadeSprite.alpha = 0.5f;
                        fadeSprite.color = Color.white;
                    }
                    else
                    {
                        fadeSprite.isVisible = false;
                    }
                }
            }
        }
        else
        {
            for (int i = firstFadeSpriteIndex; i <= lastFadeSpriteIndex; i++)
            {
                if (i < sLeaser.sprites.Length)
                    sLeaser.sprites[i].isVisible = false;
            }
        }

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    // Updates the main pMesh and pDepth arrays based on the current 'this.rotation'
    private void UpdatePoints()
    {
        for (int i = 0; i < mesh.Length; i++)
        {
            Vector3 rotatedPoint = RotatePoint(mesh[i], this.rotation);
            Vector2 projectedPoint = ProjectPoint(rotatedPoint);
            this.pMesh[i] = projectedPoint * this.scale;

            if (extents > 0.0001f) { this.pDepth[i] = (rotatedPoint.z + extents) / (2f * extents); }
            else { this.pDepth[i] = 0.5f; }
        }
    }

    public bool IsIntersect(Vector2 p1, Vector2 p2, float yTarget)
    {
        float epsilon = 1e-6f;

        float y1 = p1.y;
        float y2 = p2.y;

        float minY = Math.Min(y1, y2);
        float maxY = Math.Max(y1, y2);

        if (yTarget < minY - epsilon || yTarget > maxY + epsilon)
            return false;

        if (Math.Abs(y1 - y2) < epsilon)
            return false;

        return true;
    }

    public float GetIntersect(Vector2 p1, Vector2 p2, float yTarget)
    {
        float x1 = p1.x;
        float y1 = p1.y;
        float x2 = p2.x;
        float y2 = p2.y;

        if (Math.Abs(x1 - x2) < 1e-6f)
            return x1;

        return x1 + (yTarget - y1) * (x2 - x1) / (y2 - y1);
    }

    public Dictionary<float, List<Vector2[]>> SegmentBucketSort(float[] heights, Vector2[][] segments)
    {
        Dictionary<float, List<Vector2[]>> buckets = new Dictionary<float, List<Vector2[]>>();

        for (int i = 0; i < heights.Length; i++)
        {
            List<Vector2[]> bucket = new List<Vector2[]>();
            for (int j = 0; j < segments.Length; j++)
            {
                if (IsIntersect(segments[j][0], segments[j][1], heights[i]))
                {
                    bucket.Add(segments[j]);
                }
            }
            buckets.Add(heights[i], bucket);
        }

        return buckets;
    }

    public bool[] ProjectionIntersect(Dictionary<float, List<Vector2[]>> buckets, float[] heights)
    {
        bool[] intersections = new bool[heights.Length];

        for (int i = 0; i < heights.Length; i++)
            intersections[i] = buckets[heights[i]].Any();

        return intersections;
    }

    public Vector2[] MinMaxIntersect(Dictionary<float, List<Vector2[]>> buckets, float[] heights, Vector2[][] segments)
    {
        bool[] intersections = ProjectionIntersect(buckets, heights);
        Vector2[] results = new Vector2[heights.Length];

        for (int i = 0; i < intersections.Length; i++)
        {
            Vector2 currentResult = new Vector2(float.MaxValue, float.MinValue);
            if (intersections[i])
            {
                for (int j = 0; j < buckets[heights[i]].Count; j++)
                {
                    float current = GetIntersect(buckets[heights[i]][j][0], buckets[heights[i]][j][1], heights[i]);

                    if (current < currentResult.x)
                        currentResult.x = current;
                    if (current > currentResult.y)
                        currentResult.y = current;
                }
            }
            results[i] = currentResult;
        }

        return results;
    }

    private Vector2[][] FormatPMesh(Vector2[] pMesh)
    {
        Vector2[][] segments = new Vector2[connections.Length][];
        for (int i = 0; i < connections.Length; i++)
        {
            segments[i] = new Vector2[2] { pMesh[connections[i][0]], pMesh[connections[i][1]] };
        }
        return segments;
    }

    private Vector2 ProjectPoint(Vector3 coordinate) { return new Vector2(coordinate.x, coordinate.y); }
    private Vector3 RotatePoint(Vector3 point, Vector3 angles)
    { Quaternion rotationQuat = Quaternion.Euler(angles.x, angles.y, angles.z); return rotationQuat * point; }

    /*
    private Vector3[] mesh = new Vector3[] {
        new Vector3(        0f,         1f,  0.05f), new Vector3(-0.258819f,  0.965926f,  0.05f),
        new Vector3(     -0.5f,  0.866025f,  0.05f), new Vector3(-0.707107f,  0.707107f,  0.05f),
        new Vector3(-0.866025f,       0.5f,  0.05f), new Vector3(-0.965926f,  0.258819f,  0.05f),
        new Vector3(       -1f,         0f,  0.05f), new Vector3(-0.965926f, -0.258819f,  0.05f),
        new Vector3(-0.866025f,      -0.5f,  0.05f), new Vector3(-0.707107f, -0.707107f,  0.05f),
        new Vector3(     -0.5f, -0.866025f,  0.05f), new Vector3(-0.258819f, -0.965926f,  0.05f),
        new Vector3(        0f,        -1f,  0.05f), new Vector3( 0.258819f, -0.965926f,  0.05f),
        new Vector3(      0.5f, -0.866025f,  0.05f), new Vector3( 0.707107f, -0.707107f,  0.05f),
        new Vector3( 0.866025f,      -0.5f,  0.05f), new Vector3( 0.965926f, -0.258819f,  0.05f),
        new Vector3(        1f,         0f,  0.05f), new Vector3( 0.965926f,  0.258819f,  0.05f),
        new Vector3( 0.866025f,       0.5f,  0.05f), new Vector3( 0.707107f,  0.707107f,  0.05f),
        new Vector3(      0.5f,  0.866025f,  0.05f), new Vector3( 0.258819f,  0.965926f,  0.05f),

        new Vector3(        0f,         1f, -0.05f), new Vector3(-0.258819f,  0.965926f, -0.05f),
        new Vector3(     -0.5f,  0.866025f, -0.05f), new Vector3(-0.707107f,  0.707107f, -0.05f),
        new Vector3(-0.866025f,       0.5f, -0.05f), new Vector3(-0.965926f,  0.258819f, -0.05f),
        new Vector3(       -1f,         0f, -0.05f), new Vector3(-0.965926f, -0.258819f, -0.05f),
        new Vector3(-0.866025f,      -0.5f, -0.05f), new Vector3(-0.707107f, -0.707107f, -0.05f),
        new Vector3(     -0.5f, -0.866025f, -0.05f), new Vector3(-0.258819f, -0.965926f, -0.05f),
        new Vector3(        0f,        -1f, -0.05f), new Vector3( 0.258819f, -0.965926f, -0.05f),
        new Vector3(      0.5f, -0.866025f, -0.05f), new Vector3( 0.707107f, -0.707107f, -0.05f),
        new Vector3( 0.866025f,      -0.5f, -0.05f), new Vector3( 0.965926f, -0.258819f, -0.05f),
        new Vector3(        1f,         0f, -0.05f), new Vector3( 0.965926f,  0.258819f, -0.05f),
        new Vector3( 0.866025f,       0.5f, -0.05f), new Vector3( 0.707107f,  0.707107f, -0.05f),
        new Vector3(      0.5f,  0.866025f, -0.05f), new Vector3( 0.258819f,  0.965926f, -0.05f),

        new Vector3(-0.596728f,  0.666834f,  0.05f), new Vector3(    -0.45f,  0.779423f,  0.05f),
        new Vector3(-0.232938f,  0.869333f,  0.05f), new Vector3(        0f,       0.9f,  0.05f),
        new Vector3( 0.232937f,  0.869333f,  0.05f), new Vector3(     0.45f,  0.779423f,  0.05f),
        new Vector3( 0.596728f,  0.666834f,  0.05f), new Vector3(-0.000302f,  0.070408f,  0.05f),

        new Vector3(-0.596728f, -0.666834f,  0.05f), new Vector3(    -0.45f, -0.779423f,  0.05f),
        new Vector3(-0.232938f, -0.869333f,  0.05f), new Vector3(        0f,      -0.9f,  0.05f),
        new Vector3( 0.232937f, -0.869333f,  0.05f), new Vector3(     0.45f, -0.779423f,  0.05f),
        new Vector3( 0.596728f, -0.666834f,  0.05f), new Vector3(-0.000302f, -0.070408f,  0.05f),

        new Vector3(-0.596728f,  0.666834f, -0.05f), new Vector3(    -0.45f,  0.779423f, -0.05f),
        new Vector3(-0.232938f,  0.869333f, -0.05f), new Vector3(        0f,       0.9f, -0.05f),
        new Vector3( 0.232937f,  0.869333f, -0.05f), new Vector3(     0.45f,  0.779423f, -0.05f),
        new Vector3( 0.596728f,  0.666834f, -0.05f), new Vector3(-0.000302f,  0.070408f, -0.05f),

        new Vector3(-0.596728f, -0.666834f, -0.05f), new Vector3(    -0.45f, -0.779423f, -0.05f),
        new Vector3(-0.232938f, -0.869333f, -0.05f), new Vector3(        0f,      -0.9f, -0.05f),
        new Vector3( 0.232937f, -0.869333f, -0.05f), new Vector3(     0.45f, -0.779423f, -0.05f),
        new Vector3( 0.596728f, -0.666834f, -0.05f), new Vector3(-0.000302f, -0.070408f, -0.05f),

        new Vector3(-0.666834f,  0.596728f,  0.05f), new Vector3(-0.779423f,      0.45f,  0.05f),
        new Vector3(-0.869333f,  0.232937f,  0.05f), new Vector3(     -0.9f,         0f,  0.05f),
        new Vector3(-0.869333f, -0.232938f,  0.05f), new Vector3(-0.779423f,     -0.45f,  0.05f),
        new Vector3(-0.666834f, -0.596728f,  0.05f), new Vector3(-0.070408f,  0.000302f,  0.05f),

        new Vector3( 0.666834f,  0.596728f,  0.05f), new Vector3( 0.779423f,      0.45f,  0.05f),
        new Vector3( 0.869333f,  0.232937f,  0.05f), new Vector3(      0.9f,         0f,  0.05f),
        new Vector3( 0.869333f, -0.232938f,  0.05f), new Vector3( 0.779423f,     -0.45f,  0.05f),
        new Vector3( 0.666834f, -0.596728f,  0.05f), new Vector3( 0.070408f,  0.000302f,  0.05f),

        new Vector3(-0.666834f,  0.596728f, -0.05f), new Vector3(-0.779423f,      0.45f, -0.05f),
        new Vector3(-0.869333f,  0.232937f, -0.05f), new Vector3(     -0.9f,         0f, -0.05f),
        new Vector3(-0.869333f, -0.232938f, -0.05f), new Vector3(-0.779423f,     -0.45f, -0.05f),
        new Vector3(-0.666834f, -0.596728f, -0.05f), new Vector3(-0.070408f,  0.000302f, -0.05f),

        new Vector3( 0.666834f,  0.596728f, -0.05f), new Vector3( 0.779423f,      0.45f, -0.05f),
        new Vector3( 0.869333f,  0.232937f, -0.05f), new Vector3(      0.9f,         0f, -0.05f),
        new Vector3( 0.869333f, -0.232938f, -0.05f), new Vector3( 0.779423f,     -0.45f, -0.05f),
        new Vector3( 0.666834f, -0.596728f, -0.05f), new Vector3( 0.070408f,  0.000302f, -0.05f),
    };
    private int[][] connections = new int[][] {
        new int[] { 0,  1}, new int[] { 1,  2}, new int[] { 2, 3 }, new int[] { 3, 4 },
        new int[] { 4, 5 }, new int[] { 5, 6 }, new int[] { 6, 7 }, new int[] { 7, 8 },
        new int[] { 8, 9 }, new int[] { 9, 10 }, new int[] { 10, 11 }, new int[] { 11, 12 },
        new int[] { 12, 13 }, new int[] { 13, 14 }, new int[] { 14, 15 }, new int[] { 15, 16 },
        new int[] { 16, 17 }, new int[] { 17, 18 }, new int[] { 18, 19 }, new int[] { 19, 20 },
        new int[] { 20, 21 }, new int[] { 21, 22 }, new int[] { 22, 23 }, new int[] { 23, 0 },

        new int[] { 24, 25 }, new int[] { 25, 26 }, new int[] { 26, 27 }, new int[] { 27, 28 },
        new int[] { 28, 29 }, new int[] { 29, 30 }, new int[] { 30, 31 }, new int[] { 31, 32 },
        new int[] { 32, 33 }, new int[] { 33, 34 }, new int[] { 34, 35 }, new int[] { 35, 36 },
        new int[] { 36, 37 }, new int[] { 37, 38 }, new int[] { 38, 39 }, new int[] { 39, 40 },
        new int[] { 40, 41 }, new int[] { 41, 42 }, new int[] { 42, 43 }, new int[] { 43, 44 },
        new int[] { 44, 45 }, new int[] { 45, 46 }, new int[] { 46, 47 }, new int[] { 47, 24 },

        new int[] { 48, 49 }, new int[] { 49, 50 }, new int[] { 50, 51 }, new int[] { 51, 52 },
        new int[] { 52, 53 }, new int[] { 53, 54 }, new int[] { 54, 55 }, new int[] { 55, 48 },

        new int[] { 56, 57 }, new int[] { 57, 58 }, new int[] { 58, 59 }, new int[] { 59, 60 },
        new int[] { 60, 61 }, new int[] { 61, 62 }, new int[] { 62, 63 }, new int[] { 63, 56 },

        new int[] { 64, 65 }, new int[] { 65, 66 }, new int[] { 66, 67 }, new int[] { 67, 68 },
        new int[] { 68, 69 }, new int[] { 69, 70 }, new int[] { 70, 71 }, new int[] { 71, 64 },

        new int[] { 72, 73 }, new int[] { 73, 74 }, new int[] { 74, 75 }, new int[] { 75, 76 },
        new int[] { 76, 77 }, new int[] { 77, 78 }, new int[] { 78, 79 }, new int[] { 79, 72 },

        new int[] { 80, 81 }, new int[] { 81, 82 }, new int[] { 82, 83 }, new int[] { 83, 84 },
        new int[] { 84, 85 }, new int[] { 85, 86 }, new int[] { 86, 87 }, new int[] { 87, 80 },

        new int[] { 88, 89 }, new int[] { 89, 90 }, new int[] { 90, 91 }, new int[] { 91, 92 },
        new int[] { 92, 93 }, new int[] { 93, 94 }, new int[] { 94, 95 }, new int[] { 95, 88 },

        new int[] { 96, 97 }, new int[] { 97, 98 }, new int[] { 98, 99 }, new int[] { 99, 100 },
        new int[] { 100, 101 }, new int[] { 101, 102 }, new int[] { 102, 103 }, new int[] { 103, 96 },

        new int[] { 104, 105 }, new int[] { 105, 106 }, new int[] { 106, 107 }, new int[] { 107, 108 },
        new int[] { 108, 109 }, new int[] { 109, 110 }, new int[] { 110, 111 }, new int[] { 111, 104 },
    };
    */
    private Vector3[] mesh = new Vector3[] {
        new Vector3(-0.010624f, 0.280556f, 0f),
        new Vector3(-0.009825f, 0.258718f, 0f),
        new Vector3(0.01148f, 0.259517f, 0f),
        new Vector3(0.010948f, 0.282154f, 0f),

        new Vector3(0.27098f, 0.268199f, 0f),
        new Vector3(0.293617f, 0.268998f, 0f),
        new Vector3(0.294949f, 0.290037f, 0f),
        new Vector3(0.27098f, 0.291635f, 0f),

        new Vector3(0.139153f, 0.26021f, 0f),
        new Vector3(0.160991f, 0.259677f, 0f),

        new Vector3(0.309413f, 0.614668f, 0f),

        new Vector3(0.267859f, 0.668095f, 0f),
        new Vector3(0.172878f, 0.67601f, 0f),
        new Vector3(0.030407f, 0.662159f, 0f),
        new Vector3(-0.011147f, 0.632477f, 0f),
        new Vector3(-0.076446f, 0.7235f, 0f),
        new Vector3(-0.16549f, 0.812544f, 0f),
        new Vector3(-0.254534f, 0.854098f, 0f),
        new Vector3(-0.321812f, 0.840247f, 0f),
        new Vector3(-0.347536f, 0.759118f, 0f),
        new Vector3(-0.307961f, 0.664137f, 0f),
        new Vector3(-0.226831f, 0.563221f, 0f),
        new Vector3(-0.203086f, 0.507815f, 0f),
        new Vector3(-0.214959f, 0.193192f, 0f),
        new Vector3(-0.26047f, -0.153091f, 0f),
        new Vector3(-0.298067f, -0.319306f, 0f),
        new Vector3(-0.367324f, -0.396478f, 0f),
        new Vector3(-0.507815f, -0.461777f, 0f),
        new Vector3(-0.721522f, -0.519161f, 0f),
        new Vector3(-0.901589f, -0.590397f, 0f),
        new Vector3(-0.953037f, -0.651738f, 0f),
        new Vector3(-0.939185f, -0.750676f, 0f),
        new Vector3(-0.858056f, -0.835763f, 0f),
        new Vector3(-0.636435f, -0.891168f, 0f),
        new Vector3(-0.329727f, -0.901062f, 0f),
        new Vector3(0.038323f, -0.893147f, 0f),
        new Vector3(0.277753f, -0.855551f, 0f),
        new Vector3(0.455841f, -0.784315f, 0f),
        new Vector3(0.558737f, -0.69725f, 0f),
        new Vector3(0.635908f, -0.693292f, 0f),
        new Vector3(0.673505f, -0.768485f, 0f),
        new Vector3(0.677462f, -0.831805f, 0f),
        new Vector3(0.912935f, -0.837742f, 0f),
        new Vector3(0.954489f, -0.796188f, 0f),
        new Vector3(0.930744f, -0.752655f, 0f),
        new Vector3(0.847636f, -0.738804f, 0f),
        new Vector3(0.815975f, -0.580503f, 0f),
        new Vector3(0.766506f, -0.467713f, 0f),
        new Vector3(0.669547f, -0.412308f, 0f),
        new Vector3(0.598312f, -0.406372f, 0f),
        new Vector3(0.58446f, -0.180793f, 0f),
        new Vector3(0.542906f, 0.028955f, 0f),
        new Vector3(0.538949f, 0.323791f, 0f),
        new Vector3(0.527076f, 0.592902f, 0f),
        new Vector3(0.481565f, 0.743288f, 0f),
        new Vector3(0.420223f, 0.830353f, 0f),
        new Vector3(0.333158f, 0.83629f, 0f),
        new Vector3(0.279731f, 0.745267f, 0f),
    };
    private int[][] connections = new int[][] {
        new int[] {0, 1},
        new int[] {1, 2},
        new int[] {2, 3},
        new int[] {3, 0},

       new int[] {4, 5},
        new int[] {5, 6},
        new int[] {6, 7},
        new int[] {7, 4},

        new int[] {8, 9},

        new int[] {10, 11},

        new int[] {11, 12},
        new int[] {12, 13},
        new int[] {13, 14},
        new int[] {14, 15},
        new int[] {15, 16},
        new int[] {16, 17},
        new int[] {17, 18},
        new int[] {18, 19},
        new int[] {19, 20},
        new int[] {20, 21},
        new int[] {21, 22},
        new int[] {22, 23},
        new int[] {23, 24},
        new int[] {24, 25},
        new int[] {25, 26},
        new int[] {26, 27},
        new int[] {27, 28},
        new int[] {28, 29},
        new int[] {29, 30},
        new int[] {30, 31},
        new int[] {31, 32},
        new int[] {32, 33},
        new int[] {33, 34},
        new int[] {34, 35},
        new int[] {35, 36},
        new int[] {36, 37},
        new int[] {37, 38},
        new int[] {38, 39},
        new int[] {39, 40},
        new int[] {40, 41},
        new int[] {41, 42},
        new int[] {42, 43},
        new int[] {43, 44},
        new int[] {44, 45},
        new int[] {45, 46},
        new int[] {46, 47},
        new int[] {47, 48},
        new int[] {48, 49},
        new int[] {49, 50},
        new int[] {50, 51},
        new int[] {51, 52},
        new int[] {52, 53},
        new int[] {53, 54},
        new int[] {54, 55},
        new int[] {55, 56},
        new int[] {56, 57},
        new int[] {57, 11},
    };
}