using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;   // debug output

namespace oled
{
    public partial class Form1 : Form
    {
        const byte SCRSIZE = 128;
        const byte SC = SCRSIZE-1;
        const byte LEFT = 10;
        const byte TOP = 10;

        public System.Drawing.Graphics formGraphics, bmg;
        System.Drawing.SolidBrush delBrush;
        public Bitmap bm;

        Asteroids asteroids;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            formGraphics = this.CreateGraphics();
            delBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromName("Control"));
            
            bm = new Bitmap(this.Width, this.Height);
            bmg = Graphics.FromImage(bm);

            asteroids = new Asteroids(this);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            asteroids.init();
            asteroids.run();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            asteroids.stop();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            asteroids.update();
            //if(e.GetType() == typeof(PaintEventArgs))
            //    formGraphics.DrawRectangle(Pens.Black, LEFT-1, TOP-1, SCRSIZE+1, SCRSIZE+1);
        }

        bool sizeChanged = false;

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if(sizeChanged){
                formGraphics = this.CreateGraphics();
                bm = new Bitmap(this.Width, this.Height);
                bmg = Graphics.FromImage(bm);
                asteroids.reinit();
                sizeChanged = false;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            sizeChanged = true;
        }
    }
}

//------------------------------------------------------------------------------
