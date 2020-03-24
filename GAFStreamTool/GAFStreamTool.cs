using GAFStreamTool.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace GAFStreamTool
{
    public partial class GAFStreamTool : Form
    {
        private readonly List<PickComponent> _pickComponents;

        public GAFStreamTool()
        {
            InitializeComponent();
            _pickComponents = new List<PickComponent>()
            {
                new PickComponent(TB_Pick1PickedTeam, TB_Pick1PickedBy, L_Pick1PickedByDesc, PB_Pick1Image, "null"),
                new PickComponent(TB_Pick2PickedTeam, TB_Pick2PickedBy, L_Pick1PickedByDesc, PB_Pick2Image, "null"),
                new PickComponent(TB_Pick3PickedTeam, TB_Pick3PickedBy, L_Pick1PickedByDesc, PB_Pick3Image, "null"),
            };
            L_Loading.Visible = false;
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

        public void HideNotification()
        {
            SetNotificationVisibility(false);
        }

        public void ShowNotification()
        {
            SetNotificationVisibility(true);
        }

        private void SetNotificationVisibility(bool state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => L_Loading.Visible = state));
                return;
            }

            L_Loading.Visible = state;
        }

        public PickComponent GetPick(int slot)
        {
            return _pickComponents[slot - 1];
        }

        /// <summary>
        /// Sets and shows a pick or resets and hides it if all values are null
        /// </summary>
        /// <param name="slot">slot</param>
        /// <param name="pickedTeam">team name</param>
        /// <param name="pickedBy">commentator name</param>
        /// <param name="img">team image</param>
        public void SetPick(int slot, string pickedTeam = null, string pickedBy = null, Image img = null)
        {
            PickComponent comp = GetPick(slot);

            if (pickedTeam == null && pickedBy == null && img == null)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        comp.TBTeam.Visible = false;
                        comp.TBPickedBy.Visible = false;
                        comp.PBImage.Visible = false;

                        comp.TBTeam.Text = null;
                        comp.TBPickedBy.Text = null;
                        comp.PBImage.Image = null;
                    }));
                    return;
                }

                comp.TBTeam.Visible = false;
                comp.TBPickedBy.Visible = false;
                comp.PBImage.Visible = false;

                comp.TBTeam.Text = null;
                comp.TBPickedBy.Text = null;
                comp.PBImage.Image = null;
            }

            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    if (pickedTeam != null)
                        comp.TBTeam.Text = pickedTeam;

                    if (pickedBy != null)
                        comp.TBPickedBy.Text = pickedBy;

                    if (img != null)
                        comp.PBImage.Image = img;

                    comp.TBTeam.Visible = true;
                    comp.TBPickedBy.Visible = true;
                    comp.PBImage.Visible = true;
                }));
                return;
            }

            if (pickedTeam != null)
                comp.TBTeam.Text = pickedTeam;

            if (pickedBy != null)
                comp.TBPickedBy.Text = pickedBy;

            if (img != null)
                comp.PBImage.Image = img;

            comp.TBTeam.Visible = true;
            comp.TBPickedBy.Visible = true;
            comp.PBImage.Visible = true;
        }

        public void CheckForUpdatedPics(params Pick[] picks)
        {
            int[] slotsUsed = new int[3]
            {
                0,
                0,
                0
            };

            foreach(Pick p in picks)
            {
                PickComponent comp = _pickComponents.FirstOrDefault(pc => pc.IsPickedBy(p));

                if (comp == null)
                    continue;

                slotsUsed[comp.Id - 1] = 1;

                if (!comp.PBImagePath.Equals(p.Image) ||
                    !comp.TBPickedBy.Text.Equals(p.PickedBy) ||
                    !comp.TBTeam.Text.Equals(p.Team))
                    UpdatePick(comp.Id, p);
            }

            for (int i = 0; i < slotsUsed.Length; i++)
                if (slotsUsed[i] == 0)
                    SetPick(i + 1);
        }

        public void UpdatePick(int slot, Pick pick)
        {
            PickComponent comp = _pickComponents[slot - 1];

            Image img = null;
            if (!comp.PBImagePath.Equals(pick.Image))
            {
                string path = Path.Combine("cachedImage\\", pick.Team + ".image");

                if (!File.Exists(path))
                    using (WebClient wc = new WebClient())
                        wc.DownloadFile(pick.Image, path);

                img = Image.FromFile(path);
            }

            SetPick(slot, pick.Team, pick.PickedBy, img);
        }
    }
}
