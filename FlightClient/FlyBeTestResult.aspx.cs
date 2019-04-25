using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlightClient
{
    public partial class FlyBeTestResult : System.Web.UI.Page
    {
        public string ENC = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!string.IsNullOrEmpty(Request.Form["tbRes"]))
                ENC = Request.Form["tbRes"].ToString();

        }
    }
}