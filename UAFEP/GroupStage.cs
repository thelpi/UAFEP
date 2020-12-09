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
        private readonly bool _oneLeg;
        private readonly List<Team> _qualifiedTeams;
        private readonly GroupStageTieType _tieType;
        private KnockoutStage _tieKnockoutStage;
        private readonly int _qualifiedCount;

        /// <summary>
        /// Collection of <see cref="Group"/>.
        /// </summary>
        public IReadOnlyCollection<Group> Groups { get; }

        /// <summary>
        /// List of qualified teams.
        /// </summary>
        public IReadOnlyCollection<Team> QualifiedTeams { get { return _qualifiedTeams; } }

        /// <summary>
        /// Indicates if the group stage is complete.
        /// </summary>
        public bool IsComplete { get { return _qualifiedTeams.Count == _qualifiedCount; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="teamsBySeed">
        /// Collection of <see cref="Team"/> for each level of seed;
        /// every sub-collection except the last one must be a modulo of <paramref name="groupCount"/>.
        /// </param>
        /// <param name="qualifiedCount">Count of teams qualified for next round (overall on every group).</param>
        /// <param name="groupCount">Expected number of groups.</param>
        /// <param name="oneLeg">Indicates if groups matches are one-leg on neutral ground.</param>
        /// <param name="tieType"></param>
        /// <exception cref="ArgumentNullException"><paramref name="teamsBySeed"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="teamsBySeed"/> contains null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">At least one group is required.</exception>
        /// <exception cref="ArgumentException">One sub-list of teams contains null.</exception>
        /// <exception cref="ArgumentException">One sub-list is empty or not a modulo of <paramref name="groupCount"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Invalid <paramref name="qualifiedCount"/> value.</exception>
        public GroupStage(int groupCount, bool oneLeg, int qualifiedCount, GroupStageTieType tieType, params IList<Team>[] teamsBySeed)
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

            if (qualifiedCount < 1 || qualifiedCount > teamsBySeed.SelectMany(ts => ts).Count())
            {
                throw new ArgumentOutOfRangeException(nameof(qualifiedCount), qualifiedCount, "Invalid qualified count.");
            }

            _tieKnockoutStage = null;
            _oneLeg = oneLeg;
            _qualifiedTeams = new List<Team>();
            _tieType = tieType;
            _qualifiedCount = qualifiedCount;
            Groups = BuildRandomizedGroups(teamsBySeed, groupCount, oneLeg);
        }

        /// <summary>
        /// Plays a single <see cref="MatchDay"/> for every group.
        /// </summary>
        /// <exception cref="InvalidOperationException">The group stage is complete.</exception>
        public void Play()
        {
            if (_qualifiedTeams.Count == _qualifiedCount)
            {
                throw new InvalidOperationException("The group stage is complete.");
            }

            bool stillPlaying = false;
            foreach (var group in Groups)
            {
                if (group.NextMatchDay != null)
                {
                    group.Play();
                    stillPlaying = true;
                }
            }

            if (!stillPlaying)
            {
                if (_tieKnockoutStage != null)
                {
                    PlayKnockOut();
                }
                else
                {
                    var divideQualifiedByGroup = _qualifiedCount / Groups.Count;
                    for (int i = 0; i < divideQualifiedByGroup; i++)
                    {
                        _qualifiedTeams.AddRange(GetTeamsAtSpecifiedRanking(i + 1).Select(r => r.Team));
                    }

                    var restQualified = _qualifiedCount % Groups.Count;
                    if (restQualified > 0)
                    {
                        var restTeams = GetTeamsAtSpecifiedRanking(divideQualifiedByGroup);
                        var restTeamsSorted = GroupRanking.Sort(restTeams).Select(r => r.Team);
                        switch (_tieType)
                        {
                            case GroupStageTieType.KnockOut:
                                _tieKnockoutStage = new KnockoutStage(restTeamsSorted.ToList(), _oneLeg, _oneLeg, restQualified);
                                PlayKnockOut();
                                break;
                            case GroupStageTieType.Mixed:
                                _tieKnockoutStage = new KnockoutStage(restTeamsSorted.Take(restQualified).ToList(), _oneLeg, _oneLeg, restQualified);
                                PlayKnockOut();
                                break;
                            case GroupStageTieType.Ranking:
                                _qualifiedTeams.AddRange(restTeamsSorted.Take(restQualified));
                                break;
                        }
                    }
                }
            }
        }

        private void PlayKnockOut()
        {
            _tieKnockoutStage.Play();
            var qualifiedTeams = _tieKnockoutStage.GetQualifiedTeams();
            if (qualifiedTeams.Count == _qualifiedCount % Groups.Count)
            {
                _qualifiedTeams.AddRange(qualifiedTeams);
            }
        }

        private IEnumerable<GroupRanking> GetTeamsAtSpecifiedRanking(int r)
        {
            foreach (var group in Groups)
            {
                var ranking = group.GetRanking();
                yield return ranking[r];
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
