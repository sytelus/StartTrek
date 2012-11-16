using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;

namespace Projection
{
    public partial class AuxForm : Form
    {
        public AuxForm()
        {
            InitializeComponent();
        }


        public void DrawDebugPoints(Scene scene, Camera camera, Matrix world, Matrix view, Matrix projection, float viewPortWidth, float viewPortHeight)
        {
            this.Height = (int)(viewPortHeight * 1.2);
            this.Width = (int) (viewPortWidth * 1.2);

            graphicsContext.Clear(Color.CornflowerBlue);

            var writeToCaptureFile = captureStreamWriter != null && UpdateLastCameraVectors(camera);

            StringBuilder line = null;
            if (writeToCaptureFile)
            {
                line = new StringBuilder();
                line.Append(camera.Position.X);
                line.Append("\t");
                line.Append(camera.Position.Y);
                line.Append("\t");
                line.Append(camera.Position.Z);
                line.Append("\t");
                line.Append(camera.Up.X);
                line.Append("\t");
                line.Append(camera.Up.Y);
                line.Append("\t");
                line.Append(camera.Up.Z);
                line.Append("\t");
                line.Append(camera.Forward.X);
                line.Append("\t");
                line.Append(camera.Forward.Y);
                line.Append("\t");
                line.Append(camera.Forward.Z);
            }

            foreach (var object3D in scene.Objects)
            {
                foreach (var debugVertex in object3D.GetDebugVertices())
                {
                    Matrix matrix = Matrix.Multiply(Matrix.Multiply(world, view), projection);
                    Vector3 vector = Vector3.Transform(debugVertex.Position, matrix);
                    float a = (((debugVertex.Position.X * matrix.M14) + (debugVertex.Position.Y * matrix.M24)) + (debugVertex.Position.Z * matrix.M34)) + matrix.M44;
                    if (!XnaExtentions.WithinEpsilon(a, 1f))
                        vector = (Vector3)(vector / a);

                    var x = (((vector.X + 1f)*0.5f)*this.Width);
                    var y = (((-vector.Y + 1f)*0.5f)*this.Height);

                    //var vertexColor = debugVertex.Color.ToSystemDrawingColor();
                    //if (vertexColor == Color.CornflowerBlue)
                    var    vertexColor = Color.Red;

                    graphicsContext.FillEllipse(new SolidBrush(vertexColor), new RectangleF(x, y, 8, 8));

                    if (writeToCaptureFile)
                    {
                        line.Append("\t");
                        line.Append(x);
                        line.Append("\t");
                        line.Append(y);
                    }
                }
            }

            if (writeToCaptureFile)
            {
                captureStreamWriter.WriteLine(line);
            }
        }

        private Vector3 lastPosition, lastUp, lastForward;
        private bool UpdateLastCameraVectors(Camera camera)
        {
            if (camera.Position != lastPosition || camera.Up != lastUp || camera.Forward != lastForward)
            {
                lastPosition = camera.Position;
                lastUp = camera.Up;
                lastForward = camera.Forward;

                return true;
            }

            return false;
        }

        private Graphics graphicsContext;
        private void AuxForm_Load(object sender, EventArgs e)
        {
            graphicsContext = pictureBox1.CreateGraphics();
        }

        private StreamWriter captureStreamWriter;
        private void checkBoxEnableSave_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableSave.Checked)
                captureStreamWriter = File.CreateText(textBoxSaveFilePath.Text);
            else
            {
                CloseCaptureStream();
            }
        }

        private void CloseCaptureStream()
        {
            if (captureStreamWriter != null)
                captureStreamWriter.Close();

            captureStreamWriter = null;
        }

        private void AuxForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseCaptureStream();
            if (graphicsContext != null)
                graphicsContext.Dispose();
        }
    }
}
