using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
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

        public static Vector3 Clone(this Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static bool IsValid(this Vector3 vector)
        {
            return !(double.IsNaN(vector.X) || double.IsNaN(vector.Y) || double.IsNaN(vector.Z)) ;
        }

        public static void Validate(this Vector3 vector)
        {
            if (!IsValid(vector))
                throw new Exception(string.Format("Vector {0} is not valid", vector.ToString()));
        }

        public static Vector3 GetZVector(this Vector3 vector)
        {
            return new Vector3(0, 0, vector.Z);
        }
        public static Vector3 GetYVector(this Vector3 vector)
        {
            return new Vector3(0, vector.Y, 0);
        }
        public static Vector3 GetXVector(this Vector3 vector)
        {
            return new Vector3(vector.X, 0, 0);
        }

        public static Vector3 GetWithNewLength(this Vector3 vector, float newLength)
        {
            var lengthFactor = newLength / vector.Length();
            return new Vector3(vector.X * lengthFactor, vector.Y * lengthFactor, vector.Z * lengthFactor);
        }

        public static float AngleWith(this Vector3 vector, Vector3 anotherVector, bool inRadiance = true)
        {
            const float closeTo1 = 1 - 1E-3f;

            var normalizedDotProduct = Vector3.Dot(vector, anotherVector)/vector.Length()/anotherVector.Length();
            if (normalizedDotProduct > closeTo1)   //normalized dot product can have bad rounding errors
                normalizedDotProduct = 1;
            else if (normalizedDotProduct < -closeTo1)
                normalizedDotProduct = -1;

            var angleInRadiance = (float) Math.Acos(normalizedDotProduct);
            if (inRadiance)
                return angleInRadiance;
            else
                return MathHelper.ToDegrees(angleInRadiance);
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

        public static Vector3 SafeCross(this Vector3 main, Vector3 crossWith, Vector3 alternateCrossWith)
        {
            var cross = Vector3.Cross(main, crossWith);
            if (!cross.IsValid() || cross.LengthSquared() < 1E-02)
                cross = Vector3.Cross(main, alternateCrossWith);

            return cross;
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

        public static void AddToTranslation(this Matrix matrix, Vector3 vector)
        {
            matrix.Translation += vector;
        }

        private static KnownColor[] knownColorNames = (KnownColor[])Enum.GetValues(typeof(System.Drawing.KnownColor));
        private static Random random = new Random(42);
        public static Microsoft.Xna.Framework.Color GetRandomColor()
        {
            var randomColorName = knownColorNames[random.Next(knownColorNames.Length)];
            var color = System.Drawing.Color.FromKnownColor(randomColorName);
            return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        }

        public static System.Drawing.Color ToSystemDrawingColor(this Microsoft.Xna.Framework.Color xnaColor)
        {
            return System.Drawing.Color.FromArgb(xnaColor.R, xnaColor.G, xnaColor.B, 0);
        }

        public static bool WithinEpsilon(float a, float b)
        {
            float num = a - b;
            return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
        }
    }
}
