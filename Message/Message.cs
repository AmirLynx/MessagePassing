using System;
using System.Collections.Generic;

namespace Message
{
    public enum MessageType
    {
        forward = 0,
        server,
        connect,
        disconnect,
        update,
        response
    }
    public class MessageClass
    {
        public MessageType type { get; set; }
        public string jsonString { get; set; }
        /// <summary>
        /// Basic Message Template
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonString">Conver other templates to base template</param>
        public MessageClass(MessageType type, string jsonString)
        {
            this.type = type;
            this.jsonString = jsonString;
        }
    }
    public class ForwardClass
    {
        public int from { get; set; }
        public int to { get; set; }
        public string message { get; set; }
        /// <summary>
        /// Forward message template
        /// </summary>
        /// <param name="from">Sender id</param>
        /// <param name="to">Reciver id</param>
        /// <param name="message">Message</param>

        public ForwardClass(int from, int to, string message)
        {
            this.from = from;
            this.to = to;
            this.message = message;
        }
    }
    public class ServerClass
    {
        public int from { get; set; }
        public string message { get; set; }
        /// <summary>
        /// Server message template
        /// </summary>
        /// <param name="from">Sender id</param>
        /// <param name="message">Message</param>
        public ServerClass(int from, string message)
        {
            this.from = from;
            this.message = message;
        }
    }
    public class UpdateClass
    {
        public int from { get; set; }
        public List<int> clients { get; set; }
        public UpdateClass(int from, List<int> clients)
        {
            this.from = from;
            this.clients = clients;
        }
    }
    public class ConnectClass
    {
        public bool isConnect { get; set; }
        public int id { get; set; }
        /// <summary>
        /// Connection response
        /// </summary>
        /// <param name="isConnect">Is connect or not</param>
        /// <param name="id">Sender id</param>
        public ConnectClass(bool isConnect, int id)
        {
            this.isConnect = isConnect;
            this.id = id;
        }
    }
    public class ResponseClass
    {
        public string message { get; set; }
    }
    public class DisconnectClass
    {
        public bool isDisconnect { get; set; }
        public int id { get; set; }
        /// <summary>
        /// Connection response
        /// </summary>
        /// <param name="isConnect">Is connect or not</param>
        /// <param name="id">Sender id</param>
        public DisconnectClass(bool isConnect, int id)
        {
            this.isDisconnect = isConnect;
            this.id = id;
        }
    }
    public class InfoClass
    {
        public string data { get; set; }
    }

    public class typeEventArgs : EventArgs
    {
        private readonly MessageType _msgType;

        public typeEventArgs(MessageType type)
        {
            _msgType = type;
        }

        public MessageType GetMessageType
        {
            get { return _msgType; }
        }
            
    }
}
