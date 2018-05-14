using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealVis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealViz.Visualization
{
    public class RLCar
    {
        const float RetargetTime = 0.0125f;
        const float Acceleration = 0.075f;
        const float Padding = 1.1f;
        const float VerticalOffset = 128f;
        const float Deviation = 8f;

        readonly Texture2D carTexture;
        readonly Color color;
        readonly Random random;
        readonly Vector2 basePosition;

        Vector2 position;
        Vector2 velocity;
        Vector2 target;

        float sinceRetarget;

        /// <summary>
        /// Creates a new RLCar instance
        /// </summary>
        /// <param name="carTexture"></param>
        /// <param name="color"></param>
        /// <param name="random"></param>
        public RLCar(Texture2D carTexture, Color color, Random random)
        {
            this.carTexture = carTexture;
            this.color = color;
            this.random = random;

            position = Vector2.Zero;
            velocity = Vector2.Zero;
            target = Vector2.Zero;

            basePosition = new Vector2(MainGame.ViewportWidth / 2 - carTexture.Width / 2,
                 MainGame.ViewportHeight - carTexture.Height - VerticalOffset);

            sinceRetarget = 0f;
        }

        /// <summary>
        /// Updates the RLCar's position
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (target != position)
            {
                Vector2 direction = target - position;
                direction.Normalize();
                direction *= Acceleration;

                velocity += direction;
                velocity /= Padding;
                position += velocity;
            }

            sinceRetarget += gameTime.ElapsedGameTime.Milliseconds * 0.001f;

            if (sinceRetarget > RetargetTime)
            {
                sinceRetarget = 0f;
                double angle = random.NextDouble() * Math.PI * 2;
                target = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Deviation;
            }
        }

        /// <summary>
        /// Draws the RLCar
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(carTexture, basePosition + position, color);
        }
    }
}
