using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReportUnit.Model;
using System.Xml.Linq;

namespace ReportUnit.Utils
{
    internal class ReportUtil
    {
        // fixture level status codes
        public static Status GetFixtureStatus(IEnumerable<Test> tests)
        {
            return GetFixtureStatus(tests.Select(t => t.Status).ToList());
        }

        // fixture level status codes
        public static Status GetFixtureStatus(List<Status> statuses)
        {
            if (statuses.Any(x => x == Status.Failed)) return Status.Failed;
            if (statuses.Any(x => x == Status.Error)) return Status.Error;
            if (statuses.Any(x => x == Status.Inconclusive)) return Status.Inconclusive;
            if (statuses.Any(x => x == Status.Passed)) return Status.Passed;
            if (statuses.Any(x => x == Status.Skipped)) return Status.Skipped;

            return Status.Unknown;
        }

        /// <summary>
        /// Returns categories for the direct children or all descendents of an XElement
        /// </summary>
        /// <param name="elem">XElement to parse</param>
        /// <param name="allDescendents">If true, return all descendent categories.  If false, only direct children</param>
        /// <returns></returns>
        public static HashSet<string> GetCategories(XElement elem, bool allDescendents)
        {
            //Define which function to use
            var parser = allDescendents
                ? new Func<XElement, string, IEnumerable<XElement>>((e, s) => e.Descendants(s))
                : new Func<XElement, string, IEnumerable<XElement>>((e, s) => e.Elements(s));

            //Grab unique categories
            HashSet<string> categories = new HashSet<string>();
            bool hasCategories = parser(elem, "categories").Any();
            if (hasCategories)
            {
                List<XElement> cats = parser(elem, "categories").Elements("category").ToList();

                cats.ForEach(x =>
                {
                    string cat = x.Attribute("name").Value;
                    categories.Add(cat);
                });
            }

            return categories;
        }
    }
}
