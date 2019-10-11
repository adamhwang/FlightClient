using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;

namespace FlightClient
{
    public partial class Eastern : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Convert.ToInt32(ddlAmount.SelectedValue); i++)
            {
                sb.Append(Guid.NewGuid());
                sb.Append(Environment.NewLine);
            }

            tbRes.Text = sb.ToString();
        }
    }
}