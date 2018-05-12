using CSCore.DSP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealVis;
using RealVis.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealViz.Visualization
{
    public class RLSpectrum : SpectrumVisualizer
    {
        const float Speed = 50f;
        const int CarOffset = 128;

        const string nameString = "mackinnonbuck";
        const string messageString = "stream   starting   soon...";

        readonly MainGame mainGame;
        readonly BasicEffect effect;
        readonly Random random;
        readonly RLMatrix matrix;
        readonly Car car;
        
        readonly Texture2D carTexture;
        readonly SpriteFont spriteFont;

        readonly Vector2 nameSize;
        readonly Vector2 messageSize;

        SpectrumPointData[] spectrumPoints;

        public RLSpectrum(MainGame game, SpectrumProvider provider, FftSize fftSize) : base(fftSize)
        {
            mainGame = game;
            SpectrumProvider = provider;
            UseAverage = true;
            Resolution = 50;
            IsXLogScale = true;
            ScalingStrategy = ScalingStrategy.Decibel;
            
            effect = new BasicEffect(game.GraphicsDevice)
            {
                DiffuseColor = new Vector3(1f, 0f, 1f)
            };

            random = new Random();

            matrix = new RLMatrix(game, 75, Resolution);

            carTexture = game.Content.Load<Texture2D>("Images/Octane");
            spriteFont = game.Content.Load<SpriteFont>("Fonts/Lazer84");

            nameSize = spriteFont.MeasureString(nameString);
            messageSize = spriteFont.MeasureString(messageString);

            car = new Car(carTexture, new Color(0f, 0.75f, 1f), random);
        }

        protected override void Update(GameTime gameTime, float[] fftBuffer)
        { 
            spectrumPoints = CalculateSpectrumPoints(1.0, fftBuffer);

            matrix.Update(gameTime, Speed, spectrumPoints);
            car.Update(gameTime);
        }

        protected override void Render(GameTime gameTime, SpriteBatch spriteBatch, float[] fftBuffer)
        {
            spriteBatch.Begin();

            matrix.Render(gameTime, spriteBatch);

            car.Draw(spriteBatch);

            spriteBatch.DrawString(spriteFont, nameString, new Vector2(MainGame.ViewportWidth / 2 - nameSize.X / 2, 0f),
                new Color(1f, 0.75f, 0f));

            spriteBatch.DrawString(spriteFont, messageString, new Vector2(MainGame.ViewportWidth / 2 - messageSize.X / 2,
                messageSize.Y), new Color(0f, 0.75f, 1f));

            spriteBatch.End();
        }
    }
}
