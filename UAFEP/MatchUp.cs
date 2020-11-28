using System;

namespace UAFEP
{
    public class MatchUp
    {
        private const double _homeAdvantageRate = 1.33;
        private const double _drawRate = 0.25;
        private const double _goalsAvg = 2.5;
        private const double _goalsStdDev = 1.7;

        public Team HomeTeam { get; }
        public Team AwayTeam { get; }
        public bool Played { get; private set; }
        public int HomeScore { get; private set; }
        public int AwayScore { get; private set; }

        public MatchUp(Team homeTeam, Team awayTeam)
        {
            HomeTeam = homeTeam ?? throw new ArgumentNullException(nameof(homeTeam));
            AwayTeam = awayTeam ?? throw new ArgumentNullException(nameof(awayTeam));

            if (homeTeam == awayTeam)
            {
                throw new ArgumentException("Away team is equals to home team.", nameof(awayTeam));
            }
        }

        public bool IncludeTeam(Team team)
        {
            return HomeTeam == team || AwayTeam == team;
        }

        public Team Winner()
        {
            if (!Played)
            {
                throw new InvalidOperationException("Not played yet.");
            }
            
            return HomeScore > AwayScore ? HomeTeam : (AwayScore > HomeScore ? AwayTeam : null);
        }

        public Team Loser()
        {
            var winner = Winner();
            return winner == HomeTeam ? AwayTeam : (winner == AwayTeam ? HomeTeam : null);
        }

        public void Play()
        {
            if (Played)
            {
                throw new InvalidOperationException("Already played.");
            }

            // use fake teams stats
            var result = ComputeMatchResult(false, _homeAdvantageRate, _drawRate, _goalsAvg, _goalsStdDev, 3, 3, 3, 3);

            HomeScore = result.Item1;
            AwayScore = result.Item2;

            Played = true;
        }

        public override string ToString()
        {
            return $"{HomeTeam} - {AwayTeam}";
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
