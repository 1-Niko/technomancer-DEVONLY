using UnityEngine;

namespace Slugpack
{
    static class LizardHooks
    {
        internal static void Apply()
        {
            On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
            On.Lizard.ctor += Lizard_ctor;
        }

        private static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
        }

        private static CreatureTemplate LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
        {
            CreatureTemplate result;
            if (type == CreatureTemplateType.PastGreen)
            {
                Debug.Log("Applying PastGreen template parameters!");
                CreatureTemplate creatureTemplate = orig.Invoke(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                LizardBreedParams lizardBreedParams = creatureTemplate.breedParameters as LizardBreedParams;
                creatureTemplate.type = type;
                creatureTemplate.name = "PastGreen";
                lizardBreedParams.baseSpeed = 6.7f;
                lizardBreedParams.bodySizeFac = 1.2f;
                lizardBreedParams.bodyRadFac = 1f;
                lizardBreedParams.bodyMass = 7.5f;
                lizardBreedParams.limbThickness = 1f;
                lizardBreedParams.headSize = 0.9f;
                creatureTemplate.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);
                creatureTemplate.doPreBakedPathing = false;
                creatureTemplate.breedParameters = lizardBreedParams;
                result = creatureTemplate;
            }
            else
            {
                result = orig.Invoke(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            }
            return result;
        }
    }
}