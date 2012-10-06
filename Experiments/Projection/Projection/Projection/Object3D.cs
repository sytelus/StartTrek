using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    internal class Object3D
    {
        private GraphicsDevice device;
        private VertexBuffer vertexBuffer;
        private Vector3 spaceSize;


        public Object3D(GraphicsDevice device, Vector3 spaceSize)
        {
            this.device = device;
            this.spaceSize = spaceSize;
            BuildVertexBuffer();
        }

        private void BuildVertexBuffer()
        {
            var objectVertices = GetObjectVertices().ToArray();

            vertexBuffer = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, objectVertices.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData<VertexPositionColor>(objectVertices);
        }

        private IEnumerable<VertexPositionColor> GetObjectVertices()
        {
            var center = this.spaceSize/2;
            var halfLength = this.spaceSize/20;

            var faceVectors = new[]
                                    {
                                        new Vector3(center.X + halfLength.X, center.Y - halfLength.Y, center.Z - halfLength.Z), 
                                        new Vector3(center.X - halfLength.X, center.Y + halfLength.Y, center.Z - halfLength.Z), 
                                        new Vector3(center.X - halfLength.X, center.Y - halfLength.Y, center.Z - halfLength.Z), 

                                        new Vector3(center.X + halfLength.X, center.Y + halfLength.Y, center.Z - halfLength.Z), 
                                        new Vector3(center.X + halfLength.X, center.Y - halfLength.Y, center.Z - halfLength.Z), 
                                        new Vector3(center.X - halfLength.X, center.Y + halfLength.Y, center.Z - halfLength.Z),                                        

                                        new Vector3(center.X + halfLength.X, center.Y - halfLength.Y, center.Z + halfLength.Z), 
                                        new Vector3(center.X - halfLength.X, center.Y + halfLength.Y, center.Z + halfLength.Z), 
                                        new Vector3(center.X - halfLength.X, center.Y - halfLength.Y, center.Z + halfLength.Z), 

                                        new Vector3(center.X + halfLength.X, center.Y + halfLength.Y, center.Z + halfLength.Z), 
                                        new Vector3(center.X + halfLength.X, center.Y - halfLength.Y, center.Z + halfLength.Z), 
                                        new Vector3(center.X - halfLength.X, center.Y + halfLength.Y, center.Z + halfLength.Z)                                        
                                    };

            for(var rotateCount = 0; rotateCount < 3; rotateCount++)
            {
                var color = XnaExtentions.GetRandomColor();
                foreach (var faceVector in faceVectors)
                {
                    yield return new VertexPositionColor(faceVector.RotateComponent(rotateCount), color);
                }
            }

        }

        public void Draw(Camera camera, BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
            effect.World = Matrix.Identity;
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(vertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
            }
        }
    }
}