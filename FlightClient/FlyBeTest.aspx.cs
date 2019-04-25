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
                //ENC_TIME.Text = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmmss");

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
                    DepDateOut.Text = Request.QueryString["DepDate"].ToString() + "0000";
                else
                    if (!string.IsNullOrEmpty(Request.QueryString["DepDateOut"]))
                    DepDateOut.Text = Request.QueryString["DepDateOut"].ToString() + "0000";
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["DepDateRet"]))
                    DepDateRet.Text = Request.QueryString["DepDateRet"].ToString() + "0000";
                

                if (!string.IsNullOrEmpty(Request.QueryString["ORI"]))
                    ORI.Text = Request.QueryString["ORI"].ToString();
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["DES"]))
                    DES.Text = Request.QueryString["DES"].ToString();
                else
                    allOK = false;

                if (!string.IsNullOrEmpty(Request.QueryString["TripType"]))
                {
                    if (ddlTripType.Items.FindByText(Request.QueryString["TripType"].ToString()) != null)
                    {
                        ddlTripType.ClearSelection();
                        ddlTripType.Items.FindByText(Request.QueryString["TripType"].ToString()).Selected = true;
                    }
                }

                //If we have all necessary items perform encryption 
                if (allOK)
                    ClientScript.RegisterStartupScript(typeof(string), "CreateStringAndEncrypt", "Go()", true);
            }
            //else
            //{
            //    if (!string.IsNullOrEmpty(Request.Form["tbRes"]))
            //    {
            //        string URL = Request.Url.AbsoluteUri;
            //        string[] urlItems = URL.Split('/');
            //        string endUrl = string.Empty;
            //        for (int i = 0; i < urlItems.Length - 1; i++)
            //            endUrl += urlItems[i] + "/";
            //        endUrl += "FlyBeTestResult.aspx";
            //        //System.Net.HttpWebRequest HttpReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(endUrl);
            //        //HttpReq.Method = "POST";

            //        //System.IO.StreamWriter sOut = new System.IO.StreamWriter(HttpReq.GetRequestStream());
            //        //sOut.Write("ENC=" + Request.Form["tbRes"].ToString());
            //        //sOut.Close();

            //        endUrl += "?ENC=" + Request.Form["tbRes"].ToString();

            //        Response.Redirect(endUrl);
            //    }
            //}

        }
    }
}