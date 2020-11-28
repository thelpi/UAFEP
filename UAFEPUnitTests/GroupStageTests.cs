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
            var teams = JsonConvert.DeserializeObject<List<Team>>(TestTools.GetFileContent("teams"));

            var gs = new GroupStage(teams, 9, false);

            Assert.AreEqual(9, gs.Groups.Count);
            Assert.IsTrue(teams.All(t => gs.Groups.SelectMany(g => g.Teams).Contains(t)));
        }
    }
}
