﻿using GAFStreamTool.Data;
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
        }

        public void InitializeForeAndBackgroundColor()
        {
            int r = RegistryEditor.GetInt("ColorTextR");
            int g = RegistryEditor.GetInt("ColorTextG");
            int b = RegistryEditor.GetInt("ColorTextB");

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    NUM_TextR.Value = r;
                    NUM_TextG.Value = g;
                    NUM_TextB.Value = b;

                    PB_TextPreview.BackColor = Color.FromArgb(r, g, b);
                }));
            }
            else
            {
                NUM_TextR.Value = r;
                NUM_TextG.Value = g;
                NUM_TextB.Value = b;

                PB_TextPreview.BackColor = Color.FromArgb(r, g, b);
            }

            SetTextColor(r, g, b);

            r = RegistryEditor.GetInt("ColorBackgroundR");
            g = RegistryEditor.GetInt("ColorBackgroundG");
            b = RegistryEditor.GetInt("ColorBackgroundB");

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    NUM_BackgroundR.Value = r;
                    NUM_BackgroundR.Value = r;
                    NUM_BackgroundR.Value = g;

                    PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);
                }));
            }
            else
            {
                NUM_BackgroundR.Value = r;
                NUM_BackgroundR.Value = r;
                NUM_BackgroundR.Value = g;

                PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);
            }
            
            SetBackgroundColor(r, g, b);

        }

        public void SetBackgroundColor(int r, int g, int b)
        {
            OnBackgroundColorChange?.Invoke(this, new ColorEventArgs(r, g, b));

            RegistryEditor.Set("ColorBackgroundR", r);
            RegistryEditor.Set("ColorBackgroundG", g);
            RegistryEditor.Set("ColorBackgroundB", b);
            PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);

            if (Program.MainForm.InvokeRequired)
            {
                Program.MainForm.Invoke(new Action(() =>
                {
                    Program.MainForm.BackColor = Color.FromArgb(r, g, b);
                }));
            }
            else
                Program.MainForm.BackColor = Color.FromArgb(r, g, b);
        }

        public void SetTextColor(int r, int g, int b)
        {
            OnTextColorChange?.Invoke(this, new ColorEventArgs(r, g, b));

            RegistryEditor.Set("ColorTextR", r);
            RegistryEditor.Set("ColorTextG", g);
            RegistryEditor.Set("ColorTextB", b);
            PB_TextPreview.BackColor = Color.FromArgb(r, g, b);

            if (Program.MainForm.InvokeRequired)
            {
                Program.MainForm.Invoke(new Action(() =>
                {
                    Color c = Color.FromArgb(r, g, b);

                    foreach(var pc in Program.MainForm.PickComponents)
                    {
                        pc.LPickedByInfo.ForeColor = c;
                        pc.TBPickedBy.ForeColor = c;
                        pc.TBTeam.ForeColor = c;
                    }
                }));
            }
            else
            {
                Color c = Color.FromArgb(r, g, b);

                foreach (var pc in Program.MainForm.PickComponents)
                {
                    pc.LPickedByInfo.ForeColor = c;
                    pc.TBPickedBy.ForeColor = c;
                    pc.TBTeam.ForeColor = c;
                }
            }
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
            Pick.StopAutoPickUpdate();
            Program.MatchToTrack = TB_TrackMatch.Text;
            Pick.Picks.Clear();

            B_RefreshPicks_Click(sender, e);

            Pick.StartAutoPickUpdate();
        }

        private void B_RefreshPicks_Click(object sender, EventArgs e)
        {
            Packets.MatchPicksPacket matches = new Packets.MatchPicksPacket(Program.MatchToTrack);
            PacketWriter writer = new PacketWriter(matches);
            matches.Send(writer, Program.Client);
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
                        {
                            PB_ConnectionState.BackColor = Color.Green;
                            Program.Client.Authenticated = true;
                        }
                        else
                        {
                            PB_ConnectionState.BackColor = Color.Orange;
                            MessageBox.Show("Failed to authenticate");
                        }
                    }));
                }));

                PacketWriter writer = new PacketWriter(auth);
                auth.Send(writer, Program.Client);
            });
        }

        private void B_TestPing_Click(object sender, EventArgs e)
        {
            Packets.PingPacket ping = new Packets.PingPacket(false, new Action<string>(s =>
            {
                MessageBox.Show("Recieved response for test ping: " + s);
            }));
            PacketWriter writer = new PacketWriter(ping);
            ping.Send(writer, Program.Client);
        }

        private void SetPreviewColorBackground(int r = -1, int g = -1, int b = -1)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (r == -1)
                        r = (int)NUM_BackgroundR.Value;
                    if (g == -1)
                        g = (int)NUM_BackgroundG.Value;
                    if (b == -1)
                        b = (int)NUM_BackgroundB.Value;

                    PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);
                }));
                return;
            }

            if (r == -1)
                r = (int)NUM_BackgroundR.Value;
            if (g == -1)
                g = (int)NUM_BackgroundG.Value;
            if (b == -1)
                b = (int)NUM_BackgroundB.Value;

            PB_BackgroundPreview.BackColor = Color.FromArgb(r, g, b);
        }

        private void SetPreviewColorText(int r = -1, int g = -1, int b = -1)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (r == -1)
                        r = (int)NUM_TextR.Value;
                    if (g == -1)
                        g = (int)NUM_TextG.Value;
                    if (b == -1)
                        b = (int)NUM_TextB.Value;

                    PB_TextPreview.BackColor = Color.FromArgb(r, g, b);
                }));
                return;
            }

            if (r == -1)
                r = (int)NUM_TextR.Value;
            if (g == -1)
                g = (int)NUM_TextG.Value;
            if (b == -1)
                b = (int)NUM_TextB.Value;

            PB_TextPreview.BackColor = Color.FromArgb(r, g, b);
        }

        private void NUM_TextR_ValueChanged(object sender, EventArgs e)
        {
            SetPreviewColorText();
        }

        private void NUM_TextG_ValueChanged(object sender, EventArgs e)
        {
            SetPreviewColorText();
        }

        private void NUM_TextB_ValueChanged(object sender, EventArgs e)
        {
            SetPreviewColorText();
        }

        private void NUM_BackgroundR_ValueChanged(object sender, EventArgs e)
        {
            SetPreviewColorBackground();
        }

        private void NUM_BackgroundG_ValueChanged(object sender, EventArgs e)
        {
            SetPreviewColorBackground();
        }

        private void NUM_BackgroundB_ValueChanged(object sender, EventArgs e)
        {
            SetPreviewColorBackground();
        }
    }
}
