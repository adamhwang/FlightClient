using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlightClient
{
    public partial class JSONPathTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack) return;
        }

        protected void btmJSONPathCmd_Click(object sender, EventArgs e)
        {
            FlightClient.App_Backend.JSONPath JPATH = new App_Backend.JSONPath();
            JPATH.JSONStr = tbReq.Text;

            //JPATH.JSONPathCmd = tbCmd.Text;

            List<string> cmdList = new List<string>();

            if (!string.IsNullOrEmpty(tbCmd.Text))
            {
                string[] cmds = tbCmd.Text.Split('|');
                for (int i = 0; i < cmds.Length; i++)
                    cmdList.Add(cmds[i]);
            }

            JPATH.JSONPathCmd = cmdList.ToArray();
            tbRes.Text = JPATH.Result();

            //if (!string.IsNullOrEmpty(tbRes.Text))
            //    ClientScript.RegisterStartupScript(typeof(string), "JSONBeuatify", string.Format("beautifyJSON('{0}')", tbRes.ClientID), true);


        }
    }
}