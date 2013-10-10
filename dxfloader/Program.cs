using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace dxfloader
{
    class Program
    {
        public static string FileName;

        [STAThread]
        static void Main(string[] args)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "dxf";
            dlg.Filter = "Файлы dxf (*.dxf)|*.dxf";
            dlg.FilterIndex = 0;
            dlg.Title = "Выберите dxf файл для конвертации";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileName = dlg.FileName;
                Application.Run(new MainForm());
            }
        }
    }
}
