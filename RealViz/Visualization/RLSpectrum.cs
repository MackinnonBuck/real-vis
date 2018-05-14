using CSCore.DSP;
using Gma.System.MouseKeyHook;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RealVis;
using RealVis.Visualization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealViz.Visualization
{
    public class RLSpectrum : SpectrumVisualizer
    {
        const int LowCut = 15;
        const int HighCut = 3;
        const int MatrixResolution = 100;
        const int MatrixNumEntries = 100;
        const int LineCache = 10;
        const float Speed = 75f;
        const int CarOffset = 128;
        const int MessagePadding = 24;
        const float CountdownSeconds = 340;
        const string OBSProcess = "obs64";

        readonly MainGame game;
        readonly BasicEffect effect;
        readonly Random random;
        readonly RLMatrix matrix;
        readonly RLLinePlot linePlot;
        readonly RLCar car;
        readonly RLCountdown countdown;
        
        readonly Color RetroBlue = new Color(0f, 0.85f, 1f, 0.65f);
        readonly Color RetroPurple = new Color(1f, 0f, 1f);
        readonly Color RetroRed = new Color(1f, 0f, 0f);

        readonly Texture2D carTexture;
        readonly Texture2D messageTexture;
        readonly SpriteFont spriteFont;

        readonly SpectrumPointData[] constrainedPoints;
        SpectrumPointData[] spectrumPoints;

        IKeyboardMouseEvents globalHook;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        const UInt32 WM_KEYDOWN = 0x0100;
        const int VK_PRIOR = 0x21;

        /// <summary>
        /// Initializes a new RLSpectrum instance
        /// </summary>
        /// <param name="game"></param>
        /// <param name="provider"></param>
        /// <param name="fftSize"></param>
        public RLSpectrum(MainGame game, SpectrumProvider provider, FftSize fftSize) : base(fftSize)
        {
            this.game = game;

            SpectrumProvider = provider;
            UseAverage = true;
            Resolution = MatrixResolution + LowCut + HighCut;
            IsXLogScale = true;
            ScalingStrategy = ScalingStrategy.Decibel;

            effect = new BasicEffect(game.GraphicsDevice)
            {
                DiffuseColor = RetroPurple.ToVector3()
            };

            random = new Random();

            matrix = new RLMatrix(game, MatrixNumEntries, MatrixResolution);

            linePlot = new RLLinePlot(MatrixResolution, LineCache, RetroPurple);

            carTexture = game.Content.Load<Texture2D>("Images/Octane");
            messageTexture = game.Content.Load<Texture2D>("Images/Message");
            spriteFont = game.Content.Load<SpriteFont>("Fonts/Lazer84");

            car = new RLCar(carTexture, RetroBlue, random);

            countdown = new RLCountdown(spriteFont, RetroRed);
            countdown.Alarm += CountdownAlarm;

            constrainedPoints = new SpectrumPointData[MatrixResolution];

            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += KeyDown;
        }

        /// <summary>
        /// Updates the RLSpectrum matrix and line plot
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="fftBuffer"></param>
        protected override void Update(GameTime gameTime, float[] fftBuffer)
        {
            spectrumPoints = CalculateSpectrumPoints(1.0, fftBuffer);
            Array.ConstrainedCopy(spectrumPoints, LowCut, constrainedPoints, 0, MatrixResolution);

            matrix.Update(gameTime, Speed, constrainedPoints);
            linePlot.Update(gameTime, constrainedPoints);
            car.Update(gameTime);
            countdown.Update(gameTime);
        }

        /// <summary>
        /// Renders the RLSpectrum matrix and line plot
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="fftBuffer"></param>
        protected override void Render(GameTime gameTime, SpriteBatch spriteBatch, float[] fftBuffer)
        {
            matrix.Render(gameTime, spriteBatch);

            spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            linePlot.Draw(spriteBatch);

            car.Draw(spriteBatch);

            countdown.Draw(spriteBatch);

            spriteBatch.Draw(messageTexture, new Vector2(MainGame.ViewportWidth * 0.5f - messageTexture.Width * 0.5f, 0f), Color.White);

            spriteBatch.End();
        }

        /// <summary>
        /// Switches the OBS scene when the timer hits zero
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountdownAlarm(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName(OBSProcess);

            foreach (Process p in processes)
                PostMessage(p.MainWindowHandle, WM_KEYDOWN, VK_PRIOR, 0);
        }

        /// <summary>
        /// Starts the countdown when the play button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Figure out a better way of doing this?
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.MediaPlayPause:
                    if (countdown.Counting)
                        countdown.Stop();
                    else
                        countdown.Start(CountdownSeconds);
                    break;
            }
        }
    }
}
