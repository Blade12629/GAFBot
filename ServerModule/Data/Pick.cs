using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerModule.Data
{
    public class Pick : IEquatable<Pick>
    {
        private static List<Pick> _picks;
        public static List<Pick> Picks
        {
            get
            {
                if (_picks == null)
                    _picks = new List<Pick>();

                return _picks;
            }
        }

        public string PickedBy;
        public string Team;
        public string Match;
        public string Image;

        public Pick(string pickedBy, string team, string match, string image)
        {
            PickedBy = pickedBy;
            Team = team;
            Match = match;
            Image = image;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Pick);
        }

        public bool Equals(Pick other)
        {
            return other != null &&
                   PickedBy == other.PickedBy &&
                   Team == other.Team &&
                   Match == other.Match;
        }

        public override int GetHashCode()
        {
            var hashCode = 357435156;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PickedBy);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Team);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Match);
            return hashCode;
        }

        public static bool operator ==(Pick pick1, Pick pick2)
        {
            return EqualityComparer<Pick>.Default.Equals(pick1, pick2);
        }

        public static bool operator !=(Pick pick1, Pick pick2)
        {
            return !(pick1 == pick2);
        }
    }
}
