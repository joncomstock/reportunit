using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

using ReportUnit.Logging;
using ReportUnit.Model;
using ReportUnit.Parser;

namespace ReportUnit
{
    public class ReportUnitService
    {
        private const string _ns = "ReportUnit.Parser";
        public const string SummaryTitle = "Summary";
        private Logger _logger = Logger.GetLogger();

        public ReportUnitService() { }

        public void CreateReport(string input, string outputDirectory)
        {
    		var attributes = File.GetAttributes(input);
		IEnumerable<FileInfo> filePathList;

        	 if ((FileAttributes.Directory & attributes) == FileAttributes.Directory)
        	{
				filePathList = new DirectoryInfo(input).GetFiles("*.xml", SearchOption.AllDirectories)
					.OrderByDescending(f => f.CreationTime);
	        }
	        else
	        {
				filePathList = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles(input);
	        }

        	InitializeRazor();

        	var compositeTemplate = new CompositeTemplate();
	
        	foreach (var filePath in filePathList)
        	{
            	var testRunner = GetTestRunner(filePath.FullName);

            	if (!(testRunner.Equals(TestRunner.Unknown)))
            	{
                    IParser parser = (IParser)Assembly.GetExecutingAssembly().CreateInstance(_ns + "." + Enum.GetName(typeof(TestRunner), testRunner));
                    var report = parser.Parse(filePath.FullName, testRunner);
                    compositeTemplate.AddReport(report);
                }
            }

            if (compositeTemplate.ReportList.Count > 1)
            {
                compositeTemplate.Title = SummaryTitle;
                compositeTemplate.SideNavLinks = compositeTemplate.SideNavLinks.Insert(0, Templates.SideNav.IndexLink);
                compositeTemplate.SideNav = Engine.Razor.RunCompile(compositeTemplate.SideNavHtml, "sidenavhtml", 
                    typeof(CompositeTemplate), compositeTemplate, null);

                string summary = Engine.Razor.RunCompile(Templates.Summary.GetSource(), "summary", 
                    typeof(CompositeTemplate), compositeTemplate, null);
                File.WriteAllText(Path.Combine(outputDirectory, "Index.html"), summary);
            }            

			foreach (var report in compositeTemplate.ReportList)
			{
                compositeTemplate.Title = report.FileName;
                report.SideNavLinks = compositeTemplate.SideNavLinks;
			    report.SideNav = Engine.Razor.RunCompile(compositeTemplate.SideNavHtml, "reportsidenav", 
                    typeof(CompositeTemplate), compositeTemplate, null);

			    report.Head = compositeTemplate.HeadHtml;
			    report.ScriptFooter = compositeTemplate.ScriptFooterHtml;

                var html = Engine.Razor.RunCompile(Templates.File.GetSource(compositeTemplate.ReportList), "report", typeof(Report), report, null);
                File.WriteAllText(Path.Combine(outputDirectory, report.FileName + ".html"), html);
            }
        }

        private TestRunner GetTestRunner(string inputFile)
        {
            var testRunner = new ParserFactory(inputFile).GetTestRunnerType();

            _logger.Info("The file " + inputFile + " contains " + Enum.GetName(typeof(TestRunner), testRunner) + " test results");

            return testRunner;
        }

        private void InitializeRazor()
        {
            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration();
            templateConfig.DisableTempFileLocking = true;
            templateConfig.EncodedStringFactory = new RawStringFactory();
            templateConfig.CachingProvider = new DefaultCachingProvider(x => { });
            var service = RazorEngineService.Create(templateConfig);
            Engine.Razor = service;
        }
    }
}
