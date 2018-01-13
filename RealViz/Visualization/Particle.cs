using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVis.Visualization
{
    public class Particle
    {
        const float ZoomSpeed = 0.1f;
        const float ScaleFactor = 0.1f;

        const float RetargetTime = 0.25f;
        const float LooseAcceleration = 0.05f;
        const float Padding = 1.01f;

        Texture2D particleTexture;
        Color color;
        Random random;
        Vector2 realPosition;
        Vector2 loosePosition;
        Vector2 looseTarget;
        Vector2 looseVelocity;

        float lifeTime;
        float sinceRetarget;

        /// <summary>
        /// Initializes a new Particle instance.
        /// </summary>
        /// <param name="particleTexture"></param>
        /// <param name="random"></param>
        /// <param name="MainGame.ViewportWidth"></param>
        /// <param name="MainGame.ViewportHeight"></param>
        public Particle(Texture2D particleTexture, Random random)
        {
            this.particleTexture = particleTexture;
            this.random = random;

            color = new HslColor(random.Next(0, 255), 0.25f, 0.75f).ToRgb();

            Reset();
        }

        /// <summary>
        /// Updates the particle's transformational information.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="boostDeltaTime"></param>
        public void Update(float deltaTime, float boostDeltaTime)
        {
            if (looseTarget != loosePosition)
            {
                Vector2 looseDirection = looseTarget - loosePosition;
                looseDirection.Normalize();
                looseDirection *= LooseAcceleration;

                looseVelocity += looseDirection;
                looseVelocity /= Padding;
                loosePosition += looseVelocity;
            }

            sinceRetarget += deltaTime;

            if (sinceRetarget > RetargetTime)
            {
                sinceRetarget = 0f;
                double angle = random.NextDouble() * Math.PI * 2;
                looseTarget = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }

            lifeTime += boostDeltaTime;
            realPosition += (realPosition - new Vector2(MainGame.ViewportWidth / 2, MainGame.ViewportHeight / 2)) * lifeTime * boostDeltaTime * ZoomSpeed;

            if (realPosition.Y < 0 || realPosition.Y > MainGame.ViewportHeight || realPosition.X < 0 || realPosition.X > MainGame.ViewportWidth)
                Reset();
        }

        /// <summary>
        /// Renders the Particle.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Render(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(particleTexture, realPosition + loosePosition, null, color, 0f, new Vector2(8), lifeTime * ScaleFactor, SpriteEffects.None, 1f);
        }

        /// <summary>
        /// Resets the particle (puts it back in its initial state when it goes outside the screen).
        /// </summary>
        private void Reset()
        {
            realPosition = new Vector2(random.Next(MainGame.ViewportWidth), random.Next(MainGame.ViewportHeight));
            loosePosition = Vector2.Zero;
            looseTarget = Vector2.Zero;
            looseVelocity = Vector2.Zero;
            lifeTime = 0f;
            sinceRetarget = 0f;
        }
    }
}
