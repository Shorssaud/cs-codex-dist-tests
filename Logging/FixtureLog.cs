﻿using NUnit.Framework;

namespace Logging
{
    public class FixtureLog : BaseLog
    {
        private readonly DateTime start;
        private readonly string fullName;

        public FixtureLog(LogConfig config)
        {
            start = DateTime.UtcNow;
            var folder = DetermineFolder(config);
            var fixtureName = GetFixtureName();
            fullName = Path.Combine(folder, fixtureName);
        }

        public TestLog CreateTestLog()
        {
            return new TestLog(fullName);
        }

        protected override LogFile CreateLogFile()
        {
            return new LogFile(fullName, "log");
        }

        private string DetermineFolder(LogConfig config)
        {
            return Path.Join(
               config.LogRoot,
               $"{start.Year}-{Pad(start.Month)}",
               Pad(start.Day));
        }

        private string GetFixtureName()
        {
            var test = TestContext.CurrentContext.Test;
            var className = test.ClassName!.Substring(test.ClassName.LastIndexOf('.') + 1);
            return $"{Pad(start.Hour)}-{Pad(start.Minute)}-{Pad(start.Second)}Z_{className.Replace('.', '-')}";
        }

        private static string Pad(int n)
        {
            return n.ToString().PadLeft(2, '0');
        }

    }
}
