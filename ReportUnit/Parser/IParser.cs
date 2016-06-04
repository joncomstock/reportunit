using ReportUnit.Model;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ReportUnit.Parser
{
    public interface IParser
    {
        Report Parse(string filePath, TestRunner testRunner);
        RunInfo CreateRunInfo(XElement elem, Report report);
        //Report ProcessTestSuites(XDocument doc, Report report, TestRunner testRunner);
        //TestSuite ProcessTestCases(Report report, XElement ts, TestSuite testSuite, TestRunner testRunner);

        //Report BuildReportCounts(XDocument doc, Report report);
        //Report BuildReportDuration(XDocument doc, Report report);
        //Report BuildReportStatusMessages(XDocument doc, Report report);
        //TestSuite BuildTestSuiteErrorLog(TestSuite testSuite, XElement ts);
        //Test BuildTestCaseTimeInfo(Test test, XElement tc);
    }
}
