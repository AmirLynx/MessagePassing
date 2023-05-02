using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class PropertiesForm : Form
    {
        public PropertiesForm()
        {
            InitializeComponent();
            label1.Text = label1.Text + " " + IPAddress.Any;
        }

        private void txt_ipaddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '1':
                    e.Handled = false;
                    break;
                case '2':
                    e.Handled = false;
                    break;
                case '3':
                    e.Handled = false;
                    break;
                case '4':
                    e.Handled = false;
                    break;
                case '5':
                    e.Handled = false;
                    break;
                case '6':
                    e.Handled = false;
                    break;
                case '7':
                    e.Handled = false;
                    break;
                case '8':
                    e.Handled = false;
                    break;
                case '9':
                    e.Handled = false;
                    break;
                case '0':
                    e.Handled = false;
                    break;
                case '.':
                    e.Handled = false;
                    break;
                case (char)Keys.Back:
                    e.Handled = false;
                    break;
                case (char)Keys.Tab:
                    e.Handled = false;
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }

        private void txt_port_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '1':
                    e.Handled = false;
                    break;
                case '2':
                    e.Handled = false;
                    break;
                case '3':
                    e.Handled = false;
                    break;
                case '4':
                    e.Handled = false;
                    break;
                case '5':
                    e.Handled = false;
                    break;
                case '6':
                    e.Handled = false;
                    break;
                case '7':
                    e.Handled = false;
                    break;
                case '8':
                    e.Handled = false;
                    break;
                case '9':
                    e.Handled = false;
                    break;
                case '0':
                    e.Handled = false;
                    break;
                case (char)Keys.Back:
                    e.Handled = false;
                    break;
                case (char)Keys.Tab:
                    e.Handled = false;
                    break;
                default:
                    e.Handled = true;
                    break;
            }
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_ipaddress.Text) && !string.IsNullOrEmpty(txt_port.Text))
            {
                try
                {
                    new serverForm(IPAddress.Parse(txt_ipaddress.Text), int.Parse(txt_port.Text)).Show();
                    this.Hide();
                }
                catch (Exception)
                {

                }

            }
        }

        private void lbl_exit_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_ipaddress.Text) && !string.IsNullOrEmpty(txt_port.Text))
            {
                try
                {
                    new serverForm(IPAddress.Any, 25565).Show();
                    this.Hide();
                }
                catch (Exception)
                {

                }

            }
        }
    }
}
