using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;
using System.Text;
using Tesseract;


namespace FlightClient
{
    public partial class ImageTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            phImg.Visible = false;
            if (!Page.IsPostBack)
            {
                if (File.Exists(Server.MapPath("Images/tmp.png")))
                    File.Delete(Server.MapPath("Images/tmp.png"));
                return;
            }
        }

        protected void btnCheckData_Click(object sender, EventArgs e)
        {
            string rndStr = RandomString(10);
            string imgURL = string.Format("Images/tmp_{0}.png", rndStr);

            

            if (!string.IsNullOrEmpty(txtBinData.Text))
            {
                if (!cbIsImgURL.Checked)
                {
                    string strPath = Server.MapPath(imgURL);
                    MemoryStream stream = new MemoryStream(Convert.FromBase64String(txtBinData.Text));
                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    img.Save(strPath);
                }
                else
                    imgURL = txtBinData.Text;
            }
            phImg.Visible = File.Exists(Server.MapPath(imgURL));

            string imgText = string.Empty;

            if (File.Exists(Server.MapPath(imgURL)))
            {
                imgPNG.ImageUrl = imgURL;

                using (var engine = new TesseractEngine(Server.MapPath("tessdata"), "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(Server.MapPath(imgURL)))
                    {
                        using (var page = engine.Process(img))
                        {
                            imgText = page.GetText();
                        }
                    }
                }
            }
            tbImgTxt.Text = imgText;



        }

        private static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}