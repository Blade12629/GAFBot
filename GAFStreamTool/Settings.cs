using GAFStreamTool.Data;
using GAFStreamTool.Encryption;
using GAFStreamTool.Network.Packets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GAFStreamTool
{
    public partial class Settings : Form
    {
        public event EventHandler<ColorEventArgs> OnBackgroundColorChange;
        public event EventHandler<ColorEventArgs> OnTextColorChange;
        public event EventHandler<string> OnApiKeyChange;
        private Task _connectionTask;
        
        public static string ApiKey { get; private set; }

        public Settings()
        {
            InitializeComponent();

            int r = RegistryEditor.GetInt("ColorTextR");
            int g = RegistryEditor.GetInt("ColorTextG");
            int b = RegistryEditor.GetInt("ColorTextB");

            NUM_TextR.Value = r;
            NUM_TextG.Value = g;
            NUM_TextB.Value = b;

            PB_TextPreview.BackColor = Color.FromArgb(r, g, b);
            
            r = RegistryEditor.GetInt("ColorBackgroundR");
            g = RegistryEditor.GetInt("ColorBackgroundG");
            b = RegistryEditor.GetInt("ColorBackgroundB");

            NUM_BackgroundR.Value = r;
            NUM_BackgroundR.Value = r;
            NUM_BackgroundR.Value = g;

            PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);
        }

        public void SetBackgroundColor(int r, int g, int b)
        {
            OnBackgroundColorChange?.Invoke(this, new ColorEventArgs(r, g, b));

            RegistryEditor.Set("ColorBackgroundR", r);
            RegistryEditor.Set("ColorBackgroundG", g);
            RegistryEditor.Set("ColorBackgroundB", b);
            PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);
        }

        public void SetTextColor(int r, int g, int b)
        {
            OnTextColorChange?.Invoke(this, new ColorEventArgs(r, g, b));

            RegistryEditor.Set("ColorTextR", r);
            RegistryEditor.Set("ColorTextG", g);
            RegistryEditor.Set("ColorTextB", b);
            PB_TextPreview.BackColor = Color.FromArgb(r, g, b);
        }

        private void B_SetBackgroundColor_Click(object sender, EventArgs e)
        {
            int r = (int)NUM_BackgroundR.Value;
            int g = (int)NUM_BackgroundG.Value;
            int b = (int)NUM_BackgroundB.Value;

            SetBackgroundColor(r, g, b);
        }

        private void B_SetTextColor_Click(object sender, EventArgs e)
        {
            int r = (int)NUM_TextR.Value;
            int g = (int)NUM_TextG.Value;
            int b = (int)NUM_TextB.Value;

            SetTextColor(r, g, b);
        }

        private void B_TrackMatch_Click(object sender, EventArgs e)
        {
            Program.MatchToTrack = TB_TrackMatch.Text;
            Pick.Picks.Clear();

            Packets.MatchPicksPacket matches = new Packets.MatchPicksPacket(TB_TrackMatch.Text);
            PacketWriter writer = new PacketWriter(matches);
            matches.Send(writer, Program.Client);
        }

        private void B_RefreshPicks_Click(object sender, EventArgs e)
        {

        }
        
        private void B_Connect_Click(object sender, EventArgs e)
        {
            _connectionTask = Task.Run(() =>
            {
                if (Program.Client != null &&
                    (Program.Client.CurrentState != Network.Client.State.Disconnected ||
                    Program.Client.CurrentState != Network.Client.State.Failed))
                    Program.Client.Dispose();

                Program.Client = new Network.Client(Program.Config.Host, int.Parse(Program.Config.Port));
                Program.Client.ConnectAsync();

                Invoke(new Action(() =>
                {
                    PB_ConnectionState.BackColor = Color.Yellow;
                }));

                while (Program.Client.CurrentState == Network.Client.State.Connecting)
                    Task.Delay(5).Wait();

                if (Program.Client.CurrentState == Network.Client.State.Failed)
                {
                    MessageBox.Show($"Failed to connect to: {Program.Client.Host}:{Program.Client.Port}");
                }
                
                Invoke(new Action(() =>
                {
                    PB_ConnectionState.BackColor = Color.Blue;
                }));

                Program.Client.StartReading();

                Packets.AuthPacket auth = new Packets.AuthPacket(Program.Key.Key, new Action<bool>(b =>
                { 
                    Invoke(new Action(() =>
                    {
                        if (b)
                            PB_ConnectionState.BackColor = Color.Green;
                        else
                            PB_ConnectionState.BackColor = Color.Orange;
                    }));
                }));
                PacketWriter writer = new PacketWriter(auth);
                auth.Send(writer, Program.Client);

            });
        }
    }
}
