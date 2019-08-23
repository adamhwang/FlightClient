using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using System.Security.Cryptography;

namespace FlightClient
{
    public partial class SHA : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            lblMess.Visible = false;
            if (!string.IsNullOrEmpty(tbContent.Text))
            {
                var encoding = Encoding.UTF8;

                if (ddlSHAType.SelectedValue.Equals("1"))
                {
                    
                    using (SHA1Managed hmac = new SHA1Managed())
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = hmac.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("256"))
                {
                    using (HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = hmac.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("512"))
                {
                    using (HMACSHA512 hmac = new HMACSHA512(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = hmac.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }
            }
            else
                lblMess.Visible = true;
        }
    }
}