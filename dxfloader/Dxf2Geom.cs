using System;
using System.Collections.Generic;
using System.Text;

namespace dxfloader
{
    class Dxf2GeomVisitor : IDxfVisitor
    {
        #region IDxfVisitor Members

        private LinkedList<Entity> _entities = new LinkedList<Entity>();

        public void Visit(AcDbLine line)
        {
            Line newline = new Line();
            newline.StartPoint.X = line.StartPoint.X;
            newline.StartPoint.Y = line.StartPoint.Y;
            newline.EndPoint.X = line.EndPoint.X;
            newline.EndPoint.Y = line.EndPoint.Y;
            _entities.AddLast(newline);
        }

        public void Visit(AcDbPolyline polyline)
        {
            Point2D from;
            from.X = polyline.Points[0].X;
            from.Y = polyline.Points[0].Y;
            for (int i = 1; i < polyline.Points.Length; i++)
            {
                Line newline = new Line();
                newline.StartPoint = from;
                Point2D to;
                to.X = polyline.Points[i].X;
                to.Y = polyline.Points[i].Y;
                newline.EndPoint = to;
                _entities.AddLast(newline);
                from = to;
            }
            if (polyline.Closed)
            {
                Line newline = new Line();
                newline.StartPoint = from;
                newline.EndPoint.X = polyline.Points[0].X;
                newline.EndPoint.Y = polyline.Points[0].Y;
                _entities.AddLast(newline);
            }
        }

        public void Visit(AcDbCircle circle)
        {
            Arc newarc = new Arc();
            newarc.Center.X = circle.Center.X;
            newarc.Center.Y = circle.Center.Y;
            newarc.Radius = circle.Radius;
            newarc.StartAngle = 0;
            newarc.SweepAngle = 360;
            _entities.AddLast(newarc);
        }

        public void Visit(AcDbArc arc)
        {
            Arc newarc = new Arc();
            newarc.Center.X = arc.Center.X;
            newarc.Center.Y = arc.Center.Y;
            newarc.Radius = arc.Radius;
            newarc.StartAngle = arc.StartAngle;
            newarc.SweepAngle = arc.EndAngle - arc.StartAngle;
            _entities.AddLast(newarc);
        }

        public LinkedList<Entity> Result
        {
            get { return _entities; }
        }

        #endregion
    }

    public class Dxf2GeomCvt
    {
        public static LinkedList<Entity> Convert(LinkedList<AcDbEntity> entities)
        {
            Dxf2GeomVisitor vis = new Dxf2GeomVisitor();
            foreach (AcDbEntity ent in entities)
                ent.Accept(vis);
            return vis.Result;
        }
    }
}
