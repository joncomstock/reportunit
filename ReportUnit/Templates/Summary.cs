using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportUnit.Model;
using ReportUnit.Utils;

namespace ReportUnit.Templates
{
    internal class Summary : CompositeTemplate
    {
        public static string GetSource()
        {
            return ReportUtil.FormatTemplate(@"
                <!DOCTYPE html>
                <html lang='en'>
                <!--
	                ReportUnit 1.5 | http://reportunit.relevantcodes.com/
	                Created by Anshoo Arora (Relevant Codes) | Released under the MIT license

	                Template from:

		                ExtentReports Library | http://relevantcodes.com/extentreports-for-selenium/ | https://github.com/anshooarora/
		                Copyright (c) 2015, Anshoo Arora (Relevant Codes) | Copyrights licensed under the New BSD License | http://opensource.org/licenses/BSD-3-Clause
		                Documentation: http://extentreports.relevantcodes.com 
                -->
                <head>
                    @Model.HeadHtml
                </head>
                <body class='summary'>    
	                @Model.SideNav
                    <main>
			            <!--<div class='row dashboard'>
				            <div class='col s12 m6 l4'>
					            <div class='card-panel'>
						            <div alt='Count of all passed tests' title='Count of all passed tests'>Suite Summary</div>    
						            <div class='chart-box'>
							            <canvas class='text-centered' id='suite-analysis'></canvas>
						            </div>
						            <div>
							            <span class='weight-light'><span class='suite-pass-count weight-normal'></span> suites(s) passed</span>
						            </div> 
						            <div>
							            <span class='weight-light'><span class='suite-fail-count weight-normal'></span> suites(s) failed, <span class='suite-others-count weight-normal'></span> others</span>
						            </div> 
					            </div>
				            </div>
				            <div class='col s12 m6 l4'>
					            <div class='card-panel'>
						            <div alt='Count of all failed tests' title='Count of all failed tests'>Tests Summary</div>
						            <div class='chart-box'>
							            <canvas class='text-centered' id='test-analysis'></canvas>
						            </div>
						            <div>
							            <span class='weight-light'><span class='test-pass-count weight-normal'></span> test(s) passed</span>
						            </div> 
						            <div>
							            <span class='weight-light'><span class='test-fail-count weight-normal'></span> test(s) failed, <span class='test-others-count weight-normal'></span> others</span>
						            </div> 
					            </div>
				            </div>
				            <div class='col s12 m12 l4'>
					            <div class='card-panel'>
						            <div alt='Count of all inconclusive tests' title='Count of all inconclusive tests'>Pass Percentage</div>
						            <div class='panel-lead pass-percentage'></div>
						            <div class='progress'>
							            <div class='determinate'></div>
						            </div>
					            </div>
				            </div>
			            </div>-->
			            <div class='row'>
                            <div class='col s12'>
                                <div class='card-panel'>
                                    <table class='table bordered'>
                                        <thead>
                                            <tr>
                                                <th>File</th>
                                                <th>TestRunner</th>
                                                <th class='right-align'>Total Tests</th>
                                                <th class='right-align'>Passed</th>
                                                <th class='right-align'>Failed</th>
                                                <th class='right-align'>Others</th>
                                                <th class='center-align'>Quick Summary</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            @{  
                                                double totalTests =  0;
                                                double totalPassed = 0;
                                                double totalFailed = 0;
                                                double totalOthers = 0;                                                
                                            }

                                            @for (int ix = 0; ix < Model.ReportList.Count; ix++)
                                            {
                                                <tr>
                                                    <td><a href='./@(Model.ReportList[ix].FileName).html'>@Model.ReportList[ix].FileName</a></td>
                                                    <td>@Model.ReportList[ix].TestRunner</td>
                                                    <td class='right-align'>@Model.ReportList[ix].Total</td>
                                                    <td class='right-align'>@Model.ReportList[ix].Passed</td>
                                                    <td class='right-align'>@Model.ReportList[ix].Failed</td>
                                                    <td class='right-align'>@(Model.ReportList[ix].Total - (Model.ReportList[ix].Passed + Model.ReportList[ix].Failed))</td>
                                                    <td class='center-align'>
                                                        @{var total = Model.ReportList[ix].Total;
                                                            var passed = Model.ReportList[ix].Passed;
                                                            var failed = Model.ReportList[ix].Failed;
                                                            var others = Model.ReportList[ix].Total - (Model.ReportList[ix].Passed + Model.ReportList[ix].Failed);

                                                            totalPassed += passed;
                                                            totalFailed += failed;
                                                            totalOthers += others;
                                                            totalTests += passed + failed + others;
                                                        }
                                                        @if (Model.ReportList[ix].Total != 0) 
                                                        {
                                                            <div class='progress2'>
                                                                @if (passed != 0)
                                                                {
	                                                                <div style='width: @((passed / total) * 100)%' class='progress-bar2 progress-bar-success progress-bar-striped'>
		                                                                <span class='sr-only'>@passed</span>
	                                                                </div> 
                                                                }
                                                                @if  (failed != 0)
                                                                {
	                                                                <div style='width: @((failed / total) * 100)%' class='progress-bar2 progress-bar-danger progress-bar-striped'>
		                                                                <span class='sr-only'>@failed</span>
	                                                                </div>
                                                                }
                                                                @if  (others != 0)
                                                                {
	                                                                <div style='width: @((others / total) * 100)%' class='progress-bar2 progress-bar-warning progress-bar-striped'>
		                                                                <span class='sr-only'>@others</span>
	                                                                </div>
                                                                }                                                                
                                                            </div>
                                                        }
                                                        else
                                                        {
                                                            <div class='progress2'>
	                                                            <div style='width: 0%' class='progress-bar2 progress-bar-success progress-bar-striped'>
		                                                            <span class='sr-only'>0</span>
	                                                            </div>
                                                            </div>
                                                        }
                                                    </td>
                                                </tr>
                                            }
                                            <tr>                                                
                                                <td colspan='2'><span class='weight-normal'>Totals</span></td>
                                                <td id='total-tests' class='right-align'>@totalTests</td>
                                                <td id='total-passed' class='right-align'>@totalPassed</td>
                                                <td id='total-failed' class='right-align'>@totalFailed</td>
                                                <td id='total-others' class='right-align'>@totalOthers</td>
                                                <td id='total-total' class='center-align'>                                                                                                    
                                                    @if (totalTests != 0) 
                                                        {
                                                            <div class='progress2'>
                                                                @if (totalPassed != 0)
                                                                {
	                                                                <div style='width: @((totalPassed / totalTests) * 100)%' class='progress-bar2 progress-bar-success progress-bar-striped'>
		                                                                <span class='sr-only'>@totalPassed</span>
	                                                                </div> 
                                                                }
                                                                @if  (totalFailed != 0)
                                                                {
	                                                                <div style='width: @((totalFailed / totalTests) * 100)%' class='progress-bar2 progress-bar-danger progress-bar-striped'>
		                                                                <span class='sr-only'>@totalFailed</span>
	                                                                </div>
                                                                }
                                                                @if  (totalOthers != 0)
                                                                {
	                                                                <div style='width: @((totalOthers / totalTests) * 100)%' class='progress-bar2 progress-bar-warning progress-bar-striped'>
		                                                                <span class='sr-only'>@totalOthers</span>
	                                                                </div>
                                                                }                                                                
                                                            </div>
                                                        }
                                                        else
                                                        {
                                                            <div class='progress2'>
	                                                            <div style='width: 0%' class='progress-bar2 progress-bar-success progress-bar-striped'>
		                                                            <span class='sr-only'>0</span>
	                                                            </div>
                                                            </div>
                                                        }
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
			            </div>
                    </main>
	                <div id='modal2' class='modal'>
		                <div class='modal-content'>
			                <h4>Console Log</h4>
		                </div>
		                <div class='modal-footer'>
			                <a href='#!' class='modal-action modal-close waves-effect waves-green btn-flat'>Close</a>
		                </div>
	                </div>
                </body>
                @Model.ScriptFooterHtml
                
            </html>
            ");
        }
    }
}
