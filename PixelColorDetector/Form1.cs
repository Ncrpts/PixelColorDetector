using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;

namespace PixelColorDetector
{
    public partial class Form1 : Form
    {
        FormViseur viseur;

        //System.Media.SoundPlayer playerViolet;
        //System.Media.SoundPlayer playerRouge;


        WaveOutEvent outputDeviceViolet, outputDeviceRouge;


        


        public Form1()
        {
            InitializeComponent();

            //playerViolet = new System.Media.SoundPlayer();
            //playerRouge = new System.Media.SoundPlayer();

            outputDeviceViolet = new WaveOutEvent();
            outputDeviceRouge = new WaveOutEvent();
            InitializeAudioOutComboBox();
        }

        private void InitializeAudioOutComboBox()
        {
            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                comboBoxDevice.Items.Add(caps.ProductName);
                //Console.WriteLine($"{n}: {caps.ProductName}");
            }

            if (comboBoxDevice.Items.Count >= Properties.Settings.Default.PlaybackDeviceIndex+1)
                comboBoxDevice.SelectedIndex = Properties.Settings.Default.PlaybackDeviceIndex;
            else
                comboBoxDevice.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            viseur = new FormViseur(this);

            if (Properties.Settings.Default.ViseurShown)
            {
                checkBox1.Checked = true;
                viseur.Show();
                Point newPoint = new Point(Properties.Settings.Default.Xcoord - 25, Properties.Settings.Default.Ycoord - 25);
                viseur.Location = newPoint;
            }
            else
                checkBox1.Checked = false;

            textBoxXCoord.Text = Properties.Settings.Default.Xcoord.ToString();
            textBoxYCoord.Text = Properties.Settings.Default.Ycoord.ToString();

            textBox1.Text = Properties.Settings.Default.SonViolet;
            textBox2.Text = Properties.Settings.Default.SonRouge;

            checkBoxUseVioletSound.Checked = Properties.Settings.Default.useSonViolet;
            checkBoxUseRougeSound.Checked = Properties.Settings.Default.useSonRouge;

            checkBox2.Checked = Properties.Settings.Default.useAutoScan;

            numericUpDown1.Value = Properties.Settings.Default.AutoScanTimerValue;
            trackBar1.Value = Properties.Settings.Default.AutoScanTimerValue;
            timer1.Interval = Properties.Settings.Default.AutoScanTimerValue;

            trackBarVolumeRouge.Value = (int) Properties.Settings.Default.VolumeRouge;
            trackBarVolumeViolet.Value = (int)Properties.Settings.Default.VolumeViolet;


            ResetSoundFiles();
            
            buttonColorPickerViolet.BackColor = ColorTranslator.FromHtml(Properties.Settings.Default.SavedVioletColorStr);
            buttonColorPickerRouge.BackColor = ColorTranslator.FromHtml(Properties.Settings.Default.SavedRougeColorStr);
            //ChangeTextBoxCoords(viseur.Location.X - 25, viseur.Location.Y - 25);

        }

        private void PerformSingleScan()
        {
            int xCoord = Int32.Parse(textBoxXCoord.Text);
            int yCoord = Int32.Parse(textBoxYCoord.Text);

            Color retrievedColor = GetColorAt(xCoord, yCoord);

            panelColorIndicator.BackColor = retrievedColor;
            labelColor.Text = "Color: " + ColorTranslator.ToHtml(retrievedColor);
            labelColor.ForeColor = retrievedColor;
            CheckColor(retrievedColor);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PerformSingleScan();
        }

        private bool soundVioletAlreadyPlayed = false;
        private bool soundRedAlreadyPlayed = false;

        private bool soundVioletSet = false;
        private bool soundRougeSet = false;


        private void CheckColor(Color color)
        {
            if(IsColorVioletTrigger(color))
            {
                labelFoundColor.Text = "Couleur détecté : " + ColorTranslator.ToHtml(color);
                labelFoundColor.ForeColor = color;
                soundRedAlreadyPlayed = false;
                if (soundVioletAlreadyPlayed == false && checkBoxUseVioletSound.Checked && soundVioletSet)
                {
                    if(outputDeviceViolet.PlaybackState == PlaybackState.Stopped)
                    {
                        outputDeviceViolet.DeviceNumber = Properties.Settings.Default.PlaybackDeviceIndex;
                        audioFileViolet.Volume = trackBarVolumeViolet.Value / 100.0f;
                        outputDeviceViolet.Init(audioFileViolet);
                    }

                    //System.Media.SystemSounds.Asterisk.Play();
                    //playerViolet.Play();
                    outputDeviceViolet.Stop();
                    audioFileViolet.Position = 0;
                    outputDeviceViolet.Play();
                    soundVioletAlreadyPlayed = true;
                    
                }
            }
            else if(IsColorRedTrigger(color))
            {
                labelFoundColor.Text = "Couleur détecté : " + ColorTranslator.ToHtml(color);
                labelFoundColor.ForeColor = color;
                soundVioletAlreadyPlayed = false;
                if (soundRedAlreadyPlayed == false && checkBoxUseRougeSound.Checked && soundRougeSet)
                {
                    if (outputDeviceRouge.PlaybackState == PlaybackState.Stopped)
                    { 
                        outputDeviceRouge.DeviceNumber = Properties.Settings.Default.PlaybackDeviceIndex;
                        audioFileRouge.Volume = trackBarVolumeRouge.Value / 100.0f;
                        outputDeviceRouge.Init(audioFileRouge);
                    }

                    outputDeviceRouge.Stop();
                    audioFileRouge.Position = 0;
                    outputDeviceRouge.Play();
                    //System.Media.SystemSounds.Asterisk.Play();
                    //playerRouge.Play();
                    soundRedAlreadyPlayed = true;
                }
            }
            else
            {
                labelFoundColor.Text = "";
                soundRedAlreadyPlayed = false;
                soundVioletAlreadyPlayed = false;
            }
        }

