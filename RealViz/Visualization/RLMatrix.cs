using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealVis;
using RealVis.Visualization;

namespace RealViz.Visualization
{
    public class RLMatrix
    {
        const float Offset = -30;
        const float MatrixRange = 60f;
        const float MatrixSpan = 26;
        const float WaveScale = 75f;
        const float LaneScale = 3f;
        const int LaneWidth = 5;

        readonly MainGame game;
        readonly CircularBuffer<VertexPositionTexture[]> matrix;
        readonly VertexPositionTexture[][] columns;
        readonly int resolution;
        readonly int numEntries;
        readonly BasicEffect effect;
        readonly float matrixSegment;
        readonly int laneStart;
        readonly int laneEnd;
        readonly float[] scaledPointData;

        float zOffset;

        /// <summary>
        /// Creates a new RLMatrix instance
        /// </summary>
        /// <param name="game"></param>
        /// <param name="numEntries"></param>
        /// <param name="resolution"></param>
        public RLMatrix(MainGame game, int numEntries, int resolution)
        {
            this.game = game;
            this.resolution = resolution;
            this.numEntries = numEntries;

            matrix = new CircularBuffer<VertexPositionTexture[]>(numEntries);

            for (int i = 0; i < numEntries; i++)
            {
                VertexPositionTexture[] vpt = new VertexPositionTexture[resolution];

                for (int j = 0; j < resolution; j++)
                    vpt[j].Position = new Vector3(MatrixSpan * -0.5f + j * (MatrixSpan / (resolution - 1)), 0f, 0f);

                matrix.Add(vpt);
            }

            columns = new VertexPositionTexture[resolution][];

            for (int i = 0; i < resolution; i++)
                columns[i] = new VertexPositionTexture[numEntries];

            effect = new BasicEffect(game.GraphicsDevice)
            {
                DiffuseColor = new Vector3(1f, 0f, 1f)
            };

            Vector3 cameraPosition = new Vector3(0, 20, 40);
            var cameraLookAtVector = Vector3.Zero;
            var cameraUpVector = Vector3.UnitY;

            effect.View = Matrix.CreateLookAt(cameraPosition, cameraLookAtVector, cameraUpVector);

            float aspectRatio = MainGame.ViewportHeight / (float)MainGame.ViewportHeight;
            float fieldOfView = MathHelper.PiOver4;
            float nearClipPlane = 1;
            float farClipPlane = 200;

            effect.Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClipPlane, farClipPlane);

            matrixSegment = MatrixRange / numEntries;

            laneStart = (resolution - LaneWidth) / 2;
            laneEnd = (resolution + LaneWidth) / 2;

            zOffset = 0f;

            scaledPointData = new float[resolution];
        }

        /// <summary>
        /// Updates the RLMatrix's vertices
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="speed"></param>
        /// <param name="pointData"></param>
        public void Update(GameTime gameTime, float speed, SpectrumBase.SpectrumPointData[] pointData)
        {
            Process(pointData, WaveScale, 0, laneStart);
            Process(pointData, LaneScale, laneStart, laneEnd);
            Process(pointData, WaveScale, laneEnd, resolution);

            for (int i = 0; i < laneStart; i++)
                scaledPointData[i] *= (float)(laneStart - i) / laneStart;

            for (int i = laneEnd; i < resolution; i++)
                scaledPointData[i] *= (float)(i - laneEnd) / (resolution - laneEnd);

            zOffset += gameTime.ElapsedGameTime.Milliseconds * 0.001f * speed;

            for (; zOffset > matrixSegment; zOffset -= matrixSegment)
            {
                VertexPositionTexture[] newVpt = matrix.Back;

                for (int i = 0; i < resolution; i++)
                    newVpt[i].Position.Y = scaledPointData[i];

                matrix.Add(newVpt);
            }

            for (int i = 0; i < numEntries; i++)
                for (int j = 0; j < resolution; j++)
                    matrix[i][j].Position.Z = Offset + i * matrixSegment + zOffset;
        }

        /// <summary>
        /// Renders the RLMatrix's vertices
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spirteBatch"></param>
        public void Render(GameTime gameTime, SpriteBatch spirteBatch)
        {
            for (int i = 0; i < numEntries; i++)
                for (int j = 0; j < resolution; j++)
                    columns[j][i] = matrix[i][j];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (VertexPositionTexture[] vtp in matrix)
                    game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vtp, 0, vtp.Length - 1);

                for (int i = 0; i < resolution; i++)
                    game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, columns[i], 0, numEntries - 1);
            }
        }

        /// <summary>
        /// Runs the processing pipeline on teh given point data
        /// </summary>
        /// <param name="pointData"></param>
        /// <param name="scale"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void Process(SpectrumBase.SpectrumPointData[] pointData, float scale, int min, int max)
        {
            CalculateRange(pointData, min, max, out double low, out double average);
            Normalize(pointData, scale, min, max, low, average);
            Balance(min, max);
        }

        /// <summary>
        /// Calculates the low boundary and average value of the given point data
        /// </summary>
        /// <param name="pointData"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="low"></param>
        /// <param name="average"></param>
        private void CalculateRange(SpectrumBase.SpectrumPointData[] pointData, int min, int max, out double low, out double average)
        {
            low = double.MaxValue;
            average = 0.0;

            for (int i = min; i < max; i++)
            {
                average += pointData[i].Value;

                if (low > pointData[i].Value)
                    low = pointData[i].Value;
            }

            average /= (max - min);
        }

        /// <summary>
        /// Normalizes the point data based on the amplitude and low boundary of the point data
        /// </summary>
        /// <param name="pointData"></param>
        /// <param name="scale"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="low"></param>
        /// <param name="average"></param>
        private void Normalize(SpectrumBase.SpectrumPointData[] pointData, float scale, int min, int max, double low, double average)
        {
            for (int i = min; i < max; i++)
                scaledPointData[i] = (float)(pointData[i].Value - low) * (float)average * scale;
        }

        /// <summary>
        /// Balances the scaled point data in the given range
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void Balance(int min, int max)
        {
            float avg = (scaledPointData[max - 1] + scaledPointData[min]) * 0.5f;

            for (int i = min; i < max; i++)
                scaledPointData[i] += avg * MathHelper.Lerp(-1f, 1f, (float)(i - min) / (max - min));
        }
    }
}
