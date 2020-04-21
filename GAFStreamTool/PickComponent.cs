using GAFStreamTool.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GAFStreamTool
{
    public class PickComponent : IEquatable<PickComponent>
    {
        private TextBox _tbTeam;
        private TextBox _tbPickedBy;
        private TextBox _tbPickedByDesc;
        private PictureBox _pbImage;
        private string _pbImagePath;

        public int Slot { get; private set; }
        public Pick CurrentPick { get; private set; }
        public bool Visible { get; private set; }

        public PickComponent(int slot, TextBox tBTeam, TextBox tBPickedBy, TextBox tbPickedByDesc, PictureBox pBImage, string pbImagePath)
        {
            _tbTeam = tBTeam;
            _tbPickedBy = tBPickedBy;
            _tbPickedByDesc = tbPickedByDesc;
            _pbImage = pBImage;
            _pbImagePath = pbImagePath;
        }

        public void Show()
        {
            SetVisibility(true);
        }

        public void Hide()
        {
            SetVisibility(false);
        }
        
        private void SetVisibility(bool visible)
        {
            Form form = _tbTeam.FindForm();
            Action visAction = new Action(() =>
            {
                _tbTeam.Visible = visible;
                _tbPickedBy.Visible = visible;
                _tbPickedByDesc.Visible = visible;
                _pbImage.Visible = visible;

                if (visible)
                    Visible = true;
                else
                    Visible = false;
            });

            if (form != null && form.InvokeRequired)
            {
                form.Invoke(visAction);
                return;
            }

            visAction();
        }

        /// <summary>
        /// Updates the current pick
        /// </summary>
        /// <param name="pick">Pick used to update, leave null to reset</param>
        public void Update(Pick pick)
        {
            try
            {
                if (pick == null)
                {
                    CurrentPick = null;
                    _tbTeam.Text = "";
                    _tbPickedBy.Text = "";
                    _pbImagePath = "";
                    _pbImage.Image = null;

                    Hide();
                    return;
                }

                CurrentPick = pick;
                _tbTeam.Text = pick.Team;
                _tbPickedBy.Text = pick.PickedBy;
                _pbImage.Image = GetImage(pick.Image);
                _pbImagePath = pick.Image;

                Show();
            }
            catch (Exception ex)
            {
                Program.Client.Dispose();
                MessageBox.Show(ex.ToString());
            }
        }

        public void UpdateTextBackColor(int r, int g, int b)
        {
            return;
            Form f = _pbImage.FindForm();
            Action setTextBackColor = new Action(() =>
            {
                Color c = Color.FromArgb(r, g, b);

                _tbPickedByDesc.BackColor = c;
                _tbPickedBy.BackColor = c;
                _tbTeam.BackColor = c;
            });

            if (f.InvokeRequired)
            {
                f.Invoke(setTextBackColor);
                return;
            }

            setTextBackColor();
        }

        public void UpdateTextColor(int r, int g, int b)
        {
            Form f = _pbImage.FindForm();
            Action setTextColor = new Action(() =>
            {
                Color c = Color.FromArgb(r, g, b);

                _tbPickedByDesc.ForeColor = c;
                _tbPickedBy.ForeColor = c;
                _tbTeam.ForeColor = c;
            });
            
            if (f.InvokeRequired)
            {
                f.Invoke(setTextColor);
                return;
            }

            setTextColor();
        }

        private Image GetImage(string url)
        {
            string path = System.IO.Path.Combine("cachedImage\\", FixFilePath(_tbTeam.Text) + ".img");

            if (File.Exists(path))
                return LoadImage(path);

            using (System.Net.WebClient wc = new System.Net.WebClient())
                wc.DownloadFile(url, path);

            return LoadImage(path);
        }

        private Image LoadImage(string path)
        {
            using (FileStream fstream = File.OpenRead(path))
            {
                using (MemoryStream mstream = new MemoryStream())
                {
                    fstream.Position = 0;
                    fstream.CopyTo(mstream);

                    return Image.FromStream(mstream);
                }
            }
        }

        private char[] _forbiddenFileChars = new char[]
        {
            '\\',
            '/',
            ':',
            '*',
            '?',
            '"',
            '<',
            '>',
            '|'
        };

        private string FixFilePath(string path)
        {
            string result = "";

            for (int i = 0; i < path.Length; i++)
            {
                if (_forbiddenFileChars.Contains(path[i]))
                    result += "-";
                else
                    result += path[i];
            }

            return result;
        }

        public bool IsPickedBy(Pick pick)
        {
            return IsPickedBy(pick.PickedBy);
        }

        public bool IsPickedBy(string pickedBy, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            if (string.IsNullOrEmpty(_tbPickedBy.Text) || !_tbPickedBy.Text.Equals(pickedBy, comparison))
                return false;

            return true;
        }

        public bool IsPicked()
        {
            return !string.IsNullOrEmpty(_tbPickedBy.Text);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PickComponent);
        }

        public bool Equals(PickComponent other)
        {
            return other != null &&
                   EqualityComparer<string>.Default.Equals(_tbTeam.Text, other._tbTeam.Text) &&
                   EqualityComparer<string>.Default.Equals(_tbPickedBy.Text, other._tbPickedBy.Text);
        }
        
        public override int GetHashCode()
        {
            var hashCode = -1056656390;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_tbTeam.Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_tbPickedBy.Text);
            return hashCode;
        }

        public static bool operator ==(PickComponent component1, PickComponent component2)
        {
            return EqualityComparer<PickComponent>.Default.Equals(component1, component2);
        }

        public static bool operator !=(PickComponent component1, PickComponent component2)
        {
            return !(component1 == component2);
        }
    }
}
