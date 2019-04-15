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
        private string encKey = "5615AB59DFF60EC66A000F298BBCA8723AFF54ADEE053A8F6EB230A4D9FC9410";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                tbKey.Text = encKey;
                ENC_TIME.Text = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmmss");
                DepDate.Text = DateTime.Now.AddDays(21).ToString("yyyyMMdd0000");
            }
            
        }

        protected void btnEncr_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbReq.Text) && !string.IsNullOrEmpty(tbKey.Text))
            {
                tbRes.Text = hex2s(tbKey.Text);
                
            }
        }


        private string hex2s(string c)
        {
            var b = ""; if (c.IndexOf("0x") == 0 || c.IndexOf("0X") == 0)
            {
                c = c.Substring(2);
            }
            if (c.Length % 2 !=0)
            {
                c += "0";
            }
            for (var a = 0; a < c.Length; a += 2)
            {
                b += (char)(Convert.ToInt32(c.Substring(a, 2), 16));
            }
            return b;
        }


    }
}