using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Microsoft.VisualBasic;
using System.IO;

[assembly: AssemblyVersion("1.0.0.1")]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            string svgpath = Interaction.InputBox("Where to save SVG?", "SVG Path", "");
            double divergenceScaling = double.Parse(Interaction.InputBox("Enter the ratio of the source to slot distance to the source to axis distance. For most machines, this is 0.95.", "SSD/SAD", "0.95"));
            foreach (Beam b in context.PlanSetup.Beams)
            {
                // Beams can technically handle more than one block per beam - I'm not having it.
                if (b.Blocks.Count() != 1) continue;

                double bx;
                double by;
                double t_wall = 2; // in mm

                GetBlockDims(b.Applicator.Id, out bx, out by);
                var badOutlines = b.Blocks.FirstOrDefault().Outline;
                var fixedOutlines = FixOutlines(badOutlines, divergenceScaling);

                ExportToSVG(IndexingPlate(bx, by, divergenceScaling, t_wall).Concat(fixedOutlines).ToArray(),
                            System.IO.Path.Combine(svgpath, $"{context.Patient.Id}-{context.PlanSetup.Id}-{b.Id}-Block.svg"),
                            300, 300);
                
                // The tallest the svg could possibly be is 25 cm * 10 mm/cm * 0.95 divergence scaling
                // + 2 * (12.5 mm for frame + 4mm for default metal insert in frame) + index thickness, t_wall, 2 mm
                // = 271.5. So, 300 is round and covers safely. 
            }
        }

        //public void PlotBlock(string applicator, Point[][] outline)
        //{
        //    var window = new Window();
        //    var canvas = new Canvas();
        //    window.Content = canvas;

        //    double block_x;
        //    double block_y;

        //    GetBlockDims(applicator, out block_x, out block_y);

        //    double plotScale = 2.0;

        //    window.Width = block_x * 10 * plotScale * 1.1;
        //    window.Height = block_y * 10 * plotScale * 1.1;
        //    // cm -> mm -> plot scaling -> padding.

        //    // First set of polylines from outline
        //    foreach (var aperture in outline)
        //    {
        //        var aperturePolyLine = new Polyline
        //        {
        //            Stroke = Brushes.Red,
        //            StrokeThickness = 2
        //        };

        //        foreach (var p in aperture)
        //        {
        //            aperturePolyLine.Points.Add(new System.Windows.Point(
        //                plotScale * (p.X + (window.Width / 2)),
        //                plotScale * ((window.Height / 2) - p.Y)
        //            ));
        //        }
        //        // close the polyline:
        //        aperturePolyLine.Points.Add(new System.Windows.Point(
        //                plotScale * (aperture[0].X + (window.Width / 2)),
        //                plotScale * ((window.Height / 2) - aperture[0].Y)
        //            ));
        //        canvas.Children.Add(aperturePolyLine);
        //    }

        //    // Second polyline from block dimensions
        //    var blockEdgePolyline = new Polyline
        //    {
        //        Stroke = Brushes.Blue,
        //        StrokeThickness = 2
        //    };

        //    foreach (var p in BlockDimsToPoints(block_x, block_y))
        //    {
        //        blockEdgePolyline.Points.Add(new System.Windows.Point(
        //            plotScale * (p.X + (window.Width / 2)),
        //            plotScale * ((window.Height / 2) - p.Y)
        //        ));
        //    }

        //    canvas.Children.Add(blockEdgePolyline);

        //    window.ShowDialog();
        //}

        /// <summary>
        /// A method that converts the block ID string into dimensions of the block, as double.
        /// Is this overkill? Could it have been regex? Yes and yes. However - this is far more
        /// readable, extensible, and maintainable.
        /// </summary>
        /// <param name="applicator">String ID of the applicator.</param>
        /// <param name="block_x">Nominal block width, at isocenter.</param>
        /// <param name="block_y">Nominal block height, at isocenter.</param>
        public void GetBlockDims(string applicator, out double block_x, out double block_y)
        {
            switch (applicator)
            {
                case "A06": block_x = block_y = 6; break;
                case "A10": block_x = block_y = 10; break;
                case "A15": block_x = block_y = 15; break;
                case "A20": block_x = block_y = 20; break;
                case "A25": block_x = block_y = 25; break;
                case "A10X6": block_x = 10; block_y = 6; break; // I have not tested the 10x6 case, as we do not use it at my clinic presently.

                default:
                    MessageBox.Show($"I don't recognize your applicator called {applicator}. Please enter dimensions in cm:");
                    block_x = double.Parse(Interaction.InputBox("Enter X value:", "Custom Size", "0"));
                    block_y = double.Parse(Interaction.InputBox("Enter Y value:", "Custom Size", "0"));
                    break;
            }
        }

        /// <summary>
        /// Builds the indexing plate Point[][] array to which the apertures will be appended at export.
        /// </summary>
        /// <param name="x_cm">Nominal width of cutout, in cm, at iso.</param>
        /// <param name="y_cm">Nominal height of cutout, in cm, at iso.</param>
        /// <param name="divergenceScaling">Scaling factor to account for block being at reduced SSD vs isocenter. Defined as SSD (source slot distance) / SAD. </param>
        /// <param name="t_wall">The vertical thickness of the indexing arms - 2mm works.</param>
        /// <returns></returns>
        public Point[][] IndexingPlate(double x_cm, double y_cm, double divergenceScaling, double t_wall)
        {
            // the frame is 12.5 mm cross-plane wide...
            // then there's 3 mm of remaining metal (4 mm if it's 25 cone).
            // then you multiply by 2 for both sides. learned that the hard way. doh.
            // slot is 91 mm wide unless 6 cone, in which case it's 53 mm.
            double metalMargin_mm = x_cm == 25.0 ? 4.0 : 3.0;
            double slotwidth_mm = x_cm == 6.0 ? 53.0 : 91.0;

            double x_mm = x_cm * 10 * divergenceScaling + 2 * (12.5 + metalMargin_mm);
            double y_mm = y_cm * 10 * divergenceScaling + 2 * (12.5 + metalMargin_mm);

            double hx = x_mm / 2;
            double hy = y_mm / 2;

            double halfSlot = slotwidth_mm / 2; 

            Point[] plate =
            {
                new Point(-hx,  hy),
                new Point( hx,  hy),
                new Point( hx, -hy),
                new Point(-hx, -hy),
                new Point(-hx,  hy)
            };

            Point[] leftWall =
            {
                new Point(-hx,        hy + t_wall),
                new Point(-halfSlot,  hy + t_wall),
                new Point(-halfSlot,  hy),
                new Point(-hx,        hy),
                new Point(-hx,        hy + t_wall)
            };

            Point[] rightWall =
            {
                new Point( halfSlot,  hy + t_wall),
                new Point( hx,        hy + t_wall),
                new Point( hx,        hy),
                new Point( halfSlot,  hy),
                new Point( halfSlot,  hy + t_wall)
            };

            return new Point[][]
            {
                plate,
                leftWall,
                rightWall
            };
        }

        /// <summary>
        /// Exports contours to SVG. Basically a kitchen drawer method that does string algebra.
        /// </summary>
        /// <param name="contours">An array of Point[]. Each Point[] is a single aperture, indexing arm, or the print base.</param>
        /// <param name="filename">Where to save.</param>
        /// <param name="canvasWidth">Canvas width, in mm. 300 is enough.</param>
        /// <param name="canvasHeight">Canvas height, in mm. 300 is enough.</param>
        public static void ExportToSVG(Point[][] contours, string filename, double canvasWidth, double canvasHeight)
        {
            var sb = new StringBuilder();

            // evil header
            sb.AppendLine($"<svg xmlns='http://www.w3.org/2000/svg' width='{canvasWidth}mm' height='{canvasHeight}mm' viewBox='0 0 {canvasWidth} {canvasHeight}'>");

            // Translate to center 
            // Flip Y axis (SVG origin is top left)
            // but Eclipse has different plans....
            sb.AppendLine("<g transform='translate(" + (canvasWidth / 2) + "," + (canvasHeight / 2) + ") scale(1, -1)'>");

            foreach (var contour in contours)
            {
                string pointsStr = string.Join(" ", contour.Select(p => $"{p.X},{p.Y}"));
                sb.AppendLine($"  <polyline points='{pointsStr}' stroke='red' stroke-width='1' fill='none' />");
            }

            sb.AppendLine("</g>");
            sb.AppendLine("</svg>");

            File.WriteAllText(filename, sb.ToString());
        }

        /// <summary>
        /// Appends the first point to the end of the point array, if it's not already there, to ensure closedness of the polyline.
        /// Also, Eclipse returns cutout outlines projected to isocenter, meaning we need to undo that divergence
        /// to make the cutouts the right size at the slot (nominally 95 cm on Varian machines). 
        /// </summary>
        public static Point[][] FixOutlines(Point[][] contours, double divergenceScaling)
        {
            Point[][] closed = new Point[contours.Length][];

            for (int i = 0; i < contours.Length; i++)
            {
                // setup and allocate for new outlines
                var contour = contours[i];

                bool isClosed =
                    contour.Length > 1 &&
                    contour[0].X == contour[contour.Length - 1].X &&
                    contour[0].Y == contour[contour.Length - 1].Y;

                int newLength = isClosed ? contour.Length : contour.Length + 1;

                Point[] newContour = new Point[newLength];

                // Copy original points and scale
                for (int j = 0; j < contour.Length; j++)
                {
                    newContour[j] = new Point(
                        contour[j].X * divergenceScaling,
                        contour[j].Y * divergenceScaling
                    );
                }

                // Add closing point if needed and scale
                if (!isClosed)
                {
                    newContour[newLength - 1] = new Point(
                        contour[0].X * divergenceScaling,
                        contour[0].Y * divergenceScaling
                    );
                }

                closed[i] = newContour;
            }

            return closed;
        }
    }
}
