using CSCore.DSP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVis.Visualization
{
    public abstract class SpectrumVisualizer : SpectrumBase
    {
        private bool isInitialized;
        private float[] fftBuffer;

        protected int ViewportWidth { get; private set; }
        protected int ViewportHeight { get; private set; }

        protected abstract void Update(GameTime gameTime, float[] fftBuffer);
        protected abstract void Render(GameTime gameTime, SpriteBatch spriteBatch, float[] fftBuffer);

        /// <summary>
        /// Initializes the SpectrumVisualizer.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="fftSize"></param>
        public SpectrumVisualizer(GraphicsDevice device, FftSize fftSize)
        {
            FftSize = fftSize;

            ViewportWidth = device.Viewport.Width;
            ViewportHeight = device.Viewport.Height;
        }

        /// <summary>
        /// Sets the resolution for the SpectrumVisualizer.
        /// </summary>
        public int Resolution
        {
            get => SpectrumResolution;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                SpectrumResolution = value;
                UpdateFrequencyMapping();
            }
        }

        /// <summary>
        /// Updates the SpectrumVisualizer's spectrum points.
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateVisualizer(GameTime gameTime)
        {
            if (!isInitialized)
            {
                UpdateFrequencyMapping();
                isInitialized = true;
            }

            fftBuffer = new float[(int)FftSize];

            if (SpectrumProvider.GetFftData(fftBuffer, this))
                Update(gameTime, fftBuffer);
        }

        /// <summary>
        /// Renders the SpectrumVisualizer.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public void RenderVisualizer(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (isInitialized)
                Render(gameTime, spriteBatch, fftBuffer);
        }
    }
}
