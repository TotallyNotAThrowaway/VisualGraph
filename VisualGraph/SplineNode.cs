using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VisualGraph
{
    class SplineNode                            //описывает точку постоения сплайна
    {
        public Point Constraint;                //точка привязки
        public Point Position;                  //текущая позиция
        public PointF Speed;                    //текущая скорость
        public Point LeftLean, RightLean;       //точки тяготения слева и справа "условно"

        const float damping = 0.98f;           //потеря энергии при отрисовке кадра (оставшаяся энергия)
        const float force = -0.02f;             //сила притяжения к точке привязки
        public const int nodeForce = 20;        //расстояние от точки до точки тяготения
        const float speedForce = 0.9f;          //множитель входной скорости
        const float maxSpeed = 0.5f;            //максимальная скорость при создании точки
        const int spread = 10;                  //разброс начального значения точки от точки привязки

        public SplineNode(Point Constraint, PointF Speed, bool locked = false)                       //конструктор объекта
        {
            Random rand = new Random();
            this.Constraint = Constraint;
            this.Speed.X = Math.Min(Speed.X, maxSpeed) * speedForce;
            this.Speed.Y = Math.Min(Speed.Y, maxSpeed) * speedForce;

            Position.X = Constraint.X + rand.Next(-spread, spread);// + Convert.ToInt32(rand.Next() * this.Speed.X + rand.Next() * (this.Speed.Y * multiplier));
            Position.Y = Constraint.Y + rand.Next(-spread, spread);// + Convert.ToInt32(rand.Next() * this.Speed.Y + rand.Next() * (this.Speed.X * multiplier));

            if (locked)
            {
                this.Speed = new PointF(0f, 0f);
                this.Position = Constraint;
            }

            LeftLean.X = Position.X - Convert.ToInt32(Speed.X * nodeForce);
            LeftLean.Y = Position.Y - Convert.ToInt32(Speed.Y * nodeForce);
            RightLean.X = Position.X + Convert.ToInt32(Speed.X * nodeForce);
            RightLean.Y = Position.Y + Convert.ToInt32(Speed.Y * nodeForce);
            //для точек привязки, тяготения и текущей точки(белой) присваивается положение по вызову.

        }

        public int Update()                     //функция расчёта "колебания" точки
        {

            Speed.X += (Position.X - Constraint.X) * force;
            Speed.Y += (Position.Y - Constraint.Y) * force;
            Speed.X *= damping;
            Speed.Y *= damping;
            Position.X = Convert.ToInt32(Position.X + Speed.X);
            Position.Y = Convert.ToInt32(Position.Y + Speed.Y);
            LeftLean.X = Convert.ToInt32(LeftLean.X + Speed.X);
            LeftLean.Y = Convert.ToInt32(LeftLean.Y + Speed.Y);
            RightLean.X = Convert.ToInt32(RightLean.X + Speed.X);
            RightLean.Y = Convert.ToInt32(RightLean.Y + Speed.Y);

            return 0;
        }
    }
}
