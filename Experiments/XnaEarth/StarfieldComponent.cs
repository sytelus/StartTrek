//-----------------------------------------------------------------------------
// Copyright (c) 2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAEarth
{
    /// <summary>
    /// Internal vertex structure used by the StarfieldComponent for the 2D
    /// billboards used to represent the stars in the star field.
    /// </summary>
    struct StarVertex : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0)
        );

        public Vector4 Position;
        public Vector4 TexCoord;

        public StarVertex(float x, float y, float z, float w, float u, float v, float dx, float dy)
            : this(new Vector4(x, y, z, w), new Vector2(u, v), new Vector2(dx, dy))
        {

        }

        public StarVertex(Vector3 position, Vector2 texCoord, Vector2 offset)
            : this(new Vector4(position, 1.0f), texCoord, offset)
        {
        }
                
        public StarVertex(Vector4 position, Vector2 texCoord, Vector2 offset)
        {
            Position = position;

            // Coordinates for the texture map.
            TexCoord.X = texCoord.X;
            TexCoord.Y = texCoord.Y;

            // The 2D offset vector.
            TexCoord.Z = offset.X;
            TexCoord.W = offset.Y;
        }
                
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }


    /// <summary>
    /// An XNA component to generate and draw a randomly generated Starfield.
    /// </summary>
    public class StarfieldComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {       
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private Texture2D starTexture;
        private Effect effect;

        public Vector2 BillboardSize { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }
        
        public StarfieldComponent(Game game) : base(game)
        {
            BillboardSize = new Vector2(1.0f, 1.0f);
            ProjectionMatrix = Matrix.Identity;
            ViewMatrix = Matrix.Identity;
            WorldMatrix = Matrix.Identity;
        }

        public override void Draw(GameTime gameTime)
        {
            // Save current states.

            RasterizerState prevRasterizerState = GraphicsDevice.RasterizerState;
            BlendState prevBlendState = GraphicsDevice.BlendState;

            // First pass:
            // Render the non-transparent pixels of the billboards and store
            // their depths in the depth buffer.

            effect.Parameters["world"].SetValue(WorldMatrix);
            effect.Parameters["view"].SetValue(ViewMatrix);
            effect.Parameters["projection"].SetValue(ProjectionMatrix);
            effect.Parameters["billboardSize"].SetValue(BillboardSize);
            effect.Parameters["colorMap"].SetValue(starTexture);
            effect.Parameters["alphaTestDirection"].SetValue(1.0f);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            DrawStars();

            // Second pass:
            // Render the transparent pixels of the billboards.
            // Disable depth buffer writes to ensure that the depth values from
            // the first pass are used instead.

            effect.Parameters["alphaTestDirection"].SetValue(-1.0f);

            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            DrawStars();

            // Restore original states.

            GraphicsDevice.BlendState = prevBlendState;
            GraphicsDevice.RasterizerState = prevRasterizerState;
           
            base.Draw(gameTime);
        }

        public void Generate(int totalStars, float radius)
        {
            List<Vector3> starPositions = GenerateStars(totalStars, radius);
            GenerateGeometry(starPositions);
        }
                
        public override void Initialize()
        {
            base.Initialize();

            starTexture = Game.Content.Load<Texture2D>(@"Textures\star-small");
            
            effect = Game.Content.Load<Effect>(@"Effects\starfield");
            effect.CurrentTechnique = effect.Techniques["main"];
        }

        private void DrawStars()
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     0,
                                                     0,
                                                     vertexBuffer.VertexCount,
                                                     0,
                                                     (int)(vertexBuffer.VertexCount * 0.5f));
            }

            GraphicsDevice.Indices = null;
            GraphicsDevice.SetVertexBuffer(null);
        }
        

        private void GenerateGeometry(List<Vector3> starPositions)
        {
            int totalVertices = starPositions.Count * 4;

            if (totalVertices > short.MaxValue)
                GenerateGeometryWith32BitIndices(starPositions);
            else
                GenerateGeometryWith16BitIndices(starPositions);
        }

        private void GenerateGeometryWith32BitIndices(List<Vector3> stars)
        {
            List<StarVertex> vertices = new List<StarVertex>();
            List<int> indices = new List<int>();
            int baseIndex = 0;

            for (int i = 0; i < stars.Count; ++i)
            {
                vertices.Add(new StarVertex(stars[i], new Vector2(0.0f, 0.0f), new Vector2(-1.0f, 1.0f)));
                vertices.Add(new StarVertex(stars[i], new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f)));
                vertices.Add(new StarVertex(stars[i], new Vector2(0.0f, 1.0f), new Vector2(-1.0f, -1.0f)));
                vertices.Add(new StarVertex(stars[i], new Vector2(1.0f, 1.0f), new Vector2(1.0f, -1.0f)));

                baseIndex = i * 4;

                indices.Add(0 + baseIndex);
                indices.Add(1 + baseIndex);
                indices.Add(2 + baseIndex);
                indices.Add(2 + baseIndex);
                indices.Add(1 + baseIndex);
                indices.Add(3 + baseIndex);
            }

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(StarVertex), vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        private void GenerateGeometryWith16BitIndices(List<Vector3> stars)
        {
            List<StarVertex> vertices = new List<StarVertex>();
            List<short> indices = new List<short>();
            short baseIndex = (short)0;

            for (int i = 0; i < stars.Count; ++i)
            {
                vertices.Add(new StarVertex(stars[i], new Vector2(0.0f, 0.0f), new Vector2(-1.0f,  1.0f)));
                vertices.Add(new StarVertex(stars[i], new Vector2(1.0f, 0.0f), new Vector2( 1.0f,  1.0f)));
                vertices.Add(new StarVertex(stars[i], new Vector2(0.0f, 1.0f), new Vector2(-1.0f, -1.0f)));
                vertices.Add(new StarVertex(stars[i], new Vector2(1.0f, 1.0f), new Vector2( 1.0f, -1.0f)));

                baseIndex = (short)(i * 4);

                indices.Add((short)(0 + baseIndex));
                indices.Add((short)(1 + baseIndex));
                indices.Add((short)(2 + baseIndex));
                indices.Add((short)(2 + baseIndex));
                indices.Add((short)(1 + baseIndex));
                indices.Add((short)(3 + baseIndex));
            }

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(StarVertex), vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        /// <summary>
        /// The algorithm used to generate the Starfield comes from here:
        /// http://mathworld.wolfram.com/SpherePointPicking.html
        /// </summary>
        private List<Vector3> GenerateStars(int totalStars, float radius)
        {
            List<Vector3> vertices = new List<Vector3>();
            Random random = new Random(unchecked((int)DateTime.Now.Ticks));

            for (int i = 0; i < totalStars; ++i)
            {
                float theta = 2.0f * MathHelper.Pi * (float)random.NextDouble();
                float phi = (float)Math.Acos(2.0f * (float)random.NextDouble() - 1.0f);

                float x = radius * (float)Math.Cos(theta) * (float)Math.Sin(phi);
                float y = radius * (float)Math.Sin(theta) * (float)Math.Sin(phi);
                float z = radius * (float)Math.Cos(phi);

                vertices.Add(new Vector3(x, y, z));
            }

            return vertices;
        }
    }
}
