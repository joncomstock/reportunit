using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Text;

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
                var sideNavHtml = @"
                    <header>
                        <nav class='top-nav'>         
                            <div class='nav-wrapper'>
                                <a class='page-title'>
                                    @Model.Title
                                </a>
                                <ul class='right nav-right'>
                                    @if(Model.Title == ReportUnit.ReportUnitService.SummaryTitle)
                                    {
                                        <li class='nav-item'>
                                            <a class='modal-trigger waves-effect waves-light console-logs-icon tooltipped' data-position='bottom' data-tooltip='Console Logs' href='#modal2'><i class='mdi-action-assignment'></i></a>
                                        </li>
                                    }
                                    else{
                                        <li class='nav-item'>
                                            <a class='modal-trigger waves-effect waves-light run-info-icon tooltipped' data-position='bottom' data-tooltip='Run Info' href='#modal1'><i class='mdi-action-info-outline'></i></a>
                                        </li>
                                    }
                                    <li class='nav-item'>
                                        v1.50.0
                                    </li>
                                </ul>
                            </div>                            
                        </nav>
                        <a href='#' data-activates='nav-mobile' class='button-collapse top-nav hide-on-large-only'>
                            <i class='material-icons'>menu</i>
                        </a>
                        <ul id='nav-mobile' class='side-nav fixed'>
                            <li class='logo'>
                                <a id='logo-container' href='#' class='brand-logo'><span>ReportUnit</span></a>
                            </li>
                            @Model.SideNavLinks
                        </ul>                                                       
                    </header>".Replace("\r\n", "").Replace("\t", "").Replace("    ", "");

                return sideNavHtml;
            }
        }

        public string SideNav { get; internal set; }
    }
}
