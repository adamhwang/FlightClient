using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using System.Security.Cryptography;
using System.IO;

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
            if (cbUseFile.Checked)
            {
                File2Content();
            }
            else if (!string.IsNullOrEmpty(tbContent.Text))
            {
                var encoding = Encoding.UTF8;
                //Encoding.GetEncoding("iso-8859-1")

                if (ddlSHAType.SelectedValue.Equals("SHA1"))
                {
                    switch (rblSHA1List.SelectedValue)
                    {
                        case "SHA1":
                            var msg1 = encoding.GetBytes(tbContent.Text);
                            var hash1 = SHA1.Create().ComputeHash(msg1);
                            tbRes.Text = BitConverter.ToString(hash1);
                            break;
                        case "HMACSHA1":
                            using (HMACSHA1 sha = new HMACSHA1())
                            {
                                var msg = encoding.GetBytes(tbContent.Text);
                                var hash = sha.ComputeHash(msg);

                                tbRes.Text = BitConverter.ToString(hash);
                            }

                        break;
                        case "SHA1Managed":
                            using (SHA1Managed sha = new SHA1Managed())
                            {
                                var msg = encoding.GetBytes(tbContent.Text);
                                var hash = sha.ComputeHash(msg);

                                tbRes.Text = BitConverter.ToString(hash);
                            }
                        break;
                        case "SHA1CryptoServiceProvider":
                        default:
                            using (var sha = new SHA1CryptoServiceProvider())
                            {
                                var msg = encoding.GetBytes(tbContent.Text);
                                var hash = sha.ComputeHash(msg);

                                tbRes.Text = BitConverter.ToString(hash);
                            }
                        break;
                    }
                    
                }

                if (ddlSHAType.SelectedValue.Equals("SHA256"))
                {
                    
                    using (SHA256Managed sha = new SHA256Managed())
                    //using (HMACSHA256 sha = new HMACSHA256(encoding.GetBytes(tbKey.Text)))
                    {
                        
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("SHA384"))
                {
                    using (HMACSHA384 sha = new HMACSHA384(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash);
                    }
                }

                if (ddlSHAType.SelectedValue.Equals("SHA512"))
                {
                    
                    using (HMACSHA512 sha = new HMACSHA512(encoding.GetBytes(tbKey.Text)))
                    {
                        var msg = encoding.GetBytes(tbContent.Text);
                        var hash = sha.ComputeHash(msg);

                        tbRes.Text = BitConverter.ToString(hash);
                    }
                }

                Hash2Lower();
            }
            else
                lblMess.Visible = true;
        }

        private void File2Content()
        {
            string fileLoc = HttpContext.Current.Server.MapPath("Scripts/ahktqsewxjhguuxe.js");
            StreamReader sr = new StreamReader(fileLoc);
            tbContent.Text = sr.ReadToEnd();
            sr.Close();

            FileStream fs = File.OpenRead(fileLoc);
            tbKey.Text = BitConverter.ToString(SHA1.Create().ComputeHash(fs)).ToLower().Replace("-", string.Empty);
            fs.Close();

            tbEnc.Text = Convert.ToBase64String(new ASCIIEncoding().GetBytes(tbContent.Text));


            //
            /*
            try
            {
                string SendString = "{\"key\":\"150bd9b2b0bdc67edf9dc212eca9b490\",\"method\":\"distil\",\"data\":{\"JsSha1\":\"" +
                    tbKey.Text + "\",\"JsUri\":\"https://www.aerlingus.com/ahktqsewxjhguuxe.js\",\"JsData\":\"" +
                    tbEnc.Text + "\"}}";

                System.Net.HttpWebRequest HttpReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://api.2captcha.com/in.php");
                HttpReq.Method = "POST";
                HttpReq.ContentType = "application/json";

                System.IO.StreamWriter sOut = new System.IO.StreamWriter(HttpReq.GetRequestStream());
                sOut.Write(SendString);
                sOut.Close();

                System.Net.HttpWebResponse HttpResp = (System.Net.HttpWebResponse)HttpReq.GetResponse();

                System.IO.StreamReader sReader = new System.IO.StreamReader(HttpResp.GetResponseStream());

                tbRes.Text = sReader.ReadToEnd().Replace("&amp;", "&");

                HttpResp.Close();
            }
            catch (Exception ex)
            {
                tbRes.Text = ex.Message;
            }
            */


        }

        private void Hash2Lower()
        {
            tbRes.Text = cbHash2Lower.Checked ? tbRes.Text.ToLower().Replace("-", string.Empty) : tbRes.Text;
        }

        protected void ddlSHAType_SelectedIndexChanged(object sender, EventArgs e)
        {
            rblSHA1List.Visible = ddlSHAType.SelectedValue.Equals("SHA1");
        }
    }
}