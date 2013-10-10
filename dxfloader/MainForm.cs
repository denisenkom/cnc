using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace dxfloader
{
    public partial class MainForm : Form
    {
        LinkedList<Entity> entities;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LinkedList<AcDbEntity> dxfEntities;
            using (TextReader rdr = File.OpenText(Program.FileName))
            {
                dxfEntities = Loader.Load(rdr);
            }

            entities = Dxf2GeomCvt.Convert(dxfEntities);
            workspaceControl.Entities = entities;

            // УБРАТЬ
            LinkedList<Trace> traces = Tracer.MakeTraces(entities);

            // получаем цепочки
            /*LinkedList<LinkedList<Entity>> chains = new LinkedList<LinkedList<Entity>>();
            while (entities.Count != 0)
            {
                LinkedList<Entity> chain = new LinkedList<Entity>();
                chains.AddLast(chain);
                chain.AddLast(entities.First.Value);
                Entity tail = entities.First.Value;
                entities.Remove(entities.First);
                LinkedListNode<Entity> j = entities.First;
                while (j != null)
                {
                    LinkedListNode<Entity> curr = j;
                    j = j.Next;
                    if (curr.Value.IsContinue(tail))
                    {
                        tail = curr.Value;
                        chain.AddLast(curr.Value);
                        entities.Remove(curr);
                    }
                }
            }*/
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
        }
    }
}