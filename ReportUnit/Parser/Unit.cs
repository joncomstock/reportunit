using ReportUnit.Logging;
using ReportUnit.Model;
using ReportUnit.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ReportUnit.Parser
{
    public abstract class Unit : IParser
    {
        public string resultsFile;

        private Logger logger = Logger.GetLogger();        

        public Report ProcessTestSuites(XDocument doc, Report report)
        {
            IEnumerable<XElement> suites = doc
                .Descendants(ReportUtil.TestSuite)
                .Where(x => x.Attribute(ReportUtil.Type).Value.Equals("TestFixture", StringComparison.CurrentCultureIgnoreCase));

            suites.AsParallel().ToList().ForEach(ts =>
            {
                var testSuite = new TestSuite();
                testSuite.Name = ts.Attribute(ReportUtil.Name).Value;

                // Suite Time Info
                testSuite = BuildTestSuiteTimeInfo(doc, testSuite, ts);

                // any error messages and/or stack-trace
                testSuite = BuildTestSuiteErrorLog(testSuite, ts);

                testSuite = ProcessTestCases(report, ts, testSuite);

                testSuite.Status = ReportUtil.GetFixtureStatus(testSuite.TestList);

                report.TestSuiteList.Add(testSuite);
            });

            return report;
        }

        public TestSuite ProcessTestCases(Report report, XElement ts, TestSuite testSuite)
        {
            // get test suite level categories
            var suiteCategories = GetCategories(ts, false);

            // Test Casesreport
            ts.Descendants(ReportUtil.TestCase).AsParallel().ToList().ForEach(tc =>
            {
                var test = new Test();

                test.Name = tc.Attribute(ReportUtil.Name).Value;
                test.Status = StatusExtensions.ToStatus(tc.Attribute(ReportUtil.Result).Value);

                // main a master list of all status
                // used to build the status filter in the view
                report.StatusList.Add(test.Status);

                // TestCase Time Info
                test = BuildTestCaseTimeInfo(test, tc);

                // description
                test = BuildTestCaseDescription(test, tc);

                // get test case level categories
                var categories = GetCategories(tc, true);

                //Merge test level categories with suite level categories and add to test and report
                categories.UnionWith(suiteCategories);
                test.CategoryList.AddRange(categories);
                report.CategoryList.AddRange(categories);

                // error and other status messages
                test = BuildTestCaseErrorLog(test, tc);

                // add NUnit console output to the status message
                test.StatusMessage += tc.Element(ReportUtil.Output) != null
                  ? tc.Element(ReportUtil.Output).Value.Trim()
                  : "";

                testSuite.TestList.Add(test);
            });

            return testSuite;
        }

        public Report Parse(string resultsFile)
        {
            this.resultsFile = resultsFile;

            XDocument doc = XDocument.Load(resultsFile);

            Report report = new Report();

            var envElem = doc.Descendants("environment").First();
            report = InitReport(envElem, report);            

            // run-info & environment values -> RunInfo
            var runInfo = CreateRunInfo(envElem, report);
            if (runInfo != null)
            {
                report.AddRunInfo(runInfo.Info);
            }

            // report counts
            report = BuildReportCounts(doc, report);

            // report duration
            report = BuildReportDuration(doc, report);

            // report status messages
            report = BuildReportStatusMessages(doc, report);

            report = ProcessTestSuites(doc, report);            

            //Sort category list so it's in alphabetical order
            report.CategoryList.Sort();

            return report;
        }

        public Report InitReport(XElement elem, Report report)
        {
            report.FileName = Path.GetFileNameWithoutExtension(resultsFile);
            report.AssemblyName = elem != null ? elem.Value : null;
            report.TestRunner = TestRunner.NUnit;

            return report;
        }

        public RunInfo CreateRunInfo(XElement elem, Report report)
        {
            RunInfo runInfo = new RunInfo();
            runInfo.TestRunner = report.TestRunner;

            XElement env = elem;
            runInfo.Info.Add("Test Results File", resultsFile);
            if (env.Attribute("nunit-version") != null)
                runInfo.Info.Add("NUnit Version", env.Attribute("nunit-version").Value);
            runInfo.Info.Add("Assembly Name", report.AssemblyName);
            runInfo.Info.Add("OS Version", env.Attribute("os-version").Value);
            runInfo.Info.Add("Platform", env.Attribute("platform").Value);
            runInfo.Info.Add("CLR Version", env.Attribute("clr-version").Value);
            runInfo.Info.Add("Machine Name", env.Attribute("machine-name").Value);
            runInfo.Info.Add("User", env.Attribute("user").Value);
            runInfo.Info.Add("User Domain", env.Attribute("user-domain").Value);

            return runInfo;
        }

        public virtual Report BuildReportCounts(XDocument doc, Report report)
        {
            // report counts
            report.Total = doc.Descendants(ReportUtil.TestCase).Count();

            report.Passed =
                doc.Root.Attribute(ReportUtil.Passed) != null
                    ? Int32.Parse(doc.Root.Attribute(ReportUtil.Passed).Value)
                    : doc.Descendants(ReportUtil.TestCase).Where(x => x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.Passed, StringComparison.CurrentCultureIgnoreCase)).Count();

            report.Failed =
                doc.Root.Attribute(ReportUtil.Failed) != null
                    ? Int32.Parse(doc.Root.Attribute(ReportUtil.Failed).Value)
                    : doc.Descendants(ReportUtil.TestCase).Where(x => x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.Failed, StringComparison.CurrentCultureIgnoreCase)).Count();

            report.Errors =
                doc.Root.Attribute(ReportUtil.Errors) != null
                    ? Int32.Parse(doc.Root.Attribute(ReportUtil.Errors).Value)
                    : 0;

            report.Inconclusive =
                doc.Root.Attribute(ReportUtil.Inconclusive) != null
                    ? Int32.Parse(doc.Root.Attribute(ReportUtil.Inconclusive).Value)
                    : 0;

            report.Skipped =
                doc.Root.Attribute(ReportUtil.Skipped) != null
                    ? Int32.Parse(doc.Root.Attribute(ReportUtil.Skipped).Value)
                    : 0;

            report.Skipped +=
                doc.Root.Attribute(ReportUtil.Ignored) != null
                    ? Int32.Parse(doc.Root.Attribute(ReportUtil.Ignored).Value)
                    : 0;

            return report;
        }

        public virtual Report BuildReportDuration(XDocument doc, Report report)
        {
            // report duration
            report.StartTime =
                doc.Root.Attribute(ReportUtil.StartTime) != null
                    ? doc.Root.Attribute(ReportUtil.StartTime).Value
                    : doc.Root.Attribute(ReportUtil.Date).Value + " " + doc.Root.Attribute(ReportUtil.Time).Value;

            report.EndTime =
                doc.Root.Attribute(ReportUtil.EndTime) != null
                    ? doc.Root.Attribute(ReportUtil.EndTime).Value
                    : "";

            return report;
        }

        public virtual Report BuildReportStatusMessages(XDocument doc, Report report)
        {
            var testSuiteTypeAssembly = doc.Descendants(ReportUtil.TestSuite)
                .Where(x => x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.Failed, StringComparison.CurrentCultureIgnoreCase) && x.Attribute("type").Value.Equals(ReportUtil.Assembly, StringComparison.CurrentCultureIgnoreCase));
            report.StatusMessage = testSuiteTypeAssembly != null && testSuiteTypeAssembly.Count() > 0
                ? testSuiteTypeAssembly.First().Value
                : "";

            return report;
        }

        public virtual TestSuite BuildTestSuiteTimeInfo(XDocument doc, TestSuite testSuite, XElement ts)
        {
            testSuite.StartTime =
                    ts.Attribute(ReportUtil.StartTime) != null
                        ? ts.Attribute(ReportUtil.StartTime).Value
                        : string.Empty;

            testSuite.StartTime =
                String.IsNullOrEmpty(testSuite.StartTime) && ts.Attribute(ReportUtil.Time) != null
                    ? ts.Attribute(ReportUtil.Time).Value
                    : testSuite.StartTime;

            testSuite.EndTime =
                ts.Attribute(ReportUtil.EndTime) != null
                    ? ts.Attribute(ReportUtil.EndTime).Value
                    : "";

            return testSuite;
        }

        /// <summary>
        /// Returns categories for the direct children or all descendents of an XElement
        /// </summary>
        /// <param name="elem">XElement to parse</param>
        /// <param name="allDescendents">If true, return all descendent categories.  If false, only direct children</param>
        /// <returns></returns>
        public HashSet<string> GetCategories(XElement elem, bool allDescendents)
        {
            //Define which function to use
            var parser = allDescendents
                ? new Func<XElement, string, IEnumerable<XElement>>((e, s) => e.Descendants(s))
                : new Func<XElement, string, IEnumerable<XElement>>((e, s) => e.Elements(s));

            //Grab unique categories
            HashSet<string> categories = new HashSet<string>();
            bool hasCategories = parser(elem, ReportUtil.Categories).Any();
            if (hasCategories)
            {
                List<XElement> cats = parser(elem, ReportUtil.Categories).Elements(ReportUtil.Category).ToList();

                cats.ForEach(x =>
                {
                    string cat = x.Attribute(ReportUtil.Name).Value;
                    categories.Add(cat);
                });
            }

            // if this is a parameterized test, get the categories from the parent test-suite
            var parameterizedTestElement = elem
                .Ancestors(ReportUtil.TestSuite).ToList()
                .Where(x => x.Attribute("type").Value.Equals("ParameterizedTest", StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();

            if (null != parameterizedTestElement)
            {
                var paramCategories = GetCategories(parameterizedTestElement, false);
                categories.UnionWith(paramCategories);
            }

            return categories;
        }

        public virtual TestSuite BuildTestSuiteErrorLog(TestSuite testSuite, XElement ts)
        {
            var failure = ts.Element(ReportUtil.Failure);
            if (failure != null)
            {
                var message = failure.Element(ReportUtil.Message);
                if (message != null)
                {
                    testSuite.StatusMessage = message.Value;
                }

                var stackTrace = failure.Element(ReportUtil.StackTrace);
                if (stackTrace != null && !string.IsNullOrWhiteSpace(stackTrace.Value))
                {
                    testSuite.StatusMessage = string.Format(
                        "{0}\n\nStack trace:\n{1}", testSuite.StatusMessage, stackTrace.Value);
                }
            }

            return testSuite;
        }

        public virtual Test BuildTestCaseTimeInfo(Test test, XElement tc)
        {
            test.StartTime =
                        tc.Attribute(ReportUtil.StartTime) != null
                            ? tc.Attribute(ReportUtil.StartTime).Value
                            : "";
            test.StartTime =
                String.IsNullOrEmpty(test.StartTime) && (tc.Attribute(ReportUtil.Time) != null)
                    ? tc.Attribute(ReportUtil.Time).Value
                    : test.StartTime;
            test.EndTime =
                tc.Attribute(ReportUtil.EndTime) != null
                    ? tc.Attribute(ReportUtil.EndTime).Value
                    : "";

            return test;
        }

        public virtual Test BuildTestCaseDescription(Test test, XElement tc)
        {
            var description =
                        tc.Descendants(ReportUtil.Property)
                        .Where(c => c.Attribute(ReportUtil.Name).Value.Equals(ReportUtil.Description, StringComparison.CurrentCultureIgnoreCase));
            test.Description =
                description.Count() > 0
                    ? description.ToArray()[0].Attribute(ReportUtil.Value).Value
                    : "";

            return test;
        }

        public virtual Test BuildTestCaseErrorLog(Test test, XElement tc)
        {
            test.StatusMessage =
                        tc.Element(ReportUtil.Failure) != null
                            ? tc.Element(ReportUtil.Failure).Element(ReportUtil.Message).Value.Trim()
                            : "";
            test.StatusMessage +=
                tc.Element(ReportUtil.Failure) != null
                    ? tc.Element(ReportUtil.Failure).Element(ReportUtil.StackTrace) != null
                        ? tc.Element(ReportUtil.Failure).Element(ReportUtil.StackTrace).Value.Trim()
                        : ""
                    : "";

            test.StatusMessage += tc.Element(ReportUtil.Reason) != null && tc.Element(ReportUtil.Reason).Element(ReportUtil.Message) != null
                ? tc.Element(ReportUtil.Reason).Element(ReportUtil.Message).Value.Trim()
                : "";

            return test;
        }
    }
}
