using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pom.Pom;

namespace Slugpack
{
    public class TrainObjectData : ManagedData
    {
        [IntegerField("A", 0, 65535, 1, ManagedFieldWithPanel.ControlType.slider, displayName: "Seed")]
        public int seed;

        public TrainObjectData(PlacedObject owner) : base(owner, null)
        {
        }
    }

    public class ProceduralTrainObject : UpdatableAndDeletable
    {
        public ProceduralTrainObject(PlacedObject placedObject, Room room)
        {
            this.placedObject = placedObject;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (dynamicSprite == null)
            {
                dynamicSprite = new ProceduralTrain(this.placedObject.pos, (this.placedObject.data as TrainObjectData).seed, 28, 0f, 0f, 0f, 0f, this.room); // (this.placedObject.data as TrainObjectData).seed);
                this.room.AddObject(dynamicSprite);
            }
            dynamicSprite.pos = this.placedObject.pos;
            this.dynamicSprite.seed = (this.placedObject.data as TrainObjectData).seed;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        PlacedObject placedObject;

        ProceduralTrain dynamicSprite;
    }

    public class ProceduralTrain : CosmeticSprite
    {
        private static readonly string[,] spriteNames = new string[,] {
            {
                "Train Car N_0", "Train Car N_1", "Train Car N_2",
                "Train Car N_3", "Train Car N_4", "Train Car N_5",
                "Train Car N_6", "Train Car N_6", "Train Car N_6",
                "Train Car N_6"
            },
            {
                "Train Car NE_0", "Train Car NE_1", "Train Car NE_2",
                "Train Car NE_3", "Train Car NE_4", "Train Car NE_5",
                "Train Car NE_6", "Train Car NE_6", "Train Car NE_6",
                "Train Car NE_6"
            },
            {
                "Train Car NW_0", "Train Car NW_1", "Train Car NW_2",
                "Train Car NW_3", "Train Car NW_4", "Train Car NW_5",
                "Train Car NW_6", "Train Car NW_6", "Train Car NW_6",
                "Train Car NW_6"
            },
            {
                "Train Wheel N_0", "Train Wheel N_1", "Train Wheel N_2",
                "Train Wheel N_3", "Train Wheel N_4", "Train Wheel N_5",
                "Train Wheel N_5", "Train Wheel N_5", "Train Wheel N_5",
                "Train Wheel N_5"
            }
        };

        private static readonly string[] pipeEntranceNames = new string[] {
            "tpi_s_u",
            "tpi_s_d",
            "tpi_s_r",
            "tpi_s_l",
            "tpi_p_h",
            "tpi_p_v"
        };

        private static Dictionary<int, float> offsetLengths = new Dictionary<int, float>
        {
            {22, 91.5f}, {24, 112.0f}, {26, 132.0f},
            {28, 153.0f}, {30, 172.0f}, {32, 242.5f},
            {34, 263.0f}, {36, 283.0f}, {38, 303.5f},
            {40, 323.5f}, {42, 343.5f}, {44, 364.0f},
            {46, 384.0f}, {48, 404.0f}, {50, 424.0f}
        };

