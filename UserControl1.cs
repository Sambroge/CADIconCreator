using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TFlex.Model;
using TFlex.Model.Model3D;

namespace CADIconCreator
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var _document = TFlex.Application.ActiveDocument;

            _document.BeginChanges("Создание иконок.");
            if (checkBox1.Checked)
            {
                foreach (var obj in _document.GetObjects())
                    if (obj is Construction3D con) con.VisibleInScene = false;
            }
            if (checkBox2.Checked)
            {
                TFlex.Application.RunSystemCommand("ZoomMax");
            }
            if(checkBox3.Checked)
            {
                ExportToBitmap3D param = new ExportToBitmap3D(_document.ActiveView);
                param.Height = 32;
                param.Width = 32;
                param.PixelSize = true;
                param.Annotations = false;
                param.Constructions = false;
                param.Dpi = 72;
                param.Export("C:\\Users\\Tarnogradskij\\Desktop\\импорт картинок\\Иконки\\32.bmp");
            }            
            if(checkBox3.Checked)
            {
                ExportToBitmap3D param = new ExportToBitmap3D(_document.ActiveView);
                param.Height = 128;
                param.Width = 128;
                param.PixelSize = true;
                param.Annotations = false;
                param.Constructions = false;
                param.Dpi = 72;
                param.Export("C:\\Users\\Tarnogradskij\\Desktop\\импорт картинок\\Иконки\\128.bmp");
            }            
            if(checkBox3.Checked)
            {
                ExportToBitmap3D param = new ExportToBitmap3D(_document.ActiveView);
                param.Height = 256;
                param.Width = 256;
                param.PixelSize = true;
                param.Annotations = false;
                param.Constructions = false;
                param.Dpi = 72;
                param.Export("C:\\Users\\Tarnogradskij\\Desktop\\импорт картинок\\Иконки\\256.bmp");
                _document.ImportIcon("C:\\Users\\Tarnogradskij\\Desktop\\импорт картинок\\Иконки\\256.bmp");
                _document.Save();
            }
            _document.EndChanges();
            
        }
    }
}
