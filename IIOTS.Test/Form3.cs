using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IIOTS.Util; 
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using IIOTS.Driver;
using IIOTS.Communication;

namespace IIOTS.Test
{

    public partial class Form3 : Form
    {

        public Form3()
        {
            InitializeComponent();
            socketServer.ReceiveEvent += SocketServer_ReceiveEvent  ;
        }

        private void SocketServer_ReceiveEvent (string ClientId, byte[] bytes)
        {
            this.Invoke(() =>
            {
                textBox2.Text = bytes.ToASCIIString();
            });
        }

        SocketServer socketServer = new SocketServer(2000)
        {
            HeadBytes = new byte[2] { 2, 3 },
            EndBytes = new byte[2] { 3, 2 }
        };
 



        private void button1_Click(object sender, EventArgs e)
        {
            socketServer.Send(new byte[2] { 2, 3 }.AddBytes(textBox1.Text.ToBytes()).AddBytes(new byte[2] { 3, 2 }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            socketClient.Send(new byte[2] { 2, 3 }.AddBytes(textBox3.Text.ToBytes()).AddBytes(new byte[2] { 3, 2 }));
        }
        SocketClient socketClient;
        private void button3_Click(object sender, EventArgs e)
        {
            socketClient = new SocketClient("127.0.0.1:2000")
            {
                LoginBytes = new byte[4] { 3, 2, 3, 2 },
                HeadBytes = new byte[2] { 2, 3 },
                EndBytes = new byte[2] { 3, 2 },
            };
            socketClient.ReceiveEvent += SocketClient_ReceiveEvent;
        }

        private void SocketClient_ReceiveEvent(Socket socket, byte[] bytes)
        {
            this.Invoke(() =>
            {
                textBox4.Text = bytes.ToASCIIString();
            });


        }
    }
}
