using System;
using System.Collections.Generic;
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
        private const int PORT = 30120;
        private const string IPADRESS = "127.0.0.1";

        private TcpListener _serverSocket;
        private TcpClient _clientSocket;

        private List<ClientClass> _clients = new List<ClientClass>();

        public serverForm()
        {
            InitializeComponent();
        }

        private void server_Load(object sender, EventArgs e)
        {
            logListView.Text = logListView.Text + "Server Loaded ...";
            _serverSocket = new TcpListener(IPAddress.Parse(IPADRESS), PORT);
            _serverSocket.Start();
            logListView.Text = logListView.Text + "\nServer Started .";
            _clientSocket = new TcpClient();
            Task serverSocketTask = new Task(acceptClients);
            serverSocketTask.Start();
        }

        private void acceptClients()
        {
            int currentId = 0;
            while (true)
            {
                _clientSocket = _serverSocket.AcceptTcpClient();
                var clClass = new ClientClass();
                this.Invoke((MethodInvoker)delegate ()
                {
                    logListView.Text = logListView.Text + "\n" + _clientSocket.Client.LocalEndPoint + " Connected";
                    currentId = clientComboBox.Items.Add(_clientSocket.Client.Handle);
                    if (currentId == 0)
                    {
                        clientComboBox.SelectedIndex = currentId;
                    }
                    _clients.Add(clClass);
                    logListView.Text = logListView.Text + "\n" + _clientSocket.Client.Handle + " added";
                });
                clClass.startClient(_clientSocket, currentId, chatEvent, clientMessage);

                var firstData = _clientSocket.GetStream();
                var clientIdes = _clients.Select(x => x.clientNo).ToList();
                var jsonClientIdes = Newtonsoft.Json.JsonConvert.SerializeObject(clientIdes);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes("$$"+jsonClientIdes+"##");
                firstData.Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        private void clientMessage(object sender, EventArgs e)
        {
            var data = sender as messagePassData;
            if (_clients.Exists(x => x.clientNo == data.from) && _clients.Exists(x => x.clientNo == data.to))
            {
                var from = _clients.Where(x => x.clientNo == data.from).FirstOrDefault();
                var to = _clients.Where(x => x.clientNo == data.to).FirstOrDefault();
                if (from.clientSocket.Connected)
                {
                    if (to.clientSocket.Connected)
                    {
                        to.sendMessage(data.message, from.clientNo.ToString());
                    }
                    else
                    {
                        from.sendMessage("This Client not online");
                    }
                }
                else
                {
                    from.sendMessage("Wrong in progress");
                }
            }
        }

        private void chatEvent(object sender, EventArgs e)
        {
            var currentId = -1;
            this.Invoke((MethodInvoker)delegate ()
            {
                currentId = clientComboBox.SelectedIndex;
            });
            var cl = sender as ClientClass;
            if (cl.clientNo == currentId)
            {
                if (cl.chats != null)
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        chatListView.ResetText();
                    });

                    var chat = cl.chats.ToList();
                    foreach (string message in chat)
                    {
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            chatListView.Text = chatListView.Text + message + "\n";
                        });
                    }
                }
            }
            else
            {
                MessageBox.Show("peyda nashode");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(textBox1.Text))
            {
                var currentId = clientComboBox.SelectedIndex;
                if (_clients.Exists(x => x.clientNo == currentId))
                {
                    _clients.Where(x => x.clientNo == currentId).FirstOrDefault().sendMessage(textBox1.Text);
                    textBox1.ResetText();
                    textBox1.Focus();
                }
            }
        }

        private void clientComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currentId = clientComboBox.SelectedIndex;
            if (_clients.Exists(x => x.clientNo == currentId))
            {
                var clientData = _clients.Where(x => x.clientNo == currentId).FirstOrDefault();
                chatListView.ResetText();
                if (clientData.chats != null)
                {
                    var chat = clientData.chats.ToList();
                    foreach (string message in chat)
                    {
                        chatListView.Text = chatListView.Text + message + "\n";
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
}
