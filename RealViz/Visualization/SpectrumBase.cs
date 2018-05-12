using CSCore;
using CSCore.DSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVis.Visualization
{
    public class SpectrumBase
    {
        private const int ScaleFactorLinear = 9;
        protected const int ScaleFactorSqr = 2;
        protected const double MinDbValue = -90;
        protected const double MaxDbValue = 0;
        protected const double DbScale = (MaxDbValue - MinDbValue);

        public struct SpectrumPointData
        {
            public int SpectrumPointIndex { get; set; }
            public double Value { get; set; }
        }

        private int fftSize;
        private bool isXLogScale;
        private int maxFftIndex;
        private int maximumFrequency = 20000;
        private int maximumFrequencyIndex;
        private int minimumFrequency = 20;
        private int minimumFrequencyIndex;
        private ScalingStrategy scalingStrategy;
        private int[] spectrumIndexMax;
        private int[] spectrumLogScaleIndexMax;
        private SpectrumProvider spectrumProvider;

        protected int SpectrumResolution { get; set; }
        private bool useAverage;

        /// <summary>
        /// The maximum captured frequency.
        /// </summary>
        public int MaximumFrequency
        {
            get => maximumFrequency;
            set
            {
                if (value <= minimumFrequency)
                    throw new ArgumentOutOfRangeException("value", "MaximumFrequency out of range.");

                maximumFrequency = value;
                UpdateFrequencyMapping();
            }
        }

        /// <summary>
        /// The minimum captured frequency.
        /// </summary>
        public int MinimumFrequency
        {
            get => minimumFrequency;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "MinimumFrequency must be non-negative");

                minimumFrequency = value;
                UpdateFrequencyMapping();
            }
        }

        /// <summary>
        /// The <see cref="SpectrumProvider"/> instance.
        /// </summary>
        public SpectrumProvider SpectrumProvider
        {
            get => spectrumProvider;
            set => spectrumProvider = value ?? throw new ArgumentNullException("value");
        }

        /// <summary>
        /// Sets or gets if X log scaling is used.
        /// </summary>
        public bool IsXLogScale
        {
            get => isXLogScale;
            set
            {
                isXLogScale = value;
                UpdateFrequencyMapping();
            }
        }

        /// <summary>
        /// Sets the <see cref="ScalingStrategy"/>.
        /// </summary>
        public ScalingStrategy ScalingStrategy
        {
            get => scalingStrategy;
            set => scalingStrategy = value;
        }

        /// <summary>
        /// Sets or gets if the average frequencies are used.
        /// </summary>
        public bool UseAverage
        {
            get => useAverage;
            set => useAverage = value;
        }

        /// <summary>
        /// Sets or gets the FFT sample size.
        /// </summary>
        public FftSize FftSize
        {
            get => (FftSize)fftSize;
            protected set
            {
                if ((int)Math.Log((int)value, 2) % 1 != 0)
                    throw new ArgumentOutOfRangeException("value");

                fftSize = (int)value;
                maxFftIndex = fftSize / 2 - 1;
            }
        }

        /// <summary>
        /// Updates the frequencies stored by the raw frequency map.
        /// </summary>
        protected virtual void UpdateFrequencyMapping()
        {
            maximumFrequencyIndex = Math.Min(spectrumProvider.GetFftBandIndex(MaximumFrequency) + 1, maxFftIndex);
            minimumFrequencyIndex = Math.Min(spectrumProvider.GetFftBandIndex(minimumFrequency), maxFftIndex);

            int actualResolution = SpectrumResolution;

            int indexCount = maximumFrequencyIndex - minimumFrequencyIndex;
            double linearIndexBucketSize = Math.Round(indexCount / (double)actualResolution, 3);

            spectrumIndexMax = spectrumIndexMax.CheckBuffer(actualResolution, true);
            spectrumLogScaleIndexMax = spectrumLogScaleIndexMax.CheckBuffer(actualResolution, true);

            double maxLog = Math.Log(actualResolution, actualResolution);

            for (int i = 1; i < actualResolution; i++)
            {
                int logIndex = (int)((maxLog - Math.Log((actualResolution + 1) - i, (actualResolution + 1))) * indexCount) + minimumFrequencyIndex;

                spectrumIndexMax[i - 1] = minimumFrequencyIndex + (int)(i * linearIndexBucketSize);
                spectrumLogScaleIndexMax[i - 1] = logIndex;
            }

            if (actualResolution > 0)
                spectrumIndexMax[spectrumIndexMax.Length - 1] = spectrumLogScaleIndexMax[spectrumLogScaleIndexMax.Length - 1] = maximumFrequencyIndex;
        }

        /// <summary>
        /// Finds and returns an aray of <see cref="SpectrumPointData"/> containing the significant spectrum points.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="fftBuffer"></param>
        /// <returns></returns>
        protected virtual SpectrumPointData[] CalculateSpectrumPoints(double maxValue, float[] fftBuffer)
        {
            var dataPoints = new List<SpectrumPointData>();

            double value0 = 0, value = 0;
            double lastValue = 0;
            double actualMaxValue = maxValue;
            int spectrumPointIndex = 0;

            for (int i = minimumFrequencyIndex; i <= maximumFrequencyIndex; i++)
            {
                switch (scalingStrategy)
                {
                    case ScalingStrategy.Decibel:
                        value0 = (((20 * Math.Log10(fftBuffer[i])) - MinDbValue) / DbScale) * actualMaxValue;
                        break;
                    case ScalingStrategy.Linear:
                        value0 = (fftBuffer[i] * ScaleFactorLinear) * actualMaxValue;
                        break;
                    case ScalingStrategy.Sqrt:
                        value0 = ((Math.Sqrt(fftBuffer[i])) * ScaleFactorSqr) * actualMaxValue;
                        break;
                }

                bool recalc = true;

                value = Math.Max(0, Math.Max(value0, value));

                while (spectrumPointIndex <= spectrumIndexMax.Length - 1 &&
                    i == (isXLogScale ? spectrumLogScaleIndexMax[spectrumPointIndex] : spectrumIndexMax[spectrumPointIndex]))
                {
                    if (!recalc)
                        value = lastValue;

                    if (value > maxValue)
                        value = maxValue;

                    if (useAverage && spectrumPointIndex > 0)
                        value = (lastValue + value) / 2.0;

                    dataPoints.Add(new SpectrumPointData { SpectrumPointIndex = spectrumPointIndex, Value = value });

                    lastValue = value;
                    value = 0.0;
                    spectrumPointIndex++;
                    recalc = false;
                }
            }

            return dataPoints.ToArray();
        }
    }
}
