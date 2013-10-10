using System;
using System.Collections.Generic;

namespace dxfloader
{
    public enum PassingType
    {
        FreePass, Mill
    }

    public abstract class IRCommand
    {
        public PassingType PassingType;

        // функции для определения входных и выходных
        // углов
        public abstract bool HaveAngles(); // есть ли углы, например для точки нет
        public abstract double InAngle();
        public abstract double OutAngle();
    }

    public class IRLine : IRCommand
    {
        public double Dx; // meters
        public double Dy;

        public IRLine()
        {
        }

        public IRLine(double dx, double dy, PassingType passingType)
        {
            Dx = dx;
            Dy = dy;
            PassingType = passingType;
        }

        public IRLine(Point2D from, Point2D to)
        {
            Dx = to.X - from.X;
            Dy = to.Y - from.Y;
        }

        public IRLine(Point2D from, Point2D to, PassingType passingType)
        {
            Dx = to.X - from.X;
            Dy = to.Y - from.Y;
            PassingType = passingType;
        }

        public override bool HaveAngles()
        {
            return true;
        }

        public override double InAngle()
        {
            return Math.Atan2(Dy, Dx);
        }

        public override double OutAngle()
        {
            return Math.Atan2(Dy, Dx);
        }
    }

    public class IRArc : IRCommand
    {
        public double StartAngle; // градусы
        public double SweepAngle; // градусы
        public double Radius;

        public override bool HaveAngles()
        {
            return true;
        }

        public override double InAngle()
        {
            return Utils.NormalizeAngle(StartAngle * Math.PI / 180 + Math.PI / 2);
        }

        public override double OutAngle()
        {
            return Utils.NormalizeAngle((StartAngle + SweepAngle) * Math.PI / 180 + Math.PI / 2);
        }
    }

    // для, например, выполнения отверстий
    public class IRPoint : IRCommand
    {
        public override bool HaveAngles()
        {
            return false;
        }

        public override double InAngle()
        {
            return 0;
        }

        public override double OutAngle()
        {
            return 0;
        }
    }


    public class AxisCommand
    {
    }

    // ускоренное или замедленное движение вдоль линии
    public class LinearAccelerate : AxisCommand
    {
        public double AccelX; // метр/сек^2
        public double AccelY;
        public double AccelZ;
        public double Dx;
        public double Dy;
        public double Dz;
    }

    // равномерное движение вдоль линии
    public class LinearMove : AxisCommand
    {
        public double SpeedX; // метр/сек
        public double SpeedY;
        public double SpeedZ;
        public double Dx;
        public double Dy;
        public double Dz;
    }

    // ускоренное или замедленное движение вдоль дуги
    public class ArcAccelerate : AxisCommand
    {
        public double StartAngle;
        public double SweepAngle;
        public double Acceleration; // метр/сек^2
    }

    // равномерное движение по дуге
    public class ArcMove : AxisCommand
    {
        public double StartAngle;
        public double SweepAngle;
        public double Speed; // метр/сек
    }

    public class Accelerator
    {
        public static LinkedList<AxisCommand> Translate(Trace trace)
        {
            return null;
        }
    }

    public class Generator
    {
        class Ent2IRCmd : IEntityVisitor
        {
            #region IEntityVisitor Members
            public IRCommand Result;

            public void Visit(Line line)
            {
                Result = new IRLine(line.StartPoint, line.EndPoint);
            }

            public void Visit(Arc arc)
            {
                IRArc newarc = new IRArc();
                newarc.StartAngle = arc.StartAngle;
                newarc.SweepAngle = arc.SweepAngle;
                newarc.Radius = arc.Radius;
                Result = newarc;
            }

            #endregion
        }

        public static LinkedList<IRCommand> Generate(LinkedList<Trace> traces)
        {
            LinkedList<IRCommand> result = new LinkedList<IRCommand>();
            Ent2IRCmd cvt = new Ent2IRCmd();
            result = new LinkedList<IRCommand>();

            Point2D from = new Point2D();
            while (traces.Count != 0)
            {
                Trace trace = ClosestTraceChooser.ChooseTrace(from, traces);
                traces.Remove(trace);
                Point2D to = trace.CalcStartPoint();

                result.AddLast(new IRLine(from, to, PassingType.FreePass));
                foreach (Entity ent in trace.Entities)
                {
                    ent.Accept(cvt);
                    IRCommand cmd = cvt.Result;
                    cmd.PassingType = PassingType.Mill;
                    result.AddLast(cmd);
                }

                from = trace.CalcEndPoint();
            }
            return result;
        }
    }
}
