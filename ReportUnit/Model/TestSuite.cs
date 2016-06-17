using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportUnit.Model
{
    public class TestSuite : ReportOutput
    {
        public TestSuite()
        {
            TestList = new List<Test>();
            this.Status = Status.Unknown;
        }        

        public List<Test> TestList { get; private set; }
    }
}
