using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class client : Form
    {
        const int PORT_NO = 30120;
        const string SERVER_IP = "127.0.0.1";
        TcpClient cl;
        Task th;
        NetworkStream nwStream;
        public client()
        {
            InitializeComponent();
        }

        private void client_Load(object sender, EventArgs e)
        {
            cl = new TcpClient(SERVER_IP, PORT_NO);
            th = new Task(lictinerThread);
            th.Start();
        }

        private void lictinerThread()
        {
            while (true)
            {
                Thread.Sleep(1);
                if (cl != null)
                {
                    if (!cl.Connected)
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            cl = new TcpClient(SERVER_IP, PORT_NO);
                            label1.ForeColor = Color.Red;
                            label1.Text = "Disconnecte";
                        });

                    }
                    else
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            label1.ForeColor = Color.Green;
                            label1.Text = "Connected";

                        });
                    }
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        label1.ForeColor = Color.Orange;
                        label1.Text = "Wait for conenct";
                    });

                }
                
                nwStream = cl.GetStream();

                if (nwStream.CanRead)
                {
                    byte[] bytesToRead = new byte[cl.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, cl.ReceiveBufferSize);
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        richTextBox1.Text = richTextBox1.Text + "Server : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead) + "\n";
                    });
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && cl.Connected)
            {
                string textToSend = textBox1.Text;

                if (nwStream.CanWrite)
                {
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                    richTextBox1.Text = richTextBox1.Text + ("You : " + textToSend + "\n");
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                }
            }
        }
    }
}
