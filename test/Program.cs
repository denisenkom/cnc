using System;
using System.Collections.Generic;
using System.Text;

namespace test
{
    public struct Segment
    {
        public Point Start;
        public Point End;
    }
    
    public class Utils
    {
        public static double Distance(Point pt1, Point pt2)
        {
            double dx = pt2.X - pt1.X;
            double dy = pt2.Y - pt1.Y;
            double dz = pt2.Z - pt1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static double DistanceXYFromLine(Point pt, Point l1, Point l2, out Point closest)
        {
            double x1 = l1.X;
            double y1 = l1.Y;
            double dx = l2.X - x1;
            double dy = l2.Y - y1;
            double seglen = Math.Sqrt(dx * dx + dy * dy);
            double nd = -((pt.X - x1) * dy - (pt.Y - y1) * dx) / seglen;
            double na = dy / seglen;
            double nb = -dx / seglen;
            closest = new Point(na * nd + pt.X, nb * nd + pt.Y, 0);
            return Math.Abs(nd);
        }
    }

    public struct Point
    {
        public double X, Y, Z;

        public Point(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public override string ToString()
        {
            return base.ToString() + "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public static bool operator ==(Point lhs, Point rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
        }

        public static bool operator !=(Point lhs, Point rhs)
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

    class Program
    {
        class MyClass
        {
            public string Name;

            public MyClass(string name) { Name = name; }
        }

        static void Main(string[] args)
        {
            LinkedList<MyClass> mylist = new LinkedList<MyClass>();
            mylist.AddLast(new MyClass("obj1"));
            mylist.AddLast(new MyClass("obj2"));
            LinkedListNode<MyClass> node = mylist.First;
            mylist.Remove(node);
            mylist.AddLast(node);
        }
    }
}
