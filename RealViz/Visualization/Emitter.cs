using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVis.Visualization
{
    public class Emitter
    {
        const int NumParticles = 1000;

        Random random;
        Particle[] particles;

        /// <summary>
        /// Controls how fast the emitted particles move.
        /// </summary>
        public float Boost { get; set; }

        /// <summary>
        /// Initializes a new Emitter instance.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="viewportWidth"></param>
        /// <param name="viewportHeight"></param>
        public Emitter(MainGame game, int viewportWidth, int viewportHeight)
        {
            Texture2D particleTexture = game.Content.Load<Texture2D>("Images/Particle");
            random = new Random();

            Boost = 1f;

            particles = new Particle[NumParticles];

            for (int i = 0; i < NumParticles; i++)
                particles[i] = new Particle(particleTexture, random, viewportWidth, viewportHeight);
        }

        /// <summary>
        /// Updates each emitted <see cref="Particle"/>.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            float boostDeltaTime = deltaTime * Boost;

            foreach (Particle p in particles)
                p.Update(deltaTime, boostDeltaTime);
        }

        /// <summary>
        /// Renders each emitted <see cref="Particle"/>.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void Render(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Particle p in particles)
                p.Render(gameTime, spriteBatch);
        }
    }
}
