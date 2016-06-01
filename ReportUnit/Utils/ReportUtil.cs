using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReportUnit.Model;
using System.Xml.Linq;

namespace ReportUnit.Utils
{
    internal class ReportUtil
    {
        /* A */
        public const string Assembly = "assembly";
        /* D */
        public const string Date = "date";
        /* E */
        public const string EndTime = "end-time";
        public const string Errors = "errors";
        /* F */
        public const string Failed = "failed";
        public const string Failure = "failure";
        public const string Failures = "failures";
        /* I */
        public const string Inconclusive = "inconclusive";
        public const string Ignored = "ignored";
        /* M */
        public const string Message = "message";
        /* N */
        public const string Name = "name";
        /* O */
        public const string Output = "output";
        /* P */
        public const string Passed = "passed";
        public const string Property = "property";
        /* R */
        public const string Reason = "reason";
        public const string Result = "result";
        /* S */
        public const string Skipped = "skipped";
        public const string StackTrace = "stack-trace";
        public const string StartTime = "start-time";
        /* T */        
        public const string Time = "time";
        public const string Type = "type";
        /* V */
        public const string Value = "value";
        
        /* NUnit specific ? */
        public const string TestSuite = "test-suite";
        public const string TestCase = "test-case";
        public const string Categories = "categories";
        public const string Category = "category";
        public const string Description = "description";

        /* XUnit specific ? */
        public const string Fail = "fail";
        public const string Test = "test";
        public const string Pass = "pass";
        public const string RunDate = "run-date";
        public const string RunTime = "run-time";
        public const string ExceptionType = "exception-type";


        // fixture level status codes
        public static Status GetFixtureStatus(IEnumerable<Test> tests)
        {
            return GetFixtureStatus(tests.Select(t => t.Status).ToList());
        }

        // fixture level status codes
        public static Status GetFixtureStatus(List<Status> statuses)
        {
            if (statuses.Any(x => x == Status.Failed)) return Status.Failed;
            if (statuses.Any(x => x == Status.Error)) return Status.Error;
            if (statuses.Any(x => x == Status.Inconclusive)) return Status.Inconclusive;
            if (statuses.Any(x => x == Status.Passed)) return Status.Passed;
            if (statuses.Any(x => x == Status.Skipped)) return Status.Skipped;

            return Status.Unknown;
        }       
    }
}
