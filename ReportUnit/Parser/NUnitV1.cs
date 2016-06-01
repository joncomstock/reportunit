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
        public NUnitV1(string resultsFile) { this.resultsFile = resultsFile; }
    }
}
