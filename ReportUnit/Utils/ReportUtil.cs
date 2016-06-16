using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReportUnit.Model;
using System.Xml.Linq;
using ReportUnit.Parser;

namespace ReportUnit.Utils
{
    internal class ReportUtil
    {
        /* A */
        public const string Assemblies = "assemblies";
        public const string Assembly = "assembly";
        /* C */
        public const string Count = "count";
        /* D */
        public const string Date = "date";
        /* E */
        public const string EndTime = "end-time";
        public const string Errors = "errors";
        /* F */
        public const string Failed = "failed";
        public const string Failure = "failure";
        public const string Failures = "failures";
        public const string False = "false";
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
        public const string Root = "root";
        public const string RootAlternate = "rootalternate";
        /* S */
        public const string Skipped = "skipped";
        public const string StackTrace = "stack-trace";
        public const string StartTime = "start-time";
        public const string Success = "success";
        /* T */
        public const string Time = "time";
        public const string True = "true";
        public const string Type = "type";
        /* V */
        public const string Value = "value";

        /* NUnit specific ? */
        public const string TestSuite = "test-suite";
        public const string TestCase = "test-case";
        public const string TestResults = "test-results";
        public const string TestRun = "test-run";
        public const string Categories = "categories";
        public const string Category = "category";
        public const string Description = "description";
        public const string Environment = "environment";

        /* XUnit specific ? */
        public const string Collection = "collection";
        public const string Fail = "fail";
        public const string Test = "test";
        public const string Pass = "pass";
        public const string RunDate = "run-date";
        public const string RunTime = "run-time";
        public const string ExceptionType = "exception-type";
        public const string Trait = "trait";

        /// <summary>
        /// This Dictionary contains the lookup string for any element you need.  Format is TestRunner + Lookup Category = Node Name
        /// </summary>
        public static Dictionary<Tuple<TestRunner, string>, string> NodeLookup = new Dictionary<Tuple<TestRunner, string>, string>
        {
            /* NUnit */
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Root), TestResults },
           { new Tuple<TestRunner, string>(TestRunner.NUnit, RootAlternate), TestRun },
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Environment), Environment },
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Count), TestCase },
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Result + Passed), Success},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Result + Failed), Failure},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, TestSuite), TestSuite},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, TestCase), TestCase},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Failure), Failure},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Message), Message},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, StackTrace), StackTrace},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Property), Property},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Category), Category},
           { new Tuple<TestRunner, string>(TestRunner.NUnit, Output), Output},

           /* NUnitV1 */
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Root), TestResults },
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Environment), TestResults },
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Count), TestCase },
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Result + Passed), Passed},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Result + Failed), Failed},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, TestSuite), TestSuite},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, TestCase), TestCase},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Failure), Failure},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Message), Message},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, StackTrace), StackTrace},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Property), Property},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Category), Property},
           { new Tuple<TestRunner, string>(TestRunner.NUnitV1, Output), Output},

           /* XUnitV2 */
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Root), Assemblies },
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Environment), Assembly },
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Count), Test },
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Result + Passed), Pass},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Result + Failed), Fail},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, TestSuite), Collection},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, TestCase), Test},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Failure), Failure},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Message), Message},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, StackTrace), StackTrace},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Property), Property},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Category), Trait},
           { new Tuple<TestRunner, string>(TestRunner.XUnitV2, Output), Output},
        };

        /// <summary>
        /// Pass in a Tuple of the type of tests (TestRunner) and the lookup string (most likely and XElement) and it will return the proper string.
        /// </summary>
        /// <param name="lookup">A Tuple of test type and identifier string</param>
        /// <returns>The string (most likely an XElement) that matches the lookup</returns>
        public static string GetTestRunnerNode(Tuple<TestRunner, string> lookup)
        {
            return NodeLookup.FirstOrDefault(x => x.Key.Equals(lookup)).Value;
        }

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

        public static string FormatTemplate(string source)
        {
            return source.Replace("\r\n", "").Replace("\t", "").Replace("    ", "");
        }
    }
}
