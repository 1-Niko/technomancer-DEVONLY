using Steamworks;
using static UnityEngine.UI.Image;

namespace Slugpack;

internal static class PlayerGraphicsHooks
{
    public class HairPhysics
    {
        public Vector2 basePosition;
        public float targetBaseAngle;
        public readonly float[] segmentLengths;
        public readonly float[] segmentAngles;
        public readonly float[] segmentAngVels;
        public readonly Vector2[] jointPositions;

        public readonly Vector2[] renderPath;
        private readonly Vector2[] controlPath;
        private const int numPathPoints = 12;
        private const int maxPathIndex = numPathPoints - 1;
        private const int pathSmoothIters = 5;
        private readonly float maxPathSmoothAdjust;

        private readonly Vector2[][] segmentBounds;
        private readonly Vector2[][] jointConnectors;
        private readonly float segOutlineOffset;

        private const int numSegments = 4;
        //private static readonly float[] baseStiffnessFactors = { 100.0f, 2.3f, 2.1f, 1.0f };
        private static readonly float[] baseStiffnessFactors = { 100.0f, 2.3f, 1.5f, 0.2f };
        private const float baseInertia = 1.0f, stiffnessScale = 15.0f;
        private const float gravityEffect = 0.01f, flowEffect = 0.02f;
        private const float dampingFactor = 1.0f, angleConstraintStrength = 50.0f;
        private const float inertiaFromLengthFactor = 5e-5f, limitHitDamping = 0.2f, minInertia = 0.01f;

        private const float defaultOutlineOffset = 20.0f;
        private const float defMaxPathSmoothAdj = 5.0f;

        private readonly System.Random randomGen = new System.Random(); // Changed
        private readonly float[] segmentStiffness;

        public bool JointA { get; private set; }
        public bool JointB { get; private set; }

        public HairPhysics(Vector2 initialBasePos, float initialBaseAngleDeg, float[] initialSegmentLengths)
        {
            if (initialSegmentLengths == null || initialSegmentLengths.Length != numSegments)
                throw new ArgumentException($"initialSegmentLengths must be an array of size {numSegments}");

            basePosition = initialBasePos;
            segOutlineOffset = defaultOutlineOffset;
            maxPathSmoothAdjust = defMaxPathSmoothAdj;

            segmentLengths = new float[numSegments]; initialSegmentLengths.CopyTo(segmentLengths, 0);
            segmentAngles = new float[numSegments];
            segmentAngVels = new float[numSegments];
            jointPositions = new Vector2[numSegments + 1];

            segmentBounds = new Vector2[numSegments][];
            for (int i = 0; i < numSegments; ++i) segmentBounds[i] = new Vector2[4];

            jointConnectors = new Vector2[numSegments - 1][];
            for (int i = 0; i < numSegments - 1; ++i) jointConnectors[i] = new Vector2[2];

            renderPath = new Vector2[numPathPoints];
            controlPath = new Vector2[numPathPoints];

            segmentStiffness = new float[numSegments];
            for (int i = 0; i < numSegments; i++)
                segmentStiffness[i] = baseStiffnessFactors[i] * stiffnessScale;

            resetChain(initialBaseAngleDeg);
        }

        public void setBaseAngleDeg(float angleDeg) => targetBaseAngle = angleDeg * (Mathf.PI / 180.0f);

        public void resetChain(float baseAngleDeg)
        {
            targetBaseAngle = baseAngleDeg * (Mathf.PI / 180.0f);
            segmentAngles[0] = targetBaseAngle;
            for (int i = 0; i < numSegments; ++i)
            {
                segmentAngVels[i] = 0.0f;
                segmentAngles[i] = (i > 0) ? normalizeAngle(segmentAngles[i - 1]) : normalizeAngle(segmentAngles[i]);
            }
            updateGeometry();
        }

