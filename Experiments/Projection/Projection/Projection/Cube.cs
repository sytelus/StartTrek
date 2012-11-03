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
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        public Cube(GraphicsDevice graphicsDevice, Vector3 position, Vector3 bounds)
            : base(graphicsDevice, position, position, Vector3.Up)
        {
            this.Bounds = bounds;
            this.halfWidth = bounds/2;
            BuildVertexBuffer();
        }

        private void BuildVertexBuffer()
        {
            var objectVertices = GetObjectVertices();
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
        }

        public override bool RequiresUpdate
        {
            get { return false; }
        }

        private VertexPositionColor[] GetObjectVertices()
        {
            var minPoint = this.Position - halfWidth;
            var maxPoint = this.Position + halfWidth;

            var cubeVertices = new VertexPositionColor[8];

            cubeVertices[0].Position = new Vector3(minPoint.X, minPoint.Y, minPoint.Z);
            cubeVertices[1].Position = new Vector3(minPoint.X, minPoint.Y, maxPoint.Z);
            cubeVertices[2].Position = new Vector3(maxPoint.X, minPoint.Y, maxPoint.Z);
            cubeVertices[3].Position = new Vector3(maxPoint.X, minPoint.Y, minPoint.Z);
            cubeVertices[4].Position = new Vector3(minPoint.X, maxPoint.Y, minPoint.Z);
            cubeVertices[5].Position = new Vector3(minPoint.X, maxPoint.Y, maxPoint.Z);
            cubeVertices[6].Position = new Vector3(maxPoint.X, maxPoint.Y, maxPoint.Z);
            cubeVertices[7].Position = new Vector3(maxPoint.X, maxPoint.Y, minPoint.Z);

            cubeVertices[0].Color = Color.Black;
            cubeVertices[1].Color = Color.Red;
            cubeVertices[2].Color = Color.Yellow;
            cubeVertices[3].Color = Color.Green;
            cubeVertices[4].Color = Color.Blue;
            cubeVertices[5].Color = Color.Magenta;
            cubeVertices[6].Color = Color.White;
            cubeVertices[7].Color = Color.Cyan;

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
