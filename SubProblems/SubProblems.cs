/*
 * This code is not intended to be executed!
 * The purpose of this file is:
 *  a. To breakdown the full 3D scanning problem into its component parts. 
 *  b. To link it up with references, notes etc.
 *  c. To communicate precisely. (given that I'm yet to grasp the jargons)
 *  d. List untested assumptions.
 * Each method is a subproblem to solve. Pay close attention to the arguments it needs (and more importantly, the arguments that it does NOT need) 
 * (Note that all experimentation will most likely happen in octave.)
 */

using System.Collections.Generic;
using System.Linq;

namespace SubProblems
{
    class Point3D
    {
        double x, y, z;
    }

    class Point2D
    {
        double x, y;
    }

    class CameraOrientation
    {
        Point3D position;
        Point3D viewVector;
    }

    class Image2D {/*...*/}

    class Mesh3D{/*....*/}

    //Configuration and methods to turn a 3D point into 2D pixel corodinates
    //Reference: http://szeliski.org/Book/ Section 2.1.5
    class CameraModel
    {
        //captures information about focal length, 
        //pixel spacing on the image sensor and position and orientation of the sensor w.r.t the camera projection point
        private float[][] CalibratoinMatrix;

        public Point2D ProjectPoint(Point3D worldPoint)
        {
            // return matrix product of CalibratoinMatrix and worldPoint
            return null;
        }
    }

    class Scanner3D
    {

        //Main flow of the full 3D scanning problem
        //This is one way of breaking the problem down. They may be other (better) ways
        static int Main(string[] args)
        {
            CameraModel model = new CameraModel(/*Loaded from device dependent configuration.*/);

            List<KeyValuePair<Image2D, CameraOrientation>> imageStream = FetchImageStream();

            List<Point3D> pointCloud = ImageStreamTo3DPointCloud(imageStream, model);

            //Optional part
            Mesh3D mesh = PointCloudTo3DMesh(pointCloud);

            return 0;
        }

        private static List<KeyValuePair<Image2D,CameraOrientation>> FetchImageStream()
        {
 	        //Device dependent code that reads a stream of images and corresponding camera positions.
            //Untested assumption: We can get this from a phone camera.
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageStream">The sequence of images captured from the camera along with the camera positions at the time of capture.</param>
        /// <param name="cameraModel"></param>
        /// <returns></returns>
        static List<Point3D> ImageStreamTo3DPointCloud(List<KeyValuePair<Image2D, CameraOrientation>> imageStream, CameraModel cameraModel)
        {
            /*
             * General idea: The given *sequence* of images is aligned. Note: Alignment is NOT the same as image stiching. We don't have to stich the images. 
             * We just need to understand "hey... this pixel(5,10) in this image corresponds to the pixel(100, 103) in the other image"
             * Definition of correspondence: If pixel A in image 1 CORRESPONDS to pixel B in image 2, it means that there is a single 3D point in the real world of which A and B are the "pictures".
             * 
             * Once the images are aligned, for every world point, we have the views from different positions, we derive the 3D point from this information.
             * 
             * Note: I'm decoupling the alignment and 3d reconstruction phases for the sake of clarity. It is quite possible that the ideal solution would do these 2 in a single step by optimizing them jointly.
             */

            /*
            alignment = mapping from worldPoint ID to <imageID, Pixel> pairs
            e.g. alignment[5] =  [
                                    {1, (5,8)}, 
             *                      {2, (6,9)},
             *                      {3, (7,9)}
                                 ] 
             * means that a point in the 3D world with the ID 5 corresponds to pixel (5,8) in the first image, (6,9) in the second image and (7,9) in the third image.
             */
            List<List<KeyValuePair<int, Point2D>>> alignment =  AlignImages(imageStream.Select(ci => ci.Key));

            List<Point3D> pointCloud = new List<Point3D>();
            
            //Now, for each of the world points, since we have stereo images, we can derive the 3D points
            //Again, note how each point is solved for in isolation, ideally, we might do this jointly.
            for (int realWorldPointID = 0; realWorldPointID < alignment.Count; realWorldPointID++ )
            {
                Point3D worldPoint = StereoToPoint3D(alignment[realWorldPointID].Select(kvp => new KeyValuePair<Point2D, CameraOrientation>(kvp.Value, imageStream[kvp.Key].Value)), cameraModel);
                pointCloud.Add(worldPoint);
            }
            return pointCloud;
        }

        private static Point3D StereoToPoint3D(IEnumerable<KeyValuePair<Point2D, CameraOrientation>> stereoImages, CameraModel cameraModel)
        {
            //This is a straightforward problem to solve once we know the cameraModel
            //Basically, each data point in stereoImages constrains the position of the 3D point to a line,
            //the point to return is the intersection of all the lines
            //ideally, we need only 2 images from different positions to compute the 3D point

            //This is the problem that we should try to solve first!
           
            return null;
        }

        private static List<List<KeyValuePair<int, Point2D>>> AlignImages(IEnumerable<Image2D> imageStream)
        {
 	        //This looks like the hardest part to solve
            //However, since the image is a "stream" as opposed to an unordered set of images, the problem is most likely tractable
            //Note that we have no use for the cameraModel or CameraOrientation here. 
            //I can imagine some ways in which the change in camera orientation between consecutive images could be leveraged. But lets give the problem a shot without using this information, for now.
            return null;

        }

        static Mesh3D PointCloudTo3DMesh(List<Point3D> pointCloud3D)
        {
            /*
             * Wether or not we need to solve this problem depends on what we intend to do with the scanned object.
             * A dense 3D point-cloud should suffice to describe any arbitrary 3D shape accurately.
             * But turning this point cloud into higher level abstractions like planes, curved surfaces etc. will be non-trivial.
             * (since it is basically a "modelling" problem.)
             */
            return null;
        }

    }
}