        private float normalizeAngle(float angleRad) => (angleRad + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;

        private float calculateDamping(float speed)
        {
            const float minSpeed = 10.0f, maxSpeed = 30.0f;
            const float minDamp = 1.0f, midDamp = 12.0f, maxDamp = 60.0f;

            float speedRange = maxSpeed - minSpeed;
            float normalizedSpeed = (speedRange < 1e-3f) ? 0.5f : (speed - minSpeed) / speedRange;

            float clampedPercent = Math.Max(0.0f, Math.Min(100.0f, normalizedSpeed * 100.0f));

            if (clampedPercent <= 50f)
                return minDamp + (clampedPercent / 50f) * (midDamp - minDamp);
            else
                return midDamp + ((clampedPercent - 50f) / 50f) * (maxDamp - midDamp);
        }

        private static Vector2? getLineIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End, float epsilon = 1e-9f)
        {
            float p1x = line1Start.x, p1y = line1Start.y; float p2x = line1End.x, p2y = line1End.y; // Changed .X .Y to .x .y
            float p3x = line2Start.x, p3y = line2Start.y; float p4x = line2End.x, p4y = line2End.y; // Changed .X .Y to .x .y
            float denominator = (p1x - p2x) * (p3y - p4y) - (p1y - p2y) * (p3x - p4x);
            if (Mathf.Abs(denominator) < epsilon) return null;
            float tNumerator = (p1x - p3x) * (p3y - p4y) - (p1y - p3y) * (p3x - p4x);
            float tParam = tNumerator / denominator;

            return new Vector2(p1x + tParam * (p2x - p1x), p1y + tParam * (p2y - p1y));
        }

        private void updateJointPositions()
        {
            jointPositions[0] = basePosition;
            Vector2 currentPos = basePosition;
            for (int i = 0; i < numSegments; i++)
            {
                // Changed Vector2.FromPolar to new Vector2(...)
                currentPos += new Vector2(segmentLengths[i] * Mathf.Cos(segmentAngles[i]), segmentLengths[i] * Mathf.Sin(segmentAngles[i]));
                jointPositions[i + 1] = currentPos;
            }
        }

        private void updateSegmentBounds()
        {
            for (int i = 0; i < numSegments; i++)
            {
                Vector2 segStartPos = jointPositions[i];
                Vector2 segEndPos = jointPositions[i + 1];
                float currentSegAngle = (i == numSegments - 1 && numSegments > 1) ?
                                            segmentAngles[numSegments - 2] :
                                            segmentAngles[i];

                float leftNormalAngle = currentSegAngle - Mathf.PI / 2.0f;
                float rightNormalAngle = currentSegAngle + Mathf.PI / 2.0f;

                // Changed Vector2.FromPolar to new Vector2(...)
                Vector2 leftOffsetVec = new Vector2(segOutlineOffset * Mathf.Cos(leftNormalAngle), segOutlineOffset * Mathf.Sin(leftNormalAngle));
                segmentBounds[i][0] = segStartPos + leftOffsetVec;
                segmentBounds[i][2] = segEndPos + leftOffsetVec;

                // Changed Vector2.FromPolar to new Vector2(...)
                Vector2 rightOffsetVec = new Vector2(segOutlineOffset * Mathf.Cos(rightNormalAngle), segOutlineOffset * Mathf.Sin(rightNormalAngle));
                segmentBounds[i][1] = segStartPos + rightOffsetVec;
                segmentBounds[i][3] = segEndPos + rightOffsetVec;
            }
        }