        private bool IsColorVioletTrigger(Color color)
        {
            if (ColorTranslator.ToHtml(color) == Properties.Settings.Default.SavedVioletColorStr)
                return true;
            else
                return false;
        }

        private bool IsColorRedTrigger(Color color)
        {
            if (ColorTranslator.ToHtml(color) == Properties.Settings.Default.SavedRougeColorStr)
                return true;
            else
                return false;
        }



        public void ChangeTextBoxCoords(int x, int y)
        {
            textBoxXCoord.Text = x.ToString();
            textBoxYCoord.Text = y.ToString();

        }

        Bitmap bmp = new Bitmap(1, 1);

        Color GetColorAt(int x, int y)
        {
            Rectangle bounds = new Rectangle(x, y, 1, 1);
            using (Graphics g = Graphics.FromImage(bmp))
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            return bmp.GetPixel(0, 0);
        }

        private void textBoxXCoord_TextChanged(object sender, EventArgs e)
        {
            if(Int32.TryParse(textBoxXCoord.Text, out int parsed))
            { 
                Point newPoint = new Point(parsed - 25, viseur.Location.Y);
                viseur.Location = newPoint;
                Properties.Settings.Default.Xcoord = parsed;
                Properties.Settings.Default.Save();
            }
        }

        private void textBoxYCoord_TextChanged(object sender, EventArgs e)
        {
            if (Int32.TryParse(textBoxYCoord.Text, out int parsed))
            {
                Point newPoint = new Point(viseur.Location.X, parsed - 25);
                viseur.Location = newPoint;
                Properties.Settings.Default.Ycoord = parsed;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                viseur.Hide();
                Properties.Settings.Default.ViseurShown = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                viseur.Show();
                Point newPoint = new Point(Properties.Settings.Default.Xcoord - 25, Properties.Settings.Default.Ycoord - 25);
                viseur.Location = newPoint;
                Properties.Settings.Default.ViseurShown = true;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e) //check if user want automatic scan
        {
            Properties.Settings.Default.useAutoScan = checkBox2.Checked;
            Properties.Settings.Default.Save();
            switchAutoScan();
        }

        private void switchAutoScan()
        {
            if (checkBox2.Checked == true)
            {
                button1.Enabled = false;
                trackBar1.Enabled = true;
                numericUpDown1.Enabled = true;
                label3.Enabled = true;
                timer1.Enabled = true;
            }
            else
            {
                button1.Enabled = true;
                trackBar1.Enabled = false;
                numericUpDown1.Enabled = false;
                label3.Enabled = false;
                timer1.Enabled = false;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            numericUpDown1.Value = trackBar1.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int) numericUpDown1.Value;
            timer1.Interval = (int) numericUpDown1.Value;
            Properties.Settings.Default.AutoScanTimerValue = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            PerformSingleScan();
        }

        private void labelColor_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenSoundFile("Violet");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenSoundFile("Rouge");
        }

