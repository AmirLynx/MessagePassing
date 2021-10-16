using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MessagePassing
{
    // client 2
    public partial class Form1 : Form
    {
        private readonly int port = 80;
        private readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        TcpListener listiner;
        TcpClient client;
        public Form1()
        {
            InitializeComponent();
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

        private void Form1_Load(object sender, System.EventArgs e)
        {
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
    }
}
