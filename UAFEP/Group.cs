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
        /// <param name="oneLeg">Indicates if matches are one-leg on neutral ground.</param>
        /// <exception cref="ArgumentNullException"><paramref name="teams"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least 3 teams required.</exception>
        /// <exception cref="ArgumentException">Teams list contains null.</exception>
        public Group(IEnumerable<Team> teams, bool oneLeg)
        {
            if (teams == null)
            {
                throw new ArgumentNullException(nameof(teams));
            }

            if (teams.Count() < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(teams), teams.Count(), "At least 3 teams required.");
            }

            if (teams.Contains(null))
            {
                throw new ArgumentException("Teams list contains null.", nameof(teams));
            }

            MatchDays = BuildMatchDays(teams, oneLeg);
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

        /// <summary>
        /// Generates the ranking of the group.
        /// </summary>
        /// <returns>A dictionnary where the key is the rank.</returns>
        public IReadOnlyDictionary<int, GroupRanking> GetRanking()
        {
            var rankings = Teams.Select(t => new GroupRanking(t, GetTeamMatches(t))).ToList();

            rankings = rankings
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalsDifference)
                .ThenByDescending(r => r.Goals)
                .ToList();

            return rankings.ToDictionary(r => rankings.IndexOf(r) + 1, r => r);
        }

        private static List<MatchDay> BuildMatchDays(IEnumerable<Team> teams, bool oneLeg)
        {
            var teamsList = new List<Team>(teams);

            if (teamsList.Count == 3)
            {
                return BuildThreeTeamsMatchDays(teamsList, oneLeg);
            }
            else if (teamsList.Count == 4)
            {
                return BuildFourTeamsMatchDays(teamsList, oneLeg);
            }
            else
            {
                if (teamsList.Count % 2 == 1)
                {
                    teamsList.Add(null);
                }

                var orderedMatchDays = new List<MatchDay>
                {
                    BuildFirstMatchDay(teamsList, oneLeg)
                };

                var exemptAsFirstTeam = false;
                for (var i = 1; i < teamsList.Count - 1; i++)
                {
                    orderedMatchDays.Add(BuildNextMatchDay(teamsList, orderedMatchDays[i - 1], ref exemptAsFirstTeam, oneLeg));
                }

                orderedMatchDays.AddRange(BuildReversedMatchDays(orderedMatchDays, oneLeg));

                return BuildAlternatedMatchDays(orderedMatchDays, oneLeg);
            }
        }

        private static List<MatchDay> BuildThreeTeamsMatchDays(List<Team> teamsList, bool oneLeg)
        {
            var mds = new List<MatchDay>
            {
                new MatchDay(new Match[]
                {
                    new Match(teamsList[0], teamsList[1], oneLeg),
                    new Match(teamsList[2])
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[1], teamsList[2], oneLeg),
                    new Match(teamsList[0])
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[2], teamsList[0], oneLeg),
                    new Match(teamsList[1])
                })
            };
            if (!oneLeg)
            {
                mds.Add(mds[0].Reverse());
                mds.Add(mds[1].Reverse());
                mds.Add(mds[2].Reverse());
            }

            return mds;
        }

        private static List<MatchDay> BuildFourTeamsMatchDays(List<Team> teamsList, bool oneLeg)
        {
            var mds = new List<MatchDay>
            {
                new MatchDay(new Match[]
                {
                    new Match(teamsList[0], teamsList[1], oneLeg),
                    new Match(teamsList[2], teamsList[3], oneLeg)
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[3], teamsList[0], oneLeg),
                    new Match(teamsList[1], teamsList[2], oneLeg)
                }),
                new MatchDay(new Match[]
                {
                    new Match(teamsList[1], teamsList[3], oneLeg),
                    new Match(teamsList[0], teamsList[2], oneLeg)
                })
            };
            if (!oneLeg)
            {
                mds.Add(mds[2].Reverse());
                mds.Add(mds[0].Reverse());
                mds.Add(mds[1].Reverse());
            }

            return mds;
        }

        private static MatchDay BuildFirstMatchDay(List<Team> teams, bool oneLeg)
        {
            return new MatchDay(
                Enumerable.Range(0, teams.Count)
                    .Where(i => i % 2 == 0)
                    .Select(i => teams[i + 1] == null ?
                        new Match(teams[i]) :
                        new Match(teams[i], teams[i + 1], oneLeg))
                    .ToArray());
        }

        private static MatchDay BuildNextMatchDay(List<Team> teams, MatchDay previousMatchDay, ref bool exemptAsFirstTeam, bool oneLeg)
        {
            var oldTab = new Team[teams.Count / 2, 2];
            for (var j = 0; j < previousMatchDay.Matches.Count; j++)
            {
                var match = previousMatchDay.Matches.ElementAt(j);
                if (match.IsExempt)
                {
                    oldTab[j, 0] = exemptAsFirstTeam ? match.Team2 : match.Team1;
                    oldTab[j, 1] = exemptAsFirstTeam ? match.Team1 : match.Team2;
                }
                else
                {
                    oldTab[j, 0] = match.Team1;
                    oldTab[j, 1] = match.Team2;
                }
            }

            var newTab = new Team[teams.Count / 2, 2];
            for (var k = 0; k < oldTab.GetLength(0); k++)
            {
                for (var l = 0; l < oldTab.GetLength(1); l++)
                {
                    bool newIsFirst = false;
                    if (k == 0 && l == 0)
                    {
                        newTab[0, 0] = oldTab[k, l];
                        newIsFirst = true;
                    }
                    else if (l == 1 && k < oldTab.GetLength(0) - 1)
                    {
                        newTab[k + 1, 1] = oldTab[k, l];
                    }
                    else if (l == 1)
                    {
                        newTab[oldTab.GetLength(0) - 1, 0] = oldTab[k, l];
                        newIsFirst = true;
                    }
                    else if (k > 1)
                    {
                        newTab[k - 1, 0] = oldTab[k, l];
                        newIsFirst = true;
                    }
                    else
                    {
                        newTab[0, 1] = oldTab[k, l];
                    }
                    if (oldTab[k, l] == null)
                    {
                        exemptAsFirstTeam = newIsFirst;
                    }
                }
            }
            
            return new MatchDay(
                Enumerable.Range(0, oldTab.GetLength(0))
                    .Select(k => newTab[k, 0] == null
                        ? new Match(newTab[k, 1])
                        : (newTab[k, 1] == null
                            ? new Match(newTab[k, 0])
                            : new Match(newTab[k, 0], newTab[k, 1], oneLeg)
                        ))
                    .ToArray());
        }

        private static List<MatchDay> BuildReversedMatchDays(List<MatchDay> matchDays, bool oneLeg)
        {
            return matchDays
                .Select(md =>
                    new MatchDay(md.Matches
                        .Select(cm => cm.Team2 == null
                            ? new Match(cm.Team1)
                            : new Match(cm.Team2, cm.Team1, oneLeg))
                        .ToArray()))
                .ToList();
        }

        private static List<MatchDay> BuildAlternatedMatchDays(List<MatchDay> orderedMatchDays, bool oneLeg)
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

            if (oneLeg)
            {
                return alternedMatchDays.Take(alternedMatchDays.Count / 2).ToList();
            }

            return alternedMatchDays;
        }
    }
}
