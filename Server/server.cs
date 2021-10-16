using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class serverForm : Form
    {
        const int PORT = 30120;
        const string IPADRESS = "127.0.0.1";
        IPAddress localAdd;
        TcpListener listener;
        TcpClient cl;
        Task th;
        NetworkStream nwStream;
        public serverForm()
        {
            InitializeComponent();
        }

        private void server_Load(object sender, EventArgs e)
        {
            localAdd = IPAddress.Parse(IPADRESS);
            listener = new TcpListener(localAdd, PORT);
            listener.Start();
            cl = listener.AcceptTcpClient();
            label1.Text = "Server Started !";
            th = new Task(lictinerThread);
            th.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string textToSend = textBox1.Text;
            if (nwStream.CanWrite)
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                richTextBox1.Text = richTextBox1.Text + "You : " + textToSend + "\n";
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        private void lictinerThread()
        {
            while (true)
            {
                nwStream = cl.GetStream();

                if (cl != null)
                {
                    if (!cl.Connected)
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            cl = listener.AcceptTcpClient();
                            label1.ForeColor = Color.Red;
                            label1.Text = "Disconnected";
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

                if (nwStream.CanRead)
                {
                    byte[] buffer = new byte[cl.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(buffer, 0, cl.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);


                    this.Invoke((MethodInvoker)delegate ()
                    {
                        richTextBox1.Text = richTextBox1.Text + "" + cl.Client.LocalEndPoint + " : " + dataReceived + "\n";
                    });
                }

                Thread.Sleep(1);
            }
        }
    }
}
