using System;

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
        /// Constructor.
        /// </summary>
        /// <param name="firstTeam">First (home) team.</param>
        /// <param name="secondTeam">Second (away) team.</param>
        /// <param name="neutral">Optionnal; indicates a neutral ground.</param>
        /// <exception cref="ArgumentNullException"><paramref name="firstTeam"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="secondTeam"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">Second team is equals to first team.</exception>
        public Match(Team firstTeam, Team secondTeam, bool neutral = false) : this(firstTeam)
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