        private void OpenSoundFile(string strColor)
        {
            TextBox textBoxToUpdate;
            if (strColor == "Violet")
            {
                textBoxToUpdate = textBox1;
            }
            else if (strColor == "Rouge")
            {
                textBoxToUpdate = textBox2;
            }
            else
                return;

            openFileDialog1.Title = "Choisissez un fichier de Son à utiliser pour la couleur " + strColor;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.  
                textBoxToUpdate.Text = openFileDialog1.FileName;
                ResetSoundFiles();
            }

        }

        AudioFileReader audioFileViolet, audioFileRouge;

        private void ResetSoundFiles()
        {
            if(textBox1.Text != "(pas de son)")
            {
                soundVioletSet = true;
                //playerViolet.SoundLocation = @textBox1.Text;
                audioFileViolet = new AudioFileReader(@textBox1.Text);
                
                Properties.Settings.Default.SonViolet = @textBox1.Text;
                Properties.Settings.Default.Save();
            }

            if (textBox2.Text != "(pas de son)")
            {
                soundRougeSet = true;
                //playerRouge.SoundLocation = @textBox2.Text;
                audioFileRouge = new AudioFileReader(@textBox2.Text);

                Properties.Settings.Default.SonRouge = @textBox2.Text;
                Properties.Settings.Default.Save();
            }

            buttonTestSonRouge.Enabled = soundRougeSet;
            buttonTestSonViolet.Enabled = soundVioletSet;

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        { 
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
            
        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void toolStripMenuItemQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void comboBoxDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PlaybackDeviceIndex = comboBoxDevice.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void ColorPicking_Click(object sender, EventArgs e)
        {
            string senderName = ((Button)sender).Name;

            if (senderName == "buttonColorPickerViolet")
            {
                colorDialog1.Color = buttonColorPickerViolet.BackColor;
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    buttonColorPickerViolet.BackColor = colorDialog1.Color;
                    Properties.Settings.Default.SavedVioletColorStr = ColorTranslator.ToHtml(colorDialog1.Color);
                }
            }
            else if (senderName == "buttonColorPickerRouge")
            {
                colorDialog1.Color = buttonColorPickerRouge.BackColor;
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    buttonColorPickerRouge.BackColor = colorDialog1.Color;
                    Properties.Settings.Default.SavedRougeColorStr = ColorTranslator.ToHtml(colorDialog1.Color);
                }
                
            }
        }

        private void buttonSaveToViolet_Click(object sender, EventArgs e)
        {
            if (buttonColorPickerViolet.BackColor == labelColor.ForeColor)
                return;


            var confirmResult = MessageBox.Show("Êtes-vous sûr de vouloir remplacer la detection de " + ColorTranslator.ToHtml(buttonColorPickerViolet.BackColor) + " par " + ColorTranslator.ToHtml(labelColor.ForeColor) + "?",
                                     "Confirmation de changement de Couleur",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                buttonColorPickerViolet.BackColor = labelColor.ForeColor;
                Properties.Settings.Default.SavedVioletColorStr = ColorTranslator.ToHtml(buttonColorPickerViolet.BackColor);
                Properties.Settings.Default.Save();
            }
            
        }

        private void buttonSaveToRouge_Click(object sender, EventArgs e)
        {
            if (buttonColorPickerRouge.BackColor == labelColor.ForeColor)
                return;

            var confirmResult = MessageBox.Show("Êtes-vous sûr de vouloir remplacer la detection de " + ColorTranslator.ToHtml(buttonColorPickerRouge.BackColor) + " par " + ColorTranslator.ToHtml(labelColor.ForeColor) + "?",
                                     "Confirmation de changement de Couleur",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                buttonColorPickerRouge.BackColor = labelColor.ForeColor;
                Properties.Settings.Default.SavedRougeColorStr = ColorTranslator.ToHtml(buttonColorPickerRouge.BackColor);
                Properties.Settings.Default.Save();
            }

        }

        private void buttonTestSonViolet_Click(object sender, EventArgs e)
        {
            if (soundVioletSet)
            {
                if (outputDeviceViolet.PlaybackState == PlaybackState.Stopped)
                {
                    outputDeviceViolet.DeviceNumber = Properties.Settings.Default.PlaybackDeviceIndex;
                    audioFileViolet.Volume = trackBarVolumeViolet.Value / 100.0f;
                    outputDeviceViolet.Init(audioFileViolet);
                }

                outputDeviceViolet.Stop();
                audioFileViolet.Position = 0;
                outputDeviceViolet.Play();
            }
        }

        private void buttonTestSonRouge_Click(object sender, EventArgs e)
        {
            if (soundRougeSet)
            {
                if (outputDeviceRouge.PlaybackState == PlaybackState.Stopped)
                {
                    outputDeviceRouge.DeviceNumber = Properties.Settings.Default.PlaybackDeviceIndex;
                    audioFileRouge.Volume = trackBarVolumeRouge.Value / 100.0f;
                    outputDeviceRouge.Init(audioFileRouge);
                }

                outputDeviceRouge.Stop();
                audioFileRouge.Position = 0;
                outputDeviceRouge.Play();
            }
        }

        private void trackBarVolumeViolet_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBarVolumeViolet_ValueChanged(object sender, EventArgs e)
        {
            //outputDeviceViolet.Volume = trackBarVolumeViolet.Value / 100.0f;
            if(soundVioletSet)
                audioFileViolet.Volume = trackBarVolumeViolet.Value / 100.0f;

            Properties.Settings.Default.VolumeViolet = trackBarVolumeViolet.Value;
            Properties.Settings.Default.Save();
        }

        private void trackBarVolumeRouge_ValueChanged(object sender, EventArgs e)
        {
            if (soundRougeSet)
                audioFileRouge.Volume = trackBarVolumeRouge.Value / 100.0f;

            Properties.Settings.Default.VolumeRouge = trackBarVolumeRouge.Value;
            Properties.Settings.Default.Save();
        }

        private void checkBoxUseVioletSound_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.useSonViolet = checkBoxUseVioletSound.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBoxUseRougeSound_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.useSonRouge = checkBoxUseRougeSound.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
