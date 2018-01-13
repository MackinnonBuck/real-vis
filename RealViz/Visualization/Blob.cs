using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVis.Visualization
{
    public class Blob
    {
        const float RetargetTime = 0.25f;
        const float Acceleration = 0.05f;
        const float Padding = 1.05f;

        Texture2D circleTexture;
        Color color;
        Vector2 position;
        Vector2 velocity;
        Vector2 target;
        Random random;

        float sinceRetarget;

        /// <summary>
        /// Controls how far the Blob deviates outside of the main circle.
        /// </summary>
        public float Deviation { get; set; }

        /// <summary>
        /// Initializes a new Blob instance.
        /// </summary>
        /// <param name="circleTexture"></param>
        /// <param name="color"></param>
        /// <param name="random"></param>
        /// <param name="viewportWidth"></param>
        /// <param name="viewportHeight"></param>
        public Blob(Texture2D circleTexture, Color color, Random random)
        {
            this.circleTexture = circleTexture;
            this.color = color;
            this.random = random;

            position = Vector2.Zero;
            velocity = Vector2.Zero;
            target = Vector2.Zero;
            sinceRetarget = 0f;

            Deviation = 5f;
        }

        /// <summary>
        /// Updates the Blob's transformational information.
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
                target = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
        }

        /// <summary>
        /// Renders the Blob.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="radius"></param>
        public void Render(GameTime gameTime, SpriteBatch spriteBatch, int radius)
        {
            spriteBatch.Draw(circleTexture, new Rectangle(MainGame.ViewportWidth / 2 + (int)(position.X * Deviation), MainGame.ViewportHeight / 2 + (int)(position.Y * Deviation),
                radius * 2, radius * 2), null, color, 0.0f, new Vector2(BlobSpectrum.SourceRadius), SpriteEffects.None, 1);
        }
    }
}
