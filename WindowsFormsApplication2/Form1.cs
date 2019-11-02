using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Bitmap b;
        Graphics g;
        List<List<Point>> pos = new List<List<Point>>();
        List<PictureBox> pbs = new List<PictureBox>();
        List<Label> lbs = new List<Label>();
        List<Point> centerPos = new List<Point>();

        int counter = 0;

        double nextPageMax = 0;
        double nextPageMin = 0;

        bool is_autoClick = false;

        public Form1()
        {
            InitializeComponent();

            Location = new Point(0, 300);

            for (int i = 0; i < 4; i++)
            {
                pos.Add(new List<Point>());
            }

            pbs.Add(pictureBox2);
            pbs.Add(pictureBox3);
            pbs.Add(pictureBox4);
            pbs.Add(pictureBox5);

            lbs.Add(label2);
            lbs.Add(label3);
            lbs.Add(label4);
            lbs.Add(label5);

            b = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            g = Graphics.FromImage(b);
            g.CopyFromScreen(new Point(), new Point(), Screen.PrimaryScreen.Bounds.Size);
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            g.CopyFromScreen(new Point(), new Point(), Screen.PrimaryScreen.Bounds.Size);

            for (int i = 0; i < 4; i++)
            {
                pbs[i].Image = b.Clone(new Rectangle(pos[i][0].X, pos[i][0].Y, pos[i][1].X - pos[i][0].X, pos[i][1].Y - pos[i][0].Y), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }

            string ans = findDifferent(pbs, (int)numericUpDown2.Value).ToString();

            label1.Text = ans;

            bool next = is_next(pbs[3]);

            numericUpDown1.Value = (decimal)((nextPageMax + nextPageMin) / 2);

            button3.Enabled = next;

            int a = int.Parse(ans);

            if (a != -1)
            {
                label1.ForeColor = Color.Green;

                for (int i = 0; i < lbs.Count; i++)
                {
                    if (i == a)
                    {
                        lbs[i].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        lbs[i].BackColor = SystemColors.Control;
                    }
                }

                if (next)
                {

                    Point p = (Point)listBox1.Items[a * 2];
                    Point pp = (Point)listBox1.Items[a * 2 + 1];

                    int x = (p.X + pp.X) / 2;
                    int y = (p.Y + pp.Y) / 2;

                    Point ansPos = new Point(x, y);

                    Cursor.Position = ansPos;
                }
            }
            else
            {
                label1.Text = "找不到目標！";
                label1.ForeColor = Color.Red;
            }

            pictureBox1.BackColor = b.GetPixel(MousePosition.X, MousePosition.Y);

            label6.Text = b.GetPixel(MousePosition.X, MousePosition.Y).GetBrightness().ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(MousePosition);
            pos[counter / 2].Add(MousePosition);
            counter++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                button2.Text = "Stop";
                timer1.Start();
                timer2.Stop();
            }
            else
            {
                button2.Text = "Start";
                timer1.Stop();
                timer2.Start();
            }
        }

        private bool is_next(PictureBox pb)
        {
            float dif = 0;

            Bitmap b = new Bitmap(pb.Image);

            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    if (b.GetPixel(x, y).GetBrightness() > 0.9)
                    {
                        continue;
                    }

                    dif += b.GetPixel(x, y).GetHue();
                }
            }

            if (dif > nextPageMax)
            {
                nextPageMax = dif;
            }

            if (dif < nextPageMin || nextPageMin == 0)
            {
                nextPageMin = dif;
            }

            if (dif > (nextPageMax + nextPageMin) / 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int findDifferent(List<PictureBox> pb, int limit)
        {
            double[] dif = new double[4];

            for (int i = 0; i < 4; i++)
            {
                Bitmap b = new Bitmap(pb[i].Image);

                for (int x = 0; x < b.Width; x++)
                {
                    for (int y = 0; y < b.Height; y++)
                    {
                        if (b.GetPixel(x, y).GetBrightness() > 0.9)
                        {
                            continue;
                        }

                        dif[i] += b.GetPixel(x, y).GetHue();
                        dif[i] += b.GetPixel(x, y).GetSaturation();
                        dif[i] += b.GetPixel(x, y).GetBrightness();
                    }
                }

                lbs[i].Text = ((long)dif[i]).ToString();
            }

            if (Math.Abs(dif[0] - dif[1]) > limit && Math.Abs(dif[0] - dif[2]) > limit)
            {
                return 0;
            }
            else if (Math.Abs(dif[1] - dif[0]) > limit && Math.Abs(dif[1] - dif[2]) > limit)
            {
                return 1;
            }
            else if (Math.Abs(dif[2] - dif[0]) > limit && Math.Abs(dif[2] - dif[1]) > limit)
            {
                return 2;
            }

            return -1;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            g.CopyFromScreen(new Point(), new Point(), Screen.PrimaryScreen.Bounds.Size);
            label6.Text = b.GetPixel(MousePosition.X, MousePosition.Y).GetBrightness().ToString();
            pictureBox1.BackColor = b.GetPixel(MousePosition.X, MousePosition.Y);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "POS(*.pos)|*.pos";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(sfd.FileName);

                foreach (Point p in listBox1.Items)
                {
                    string c = p.X + "," + p.Y;
                    sw.WriteLine(c);
                }

                sw.WriteLine("chooseLimit:" + numericUpDown2.Value);

                sw.WriteLine("clickLimit:" + numericUpDown1.Value);

                sw.Close();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "POS(*.pos)|*.pos";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                resetValue();

                StreamReader sr = new StreamReader(ofd.FileName);

                while (!sr.EndOfStream)
                {
                    string readIn = sr.ReadLine();

                    if (readIn.IndexOf("chooseLimit:") != -1)
                    {
                        numericUpDown2.Value = int.Parse(readIn.Split(':')[1]);
                        continue;
                    }
                    else if (readIn.IndexOf("clickLimit:") != -1)
                    {
                        numericUpDown1.Value = int.Parse(readIn.Split(':')[1]);
                        continue;
                    }

                    string[] xy = readIn.Split(',');

                    Point p = new Point(int.Parse(xy[0]), int.Parse(xy[1]));

                    listBox1.Items.Add(p);
                    pos[counter / 2].Add(p);
                    counter++;
                }

                sr.Close();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            resetValue();
        }

        private void resetValue()
        {
            foreach (List<Point> l in pos)
            {
                l.Clear();
            }
            listBox1.Items.Clear();
            counter = 0;

            nextPageMax = 0;
            nextPageMin = 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            is_autoClick = checkBox2.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
