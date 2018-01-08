using CSCore.DSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVis.Visualization
{
    public class SpectrumProvider : FftProvider
    {
        private readonly int sampleRate;
        private readonly List<object> contexts = new List<object>();

        /// <summary>
        /// Initializes a new SpectrumProvider instance.
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        /// <param name="fftSize"></param>
        public SpectrumProvider(int channels, int sampleRate, FftSize fftSize)
            : base(channels, fftSize)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException("Invalid sampleRate");

            this.sampleRate = sampleRate;
        }

        /// <summary>
        /// Gets the index of the given frequency.
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public int GetFftBandIndex(float frequency)
        {
            int fftSize = (int)FftSize;
            double f = sampleRate / 2.0;
            return (int)((frequency / f) * (fftSize / 2));
        }

        /// <summary>
        /// Gets the FFT data from the given context.
        /// </summary>
        /// <param name="fftResultBuffer"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool GetFftData(float[] fftResultBuffer, object context)
        {
            if (contexts.Contains(context))
                return false;

            contexts.Add(context);
            GetFftData(fftResultBuffer);
            return true;
        }

        /// <summary>
        /// Adds a sample to the SampleProvider.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="count"></param>
        public override void Add(float[] samples, int count)
        {
            base.Add(samples, count);

            if (count > 0)
                contexts.Clear();
        }

        /// <summary>
        /// Adds a sample to the SampleProvider.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public override void Add(float left, float right)
        {
            base.Add(left, right);
            contexts.Clear();
        }
    }
}
