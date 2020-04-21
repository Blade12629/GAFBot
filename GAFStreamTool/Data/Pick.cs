using GAFStreamTool.Network.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GAFStreamTool.Data
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
        public static bool PicksAutoUpdateEnabled { get; private set; }

        public static void AddPick(Pick p)
        {
            _picks.Add(p);
        }

        public static bool ContainsPickedBy(Pick p)
        {
            Pick pi = _picks.FirstOrDefault(pic => pic.PickedBy.Equals(p.PickedBy));

            if (pi == null)
                return false;
            else
                return true;
        }

        public static void RemovePicks(Pick p)
        {
            _picks.RemoveAll(pic => pic.PickedBy.Equals(p.PickedBy));
        }

        private static Timer _pickUpdateTimer;

        private static void SendPickUpdateRequest()
        {
            if (string.IsNullOrEmpty(Program.MatchToTrack))
                return;

            Packets.MatchPicksPacket matchPicks = new Packets.MatchPicksPacket(Program.MatchToTrack);
            PacketWriter writer = new PacketWriter(matchPicks);
            matchPicks.Send(writer, Program.Client);
        }

        public static void StartAutoPickUpdate()
        {
            if (PicksAutoUpdateEnabled)
                StopAutoPickUpdate();

            PicksAutoUpdateEnabled = true;

            _pickUpdateTimer = new Timer()
            {
                Interval = 5000,
                AutoReset = true
            };
            _pickUpdateTimer.Elapsed += (s, e) => SendPickUpdateRequest();
            _pickUpdateTimer.Start();
        }

        public static void StopAutoPickUpdate()
        {
            if (!PicksAutoUpdateEnabled)
                return;

            _pickUpdateTimer.Stop();
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
