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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class client : Form
    {
        public class chat
        {
            public int id { get; set; }
            public List<string> chats { get; set; } = new List<string>();
            public chat(int id)
            {
                this.id = id;
                this.chats = new List<string>();
            }
        }

        private int PORT_NO = 30120;
        private IPAddress SERVER_IP = IPAddress.Parse("127.0.0.1");
        private TcpClient socket;
        private Thread straemThraed;
        private NetworkStream stream;
        private List<chat> _chats;
        private int currentId = -2;

        public client(IPAddress ip, int port)
        {
            PORT_NO = port;
            SERVER_IP = ip;
            InitializeComponent();
            _chats = new List<chat>();
        }

        private void client_Load(object sender, EventArgs e)
        {
            new Task(connectionTask).Start();
        }

        private async void connectionTask()
        {
            while (true)
            {
                if (socket != null)
                {
                    if (!socket.Connected)
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            socket = null;
                            straemThraed.Abort();
                            stream.Close();
                            label1.ForeColor = Color.Red;
                            label1.Text = "Disconnected !";
                        });

                    }
                    else
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            label1.ForeColor = Color.Green;
                            if (currentId == -2)
                            {
                                label1.Text = "Connected ";
                            }
                            else
                            {
                                label1.Text = "Connected ( Id : " + currentId + " )";
                            }
                        });
                    }
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        try
                        {
                            socket = new TcpClient(SERVER_IP.ToString(), PORT_NO);
                            if (socket != null)
                            {
                                straemThraed = new Thread(streamThread);
                                straemThraed.IsBackground = true;
                                straemThraed.Start();
                            }
                        }
                        catch (SocketException e)
                        {
                            label1.ForeColor = Color.Yellow;
                            label1.Text = e.Message;
                        }
                        catch (Exception e)
                        {
                            label1.ForeColor = Color.Orange;
                            label1.Text = e.Message;
                        }
                    });
                }
                await Task.Delay(5000);
            }
        }

        private void streamThread()
        {
            while (true)
            {
                Thread.Sleep(1);
                if (socket != null && socket.Connected)
                {
                    if (socket.Available > 0)
                    {
                        stream = socket.GetStream();
                        if (stream.CanRead)
                        {
                            byte[] bytesToRead = new byte[socket.ReceiveBufferSize];
                            int bytesRead = stream.Read(bytesToRead, 0, socket.ReceiveBufferSize);
                            this.Invoke((MethodInvoker)delegate ()
                            {
                                var serverMessage = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                                var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.MessageClass>(serverMessage);
                                switch (temp.type)
                                {
                                    case Message.MessageType.forward:
                                        var tempForward = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.ForwardClass>(temp.jsonString);
                                        int selectedIdFW = int.Parse(comboBox1.SelectedItem.ToString());
                                        var cht = _chats.Where(x => x.id == tempForward.from).FirstOrDefault();
                                        if (cht == null)
                                        {
                                            cht.chats = new List<string>();
                                            cht.chats.Add(tempForward.from + " : " + tempForward.message + "\n");
                                        }
                                        else
                                        {
                                            cht.chats.Add(tempForward.from + " : " + tempForward.message + "\n");
                                        }
                                        if (selectedIdFW == tempForward.from)
                                        {
                                            richTextBox1.Text = richTextBox1.Text + tempForward.from + " : " + tempForward.message + "\n";
                                            richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                            richTextBox1.ScrollToCaret();
                                        }
                                        break;
                                    case Message.MessageType.server:
                                        var tempServer = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.ServerClass>(temp.jsonString);
                                        int selectedIdSV = int.Parse(comboBox1.SelectedItem.ToString());
                                        var chtsv = _chats.Where(x => x.id == -1).FirstOrDefault();
                                        if (chtsv == null)
                                        {
                                            chtsv.chats = new List<string>();
                                            chtsv.chats.Add("Server : " + tempServer.message + "\n");
                                        }
                                        else
                                        {
                                            chtsv.chats.Add("Server : " + tempServer.message + "\n");
                                        }
                                        if (selectedIdSV == -1)
                                        {
                                            richTextBox1.Text = richTextBox1.Text + "Server : " + tempServer.message + "\n";
                                            richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                            richTextBox1.ScrollToCaret();
                                        }
                                        break;
                                    case Message.MessageType.update:
                                        var tempUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.UpdateClass>(temp.jsonString);
                                        currentId = tempUpdate.from;
                                        List<chat> oldChatList = new List<chat>();

                                        if (_chats.Count > 0)
                                        {
                                            oldChatList.AddRange(_chats);
                                            _chats.Clear();
                                        }

                                        comboBox1.Items.Clear();

                                        foreach (var item in tempUpdate.clients)
                                        {
                                            comboBox1.Items.Add(item);
                                            if (oldChatList.Count > 0 && oldChatList.Exists(x => x.id == item))
                                            {
                                                var cChat = new chat(item);
                                                cChat.chats = oldChatList.Where(x => x.id == item).FirstOrDefault().chats;
                                                _chats.Add(cChat);
                                            }
                                            else
                                            {
                                                richTextBox1.Text = richTextBox1.Text + item;
                                                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                                richTextBox1.ScrollToCaret();
                                                _chats.Add(new chat(item));
                                            }
                                        }

                                        comboBox1.SelectedIndex = 0;
                                        break;
                                    case Message.MessageType.response:
                                        break;
                                    default:
                                        break;
                                }
                            });
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && socket != null && socket.Connected)
            {
                string textToSend = textBox1.Text;

                if (stream.CanWrite)
                {
                    int selectedId = int.Parse(comboBox1.SelectedItem.ToString());
                    if (selectedId == -1)
                    {
                        var tempServer = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.ServerClass(currentId, textToSend));
                        var tempMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.server, tempServer));
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(tempMessage);
                        richTextBox1.Text = richTextBox1.Text + ("You : " + textToSend + "\n");
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        var cht = _chats.Where(x => x.id == -1).FirstOrDefault();
                        if (cht == null)
                        {
                            cht.chats = new List<string>();
                            cht.chats.Add("You : " + textToSend + "\n");
                        }
                        else
                        {
                            cht.chats.Add("You : " + textToSend + "\n");
                        }
                        stream.Write(bytesToSend, 0, bytesToSend.Length);
                        textBox1.ResetText();
                    }
                    else
                    {
                        var tempServer = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.ForwardClass(currentId, selectedId, textToSend));
                        var tempMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.forward, tempServer));
                        byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(tempMessage);
                        richTextBox1.Text = richTextBox1.Text + ("You : " + textToSend + "\n");
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                        var cht = _chats.Where(x => x.id == selectedId).FirstOrDefault();
                        if (cht.chats == null)
                        {
                            cht.chats = new List<string>();
                            cht.chats.Add("You : " + textToSend + "\n");
                        }
                        else
                        {
                            cht.chats.Add("You : " + textToSend + "\n");
                        }
                        stream.Write(bytesToSend, 0, bytesToSend.Length);
                        textBox1.ResetText();
                    }
                }
                else
                {
                    label1.ForeColor = Color.Orange;
                    label1.Text = "Reconnecting . . .";
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

        private void client_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (socket != null && socket.Connected)
            {
                var tempServer = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.DisconnectClass(true, currentId));
                var tempMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.disconnect, tempServer));
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(tempMessage);
                stream.Write(bytesToSend, 0, bytesToSend.Length);
                Thread.Sleep(1000);
            }
            Process.GetCurrentProcess().Kill();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            var comboVal = int.Parse(cmb.SelectedItem.ToString());
            richTextBox1.ResetText();
            var selecteChats = _chats.Where(x => x.id == comboVal).FirstOrDefault();
            if (selecteChats != null && selecteChats.chats != null && selecteChats.chats.Count > 0)
            {
                foreach (var item in selecteChats.chats)
                {
                    richTextBox1.Text = richTextBox1.Text + item + "\n";
                }
            }
        }

        private void btn_changeIp_Click(object sender, EventArgs e)
        {
            new PropertiesForm().Show();
            this.Hide();
        }
    }
}
