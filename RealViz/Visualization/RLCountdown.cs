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
    public class RLCountdown
    {
        public event EventHandler Alarm;

        readonly SpriteFont spriteFont;
        readonly Color color;

        float secondsLeft;

        public bool Counting { get; private set; }

        /// <summary>
        /// Initializes a new RLCountdown instance
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="color"></param>
        public RLCountdown(SpriteFont spriteFont, Color color)
        {
            this.spriteFont = spriteFont;
            this.color = color;

            secondsLeft = 0f;
            Counting = false;
        }

        /// <summary>
        /// Starts the countdown
        /// </summary>
        /// <param name="seconds"></param>
        public void Start(float seconds)
        {
            Counting = true;
            secondsLeft = seconds;
        }

        /// <summary>
        /// Stops the countdown
        /// </summary>
        public void Stop()
        {
            Counting = false;
            secondsLeft = 0;
        }

        /// <summary>
        /// Updates the RLCountdown
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (!Counting)
                return;

            secondsLeft -= gameTime.ElapsedGameTime.Milliseconds * 0.001f;

            if (secondsLeft < 0f)
            {
                Counting = false;
                secondsLeft = 0f;
                Alarm(this, new EventArgs());
            }
        }

        /// <summary>
        /// Draws the RLCountdown time
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Counting)
                return;

            TimeSpan timeSpan = TimeSpan.FromSeconds(secondsLeft);
            string timeString = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

            spriteBatch.DrawString(spriteFont, timeString, Vector2.Zero, color);
        }
    }
}
