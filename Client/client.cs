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
                            label1.Text = "Disconnected";
                        });

                    }
                    else
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            label1.ForeColor = Color.Green;
                            label1.Text = "Connected to "+ cl.Client.RemoteEndPoint + " ( "+ cl.Client.Handle + " )";

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
                        var serverMessage = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                        if(serverMessage.StartsWith("$$") && serverMessage.EndsWith("##"))
                        {
                            var clients = serverMessage.Replace("$", "");
                            clients = clients.Replace("#", "");
                            var clientsId = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(clients);
                            foreach (var item in clientsId)
                            {
                                comboBox1.Items.Add("Client-" + item);
                            }
                        }
                        else
                        {
                            richTextBox1.Text = richTextBox1.Text + serverMessage + "\n";
                        }
                        
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
                    if (comboBox1.SelectedIndex == 0)
                    {
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                        richTextBox1.Text = richTextBox1.Text + ("You : " + textToSend);
                        nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                        textBox1.ResetText();
                    }
                    else
                    {
                        var st = Newtonsoft.Json.JsonConvert.SerializeObject(new messagePassData()
                        {
                            message = textToSend,
                            to = int.Parse(comboBox1.SelectedItem.ToString().Replace("Client-", ""))
                        });
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("$$" + st);
                        richTextBox1.Text = richTextBox1.Text + ("You : " + textToSend + "\n");
                        nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                        textBox1.ResetText();
                    }
                    
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, EventArgs.Empty);
            }
        }
    }
    public class messagePassData
    {
        public int from { get; set; }
        public int to { get; set; }
        public string message { get; set; }
    }
}
