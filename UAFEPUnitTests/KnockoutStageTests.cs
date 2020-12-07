using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UAFEP;

namespace UAFEPUnitTests
{
    [TestClass]
    public class KnockoutStageTests
    {
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void KnockoutStage_Ctor_Nominal(bool oneLeg)
        {
            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams"));

            var kos = new KnockoutStage(teams, oneLeg, oneLeg);

            var matchDays = kos.NextMatchDayToPlay;

            Assert.AreEqual(23, matchDays.Matches.Count);

            var actualTeams = matchDays.Matches.SelectMany(m => m.Teams);
            Assert.IsTrue(actualTeams.All(t => teams.Skip(9).ToList().Contains(t)));

            Assert.AreEqual(oneLeg ? 1 : 2, kos.MatchDays.Count);
        }

        [TestMethod]
        public void KnockoutStage_Play_Nominal()
        {
            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams"));

            var kos = new KnockoutStage(teams, false, true);

            Assert.IsTrue(teams.Skip(9).ToList().All(t => kos.NextMatchDayToPlay.Matches.SelectMany(m => m.Teams).Contains(t)));

            kos.Play();
            kos.Play();

            Assert.AreEqual(16, kos.NextMatchDayToPlay.Matches.Count);
            Assert.IsTrue(teams.Take(9).ToList().All(t => kos.NextMatchDayToPlay.Matches.SelectMany(m => m.Teams).Contains(t)));

            kos.Play();
            kos.Play();

            Assert.AreEqual(8, kos.NextMatchDayToPlay.Matches.Count);
            kos.Play();
            kos.Play();

            Assert.AreEqual(4, kos.NextMatchDayToPlay.Matches.Count);
            kos.Play();
            kos.Play();

            Assert.AreEqual(2, kos.NextMatchDayToPlay.Matches.Count);
            kos.Play();
            kos.Play();

            Assert.AreEqual(1, kos.NextMatchDayToPlay.Matches.Count);
            kos.Play();

            Assert.IsNull(kos.NextMatchDayToPlay);
        }
    }
}
