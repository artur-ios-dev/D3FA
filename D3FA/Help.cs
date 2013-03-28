using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace D3FA
{
    public partial class Help : Form
    {
        Button hasFocus = null;

        public Help()
        {
            InitializeComponent();

            button3.Text = Properties.Settings.Default.addLegMod + " + " + Properties.Settings.Default.addLegBind;
            button4.Text = Properties.Settings.Default.startTimerMod + " + " + Properties.Settings.Default.startTimerBind;
            button5.Text = Properties.Settings.Default.stopTimerMod + " + " + Properties.Settings.Default.stopTimerBind;
            button6.Text = Properties.Settings.Default.addDEMod + " + " + Properties.Settings.Default.addDEBind;
            button7.Text = Properties.Settings.Default.addEKMod + " + " + Properties.Settings.Default.addEKBind;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Close();
        }

        private void picturePayPal_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=A7GQAM2ZJ8D66");
        }

        private void label4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=A7GQAM2ZJ8D66");
        }

        private void Help_KeyUp(object sender, KeyEventArgs e)
        {
            String tmp;
            tmp = e.Modifiers.ToString() + " + " + e.KeyCode.ToString();

            if (hasFocus != null)
            {
                hasFocus.Text = tmp;
                if (hasFocus.Name == "button3")
                {
                    Properties.Settings.Default.addLegMod = e.Modifiers.ToString();
                    Properties.Settings.Default.addLegBind = e.KeyCode.ToString();
                }
                else if (hasFocus.Name == "button4")
                {
                    Properties.Settings.Default.startTimerMod = e.Modifiers.ToString();
                    Properties.Settings.Default.startTimerBind = e.KeyCode.ToString();
                }
                else if (hasFocus.Name == "button5")
                {
                    Properties.Settings.Default.stopTimerMod = e.Modifiers.ToString();
                    Properties.Settings.Default.stopTimerBind = e.KeyCode.ToString();
                }
                else if (hasFocus.Name == "button6")
                {
                    Properties.Settings.Default.addDEMod = e.Modifiers.ToString();
                    Properties.Settings.Default.addDEBind = e.KeyCode.ToString();
                }
                else if (hasFocus.Name == "button7")
                {
                    Properties.Settings.Default.addEKMod = e.Modifiers.ToString();
                    Properties.Settings.Default.addEKBind = e.KeyCode.ToString();
                }

                hasFocus = null;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Text = " ... ";
            hasFocus = button3;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Text = " ... ";
            hasFocus = button4;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Text = " ... ";
            hasFocus = button5;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.Text = " ... ";
            hasFocus = button6;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            button7.Text = " ... ";
            hasFocus = button7;
        }
    }
}
