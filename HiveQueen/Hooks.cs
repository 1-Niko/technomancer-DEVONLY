using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using LizardCosmetics;
using System.Collections;

namespace HiveQueen;

sealed class Hooks
{
	internal static void Apply()
	{
        On.LizardGraphics.ctor += LizardGraphics_ctor;
        On.LizardAI.ctor += LizardAI_ctor;
        //On.Lizard.ctor += Lizard_ctor;
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += (orig, type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate) =>
        {
            List<TileTypeResistance> list = new List<TileTypeResistance>();
            List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
            if (type == CreatureTemplateType.HiveQueen)
            {
                var temp = orig(CreatureTemplate.Type.YellowLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                var breedParams = (temp.breedParameters as LizardBreedParams)!;
                breedParams.terrainSpeeds[1] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
                list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
                breedParams.terrainSpeeds[2] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
                list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1.2f, PathCost.Legality.Allowed));
                breedParams.terrainSpeeds[3] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
                list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 0.8f, PathCost.Legality.Allowed));
                breedParams.terrainSpeeds[4] = new LizardBreedParams.SpeedMultiplier(0.8f, 1f, 1f, 1f);
                list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed));
                breedParams.terrainSpeeds[5] = new LizardBreedParams.SpeedMultiplier(0.6f, 1f, 1f, 1f);
                list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1.2f, PathCost.Legality.Allowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Unallowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 2f, PathCost.Legality.Unallowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 15f, PathCost.Legality.Unallowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 1.1f, PathCost.Legality.Allowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 1.1f, PathCost.Legality.Allowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 1.1f, PathCost.Legality.Allowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 2f, PathCost.Legality.Unallowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 2f, PathCost.Legality.Unallowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 2f, PathCost.Legality.Unallowed));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.LizardTurn, 2f, PathCost.Legality.Unwanted));
                list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BigCreatureShortCutSqueeze, 2f, PathCost.Legality.Unallowed));
                temp.type = type;
                temp.name = "HiveQueen";
                breedParams.bodyMass = 20f;
                breedParams.bodySizeFac = 3f;
                breedParams.headSize = 2.1f;
                temp.bodySize = 3f;
                breedParams.limbSize = 2.5f;
                breedParams.bodyLengthFac = 1.8f;
                breedParams.bodyRadFac = 0.5f;
                breedParams.baseSpeed = 1.9f;

                //breedParams.tongue = true;
                //breedParams.tongueAttackRange = 440f;
                //breedParams.tongueWarmUp = 30;
                //breedParams.tongueSegments = 10;
                //breedParams.tongueChance = 0.7f;

                temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);

                CreatureTemplate creatureTemplate = new CreatureTemplate(type, lizardAncestor, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
                creatureTemplate.breedParameters = breedParams;
                creatureTemplate.baseDamageResistance = breedParams.toughness * 2f;
                creatureTemplate.baseStunResistance = breedParams.toughness;
                creatureTemplate.damageRestistances[(int)Creature.DamageType.Bite, 0] = 2.5f;
                creatureTemplate.damageRestistances[(int)Creature.DamageType.Bite, 1] = 3f;
                creatureTemplate.meatPoints = 12;

                //creatureTemplate.jumpAction = "Tongue";

                return temp;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        };
    }

    /*private static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Template.type == CreatureTemplateType.HiveQueen && self.tongue == null)
        {
            self.tongue = new LizardTongue(self);
        }
    }*/

    private static void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if (self.lizard.Template.type == CreatureTemplateType.HiveQueen)
        {
            self.yellowAI = new YellowAI(self);
            self.AddModule(self.yellowAI);
        }
    }

    private static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.lizard.abstractCreature.creatureTemplate.type == CreatureTemplateType.HiveQueen)
        {
            var num = self.startOfExtraSprites + self.extraSprites;
            self.ivarBodyColor = Color.white;
            for (int j = 0; j < 2; j++)
            {
                MultipleAntennae g = new MultipleAntennae(self, num, j);
                g.length = Random.value + ((j == 0) ? 10 : 4);
                g.segments = Mathf.FloorToInt(Mathf.Lerp(3f, 8f, Mathf.Pow(g.length, Mathf.Lerp(1f, 6f, g.length))));
                g.alpha = g.length * 0.9f + Random.value * 0.1f;
                g.antennae = new GenericBodyPart[2, g.segments];
                for (int i = 0; i < g.segments; i++)
                {
                    g.antennae[0, i] = new GenericBodyPart(g.lGraphics, 7f, 0.6f, 0.9f, g.lGraphics.lizard.mainBodyChunk);
                    g.antennae[1, i] = new GenericBodyPart(g.lGraphics, 7f, 0.6f, 0.9f, g.lGraphics.lizard.mainBodyChunk);
                }
                num = self.AddCosmetic(num, g);
            }

            num = self.AddCosmetic(num, new LongShoulderScales(self, num));
            //num = self.AddCosmetic(num, new LongBodyScales(self, num));

            /*self.tongue = new GenericBodyPart[self.lizard.lizardParams.tongueSegments];
            for (int m = 0; m < self.tongue.Length; m++)
            {
                self.tongue[m] = new GenericBodyPart(self, 1f, 1f, 0.9f, self.lizard.mainBodyChunk);
            }*/
        }
    }
}