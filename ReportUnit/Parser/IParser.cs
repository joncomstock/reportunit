using ReportUnit.Model;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ReportUnit.Parser
{
    public interface IParser
    {
        Report Parse(string filePath);
        RunInfo CreateRunInfo(XElement elem, Report report);
        Report ProcessTestSuites(XDocument doc, Report report);
        TestSuite ProcessTestCases(Report report, XElement ts, TestSuite testSuite);

        //Report BuildReportCounts(XDocument doc, Report report);
        //Report BuildReportDuration(XDocument doc, Report report);
        //Report BuildReportStatusMessages(XDocument doc, Report report);
        //TestSuite BuildTestSuiteErrorLog(TestSuite testSuite, XElement ts);
        //Test BuildTestCaseTimeInfo(Test test, XElement tc);
    }
}
