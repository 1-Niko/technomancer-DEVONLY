using System; // Required for Func and ArgumentNullException
using UnityEngine; // Required for Vector2, Color, Mathf

// Assuming FSprite, RoomCamera, BackgroundScene, AboveCloudsView, Constants 
// are defined elsewhere in your project (e.g., Rain World modding context).
// using UnityEngine.Purchasing; // This was in the original, keep if relevant to your project.

namespace Slugpack
{
    public class DistantSatellite : BackgroundScene.BackgroundSceneElement
    {
        private AboveCloudsView AboveCloudsScene
        {
            get
            {
                return this.scene as AboveCloudsView;
            }
        }

        // Stores the function that defines the satellite's path
        private Func<float, float> pathFunction;
        // Stores the movement speed of the satellite
        private float movementSpeed;

        // Constructor updated to take pathFunction and movementSpeed instead of orbital
        public DistantSatellite(
            AboveCloudsView aboveCloudsScene,
            Vector2 pos,
            float depth,
            float minusDepthForLayering,
            Color nightColour,
            int nightTimer,
            float nightActivity,
            int nightThreshold,
            bool forceVisible,
            Func<float, float> pathFunction, // New: function defining the path
            float movementSpeed             // New: speed of movement
        ) : base(aboveCloudsScene, pos, depth - minusDepthForLayering)
        {
            this.minusDepthForLayering = minusDepthForLayering;

            this.colour_night = nightColour;
            this.night_timer = nightTimer;
            this.night_activity = nightActivity;
            this.alter_threshold = nightThreshold;
            this.forceVisible = forceVisible;

            if (pathFunction == null)
            {
                throw new ArgumentNullException(nameof(pathFunction), "Path function cannot be null.");
            }
            this.pathFunction = pathFunction;
            this.movementSpeed = movementSpeed;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            timer++;

            // Update movementTimer using the specified movementSpeed
            movementTimer = movementTimer + this.movementSpeed;

            // Day/night cycle logic (unchanged)
            if (!isNight && this.room.world.rainCycle.ShaderLight == -1)
            {
                if (timer % day_timer == 0)
                {
                    nightCountdown--;
                }
                if (nightCountdown == 0)
                {
                    isNight = true;
                }
            }
            else
            {
                nightCountdown = alter_threshold / day_timer;
            }
        }

        // Twinkle function (unchanged, and appears unused in the provided snippet)
        private float Twinkle(float offset)
        {
            float calculation = (Mathf.Sin(2f * ((timer + offset) / 512f)) + Mathf.Cos(Mathf.PI * ((timer + offset) / 512f))) / 16f;
            return calculation;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel", true);
            sLeaser.sprites[0].scale = 1f;
            sLeaser.sprites[0].isVisible = true;
            sLeaser.sprites[0].color = new Color(1f, 1f, 1f); // Default color, overridden in DrawSprites
            if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
            {
                sLeaser.sprites[0].shader = shaders.Satellite;
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_SkyMask", shaders._skymask);
            }
            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = base.DrawPos(new Vector2(camPos.x, camPos.y + this.AboveCloudsScene.yShift), rCam.hDisplace);

            // orbitCounter determines the X position relative to the base vector
            // and is the input to the path function. It sweeps from -750f to 750f.
            float orbitCounter = (movementTimer % 1500f) - 750f;
            sLeaser.sprites[0].x = vector.x + orbitCounter;

            // Calculate yOffset using the provided path function
            float yOffset = this.pathFunction(orbitCounter);

            // The large switch statement based on 'orbital' has been removed.
            // The yOffset is now determined by the 'pathFunction'.

            sLeaser.sprites[0].y = vector.y - yOffset; // Note: yOffset is subtracted
            sLeaser.sprites[0].scale = 1f;
            sLeaser.sprites[0].isVisible = true; // Could be combined with 'forceVisible' if needed
            sLeaser.sprites[0].color = colour_night;

            if (Constants.shaders_enabled && Constants.SlugpackShaders.TryGetValue(rCam.room.game.rainWorld, out var shaders))
            {
                sLeaser.sprites[0].shader = shaders.Satellite;
                sLeaser.sprites[0]._renderLayer?._material?.SetTexture("_SkyMask", shaders._skymask);
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        // Member fields
        public float minusDepthForLayering;
        public Color colour_night = new Color(0f, 1f, 0f, 1f); // Changed from Vector4 to Color for consistency
        public int timer;
        public float movementTimer;
        public int day_timer = 60;
        public int night_timer = 80;        // Unused in this snippet
        public float night_activity = 0.5f; // Unused in this snippet
        public int alter_threshold = 3000;
        public bool isNight = false;
        public int nightCountdown;
        public bool forceDay;               // Unused in this snippet
        public bool forceNight;             // Unused in this snippet
        public bool forceVisible;           // Is a constructor parameter, but not directly used for sLeaser.sprites[0].isVisible in this snippet.
                                            // Base class or other systems might use it.
                                            // public int orbital; // This field has been removed
    }
}