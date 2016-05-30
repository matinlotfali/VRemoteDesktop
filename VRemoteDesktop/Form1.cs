using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace VRemoteDesktop
{
    public class Message    //saakhtaare payaame json
    {
        public string MessageType;
        public string DesktopName;
        public string Button;
    }

    public partial class Form1 : Form
    {
        string connectedIP = null;  //addresse IP ke be an motasel hastim.
        DateTime lastMessageTime;   //zamane akharin payaame daryaft shode. 2 daqiqe ekhtelaafe mojaaz ast.
        Thread t;                   //thread halqeye bi nahayat entezaar vase payaam.
        UdpClient client = null;    //shenavandeye udp

        public Form1()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            stopButton.Enabled = true;
            startButton.Enabled = false;
            t = new Thread(() =>                                                //tarife thread
            {
                try
                {
                    client = new UdpClient(7711);                               //tanzim rooye port 7711
                    for (; ; )                                                  //ta abad becharkh
                    {
                        IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);       //az har IP
                        byte[] bytes = client.Receive(ref ip);                  //yek payaam daryaft kon va ferestande ro tooye ip beriz
                        string str = Encoding.UTF8.GetString(bytes);            //tarjome kon be String (encode UTF8)
                        string ipStr = ip.Address.ToString();                   //ip ferestande ro begir

                        JsonSerializerSettings settings = new JsonSerializerSettings(); //tanzimaate json
                        settings.NullValueHandling = NullValueHandling.Ignore;          //bikhiale moteghayere null besho
                        Message message = JsonConvert.DeserializeObject<Message>(str, settings);    //payaame json ro tarjome kon be objecte Message
                        switch (message.MessageType)                                                //baste be noe payaam tasmim begir
                        {
                            case "scan":                                        //agar device donbalet migasht
                                {
                                    if (connectedIP == null || connectedIP == ipStr || (DateTime.Now - lastMessageTime).TotalMinutes >= 2) //agar be kasi vasl naboodi
                                    {
                                        Message reply = new Message();                                  //payaam besaaz
                                        reply.MessageType = "replyScan";                                //noe payaam replyScan
                                        reply.DesktopName = textBox1.Text;                              //esmeto bezar toosh
                                        string data = JsonConvert.SerializeObject(reply, settings);     //Message ro tarjome kon be string ba json
                                        bytes = Encoding.UTF8.GetBytes(data);                           //string ro tarjome kon be byte (encode UTF8)
                                        client.Send(bytes, bytes.Length, ipStr, 7711);                  //ersal kon
                                        Console.ForegroundColor = ConsoleColor.Green;
                                    }
                                    else
                                        Console.ForegroundColor = ConsoleColor.Red;
                                }
                                break;

                            case "connect":                                 //agar device entekhaabet kard vase etesaal

                                if (connectedIP == null || connectedIP == ipStr || (DateTime.Now - lastMessageTime).TotalMinutes >= 2) //agar be kasi vasl naboodi
                                {
                                    BeginInvoke((MethodInvoker)delegate
                                        {
                                            if (MessageBox.Show("The IP " + ipStr + " wants to connect?", "Connect", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                            {
                                                Message reply = new Message();                              //payaam besaz
                                                reply.MessageType = "connected";                            //noe payaam connected
                                                string data = JsonConvert.SerializeObject(reply, settings); //Message ro tarjome kon be string ba json
                                                bytes = Encoding.UTF8.GetBytes(data);                       //string ro tarjome kon be byte (encode UTF8)
                                                client.Send(bytes, bytes.Length, ipStr, 7711);              //ersal kon
                                                connectedIP = ipStr;                                        //tanzim kon ke be che IP motasel shodi
                                                lastMessageTime = DateTime.Now;                             //tanzim kon akharin payaame daryafti key boode
                                            }
                                        });
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }
                                else
                                    Console.ForegroundColor = ConsoleColor.Red;
                                break;

                            case "buttonTouchDown":                             //agar dokmeyi zade shod (paayin ravande)

                                if (connectedIP == ipStr && (DateTime.Now - lastMessageTime).TotalMinutes < 2)  //agar be hamun device vasl boodi
                                {
                                    string button = message.Button;             //bebin kodoom dokme boode
                                    //
                                    //                                          harkari ke marboot be in dokme hast (etelaa resaani be computer)
                                    //
                                    lastMessageTime = DateTime.Now;             //tanzim kon akharin payaami daryafti key boode
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }
                                else
                                    Console.ForegroundColor = ConsoleColor.Red;
                                break;

                            case "buttonTouchUp":                               //agar dokmeyi ke zade shode bood vel shod (bala ravande)

                                if (connectedIP == ipStr && (DateTime.Now - lastMessageTime).TotalMinutes < 2)  //agar be hamun device vasl boodi
                                {
                                    string button = message.Button;             //bebin kodoom dokme boode
                                    //
                                    //                                          harkari ke marboot be in dokme hast (etelaa resaani be computer)
                                    //
                                    lastMessageTime = DateTime.Now;             //tanzim kon akharin payaami daryafti key boode 
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }
                                else
                                    Console.ForegroundColor = ConsoleColor.Red;
                                break;

                            case "stillConnected":                          //agar device etela dad ke hanooz behet vasle (har 1 daqiqe)
                            default:                                        //ya har payaame digeyi az device gerefti

                                if (connectedIP == ipStr && (DateTime.Now - lastMessageTime).TotalMinutes < 2)  //agar be hamun device vasl boodi
                                {
                                    lastMessageTime = DateTime.Now;         //tanzim kon akharin payaami daryafti key boode 
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }
                                else
                                    Console.ForegroundColor = ConsoleColor.Red;
                                break;
                        }
                        Console.WriteLine(ipStr + ": " + str);              //har payaami gerefti chaap kon ke az ki gerefti va chi gerefti
                    }
                }
                catch (ThreadAbortException)
                { }
            });
            t.Start();          //thread ro ejra kon
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = true;
            stopButton.Enabled = false;
            t.Abort();          //thread ro beband
            client.Close();     //udp ro ham beband
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (t != null)
                t.Abort();
        }
    }
}
