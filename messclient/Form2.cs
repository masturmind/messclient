using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace messclient
{
    //first window client needs to go through
    public partial class Form2 : Form
    {

        private IPAddress ip;
        private string userName { get; set; }
        bool correctIP = false, correctUsername = false, usernameUsed = true, connected = false;
        //used to check username correctness
        Regex r = new Regex("^[a-zA-Z0-9]*$");

        //"Exit" button
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
       private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            this.Close();
        }

            public Form2()
        {
            InitializeComponent();
            this.Text = "msgClient";
            label1.Text = "Please enter the IP address";
            label2.Text = "Please enter your username";
            textBox1.Text = "";
            textBox2.Text = "";
            AcceptButton = button1;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            usernameUsed = false;
            connected = false;
            string in1 = textBox1.Text;
            string in2 = textBox2.Text;
            //ip address check and username check are both performed
            if (IPAddress.TryParse(in1, out ip))
            {
                ip = IPAddress.Parse(in1);
                correctIP = true;
            }
            else
            {
                label1.Text = "Please enter a valid IPv4 address";
                correctIP = false;
                label3.Text = "";
            }

            if (r.IsMatch(in2) && in2.Length > 2 && in2.Length < 13)
            {
                userName = in2;
                correctUsername = true;
            }
            else
            {
                userName = in2; ////
                label2.Text = "Letters and numbers only, 3-12 chars";
                correctUsername = false;
                label3.Text = "";
            }
            //if both are correct, client will try to send a chosen username, which is 
            //anticipated by the server
            if (correctIP && correctUsername)
           {
                TcpClient clientsocket = new TcpClient();
                NetworkStream serverStream = default(NetworkStream);

                try
                {
                    //port 8888 is chosen for the app
                    clientsocket.Connect(ip, 8888);
                    connected = true;
                    serverStream = clientsocket.GetStream();
                    byte[] outStream = Encoding.Unicode.GetBytes("I:" + userName + "$");
                    serverStream.Write(outStream, 0, outStream.Length);
                    string answer = null;
                    //waiting for an answer from the server
                    while (true)
                    {
                        answer = "";

                        serverStream = clientsocket.GetStream();
                        byte[] inStream = new byte[10025];
                        serverStream.Read(inStream, 0, inStream.Length);
                        answer = Encoding.Unicode.GetString(inStream);
                        if (answer.StartsWith("I:"))
                        {
                            answer = answer.Substring(0, answer.IndexOf("$"));
                            answer = answer.Substring(2, answer.Length - 2);
                            //distinguishing whether the username is ok
                            if (answer == "UsernameExists") { usernameUsed = true; break; } 
                            if (answer == "UrOK") { break; } 
                        }
                        else
                        {
                            this.Hide();
                            var p = new PopForm("DunnoWhatHappened");
                            p.Closed += (s, args) => this.Close();
                            p.Show();

                        }
                    }
                }
                catch
                {
                    connected = false;
                }
                if (connected && !usernameUsed)
                {
                /*  this.Hide();
                  var f = new Form1(ip, userName, clientsocket);
                  f.Closed += (s, args) => this.Close();
                  f.Show();*/////
                this.Hide();
                var f = new Form3(ip,userName, clientsocket);
                f.Closed += (s, args) => this.Close();
                f.Show();

                }
                //if the connection attempt failed for some reason, e.g. server not running on a specified ip
                else if (!connected)
                {
                    label3.Text = "Could not connect";
                }
                else if (usernameUsed)
                {
                    this.Hide();
                    var p = new PopForm("Username exists");
                    p.Closed += (s, args) => this.Show();
                    p.Show();
                }
            }


        }



    }



}

