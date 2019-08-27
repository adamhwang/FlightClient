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
        private string _content = string.Empty;
        private string _key = string.Empty;
        private string _encType = string.Empty;
        private bool _buttonClicked = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!Page.IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request["Content"]))
                    _content = Request["Content"].ToString();

                if (!string.IsNullOrEmpty(Request["Key"]))
                    _key = Request["Key"].ToString();

                if (!string.IsNullOrEmpty(Request["EncType"]))
                    _encType = Request["EncType"].ToString();

            }
            /*
            else
            {
                if (!string.IsNullOrEmpty(Request.Form["Content"]))
                    _content = Request.Form["Content"].ToString();

                if (!string.IsNullOrEmpty(Request.Form["Key"]))
                    _key = Request.Form["Key"].ToString();

                if (!string.IsNullOrEmpty(Request.Form["EncType"]))
                    _encType = Request.Form["EncType"].ToString();

            }
            */
            

            //Button1.Click += new EventHandler(this.Button1_Click);


            if (!string.IsNullOrEmpty(_content) && !string.IsNullOrEmpty(_encType) && !_buttonClicked)
            {
                tbContent.Text = _content;
                tbKey.Text = _key;

                if (ddlSHAType.Items.FindByText(_encType) != null)
                {
                    ddlSHAType.ClearSelection();
                    ddlSHAType.Items.FindByText(_encType).Selected = true;
                }

                doEncrypt();
            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            _buttonClicked = true;
            doEncrypt();
        }

        private void doEncrypt()
        {
            lblMess.Visible = false;
            if (!string.IsNullOrEmpty(tbContent.Text))
            {
                var encoding = Encoding.UTF8;

                if (ddlSHAType.SelectedValue.Equals("SHA1"))
                {
                    using (HMACSHA1 sha = new HMACSHA1())
                    //using (SHA1Managed sha = new SHA1Managed())
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("SHA256"))
                {
                    
                    using (HMACSHA256 sha = new HMACSHA256(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("SHA384"))
                {
                    using (HMACSHA384 sha = new HMACSHA384(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("SHA512"))
                {
                    
                    using (HMACSHA512 sha = new HMACSHA512(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
                    }
                }
            }
            else
                lblMess.Visible = true;
        }
    }
}