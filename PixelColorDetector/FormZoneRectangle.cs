using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelColorDetector
{
    public partial class FormZoneRectangle : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private Form1 mainForm;
        public FormZoneRectangle(Form1 senderForm)
        {
            InitializeComponent();
            mainForm = senderForm;
            this.TopMost = true;
            this.TransparencyKey = Color.LimeGreen;

            this.Size = new System.Drawing.Size(56 + Properties.Settings.Default.zoneWidth, 56 + Properties.Settings.Default.zoneHeight);
            

        }

        private void FormZoneRectangle_Load(object sender, EventArgs e)
        {
            
        }

        private void FormZoneRectangle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void FormZoneRectangle_ResizeEnd(object sender, EventArgs e)
        {
            int zoneHeight = this.Height - 56;
            int zoneWidth = this.Width - 56;
            Properties.Settings.Default.zoneHeight = zoneHeight;
            Properties.Settings.Default.zoneWidth = zoneWidth;
            Properties.Settings.Default.Save();

            //Debug.WriteLine(this.Width.ToString() + " width & " + this.Height.ToString() + " height");
            Debug.WriteLine(zoneWidth + " width & " + zoneHeight + " height");
        }

        private void FormZoneRectangle_LocationChanged(object sender, EventArgs e)
        {
            if (this.Focused)
                mainForm.ChangeTextBoxCoords(this.Location.X + 28, this.Location.Y + 28);
        }
    }
}
