using UnityEngine.PlayerLoop;

namespace Slugpack;

internal static class PlayerGraphicsHooks
{
    public class TechnomancerFur : CosmeticSprite
    {
        public TechnomancerFur(Room room, PlayerGraphics owner)
        {
            this.pos = pos;
            this.room = room;
            this.owner = owner;
        }

        public override void Update(bool eu)
        {
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].scale = 100f;
            sLeaser.sprites[0].SetPosition(pos);
            sLeaser.sprites[0].isVisible = !owner.player.inShortcut;
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

        public Room room;

        public PlayerGraphics owner;
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
    }


    private static TechnomancerFur TechyFur;
}
