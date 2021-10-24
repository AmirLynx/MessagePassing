using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public class ClientClass
    {
        public TcpClient clientSocket { get; set; }
        public int clientNo { get; set; }
        public EventHandler chatEvent { get; set; }
        public EventHandler clientEvent { get; set; }
        public List<string> chats { get; set; }
        private NetworkStream ns { get; set; }

        public void startClient(TcpClient inClientSocket, int no, EventHandler chatEvent, EventHandler clientMessage)
        {
            this.clientSocket = inClientSocket;
            this.clientNo = no;
            this.chatEvent += chatEvent;
            this.clientEvent += clientMessage;
            chats = new List<string>();
            Thread clientThread = new Thread(processChat);
            clientThread.Start();
        }
        public void sendMessage(string text, string user = "Server")
        {
            text = user + " : " + text;
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(text);
            this.chats.Add(text);
            ns.Write(bytesToSend, 0, bytesToSend.Length);
            chatEvent.Invoke(this, EventArgs.Empty);
        }

        private void processChat()
        {
            Byte[] sendBytes = null;
            string serverResponse = null;

            while (true)
            {
                try
                {
                    ns = clientSocket.GetStream();
                    byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                    int bytesRead = ns.Read(buffer, 0, clientSocket.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (dataReceived.StartsWith("$$"))
                    {
                        var conv = Newtonsoft.Json.JsonConvert.DeserializeObject<messagePassData>(dataReceived.Replace("$", ""));
                        clientEvent.Invoke(new messagePassData() 
                        {
                            from = this.clientNo,
                            to = conv.to,
                            message = conv.message
                        }, EventArgs.Empty);
                    }
                    else
                    {
                        this.chats.Add("Client: " + dataReceived);
                        serverResponse = " (Recived)";
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        ns.Write(sendBytes, 0, sendBytes.Length);
                        ns.Flush();
                        chatEvent.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    this.chats.Add(ex.Message);
                }
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
