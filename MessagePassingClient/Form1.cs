using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MessagePassingClient
{
    // client 1
    public partial class Form1 : Form
    {
        private readonly int port = 5000;
        private readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private TcpListener listiner;
        private TcpClient client;
        public Form1()
        {
            InitializeComponent();

            Thread messageThread = new Thread(reciveMessage);
            listiner = new TcpListener(ip, port);
            listiner.Start();

            client = listiner.AcceptTcpClient();

            if (client.Connected)
            {
                messageThread.Start();
                this.Name = "Message Passing ( Connected )";
            }
            else
            {
                this.Name = "Message Passing ( not Connected )";
            }
        }

        private void reciveMessage()
        {
            if (client.Connected)
            {
                NetworkStream ns = client.GetStream();
                var buffer = new byte[client.ReceiveBufferSize];
                int br = ns.Read(buffer, 0, client.ReceiveBufferSize);
                string dr = Encoding.ASCII.GetString(buffer, 0, br);

                messageBox.Items.Add(dr);
            }
        }

        private void sendMessage(string msg)
        {
            if (client.Connected)
            {
                NetworkStream ns = client.GetStream();
                var bts = ASCIIEncoding.ASCII.GetBytes(msg);

                ns.Write(bts, 0 , bts.Length);
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) && string.IsNullOrWhiteSpace(textBox1.Text) && client.Connected)
            {
                sendMessage(textBox1.Text);
            }
        }
    }
}
