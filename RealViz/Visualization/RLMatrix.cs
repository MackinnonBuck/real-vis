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
    public class RLMatrix
    {
        const float Offset = -30;
        const float MatrixRange = 60f;
        const float MatrixSpan = 26f;
        const float LaneSize = 15f;
        const float LaneDamping = 0.25f;
        const float WaveScale = 4f;

        readonly MainGame mainGame;
        readonly CircularBuffer<VertexPositionTexture[]> matrix;
        readonly VertexPositionTexture[][] columns;
        readonly int entryResolution;
        readonly int entryCount;
        readonly BasicEffect effect;
        readonly float matrixSegment;

        float zOffset;
        float[] scaledPointData;

        public RLMatrix(MainGame game, int count, int resolution)
        {
            mainGame = game;
            entryResolution = resolution;
            entryCount = count;
            matrix = new CircularBuffer<VertexPositionTexture[]>(count);

            for (int i = 0; i < count; i++)
            {
                VertexPositionTexture[] vpt = new VertexPositionTexture[resolution];

                for (int j = 0; j < resolution; j++)
                    vpt[j].Position = new Vector3(MatrixSpan * -0.5f + j * (MatrixSpan / (resolution - 1)), 0f, 0f);

                matrix.Add(vpt);
            }

            columns = new VertexPositionTexture[resolution][];

            for (int i = 0; i < resolution; i++)
                columns[i] = new VertexPositionTexture[count];

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

            matrixSegment = MatrixRange / count;

            zOffset = 0f;

            scaledPointData = new float[resolution];
        }

        public void Update(GameTime gameTime, float speed, SpectrumBase.SpectrumPointData[] pointData)
        {
            while (zOffset > matrixSegment)
            {
                VertexPositionTexture[] newVpt = matrix.Back;

                for (int i = 0; i < entryResolution; i++)
                    newVpt[i].Position.Y = Math.Max(scaledPointData[i] * 5f *
                        Math.Min(LaneSize, Math.Abs(i - (float)(entryResolution - 1) / 2)) / LaneSize - LaneDamping, 0);

                matrix.Add(newVpt);

                zOffset -= matrixSegment;
            }

            double lowerMin = double.MaxValue;

            for (int i = 0; i < entryResolution / 2; i++)
                if (lowerMin > pointData[i].Value)
                    lowerMin = pointData[i].Value;

            for (int i = 0; i < entryResolution / 2; i++)
                scaledPointData[i] = (float)(pointData[i].Value - lowerMin) * WaveScale;

            double upperMin = double.MaxValue;

            for (int i = entryResolution / 2; i < entryResolution; i++)
                if (upperMin > pointData[i].Value)
                    upperMin = pointData[i].Value;

            for (int i = entryResolution / 2; i < entryResolution; i++)
                scaledPointData[i] = (float)(pointData[i].Value - upperMin) * WaveScale;

            zOffset += gameTime.ElapsedGameTime.Milliseconds * 0.001f * speed;

            for (int i = 0; i < entryCount; i++)
                for (int j = 0; j < entryResolution; j++)
                    matrix[i][j].Position.Z = Offset + i * matrixSegment + zOffset; 
        }

        public void Render(GameTime gameTime, SpriteBatch spirteBatch)
        {
            for (int i = 0; i < entryCount; i++)
                for (int j = 0; j < entryResolution; j++)
                    columns[j][i] = matrix[i][j];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (VertexPositionTexture[] vtp in matrix)
                    mainGame.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vtp, 0, vtp.Length - 1);

                for (int i = 0; i < entryResolution; i++)
                    mainGame.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, columns[i], 0, entryCount - 1);
            }
        }
    }
}