        private void updateJointConnectors()
        {
            float totalChainLen = 0;
            for (int i = 0; i < segmentLengths.Length; ++i) totalChainLen += segmentLengths[i];
            float maxIntersectDist = (segmentLengths.Length > 0) ? (5 * totalChainLen / segmentLengths.Length) : 100.0f;

            for (int jointIdx = 0; jointIdx < numSegments - 1; jointIdx++)
            {
                Vector2 prevSegLStart = segmentBounds[jointIdx][0], prevSegLEnd = segmentBounds[jointIdx][2];
                Vector2 prevSegRStart = segmentBounds[jointIdx][1], prevSegREnd = segmentBounds[jointIdx][3];
                Vector2 currSegLStart = segmentBounds[jointIdx + 1][0], currSegLEnd = segmentBounds[jointIdx + 1][2];
                Vector2 currSegRStart = segmentBounds[jointIdx + 1][1], currSegREnd = segmentBounds[jointIdx + 1][3];
                Vector2 actualJointPos = jointPositions[jointIdx + 1];

                Vector2? leftSideIntersect = getLineIntersection(prevSegLStart, prevSegLEnd, currSegLStart, currSegLEnd);
                // Changed .DistanceTo to Vector2.Distance
                if (leftSideIntersect == null || Vector2.Distance(actualJointPos, leftSideIntersect.Value) > maxIntersectDist)
                    jointConnectors[jointIdx][0] = (prevSegLEnd + currSegLStart) / 2.0f;
                else
                    jointConnectors[jointIdx][0] = leftSideIntersect.Value;

                Vector2? rightSideIntersect = getLineIntersection(prevSegRStart, prevSegREnd, currSegRStart, currSegREnd);
                // Changed .DistanceTo to Vector2.Distance
                if (rightSideIntersect == null || Vector2.Distance(actualJointPos, rightSideIntersect.Value) > maxIntersectDist)
                    jointConnectors[jointIdx][1] = (prevSegREnd + currSegRStart) / 2.0f;
                else
                    jointConnectors[jointIdx][1] = rightSideIntersect.Value;
            }
        }

        private void generateControlPath()
        {

            Vector2 s0LStart = segmentBounds[0][0], s0LEnd = segmentBounds[0][2];
            Vector2 s0RStart = segmentBounds[0][1], s0REnd = segmentBounds[0][3];
            Vector2 s1LStart = segmentBounds[1][0], s1LEnd = segmentBounds[1][2];
            Vector2 s1RStart = segmentBounds[1][1], s1REnd = segmentBounds[1][3];
            Vector2 s2LStart = segmentBounds[2][0], s2LEnd = segmentBounds[2][2];
            Vector2 s2RStart = segmentBounds[2][1];
            Vector2 s3LEnd = segmentBounds[3][2], s3REnd = segmentBounds[3][3];

            Vector2 j0ConnL = jointConnectors[0][0], j0ConnR = jointConnectors[0][1];
            Vector2 j1ConnL = jointConnectors[1][0], j1ConnR = jointConnectors[1][1];
            Vector2 j2ConnL = jointConnectors[2][0], j2ConnR = jointConnectors[2][1];

            Vector2 chainBasePos = basePosition;
            Vector2 firstJointPos = jointPositions[1];

            // Changed .DistanceTo to Vector2.Distance
            float distBaseToS0LEnd = Vector2.Distance(chainBasePos, s0LEnd);
            float distBaseToS1LStart = Vector2.Distance(chainBasePos, s1LStart);
            float distJoint1S1LEnd = Vector2.Distance(firstJointPos, s1LEnd);
            float distJoint1S2LStart = Vector2.Distance(firstJointPos, s2LStart);

            for (int pathPointIdx = 0; pathPointIdx < numPathPoints; pathPointIdx++)
            {
                Vector2 point = controlPath[pathPointIdx];

                if (pathPointIdx == 0) point = s0LStart;
                else if (pathPointIdx == maxPathIndex) point = s0RStart;
                else
                {
                    JointA = distBaseToS0LEnd > distBaseToS1LStart;
                    JointB = distJoint1S1LEnd > distJoint1S2LStart;

                    if (JointA && JointB)
                    {
                        point = pathPointIdx switch { 1 => j0ConnL, 2 => j1ConnL, 3 => j2ConnL, 4 => s3LEnd, 5 => s3REnd, 6 => j2ConnR, 7 => s2RStart, 8 => s1REnd, 9 => s1RStart, 10 => s0REnd, _ => point };
                    }
                    else if (!JointA && JointB)
                    {
                        point = pathPointIdx switch { 1 => s0LEnd, 2 => s1LStart, 3 => j1ConnL, 4 => j2ConnL, 5 => s3LEnd, 6 => s3REnd, 7 => j2ConnR, 8 => s2RStart, 9 => s1REnd, 10 => j0ConnR, _ => point };
                    }
                    else if (JointA && !JointB)
                    {
                        point = pathPointIdx switch { 1 => j0ConnL, 2 => s1LEnd, 3 => s2LStart, 4 => j2ConnL, 5 => s3LEnd, 6 => s3REnd, 7 => j2ConnR, 8 => j1ConnR, 9 => s1RStart, 10 => s0REnd, _ => point };
                    }
                    else
                    {
                        point = pathPointIdx switch { 1 => s0LEnd, 2 => s1LStart, 3 => s1LEnd, 4 => s2LStart, 5 => j2ConnL, 6 => s3LEnd, 7 => s3REnd, 8 => j2ConnR, 9 => j1ConnR, 10 => j0ConnR, _ => point };
                    }
                }
                controlPath[pathPointIdx] = point;
            }
        }

