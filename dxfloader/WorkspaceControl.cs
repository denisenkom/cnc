using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace dxfloader
{
    public partial class WorkspaceControl : UserControl
    {
        LinkedList<Entity> entities = null;
        DrawerVisitor drawer = new DrawerVisitor();
        PickerVisitor picker = new PickerVisitor();
        Entity selectedEntity = null;
        Matrix currentTransform;
        Matrix currentUntransform;
        Pen ordinaryPen = new Pen(Color.Black, 3);
        Pen hilitePen = new Pen(Color.Red, 4);
        bool isMatchedPoint = false;
        Point2D matchedPoint;

        public WorkspaceControl()
        {
            InitializeComponent();
            drawer.TransferPen = new Pen(Color.Gray, 0);
        }

        private void WorkspaceControl_Paint(object sender, PaintEventArgs e)
        {
            if (entities == null)
                return;

            const int rullerWidth = 20;
            const float scale = 0.5f;

            // стираем прежний рисунок
            e.Graphics.FillRectangle(Brushes.DarkGray, rullerWidth, 0, Size.Width - rullerWidth, Size.Height - rullerWidth);

            // рисуем линейку
            e.Graphics.Transform = new Matrix();
            e.Graphics.FillRectangle(Brushes.Yellow, rullerWidth + 1, Size.Height - rullerWidth, Size.Width - rullerWidth - 1, rullerWidth);
            e.Graphics.FillRectangle(Brushes.Yellow, 0, 0, rullerWidth, Size.Height - rullerWidth - 1);
            e.Graphics.DrawLine(new Pen(Color.Black), 0, Size.Height - rullerWidth, rullerWidth, Size.Height - rullerWidth);
            e.Graphics.DrawLine(new Pen(Color.Black), rullerWidth, Size.Height - rullerWidth, rullerWidth, Size.Height);

            e.Graphics.TranslateTransform(rullerWidth + 5, Size.Height - rullerWidth - 5);
            e.Graphics.ScaleTransform(scale, -scale);
            currentTransform = e.Graphics.Transform;
            currentUntransform = currentTransform.Clone();
            currentUntransform.Invert();

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            // рисуем заготовку
            e.Graphics.FillRectangle(Brushes.White, 0, 0, 2000, 1000);

            drawer.Graphics = e.Graphics;
            foreach (Entity ent in entities)
            {
                drawer.CuttingPen = (ent == selectedEntity ? hilitePen : ordinaryPen);
                ent.Accept(drawer);
            }

            if (isMatchedPoint)
            {
                double radius = PickerVisitor.Epsilon / 2;
                e.Graphics.DrawArc(new Pen(Color.DarkCyan), (float)(matchedPoint.X - radius),
                    (float)(matchedPoint.Y - radius), (float)(2 * radius), (float)(2 * radius), 0, 360);
            }

        }

        private void WorkspaceControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (entities == null)
                return;

            PointF[] pt = { new PointF(e.X, e.Y) };
            currentUntransform.TransformPoints(pt);
            picker.Cursor.X = pt[0].X;
            picker.Cursor.Y = pt[0].Y;
            Entity newSelectedEntity = null;
            bool newIsMatchedPoint = false;
            Point2D newMatchedPoint;
            newMatchedPoint.X = 0;
            newMatchedPoint.Y = 0;
            foreach (Entity ent in entities)
            {
                ent.Accept(picker);
                if (picker.MatchPoint)
                {
                    newIsMatchedPoint = true;
                    newMatchedPoint = picker.Pt;
                    break;
                }
                if (picker.Match && newSelectedEntity == null)
                {
                    newSelectedEntity = ent;
                }
            }

            if (newIsMatchedPoint)
            {
                newSelectedEntity = null;
            }

            if (newIsMatchedPoint != isMatchedPoint ||
                newIsMatchedPoint && isMatchedPoint &&
                matchedPoint != newMatchedPoint)
            {
                isMatchedPoint = newIsMatchedPoint;
                matchedPoint = newMatchedPoint;
                Invalidate();
            }

            if (newSelectedEntity != selectedEntity)
            {
                selectedEntity = newSelectedEntity;
                Invalidate();
            }
        }

        public LinkedList<Entity> Entities
        {
            get
            {
                return entities;
            }
            set
            {
                if (value != entities)
                {
                    entities = value;
                    Invalidate();
                }
            }
        }
    }
}
