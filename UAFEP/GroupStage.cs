using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    /// <summary>
    /// Represents a group stage.
    /// </summary>
    public class GroupStage
    {
        /// <summary>
        /// Collection of <see cref="Group"/>.
        /// </summary>
        public IReadOnlyCollection<Group> Groups { get; }

        public bool All()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="teamsBySeed">
        /// Collection of <see cref="Team"/> for each level of seed;
        /// every sub-collection except the last one must be a modulo of <paramref name="groupCount"/>.
        /// </param>
        /// <param name="groupCount">Expected number of groups.</param>
        /// <param name="oneLeg">Indicates if groups matches are one-leg on neutral ground.</param>
        /// <exception cref="ArgumentNullException"><paramref name="teamsBySeed"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="teamsBySeed"/> contains null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least one group is required.</exception>
        /// <exception cref="ArgumentException">One sub-list of teams contains null.</exception>
        /// <exception cref="ArgumentException">One sub-list is empty or not a modulo of <paramref name="groupCount"/>.</exception>
        public GroupStage(int groupCount, bool oneLeg, params IList<Team>[] teamsBySeed)
        {
            if (teamsBySeed == null)
            {
                throw new ArgumentNullException(nameof(teamsBySeed));
            }

            if (teamsBySeed.Contains(null))
            {
                throw new ArgumentException("Teams by seed contains null.", nameof(teamsBySeed));
            }

            if (teamsBySeed.Any(ts => ts.Contains(null)))
            {
                throw new ArgumentException("One sub-list of teams contains null.", nameof(teamsBySeed));
            }

            if (teamsBySeed.Any(ts => !ts.Any() || (ts != teamsBySeed.Last() && ts.Count() % groupCount != 0)))
            {
                throw new ArgumentException($"One sub-list is empty or not a modulo of {groupCount}.", nameof(teamsBySeed));
            }

            if (groupCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(groupCount), groupCount, "At least one group is required.");
            }

            Groups = BuildRandomizedGroups(teamsBySeed, groupCount, oneLeg);
        }

        /// <summary>
        /// Plays a single (or all) <see cref="MatchDay"/> for every group.
        /// </summary>
        /// <param name="all"><c>True</c> to play every <see cref="MatchDay"/>; <c>False</c> to play a single <see cref="MatchDay"/>.</param>
        public void Play(bool all)
        {
            foreach (var group in Groups)
            {
                group.Play(all);
            }
        }

        private static List<Group> BuildRandomizedGroups(IList<Team>[] teamsBySeed, int groupCount, bool oneLeg)
        {
            var groupsTeams = Enumerable.Range(0, groupCount).Select(i => new List<Team>()).ToList();

            foreach (var teams in teamsBySeed)
            {
                var teamsPerGroup = teams.Count / groupCount;
                var teamsToDispatch = teams.Count % groupCount;
                var randomizedTeams = teams.OrderBy(t => Tools.Random.Next()).ToList();

                for (int i = 0; i < groupCount; i++)
                {
                    var currentTeamsPerGroup = teamsPerGroup;
                    if (teamsToDispatch > 0)
                    {
                        currentTeamsPerGroup += 1;
                        teamsToDispatch--;
                    }
                    groupsTeams[i].AddRange(randomizedTeams.Take(currentTeamsPerGroup).ToList());
                    randomizedTeams.RemoveRange(0, currentTeamsPerGroup);
                }
            }

            return groupsTeams.Select(gt => new Group(gt, oneLeg)).ToList();
        }
    }
}
