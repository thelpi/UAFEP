using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    /// <summary>
    /// Represents the ranking informations for a team inside a group.
    /// </summary>
    public class GroupRanking
    {
        private const int _pointsPerWin = 3;

        /// <summary>
        /// Team.
        /// </summary>
        public Team Team { get; }
        /// <summary>
        /// Matches count.
        /// </summary>
        public int Matches { get; }
        /// <summary>
        /// Points.
        /// </summary>
        public int Points { get { return (_pointsPerWin * Wins) + Draws; } }
        /// <summary>
        /// Wins count.
        /// </summary>
        public int Wins { get; }
        /// <summary>
        /// Draws count.
        /// </summary>
        public int Draws { get; }
        /// <summary>
        /// Loses count.
        /// </summary>
        public int Loses { get; }
        /// <summary>
        /// Goals.
        /// </summary>
        public int Goals { get; }
        /// <summary>
        /// Goals against.
        /// </summary>
        public int GoalsAgainst { get; }
        /// <summary>
        /// Goal difference.
        /// </summary>
        public int GoalDifference { get { return Goals - GoalsAgainst; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <param name="matches">Matches played by the team.</param>
        /// <exception cref="ArgumentNullException"><paramref name="team"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="matches"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">Invalid matches list.</exception>
        public GroupRanking(Team team, IReadOnlyCollection<Match> matches)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            if (matches.Any(m => m == null || !m.IncludeTeam(team) || m.IsExempt || !m.Played))
            {
                throw new ArgumentException("Invalid matches list.", nameof(matches));
            }

            Team = team;
            Matches = matches.Count;
            Wins = matches.Count(m => m.GetWinner() == team);
            Draws = matches.Count(m => m.GetWinner() == null);
            Loses = matches.Count(m => m.GetLoser() == team);
            Goals = matches.Sum(m => m.Team1 == team ? m.Score1 : m.Score2);
            GoalsAgainst = matches.Sum(m => m.Team1 == team ? m.Score2 : m.Score1);
        }
    }
}