        private void smoothPath()
        {
            if (controlPath.Length == 0) return;

            for (int idx = 0; idx < numPathPoints; idx++) renderPath[idx] = controlPath[idx];

            float currentPathLen = 0;
            for (int idx = 0; idx < maxPathIndex; idx++)
                currentPathLen += Vector2.Distance(controlPath[idx + 1], controlPath[idx]); // Changed .DistanceTo

            if (currentPathLen < 1e-6f) return;
            float avgSegLen = currentPathLen / maxPathIndex;
            if (avgSegLen < 1e-6f) avgSegLen = 1.0f;

            for (int iter = 0; iter < pathSmoothIters; iter++)
            {
                renderPath[0] = controlPath[0];
                renderPath[maxPathIndex] = controlPath[maxPathIndex];

                for (int idx = maxPathIndex - 1; idx >= 1; idx--)
                {
                    Vector2 fixedPoint = renderPath[idx + 1];
                    Vector2 pointToAdjust = renderPath[idx];
                    Vector2 dirToFixed = pointToAdjust - fixedPoint;
                    float distToFixed = dirToFixed.magnitude; // Changed .Length() to .magnitude

                    if (distToFixed > 1e-6f) renderPath[idx] = fixedPoint + (dirToFixed / distToFixed) * avgSegLen; // Changed .Normalized()
                    else
                    {
                        Vector2 originalDir = controlPath[idx] - controlPath[idx + 1];
                        // Changed .LengthSquared() to .magnitude * .magnitude; Changed .Normalized()
                        renderPath[idx] = ((originalDir.magnitude * originalDir.magnitude) > 1e-6f) ? fixedPoint + (originalDir / originalDir.magnitude) * avgSegLen : fixedPoint;
                    }

                    // Changed .DistanceTo to Vector2.Distance
                    float displacement = Vector2.Distance(renderPath[idx], controlPath[idx]);
                    if (displacement > maxPathSmoothAdjust)
                    {
                        Vector2 correctionVec = renderPath[idx] - controlPath[idx];
                        // Changed .LengthSquared() to .magnitude * .magnitude; Changed .Normalized()
                        if ((correctionVec.magnitude * correctionVec.magnitude) > 1e-9f)
                            renderPath[idx] = controlPath[idx] + (correctionVec / correctionVec.magnitude) * maxPathSmoothAdjust;
                    }
                }

                for (int idx = 0; idx < maxPathIndex - 1; idx++)
                {
                    int currIdx = idx + 1;
                    Vector2 fixedPoint = renderPath[idx];
                    Vector2 pointToAdjust = renderPath[currIdx];

                    Vector2 dirToFixed = pointToAdjust - fixedPoint;
                    float distToFixed = dirToFixed.magnitude; // Changed .Length() to .magnitude

                    if (distToFixed > 1e-6f) renderPath[currIdx] = fixedPoint + (dirToFixed / distToFixed) * avgSegLen; // Changed .Normalized()
                    else
                    {
                        Vector2 originalDir = controlPath[currIdx] - controlPath[idx];
                        // Changed .LengthSquared() to .magnitude * .magnitude; Changed .Normalized()
                        renderPath[currIdx] = ((originalDir.magnitude * originalDir.magnitude) > 1e-6f) ? fixedPoint + (originalDir / originalDir.magnitude) * avgSegLen : fixedPoint;
                    }

                    // Changed .DistanceTo to Vector2.Distance
                    float displacement = Vector2.Distance(renderPath[currIdx], controlPath[currIdx]);
                    if (displacement > maxPathSmoothAdjust)
                    {
                        Vector2 correctionVec = renderPath[currIdx] - controlPath[currIdx];
                        // Changed .LengthSquared() to .magnitude * .magnitude; Changed .Normalized()
                        if ((correctionVec.magnitude * correctionVec.magnitude) > 1e-9f)
                            renderPath[currIdx] = controlPath[currIdx] + (correctionVec / correctionVec.magnitude) * maxPathSmoothAdjust;
                    }
                }
            }
        }

