using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace dxfloader
{
    public class Trace
    {
        public LinkedList<Entity> Entities = new LinkedList<Entity>();
        public bool IsLoop;

        public void SplitLoop(Entity ent, Point2D pt, DistancePointType pointType)
        {
            Debug.Assert(IsLoop);
            LinkedListNode<Entity> stop = null;
            switch (pointType)
            {
                case DistancePointType.Other:
                    Entity ent1, ent2;
                    ent.Split(pt, out ent1, out ent2);
                    LinkedListNode<Entity> node = Entities.Find(ent);
                    LinkedListNode<Entity> node1 = Entities.AddAfter(node, ent1);
                    LinkedListNode<Entity> node2 = Entities.AddAfter(node1, ent2);
                    stop = node2;
                    Entities.Remove(node);
                    break;
                case DistancePointType.Start:
                    stop = Entities.Find(ent);
                    break;
                case DistancePointType.End:
                    stop = Entities.Find(ent).Next;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            if (stop != null)
            {
                while (stop != Entities.First)
                {
                    LinkedListNode<Entity> node = Entities.First;
                    Entities.Remove(node);
                    Entities.AddLast(node);
                    node.Value.Reverse();
                }
            }
        }

        public Trace CreateReversed()
        {
            Trace newTrace = new Trace();
            foreach (Entity ent in Entities)
            {
                newTrace.Entities.AddFirst(ent);
            }
            newTrace.IsLoop = IsLoop;
            return newTrace;
        }

        public Point2D CalcStartPoint()
        {
            return Entities.First.Value.CalcStartPoint();
        }

        public Point2D CalcEndPoint()
        {
            return Entities.Last.Value.CalcEndPoint();
        }

        public void Optimize()
        {
            for (LinkedListNode<Entity> node = Entities.First; node != null; )
            {
                LinkedListNode<Entity> next = node.Next;
                // удаляем нулевые линии
                if (node.Value is Line)
                {
                    Line line = node.Value as Line;
                    if (line.StartPoint == line.EndPoint)
                    {
                        Entities.Remove(node);
                        node = next;
                        next = node.Next;
                    }
                }
                else if (node.Value is Arc)
                {
                    Arc arc = node.Value as Arc;
                    if (arc.SweepAngle == 0)
                    {
                        Entities.Remove(node);
                        node = next;
                        next = node.Next;
                    }
                }

                if (next != null && node.Value is Line && next.Value is Line)
                {
                    Line line1 = node.Value as Line;
                    Line line2 = next.Value as Line;
                    // если линии сонаправленны то их можно объединить
                    if (line1.CalcAngle() == line2.CalcAngle())
                    {
                        Line newline = new Line(line1.StartPoint, line2.EndPoint);

                        // заменяем 2 линни новой
                        LinkedListNode<Entity> rem1 = node;
                        LinkedListNode<Entity> rem2 = next;
                        next = Entities.AddBefore(node, newline);
                        Entities.Remove(rem1);
                        Entities.Remove(rem2);
                    }
                }
                else if (next != null && node.Value is Arc && next.Value is Arc)
                {
                    Arc arc1 = node.Value as Arc;
                    Arc arc2 = next.Value as Arc;

                    if (arc1.Radius == arc2.Radius &&
                        Utils.NormalizeAngle(arc1.StartAngle + arc1.SweepAngle) == arc2.StartAngle)
                    {
                        Arc newarc = new Arc();
                        newarc.Radius = arc1.Radius;
                        newarc.StartAngle = arc1.StartAngle;
                        newarc.SweepAngle = arc1.SweepAngle + arc2.SweepAngle;

                        // заменяем 2 дуги новой
                        LinkedListNode<Entity> rem1 = node;
                        LinkedListNode<Entity> rem2 = next;
                        next = Entities.AddBefore(node, newarc);
                        Entities.Remove(rem1);
                        Entities.Remove(rem2);
                    }
                }
                node = next;
            }
        }
    }

    class Tracer
    {
        class FillerVisitor : IEntityVisitor
        {
            public void Visit(Line line)
            {
                line.Info.Start = line.StartPoint;
                line.Info.End = line.EndPoint;
                line.Info.Reversed = false;
            }

            public void Visit(Arc arc)
            {
                arc.Info.Start = arc.CalcStartPoint();
                arc.Info.End = arc.CalcEndPoint();
                arc.Info.Reversed = false;
            }
        }

        private static Entity FindNextEntity(Point2D from, LinkedList<Entity> entities)
        {
            foreach (Entity ent in entities)
            {
                if (ent.Info.Start == from)
                {
                    return ent;
                }
                if (ent.Info.End == from)
                {
                    ent.Info.Reversed = true;
                    Point2D t = ent.Info.Start;
                    ent.Info.Start = ent.Info.End;
                    ent.Info.End = t;
                    return ent;
                }
            }
            return null;
        }

        private static Entity FindPrevEntity(Point2D from, LinkedList<Entity> entities)
        {
            foreach (Entity ent in entities)
            {
                if (ent.Info.End == from)
                {
                    return ent;
                }
                if (ent.Info.Start == from)
                {
                    ent.Info.Reversed = true;
                    Point2D t = ent.Info.Start;
                    ent.Info.Start = ent.Info.End;
                    ent.Info.End = t;
                    return ent;
                }
            }
            return null;
        }

        public static LinkedList<Trace> MakeTraces(LinkedList<Entity> inentities)
        {
            LinkedList<Entity> entities = new LinkedList<Entity>(inentities);
            LinkedList<Trace> result = new LinkedList<Trace>();

            // заполнение структуры AdditionalInfo для всех entity
            // значения начальной и конечной точки
            FillerVisitor vis = new FillerVisitor();
            foreach (Entity ent in entities)
            {
                ent.Accept(vis);
            }

            while (entities.Count > 0)
            {
                Trace trace = new Trace();
                Entity ent = entities.First.Value;
                trace.Entities.AddLast(ent);
                entities.RemoveFirst();
                result.AddLast(trace);

                Entity next;
                Point2D from = ent.Info.End;
                while ((next = FindNextEntity(from, entities)) != null)
                {
                    entities.Remove(next);
                    trace.Entities.AddLast(next);
                    from = next.Info.End;
                }
                from = ent.Info.Start;
                while ((next = FindPrevEntity(from, entities)) != null)
                {
                    entities.Remove(next);
                    trace.Entities.AddFirst(next);
                    from = next.Info.Start;
                }

                trace.IsLoop = trace.Entities.First.Value.Info.Start == trace.Entities.Last.Value.Info.End;

                // выполняем оптимизацию трейса
                trace.Optimize();
            }
            return result;
        }
    }

    public class ClosestTraceChooser
    {
        private struct LoopDistanceResult
        {
            public Entity Entity;
            public double Value;
            public Point2D Closest;
            public DistancePointType PointType;
        }

        private static LoopDistanceResult DistanceToLoop(Point2D from, Trace trace)
        {
            LoopDistanceResult minDist = new LoopDistanceResult();
            minDist.Value = double.MaxValue;
            foreach (Entity ent in trace.Entities)
            {
                DistanceResult dist = ent.Distance(from);
                if (minDist.Value > dist.Value)
                {
                    minDist.Value = dist.Value;
                    minDist.Entity = ent;
                    minDist.Closest = dist.Closest;
                    minDist.PointType = dist.PointType;
                }
            }
            return minDist;
        }

        private struct SelectClosestTraceResult
        {
            public Trace Trace;
            public bool NeedReverse;
            public LoopDistanceResult LoopDistance;
        }

        private static SelectClosestTraceResult SelectClosestTrace(Point2D from, LinkedList<Trace> traces)
        {
            SelectClosestTraceResult bestResult = new SelectClosestTraceResult();
            double minDistance = double.MaxValue;
            foreach (Trace trace in traces)
            {
                if (trace.IsLoop)
                {
                    LoopDistanceResult distToLoop = DistanceToLoop(from, trace);
                    if (distToLoop.Value < minDistance)
                    {
                        minDistance = distToLoop.Value;
                        bestResult.LoopDistance = distToLoop;
                        bestResult.Trace = trace;
                        bestResult.NeedReverse = false;
                    }
                }
                else
                {
                    double tostart = Utils.Distance(trace.Entities.First.Value.Info.Start, from);
                    double toend = Utils.Distance(trace.Entities.Last.Value.Info.End, from);
                    double distance = Math.Min(tostart, toend);
                    bool needReverse = toend < tostart;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestResult.Trace = trace;
                        bestResult.NeedReverse = needReverse;
                    }
                }
            }
            return bestResult;
        }

        public static Trace ChooseTrace(Point2D from, LinkedList<Trace> traces)
        {
            SelectClosestTraceResult closestTraceRes = SelectClosestTrace(from, traces);
            // если ближайшим оказался цикл то разрезаем его по ближней точке
            if (closestTraceRes.Trace.IsLoop)
            {
                Entity ent = closestTraceRes.LoopDistance.Entity;
                Point2D closest = closestTraceRes.LoopDistance.Closest;
                DistancePointType pointType = closestTraceRes.LoopDistance.PointType;
                closestTraceRes.Trace.SplitLoop(ent, closest, pointType);
            }
            else
            {
                if (closestTraceRes.NeedReverse)
                {
                    Trace reversed = closestTraceRes.Trace.CreateReversed();
                    traces.Find(closestTraceRes.Trace).Value = reversed;
                    closestTraceRes.Trace = reversed;
                }
            }
            return closestTraceRes.Trace;
        }
    }
}