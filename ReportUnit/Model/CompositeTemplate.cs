using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;
using ReportUnit.Utils;

namespace ReportUnit.Model
{
    public class CompositeTemplate
    {
        private List<Report> _reportList;

        public void AddReport(Report report)
        {
            if (_reportList == null)
            {
                _reportList = new List<Report>();
            }

            _reportList.Add(report);

            SideNavLinks += Engine.Razor.RunCompile(Templates.SideNav.Link, "sidenav", typeof(Report), report, null);
        }

        public List<Report> ReportList
        {
            get
            {
                return _reportList;
            }
        }

        public string Title { get; internal set; }

        public string SideNavLinks { get; internal set; }

        public string SideNavHtml
        {
            get
            {
                var navClass = string.Format("side-nav fixed{0}", (ReportList.Count <= 1 ? " hide" : ""));
                var className = "";
                var toolTip = "";
                var href = "";
                var iClass = "";

                if (Title == ReportUnitService.SummaryTitle)
                {
                    className = "console-logs-icon";
                    toolTip = "Console Logs";
                    href = "modal2";
                    iClass = "assignment";
                }
                else
                {
                    className = "run-info-icon";
                    toolTip = "Run Info";
                    href = "modal1";
                    iClass = "info_outline";
                }
                var listItem = string.Format(@"
                    <li class='nav-item'>
                        <a class='modal-trigger waves-effect waves-light {0} tooltipped' data-position='bottom' data-tooltip='{1}' href='#{2}'><i class='material-icons'>{3}</i></a>
                    </li>
                ", className, toolTip, href, iClass);

                var headerHtml = "<header>";
                if (ReportList.Count <= 1)
                {
                    headerHtml = "<header class='single-report'>";
                }

                var sideNavHtml = ReportUtil.FormatTemplate(string.Format(@"
                    {2}
                        <nav class='top-nav fixed'>         
                            <div class='nav-wrapper'>
                                <a class='page-title'>
                                    @Model.Title
                                </a>
                                <ul class='right nav-right'>
                                    {0}
                                    <li class='nav-item'>
                                        v1.50.0
                                    </li>
                                </ul>
                            </div>                            
                        </nav>
                        <a href='#' data-activates='nav-mobile' class='button-collapse top-nav hide-on-large-only'>
                            <i class='material-icons'>menu</i>
                        </a>
                        <a href='#' data-activates='#' class='button-shrink top-nav hide-on-med-and-down' id='button-shrink'>
                            <i class='material-icons'>chevron_left</i>
                        </a>
                        <a href='#' data-activates='#' class='button-shrink top-nav hide-on-med-and-down hide' id='button-expand'>
                            <i class='material-icons'>chevron_right</i>
                        </a>
                        <ul id='nav-mobile' class='{1}'>
                            <li class='logo'>
                                <span class='brand-logo'>ReportUnit</span>
                            </li>
                            @Model.SideNavLinks
                        </ul>                                                                                                                                    
                    </header>", listItem, navClass, headerHtml));

                return sideNavHtml;
            }
        }        

        public string SideNav { get; internal set; }

        public string HeadHtml
        {
            get
            {
                var headHtml = ReportUtil.FormatTemplate(@"
	                <meta charset='utf-8'>
	                <meta http-equiv='X-UA-Compatible' content='IE=edge'>
	                <meta name='viewport' content='width=device-width, initial-scale=1'>
	                <meta name='description' content=''>
	                <meta name='author' content=''>
	                <title>ReportUnit TestRunner Report</title>
	                <link href='https://cdnjs.cloudflare.com/ajax/libs/materialize/0.97.2/css/materialize.min.css' rel='stylesheet' type='text/css'>
	                <link href='https://fonts.googleapis.com/css?family=Open+Sans:400,600' rel='stylesheet' type='text/css'>
                    <link href='https://fonts.googleapis.com/icon?family=Material+Icons' rel='stylesheet'>
	                <!--<link href='https://cdn.rawgit.com/reportunit/reportunit/005dcf934c5a53e60b9ec88a2a118930b433c453/cdn/reportunit.css' type='text/css' rel='stylesheet' />-->
                    <!--<link href='https://cdn.rawgit.com/joncomstock/reportunit/master/cdn/reportunit.css' type='text/css' rel='stylesheet' />-->
                    <link href='https://rawgit.com/joncomstock/reportunit/master/cdn/reportunit.css' type='text/css' rel='stylesheet' />
                    <!--<link href='reportunit.css' type='text/css' rel='stylesheet' />-->
                    ");

                return headHtml;
            }
        }

        public string ScriptFooterHtml
        {
            get {
                return ReportUtil.FormatTemplate(@"
                    <script src='https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js'></script> 
                    <script src='https://cdnjs.cloudflare.com/ajax/libs/materialize/0.97.2/js/materialize.min.js'></script> 
                    <script src='https://cdnjs.cloudflare.com/ajax/libs/Chart.js/1.0.2/Chart.min.js'></script>
                    <!--<script src='https://cdn.rawgit.com/reportunit/reportunit/35df38c6ab8b35526c22b920e24993ecc9357c2a/cdn/reportunit.js' type='text/javascript'></script>-->
                    <!--<script scr='https://cdn.rawgit.com/joncomstock/reportunit/master/cdn/reportunit.js' type='text/javascript'></script>-->
                    <script src='https://rawgit.com/joncomstock/reportunit/master/cdn/reportunit.js' type='text/javascript'></script>
                    <!--<script src='reportunit.js' type = 'text/javascript'></script>-->
                ");
            }
        }
    }
}