        private void updateGeometry()
        {
            updateJointPositions();
            updateSegmentBounds();
            updateJointConnectors();
            generateControlPath();
            smoothPath();
        }

        public void updateDynamics(float deltaTime, Vector2 flowVec, float gravityMag,
                                       float bodySpeed, float baseAngleInfluence, Vector2 externalForce)
        {
            float damping = calculateDamping(bodySpeed);
            float flowMag = flowVec.magnitude; // Assuming flowVec is Vector2, .Length() -> .magnitude
            float flowAngle = Mathf.Atan2(flowVec.y, flowVec.x); // Changed .Y .X to .y .x

            float currentSegAngle_S0 = segmentAngles[0];
            float targetAngle_S0 = targetBaseAngle;
            float effStiffness_S0 = (baseInertia + segmentStiffness[0]) * 0.05f;
            float torque_S0 = effStiffness_S0 * normalizeAngle(targetAngle_S0 - currentSegAngle_S0);
            torque_S0 += gravityMag * segmentLengths[0] * Mathf.Cos(currentSegAngle_S0) * gravityEffect;

            float randFlowMag_S0 = flowMag * (1.0f + (float)(randomGen.NextDouble() * 0.4 - 0.2)); // Changed
            float randFlowAngle_S0 = flowAngle + (float)(randomGen.NextDouble() * (Mathf.PI / 6.0) - (Mathf.PI / 12.0)); // Changed
            torque_S0 += 0.5f * segmentLengths[0] * randFlowMag_S0 * Mathf.Sin(normalizeAngle(randFlowAngle_S0 - currentSegAngle_S0));

            torque_S0 += (segmentLengths[0] / 2f) * (-externalForce.y * Mathf.Cos(currentSegAngle_S0) + externalForce.x * Mathf.Sin(currentSegAngle_S0)) * flowEffect;

            float alignmentFactor_S0 = Math.Max(0f, 1f - Math.Abs(normalizeAngle(currentSegAngle_S0 - targetAngle_S0)) / Mathf.PI);
            torque_S0 -= damping * (1f + dampingFactor * alignmentFactor_S0) * segmentAngVels[0];

            float inertia_S0 = Math.Max(minInertia, inertiaFromLengthFactor * Mathf.Pow(segmentLengths[0], 3));
            segmentAngVels[0] += (inertia_S0 > 1e-9f ? (torque_S0 / inertia_S0) * deltaTime : 0f);
            segmentAngles[0] = normalizeAngle(segmentAngles[0] + segmentAngVels[0] * deltaTime);

            for (int segIdx = 1; segIdx < numSegments; segIdx++)
            {
                float prevSegAngle = segmentAngles[segIdx - 1];

                float currentSegAngle = segmentAngles[segIdx];
                float effStiffness = baseInertia + segmentStiffness[segIdx];

                float targetAngleForSeg = targetBaseAngle;
                float blendedTargetAngle = normalizeAngle((1f - baseAngleInfluence) * prevSegAngle + baseAngleInfluence * targetAngleForSeg);

                float torque = effStiffness * normalizeAngle(blendedTargetAngle - currentSegAngle);
                torque += gravityMag * segmentLengths[segIdx] * Mathf.Cos(currentSegAngle) * gravityEffect;

                float randFlowMag = flowMag * (1f + (float)(randomGen.NextDouble() * 0.4 - 0.2)); // Changed
                float randFlowAngle = flowAngle + (float)(randomGen.NextDouble() * (Mathf.PI / 6f) - (Mathf.PI / 12f)); // Changed
                torque += 0.5f * segmentLengths[segIdx] * randFlowMag * Mathf.Sin(normalizeAngle(randFlowAngle - currentSegAngle));
                torque += (segmentLengths[segIdx] / 2f) * (-externalForce.y * Mathf.Cos(currentSegAngle) + externalForce.x * Mathf.Sin(currentSegAngle)) * flowEffect;

                float alignmentFactor = Math.Max(0f, 1f - Math.Abs(normalizeAngle(currentSegAngle - blendedTargetAngle)) / Mathf.PI);
                torque -= damping * (1f + dampingFactor * alignmentFactor) * segmentAngVels[segIdx];

                float angleDiffPrev = normalizeAngle(currentSegAngle - prevSegAngle);
                float softBendLimit = Mathf.PI / 2f - 0.02f;
                float bendLimitBuffer = (20f * Mathf.PI / 180f);
                float effectiveBendLimit = softBendLimit - bendLimitBuffer;
                float constraintTorque = 0f;

                if (angleDiffPrev > effectiveBendLimit)
                {
                    float overshoot = angleDiffPrev - effectiveBendLimit;
                    constraintTorque = -(effStiffness * angleConstraintStrength) * overshoot;
                    if (angleDiffPrev > softBendLimit) constraintTorque *= 5f;
                }
                else if (angleDiffPrev < -effectiveBendLimit)
                {
                    float undershoot = angleDiffPrev - (-effectiveBendLimit);
                    constraintTorque = -(effStiffness * angleConstraintStrength) * undershoot;
                    if (angleDiffPrev < -softBendLimit) constraintTorque *= 5f;
                }
                torque += constraintTorque;

                float inertia = Math.Max(minInertia, inertiaFromLengthFactor * Mathf.Pow(segmentLengths[segIdx], 3));
                segmentAngVels[segIdx] += (inertia > 1e-9f ? (torque / inertia) * deltaTime : 0f);
                segmentAngles[segIdx] = normalizeAngle(segmentAngles[segIdx] + segmentAngVels[segIdx] * deltaTime);

                float newAngleDiffPrev = normalizeAngle(segmentAngles[segIdx] - segmentAngles[segIdx - 1]);
                float hardBendLimit = Mathf.PI / 2f - 0.005f;
                bool limitWasHit = false;

                if (newAngleDiffPrev > hardBendLimit)
                {
                    segmentAngles[segIdx] = normalizeAngle(segmentAngles[segIdx - 1] + hardBendLimit);
                    limitWasHit = true;
                }
                else if (newAngleDiffPrev < -hardBendLimit)
                {
                    segmentAngles[segIdx] = normalizeAngle(segmentAngles[segIdx - 1] - hardBendLimit);
                    limitWasHit = true;
                }
                if (limitWasHit)
                {
                    float angVelDiff = segmentAngVels[segIdx] - segmentAngVels[segIdx - 1];
                    segmentAngVels[segIdx] = segmentAngVels[segIdx - 1] - limitHitDamping * angVelDiff;
                }
            }
            updateGeometry();
        }
    }

