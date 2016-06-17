using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportUnit.Model
{
    public class ReportOutput
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Status Status { get; set; }

        /// <summary>
        /// Error or other status messages
        /// </summary>
        public string StatusMessage { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string TotalTime { get; set; }

        /// <summary>
        /// How long the test fixture took to run (in milliseconds)
        /// </summary>
        public double Duration { get; set; }
    }
}
