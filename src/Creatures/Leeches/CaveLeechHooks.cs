namespace Slugpack;

public static class CaveLeechHooks
{
    public static void Apply()
    {
        On.Leech.Update += Leech_Update;
        On.LeechGraphics.ctor += LeechGraphics_ctor;
        On.LeechGraphics.DrawSprites += LeechGraphics_DrawSprites;
    }


    private static void LeechGraphics_DrawSprites(On.LeechGraphics.orig_DrawSprites orig, LeechGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.leech.abstractCreature.creatureTemplate.type != TnEnums.CreatureType.CaveLeech)
        {
            return;
        }
        //This is for coloring the Leech when is outside the water
        ///if for black color and else for white color (outside water)
        if(self.leech.airDrown == 0) sLeaser.sprites[0].color = new(0.08f, 0.08f, 0.08f);
        else sLeaser.sprites[0].color = Color.Lerp(new(0.08f, 0.08f, 0.08f), new(1f, 1f, 1f), 0.5f + 0.5f * Mathf.InverseLerp(0.05f, 0.2f, self.leech.airDrown));
    }

    private static void Leech_Update(On.Leech.orig_Update orig, Leech self, bool eu)
    {
        //runs out of air, faster than a normal leech
        if(self.airDrown > 0 && self.Template.type == TnEnums.CreatureType.CaveLeech && self.mainBodyChunk.submersion > 0f && !self.dead)
        {
            self.airDrown -= 0.011f;
        }
        orig(self, eu);
    }

    private static void LeechGraphics_ctor(On.LeechGraphics.orig_ctor orig, LeechGraphics self, PhysicalObject ow)
    {
        orig(self, ow);

        if (self.leech.abstractCreature.creatureTemplate.type != TnEnums.CreatureType.CaveLeech)
        {
            return;
        }
        //No working, trying to override like sea leech does crashes the game
        for (int i = 0; i < self.body.Length; i++)
        {
            self.body[i] = new GenericBodyPart(self, 0.5f, 0.7f, 1f, self.leech.mainBodyChunk);
            self.bodyParts[i] = self.body[i];
            self.radiuses[i] = 0.3f;
        }
    }
}