    public class FurTuft : CosmeticSprite
    {
        public FurTuft(Room room, TechnomancerFur owner, Vector2 pos)
        {
            this.room = room;
            this.owner = owner;
            this.pos = pos;
            this.prevPos = pos;
            this.physics = new HairPhysics(pos, 0f, new float[] { 50f, 50f, 50f, 40f });
            this.colour = new Color(1f, 0f, 1f);
        }

        public override void Update(bool eu)
        {
            physics.updateDynamics(
                1/40f,
                Vector2.zero,
                10f,
                1f,
                0f,
                (this.prevPos - this.pos) * 64
            );

            renderPath = divAll(physics.renderPath, 20f);

            pathA = physics.JointA;
            pathB = physics.JointB;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(0, pos + renderPath[0]);

            int idxA = pathA ? 1 : 0;
            int idxB = pathB ? 1 : 0;

            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(1, pos + renderPath[1]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(2, pos + renderPath[2 - idxA]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(3, pos + renderPath[3 - idxA]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(4, pos + renderPath[4 - idxA - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(5, pos + renderPath[5 - idxA - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(6, pos + renderPath[6 - idxA - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(7, pos + renderPath[7 - idxA - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(8, pos + renderPath[8 - idxA - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(9, pos + renderPath[9 - idxA - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(10, pos + renderPath[9 - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(11, pos + renderPath[10 - idxB]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(12, pos + renderPath[10]);
            (sLeaser.sprites[0] as TriangleMesh).MoveVertice(13, pos + renderPath[11]);
            (sLeaser.sprites[0] as TriangleMesh).color = new UnityEngine.Color(0f, 1f, 0f);

            prevPos = pos;

            sLeaser.sprites[0].color = this.colour;

            // sLeaser.sprites[0].isVisible = true;// !owner.player.inShortcut;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
            {
            new TriangleMesh.Triangle(12, 13, 0),
            new TriangleMesh.Triangle(12, 1, 0),
            new TriangleMesh.Triangle(11, 12, 1),
            new TriangleMesh.Triangle(11, 2, 1),
            new TriangleMesh.Triangle(10, 11, 2),
            new TriangleMesh.Triangle(10, 3, 2),
            new TriangleMesh.Triangle(9, 10, 3),
            new TriangleMesh.Triangle(9, 4, 3),
            new TriangleMesh.Triangle(8, 9, 4),
            new TriangleMesh.Triangle(8, 5, 4),
            new TriangleMesh.Triangle(7, 8, 5),
            new TriangleMesh.Triangle(7, 6, 5),
            };
            sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, true, false);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Foreground");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public TechnomancerFur owner;

        public Vector2 prevPos;

        public HairPhysics physics;
        public Vector2[] renderPath;

        public bool pathA;
        public bool pathB;

        public Color colour;

        public Vector2[] divAll(Vector2[] original, float factor)
        {
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = original[i] / factor;
                original[i] = new Vector2(original[i].x, -original[i].y);
            }

            return original;
        }
    }

    public class TechnomancerFur : CosmeticSprite
    {
        public TechnomancerFur(Room room, PlayerGraphics owner)
        {
            this.room = room;
            this.owner = owner;
        }

        public override void Update(bool eu)
        {
            if (this.furTufts == null)
            {
                this.furTufts = new FurTuft[] { new FurTuft(room, this, pos) };

                for (int i = 0; i < this.furTufts.Length; i++)
                    room.AddObject(this.furTufts[i]);
            }

        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < this.furTufts.Length; i++)
            {
                this.furTufts[i].pos = pos + new Vector2(0f, 30f);
                this.furTufts[i].colour = this.colour;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void Destroy()
        {
            for (int i = 0; i < this.furTufts.Length; i++)
                this.furTufts[i].Destroy();

            base.Destroy();
        }

        public FurTuft[] furTufts;
        public PlayerGraphics owner;
        public Color colour;
    }

    public class VoyagerFur : CosmeticSprite
    {
        public VoyagerFur(Vector2 pos)
        {
            this.pos = pos;
        }

        public override void Update(bool eu)
        {
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            for (int i = 0; i < 1; i++)
            { sLeaser.sprites[i] = new FSprite("pixel", true); }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Midground");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public Vector2 pos;
    }

    internal static void Apply()
    {
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
    }

    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (self.player.Consious && self.objectLooker.currentMostInteresting != null && self.objectLooker.currentMostInteresting is Creature)
        {
            CreatureTemplate.Relationship relationship = self.player.abstractCreature.creatureTemplate.CreatureRelationship((self.objectLooker.currentMostInteresting as Creature).abstractCreature.creatureTemplate);

            // Changed Vector2.Distance (already correct, but for consistency review)
            float dangerLevel = Mathf.InverseLerp(Mathf.Lerp(40f, 250f, relationship.intensity), 10f, Vector2.Distance(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint) * (self.player.room.VisualContact(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint) ? 1f : 1.5f));
            if ((self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI != null && (self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI != null)
            {
                dangerLevel *= (self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI.CurrentPlayerAggression(self.player.abstractCreature);
            }
        }
    }

    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (TechyFur == null)
        {
            TechyFur = new TechnomancerFur(self.player.room, self);
            self.player.room.AddObject(TechyFur);
        }
        else if (TechyFur.room != self.player.room)
        {
            TechyFur.Destroy();
            TechyFur = new TechnomancerFur(self.player.room, self);
            self.player.room.AddObject(TechyFur);
        }

        orig(self, sLeaser, rCam);

        TechyFur.pos = sLeaser.sprites[0].GetPosition();

        string slug = self.player.slugcatStats.name.value;
        if (!new List<string> { "voyager", "technomancer" }.Contains(slug))
            return;

        switch (slug)
        {
            case "voyager":
                self.tail = new TailSegment[5];
                self.tail[0] = new TailSegment(self, 8f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 6f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 4.5f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 2f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
                self.tail[4] = new TailSegment(self, 1f, 7f, self.tail[3], 0.85f, 1f, 0.5f, true);
                break;

            case "technomancer":
                self.tail = new TailSegment[4];
                self.tail[0] = new TailSegment(self, 5.5f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 3.7f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 2.3f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 1f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
                break;
        }
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        string slug = self.player.slugcatStats.name.value;
        if (!new List<string> { "voyager", "technomancer" }.Contains(slug))
            return;

        float num = 0.5f + (0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * 3.1415927f * 2f));
        float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(RWCustom.Custom.DirVec(Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker), Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker)).y));
        switch (slug)
        {
            case "technomancer":
                sLeaser.sprites[0].scaleX = 0.96f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, self.malnourished), 0.05f, num) * num2, 0.15f, self.player.sleepCurlUp);
                sLeaser.sprites[1].scaleX = 0.93f + (self.player.sleepCurlUp * 0.2f) + (0.05f * num) - (0.05f * self.malnourished);
                break;

            case "voyager":
                sLeaser.sprites[0].scaleX = 1.17f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, self.malnourished), 0.05f, num) * num2, 0.15f, self.player.sleepCurlUp);
                sLeaser.sprites[1].scaleX = 1.2f + (self.player.sleepCurlUp * 0.2f) + (0.05f * num) - (0.05f * self.malnourished);
                break;
        }

        if (!OptionsMenu.furToggle.Value && sLeaser.sprites[3]?.element?.name is string text && text.StartsWith("Head"))
            sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("Fluff" + text);

        TechyFur.pos = sLeaser.sprites[0].GetPosition();
        TechyFur.colour = sLeaser.sprites[0].color;
        // Plugin.Logger.LogInfo($"{self.head.rad} {self.owner.bodyChunks[0].rad} {self.owner.bodyChunks[1].rad} {self.tail[0].rad} {self.tail[1].rad} {self.tail[2].rad} {self.tail[3].rad}");
    }

    public static TechnomancerFur TechyFur;
}