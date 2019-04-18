using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlightClient
{
    
    public partial class FlyBeTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                tbKey.Text = System.Configuration.ConfigurationManager.AppSettings["FlyBe.EncryptionKey"];
                ENC_TIME.Text = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmmss");

                bool allOK = true;

                if (!string.IsNullOrEmpty(Request.QueryString["ADT"]))
                {
                    if (ddlADT.Items.FindByText(Request.QueryString["ADT"].ToString()) != null)
                    {
                        ddlADT.ClearSelection();
                        ddlADT.Items.FindByText(Request.QueryString["ADT"].ToString()).Selected = true;
                    }
                    else
                        allOK = false;
                }
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["CHD"]))
                {
                    if (ddlCHD.Items.FindByText(Request.QueryString["CHD"].ToString()) != null)
                    {
                        ddlCHD.ClearSelection();
                        ddlCHD.Items.FindByText(Request.QueryString["CHD"].ToString()).Selected = true;
                    }
                    else
                        allOK = false;
                }
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["INF"]))
                {
                    if (ddlINF.Items.FindByText(Request.QueryString["INF"].ToString()) != null)
                    {
                        ddlINF.ClearSelection();
                        ddlINF.Items.FindByText(Request.QueryString["INF"].ToString()).Selected = true;
                    }
                    else
                        allOK = false;
                }
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["DepDate"]))
                    DepDate.Text = Request.QueryString["DepDate"].ToString() + "0000";
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["ORI"]))
                    ORI.Text = Request.QueryString["ORI"].ToString();
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["DES"]))
                    DES.Text = Request.QueryString["DES"].ToString();
                else
                    allOK = false;

                //If we have all necessary items perform encryption 
                if (allOK)
                    ClientScript.RegisterStartupScript(typeof(string), "CreateStringAndEncrypt", "Go()", true);
            }
            
        }
    }
}