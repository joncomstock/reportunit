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
    internal class XUnitV2 : IParser
    {
        private string resultsFile;

        private Logger logger = Logger.GetLogger();

        public Report Parse(string resultsFile)
        {
            this.resultsFile = resultsFile;

            XDocument doc = XDocument.Load(resultsFile);

            Report report = new Report();

            report.FileName = Path.GetFileNameWithoutExtension(resultsFile);
            var assemblyNode = doc.Root.Descendants("assembly").FirstOrDefault();
            report.AssemblyName = assemblyNode != null ? assemblyNode.Attribute("name").Value : null;
            report.TestRunner = TestRunner.XUnitV2;

            //run - info & environment values->RunInfo
            var runInfo = CreateRunInfo(assemblyNode, report);
            if (runInfo != null)
            {
                report.AddRunInfo(runInfo.Info);
            }

            // report counts
            report.Total = doc.Descendants("test").Count();

            report.Passed =
                assemblyNode.Attribute("passed") != null
                    ? Int32.Parse(assemblyNode.Attribute("passed").Value)
                    : doc.Descendants("test").Where(x => x.Attribute("result").Value.Equals("Pass", StringComparison.CurrentCultureIgnoreCase)).Count();

            report.Failed =
                assemblyNode.Attribute("failed") != null
                    ? Int32.Parse(assemblyNode.Attribute("failed").Value)
                    : doc.Descendants("test").Where(x => x.Attribute("result").Value.Equals("Fail", StringComparison.CurrentCultureIgnoreCase)).Count();

            report.Errors =
                assemblyNode.Attribute("errors") != null
                    ? Int32.Parse(assemblyNode.Attribute("errors").Value)
                    : 0;

            report.Inconclusive =
                assemblyNode.Attribute("inconclusive") != null
                    ? Int32.Parse(assemblyNode.Attribute("inconclusive").Value)
                    : Int32.Parse(assemblyNode.Attribute("inconclusive").Value);

            report.Skipped =
                assemblyNode.Attribute("skipped") != null
                    ? Int32.Parse(assemblyNode.Attribute("skipped").Value)
                    : Int32.Parse(assemblyNode.Attribute("skipped").Value);

            report.Skipped +=
                assemblyNode.Attribute("ignored") != null
                    ? Int32.Parse(assemblyNode.Attribute("ignored").Value)
                    : 0;

            // report duration
            report.StartTime =
                assemblyNode.Attribute("run-date") != null && assemblyNode.Attribute("run-time") != null
                    ? assemblyNode.Attribute("run-date").Value + " " + assemblyNode.Attribute("run-time").Value
                    : "";

            report.EndTime =
                assemblyNode.Attribute("end-time") != null
                    ? assemblyNode.Attribute("end-time").Value
                    : "";

            //test suites
            IEnumerable<XElement> collections = doc.Descendants("collection");

            collections.AsParallel().ToList().ForEach(c =>
            {
                var testSuite = new TestSuite();
                testSuite.Name = c.Attribute("name").Value;

                // Suite Time Info
                testSuite.StartTime =
                    String.IsNullOrEmpty(testSuite.StartTime) && c.Attribute("time") != null
                        ? c.Attribute("time").Value
                        : "";

                testSuite.EndTime = "";

                // any error messages and/or stack-trace
                var failure = c.Element("failure");
                if (failure != null)
                {
                    var message = failure.Element("message");
                    if (message != null)
                    {
                        testSuite.StatusMessage = message.Value;
                    }

                    var stackTrace = failure.Element("stack-trace");
                    if (stackTrace != null && !string.IsNullOrWhiteSpace(stackTrace.Value))
                    {
                        testSuite.StatusMessage = string.Format(
                            "{0}\n\nStack trace:\n{1}", testSuite.StatusMessage, stackTrace.Value);
                    }
                }

                // get test suite level categories
                //var suiteCategories = ReportUtil.GetCategories(c, false);

                // Test Cases
                c.Descendants("test").AsParallel().ToList().ForEach(tc =>
                {
                    var test = new Test();

                    test.Name = tc.Attribute("name").Value;
                    test.Status = StatusExtensions.ToStatus(tc.Attribute("result").Value);

                    // main a master list of all status
                    // used to build the status filter in the view
                    report.StatusList.Add(test.Status);

                    // TestCase Time Info
                    test.StartTime =
                        tc.Attribute("time") != null
                            ? tc.Attribute("time").Value
                            : "";
                    test.EndTime = "";

                    // get test case level categories
                    var categories = ReportUtil.GetCategories(tc, true);

                    test.CategoryList.AddRange(categories);
                    report.CategoryList.AddRange(categories);

                    var failureNode = tc.Element("failure");
                    var messageNode = failureNode != null ? failureNode.Element("message") : null;
                    var stackTraceNode = failureNode != null ? failureNode.Element("stack-trace") : null;
                    var reasonNode = tc.Element("reason");                    

                    // error and other status messages
                    test.StatusMessage =
                        failureNode != null
                            ? failureNode.Attribute("exception-type").Value.Trim()
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
        private RunInfo CreateRunInfo(XElement elem, Report report)
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
