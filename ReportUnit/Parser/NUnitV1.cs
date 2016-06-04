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
    internal class NUnitV1 : Unit
    {
        public NUnitV1() { }

        public override RunInfo CreateRunInfo(XElement elem, Report report)
        {
            RunInfo runInfo = new RunInfo();
            runInfo.TestRunner = report.TestRunner;

            runInfo.Info.Add("Test Results File", resultsFile);
            if (elem == null) return runInfo;
            runInfo.Info.Add("Assembly Name", report.AssemblyName);
            runInfo.Info.Add("Name", elem.Attribute(ReportUtil.Name).Value);
            runInfo.Info.Add("DLL Name", elem.Descendants(ReportUtil.TestSuite).First().Attribute(ReportUtil.Name).Value);

            return runInfo;
        }
    }
}
