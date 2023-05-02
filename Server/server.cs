using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int PORT = 30120;
        private IPAddress IPADRESS = IPAddress.Parse("127.0.0.1");

        private TcpListener _serverSocket;
        private TcpClient _clientSocket;
        private int nextId = 0;

        private List<ClientClass> _clients = new List<ClientClass>();

        public serverForm(IPAddress ip, int port)
        {
            PORT = port;
            IPADRESS = ip;
            InitializeComponent();
        }

        private void server_Load(object sender, EventArgs e)
        {
            logListView.Text = logListView.Text + "Server Loaded ...";
            _serverSocket = new TcpListener(IPADRESS, PORT);
            logListView.Text = logListView.Text + "\nStablished connection ...";
            _serverSocket.Start();
            logListView.Text = logListView.Text + "\nServer started .";
            logListView.Text = logListView.Text + "\nip " + IPADRESS + ":" + PORT;
            _clientSocket = new TcpClient();
            Task serverSocketTask = new Task(acceptClients);
            serverSocketTask.Start();
        }

        private void acceptClients()
        {
            nextId = 0;
            while (true)
            {
                _clientSocket = _serverSocket.AcceptTcpClient();
                this.Invoke((MethodInvoker)delegate ()
                {
                    logListView.Text = logListView.Text + "\n" + _clientSocket.Client.LocalEndPoint + " Connected";
                    logListView.SelectionStart = logListView.Text.Length;
                    logListView.ScrollToCaret();

                    clientComboBox.Items.Add(_clientSocket.Client.Handle);
                    if (nextId == 0)
                    {
                        clientComboBox.SelectedIndex = 0;
                        label1.Text = "Current Client (" + nextId + ")";
                    }
                    var current = new ClientClass(nextId, _clientSocket, client_readEventHandler, server_logEventHandler);
                    nextId++;
                    var tempUpdateClass = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.ResponseClass() { message = "Conencted !"});
                    var tempMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.response, tempUpdateClass));
                    current.sendClientMessage(tempMessage);
                    _clients.Add(current);
                    server_updateClients();
                });
            }
        }

        private void server_updateClients()
        {
            foreach (var client in _clients)
            {
                var clientsIdies = _clients.Where(x => x.ID != client.ID).Select(x => x.ID).ToList();
                clientsIdies.Add(-1);
                var updateTemplate = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.UpdateClass(client.ID, clientsIdies));
                var messageTemplate = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.update, updateTemplate));
                client.sendClientMessage(messageTemplate);
            }
        }

        private void server_logEventHandler(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                logListView.Text = logListView.Text + "\n" + sender.ToString();
                logListView.SelectionStart = logListView.Text.Length;
                logListView.ScrollToCaret();
            });
        }

        private void client_readEventHandler(object sender, EventArgs e)
        {
            if (e != null)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    var tempType = e as Message.typeEventArgs;
                    switch (tempType.GetMessageType)
                    {
                        case Message.MessageType.forward:
                            var tempSender = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.ForwardClass>(sender.ToString());
                            var clientFrom = _clients.Exists(x => x.ID == tempSender.from);
                            if (clientFrom)
                            {
                                var toClient = _clients.Where(x => x.ID == tempSender.to).FirstOrDefault();
                                if (toClient != null)
                                {
                                    string tempForward = Newtonsoft.Json.JsonConvert.SerializeObject(tempSender);
                                    var tempMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.forward, tempForward));
                                    toClient.sendClientMessage(tempMessage);
                                    server_logEventHandler($"{tempSender.from} to {tempSender.to} (Message : {tempSender.message})", EventArgs.Empty);
                                }
                            }
                            break;
                        case Message.MessageType.server:
                            var tempServer = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.ServerClass>(sender.ToString());
                            var tempClient = _clients.Where(x => x.ID == tempServer.from).FirstOrDefault();
                            if (tempClient.isOnline())
                            {
                                tempClient.chat.Add($"{tempClient.socketHandel} : {tempServer.message}");
                                server_logEventHandler($"New Message From {tempClient.socketHandel}", EventArgs.Empty);
                                if (clientComboBox.SelectedItem.ToString() == tempClient.socketHandel.ToString())
                                {
                                    chatListView.Text = chatListView.Text + tempClient.socketHandel + " : " + tempServer.message + "\n";
                                    chatListView.SelectionStart = chatListView.Text.Length;
                                    chatListView.ScrollToCaret();
                                }

                            }
                            break;
                        case Message.MessageType.connect:
                            var tempConnect = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.ConnectClass>(sender.ToString());
                            var tempCoResponse = _clients.Where(x => x.ID == tempConnect.id).FirstOrDefault();
                            if (tempCoResponse != null)
                            {
                                tempCoResponse.changeStatus(tempConnect.isConnect);
                                server_updateClients();
                            }
                            break;
                        case Message.MessageType.disconnect:
                            var tempdconnect = Newtonsoft.Json.JsonConvert.DeserializeObject<Message.DisconnectClass>(sender.ToString());
                            var tempDcResponse = _clients.Where(x => x.ID == tempdconnect.id).FirstOrDefault();
                            if (tempDcResponse != null)
                            {
                                tempDcResponse.changeStatus(!tempdconnect.isDisconnect);
                                tempDcResponse.stopThraed();
                                _clients.Remove(tempDcResponse);
                                var currentId = int.Parse(clientComboBox.SelectedItem.ToString());
                                if (currentId == tempDcResponse.socketHandel)
                                {
                                    chatListView.ResetText();
                                }
                                clientComboBox.Items.RemoveAt(tempdconnect.id);
                                clientComboBox.ResetText();
                                server_updateClients();
                                label1.Text = "Current Client";
                            }
                            break;
                        default:
                            server_logEventHandler($"Message Type Error : {tempType.GetMessageType}", EventArgs.Empty);
                            break;
                    }
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(textBox1.Text))
            {
                var currentId = int.Parse(clientComboBox.SelectedItem.ToString());
                if (_clients.Exists(x => x.socketHandel == currentId))
                {
                    var selectedClient =  _clients.Where(x => x.socketHandel == currentId).FirstOrDefault();
                    selectedClient.chat.Add($"Server : {textBox1.Text}");
                    var tempServerMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.ServerClass(-1, textBox1.Text));
                    var tempMessage = Newtonsoft.Json.JsonConvert.SerializeObject(new Message.MessageClass(Message.MessageType.server, tempServerMessage));
                    selectedClient.sendClientMessage(tempMessage);
                    if (clientComboBox.SelectedItem.ToString() == selectedClient.socketHandel.ToString())
                    {
                        chatListView.Text = chatListView.Text + "Server : " + textBox1.Text + "\n";
                        chatListView.SelectionStart = chatListView.Text.Length;
                        chatListView.ScrollToCaret();
                    }
                    textBox1.ResetText();
                    textBox1.Focus();
                }
            }
        }

        private void clientComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var currentId = int.Parse(clientComboBox.SelectedItem.ToString());
            if (_clients.Exists(x => x.socketHandel == currentId))
            {
                var clientData = _clients.Where(x => x.socketHandel == currentId).FirstOrDefault();
                label1.Text = "Current Client (" + clientData.ID + ")";
                chatListView.ResetText();
                if (clientData.chat != null)
                {
                    var chat = clientData.chat.ToList();
                    foreach (string message in chat)
                    {
                        chatListView.Text = chatListView.Text + message + "\n";
                        chatListView.SelectionStart = chatListView.Text.Length;
                        chatListView.ScrollToCaret();
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

        private void serverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var item in _clients)
            {
                item.stopThraed();
            }
            Thread.Sleep(100);
            Process.GetCurrentProcess().Kill();
        }
    }
}
