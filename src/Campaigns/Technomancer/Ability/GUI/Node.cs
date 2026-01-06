// Campaigns/Technomancer/Ability/GUI/Node.cs
/* Will control the visual aspect of the technomancy hud */

using UnityEngine;

namespace Slugpack;

public static partial class Technomancer
{
    public static partial class Ability
    {
        public class HackNode : CosmeticSprite
        {
            public HackNode(ManipulatableObject owner)
            {
                this.Owner = owner;
                this.pos = owner.pos;
                this.lastPos = owner.pos;
                this.appearTime = (int)UnityEngine.Random.Range(10f, 30f);
                this.colour = new Color(1f, 1f, 1f);
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                this.pos = ObjectRegistry.GetPos(Owner.owner.self, Owner.owner.type, Owner.room);
                this.Owner.pos = this.pos;
                this.appearTime--;
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];

                sLeaser.sprites[0] = new FSprite("pixel");

                sLeaser.sprites[0].scale = 20f;

                AddToContainer(sLeaser, rCam, null);
            }

            public void GraphicsUpdate()
            {
                this.appearTime--;
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

                Vector2 drawPos = Vector2.Lerp(base.lastPos, base.pos, timeStacker);
                sLeaser.sprites[0].SetPosition(drawPos - camPos);
                sLeaser.sprites[0].isVisible = this.appearTime < 0;
                sLeaser.sprites[0].color = colour;

                if (!isSlowed)
                    this.Destroy();
            }

            public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
            {
                newContainer ??= rCam.ReturnFContainer("HUD");

                sLeaser.sprites[0].RemoveFromContainer();
                newContainer.AddChild(sLeaser.sprites[0]);
            }

            public ManipulatableObject Owner;
            public int appearTime;

            public Color colour;
        }
    }
}