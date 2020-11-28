using System.IO;
using System.Reflection;

namespace UAFEPUnitTests
{
    public static class TestTools
    {
        public static string GetFileContent(string filenameWithoutExtension)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Datas\\{filenameWithoutExtension}.json");
            return File.ReadAllText(path);
        }
    }
}
