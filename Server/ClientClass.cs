using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientClass
    {
        public int ID { get; set; }
        public int socketHandel { get; set; }
        private TcpClient socket { get; set; }
        private Thread threading { get; set; }
        private EventHandler readEvent { get; set; }
        private EventHandler logEvent { get; set; }
        private NetworkStream stream { get; set; }
        public List<string> chat { get; private set; }
        public bool onlineStatus { get; private set; }

        public ClientClass(int id, TcpClient client, EventHandler read, EventHandler log)
        {
            this.ID = id;
            this.socket = client;
            this.socketHandel = int.Parse(client.Client.Handle.ToString());
            this.readEvent = read;
            this.logEvent = log;
            this.chat = new List<string>();
            threading = new Thread(streamThread);
            threading.Start();
        }

        public void sendClientMessage(string jsonString)
        {
            byte[] sByte = ASCIIEncoding.ASCII.GetBytes(jsonString);
            stream.Write(sByte, 0, sByte.Length);
        }

        private void streamThread()
        {
            while (true)
            {
                if (socket.Connected)
                {
                    stream = socket.GetStream();
                    if (stream.CanRead)
                    {
                        byte[] buffer = new byte[socket.ReceiveBufferSize];
                        int bytesRead = stream.Read(buffer, 0, socket.ReceiveBufferSize);
                        string dataReceived = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead);
                        var messageData = JsonConvert.DeserializeObject<Message.MessageClass>(dataReceived);
                        readEvent(messageData.jsonString, new Message.typeEventArgs(messageData.type));
                    }
                    else
                    {
                        logEvent.Invoke($"error | {ID}({socketHandel}) : client cant read stram", EventArgs.Empty);
                    }
                }
                else
                {
                    socket.Close();
                    threading.Abort();
                }
            }
        }

        public void stopThraed()
        {
            logEvent.Invoke($"Client Log Out {this.socketHandel}", EventArgs.Empty);
            this.onlineStatus = false;
            this.threading.Abort();
        }

        public void changeStatus(bool stat)
        {
            this.onlineStatus = stat;
        }

        public bool isOnline()
        {
            return socket.Connected && threading.IsAlive;
        }
    }
}
