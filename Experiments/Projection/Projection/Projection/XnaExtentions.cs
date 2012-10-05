using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    public static class XnaExtentions
    {
        public static float Components(this Vector3 vector, int i)
        {
            switch (i)
            {
                case 0:
                    return vector.X;
                case 1:
                    return vector.Y;
                case 2:
                    return vector.Z;
                default:
                    throw new ArgumentOutOfRangeException("i", "Vector3 component index must be from 0 to 2");
            }
        }

        public static Vector3 RotateComponent(this Vector3 vector, int rotateCount)
        {
            switch (rotateCount % 3)
            {
                case 0:
                    return vector;
                case 1:
                    return new Vector3(vector.Z, vector.X, vector.Y);
                case 2:
                    return new Vector3(vector.Y, vector.Z, vector.X);
                default:
                    return vector;
            }
        }

        public static int Dimensions(this Vector3 vector, int i)
        {
            return 3;
        }

        public static Vector3 CreateVertex3FromComponents(float[] components)
        {
            return new Vector3(components[0], components[1], components[2]);
        }

        public static IEnumerable<VertexPositionColor> ReverseFacedTriangles(this IEnumerable<VertexPositionColor> vertices)
        {
            VertexPositionColor[] triangle = new VertexPositionColor[3];
            int vertexCount = 0;
            foreach (var vertex in vertices)
            {
                if (vertexCount < 3)
                    triangle[vertexCount++] = vertex;
                else
                {
                    yield return triangle[2];
                    yield return triangle[1];
                    yield return triangle[0];
                    vertexCount = 0;
                }
            }

            if (vertexCount % 3 != 0)
                throw new ArgumentException("Numbers of vertexes were not in multiple of 3");
        }


        private static KnownColor[] knownColorNames = (KnownColor[])Enum.GetValues(typeof(System.Drawing.KnownColor));
        private static Random random = new Random(42);
        internal static Microsoft.Xna.Framework.Color GetRandomColor()
        {
            var randomColorName = knownColorNames[random.Next(knownColorNames.Length)];
            var color = System.Drawing.Color.FromKnownColor(randomColorName);
            return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        }
    }
}
