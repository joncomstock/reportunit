using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportUnit.Model
{
    public class Test : ReportOutput
    {
        /// <summary>
        /// Categories & features associated with the test
        /// </summary>
        public List<string> CategoryList;

        public string GetCategories()
        {
            if (CategoryList.Count == 0)
            {
                return "";
            }

            return string.Join(" ", CategoryList);
        }

        public Test()
        {
            CategoryList = new List<string>();
            Status = Status.Unknown;
        }
    }
}
