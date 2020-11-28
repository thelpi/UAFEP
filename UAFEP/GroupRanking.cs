using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    public class GroupRanking
    {
        private const int _pointsPerWin = 3;

        public Team Team { get; }
        public int Matches { get; }
        public int Points { get { return (_pointsPerWin * Wins) + Draws; } }
        public int Wins { get; }
        public int Draws { get; }
        public int Loses { get; }
        public int Goals { get; }
        public int GoalsAgainst { get; }
        public int GoalsDifference { get { return Goals - GoalsAgainst; } }

        public GroupRanking(Team team, IReadOnlyCollection<MatchUp> matches)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            if (matches.Any(m => m == null || !m.IncludeTeam(team)))
            {
                throw new ArgumentException("Invalid matches list.", nameof(matches));
            }

            Team = team;
            Matches = matches.Count;
            Wins = matches.Count(m => m.GetWinner() == team);
            Draws = matches.Count(m => m.GetWinner() == null);
            Loses = matches.Count(m => m.GetLoser() == team);
            Goals = matches.Sum(m => m.HomeTeam == team ? m.HomeScore : m.AwayScore);
            GoalsAgainst = matches.Sum(m => m.HomeTeam == team ? m.AwayScore : m.HomeScore);
        }
    }
}
