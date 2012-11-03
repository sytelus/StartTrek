using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using System.IO;

namespace InferWorld
{
    /// <summary>
    /// Represents a projection matrix and the corresponding projected point.
    /// </summary>
    public class ProjectedPoint
    {
        //3 X 4 matrix that premultiplies the world point [X, Y, Z, W] to create the 2D [X Y W]
        public DenseMatrix worldToImage { get; set; }

        //3 X 1 matrix - [X, Y, W]
        public DenseMatrix projectedPoint { get; set; }
    }


    /// <summary>
    /// Methods are inferring the world point from a set of projections.
    /// They are static because I intend to use them as delegates.
    /// We will have multiple implementations that solve the same problem.
    /// </summary>
    public static class InferWorld
    {
        static void Main(string[] args)
        {
            ShowErrorInfer3DExactLMS(100, 0.01, "./NumProjections.Vs.SquaredError_0.01.txt");
        }

        /// <summary>
        /// Uses Infer3DExactLMS to infer a random world point from a number of different projections of the point.
        /// It writes out the squared errors corresponding to different number of projections into a file.
        /// </summary>
        /// <param name="numProjections">the max number of projections to use.</param>
        /// <param name="gaussianNoiseSigma">the standard deviation of the gaussian noise to be added to the projected points.</param>
        public static void ShowErrorInfer3DExactLMS(int numProjections, double gaussianNoiseSigma, string fileName)
        {
            ContinuousUniform dist = new ContinuousUniform(0, 1);
            Normal gaussianNoise = new Normal(0, gaussianNoiseSigma);
            DenseMatrix worldPoint = new DenseMatrix(4,1);
            worldPoint = (DenseMatrix) worldPoint.Random(4,1, dist);
            ProjectedPoint[] projections = new ProjectedPoint[numProjections];

            for (int i = 0; i < projections.Length; i++)
            {
                projections[i] = new ProjectedPoint();
                projections[i].worldToImage = new DenseMatrix(3, 4);
                projections[i].worldToImage = (DenseMatrix)projections[i].worldToImage.Random(3, 4, dist);
                projections[i].projectedPoint = (projections[i].worldToImage * worldPoint);
                projections[i].projectedPoint += (DenseMatrix)projections[i].projectedPoint.Random(3, 1, gaussianNoise);
            }

            File.WriteAllLines(fileName,
                Enumerable.Range(2, numProjections)
                    .Select(i => String.Format("{0}\t{1}", i, (worldPoint - Infer3DExactLMS(projections.Take(i))).L2Norm())));

        }

        /// <summary>
        /// Infer the 3D point using an exact method given its multiple projections.
        /// Note that the projection can, in theory be made with multiple cameras from multiple positions.
        /// (since is a side effect of rolling M and V into one matrix)
        /// </summary>
        /// <param name="projections">the list of projected points of soe world point.</param>
        /// <returns>A 4X1 matrix - the inferred world point.</returns>
        public static DenseMatrix Infer3DExactLMS(IEnumerable<ProjectedPoint> projections)
        {
            DenseMatrix X = projections.Select(p => p.worldToImage).CombineRowwise();
            DenseMatrix Y = projections.Select(p => p.projectedPoint).CombineRowwise();
            DenseMatrix XTranspose = (DenseMatrix)X.Transpose();
            //http://cs229.stanford.edu/notes/cs229-notes1.pdf section 2.2
            //Closed form to find the exact solution to minimize the squared error
            return (DenseMatrix)(XTranspose * X).Inverse() * XTranspose * Y;
        }

        /// <summary>
        /// Combines a set of matrices by stacking them up rowwise.
        /// The matrices are expected to be of identical size
        /// </summary>
        /// <param name="matrices"></param>
        /// <returns>the combined matrix</returns>
        private static DenseMatrix CombineRowwise(this IEnumerable<DenseMatrix> matrices)
        {
            return
                matrices
                .Select((m, i) => new { index = i, matrix = m })
                .Aggregate(
                    new DenseMatrix(matrices.Count() * matrices.First().RowCount, matrices.First().ColumnCount),
                    (a, n) =>
                    {
                        a.SetSubMatrix(n.index * n.matrix.RowCount, n.matrix.RowCount, 0, n.matrix.ColumnCount, n.matrix);
                        return a;
                    });
        }
    }

}
