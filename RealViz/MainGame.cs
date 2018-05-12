using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using RealVis.Visualization;
using RealViz.Visualization;

namespace RealVis
{
    /// <summary>
    /// The main class for the visualizer.
    /// </summary>
    public class MainGame : Game
    {
        public const int ViewportWidth = 1280;//1920;
        public const int ViewportHeight = 720;//1080;

        public Camera2D Camera { get; private set; }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BoxingViewportAdapter viewportAdapter;

        WasapiCapture soundIn;
        IWaveSource source;
        SpectrumVisualizer spectrum;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        void SetupSampleSource(ISampleSource sampleSource)
        {
            FftSize fftSize = FftSize.Fft4096;

            SpectrumProvider spectrumProvider = new SpectrumProvider(sampleSource.WaveFormat.Channels,
                sampleSource.WaveFormat.SampleRate, fftSize);

            //spectrum = new BlobSpectrum(this, spectrumProvider, FftSize.Fft4096);
            spectrum = new RLSpectrum(this, spectrumProvider, FftSize.Fft4096);

            SingleBlockNotificationStream notificationSource = new SingleBlockNotificationStream(sampleSource);
            notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);

            source = notificationSource.ToWaveSource(16);
        }

        /// <summary>
        /// Initializes the visualizer and audio capture.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;

            graphics.HardwareModeSwitch = false;

            graphics.PreferredBackBufferWidth = ViewportWidth;
            graphics.PreferredBackBufferHeight = ViewportHeight;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, ViewportWidth, ViewportHeight);
            Camera = new Camera2D(viewportAdapter);

            soundIn = new WasapiLoopbackCapture();
            //soundIn = new WasapiCapture();
            soundIn.Initialize();

            SoundInSource inSource = new SoundInSource(soundIn);
            ISampleSource sampleSource = inSource.ToSampleSource();

            SetupSampleSource(sampleSource);

            byte[] buffer = new byte[source.WaveFormat.BytesPerSecond / 2];

            inSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = source.Read(buffer, 0, buffer.Length)) > 0) ;
            };

            soundIn.Start();

            base.Initialize();
        }

        /// <summary>
        /// Initializes the SpriteBatch for drawing.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Ends the audio capture so the process can end completely.
        /// </summary>
        protected override void UnloadContent()
        {
            if (soundIn != null)
            {
                soundIn.Stop();
                soundIn.Dispose();
                soundIn = null;
            }
            
            if (source != null)
            {
                source.Dispose();
                source = null;
            }
        }

        /// <summary>
        /// The update sequence for the visualizer.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            spectrum.UpdateVisualizer(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// The draw sequence for the visualizer.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color());  

            spectrum.RenderVisualizer(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
