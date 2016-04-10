using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VisualGraph
{
    class FullSpline
    {
        public List<Point> Vertexes = new List<Point>();
        const int ClickRadius = 20;

        public FullSpline(List<Point> data)
        {
            this.Vertexes = data;
        }

        public void Draw(Graphics g, bool clicked = false)
        {
            if (Vertexes.Count > 0)
            {
                Pen SplinePen = new Pen(Color.Black);
                if (clicked)
                {
                    SplinePen.Color = Color.Red;
                    SplinePen.Width = 2;
                }
                else if (Vertexes[0] == Vertexes[Vertexes.Count - 1])
                {
                    SplinePen.Color = Color.Fuchsia;
                    SplinePen.Width = 2;
                }
                else
                {
                    SplinePen.Color = Color.Black;
                    SplinePen.Width = 1;
                }

                g.DrawBeziers(SplinePen, Vertexes.ToArray<Point>());
            }
        }

        public bool Clicked(Point click)
        {
            foreach (Point dot in Vertexes)
            {
                if (Math.Pow(dot.X - click.X, 2) + Math.Pow(dot.Y - click.Y, 2) < Math.Pow(ClickRadius, 2))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
