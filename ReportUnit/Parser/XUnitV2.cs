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
    internal class XUnitV2 : Unit
    {
        //private string resultsFile;

        //private Logger logger = Logger.GetLogger();

        public Report Parse(string resultsFile)
        {
            this.resultsFile = resultsFile;

            XDocument doc = XDocument.Load(resultsFile);

            Report report = new Report();

            var assemblyNode = doc.Root.Descendants("assembly").FirstOrDefault();
            report = InitReport(assemblyNode, report);

            //run - info & environment values->RunInfo
            var runInfo = CreateRunInfo(assemblyNode, report);
            if (runInfo != null)
            {
                report.AddRunInfo(runInfo.Info);
            }

            // report counts
            report.Total = doc.Descendants(ReportUtil.Test).Count();

            report.Passed =
                assemblyNode.Attribute(ReportUtil.Passed) != null
                    ? Int32.Parse(assemblyNode.Attribute(ReportUtil.Passed).Value)
                    : doc.Descendants(ReportUtil.Test).Where(x => x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.Pass, StringComparison.CurrentCultureIgnoreCase)).Count();

            report.Failed =
                assemblyNode.Attribute(ReportUtil.Failed) != null
                    ? Int32.Parse(assemblyNode.Attribute(ReportUtil.Failed).Value)
                    : doc.Descendants(ReportUtil.Test).Where(x => x.Attribute(ReportUtil.Result).Value.Equals(ReportUtil.Fail, StringComparison.CurrentCultureIgnoreCase)).Count();

            report.Errors =
                assemblyNode.Attribute(ReportUtil.Errors) != null
                    ? Int32.Parse(assemblyNode.Attribute(ReportUtil.Errors).Value)
                    : 0;

            report.Inconclusive =
                assemblyNode.Attribute(ReportUtil.Inconclusive) != null
                    ? Int32.Parse(assemblyNode.Attribute(ReportUtil.Inconclusive).Value)
                    : 0;

            report.Skipped =
                assemblyNode.Attribute(ReportUtil.Skipped) != null
                    ? Int32.Parse(assemblyNode.Attribute(ReportUtil.Skipped).Value)
                    : Int32.Parse(assemblyNode.Attribute(ReportUtil.Skipped).Value);
                
            report.Skipped +=
                assemblyNode.Attribute(ReportUtil.Ignored) != null
                    ? Int32.Parse(assemblyNode.Attribute(ReportUtil.Ignored).Value)
                    : 0;

            // report duration
            report.StartTime =
                assemblyNode.Attribute(ReportUtil.RunDate) != null && assemblyNode.Attribute(ReportUtil.RunTime) != null
                    ? assemblyNode.Attribute(ReportUtil.RunDate).Value + " " + assemblyNode.Attribute(ReportUtil.RunTime).Value
                    : "";

            report.EndTime =
                assemblyNode.Attribute(ReportUtil.EndTime) != null
                    ? assemblyNode.Attribute(ReportUtil.EndTime).Value
                    : "";

            //test suites
            IEnumerable<XElement> collections = doc.Descendants("collection");

            collections.AsParallel().ToList().ForEach(c =>
            {
                var testSuite = new TestSuite();
                testSuite.Name = c.Attribute(ReportUtil.Name).Value;

                // Suite Time Info
                testSuite.StartTime =
                    String.IsNullOrEmpty(testSuite.StartTime) && c.Attribute(ReportUtil.Time) != null
                        ? c.Attribute(ReportUtil.Time).Value
                        : "";

                testSuite.EndTime = "";

                // any error messages and/or stack-trace
                var failure = c.Element(ReportUtil.Failure);
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

                // get test suite level categories
                //var suiteCategories = ReportUtil.GetCategories(c, false);

                // Test Cases
                c.Descendants(ReportUtil.Test).AsParallel().ToList().ForEach(tc =>
                {
                    var test = new Test();

                    test.Name = tc.Attribute(ReportUtil.Name).Value;
                    test.Status = StatusExtensions.ToStatus(tc.Attribute(ReportUtil.Result).Value);

                    // main a master list of all status
                    // used to build the status filter in the view
                    report.StatusList.Add(test.Status);

                    // TestCase Time Info
                    test.StartTime =
                        tc.Attribute(ReportUtil.Time) != null
                            ? tc.Attribute(ReportUtil.Time).Value
                            : "";
                    test.EndTime = "";

                    // get test case level categories
                    var categories = GetCategories(tc, true);

                    test.CategoryList.AddRange(categories);
                    report.CategoryList.AddRange(categories);

                    var failureNode = tc.Element(ReportUtil.Failure);
                    var messageNode = failureNode != null ? failureNode.Element(ReportUtil.Message) : null;
                    var stackTraceNode = failureNode != null ? failureNode.Element(ReportUtil.StackTrace) : null;
                    var reasonNode = tc.Element(ReportUtil.Reason);                    

                    // error and other status messages
                    test.StatusMessage =
                        failureNode != null
                            ? failureNode.Attribute(ReportUtil.ExceptionType).Value.Trim()
                            : "";

                    test.StatusMessage +=
                        failureNode != null
                            ? messageNode.Value.Trim()
                            : "";

                    test.StatusMessage +=
                        failureNode != null
                            ? stackTraceNode != null
                                ? stackTraceNode.Value.Trim()
                                : ""
                            : "";

                    test.StatusMessage += reasonNode != null && messageNode != null
                        ? messageNode.Value.Trim()
                        : "";

                    testSuite.TestList.Add(test);
                });

                testSuite.Status = ReportUtil.GetFixtureStatus(testSuite.TestList);

                report.TestSuiteList.Add(testSuite);
            });

            //Sort category list so it's in alphabetical order
            report.CategoryList.Sort();

            return report;
        }


        /// <summary>
        /// Pass in an XElement that contains all of the Run Info information.
        /// </summary>
        /// <param name="elem">Element to obtain Run Info information from</param>
        /// <param name="report"></param>
        /// <returns>RunInfo for the file</returns>
        public RunInfo CreateRunInfo(XElement elem, Report report)
        {
            if (elem.Attribute("environment") == null)
                return null;

            RunInfo runInfo = new RunInfo();
            runInfo.TestRunner = report.TestRunner;

            runInfo.Info.Add("Test Results File", resultsFile);
            if (elem.Attribute("test-framework") != null)
                runInfo.Info.Add("XUnit Version", elem.Attribute("test-framework").Value);
            runInfo.Info.Add("Assembly Name", report.AssemblyName);
            runInfo.Info.Add("Environment", elem.Attribute("environment").Value);

            return runInfo;
        }

        public XUnitV2() { }
    }
}
