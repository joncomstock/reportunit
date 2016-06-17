using ReportUnit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportUnit.Model;

namespace ReportUnit.Templates
{
    internal class File : CompositeTemplate
    {
        public static string GetSource(List<Report> reportList)
        {
            var mainHtml = "<main>";

            if (reportList.Count <= 1)
            {
                mainHtml = "<main class='single-report'>";
            }

            return ReportUtil.FormatTemplate(string.Format(@"
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
                    @Model.Head
                </head>
                <body>
                    @Model.SideNav
                    {0}
                        @if (Model.Total == 0)
                        {{
                            <div class='row no-tests'>
                                <div clas='col s12 m6 l4'>
                                    <div class='no-tests-message card-panel'>
                                        <p>
                                            No tests were found in @Model.FileName.
                                        </p>
                                        @if (!String.IsNullOrEmpty(@Model.StatusMessage))
                                        {{
                                            <pre>
                                                @Model.StatusMessage
                                            </pre>
                                        }}
                                    </div>
                                </div>
                            </div>
                        }}
                        else
                        {{
                            <div class='row dashboard'>
                                <div class='section'>
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
                                                <span class='weight-light'><span class='test-pass-count weight-normal'>@Model.Passed</span> test(s) passed</span>
                                            </div> 
                                            <div>
                                                <span class='weight-light'><span class='test-fail-count weight-normal'>@Model.Failed</span> test(s) failed, <span class='test-others-count weight-normal'>@(Model.Total - (Model.Passed + Model.Failed))</span> others</span>
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
                                </div>
                            </div>
                            <div class='row'>
                                <div id='suites' class='suites' data-total='@Model.Total' data-passed='@Model.Passed' data-failed='@Model.Failed' data-inconclusive='@Model.Inconclusive' data-errors='@Model.Errors' data-skipped='@Model.Skipped' >
                                    <div class='col s12 m4 l6'>
                                        <div class='card-panel suite-list'>
                                            <div class='section filters'>
                                                <div>
                                                    <a class='dropdown-button button' href='#' data-beloworigin='true' data-constrainwidth='true' data-activates='suite-toggle' alt='Filter suites' title='Filter suites'>
                                                        <i class='mdi-file-folder-open icon'>
                                                        </i>
                                                    </a>
                                                    <ul class='dropdown-content filter-dropdown' id='suite-toggle' data-filter='suite' data-filter-display='Suite: '> 
                                                    <ul>
                                                        @foreach (var status in Model.StatusList.Distinct().ToList())
                                                        {{
                                                            <li class='@status.ToString() filter-suites'><a href='#!'>@status.ToString()</a></li>
                                                        }}
                                                        <li class='divider'></li> 
                                                        <li class='clear'><a href='#!'>Clear Filters</a></li> 
                                                    </ul>
                                                </div> 
                                                <div>
                                                    <a class='dropdown-button button' href='#' data-beloworigin='true' data-constrainwidth='true' data-activates='tests-toggle' alt='Filter tests' title='Filter tests'>
                                                        <i class='mdi-action-subject icon'></i>
                                                    </a>
                                                    <ul class='dropdown-content filter-dropdown' id='tests-toggle' data-filter='test' data-filter-display='Tests: '> 
                                                    <ul>
                                                        @foreach (var status in Model.StatusList.Distinct().ToList())
                                                        {{
                                                            <li class='@status.ToString() filter-tests'><a href='#!'>@status.ToString()</a></li>
                                                        }}
                                                        <li class='divider'></li> 
                                                        <li class='clear'><a href='#!'>Clear Filters</a></li> 
                                                    </ul>
                                                </div> 
                                                @if (Model.CategoryList.Count > 0) 
                                                {{
                                                    <div> 
                                                        <a class='category-toggle dropdown-button button' href='#' data-beloworigin='true' data-constrainwidth='false' data-activates='category-toggle' alt='Filter categories' title='Filter categories'>
                                                            <i class='mdi-image-style icon'></i>
                                                        </a>
                                                        <ul class='dropdown-content filter-dropdown' id='category-toggle' data-filter='category' data-filter-display='Category: '>
                                                        <ul>
                                                            @foreach (var cat in Model.CategoryList.Distinct().ToList())
                                                            {{
                                                                <li class='@cat filter-categories'><a href='#!'>@cat</a></li>
                                                            }}
                                                            <li class='divider'></li> 
                                                            <li class='clear'><a href='#!'>Clear Filters</a></li> 
                                                        </ul> 
                                                    </div> 
                                                }}
                                                <div>
                                                    <a title='Clear Filters' alt='Clear Filters' id='clear-filters' class='clear'>
                                                        <i class='mdi-navigation-close icon'></i>
                                                    </a> 
                                                </div> &nbsp;
                                                <div> 
                                                    <a title='Enable Dashboard' alt='Enable Dashboard' id='enableDashboard' class='enabled'>
                                                        <i class='mdi-action-track-changes icon active'></i>
                                                    </a> 
                                                </div>
                                            </div>
                                            <div id='filters-applied' class='section hide'>
                                                Filter(s) Applied:                                         
                                            </div>
                                            <table id='suite-collection'>
                                                <thead>
                                                    <tr>
                                                        <th data-field='name'>Suite Name</th>
                                                        <th class='no-break' data-field='result'>Result</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    @for (var ix = 0; ix < Model.TestSuiteList.Count; ix++)
                                                    {{
                                                        <tr class='suite @Model.TestSuiteList[ix].Status.ToString().ToLower()'>
                                                            <td class='suite-name'>
                                                                <span>
                                                                    @Model.TestSuiteList[ix].Name
                                                                </span>
                                                            </td>
                                                            <td class='suite-result @Model.TestSuiteList[ix].Status.ToString().ToLower() no-break'>
                                                                <span class='label @Model.TestSuiteList[ix].Status.ToString().ToLower()'>
                                                                    @Model.TestSuiteList[ix].Status.ToString()
                                                                </span>
                                                            </td>
                                                            <td>
                                                                <i class='material-icons icon'>chevron_right</i>
                                                            </td>
                                                            <td class='suite-content hide'>
                                                                <div>
                                                                    @if (!String.IsNullOrEmpty(@Model.TestSuiteList[ix].StartTime))
                                                                    {{
                                                                        <span>Suite Start Time: </span>
                                                                        <span alt='Suite started at time' title='Suite started at time' class='startedAt label green lighten-2 text-white'>@Model.TestSuiteList[ix].StartTime</span>
                                                                    }}                                                                    
                                                                    @if (!String.IsNullOrEmpty(@Model.TestSuiteList[ix].EndTime))
                                                                    {{
                                                                        <span>Suite End Time: </span>
                                                                        <span alt='Suite ended at time' title='Suite ended at time' class='endedAt label red lighten-2 text-white'>@Model.TestSuiteList[ix].EndTime</span>
                                                                    }}
                                                                    @if (!String.IsNullOrEmpty(@Model.TestSuiteList[ix].TotalTime))
                                                                    {{
                                                                        <span>Suite Total Time: </span>
                                                                        <span alt='Suite total time' title='Suite total time' class='totalTime label blue lighten-2 text-white'>@Model.TestSuiteList[ix].TotalTime</span>
                                                                    }}
                                                                </div>
                                                                <div class='fixture-status-message'>
                                                                    @if (!String.IsNullOrEmpty(@Model.TestSuiteList[ix].Description)) 
                                                                    {{
                                                                        <div class='suite-desc'>@Model.TestSuiteList[ix].Description</div>
                                                                    }}
                                                                    @if (!String.IsNullOrEmpty(@Model.TestSuiteList[ix].StatusMessage)) 
                                                                    {{
                                                                        <div class='suite-desc'>@Model.TestSuiteList[ix].StatusMessage</div>
                                                                    }}
                                                                </div>
                                                                <table class='bordered'>
                                                                    <thead>
                                                                        <tr>
                                                                            <th>Test Name</th>
                                                                            <th class='no-break center'>Status</th>
                                                                            @if (Model.TestSuiteList.Count > 0 && Model.TestSuiteList[ix].TestList.Any(x => x.CategoryList.Count > 0))
                                                                            {{
                                                                                <th class='no-break center'>Category</th>
                                                                            }}
                                                                            @if (Model.TestSuiteList.Count > 0 && Model.TestSuiteList[ix].TestList.Where(x => !String.IsNullOrEmpty(x.Description) || !String.IsNullOrEmpty(x.StatusMessage)).Count() > 0) 
                                                                            {{
                                                                                <th class='no-break center'>Status <br /> Message</th>
                                                                            }}
                                                                            <th class='no-break center'>Time</th>
                                                                        </tr>
                                                                    </thead>
                                                                    <tbody>
                                                                        @foreach (var test in Model.TestSuiteList[ix].TestList)
                                                                        {{
                                                                            <tr class='@test.Status.ToString().ToLower() test-status'>
                                                                                <td class='test-name'>
                                                                                    @{{var testName = test.Name.Replace(""<"", ""&lt;"").Replace("">"", ""&gt;"");}}
                                                                                    @if (!String.IsNullOrEmpty(@test.Description))
                                                                                    {{
                                                                                        <a class='showDescription name' href='#'>@testName</a>
                                                                                        <p class='hide description'>@test.Description</p>
                                                                                    }}
                                                                                    else
                                                                                    {{
                                                                                        <span class='name'>@testName</span>
                                                                                    }}
                                                                                </td>
                                                                                <td class='test-status no-break center @test.Status.ToString().ToLower()'>
                                                                                    <span class='label @test.Status.ToString().ToLower()'>@test.Status.ToString()</span>
                                                                                </td>
                                                                                @if (Model.TestSuiteList.Count > 0 && Model.TestSuiteList[ix].TestList.Any(x => x.CategoryList.Count > 0))
                                                                                {{
                                                                                    <td>
                                                                                        @if (test.CategoryList.Count > 0)
                                                                                        {{
                                                                                            <div class='category-list center'>
                                                                                                @foreach (var cat in test.CategoryList)
                                                                                                {{
                                                                                                    <div class='chip no-break @cat filter-categories' data-filter='category' data-filter-display='Category: '>
                                                                                                        @cat
                                                                                                    </div>
                                                                                                    <br />
                                                                                                }}
                                                                                            </div>
                                                                                        }}
                                                                                    </td>
                                                                                }}
                                                                                @if (Model.TestSuiteList.Count > 0 && Model.TestSuiteList[ix].TestList.Where(x => !String.IsNullOrEmpty(x.StatusMessage)).Count() > 0) 
                                                                                {{
                                                                                    if (!String.IsNullOrEmpty(@test.StatusMessage)) 
                                                                                    {{
                                                                                        <td class='center'>
                                                                                            <div class='badge center showStatusMessage error modal-trigger'><i class='mdi-alert-warning'></i></div>
                                                                                            <pre class='hide'>@test.StatusMessage.Replace(""<"", ""&lt;"").Replace("">"", ""&gt;"")</pre>
                                                                                        </td>
                                                                                    }}
                                                                                    else 
                                                                                    {{
                                                                                        <td class=''></td>
                                                                                    }}
                                                                                }}
                                                                                <td class='center'>
                                                                                    @if(!String.IsNullOrEmpty(test.TotalTime))
                                                                                    {{
                                                                                        <span alt='Test total time' title='Test total time' class='label blue lighten-2 text-white'>
                                                                                            @test.TotalTime
                                                                                        </span>
                                                                                    }}
                                                                                    else
                                                                                    {{
                                                                                        <span>--</span>
                                                                                    }}
                                                                                </td>
                                                                                <td class='test-features filter-categories hide @test.GetCategories() @test.Status.ToString().ToLower()'></td>
                                                                            </tr>
                                                                        }}
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    }}
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>                                    
                                    <div class='col s12 m8 l6'>
                                        <div class='card-panel suite-details'>
                                            <h5 class='suite-name-displayed truncate'></h5>
                                            <div class='details-container'></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        }}
                        <div id='modal1' class='modal modal-fixed-footer'>
                            <div class='modal-content'>
                                <h4><!--%FILENAME%--> Run Info</h4>
                                <table class='bordered responsive-table'>
                                    <thead>
                                        <tr>
                                            <th class='no-break'>Param</th>
                                            <th>Value</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>    
                                            <td class='no-break'>TestRunner</td>
                                            <td>@Model.TestRunner.ToString()</td>
                                        </tr>
                                        @if (Model.RunInfo != null)
                                        {{
                                            foreach (var key in Model.RunInfo.Keys)
                                            {{
                                                <tr>
                                                    <td class='no-break'>@key</td>
                                                    <td>@Model.RunInfo[key]</td>
                                                </tr>
                                            }}
                                        }}
                                    </tbody>
                                </table>
                            </div>
                            <div class='modal-footer'>
                                <a href='#!' class='modal-action modal-close waves-effect waves-green btn-flat'>Close</a>
                            </div>
                            <div class='hidden total-tests'><!--%TOTALTESTS%--></div>
                            <div class='hidden total-passed'><!--%PASSED%--></div>
                            <div class='hidden total-failed'><!--%FAILED%--></div>
                            <div class='hidden total-inconclusive'><!--%INCONCLUSIVE%--></div>
                            <div class='hidden total-errors'><!--%ERRORS%--></div>
                            <div class='hidden total-skipped'><!--%SKIPPED%--></div>
                        </div>
                        <div id='dynamicModal' class='modal modal-fixed-footer'>
                            <div class='modal-content'>
                                <h4></h4>
                                <pre></pre>
                            </div>
                            <div class='modal-footer'>
                                <a href='#!' class='modal-action modal-close waves-effect waves-green btn-flat'>Close</a>                                                                    
                            </div>
                        </div>
                    </main>
                </body>
                @Model.ScriptFooter

            </html>
            ", mainHtml));
        }
    }
}
