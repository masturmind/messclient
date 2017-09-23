using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace messclient
{
    public partial class Form3 : Form
    {
        TcpClient clientSocket;
        private string username;
        private IPAddress ip;
        bool closedByStupidUser = true;
        public Form3(IPAddress ip, string username, TcpClient clientSocket)
        {
            InitializeComponent();
            this.ip = ip;
            this.username = username;
            this.clientSocket = clientSocket;
            this.Text = " Connected to " + ip;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            listBox1.SelectedItem = listBox1.Items.IndexOf(0);   
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] inStream = new byte[10025];
            //server sends a list of all rooms separated by ':'
            serverStream.Read(inStream, 0, inStream.Length);
            string answer = Encoding.Unicode.GetString(inStream);


            if (answer.StartsWith("I:"))
            {
                answer = answer.Substring(0, answer.IndexOf("$"));
                answer = answer.Substring(2, answer.Length - 2);
                string[] rooms = answer.Split(':');
                //the rooms are added to the listBox
                foreach (string room in rooms)
                {
                    if(room != "")
                    listBox1.Items.Add(room);
                }


            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                NetworkStream serverStream = clientSocket.GetStream();
                //the chosen room is send to the server
                byte[] outStream = Encoding.Unicode.GetBytes("I:" + listBox1.SelectedItem.ToString() + "$");
                serverStream.Write(outStream, 0, outStream.Length);

                string answer = "";
                serverStream = clientSocket.GetStream();
                byte[] inStream = new byte[10025];
                //answer from the server is read
                serverStream.Read(inStream, 0, inStream.Length);
                answer = Encoding.Unicode.GetString(inStream);
                if (answer.StartsWith("I:"))
                {
                    answer = answer.Substring(0, answer.IndexOf("$"));
                    answer = answer.Substring(2, answer.Length - 2);
                    //handled the case when the room got removed while choosing
                    if (answer == "RoomGone")

                    {
                        this.Hide();
                        var p = new PopForm("This room dissapeared :/");
                        p.Closed += (s, args) => this.Show();
                        p.Show();
                    }
                    //if everything is ok, the chat window is showed
                    else
                    {
                        this.Hide();
                        var f = new Form1(ip, username, clientSocket, listBox1.SelectedItem.ToString());
                        f.Show();
                        closedByStupidUser = false;
                    }
                }
            }

        }
        //handled case when the app is forced to exit
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closedByStupidUser)
            {
                NetworkStream serverStream = clientSocket.GetStream();
                byte[] outStream = Encoding.Unicode.GetBytes("I:NOPE$");
                serverStream.Write(outStream, 0, outStream.Length);
                Application.Exit();
            }
        }
    }
}
