using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RealVis;
using RealVis.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealViz.Visualization
{
    public class RLLinePlot
    {
        const int Offset = 84;
        const float Scale = 128;
        const float Alpha = 0.35f;
        const float Thickness = 2;
        
        readonly int resolution;
        readonly int numLines;
        readonly Color color;
        readonly CircularBuffer<Vector2[]> spectrumPoints;

        /// <summary>
        /// Creates a new RLLinePlot instance
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="numLines"></param>
        /// <param name="color"></param>
        public RLLinePlot(int resolution, int numLines, Color color)
        {
            this.resolution = resolution;
            this.numLines = numLines;
            this.color = color;
            spectrumPoints = new CircularBuffer<Vector2[]>(numLines);

            for (int i = 0; i < numLines; i++)
                spectrumPoints.Add(new Vector2[resolution]);
        }

        /// <summary>
        /// Updates the RLLinePlot's points
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="pointData"></param>
        public void Update(GameTime gameTime, SpectrumBase.SpectrumPointData[] pointData)
        {
            Vector2[] pointArray = spectrumPoints.Back;

            float average = CalculateAverage(pointData);

            bool nonZero = false;
            
            for (int i = 0; i < resolution; i++)
            {
                nonZero = nonZero || pointData[i].Value != 0f;
                pointArray[i] = new Vector2(i / (float)(resolution - 1) * MainGame.ViewportWidth,
                    Offset - ((float)pointData[i].Value - average) * Scale);
            }

            if (!nonZero)
                Array.Clear(pointArray, 0, pointArray.Length);

            spectrumPoints.Add(pointArray);
        }

        /// <summary>
        /// Renders the RLLinePlot's points
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < numLines; i++)
            {
                Vector2[] points = spectrumPoints[i];
                for (int j = 1; j < points.Length; j++)
                {
                    spriteBatch.DrawLine(points[j - 1], points[j], color * ((float)(numLines - i) / numLines) * Alpha, Thickness);
                }
            }
        }

        /// <summary>
        /// Calculates the value in between the lowest and highest values
        /// </summary>
        /// <param name="pointData"></param>
        /// <returns></returns>
        private float CalculateAverage(SpectrumBase.SpectrumPointData[] pointData)
        {
            double low = double.MaxValue;
            double high = double.MinValue;

            for (int i = 0; i < pointData.Length; i++)
            {
                if (pointData[i].Value > high)
                    high = pointData[i].Value;

                if (pointData[i].Value < low)
                    low = pointData[i].Value;
            }

            return (float)(low + high) * 0.5f;
        }
    }
}
