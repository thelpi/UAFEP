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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="teams">Collection of teams.</param>
        /// <param name="groupCount">Expected number of groups.</param>
        /// <param name="oneLeg">Indicates if groups matches are one-leg on neutral ground.</param>
        /// <exception cref="ArgumentNullException"><paramref name="teams"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least one group is required.</exception>
        public GroupStage(IList<Team> teams, int groupCount, bool oneLeg)
        {
            if (teams == null)
            {
                throw new ArgumentNullException(nameof(teams));
            }

            if (groupCount < 1 || groupCount > teams.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(groupCount), groupCount, "At least one group is required.");
            }

            Groups = BuildRandomizedGroups(teams, groupCount, oneLeg);
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

        private static List<Group> BuildRandomizedGroups(IList<Team> teams, int groupCount, bool oneLeg)
        {
            var teamsPerGroup = teams.Count / groupCount;
            var teamsToDispatch = teams.Count % groupCount;

            var randomizedTeams = teams.OrderBy(t => Tools.Random.Next()).ToList();

            var groups = new List<Group>();
            for (int i = 0; i < groupCount; i++)
            {
                var currentTeamsPerGroup = teamsPerGroup;
                if (teamsToDispatch > 0)
                {
                    currentTeamsPerGroup += 1;
                    teamsToDispatch--;
                }
                groups.Add(new Group(randomizedTeams.Take(currentTeamsPerGroup), oneLeg));
                randomizedTeams.RemoveRange(0, currentTeamsPerGroup);
            }

            return groups;
        }
    }
}
