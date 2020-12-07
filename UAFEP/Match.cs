using System;
using System.Collections.Generic;
using System.Linq;

namespace UAFEP
{
    /// <summary>
    /// Represents a match.
    /// </summary>
    public class Match
    {
        private const double _homeAdvantageRate = 1.33;
        private const double _drawRate = 0.25;
        private const double _goalsAvg = 2.5;
        private const double _goalsStdDev = 1.7;

        /// <summary>
        /// Indicates if the match is on neutral ground.
        /// </summary>
        /// <remarks>The value is <c>False</c> for an exemption.</remarks>
        public bool Neutral { get; }
        /// <summary>
        /// First team; home team if applicable.
        /// </summary>
        public Team Team1 { get; }
        /// <summary>
        /// Second team; away team if applicable; <c>Null</c> for exemption.
        /// </summary>
        public Team Team2 { get; }
        /// <summary>
        /// Indicates if the match has been played.
        /// </summary>
        public bool Played { get; private set; }
        /// <summary>
        /// Goals for the first team; <c>0</c> if exemption or match not played yet.
        /// </summary>
        public int Score1 { get; private set; }
        /// <summary>
        /// Goals for the second team; <c>0</c> if exemption or match not played yet.
        /// </summary>
        public int Score2 { get; private set; }
        /// <summary>
        /// <c>True</c> if it's an exemption.
        /// </summary>
        public bool IsExempt { get { return Team2 == null; } }
        /// <summary>
        /// Collection of <see cref="Team"/>.
        /// </summary>
        public IReadOnlyCollection<Team> Teams
        {
            get
            {
                return IsExempt ? new List<Team> { Team1 }
                    : new List <Team> { Team1, Team2 };
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstTeam">First (home) team.</param>
        /// <param name="secondTeam">Second (away) team.</param>
        /// <param name="neutral">Indicates a neutral ground.</param>
        /// <exception cref="ArgumentNullException"><paramref name="firstTeam"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="secondTeam"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">Second team is equals to first team.</exception>
        public Match(Team firstTeam, Team secondTeam, bool neutral) : this(firstTeam)
        {
            Team2 = secondTeam ?? throw new ArgumentNullException(nameof(secondTeam));

            if (firstTeam == secondTeam)
            {
                throw new ArgumentException("Away team is equals to home team.", nameof(secondTeam));
            }

            Neutral = neutral;
        }

        /// <summary>
        /// Constructor for exemption.
        /// </summary>
        /// <param name="firstTeam">Team exempted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="firstTeam"/> is <c>Null</c>.</exception>
        public Match(Team firstTeam)
        {
            Team1 = firstTeam ?? throw new ArgumentNullException(nameof(firstTeam));
            Team2 = null;
        }

        /// <summary>
        /// Checks if a specified team is part of the match.
        /// </summary>
        /// <param name="team">The team to check.</param>
        /// <returns><c>True</c> if part of the match; <c>False</c> otherwise.</returns>
        public bool IncludeTeam(Team team)
        {
            return Team1 == team || Team2 == team;
        }

        /// <summary>
        /// Gets the winner team.
        /// </summary>
        /// <returns>Winner team. <c>Null</c> if draw or exempt.</returns>
        /// <exception cref="InvalidOperationException">Not played yet.</exception>
        public Team GetWinner()
        {
            if (!Played)
            {
                throw new InvalidOperationException("Not played yet.");
            }

            if (IsExempt)
            {
                return null;
            }

            return Score1 > Score2 ? Team1 : (Score2 > Score1 ? Team2 : null);
        }

        /// <summary>
        /// Gets the loser team.
        /// </summary>
        /// <returns>Loser team. <c>Null</c> if draw or exempt.</returns>
        /// <exception cref="InvalidOperationException">Not played yet.</exception>
        public Team GetLoser()
        {
            var winner = GetWinner();
            return winner == Team1 ? Team2 : (winner == Team2 ? Team1 : null);
        }

        /// <summary>
        /// Plays the match; does nothing for exemption.
        /// </summary>
        /// <exception cref="InvalidOperationException">Already played; this exception applies to exemption also.</exception>
        public void Play()
        {
            if (Played)
            {
                throw new InvalidOperationException("Already played.");
            }

            if (!IsExempt)
            {
                var result = ComputeMatchResult(Neutral, _homeAdvantageRate, _drawRate, _goalsAvg, _goalsStdDev, 3, 3, 3, 3);

                Score1 = result.Item1;
                Score2 = result.Item2;
            }

            Played = true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsExempt)
            {
                return $"{Team1} exempt.";
            }
            else if (Played)
            {
                return $"{Team1} - {Team2} ({Score1}-{Score2})";
            }
            else
            {
                return $"{Team1} - {Team2}";
            }
        }

        /// <summary>
        /// Computes and gets the team qualified for the next round, assuming a knock-out stage.
        /// </summary>
        /// <returns>The qualified team.</returns>
        /// <exception cref="ArgumentException">Match is not neutral; a first-leg match is expected.</exception>
        public Team GetQualified()
        {
            if (!Neutral)
            {
                throw new ArgumentException("Match is not neutral; a first-leg match is expected.");
            }

            return GetQualifiedInternal(null);
        }

        /// <summary>
        /// Computes and gets the team qualified for the next round, assuming a knock-out stage.
        /// </summary>
        /// <returns>The qualified team.</returns>
        /// <exception cref="ArgumentException">Match is neutral; a first-leg match is expected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="firstLeg"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">Same teams are expected on first-leg.</exception>
        /// <exception cref="ArgumentException">Reversed teams 1 and 2 expected for first-leg match..</exception>
        public Team GetQualified(Match firstLeg)
        {
            if (Neutral)
            {
                throw new ArgumentException("Match is neutral; no first-leg match is expected.", nameof(firstLeg));
            }

            if (firstLeg == null)
            {
                throw new ArgumentNullException(nameof(firstLeg));
            }

            if (!firstLeg.Teams.All(t => Teams.Contains(t)))
            {
                throw new ArgumentException("Same teams are expected on first-leg.", nameof(firstLeg));
            }

            if (!IsExempt && (Team1 != firstLeg.Team2))
            {
                throw new ArgumentException("Reversed teams 1 and 2 expected for first-leg match.", nameof(firstLeg));
            }

            return GetQualifiedInternal(firstLeg);
        }

        private Team GetQualifiedInternal(Match firstLeg = null)
        {
            if (IsExempt)
            {
                return Team1;
            }

            if (firstLeg == null)
            {
                return GetWinner() ?? GetPenaltyShootoutWinner();
            }

            var firstLegWinner = firstLeg.GetWinner();
            var secondLegWinner = GetWinner();

            if (firstLegWinner != null && (secondLegWinner == null || secondLegWinner == firstLegWinner))
            {
                return firstLegWinner;
            }
            else if (secondLegWinner != null && firstLegWinner == null)
            {
                return secondLegWinner;
            }
            else
            {
                int firstTeamGoals = firstLeg.Score1 + Score2;
                int secondteamsGoals = firstLeg.Score2 + Score1;
                if (firstTeamGoals > secondteamsGoals)
                {
                    return Team2;
                }
                else if (firstTeamGoals < secondteamsGoals)
                {
                    return Team1;
                }
                else if (firstLeg.Score2 > Score2)
                {
                    return Team1;
                }
                else if (firstLeg.Score2 < Score2)
                {
                    return Team2;
                }
                else
                {
                    return GetPenaltyShootoutWinner();
                }
            }
        }

        private Team GetPenaltyShootoutWinner()
        {
            return Tools.Random.Next(0, 2) == 0 ? Team1 : Team2;
        }

        private static (int, int) ComputeMatchResult(
            bool isNeutral, double homeAdvantageRate,
            double drawRate, double goalsAvg, double goalsStdDev,
            int homeTeamOffLbl, int homeTeamDefLvl,
            int awayTeamOffLbl, int awayTeamDefLvl)
        {
            if (isNeutral)
            {
                homeAdvantageRate = 1;
            }

            var homeLevel = homeTeamOffLbl + homeTeamDefLvl;
            var awayLevel = awayTeamOffLbl + awayTeamDefLvl;

            var totalLevel = homeLevel * homeAdvantageRate + awayLevel;

            var nStart = homeLevel * homeAdvantageRate * (1 - drawRate);
            var nEnd = (homeLevel * homeAdvantageRate) + (awayLevel * drawRate);
            var resultX = Tools.Random.NextDouble() * (homeLevel * homeAdvantageRate + awayLevel);

            var goalAvgCurrent = goalsAvg * (1 + (((homeTeamOffLbl - awayTeamDefLvl) + (awayTeamOffLbl - homeTeamDefLvl)) / (double)100));
            var r = Tools.GetRdmFromNormal(goalAvgCurrent, goalsStdDev);
            while (r < 0
                || (Math.Truncate(r) % 2 > 0 && (!(resultX < nStart) && !(resultX >= nEnd)))
                || (Math.Truncate(r) == 0 && ((resultX < nStart) || (resultX >= nEnd))))
            {
                r = Tools.GetRdmFromNormal(goalAvgCurrent, goalsStdDev);
            }

            var rI = (int)Math.Truncate(r);

            if (resultX < nStart)
            {
                var slices = (rI / 2) + (rI % 2 > 0 ? 1 : 0);
                var r2 = nStart / slices;
                var start = 0.0;
                var b1 = rI + 1;
                var b2 = -1;
                while (start < resultX)
                {
                    b1 -= 1;
                    b2 += 1;
                    start += r2;
                }
                
                return (b1, b2);
            }
            else if (resultX >= nEnd)
            {
                var slices = (rI / 2) + (rI % 2 > 0 ? 1 : 0);
                var r2 = (totalLevel - nEnd) / slices;
                var start = totalLevel;
                var b1 = -1;
                var b2 = rI + 1;
                while (start > resultX)
                {
                    b1 += 1;
                    b2 -= 1;
                    start -= r2;
                }
                
                return (b1, b2);
            }
            else
            {
                return (rI / 2, rI / 2);
            }
        }
    }
}
