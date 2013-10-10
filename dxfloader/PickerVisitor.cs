using System;
using System.Collections.Generic;
using System.Text;

namespace dxfloader
{
    class PickerVisitor : IEntityVisitor
    {
        public Point2D Cursor;
        public bool Match;
        public Point2D Pt;
        public bool MatchPoint;
        public const double Epsilon = 30;

        private bool IsMatchPoint(Point2D pt)
        {
            double dx = pt.X - Cursor.X;
            double dy = pt.Y - Cursor.Y;
            return Math.Sqrt(dx * dx + dy * dy) < Epsilon / 2;
        }

        public void Visit(Line line)
        {
            // проверяем указывает ли курсор на эту линию
            double dx = line.EndPoint.X - line.StartPoint.X;
            double dy = line.EndPoint.Y - line.StartPoint.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            double cos = dx / dist;
            double sin = -dy / dist;

            double x = Cursor.X - line.StartPoint.X;
            double y = Cursor.Y - line.StartPoint.Y;
            double rotx = x * cos - y * sin;
            double roty = x * sin + y * cos;

            Match = rotx >= -Epsilon / 2 && rotx <= dist + Epsilon / 2 &&
                Math.Abs(roty) < Epsilon / 2;

            // проверяем указывает ли курсор на точки этой линии
            MatchPoint = false;
            if (IsMatchPoint(line.StartPoint))
            {
                Pt = line.StartPoint;
                MatchPoint = true;
                return;
            }
            if (IsMatchPoint(line.EndPoint))
            {
                Pt = line.EndPoint;
                MatchPoint = true;
                return;
            }
        }

        public void Visit(Arc arc)
        {
            // проверяем указывает ли на дугу
            DistanceResult dist = arc.Distance(Cursor);
            Match = dist.Value < Epsilon / 2;
            if (Match)
            {
                switch (dist.PointType)
                {
                    case DistancePointType.Start:
                        MatchPoint = true;
                        Pt = dist.Closest;
                        break;
                    case DistancePointType.End:
                        MatchPoint = true;
                        Pt = dist.Closest;
                        break;
                }
            }
        }
    }
}
