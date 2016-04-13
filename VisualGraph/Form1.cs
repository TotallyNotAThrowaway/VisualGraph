using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualGraph
{
    public partial class Form1 : Form
    {
        private BufferedGraphicsContext context;
        private BufferedGraphics grafx;
        private List<Point> Vertexes = new List<Point>();
        private bool drawing = false;
        private Point MousePos;
        private PointF MouseSpeed;
        private List<SplineNode> Nodes = new List<SplineNode>();
        private List<FullSpline> Splines = new List<FullSpline>();
        private int ClickedID = -1;
        private int ClickedVertexID = -1;
        const int radius = 50;
        bool DrawDebug = false;
        private int VertexID = -1;
        const int lockRadius = 15;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (SplineNode dot in Nodes)
            {
                dot.Update();
            }
            DrawToBuffer(grafx.Graphics);
            grafx.Render(Graphics.FromHwnd(this.Handle));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            grafx = context.Allocate(this.CreateGraphics(),
                 new Rectangle(0, 0, this.Width, this.Height));
            DrawToBuffer(grafx.Graphics);
        }

        private void DrawToBuffer(Graphics g)
        {
            g.Clear(Color.White);
            int i = 0;

            
            foreach (FullSpline spln in Splines)
            {
                spln.Draw(g, (i++ == ClickedID) ||
                            ((ClickedVertexID > -1) 
                              && 
                              ((spln.Vertexes[0] == Vertexes[ClickedVertexID]) || (spln.Vertexes[spln.Vertexes.Count - 1] == Vertexes[ClickedVertexID]))));
            }
            i = 0;

            Pen VertexPen = new Pen(Color.DarkGreen);
            VertexPen.Width = 6;
            foreach (Point dot in Vertexes)
            {
                g.DrawEllipse(VertexPen, dot.X - 3, dot.Y - 3, 6, 6);
            }
            if (ClickedVertexID > -1)
            {
                VertexPen.Width = 2;
                VertexPen.Color = Color.DarkCyan;
                g.DrawEllipse(VertexPen, Vertexes[ClickedVertexID].X - 9, Vertexes[ClickedVertexID].Y - 9, 18, 18);
            }            
            if (VertexID > -1)
            {
                g.DrawEllipse(System.Drawing.Pens.Blue, Vertexes[VertexID].X - lockRadius,
                                                         Vertexes[VertexID].Y - lockRadius,
                                                         lockRadius * 2, lockRadius * 2);
            }

            if (drawing)
            {

                i = 0;
                int j = 0;
                Point[] Spline = new Point[Nodes.Count * 3 + 1];
                foreach (SplineNode dot in Nodes)
                {

                    if (DrawDebug)
                    {
                        g.DrawEllipse(System.Drawing.Pens.Red,   dot.Constraint.X - 2, dot.Constraint.Y - 2, 4, 4);
                        g.DrawEllipse(System.Drawing.Pens.Black, dot.Position.X - 2  , dot.Position.Y   - 2, 4, 4);
                        g.DrawEllipse(System.Drawing.Pens.Blue,  dot.LeftLean.X - 2  , dot.LeftLean.Y   - 2, 4, 4);
                        g.DrawEllipse(System.Drawing.Pens.Blue,  dot.RightLean.X - 2 , dot.RightLean.Y  - 2, 4, 4);

                    }

                    if (i > 0)
                    {
                        Spline[j++] = dot.LeftLean;
                    }
                    Spline[j++] = dot.Position;
                    Spline[j++] = dot.RightLean;
                    i++;
                }
                Spline[j  ].X = MousePos.X - (int)(MouseSpeed.X * SplineNode.nodeForce);
                Spline[j++].Y = MousePos.Y - (int)(MouseSpeed.Y * SplineNode.nodeForce);
                Spline[j++] = MousePos;
                g.DrawBeziers(System.Drawing.Pens.DarkGreen, Spline);


                if (DrawDebug)
                {
                    g.DrawEllipse(System.Drawing.Pens.Green, 
                                    Nodes[Nodes.Count - 1].Constraint.X - radius,
                                    Nodes[Nodes.Count - 1].Constraint.Y - radius,
                                    radius * 2, radius * 2);

                }
                if (Nodes.Count > 1)
                {
                    if (Math.Pow(Nodes[Nodes.Count - 2].Constraint.X - MousePos.X, 2) +
                        Math.Pow(Nodes[Nodes.Count - 2].Constraint.Y - MousePos.Y, 2) < Math.Pow(radius, 2))
                    {
                        Nodes.Remove(Nodes[Nodes.Count - 1]);
                    }
                }
                if (Math.Pow(Nodes[Nodes.Count - 1].Constraint.X - MousePos.X, 2) +
                    Math.Pow(Nodes[Nodes.Count - 1].Constraint.Y - MousePos.Y, 2) > Math.Pow(radius, 2))
                {
                    Nodes.Add(new SplineNode(MousePos, MouseSpeed));
                }
            }

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            grafx.Render(e.Graphics);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            try
            {
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                if (grafx != null)
                {
                    grafx.Dispose();
                    grafx = null;
                }
                grafx = context.Allocate(this.CreateGraphics(),
                    new Rectangle(0, 0, this.Width, this.Height));
                DrawToBuffer(grafx.Graphics);
                this.Refresh();

            }
            catch (Exception ex) { }
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (VertexID == -1))
            {
                Vertexes.Add(new Point(e.X, e.Y));
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (VertexID > -1))
            {
                drawing = true;
                MouseSpeed.X = e.X - MousePos.X;
                MouseSpeed.Y = e.Y - MousePos.Y;
                if (e.Location != MousePos)
                {
                    double spd = Math.Sqrt(Math.Pow(MouseSpeed.X, 2) + Math.Pow(MouseSpeed.Y, 2));
                    MouseSpeed.X /= (float)spd;
                    MouseSpeed.Y /= (float)spd;

                }
                MousePos = e.Location;
                Nodes.Add(new SplineNode(Vertexes[VertexID], MouseSpeed, true));
            }
            else if (e.Button == MouseButtons.Middle)
            {
                DrawDebug = !DrawDebug;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            MouseSpeed.X = e.X - MousePos.X;
            MouseSpeed.Y = e.Y - MousePos.Y;
            if (e.Location != MousePos)
            {
                double spd = Math.Sqrt(Math.Pow(MouseSpeed.X, 2) + Math.Pow(MouseSpeed.Y, 2));
                MouseSpeed.X /= (float)spd;
                MouseSpeed.Y /= (float)spd;

            }
            MousePos = e.Location;
            int i = 0;
            VertexID = -1;
            foreach (Point dot in Vertexes)
            {
                if (Math.Pow(dot.X - MousePos.X, 2) + Math.Pow(dot.Y - MousePos.Y, 2) < Math.Pow(lockRadius, 2))
                {
                    VertexID = i;
                }
                i++;
            }        
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drawing = false;
                MouseSpeed.X = e.X - MousePos.X;
                MouseSpeed.Y = e.Y - MousePos.Y;
                if (e.Location != MousePos)
                {
                    double spd = Math.Sqrt(Math.Pow(MouseSpeed.X, 2) + Math.Pow(MouseSpeed.Y, 2));
                    MouseSpeed.X /= (float)spd;
                    MouseSpeed.Y /= (float)spd;

                }
                MousePos = e.Location;

                // сохраняем сплайн
                if ((VertexID > -1) && (Nodes.Count > 2))
                {
                    ClickedVertexID = -1;
                    List<Point> BuiltSpline = new List<Point>();
                    int i = 0;
                    foreach (SplineNode dot in Nodes)
                    {
                        if (i > 0)
                        {
                            BuiltSpline.Add(dot.LeftLean);
                        }
                        BuiltSpline.Add(dot.Position);
                        BuiltSpline.Add(dot.RightLean);
                        i++;
                    }
                    BuiltSpline.Add(new Point(MousePos.X - (int)(MouseSpeed.X * SplineNode.nodeForce),
                                              MousePos.Y - (int)(MouseSpeed.Y * SplineNode.nodeForce)));
                    BuiltSpline.Add(Vertexes[VertexID]);
                    Splines.Add(new FullSpline(BuiltSpline));
                }
                
                Nodes.Clear();
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            ClickedID = -1;
            ClickedVertexID = -1;
            int i = 0;
            if (!drawing)
            {
                i = 0;
                foreach (FullSpline spln in Splines)
                {
                    if (spln.Clicked(e.Location))
                    {
                        ClickedID = i;
                        break;
                    }
                    i++;
                }
            }
            i = 0;
            foreach (Point v in Vertexes)
            {
                if (Math.Pow(v.X - e.X, 2) + Math.Pow(v.Y - e.Y, 2) < Math.Pow(lockRadius, 2))
                {
                    ClickedID = -1;
                    ClickedVertexID = i;
                }
                i++;
            }
            if (e.Button == MouseButtons.Right)
            {
                if ((ClickedID>-1) || (ClickedVertexID>-1))
                {
                    удалитьToolStripMenuItem.Visible = true;
                    удалитьВсеToolStripMenuItem.Visible = false;
                }
                else
                {
                    удалитьToolStripMenuItem.Visible = false;
                    удалитьВсеToolStripMenuItem.Visible = true;
                }
                contextMenuStrip1.Show(new Point(e.X + Left, e.Y + Top));
            }
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ClickedID > -1)
            {
                Splines.RemoveAt(ClickedID);
                ClickedID = -1;
            }
            if (ClickedVertexID > -1)
            {
                List<FullSpline> toRemove = new List<FullSpline>();
                foreach (FullSpline spln in Splines)
                {
                    if((spln.Vertexes[0] == Vertexes[ClickedVertexID]) || (spln.Vertexes[spln.Vertexes.Count-1] == Vertexes[ClickedVertexID]))
                    {
                        toRemove.Add(spln);
                    }
                }
                foreach (FullSpline spln in toRemove)
                {
                    Splines.Remove(spln);
                }
                Vertexes.RemoveAt(ClickedVertexID);
                ClickedVertexID = -1;
            }
        }

        private void удалитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (FullSpline spln in Splines)
            {
                spln.Vertexes.Clear();
            }
            Splines.Clear();
            Vertexes.Clear();
            Nodes.Clear();
            VertexID = -1;
            ClickedID = -1;
            ClickedVertexID = -1;
        }
    }
}