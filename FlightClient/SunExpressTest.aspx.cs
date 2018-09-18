using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections.Specialized;

namespace FlightClient
{
    public partial class SunExpressTest : System.Web.UI.Page
    {

        private EAScrape.ScrapeInfo _scrapeInfo = null;
        private EAScrape.iFou _foundInfo = null;
        private EAScrape.WebPage _webPage = null;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn1_Click(object sender, EventArgs e)
        {
            InitScrape(false);

            _scrapeInfo.InsertVariable("DepDate_Out", date1.Text);
            _scrapeInfo.InsertVariable("DepDate_Ret", date2.Text);
            _scrapeInfo.InsertVariable("FlightContent", tb1.Text);

            DateTime d1 = DateTime.Now;

            switch (ddl1.Text)
            {
                case "SF":

                    if (!string.IsNullOrEmpty(tb1.Text))
                    {
                        SunExpress.SunExpressMain sm = new SunExpress.SunExpressMain();
                        NameValueCollection poScrapeData = new NameValueCollection();
                        poScrapeData.Add("action", "GetFlights");

                        tb2.Text = sm.GoNext(poScrapeData, ref _scrapeInfo, ref _foundInfo, _webPage);
                    }
                    break;
                case "SF RR":

                    if (!string.IsNullOrEmpty(tb1.Text) && !string.IsNullOrEmpty(ORI.Text) && !string.IsNullOrEmpty(DES.Text))
                    {
                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME", ORI.Text);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME", DES.Text);

                        SunExpress.SunExpressMain sm = new SunExpress.SunExpressMain();
                        NameValueCollection poScrapeData = new NameValueCollection();
                        poScrapeData.Add("action", "GetRRCombis");

                        tb2.Text = sm.GoNext(poScrapeData, ref _scrapeInfo, ref _foundInfo, _webPage);
                    }
                    break;
            }

            DateTime d2 = DateTime.Now;

            TimeSpan diff = d2 - d1;

            lb1.Text = string.Format("Time to process (ms): {0}", diff.Milliseconds);

        }



        private void InitScrape()
        {
            InitScrape(true);
        }

        private void InitScrape(bool ResetFlightTable)
        {
            _scrapeInfo = new EAScrape.ScrapeInfo();


            //-- init _scrapeInfo, simulate pax
            _scrapeInfo.InsertVariable("REQ_NUM_ADULT", "2", true);
            _scrapeInfo.InsertVariable("REQ_NUM_CHILD", "1", true);
            _scrapeInfo.InsertVariable("REQ_NUM_BABY", "1", true);

            _scrapeInfo.InsertVariable("SCR_CURRENT_DAY", DateTime.Now.Day.ToString("00"));
            _scrapeInfo.InsertVariable("SCR_CURRENT_MONTH", DateTime.Now.Month.ToString("00"));
            _scrapeInfo.InsertVariable("SCR_CURRENT_YEAR", DateTime.Now.Year.ToString());

            _scrapeInfo.oReq = new EAScrape.Request();
            //_foundInfo = new EAScrape.iFou(true);
            _webPage = new EAScrape.WebPage();

            _webPage.WEB_TIMEOUT = 7500;

            _scrapeInfo.PaxCount("");
            _scrapeInfo.SimulatePax();

            _scrapeInfo.SCR_FOU_REMOVE = new System.Collections.ArrayList();


            //Set some dummy data
            _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_HOLDER", "Abe Testa", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_STREET_LINE1", "Test Lane 1", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_TOWN", "Test Town", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_ZIP", "1234AA", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_COUNTRY", "NL", true);

            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_PHONE_AREA", "495", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_PHONE_NUMBER", "577777", true);

            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_MOBILE_AREA", "6", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_MOBILE_NUMBER", "12345678", true);

            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_MAIL_ADDRESS", "123@123.nl", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_MOBILE", "31612345678", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CONTACT_PHONE", "31123456789", true);

            _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_MONTH", "12", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_YEAR", "16", true);

            string optBagKG = "15";

            //Us Pax 211
            _scrapeInfo.InsertVariable("REQ_BOO_AC_BAG1", "0", true);
            _scrapeInfo.InsertVariable("REQ_BOO_AC_BAG2", "0", true);
            _scrapeInfo.InsertVariable("REQ_BOO_AC_BAG3", "0", true);

            _scrapeInfo.InsertVariable("REQ_BOO_ADULT_BAG1", "0", true);
            _scrapeInfo.InsertVariable("REQ_BOO_ADULT_BAG2", "0", true);
            _scrapeInfo.InsertVariable("REQ_BOO_CHILD_BAG1", "0", true);

            _scrapeInfo.InsertVariable("REQ_BOO_ADULT_KG1", optBagKG, true);
            _scrapeInfo.InsertVariable("REQ_BOO_ADULT_KG2", optBagKG, true);
            _scrapeInfo.InsertVariable("REQ_BOO_CHILD_KG1", optBagKG, true);


            //if (ddlChildren.SelectedIndex > 0 && cbUseChildAges.Checked)
            //{
            //    for (int c = 1; c <= ddlChildren.SelectedIndex; c++)
            //    {
            //        int childAge = 10 - c;

            //        _scrapeInfo.InsertVariable("REQ_BOO_CHILD_AGE_YEARS" + c.ToString(), childAge.ToString(), true);
            //    }

            //}
        }
    }
}