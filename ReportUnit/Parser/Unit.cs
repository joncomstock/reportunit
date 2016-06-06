using ReportUnit.Logging;
using ReportUnit.Model;
using ReportUnit.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml.Linq;

namespace ReportUnit.Parser
{
    public abstract class Unit : IParser
    {
        public string resultsFile;

        private Logger logger = Logger.GetLogger();        

        /// <summary>
        /// Processes the outer collections of tests, which are labeled as "Suites" here, and adds them to the Report.
        /// </summary>
        /// <param name="doc">XDocument to be processed</param>
        /// <param name="report">Report</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns>The Report with Test "Suites" added</returns>
        public Report ProcessTestSuites(XDocument doc, Report report, TestRunner testRunner)
        {
            IEnumerable<XElement> suites = GetTestSuites(doc, testRunner);

            suites.AsParallel().ToList().ForEach(ts =>
            {
                var testSuite = new TestSuite();
                testSuite.Name = ts.Attribute(ReportUtil.Name).Value;

                // Suite Time Info
                testSuite = BuildTestSuiteTimeInfo(doc, testSuite, ts);

                // any error messages and/or stack-trace
                testSuite = BuildTestSuiteErrorLog(testSuite, ts, testRunner);

                testSuite = ProcessTestCases(report, ts, testSuite, testRunner);

                testSuite.Status = ReportUtil.GetFixtureStatus(testSuite.TestList);

                report.TestSuiteList.Add(testSuite);
            });

            return report;
        }

        /// <summary>
        /// Retrieve the Test "Suites" based off of the TestRunner type and return them in an IEnumerable.
        /// </summary>
        /// <param name="doc">XDocument</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns>An IEnumberalbe of TestSuites</returns>
        public IEnumerable<XElement> GetTestSuites(XDocument doc, TestRunner testRunner)
        {
            var testSuites =
                doc.Descendants(
                    ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.TestSuite)));

