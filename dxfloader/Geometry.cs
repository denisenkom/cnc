using System;
using System.Diagnostics;

namespace dxfloader
{
    public struct Point2D
    {
        public double X, Y;

        public Point2D(double x, double y)
        {
            X = x; Y = y;
        }

        public static bool operator ==(Point2D lhs, Point2D rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }

        public static bool operator !=(Point2D lhs, Point2D rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public struct Point3D
    {
        public double X, Y, Z;

        public Point3D(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public override string ToString()
        {
            return base.ToString() + "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public static bool operator ==(Point3D lhs, Point3D rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
        }

        public static bool operator !=(Point3D lhs, Point3D rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Utils
    {
        public static double Distance(Point2D pt1, Point2D pt2)
        {
            double dx = pt2.X - pt1.X;
            double dy = pt2.Y - pt1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double Distance(Point3D pt1, Point3D pt2)
        {
            double dx = pt2.X - pt1.X;
            double dy = pt2.Y - pt1.Y;
            double dz = pt2.Z - pt1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static double DistanceXYFromLine(Point2D pt, Point2D l1, Point2D l2, out Point2D closest)
        {
            double x1 = l1.X;
            double y1 = l1.Y;
            double dx = l2.X - x1;
            double dy = l2.Y - y1;
            double seglen = Math.Sqrt(dx * dx + dy * dy);
            double nd = -((pt.X - x1) * dy - (pt.Y - y1) * dx) / seglen;
            double na = dy / seglen;
            double nb = -dx / seglen;
            closest = new Point2D(na * nd + pt.X, nb * nd + pt.Y);
            return Math.Abs(nd);
        }

        // градусы, нормализует угол в диапазон (-180..180]
        public static double NormalizeAngle(double angle)
        {
            if (-180 < angle && angle <= 180)
                return angle;
            else
                return angle - 360 * Math.Sign(angle) * Math.Ceiling(Math.Abs(angle) / 360);
        }
    }

    public class DistanceResult
    {
        public double Value;
        public Point2D Closest;
        public DistancePointType PointType;
    }

    public interface IEntityVisitor
    {
        void Visit(Line line);
        void Visit(Arc arc);
    }

    public abstract class Entity
    {
        private AdditionalInfo _info = null;

        public abstract void Accept(IEntityVisitor visitor);
        public abstract DistanceResult Distance(Point2D from);
        public abstract void Split(Point2D point, out Entity ent1, out Entity ent2);
        public abstract void Reverse();
        public abstract Point2D CalcStartPoint();
        public abstract Point2D CalcEndPoint();

        public AdditionalInfo Info
        {
            get
            {
                if (_info == null) { _info = new AdditionalInfo(); _info.Entity = this; }
                return _info;
            }
        }
    }

    public class Line : Entity
    {
        public Point2D StartPoint;
        public Point2D EndPoint;

        public Line()
        {
        }

        public Line(Point2D startPoint, Point2D endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void Split(Point2D point, out Entity ent1, out Entity ent2)
        {
            Split(point, out ent1, out ent2);
        }

        public void Split(Point2D point, out Line line1, out Line line2)
        {
            line1 = new Line();
            line1.StartPoint = StartPoint;
            line1.EndPoint = point;
            line2 = new Line();
            line2.StartPoint = point;
            line2.EndPoint = point;
        }

        public double CalcAngle()
        {
            return Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X);
        }

        public override DistanceResult Distance(Point2D from)
        {
            DistanceResult res = new DistanceResult();
            double d1 = Utils.Distance(from, StartPoint);
            double d2 = Utils.Distance(from, EndPoint);
            Point2D closest;
            double dm = Utils.DistanceXYFromLine(from, StartPoint, EndPoint, out closest);
            if (d1 <= d2 && d1 <= dm)
            {
                res.Value = d1;
                res.Closest = StartPoint;
                res.PointType = DistancePointType.Start;
            }
            else if (d2 <= dm)
            {
                res.Value = d2;
                res.Closest = EndPoint;
                res.PointType = DistancePointType.End;
            }
            else
            {
                res.Value = dm;
                res.Closest = closest;
                res.PointType = DistancePointType.Other;
            }
            return res;
        }

        public override void Reverse()
        {
            Point2D t = StartPoint;
            StartPoint = EndPoint;
            EndPoint = t;
        }

        public override Point2D CalcStartPoint() { return StartPoint; }
        public override Point2D CalcEndPoint() { return EndPoint; }
    }

    public class Arc : Entity
    {
        public Point2D Center;
        public double Radius;
        // градусы
        public double StartAngle;
        public double SweepAngle;

        public override void Accept(IEntityVisitor visitor)
        {
            visitor.Visit(this);
        }

        // градусы
        // расчитывает угол задаваемый вектором из Center до point
        // нормализованный в (-180..180]
        public double CalcAngle(Point2D point)
        {
            return Math.Atan2(point.Y - Center.Y,
                point.X - Center.X) * 180 / Math.PI;
        }

        public override Point2D CalcStartPoint()
        {
            return CalcPoint(StartAngle);
        }

        public override Point2D CalcEndPoint()
        {
            return CalcPoint(StartAngle + SweepAngle);
        }

        // angle не обязан быть нормализованным
        public Point2D CalcPoint(double angle /*градусы*/)
        {
            double angleRads = angle / 180 * Math.PI;
            return new Point2D(Math.Cos(angleRads) * Radius + Center.X,
                Math.Sin(angleRads) * Radius + Center.Y);
        }

        // angle должен быть нормализованным
        public bool IsAngleOnArc(double angle /*градусы*/)
        {
            Debug.Assert(Utils.NormalizeAngle(angle) == angle);
            double startAngle = StartAngle;
            double endAngle = Utils.NormalizeAngle(StartAngle + SweepAngle);
            if (startAngle <= endAngle)
                return startAngle <= angle && angle <= endAngle;
            else
                return startAngle <= angle || angle <= endAngle;
        }

        public override void Split(Point2D point, out Entity ent1, out Entity ent2)
        {
            Split(point, out ent1, out ent2);
        }

        public void Split(Point2D pt, out Arc arc1, out Arc arc2)
        {
            double angle = CalcAngle(pt);
            arc1 = new Arc();
            arc2 = new Arc();
            arc1.Center = arc2.Center = Center;
            arc1.Radius = arc2.Radius = Radius;
            arc1.StartAngle = StartAngle;
            arc2.StartAngle = angle;
            arc2.SweepAngle = StartAngle + SweepAngle - angle;
            arc1.SweepAngle = angle - StartAngle;
        }

        public override DistanceResult Distance(Point2D from)
        {
            DistanceResult res = new DistanceResult();
            double angle = CalcAngle(from);
            double endAngle = Utils.NormalizeAngle(StartAngle + SweepAngle);
            if (IsAngleOnArc(angle) && angle != StartAngle && angle != endAngle)
            {
                double distFromCntr = Utils.Distance(from, Center);
                res.Value = Math.Abs(distFromCntr - Radius);
                res.Closest = CalcPoint(angle);
                res.PointType = DistancePointType.Other;
            }
            else
            {
                Point2D startPoint = CalcPoint(StartAngle);
                Point2D endPoint = CalcPoint(endAngle);
                double l1 = Utils.Distance(from, startPoint);
                double l2 = Utils.Distance(from, endPoint);
                if (l1 <= l2)
                {
                    res.Value = l1;
                    res.Closest = startPoint;
                    res.PointType = DistancePointType.Start;
                }
                else
                {
                    res.Value = l2;
                    res.Closest = endPoint;
                    res.PointType = DistancePointType.End;
                }
            }
            return res;
        }

        public override void Reverse()
        {
            StartAngle = Utils.NormalizeAngle(StartAngle + SweepAngle);
            SweepAngle = -SweepAngle;
        }
    }
}