using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    public class MatchDay
    {
        public IReadOnlyCollection<MatchUp> Matches { get; }
        public MatchDayStatus Status { get; private set; }

        public MatchDay(params MatchUp[] matches)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            if (matches.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matches), 0, "Matches list is empty.");
            }

            Matches = new List<MatchUp>(matches);
            Status = MatchDayStatus.Pending;
        }

        public void Play()
        {
            foreach (var match in Matches.Where(m => !m.Played))
            {
                match.Play();
            }
            Status = MatchDayStatus.Complete;
        }
    }
}
