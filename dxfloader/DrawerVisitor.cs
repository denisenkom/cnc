using System.Drawing;

namespace dxfloader
{
    class DrawerVisitor : IEntityVisitor
    {
        public Graphics Graphics;
        public Pen CuttingPen;
        public Pen TransferPen;

        public void Visit(Line line)
        {
            Pen pen = /*(line.MoveType == LinearMoveType.Cutting ?*/ CuttingPen/* : TransferPen)*/;
            Graphics.DrawLine(pen, (float)line.StartPoint.X, (float)line.StartPoint.Y,
                (float)line.EndPoint.X, (float)line.EndPoint.Y);
        }

        public void Visit(Arc arc)
        {
            float x = (float)(arc.Center.X - arc.Radius);
            float y = (float)(arc.Center.Y - arc.Radius);
            float width = (float)(arc.Radius * 2);
            float height = (float)(arc.Radius * 2);
            Graphics.DrawArc(CuttingPen, x, y, width, height,
                (float)arc.StartAngle, (float)(arc.SweepAngle));
        }
    }
}
