namespace Slugpack
{
    internal static class HypothermiaHooks
    {
        internal static void Apply()
        {
            On.MoreSlugcats.HypothermiaMeter.ctor += HypothermiaMeter_ctor;
            On.MoreSlugcats.HypothermiaMeter.Update += HypothermiaMeter_Update;
            On.Creature.HypothermiaUpdate += Creature_HypothermiaUpdate;
        }

        private static void HypothermiaMeter_ctor(On.MoreSlugcats.HypothermiaMeter.orig_ctor orig, MoreSlugcats.HypothermiaMeter self, HUD.HUD hud, FContainer fContainer)
        {
            orig(self, hud, fContainer);
            if (hud.owner is Player player && !player.IsVoyager()) return;

            foreach (var circle in self.circles)
            {
                fContainer.RemoveChild(circle.sprite);
            }
            self.circles = new HUD.HUDCircle[20];
            for (int i = 0; i < self.circles.Length; i++)
            {
                self.circles[i] = new HUD.HUDCircle(hud, HUD.HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0)
                {
                    fade = 0f,
                    lastFade = 0f
                };
                //self.circles[i].fade = 0f;
                //self.circles[i].lastFade = 0f;
            }
        }

        private static void HypothermiaMeter_Update(On.MoreSlugcats.HypothermiaMeter.orig_Update orig, MoreSlugcats.HypothermiaMeter self)
        {
            orig(self);
            if (self.hud.owner is Player player && !player.IsVoyager()) return;

            for (int i = 0; i < self.circles.Length; i++)
            {
                self.circles[i].pos = self.pos + new Vector2(i * 21.35f, 0f);
            }
        }

        private static void Creature_HypothermiaUpdate(On.Creature.orig_HypothermiaUpdate orig, Creature self)
        {
            float previousHypothermia = self.Hypothermia;
            orig(self);

            if (self is Player player && !player.IsVoyager()) return;
            float currentHypothermia = self.Hypothermia;
            if (currentHypothermia - previousHypothermia > 0 && !self.room.abstractRoom.shelter)
            {
                self.Hypothermia = previousHypothermia + 0.0001f; //(currentHypothermia - previousHypothermia) / 50f;
            }
        }
    }
}