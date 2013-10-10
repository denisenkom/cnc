using System;
using System.Collections.Generic;
using System.IO;

namespace dxfloader
{
    public interface IDxfVisitor
    {
        void Visit(AcDbLine line);
        void Visit(AcDbPolyline polyline);
        void Visit(AcDbCircle circle);
        void Visit(AcDbArc arc);
    }

    public enum DistancePointType { Start, End, Other }

    public abstract class AcDbEntity
    {
        public string Layer;

        public virtual void TakeNode(DxfNode node)
        {
            if (node.TypeCode == 8)
                Layer = (string)node.Value;
        }

        public abstract void Accept(IDxfVisitor visitor);
    }

    public class AcDbPolyline : AcDbEntity
    {
        public Point3D[] Points;
        public bool Closed;
        public double Width;

        private int _currentPoint = -1; // текущая загружаемая точка. нужна только для загрузки

        public override void TakeNode(DxfNode node)
        {
            base.TakeNode(node);
            switch (node.TypeCode)
            {
                case 10:
                    _currentPoint++;
                    Points[_currentPoint].X = (double)node.Value;
                    break;
                case 20:
                    Points[_currentPoint].Y = (double)node.Value;
                    break;
                case 30:
                    Points[_currentPoint].Z = (double)node.Value;
                    break;
                case 43:
                    Width = (double)node.Value;
                    break;
                case 70:
                    Closed = ((short)node.Value == 1);
                    break;
                case 90: // количество точек
                    Points = new Point3D[(int)node.Value];
                    break;
            }
        }

        public override void Accept(IDxfVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public enum LinearMoveType { Transfer, Cutting }

    public class AcDbLine : AcDbEntity
    {
        public Point3D StartPoint;
        public Point3D EndPoint;
        public double Thickness;
        public Point3D ExtrusionDirection;

        public AcDbLine()
        {
            ExtrusionDirection.X = 0;
            ExtrusionDirection.Y = 0;
            ExtrusionDirection.Z = 1;
        }

        public override void TakeNode(DxfNode node)
        {
            base.TakeNode(node);
            switch (node.TypeCode)
            {
                case 10:
                    StartPoint.X = (double)node.Value;
                    break;
                case 20:
                    StartPoint.Y = (double)node.Value;
                    break;
                case 30:
                    StartPoint.Z = (double)node.Value;
                    break;
                case 11:
                    EndPoint.X = (double)node.Value;
                    break;
                case 21:
                    EndPoint.Y = (double)node.Value;
                    break;
                case 31:
                    EndPoint.Z = (double)node.Value;
                    break;
                case 39:
                    Thickness = (double)node.Value;
                    break;
                case 210:
                    ExtrusionDirection.X = (double)node.Value;
                    break;
                case 220:
                    ExtrusionDirection.Y = (double)node.Value;
                    break;
                case 230:
                    ExtrusionDirection.Z = (double)node.Value;
                    break;
            }
        }

        public override void Accept(IDxfVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class AcDbCircle : AcDbEntity
    {
        public Point3D Center;
        public double Radius;
        public double Thickness;
        public Point3D ExtrusionDirection;

        public AcDbCircle()
        {
            ExtrusionDirection.X = 0;
            ExtrusionDirection.Y = 0;
            ExtrusionDirection.Z = 1;
        }

        public override void TakeNode(DxfNode node)
        {
            base.TakeNode(node);
            switch (node.TypeCode)
            {
                case 10:
                    Center.X = (double)node.Value;
                    break;
                case 20:
                    Center.Y = (double)node.Value;
                    break;
                case 30:
                    Center.Z = (double)node.Value;
                    break;
                case 39:
                    Thickness = (double)node.Value;
                    break;
                case 40:
                    Radius = (double)node.Value;
                    break;
                case 210:
                    ExtrusionDirection.X = (double)node.Value;
                    break;
                case 220:
                    ExtrusionDirection.Y = (double)node.Value;
                    break;
                case 230:
                    ExtrusionDirection.Z = (double)node.Value;
                    break;
            }
        }

        public override void Accept(IDxfVisitor visitor)
        {
            visitor.Visit(this);
        }
    };

    public class AcDbArc : AcDbCircle
    {
        public double StartAngle;
        public double EndAngle;

        public override void TakeNode(DxfNode node)
        {
            base.TakeNode(node);
            switch (node.TypeCode)
            {
                case 50:
                    StartAngle = (double)node.Value;
                    break;
                case 51:
                    EndAngle = (double)node.Value;
                    break;
                case 210:
                    ExtrusionDirection.X = (double)node.Value;
                    break;
                case 220:
                    ExtrusionDirection.Y = (double)node.Value;
                    break;
                case 230:
                    ExtrusionDirection.Z = (double)node.Value;
                    break;
            }

        }

        public override void Accept(IDxfVisitor visitor)
        {
            visitor.Visit(this);
        }
    };

    class Loader
    {
        enum State { Start, Entity }

        public static LinkedList<AcDbEntity> Load(TextReader reader)
        {
            LinkedList<AcDbEntity> result = new LinkedList<AcDbEntity>();
            State state = State.Start;
            AcDbEntity entity = null;
            DxfReader rdr = new DxfReader(reader);
            while (!rdr.IsEof)
            {
                DxfNode node = rdr.Read();
                if (state == State.Start || node.TypeCode == 0)
                {
                    // завершаем текущее состояние
                    if (state != State.Start && node.TypeCode == 0)
                    {
                        state = State.Start;
                    }

                    if (node.TypeCode == 0)
                    {
                        switch ((string)node.Value)
                        {
                            case "LWPOLYLINE":
                                entity = new AcDbPolyline();
                                result.AddLast(entity);
                                state = State.Entity;
                                break;
                            case "LINE":
                                entity = new AcDbLine();
                                result.AddLast(entity);
                                state = State.Entity;
                                break;
                            case "CIRCLE":
                                entity = new AcDbCircle();
                                result.AddLast(entity);
                                state = State.Entity;
                                break;
                            case "ARC":
                                entity = new AcDbArc();
                                result.AddLast(entity);
                                state = State.Entity;
                                break;
                        }
                    }
                }
                else if (state == State.Entity)
                {
                    entity.TakeNode(node);
                }
            }
            return result;
        }
    };
}