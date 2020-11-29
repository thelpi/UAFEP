using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UAFEP;

namespace UAFEPUnitTests
{
    [TestClass]
    public class GroupStageTests
    {
        [TestMethod]
        public void GroupStage_Ctor_Nominal()
        {
            const int groupCount = 9;

            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams"));

            var seed1 = teams.Take(groupCount).ToList();
            var seed2 = teams.Skip(groupCount).Take(groupCount * 2).ToList();
            var rest = teams.Skip(groupCount * 3).ToList();

            var gs = new GroupStage(groupCount, false, seed1, seed2, rest);

            Assert.AreEqual(groupCount, gs.Groups.Count);
            Assert.IsTrue(gs.Groups.All(g => g.Teams.Count(t => seed1.Contains(t)) == 1));
            Assert.IsTrue(gs.Groups.All(g => g.Teams.Count(t => seed2.Contains(t)) == 2));
            Assert.IsTrue(teams.All(t => gs.Groups.SelectMany(g => g.Teams).Contains(t)));
        }
    }
}