            var suites = testSuites
                .Where(x => x.Descendants(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.TestCase))).Any());

            return suites;
        }

        /// <summary>
        /// Process all of the data for the inner tests, each test that is run that belongs to a parent TestSuite.
        /// </summary>
        /// <param name="report">Report</param>
        /// <param name="ts">The parent TestSuite</param>
        /// <param name="testSuite">TestSuite</param>
        /// <param name="testRunner">TestRunner</param>
        /// <returns>Returns a TestSuite filled with TestCases aka individual tests</returns>
        public TestSuite ProcessTestCases(Report report, XElement ts, TestSuite testSuite, TestRunner testRunner)
        {
            // get test suite level categories
            var suiteCategories = GetCategories(ts, false, testRunner);

            // Test Cases report
            ts.Descendants(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.TestCase))).AsParallel().ToList().ForEach(tc =>
            {
                var test = new Test();

                test.Name = tc.Attribute(ReportUtil.Name).Value;
                test.Status =
                    StatusExtensions.ToStatus(tc.Attribute(ReportUtil.Result) != null
                        ? tc.Attribute(ReportUtil.Result).Value
                        : tc.Attribute(ReportUtil.Success).Value);

                // main a master list of all status
                // used to build the status filter in the view
                report.StatusList.Add(test.Status);

                // TestCase Time Info
                test = BuildTestCaseTimeInfo(test, tc, testRunner);

                // description
                test = BuildTestCaseDescription(test, tc, testRunner);

                // get test case level categories
                var categories = GetCategories(tc, true, testRunner);

                //Merge test level categories with suite level categories and add to test and report
                categories.UnionWith(suiteCategories);
                test.CategoryList.AddRange(categories);
                report.CategoryList.AddRange(categories);

                // error and other status messages
                test = BuildTestCaseErrorLog(test, tc, testRunner);

                var statusMessage =
                    tc.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Output)));
                // add NUnit console output to the status message -- can be modified if other types need to use this
                test.StatusMessage += statusMessage != null
                    ? statusMessage.Value.Trim()
                    : "";

                testSuite.TestList.Add(test);
            });

            return testSuite;
        }

        /// <summary>
        /// Where the bulk of the processing happens.  Parse the results file to retrieve and process all the necessary data to build the
        /// dashboard and results pages.
        /// </summary>
        /// <param name="resultsFile">String version of the XML document</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns>Report</returns>
        public Report Parse(string resultsFile, TestRunner testRunner)
        {
            this.resultsFile = resultsFile;

            XDocument doc = XDocument.Load(resultsFile);

            Report report = new Report();

            var node = ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Root));
            var rootElem = doc.Elements(node).FirstOrDefault() ?? doc.Descendants(
                ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.RootAlternate))).FirstOrDefault();
            report = InitReport(rootElem, report, testRunner);

            // run-info & environment values -> RunInfo
            var elemName =
                ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(report.TestRunner, ReportUtil.Environment));
            var elem = doc.Descendants(elemName).FirstOrDefault();
            var runInfo = CreateRunInfo(elem, report);
            if (runInfo != null)
            {
                report.AddRunInfo(runInfo.Info);
            }

            // report counts
            var countNode = ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Count));
            report = BuildReportCounts(doc.Root, rootElem, countNode, report, testRunner);

            // report duration
            report = BuildReportDuration(rootElem, report);

            // report status messages
            var testSuiteTypeAssembly = doc.Descendants(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.TestSuite)))
                .FirstOrDefault(x => (x.Attribute(ReportUtil.Result) != null && x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Result + ReportUtil.Failed)), StringComparison.CurrentCultureIgnoreCase)) && x.Attribute("type").Value.Equals(ReportUtil.Assembly, StringComparison.CurrentCultureIgnoreCase));
            report = BuildReportStatusMessages(testSuiteTypeAssembly, report, testRunner);

            report = ProcessTestSuites(doc, report, testRunner);

            //Sort category list so it's in alphabetical order
            report.CategoryList.Sort();

            return report;
        }

        /// <summary>
        /// Set up the top level data about the Report.
        /// </summary>
        /// <param name="elem">XElement to retrieve top level data from</param>
        /// <param name="report">Report to add the data to</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns>Report with top level data added</returns>
        public Report InitReport(XElement elem, Report report, TestRunner testRunner)
        {
            report.FileName = Path.GetFileNameWithoutExtension(resultsFile);
            report.AssemblyName = elem != null ? elem.Value : null;
            report.TestRunner = testRunner;

            return report;
        }

        /// <summary>
        /// Add RunInfo to the Report based off of your type of TestRunner. Each type that inherits from this
        /// should create their own version, to individual not to.
        /// </summary>
        /// <param name="elem">XElement to retrieve the data from</param>
        /// <param name="report">Report to add the data to</param>
        /// <returns>A Run Info object</returns>
        public abstract RunInfo CreateRunInfo(XElement elem, Report report);

        /// <summary>
        /// Retrieve the Counts for each type of Report possibility, to display in the charts.
        /// </summary>
        /// <param name="docRoot">The Root Element of the entire document</param>
        /// <param name="countRoot">The Element that contains all of the count data</param>
        /// <param name="countNode">The Node (or series of Nodes) to serach for to manually create the Counts if countRoot doesn't contain the data</param>
        /// <param name="report">Report to add the data to</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns></returns>
        public Report BuildReportCounts(XElement docRoot, XElement countRoot, string countNode, Report report, TestRunner testRunner)
        {
            // report counts
            report.Total = docRoot.Descendants(countNode).Count();

            report.Passed =
                countRoot.Attribute(ReportUtil.Passed) != null
                    ? Int32.Parse(countRoot.Attribute(ReportUtil.Passed).Value)
                    : docRoot.Descendants(countNode).Count(x => x.Attribute(ReportUtil.Result) != null ? x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Result + ReportUtil.Passed)), StringComparison.CurrentCultureIgnoreCase) : x.Attribute(ReportUtil.Success).Value.Equals(ReportUtil.True, StringComparison.CurrentCultureIgnoreCase));

            report.Failed =
                countRoot.Attribute(ReportUtil.Failed) != null
                    ? Int32.Parse(countRoot.Attribute(ReportUtil.Failed).Value)
                    : docRoot.Descendants(countNode).Count(x => x.Attribute(ReportUtil.Result) != null ? x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Result + ReportUtil.Failed)), StringComparison.CurrentCultureIgnoreCase) : x.Attribute(ReportUtil.Success).Value.Equals(ReportUtil.False, StringComparison.CurrentCultureIgnoreCase));

            report.Errors =
                countRoot.Attribute(ReportUtil.Errors) != null
                    ? Int32.Parse(countRoot.Attribute(ReportUtil.Errors).Value)
                    : 0;

            report.Inconclusive =
                countRoot.Attribute(ReportUtil.Inconclusive) != null
                    ? Int32.Parse(countRoot.Attribute(ReportUtil.Inconclusive).Value)
                    : 0;

            report.Skipped =
                countRoot.Attribute(ReportUtil.Skipped) != null
                    ? Int32.Parse(countRoot.Attribute(ReportUtil.Skipped).Value)
                    : 0;

            report.Skipped +=
                countRoot.Attribute(ReportUtil.Ignored) != null
                    ? Int32.Parse(countRoot.Attribute(ReportUtil.Ignored).Value)
                    : 0;

            return report;
        }

        /// <summary>
        /// Add the timing info for the entire Report.
        /// </summary>
        /// <param name="root">Element that contains the timing data</param>
        /// <param name="report">Report to add the timing data to</param>
        /// <returns>Report with timing data added</returns>
        public Report BuildReportDuration(XElement root, Report report)
        {
            // report duration
            report.StartTime =
                root.Attribute(ReportUtil.StartTime) != null
                    ? root.Attribute(ReportUtil.StartTime).Value
                    : (root.Attribute(ReportUtil.Date) != null && root.Attribute(ReportUtil.Time) != null) 
                        ? (root.Attribute(ReportUtil.Date).Value + " " + root.Attribute(ReportUtil.Time).Value) 
                        : "";

            report.EndTime =
                root.Attribute(ReportUtil.EndTime) != null
                    ? root.Attribute(ReportUtil.EndTime).Value
                    : "";

            return report;
        }

        /// <summary>
        /// Add status messages to the Report.
        /// </summary>
        /// <param name="elem">Element that contains the Status Message data</param>
        /// <param name="report">Report to add the data to</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns>Report with Status Message added</returns>
        public Report BuildReportStatusMessages(XElement elem, Report report, TestRunner testRunner)
        {            
            report.StatusMessage = elem != null ? elem.Value : "";

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
        public HashSet<string> GetCategories(XElement elem, bool allDescendents, TestRunner testRunner)
        {
            //Define which function to use
            var parser = allDescendents
                ? new Func<XElement, string, IEnumerable<XElement>>((e, s) => e.Descendants(s))
                : new Func<XElement, string, IEnumerable<XElement>>((e, s) => e.Elements(s));

            //Grab unique categories
            HashSet<string> categories = new HashSet<string>();
            List<XElement> cats = parser(elem, ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Category))).ToList();
            if (cats.Any())
            {
                var attrCategories = cats.Where(x => x.Attribute(ReportUtil.Name).Value.Equals(ReportUtil.Category, StringComparison.CurrentCultureIgnoreCase));

                Console.Write("attrCategories \n");
                Console.Write(attrCategories.Any());


                if (attrCategories.Any())
                {
                    //Category is the name of the tag
                    attrCategories.ToList().ForEach(x =>
                    {
                        categories.Add(x.Attribute(ReportUtil.Value).Value);

                    });
                }
                else
                {
                    //Category is the XML tag
                    cats.ForEach(x =>
                    {
                        categories.Add(x.Attribute(ReportUtil.Name).Value);
                    });
                }
            }

            // if this is a parameterized test, get the categories from the parent test-suite
            var parameterizedTestElement = elem
                .Ancestors(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.TestSuite))).ToList()
                .Where(x => x.Attribute("type") != null && x.Attribute("type").Value.Equals("ParameterizedTest", StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();

            if (null != parameterizedTestElement)
            {
                var paramCategories = GetCategories(parameterizedTestElement, false, testRunner);
                categories.UnionWith(paramCategories);
            }

            return categories;
        }

        public TestSuite BuildTestSuiteErrorLog(TestSuite testSuite, XElement ts, TestRunner testRunner)
        {
            var failure = ts.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Failure)));
            if (failure != null)
            {
                var message = failure.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Message)));
                if (message != null)
                {
                    testSuite.StatusMessage = message.Value;
                }

                var stackTrace = failure.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.StackTrace)));
                if (stackTrace != null && !string.IsNullOrWhiteSpace(stackTrace.Value))
                {
                    testSuite.StatusMessage = string.Format(
                        "{0}\n\nStack trace:\n{1}", testSuite.StatusMessage, stackTrace.Value);
                }
            }

            return testSuite;
        }

        /// <summary>
        /// Add the timing data 
        /// </summary>
        /// <param name="test"></param>
        /// <param name="tc"></param>
        /// <returns></returns>
        public Test BuildTestCaseTimeInfo(Test test, XElement tc, TestRunner testRunner)
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

        /// <summary>
        /// Add the Description data for each individual test.
        /// </summary>
        /// <param name="test">Test to add the data to</param>
        /// <param name="tc">Test element that contains the data</param>
        /// <returns>Test with Description data added</returns>
        public Test BuildTestCaseDescription(Test test, XElement tc, TestRunner testRunner)
        {
            var description =
                        tc.Descendants(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Property)))
                        .Where(c => c.Attribute(ReportUtil.Name).Value.Equals(ReportUtil.Description, StringComparison.CurrentCultureIgnoreCase));
            test.Description =
                description.Any()
                    ? description.ToArray()[0].Attribute(ReportUtil.Value).Value
                    : "";

            return test;
        }

        /// <summary>
        /// Build up the Status Message with Error logs from each test, if errors exist.
        /// </summary>
        /// <param name="test">Test to add the data to</param>
        /// <param name="tc">Test element that contains the data</param>
        /// <param name="testRunner">Type of tests</param>
        /// <returns>Test with the Errors added if necessary</returns>
        public Test BuildTestCaseErrorLog(Test test, XElement tc, TestRunner testRunner)
        {
            var failureElement =
                tc.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Failure)));
                            
            var reasonElement =
                tc.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Reason)));

            test.StatusMessage =
                        failureElement != null
                            ? failureElement.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Message))).Value.Trim()
                            : "";
            test.StatusMessage +=
                failureElement != null
                    ? failureElement.Element(
                        ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.StackTrace))) != null
                        ? failureElement.Element(
                        ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.StackTrace))).Value.Trim()
                        : ""
                    : "";

            test.StatusMessage += reasonElement != null && reasonElement.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Message))) != null
                ? reasonElement.Element(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.Message))).Value.Trim()
                : "";

            return test;
        }
    }
}
