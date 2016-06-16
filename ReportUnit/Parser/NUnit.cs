using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Threading.Tasks;

using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

using ReportUnit.Model;
using ReportUnit.Utils;
using ReportUnit.Logging;

namespace ReportUnit.Parser
{
    internal class NUnit : Unit
    {
        public NUnit() { }

        public override RunInfo CreateRunInfo(XElement elem, Report report)
        {
            RunInfo runInfo = new RunInfo();
            runInfo.TestRunner = report.TestRunner;

            runInfo.Info.Add("Test Results File", resultsFile);
            
            if (elem == null) return runInfo;

            if (elem.Attribute("nunit-version") != null)
                runInfo.Info.Add("NUnit Version", elem.Attribute("nunit-version").Value);
            runInfo.Info.Add("CLR Version", elem.Attribute("clr-version").Value);
            runInfo.Info.Add("OS Version", elem.Attribute("os-version").Value);
            runInfo.Info.Add("Platform", elem.Attribute("platform").Value);
            runInfo.Info.Add("CWD", elem.Attribute("cwd").Value);
            runInfo.Info.Add("Machine Name", elem.Attribute("machine-name").Value);
            runInfo.Info.Add("User", elem.Attribute("user").Value);
            runInfo.Info.Add("User Domain", elem.Attribute("user-domain").Value);
            runInfo.Info.Add("Assembly Name", report.AssemblyName);

            return runInfo;
        }

        /// <summary>
        /// The assembly name will be different per XML, so retreive the appropriate one per type.
        /// </summary>
        /// <param name="elem">XElement to retrieve Assembly name from</param>
        /// <returns>The Assembly Name as a string</returns>
        public override string GetAssemblyName(XElement elem, TestRunner testRunner)
        {
            string assembly = "";

            if (elem != null)
            {
                var node = elem.Descendants(ReportUtil.GetTestRunnerNode(new Tuple<TestRunner, string>(testRunner, ReportUtil.TestSuite)))
                .FirstOrDefault(x => (x.Attribute(ReportUtil.Type) != null && x.Attribute("type").Value.Equals(ReportUtil.Assembly, StringComparison.CurrentCultureIgnoreCase)));                

                if (node != null && node.Attribute(ReportUtil.Name) != null)
                {
                    assembly = node.Attribute(ReportUtil.Name).Value;
                }
            }

            return assembly;
        }
    }
}
