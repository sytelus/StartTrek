using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    public class Cube : Object3D
    {
        private Vector3 halfWidth;
        private float[] vertexMultipliers;
        private Color[] vertexColors;

        private VertexPositionColor[] objectVertices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        public static readonly float[] DefaultVertexMultipliers = new float[] {1,1,1,1,1,1,1,1};
        public static readonly Color[] DefaultVertexColors = new Color[] { Color.Black, Color.Red, Color.Yellow, Color.Green, Color.Blue, Color.Magenta, Color.White, Color.Cyan };

        public Cube(GraphicsDevice graphicsDevice, string name, Vector3 position, Vector3 bounds, float[] vertexMultipliers = null, Color[] vertexColors = null)
            : base(graphicsDevice, name, position, position, Vector3.Up)
        {
            this.Bounds = bounds;
            this.halfWidth = bounds/2;

            this.vertexMultipliers = vertexMultipliers ?? DefaultVertexMultipliers;
            this.vertexColors = vertexColors ?? DefaultVertexColors;

            BuildVertexBuffer();
        }

        private void BuildVertexBuffer()
        {
            objectVertices = GetObjectVertices();
            vertexBuffer = new VertexBuffer(this.GraphicsDevice, VertexPositionColor.VertexDeclaration, objectVertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(objectVertices);

            var objectIndices = GetObjectIndices();
            indexBuffer = new IndexBuffer(this.GraphicsDevice, IndexElementSize.SixteenBits, objectIndices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<UInt16>(objectIndices);
        }

        public override void Draw()
        {
            this.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            this.GraphicsDevice.Indices = indexBuffer;
            this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);

            base.Draw();
        }

        public override bool RequiresUpdate
        {
            get { return false; }
        }

        public override IEnumerable<VertexPositionColor> GetDebugVertices()
        {
            return objectVertices;
        }

        private VertexPositionColor[] GetObjectVertices()
        {
            var minPoint = -halfWidth;
            var maxPoint = halfWidth;

            var cubeVertices = new VertexPositionColor[8];

            cubeVertices[0].Position = new Vector3(minPoint.X, minPoint.Y, minPoint.Z);
            cubeVertices[1].Position = new Vector3(minPoint.X, minPoint.Y, maxPoint.Z);
            cubeVertices[2].Position = new Vector3(maxPoint.X, minPoint.Y, maxPoint.Z);
            cubeVertices[3].Position = new Vector3(maxPoint.X, minPoint.Y, minPoint.Z);
            cubeVertices[4].Position = new Vector3(minPoint.X, maxPoint.Y, minPoint.Z);
            cubeVertices[5].Position = new Vector3(minPoint.X, maxPoint.Y, maxPoint.Z);
            cubeVertices[6].Position = new Vector3(maxPoint.X, maxPoint.Y, maxPoint.Z);
            cubeVertices[7].Position = new Vector3(maxPoint.X, maxPoint.Y, minPoint.Z);

            for (var vertexIndex = 0; vertexIndex < cubeVertices.Length; vertexIndex++ )
            {
                cubeVertices[vertexIndex].Position = (cubeVertices[vertexIndex].Position * this.vertexMultipliers[vertexIndex]) + this.Position;
                cubeVertices[vertexIndex].Color = this.vertexColors[vertexIndex];
            }

            return cubeVertices;
        }


        private UInt16[] GetObjectIndices()
        {
            var cubeIndices = new UInt16[36];

            //bottom face
            cubeIndices[0] = 0;
            cubeIndices[1] = 2;
            cubeIndices[2] = 3;
            cubeIndices[3] = 0;
            cubeIndices[4] = 1;
            cubeIndices[5] = 2;

            //top face
            cubeIndices[6] = 4;
            cubeIndices[7] = 6;
            cubeIndices[8] = 5;
            cubeIndices[9] = 4;
            cubeIndices[10] = 7;
            cubeIndices[11] = 6;

            //front face
            cubeIndices[12] = 5;
            cubeIndices[13] = 2;
            cubeIndices[14] = 1;
            cubeIndices[15] = 5;
            cubeIndices[16] = 6;
            cubeIndices[17] = 2;

            //back face
            cubeIndices[18] = 0;
            cubeIndices[19] = 7;
            cubeIndices[20] = 4;
            cubeIndices[21] = 0;
            cubeIndices[22] = 3;
            cubeIndices[23] = 7;

            //left face
            cubeIndices[24] = 0;
            cubeIndices[25] = 4;
            cubeIndices[26] = 1;
            cubeIndices[27] = 1;
            cubeIndices[28] = 4;
            cubeIndices[29] = 5;

            //right face
            cubeIndices[30] = 2;
            cubeIndices[31] = 6;
            cubeIndices[32] = 3;
            cubeIndices[33] = 3;
            cubeIndices[34] = 6;
            cubeIndices[35] = 7;

            return cubeIndices;
        }


    }
}
