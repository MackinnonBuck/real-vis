using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.DSP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace RealVis.Visualization
{
    public class BlobSpectrum : SpectrumVisualizer
    {
        SpectrumPointData[] spectrumPoints;
        Blob[] blobs;
        Emitter emitter;
        Random random;

        Camera2D camera;
        readonly Texture2D circleTexture;
        readonly Texture2D rvCircleTexture;

        /// <summary>
        /// The radius of the source image.
        /// </summary>
        public const int SourceRadius = 320;

        const int BlobCount = 8;
        const int BaseRadius = 160;
        const double PeakScale = 150;
        const int LowCap = 15;
        const int HighMin = 100;
        const int HighCap = 300;
        const int LowRisingBufferSize = 3;
        const int LowFallingBufferSize = 15;
        const int HighAvgBufferSize = 10;
        const float DeviationScale = 150f;
        const float EmitterBoostThreshold = 3.5f;
        const float CameraShakeThreshold = 12f;
        const double LowDeviationInfluence = 0.05f;
        const double ExpandThreshold = 0.5;

        double lowAvgBuffer;
        double highAvgBuffer;

        /// <summary>
        /// Initializes a new BlobSpectrum instance.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="device"></param>
        /// <param name="spectrumProvider"></param>
        /// <param name="fftSize"></param>
        public BlobSpectrum(MainGame game, SpectrumProvider spectrumProvider, FftSize fftSize)
            : base(fftSize)
        {
            SpectrumProvider = spectrumProvider;
            UseAverage = true;
            Resolution = 500;
            IsXLogScale = true;
            ScalingStrategy = ScalingStrategy.Linear;

            emitter = new Emitter(game, MainGame.ViewportWidth, MainGame.ViewportHeight);

            camera = game.Camera;
            circleTexture = game.Content.Load<Texture2D>("Images/Circle");
            rvCircleTexture = game.Content.Load<Texture2D>("Images/MachCircle");

            blobs = new Blob[BlobCount];

            random = new Random();

            for (int i = 0; i < blobs.Length; i++)
                blobs[i] = new Blob(circleTexture, new HslColor(i * (255 / blobs.Length), 0.5f, 0.5f).ToRgb(), random);

            lowAvgBuffer = 0;
            highAvgBuffer = 0;
        }

        /// <summary>
        /// Updates the BlobSpectrum's <see cref="Blob"/> instances and particle <see cref="Emitter"/>.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="fftBuffer"></param>
        protected override void Update(GameTime gameTime, float[] fftBuffer)
        {
            spectrumPoints = CalculateSpectrumPoints(1.0, fftBuffer);

            double lowPeak = 0.0;

            for (int i = 0; i < LowCap; i++)
                if (spectrumPoints[i].Value > lowPeak)
                    lowPeak = spectrumPoints[i].Value;

            RecalculateAverage(ref lowAvgBuffer, lowPeak > lowAvgBuffer ? LowRisingBufferSize : LowFallingBufferSize, lowPeak);

            double highAvg = 0.0;

            for (int i = HighMin; i < HighCap; i++)
                highAvg += spectrumPoints[i].Value;

            highAvg /= HighCap - HighMin;

            RecalculateAverage(ref highAvgBuffer, HighAvgBufferSize, highAvg + lowPeak * LowDeviationInfluence);

            foreach (Blob b in blobs)
            {
                b.Deviation = (float)highAvgBuffer * DeviationScale;
                b.Update(gameTime);
            }

            emitter.Boost = lowAvgBuffer > ExpandThreshold ? 1.0f + (float)(lowAvgBuffer - ExpandThreshold) * EmitterBoostThreshold : 1.0f;
            emitter.Update(gameTime);

            if (lowAvgBuffer > ExpandThreshold)
                camera.Position = new Vector2(random.NextSingle(), random.NextSingle()) * (float)(lowAvgBuffer - ExpandThreshold) * CameraShakeThreshold;
        }

        /// <summary>
        /// Renders the large blob, colored blobs, and particles.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="fftBuffer"></param>
        protected override void Render(GameTime gameTime, SpriteBatch spriteBatch, float[] fftBuffer)
        {
            Matrix cameraMatrix = camera.GetViewMatrix();

            spriteBatch.Begin(blendState: BlendState.Additive, transformMatrix: cameraMatrix);

            emitter.Render(gameTime, spriteBatch);

            int circleRadius = lowAvgBuffer > ExpandThreshold ? BaseRadius + (int)((lowAvgBuffer - ExpandThreshold) * PeakScale) : BaseRadius;

            foreach (Blob b in blobs)
                b.Render(gameTime, spriteBatch, circleRadius);

            spriteBatch.End();

            spriteBatch.Begin(transformMatrix: cameraMatrix);

            spriteBatch.Draw(rvCircleTexture, new Rectangle(MainGame.ViewportWidth / 2, MainGame.ViewportHeight / 2,
                circleRadius * 2, circleRadius * 2), null, Color.White, 0.0f, new Vector2(SourceRadius), SpriteEffects.None, 1);

            spriteBatch.End();

            spriteBatch.Begin();

            Vector2 lastPoint = new Vector2(0, MainGame.ViewportHeight - (float)(spectrumPoints[0].Value * 500.0));

            for (int i = 1; i < spectrumPoints.Length; i++)
            {
                Vector2 currentPoint = new Vector2((float)MainGame.ViewportWidth / spectrumPoints.Length * i, MainGame.ViewportHeight - (float)(spectrumPoints[i].Value * 500.0));
                spriteBatch.DrawLine(lastPoint, currentPoint, Color.White, 2);

                lastPoint = currentPoint;
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Used for recalculating an average value with the given new value.
        /// </summary>
        /// <param name="average"></param>
        /// <param name="bufferSize"></param>
        /// <param name="value"></param>
        private void RecalculateAverage(ref double average, int bufferSize, double value)
        {
            average *= (double)(bufferSize - 1) / bufferSize;
            average += value / bufferSize;
        }
    }
}
