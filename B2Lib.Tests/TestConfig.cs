using System;
using System.IO;

namespace B2Lib.Tests
{
    public static class TestConfig
    {
        private static string[] GetLines()
        {
            // To protect credentials from being committed, they've been put in a text file at: MyDocuments\B2Auth.txt
            // The text goes in two lines, with AccountId being the first, and ApplicationKey the second.
            try
            {
                return File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "B2Auth.txt"));
            }
            catch (Exception ex)
            {
                throw new Exception("Please place a B2Auth.txt folder in the MyDocuments folder. See more in TestConfig.cs", ex);
            }
        }

        public static string AccountId => GetLines()[0];

        public static string ApplicationKey => GetLines()[1];
    }
}
