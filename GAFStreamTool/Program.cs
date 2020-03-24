﻿using GAFStreamTool.Data;
using GAFStreamTool.Encryption;
using GAFStreamTool.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GAFStreamTool
{
    static class Program
    {
        public static Network.Client Client;
        public static Config Config;
        public static ApiKey Key;

        public static GAFStreamTool MainForm;
        public static Settings SettingsForm;
        public static string MatchToTrack;

        private static Thread _settingsThread;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Config = new Config("94.130.68.58", "40015");
            Config.Load();
            Key = new ApiKey();
            Key.Read();

            if (args != null && args.Length > 0 && args[0].StartsWith("-install"))
            {
                RegistryEditor.Set("ColorTextR", Config.RGBText[0]);
                RegistryEditor.Set("ColorTextG", Config.RGBText[0]);
                RegistryEditor.Set("ColorTextB", Config.RGBText[0]);

                RegistryEditor.Set("ColorBackgroundR", Config.RGBBackground[0]);
                RegistryEditor.Set("ColorBackgroundG", Config.RGBBackground[1]);
                RegistryEditor.Set("ColorBackgroundB", Config.RGBBackground[2]);
                
                if (string.IsNullOrEmpty(Key.RegisterCode))
                {
                    MessageBox.Show("Cannot register with empty register code");
                    Environment.Exit(10);
                }
                else if (string.IsNullOrEmpty(Key.Key))
                {
                    MessageBox.Show("Cannot register with empty password");
                    Environment.Exit(10);
                }

                Client = new Network.Client(Config.Host, int.Parse(Config.Port));
                Client.ConnectAsync();

                while (Client.CurrentState == Network.Client.State.Connecting)
                    Task.Delay(5).Wait();

                if (Client.CurrentState == Network.Client.State.Failed)
                {
                    MessageBox.Show($"Failed to connect to: {Client.Host}:{Client.Port}");
                    Environment.Exit(2);
                }

                Client.StartReading();
                
                bool gotResponse = false;

                Packets.RegisterApiKeyPacket register = new Packets.RegisterApiKeyPacket(new Action<byte>((b) =>
                {
                    Packets.RegisterApiKeyPacket.OnRegisterResponse = null;

                    if (b == 1)
                    {
                        MessageBox.Show("You successfully registered at GAFApi");
                        Environment.Exit(0);
                    }
                    else
                    {
                        MessageBox.Show("Failed to register at GAFApi");
                        Environment.Exit(11);
                    }

                    gotResponse = true;
                }), Key.RegisterCode, Key.Key, AESEncryption.EncryptionKey);
                
                MessageBox.Show("Sending register, now you need to wait a short time for an response");

                PacketWriter writer = new PacketWriter(register);
                register.Send(writer, Client);

                while(!gotResponse)
                {
                    Task.Delay(1).Wait();
                }

                Environment.Exit(0);
            }

            MainForm = new GAFStreamTool();
            SettingsForm = new Settings();

            _settingsThread = new Thread(new ThreadStart(() =>
            {
                Application.Run(SettingsForm);
                Environment.Exit(0);
            }));
            _settingsThread.SetApartmentState(ApartmentState.STA);
            _settingsThread.Start();

            Application.Run(MainForm);

            Client.Dispose();
        }
    }
}
