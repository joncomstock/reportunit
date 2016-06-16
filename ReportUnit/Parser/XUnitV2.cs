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
        /// <summary>
        /// Pass in an XElement that contains all of the Run Info information.
        /// </summary>
        /// <param name="elem">Element to obtain Run Info information from</param>
        /// <param name="report"></param>
        /// <returns>RunInfo for the file</returns>
        public override RunInfo CreateRunInfo(XElement elem, Report report)
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
            runInfo.Info.Add("Test Framework", elem.Attribute("test-framework").Value);
            runInfo.Info.Add("Run Date", elem.Attribute("run-date").Value);
            runInfo.Info.Add("Run Time", elem.Attribute("run-time").Value);
            runInfo.Info.Add("Config File", elem.Attribute("config-file").Value);

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
                var node = elem.Descendants(ReportUtil.Assembly).FirstOrDefault();

                if (node != null && node.Attribute(ReportUtil.Name) != null)
                {
                    assembly = node.Attribute(ReportUtil.Name).Value;
                }
            }

            return assembly;
        }

        public XUnitV2() { }
    }
}