    public ProceduralTrain(Vector2 pos, int seed, int length, float velocity, float hit_lower_bound, float hit_upper_bound, float delete_position, Room room)
        {
            this.room = room;
            this.pos = pos;
            this.seed = seed;
            this.length = length;
            this.velocity = velocity;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            // Tried just copying the Update from TrainObject, did not work as intended. Will definitely use it as a guide, but have to rewrite everything

            pos += new Vector2(this.velocity, 0);

            if (pos.x > this.room.PixelWidth + 200)
            {
                this.Destroy();
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            length = (int)(length / 2) * 2;

            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(this.seed);

            // Random Value initializations
            // Top Train accessory
            top_index[0] = Random.Range(0,35);
            top_index[1] = Random.Range(0,35);
            top_index[2] = Random.Range(0,35);

            // Top train visibility
            int configuration = Random.Range(0, 6);
            switch (configuration)
            {
                case 0:
                    top_isVisible[0] = false;
                    top_isVisible[1] = false;
                    top_isVisible[2] = false;
                    break;
                case 1:
                    top_isVisible[0] = true;
                    top_isVisible[1] = false;
                    top_isVisible[2] = false;
                    break;
                case 2:
                    top_isVisible[0] = false;
                    top_isVisible[1] = false;
                    top_isVisible[2] = true;
                    break;
                case 3:
                    top_isVisible[0] = true;
                    top_isVisible[1] = true;
                    top_isVisible[2] = false;
                    break;
                case 4:
                    top_isVisible[0] = false;
                    top_isVisible[1] = true;
                    top_isVisible[2] = true;
                    break;
                case 5:
                    top_isVisible[0] = true;
                    top_isVisible[1] = true;
                    top_isVisible[2] = true;
                    break;
            }

            int centerwheel_threshold = 31;

            for (int s = 0; s < 2; s++)
            {
                for (int k = 0; k < layer_count; k++)
                {
                    for (int i = 0; i < length; i++)
                    {
                        int isShadow = ((s == 0 ? 0 : 1) * (length * layer_count));

                        // Adjust this for the piece offsets
                        float offsetAmount = 0f;
                        for (int _i = 0; _i < i; _i++)
                            offsetAmount += Mathf.Max(Mathf.Max(20f, (-Mathf.Abs(140f * (_i - (length / 2f) + 0.5f)) + 140f) * (length > centerwheel_threshold ? 1 : 0)), Mathf.Max((-53f * _i) + 123f, (53f * _i) - ((53f * (length - 2)) - 123f)));

                        string element_name = "";

                        int tempCase;

                        if (i == length - 1)
                            tempCase = -1;  // Use a unique constant to represent the right endcap
                        else if (i == length - 2)
                            tempCase = -2;  // Use a unique constant to represent the right wheel
                        else if (i == (int)(length / 2) + (length % 2))
                            tempCase = -3;  // Use a unique constant to represent the center wheel
                        else
                            tempCase = i;

                        switch (tempCase)
                        {
                            case 0: // Left endcap
                                element_name = spriteNames[2, k];
                                break;
                            case 1: // Left wheel
                                element_name = spriteNames[3, k];
                                break;
                            case -1: // Right endcap
                                element_name = spriteNames[1, k];
                                break;
                            case -2: // Right wheel
                                element_name = spriteNames[3, k];
                                break;
                            case -3: // Center wheel condition
                                if (length > centerwheel_threshold)
                                    element_name = spriteNames[3, k];
                                else
                                    element_name = spriteNames[0, k];
                                break;
                            default:
                                element_name = spriteNames[0, k];
                                break;
                        }

                        sLeaser.sprites[i + (k * length) + isShadow].element = Futile.atlasManager.GetElementWithName(element_name);

                        Vector2 pieceOffset = new Vector2(offsetAmount, 0f);
                        Vector2 currentPos = Vector2.Lerp(lastPos, pos, timeStacker);
                        Vector2 adjustedPos = (currentPos + pieceOffset) - rCam.pos;
                        Vector2 screenPosition = adjustedPos / new Vector2(1364f / 2f, 770f / 2f);

                        sLeaser.sprites[i + (k * length) + isShadow].SetPosition(adjustedPos - new Vector2(k * (screenPosition.x - 1f), k * (screenPosition.y - 1f)));

                        sLeaser.sprites[i + (k * length) + isShadow].color = new Color(i / 9f, this.seed / 65535f, 0f, -(k / 30) + (29f / 30f));

                        sLeaser.sprites[i + (k * length) + isShadow].isVisible = Utilities.CheckIfOnScreen((currentPos + pieceOffset), this.ROOM);

                        if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
                        {
                            sLeaser.sprites[i + (k * length) + isShadow].shader = shaders.DynamicTrain;
                            sLeaser.sprites[i + (k * length) + isShadow]._renderLayer?._material?.SetTexture("_ShadowMask", shaders._shadowMask);
                        }
                    }
                }
            }

            for (int i = length * layer_count * 2; i < (length + accessory_count) * layer_count * 2; i++)
            {
                //if (i < (length * layer_count * 2) + (layer_count * accessory_count))

                Vector2 currentPos = Vector2.Lerp(lastPos, pos, timeStacker);

                int random_index = ((i - (length * layer_count * 2)) / 10) % accessory_count;

                Vector2 globalPos = (currentPos + new Vector2(135f * (random_index + 1) + (random_index * offsetLengths[length]), 123f));
                Vector2 adjustedPos = globalPos - rCam.pos; // (9f * length) - 96.5f)
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName($"traintop_{top_index[random_index]}_{i % 10}");

                Vector2 screenPosition = adjustedPos / new Vector2(1364f / 2f, 770f / 2f);

                sLeaser.sprites[i].isVisible = top_isVisible[random_index] & Utilities.CheckIfOnScreen(globalPos, this.ROOM);

                sLeaser.sprites[i].SetPosition(adjustedPos - new Vector2((i % 10) * (screenPosition.x - 1f), (i % 10) * (screenPosition.y - 1f)));

                if (Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
                {
                    sLeaser.sprites[i].shader = shaders.DynamicTrain;
                    sLeaser.sprites[i]._renderLayer?._material?.SetTexture("_ShadowMask", shaders._shadowMask);
                }
            }

            UnityEngine.Random.state = state;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            int total_sprite_count = (length + accessory_count) * layer_count * 2; // Will have to multiply by 10 for the slices, and account for randomness. Will just hardcode it for now though

            sLeaser.sprites = new FSprite[total_sprite_count];

            for (int i = 0; i < total_sprite_count; i++)
                sLeaser.sprites[i] = new FSprite("pixel", true);

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            this.lastLength = length;

            // Why did I set these sprites to be in the hud? I know I had a good reason but I don't remember it

            // Probably something to do with the shader depth, since it seems it is not rendering correctly at the moment
            // Will have to fix that when I get a chance

            // It is a much stickier issue than I had hoped, will probably have to put this off for a while


            // FOR FUTURE REFERENCE:
            // These sprites being in the hud is to solve the BLOOM issue, to keep it from bleeding through incorrectly

            FContainer hudContainer = rCam.ReturnFContainer("HUD");
            FContainer midContainer = rCam.ReturnFContainer("Midground");
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                hudContainer.AddChild(fsprite);
            }

            for (int i = 0; i < length * layer_count; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                midContainer.AddChild(sLeaser.sprites[i]);
            }

            for (int i = 1; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[i - 1]);
            }

            for (int i = length * layer_count * 2; i < (length + accessory_count) * layer_count * 2; i++)
            {
                sLeaser.sprites[i].MoveInFrontOfOtherNode(sLeaser.sprites[(length * layer_count) + 1]);
            }


            for (int i = length * layer_count * 2; i < (length + accessory_count) * layer_count * 2; i++)
            {
                if (i > (length * layer_count * 2) + (layer_count * accessory_count))
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                    midContainer.AddChild(sLeaser.sprites[i]);
                }
            }
        }

        public int layer_count = 10;

        // This includes all *possible* accessories. If one is not present, it will still take up space in the array, but will be hidden
        public const int accessory_count = 3;

        public bool[] top_isVisible = new bool[3];

        public int[] top_index = new int[3];

        public int length; // = 38;

        public int seed;

        public float offset;

        public int lastLength;

        public float velocity;

        public float hit_lower_bound;

        public float hit_upper_bound;

        public float delete_position;

        public Room ROOM;
    }
}