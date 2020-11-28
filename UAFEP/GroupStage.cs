using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    public class GroupStage
    {
        public IReadOnlyCollection<Group> Groups { get; }

        public GroupStage(IList<Team> teams, int groupCount)
        {
            if (teams == null)
            {
                throw new ArgumentNullException(nameof(teams));
            }

            if (groupCount < 1 || groupCount > teams.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(groupCount), groupCount, "At least one group is required");
            }

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
                groups.Add(new Group(randomizedTeams.Take(currentTeamsPerGroup)));
                randomizedTeams.RemoveRange(0, currentTeamsPerGroup);
            }

            Groups = groups;
        }

        public void Play(bool all)
        {
            foreach (var group in Groups)
            {
                group.Play(all);
            }
        }
    }
}
