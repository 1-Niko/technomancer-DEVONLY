namespace Slugpack;

internal static class PlayerGraphicsHooks
{
    private static readonly List<float> FEAFD765 = [64f, 32f, 16f, 8f, 4f, 2f, 1f];
    private static readonly List<float> D266428E = [-1f, 4f, 2f, 1f, 0.5f, 0.25f, 0.125f, 0.0625f, 0.03125f, 0.015625f, 0.0078125f, 0.00390625f, 0.001953125f];
    private static readonly List<float> E6AB2708 = [-1f, 8f, 4f, 2f, 1f, 0.5f, 0.25f, 0.125f, 0.0625f, 0.03125f, 0.015625f, 0.0078125f, 0.00390625f, 0.001953125f];
    private static readonly List<float> CCFC91A1 = [-1f, 256f, 128f, 64f, 32f, 16f, 8f, 4f, 2f, 1f, 0.5f, 0.25f, 0.125f, 0.0625f];
    private static readonly List<float> EC274780 = [1f, 0.5f, 0.25f, 0.125f, 0.0625f, 0.03125f, 0.015625f, 0.0078125f, 0.00390625f, 0.001953125f];
    private static readonly List<float> B9B05710 = [-1f, 1f];

    internal static void Apply()
    {
        On.PlayerGraphics.Update += (orig, self) =>
        {
            orig(self);

            if (!self.player.IsTechy(out var scanline)) return;

            if (self.player.Consious && self.objectLooker.currentMostInteresting != null && self.objectLooker.currentMostInteresting is Creature)
            {
                //scanline = ScanLineMemory.GetOrCreateValue(self.player);

                CreatureTemplate.Relationship relationship = self.player.abstractCreature.creatureTemplate.CreatureRelationship((self.objectLooker.currentMostInteresting as Creature).abstractCreature.creatureTemplate);

                float dangerLevel = Mathf.InverseLerp(Mathf.Lerp(40f, 250f, relationship.intensity), 10f, Vector2.Distance(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint) * (self.player.room.VisualContact(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint) ? 1f : 1.5f));
                if ((self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI != null && (self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI != null)
                {
                    dangerLevel *= (self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI.CurrentPlayerAggression(self.player.abstractCreature);
                }

                scanline.danger = dangerLevel;
            }
        };
        On.PlayerGraphics.InitiateSprites += (orig, self, sLeaser, rCam) => // Initiate Sprites
        {
            orig(self, sLeaser, rCam);

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

            List<BodyPart> list = Enumerable.ToList(self.bodyParts);
            _ = list.RemoveAll((BodyPart x) => x is TailSegment);
            list.AddRange(self.tail);
            self.bodyParts = [.. list];

            string binaryString = ConvertNumbersToBinaryString(FurData, 64);
            List<string> chunks = SplitStringIntoChunks(binaryString, 102);

            int slugTufts = chunks.Count(chunk => slug == ((chunk[0] == '0') ? "technomancer" : "voyager"));

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + slugTufts);

            int index = 0;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (slug == ((chunks[i][0] == '0') ? "technomancer" : "voyager") && sLeaser.sprites[index + 13] == null)
                {
                    sLeaser.sprites[index + 13] = new FSprite($"FurTuft{BinaryToFloat(2, chunks[i], FEAFD765)}", true)
                    {
                        isVisible = OptionsMenu.furToggle.Value,

                        anchorX = BinaryToFloat(50, chunks[i], EC274780),
                        anchorY = BinaryToFloat(60, chunks[i], EC274780),

                        scaleX = BinaryToFloat(70, chunks[i], B9B05710),
                        scaleY = BinaryToFloat(72, chunks[i], B9B05710)
                    };

                    index++;
                }
            }

            // DEBUG TESTING REMOVE OR COMMENT LATER
            // for (int i = 0; i < sLeaser.sprites.Length; i++)
            // {
            //     if (SlugpackShaders.TryGetValue(self.owner.room.game.rainWorld, out var Shaders))
            //         sLeaser.sprites[i].shader = Shaders.Redify;
            // }

            self.AddToContainer(sLeaser, rCam, null);
        };
        On.PlayerGraphics.DrawSprites += (orig, self, sLeaser, rCam, timeStacker, camPos) => // Draw Sprites
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.player.IsTechy(out var scanline))
            {
                //Leave this empty, you have 2 slugcats here if you check for 1 the other one will miss the sprites 
            }

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

            if (sLeaser.sprites[3]?.element?.name is string text && text.StartsWith("Head"))
                sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("Fluff" + text);

            // scanline = ScanLineMemory.GetOrCreateValue(self.player);

            scanline.screenPosition = new Vector2(((self.owner.abstractPhysicalObject.realizedObject as Creature).mainBodyChunk.pos.x - rCam.pos.x + 19) / 1400f, ((self.owner.abstractPhysicalObject.realizedObject as Creature).mainBodyChunk.pos.y - rCam.pos.y + 72) / 900f);

            string binaryString = ConvertNumbersToBinaryString(FurData, 64);
            List<string> chunks = SplitStringIntoChunks(binaryString, 102);

            int index = 0;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (slug == ((chunks[i][0] == '0') ? "technomancer" : "voyager") && sLeaser.sprites[index + 13] != null)
                {
                    float offsetX = BinaryToFloat(9, chunks[i], D266428E);
                    float offsetY = BinaryToFloat(22, chunks[i], E6AB2708);

                    sLeaser.sprites[index + 13].SetPosition(RotateAroundPoint(sLeaser.sprites[int.Parse(chunks[i][1].ToString())].GetPosition(), MultiplyVector2ByFloats(new Vector2(offsetX, offsetY), sLeaser.sprites[int.Parse(chunks[i][1].ToString())].scaleX, sLeaser.sprites[int.Parse(chunks[i][1].ToString())].scaleY), -sLeaser.sprites[int.Parse(chunks[i][1].ToString())].rotation));

                    float defaultRotation = BinaryToFloat(36, chunks[i], CCFC91A1);
                    float minimumRotation = BinaryToFloat(74, chunks[i], CCFC91A1);
                    float maximumRotation = BinaryToFloat(88, chunks[i], CCFC91A1);

                    sLeaser.sprites[index + 13].rotation = sLeaser.sprites[int.Parse(chunks[i][1].ToString())].rotation + CalculateFurRotation(defaultRotation, minimumRotation, maximumRotation, scanline.wetness, scanline.danger, num);

                    index++;
                }
            }
        };
        On.PlayerGraphics.AddToContainer += (orig, self, sLeaser, rCam, newContainer) => // Add To Container
        {
            orig(self, sLeaser, rCam, newContainer);
            string slug = self.player.slugcatStats.name.value;
            if (!new List<string> { "voyager", "technomancer" }.Contains(slug))
                return;

            string binaryString = ConvertNumbersToBinaryString(FurData, 64);
            List<string> chunks = SplitStringIntoChunks(binaryString, 102);

            int slugTufts = chunks.Count(chunk => slug == ((chunk[0] == '0') ? "technomancer" : "voyager"));

            FContainer midground = rCam.ReturnFContainer("Midground");
            FContainer foreground = rCam.ReturnFContainer("Foreground");

            if (sLeaser.sprites.Length > 13)
            {
                for (int i = 0; i < slugTufts; i++)
                {
                    foreground.RemoveChild(sLeaser.sprites[i + 13]);
                    midground.AddChild(sLeaser.sprites[i + 13]);
                    sLeaser.sprites[i + 13].MoveBehindOtherNode(sLeaser.sprites[int.Parse(chunks[i][1].ToString())]);
                }
            }
        };
    }
}