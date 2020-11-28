using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    public class Group
    {
        public IReadOnlyCollection<Team> Teams { get; }

        public IReadOnlyCollection<MatchDay> MatchDays { get; }

        public Group(IEnumerable<Team> teams)
        {
            if (teams == null)
            {
                throw new ArgumentNullException(nameof(teams));
            }

            if (teams.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(teams), teams.Count(), "Not enough teams.");
            }

            if (teams.Count() % 2 != 0)
            {
                throw new NotSupportedException("Teams count must be even.");
            }

            MatchDays = BuildMatchDays(teams);
            Teams = new List<Team>(teams);
        }

        public void Play(bool all)
        {
            if (all)
            {
                foreach (var md in MatchDays)
                {
                    md.Play();
                }
            }
            else
            {
                MatchDays.First().Play();
            }
        }

        public IReadOnlyCollection<MatchUp> GetTeamMatches(Team team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            if (!Teams.Contains(team))
            {
                throw new ArgumentException("Team is not from this group.", nameof(team));
            }

            return MatchDays.SelectMany(md => md.Matches.Where(m => m.IncludeTeam(team))).ToList();
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

            var orderedMatchDays = new List<MatchDay>
            {
                BuildFirstMatchDay(teamsList)
            };

            for (var i = 1; i < teamsList.Count - 1; i++)
            {
                orderedMatchDays.Add(BuildNextMatchDay(teamsList, orderedMatchDays[i - 1]));
            }
            
            orderedMatchDays.AddRange(BuildReversedMatchDays(orderedMatchDays));
            
            return BuildAlternatedMatchDays(orderedMatchDays);
        }

        private static MatchDay BuildFirstMatchDay(List<Team> teams)
        {
            return new MatchDay(
                Enumerable.Range(0, teams.Count)
                    .Where(i => i % 2 == 0)
                    .Select(i => new MatchUp(teams[i], teams[i + 1]))
                    .ToArray());
        }

        private static MatchDay BuildNextMatchDay(List<Team> teams, MatchDay previousMatchDay)
        {
            var halfCount = teams.Count % 2 == 0
                ? teams.Count / 2
                : teams.Count / 2 + 1;

            var oldTab = new Team[halfCount, 2];
            for (var j = 0; j < previousMatchDay.Matches.Count; j++)
            {
                oldTab[j, 0] = previousMatchDay.Matches.ElementAt(j).HomeTeam;
                oldTab[j, 1] = previousMatchDay.Matches.ElementAt(j).AwayTeam;
            }

            var newTab = new Team[halfCount, 2];
            for (var k = 0; k < oldTab.GetLength(0); k++)
            {
                for (var l = 0; l < oldTab.GetLength(1); l++)
                {
                    if (k == 0 && l == 0)
                    {
                        newTab[0, 0] = oldTab[k, l];
                    }
                    else if (l == 1 && k < oldTab.GetLength(0) - 1)
                    {
                        newTab[k + 1, 1] = oldTab[k, l];
                    }
                    else if (l == 1)
                    {
                        newTab[oldTab.GetLength(0) - 1, 0] = oldTab[k, l];
                    }
                    else if (k > 1)
                    {
                        newTab[k - 1, 0] = oldTab[k, l];
                    }
                    else
                    {
                        newTab[0, 1] = oldTab[k, l];
                    }
                }
            }

            return new MatchDay(
                Enumerable.Range(0, oldTab.GetLength(0))
                    .Select(k => new MatchUp(newTab[k, 0], newTab[k, 1]))
                    .ToArray());
        }

        private static List<MatchDay> BuildReversedMatchDays(List<MatchDay> matchDays)
        {
            return matchDays
                .Select(md =>
                    new MatchDay(md.Matches
                        .Select(cm => new MatchUp(cm.AwayTeam, cm.HomeTeam))
                        .ToArray()))
                .ToList();
        }

        private static List<MatchDay> BuildAlternatedMatchDays(List<MatchDay> orderedMatchDays)
        {
            var alternedMatchDays = new List<MatchDay>();
            var switcher = false;
            var cpt = 1;
            foreach (var day in orderedMatchDays)
            {
                if (switcher)
                {
                    var reverse = new List<MatchUp>();
                    foreach (var cm in day.Matches)
                        reverse.Add(new MatchUp(cm.AwayTeam, cm.HomeTeam));
                    alternedMatchDays.Add(new MatchDay(reverse.ToArray()));
                }
                else
                {
                    alternedMatchDays.Add(day);
                }

                if (cpt != orderedMatchDays.Count / 2)
                {
                    switcher = !switcher;
                }

                cpt++;
            }

            return alternedMatchDays;
        }
    }
}
