using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;
[assembly: CLSCompliant(true)]

namespace messclient
{

    public partial class Form1 : Form
    {

        private IPAddress ip;
        public static bool dying;
        public static bool dead;
        public static bool serverDead;
        private string username;
        private string readData = "";
        TcpClient clientSocket;
        NetworkStream serverStream = default(NetworkStream);
        public Form1(IPAddress ip, string username, TcpClient tcpclient, string room)
        {
            InitializeComponent();
            this.ip = ip;
            this.username = username;
            this.clientSocket = tcpclient;
            AcceptButton = button1;
            this.Text = " Connected to " + ip + " ||| Room: " + room;
        }
        //a void method for displaying the content of a global variable readData to the textBox
        private void msg()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
                textBox1.Text = textBox1.Text + Environment.NewLine + " >> " + readData;
        }

        //new thread started for the communication
        private void Form1_Load(object sender, EventArgs e)
        {

            readData = "Successfully connected";
            msg();
            label1.Text = "Client successfully connected to the server";

            dead = false;dying = false; serverDead = false;
            Thread ctThread = new Thread(getStuff);
            ctThread.Start();
        }    

        //"Send" button
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
                SendMessage(textBox2.Text);

            textBox2.Text = "";
            textBox2.Focus();
        }
        /*a character '$' is used as a terminator for every client-server message,
        also the message always starts with either "I:" or "M:", which specifies whether
        it's a text message from the client, or a background information for the app*/
        private void SendMessage(string msg)
        {
            try
            {
                serverStream = clientSocket.GetStream();
                byte[] outStream = Encoding.Unicode.GetBytes("M:" + msg + "$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            //handled the case when server is down
            catch
            {
                serverDead = true;
                PopForm p = new PopForm("Server down");
                p.ShowDialog();
                this.Hide();
                p.Closed += (s, args) => this.Close();
            }

        }
        private void SendInfo(string info)
        {
            try
            {
                serverStream = clientSocket.GetStream();
                byte[] outData = Encoding.Unicode.GetBytes("I:" + info + "$");
                serverStream.Write(outData, 0, outData.Length);
            }
            catch
            {
                serverDead = true;
            }
            
            if (info == "dying") dying = true;
        }
        private void getStuff()
        {
            string returndata;
            dying = false;
            dead = false;
            while (true)
            {
                
                //Thread.Sleep(500);
                returndata = "";
                if (dying)
                {
                    serverStream.Dispose();
                    clientSocket.Close();
                    dead = true;
                    break;

                }
              

                serverStream = clientSocket.GetStream();
                byte[] inStream = new byte[10025];
                try
                {
                    serverStream.Read(inStream, 0, inStream.Length);
                }
                //handled case when server down
                catch
                {
                    dead = true; break;
                }
                
                returndata = Encoding.Unicode.GetString(inStream);
                if (returndata.StartsWith("M:"))
                {
                    returndata = returndata.Substring(0, returndata.IndexOf("$"));
                    returndata = returndata.Substring(2, returndata.Length - 2);
                    readData = "" + returndata;
                    msg();
                }
                else if (returndata.StartsWith("I:"))
                {
                    returndata = returndata.Substring(0, returndata.IndexOf("$"));
                    returndata = returndata.Substring(2, returndata.Length - 2);
                    //handled the case when server is correctly shut down
                    if (returndata == "shutdown") { dying = true; serverDead = true; }
                }
            }
        }
        //always scrolls the textBox to the latest message
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }
        //"Disconnect" button
        private void button2_Click(object sender, EventArgs e)
        {
            if (!serverDead)
            {
                SendInfo("dying");
            }
            this.Hide();
            var f = new Form2();
            f.Closed += (s, args) => this.Close();
            f.Show();

        }
        bool isDead()
        { return dead;}
        //handled the case when the program is forced to close
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!serverDead)
            {
                SendInfo("dying");
            }
            //waits until an end of the network communication is confirmed
            SpinWait.SpinUntil(isDead);
            Application.Exit();
        }

    }
    }
