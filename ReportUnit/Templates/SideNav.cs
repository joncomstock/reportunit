using ReportUnit.Utils;

namespace ReportUnit.Templates
{
    public class SideNav
    {
        public static string Link
        {
            get
            {
                return ReportUtil.FormatTemplate(@"<li class='waves-effect report-item'>
	                    <a href='./@(Model.FileName).html'>
		                    <i class='mdi-file-folder-open'></i>
		                    <span class='sidenav-filename'>@Model.FileName</span>
	                    </a>
                    </li>");
            }
        }

        public static string IndexLink
        {
            get {
                return ReportUtil.FormatTemplate(@"<li class='waves-effect report-item'>
	                    <a href='./Index.html'>
		                    <i class='mdi-action-dashboard'></i>
		                    <span class='sidenav-filename'>Index</span>
	                    </a>
                    </li>");
            }
        }
    }
}
