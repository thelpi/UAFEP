using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    /// <summary>
    /// Represents a knockout stage.
    /// </summary>
    public class KnockoutStage
    {
        private int _nextTurnIndex = 0;
        private readonly List<Team> _teams;
        private readonly List<MatchDay> _matchDaysList;
        private readonly bool _oneLeg;
        private readonly bool _oneLegFinal;

        /// <summary>
        /// Gets the next non-completed <see cref="MatchDay"/>.
        /// </summary>
        public MatchDay NextMatchDayToPlay
        {
            get
            {
                return _matchDaysList[_nextTurnIndex].Status == MatchDayStatus.Complete ?
                    null : _matchDaysList[_nextTurnIndex];
            }
        }

        /// <summary>
        /// Gets every <see cref="MatchDay"/> (regardless of status).
        /// </summary>
        public IReadOnlyCollection<MatchDay> MatchDays
        {
            get
            {
                return _matchDaysList;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="teams">Collection of teams; sorted by the probablity of being exempted.</param>
        /// <param name="oneLeg">Is one-leg or not; doesn't apply for final.</param>
        /// <param name="oneLegFinal">Final is one-leg or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="teams"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="teams"/> contains <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="teams"/> contains duplicate.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least two teams are required.</exception>
        public KnockoutStage(IList<Team> teams, bool oneLeg, bool oneLegFinal)
        {
            if (teams == null)
            {
                throw new ArgumentNullException(nameof(teams));
            }

            if (teams.Contains(null))
            {
                throw new ArgumentException("Teams list contains null.", nameof(teams));
            }

            if (teams.Distinct().Count() != teams.Count)
            {
                throw new ArgumentException("Teams list contains duplicate.", nameof(teams));
            }

            if (teams.Count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(teams), teams.Count, "At least two teams are required.");
            }

            _oneLeg = oneLeg;
            _oneLegFinal = oneLegFinal;
            _teams = new List<Team>(teams);
            _matchDaysList = new List<MatchDay>
            {
                BuildFirstMatchDay(teams, oneLeg)
            };
            if (IsTwoLegContext(teams, oneLeg, oneLegFinal))
            {
                _matchDaysList.Add(_matchDaysList[0].Reverse());
            }
        }

        private static bool IsTwoLegContext(IList<Team> teams, bool oneLeg, bool oneLegFinal)
        {
            return (!oneLeg && teams.Count > 2) || (teams.Count == 2 && !oneLegFinal);
        }

        /// <summary>
        /// Plays the next round.
        /// </summary>
        /// <exception cref="InvalidOperationException">No more matches to play.</exception>
        public void Play()
        {
            if (NextMatchDayToPlay == null)
            {
                throw new InvalidOperationException("No more matches to play.");
            }

            _matchDaysList[_nextTurnIndex].Play();

            if (_nextTurnIndex == _matchDaysList.Count - 1)
            {
                var teams = GetNextRoundTeams();
                if (teams.Count > 1)
                {
                    var matchDay = BuildMatchDayForTeams(teams, _oneLeg);
                    _matchDaysList.Add(matchDay);
                    if (IsTwoLegContext(teams, _oneLeg, _oneLegFinal))
                    {
                        _matchDaysList.Add(matchDay.Reverse());
                    }
                    _nextTurnIndex++;
                }
            }
            else
            {
                _nextTurnIndex++;
            }
        }

        private IList<Team> GetNextRoundTeams()
        {
            var winners = _matchDaysList[_nextTurnIndex]
                            .Matches
                            .Select(m => m.GetQualified())
                            .ToList();
            var involved = _matchDaysList[_nextTurnIndex]
                            .Matches
                            .SelectMany(m => m.Teams)
                            .ToList();

            var teams = new List<Team>(winners);
            teams.AddRange(_teams.Where(t => _teams.IndexOf(t) < involved.Min(it => _teams.IndexOf(it))));
            return teams;
        }

        private static MatchDay BuildFirstMatchDay(IList<Team> teams, bool neutral)
        {
            int previousTeamsCount = 0;
            foreach (int teamsCount in GetExpectedTeamsCountByTurn())
            {
                if (teamsCount >= teams.Count)
                {
                    if (teamsCount > teams.Count)
                    {
                        teams = GetRemainingTeamsForPartialMatchDay(teams, previousTeamsCount);
                    }
                    return BuildMatchDayForTeams(teams, neutral);
                }
                previousTeamsCount = teamsCount;
            }
            throw new InvalidOperationException("This case should never occurs.");
        }

        private static IList<Team> GetRemainingTeamsForPartialMatchDay(IList<Team> teams, int previousTeamsCount)
        {
            var remainingTeams = (teams.Count - previousTeamsCount) * 2;

            return teams
                .Skip(teams.Count - remainingTeams)
                .Take(remainingTeams)
                .ToList();
        }

        private static MatchDay BuildMatchDayForTeams(IList<Team> teams, bool neutral)
        {
            var matches = teams
                .OrderBy(t => Tools.Random.Next())
                .Where(t => teams.IndexOf(t) % 2 == 0)
                .Select(t => Match.CreateOneOrSingleLeg(t, teams[teams.IndexOf(t) + 1], neutral))
                .ToArray();

            return new MatchDay(matches);
        }

        private static IEnumerable<int> GetExpectedTeamsCountByTurn()
        {
            const int max = 32; // max teams count allowed by integer
            int i = 1;
            while (i < max)
            {
                yield return (int)Math.Pow(2, i);
                i++;
            }
        }
    }
}
