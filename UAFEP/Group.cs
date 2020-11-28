using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    /// <summary>
    /// Represents a group.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Collection of <see cref="Team"/>.
        /// </summary>
        public IReadOnlyCollection<Team> Teams { get; }

        /// <summary>
        /// Collection of <see cref="MatchDay"/>.
        /// </summary>
        public IReadOnlyCollection<MatchDay> MatchDays { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="teams">Collection of <see cref="Team"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="teams"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least 3 teams required.</exception>
        public Group(IEnumerable<Team> teams)
        {
            if (teams == null)
            {
                throw new ArgumentNullException(nameof(teams));
            }

            if (teams.Count() < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(teams), teams.Count(), "At least 3 teams required.");
            }

            MatchDays = BuildMatchDays(teams);
            Teams = new List<Team>(teams);
        }

        /// <summary>
        /// Plays a single, or every, non-completed match day.
        /// </summary>
        /// <param name="all"><c>True</c> to play every match day; <c>False</c> to play a single one.</param>
        public void Play(bool all)
        {
            if (all)
            {
                foreach (var md in MatchDays.Where(md => md.Status != MatchDayStatus.Complete))
                {
                    md.Play();
                }
            }
            else
            {
                MatchDays.First(md => md.Status != MatchDayStatus.Complete).Play();
            }
        }

        /// <summary>
        /// Gets every <see cref="Match"/> of a specified team.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <param name="played">Optionnal; allows to filter on (non-)played matches only.</param>
        /// <param name="excludeExempt">Optionnal; excludes exempted matches.</param>
        /// <returns>Collection of <see cref="Match"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="team"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="team"/> is not from this group.</exception>
        public IReadOnlyCollection<Match> GetTeamMatches(Team team, bool? played = null, bool excludeExempt = false)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            if (!Teams.Contains(team))
            {
                throw new ArgumentException("Team is not from this group.", nameof(team));
            }

            return MatchDays
                .SelectMany(md =>
                    md.Matches.Where(m =>
                        m.IncludeTeam(team)
                        && (!played.HasValue || played.Value == m.Played)
                        && (!excludeExempt || !m.IsExempt)))
                .ToList();
        }

        public IReadOnlyDictionary<int, GroupRanking> GetRanking()
        {
            var rankings = Teams.Select(t => new GroupRanking(t, GetTeamMatches(t))).ToList();

            rankings = rankings
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalsDifference)
                .ThenByDescending(r => r.Goals)
                .ToList();

            // treat equality

            return rankings.ToDictionary(r => rankings.IndexOf(r) + 1, r => r);
        }

        private static List<MatchDay> BuildMatchDays(IEnumerable<Team> teams)
        {
            var teamsList = new List<Team>(teams);

            if (teamsList.Count == 3)
            {
                return BuildThreeTeamsMatchDays(teamsList);
            }
            else if (teamsList.Count == 4)
            {
                return BuildFourTeamsMatchDays(teamsList);
            }
            else
            {
                if (teamsList.Count % 2 == 1)
                {
                    teamsList.Add(null);
                }

                var orderedMatchDays = new List<MatchDay>
                {
                    BuildFirstMatchDay(teamsList)
                };

                var inversedExempt = false;
                for (var i = 1; i < teamsList.Count - 1; i++)
                {
                    orderedMatchDays.Add(BuildNextMatchDay(teamsList, orderedMatchDays[i - 1], ref inversedExempt));
                }

                orderedMatchDays.AddRange(BuildReversedMatchDays(orderedMatchDays));

                return BuildAlternatedMatchDays(orderedMatchDays);
            }
        }

        private static List<MatchDay> BuildThreeTeamsMatchDays(List<Team> teamsList)
        {
            var mds = new List<MatchDay>
            {
                new MatchDay(new Match[]
                {
                    new Match(teamsList[0], teamsList[1]),
                    new Match(teamsList[2])
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[1], teamsList[2]),
                    new Match(teamsList[0])
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[2], teamsList[0]),
                    new Match(teamsList[1])
                })
            };
            mds.Add(mds[0].Reverse());
            mds.Add(mds[1].Reverse());
            mds.Add(mds[2].Reverse());

            return mds;
        }

        private static List<MatchDay> BuildFourTeamsMatchDays(List<Team> teamsList)
        {
            var mds = new List<MatchDay>
            {
                new MatchDay(new Match[]
                {
                    new Match(teamsList[0], teamsList[1]),
                    new Match(teamsList[2], teamsList[3])
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[3], teamsList[0]),
                    new Match(teamsList[1], teamsList[2])
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[1], teamsList[3]),
                    new Match(teamsList[0], teamsList[2])
                })
            };
            mds.Add(mds[2].Reverse());
            mds.Add(mds[0].Reverse());
            mds.Add(mds[1].Reverse());

            return mds;
        }

        private static MatchDay BuildFirstMatchDay(List<Team> teams)
        {
            return new MatchDay(
                Enumerable.Range(0, teams.Count)
                    .Where(i => i % 2 == 0)
                    .Select(i => teams[i + 1] == null ?
                        new Match(teams[i]) :
                        new Match(teams[i], teams[i + 1]))
                    .ToArray());
        }

        private static MatchDay BuildNextMatchDay(List<Team> teams, MatchDay previousMatchDay, ref bool inversedExempt)
        {
            var oldTab = new Team[teams.Count / 2, 2];
            for (var j = 0; j < previousMatchDay.Matches.Count; j++)
            {
                var match = previousMatchDay.Matches.ElementAt(j);
                if (match.IsExempt)
                {
                    oldTab[j, 0] = inversedExempt ? match.AwayTeam : match.HomeTeam;
                    oldTab[j, 1] = inversedExempt ? match.HomeTeam : match.AwayTeam;
                }
                else
                {
                    oldTab[j, 0] = match.HomeTeam;
                    oldTab[j, 1] = match.AwayTeam;
                }
            }

            var newTab = new Team[teams.Count / 2, 2];
            for (var k = 0; k < oldTab.GetLength(0); k++)
            {
                for (var l = 0; l < oldTab.GetLength(1); l++)
                {
                    bool newIsHome = false;
                    if (k == 0 && l == 0)
                    {
                        newTab[0, 0] = oldTab[k, l];
                        newIsHome = true;
                    }
                    else if (l == 1 && k < oldTab.GetLength(0) - 1)
                    {
                        newTab[k + 1, 1] = oldTab[k, l];
                    }
                    else if (l == 1)
                    {
                        newTab[oldTab.GetLength(0) - 1, 0] = oldTab[k, l];
                        newIsHome = true;
                    }
                    else if (k > 1)
                    {
                        newTab[k - 1, 0] = oldTab[k, l];
                        newIsHome = true;
                    }
                    else
                    {
                        newTab[0, 1] = oldTab[k, l];
                    }
                    if (oldTab[k, l] == null)
                    {
                        inversedExempt = newIsHome;
                    }
                }
            }
            
            return new MatchDay(
                Enumerable.Range(0, oldTab.GetLength(0))
                    .Select(k => newTab[k, 0] == null
                        ? new Match(newTab[k, 1])
                        : (newTab[k, 1] == null
                            ? new Match(newTab[k, 0])
                            : new Match(newTab[k, 0], newTab[k, 1])
                        ))
                    .ToArray());
        }

        private static List<MatchDay> BuildReversedMatchDays(List<MatchDay> matchDays)
        {
            return matchDays
                .Select(md =>
                    new MatchDay(md.Matches
                        .Select(cm => cm.AwayTeam == null
                            ? new Match(cm.HomeTeam)
                            : new Match(cm.AwayTeam, cm.HomeTeam))
                        .ToArray()))
                .ToList();
        }

        private static List<MatchDay> BuildAlternatedMatchDays(List<MatchDay> orderedMatchDays)
        {
            var alternedMatchDays = new List<MatchDay>();
            var switcher = false;
            for (var i = 0; i < orderedMatchDays.Count; i++)
            {
                alternedMatchDays.Add(
                    switcher
                        ? orderedMatchDays[i].Reverse()
                        : orderedMatchDays[i]);

                if ((i + 1) != orderedMatchDays.Count / 2)
                {
                    switcher = !switcher;
                }
            }

            return alternedMatchDays;
        }
    }
}
