using GAFStreamTool.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace GAFStreamTool
{
    public partial class GAFStreamTool : Form
    {
        private readonly List<PickComponent> _pickComponents;
        public IReadOnlyList<PickComponent> PickComponents => _pickComponents;
        public bool Closed;

        public GAFStreamTool()
        {
            InitializeComponent();
            _pickComponents = new List<PickComponent>()
            {
                new PickComponent(1, TB_Pick1PickedTeam, TB_Pick1PickedBy, TB_Pick1Desc, PB_Pick1Image, "null"),
                new PickComponent(2, TB_Pick2PickedTeam, TB_Pick2PickedBy, TB_Pick2Desc, PB_Pick2Image, "null"),
                new PickComponent(3, TB_Pick3PickedTeam, TB_Pick3PickedBy, TB_Pick3Desc, PB_Pick3Image, "null"),
            };


            if (!Directory.Exists("cachedImage\\"))
                Directory.CreateDirectory("cachedImage\\");

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.YellowGreen;
            TransparencyKey = Color.YellowGreen;

            TB_Pick1Desc.BackColor = Color.YellowGreen;
            TB_Pick2Desc.BackColor = Color.YellowGreen;
            TB_Pick3Desc.BackColor = Color.YellowGreen;

            TB_Pick1PickedBy.BackColor = Color.YellowGreen;
            TB_Pick2PickedBy.BackColor = Color.YellowGreen;
            TB_Pick3PickedBy.BackColor = Color.YellowGreen;

            TB_Pick1PickedTeam.BackColor = Color.YellowGreen;
            TB_Pick2PickedTeam.BackColor = Color.YellowGreen;
            TB_Pick3PickedTeam.BackColor = Color.YellowGreen;

            Task.Run(() =>
            {
                while (Program.SettingsForm == null)
                    Task.Delay(1).Wait();
                
                Program.SettingsForm.InitializeForeAndBackgroundColor();

            });
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.YellowGreen, e.ClipRectangle);
        }

        public Image CreateTestImage()
        {
            Bitmap bmp = new Bitmap(80, 80);
            int xMid = 40;
            int yMid = 40;

            int rot = 0;
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                using (Pen p = new Pen(Brushes.Red))
                {
                    int x = 5;
                    int y = 75;

                    while (rot < 4)
                    {
                        switch (rot)
                        {
                            case 0:
                                if (x == 80 - 5)
                                {
                                    rot++;
                                    goto case 1;
                                }

                                x += 5;
                                break;

                            case 1:
                                if (y == 5)
                                {
                                    rot++;
                                    goto case 2;
                                }

                                y -= 5;
                                break;

                            case 2:

                                if (x == 5)
                                {
                                    rot++;
                                    goto case 3;
                                }

                                x -= 5;
                                break;

                            case 3:
                                if (y == 80 - 5)
                                    return bmp;

                                y += 5;
                                break;
                        }

                        int invertedX = x * -1;
                        int invertedY = y * -1;

                        gfx.DrawLine(p, xMid + x, yMid + y, xMid - x, yMid - y);
                    }
                }
            }

            return bmp;
        }
        
        public PickComponent GetPick(int slot)
        {
            return _pickComponents[slot - 1];
        }
        
        public void UpdatePicks(params Pick[] picks)
        {
            List<PickComponent> components = _pickComponents.ToList();
            List<Pick> newPicks = picks.ToList();
            
            Action updateAc = new Action(() =>
            {
                //Update existing picks if they are still valid
                for (int i = 0; i < components.Count; i++)
                {
                    if (!components[i].IsPicked())
                        continue;

                    int index = newPicks.FindIndex(p => components[i].IsPickedBy(p));

                    if (index == -1)
                        continue;
                    
                    components[i].Update(newPicks[index]);
                    components.RemoveAt(i);
                    newPicks.RemoveAt(i);
                }

                //if there are components left, clear them and set them
                for (int i = 0; i < components.Count; i++)
                {
                    if (i < newPicks.Count)
                        components[i].Update(newPicks[i]);
                    else
                        components[i].Update(null);
                }
            });
            
            Invoke(updateAc);
        }

        private void GAFStreamTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Program.SettingsForm != null && !Program.SettingsForm.Closed)
                Program.SettingsForm.Close();

            Pick.StopAutoPickUpdate();

            if (Program.Client != null && 
                Program.Client.CurrentState != Network.Client.State.Failed &&
                Program.Client.CurrentState != Network.Client.State.Disconnected &&
                Program.Client.CurrentState != Network.Client.State.Disconnecting)
                Program.Client.Dispose();
        }

        private void GAFStreamTool_Load(object sender, EventArgs e)
        {

        }
    }
}
