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
        /// Home team; or team exempted.
        /// </summary>
        public Team HomeTeam { get; }
        /// <summary>
        /// Away team; <c>Null</c> if exemption.
        /// </summary>
        public Team AwayTeam { get; }
        /// <summary>
        /// Indicates if the match has been played.
        /// </summary>
        public bool Played { get; private set; }
        /// <summary>
        /// Goals for the home team; <c>0</c> if exemption or match not played yet.
        /// </summary>
        public int HomeScore { get; private set; }
        /// <summary>
        /// Goals for the away team; <c>0</c> if exemption or match not played yet.
        /// </summary>
        public int AwayScore { get; private set; }
        /// <summary>
        /// <c>True</c> if it's an exemption.
        /// </summary>
        public bool IsExempt { get { return AwayTeam == null; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="homeTeam">Home team.</param>
        /// <param name="awayTeam">Away team.</param>
        /// <exception cref="ArgumentNullException"><paramref name="homeTeam"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="awayTeam"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException">Away team is equals to home team.</exception>
        public Match(Team homeTeam, Team awayTeam) : this(homeTeam)
        {
            AwayTeam = awayTeam ?? throw new ArgumentNullException(nameof(awayTeam));

            if (homeTeam == awayTeam)
            {
                throw new ArgumentException("Away team is equals to home team.", nameof(awayTeam));
            }
        }

        /// <summary>
        /// Constructor for exemption.
        /// </summary>
        /// <param name="team">Team exempted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="team"/> is <c>Null</c>.</exception>
        public Match(Team team)
        {
            HomeTeam = team ?? throw new ArgumentNullException(nameof(team));
            AwayTeam = null;
        }

        /// <summary>
        /// Checks if a specified team is part of the match.
        /// </summary>
        /// <param name="team">The team to check.</param>
        /// <returns><c>True</c> if part of the match; <c>False</c> otherwise.</returns>
        public bool IncludeTeam(Team team)
        {
            return HomeTeam == team || AwayTeam == team;
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

            return HomeScore > AwayScore ? HomeTeam : (AwayScore > HomeScore ? AwayTeam : null);
        }

        /// <summary>
        /// Gets the loser team.
        /// </summary>
        /// <returns>Loser team. <c>Null</c> if draw or exempt.</returns>
        /// <exception cref="InvalidOperationException">Not played yet.</exception>
        public Team GetLoser()
        {
            var winner = GetWinner();
            return winner == HomeTeam ? AwayTeam : (winner == AwayTeam ? HomeTeam : null);
        }

        /// <summary>
        /// Plays the match; does nothing for exemption.
        /// </summary>
        /// <exception cref="InvalidOperationException">Already played.</exception>
        /// <remarks>Exception can still occur for exemption.</remarks>
        public void Play()
        {
            if (Played)
            {
                throw new InvalidOperationException("Already played.");
            }

            if (!IsExempt)
            {
                // use fake teams stats
                var result = ComputeMatchResult(false, _homeAdvantageRate, _drawRate, _goalsAvg, _goalsStdDev, 3, 3, 3, 3);

                HomeScore = result.Item1;
                AwayScore = result.Item2;
            }

            Played = true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsExempt)
            {
                return $"{HomeTeam} exempt.";
            }
            else if (Played)
            {
                return $"{HomeTeam} - {AwayTeam} ({HomeScore}-{AwayScore})";
            }
            else
            {
                return $"{HomeTeam} - {AwayTeam}";
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
