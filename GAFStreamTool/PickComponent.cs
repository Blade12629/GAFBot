using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GAFStreamTool
{
    public class PickComponent : IEquatable<PickComponent>
    {
        public TextBox TBTeam;
        public TextBox TBPickedBy;
        public Label LPickedByInfo;
        public PictureBox PBImage;
        public string PBImagePath;
        public int Id;

        public bool Visible
        {
            get
            {
                return TBTeam.Visible || TBPickedBy.Visible || LPickedByInfo.Visible || PBImage.Visible;
            }
        }

        public PickComponent(TextBox tBTeam, TextBox tBPickedBy, Label lPickedByInfo, PictureBox pBImage, string pbImagePath)
        {
            TBTeam = tBTeam;
            TBPickedBy = tBPickedBy;
            LPickedByInfo = lPickedByInfo;
            PBImage = pBImage;
            PBImagePath = pbImagePath;
        }

        public void Show(Form form = null)
        {
            SetVisibilityState(true, form);
        }

        public void Hide(Form form = null)
        {
            SetVisibilityState(false, form);
        }

        private void SetVisibilityState(bool state, Form form = null)
        {
            if (form != null && form.InvokeRequired)
            {
                form.Invoke(new Action(() =>
                {
                    TBTeam.Visible = state;
                    TBPickedBy.Visible = state;
                    LPickedByInfo.Visible = state;
                    PBImage.Visible = state;
                }));
                return;
            }

            TBTeam.Visible = state;
            TBPickedBy.Visible = state;
            LPickedByInfo.Visible = state;
            PBImage.Visible = state;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PickComponent);
        }

        public bool Equals(PickComponent other)
        {
            return other != null &&
                   EqualityComparer<string>.Default.Equals(TBTeam.Text, other.TBTeam.Text) &&
                   EqualityComparer<string>.Default.Equals(TBPickedBy.Text, other.TBPickedBy.Text);
        }

        public bool Equals(Data.Pick pick)
        {
            return pick != null &&
                   pick.Team.Equals(TBTeam.Text, StringComparison.CurrentCultureIgnoreCase) &&
                   pick.Team.Equals(TBPickedBy.Text, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsPickedBy(Data.Pick pick)
        {
            return pick != null &&
                   pick.PickedBy.Equals(TBPickedBy.Text, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            var hashCode = -1056656390;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TBTeam.Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TBPickedBy.Text);
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
