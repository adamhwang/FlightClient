using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;


namespace FlightClient
{
    public partial class DeEncode : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnURLDecode_Click(object sender, EventArgs e)
        {
            tbRes.Text = HttpUtility.UrlDecode(tbReq.Text);
        }

        protected void btnURLEncode_Click(object sender, EventArgs e)
        {
            tbRes.Text = HttpUtility.UrlEncode(tbReq.Text);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            tbRes.Text = HttpUtility.HtmlDecode(tbReq.Text);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            tbRes.Text = HttpUtility.HtmlEncode(tbReq.Text);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                tbRes.Text = Encoding.UTF8.GetString(Convert.FromBase64String(tbReq.Text));
            }
            catch
            {
                tbRes.Text = "Unable to base64 decode";
            }
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            

            try
            {
                tbRes.Text = Convert.ToBase64String(new ASCIIEncoding().GetBytes(tbReq.Text));
            }
            catch
            {
                tbRes.Text = "Unable to base64 encode";
            }
        }
    }
}