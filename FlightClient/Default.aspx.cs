using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Xml;
using System.Text;
using System.Data;




namespace FlightClient
{
    public partial class Default : System.Web.UI.Page
    {
        private EAScrape.ScrapeInfo _scrapeInfo = null;
        private EAScrape.iFou _foundInfo = null;
        private EAScrape.WebPage _webPage = null;

        DateTime _timeBeforeRQ = DateTime.MinValue;
        DateTime _timeAfterRQ = DateTime.MinValue;

        protected string _timelapse = string.Empty;

        public string qstring = "?A=1&C=0&I=0";
        protected void Page_Load(object sender, EventArgs e)
        {
            qstring = "?A=" + ddlAdults.SelectedValue + "&C=" + ddlChildren.SelectedValue + "&I=" + ddlBabies.SelectedValue + "&lowfare=";

            qstring += cbLowFare.Checked.ToString().ToLower();

            ClientScript.RegisterStartupScript(typeof(string), "copyValues", "copyValues();", true);

            qstring += string.Format("&Out={0}&Ret={1}", Server.UrlEncode(hOutwardSelected.Value), Server.UrlEncode(hReturnSelected.Value));

            tbDepart.Attributes.Add("onkeyup", "javascript:checkDepart(this.value);");
            tbArrive.Attributes.Add("onkeyup", "javascript:checkArrive(this.value);");

            if (Page.IsPostBack) return;
            Session["AvailResponse"] = null;
            Session["SelectedFlightResponse"] = null;

            GetRoutes();
            initPage();

            ddlValidYear.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                ddlValidYear.Items.Add(new ListItem(DateTime.Today.AddYears(i).ToString("yyyy")));
            }
            if (ddlValidYear.Items.FindByText(DateTime.Today.ToString("yyyy")) != null)
            {
                ddlValidYear.ClearSelection();
                ddlValidYear.Items.FindByText(DateTime.Today.ToString("yyyy")).Selected = true;
            }

            if (ddlValidMonth.Items.FindByText(DateTime.Today.ToString("MM")) != null)
            {
                ddlValidMonth.ClearSelection();
                ddlValidMonth.Items.FindByText(DateTime.Today.ToString("MM")).Selected = true;
            }
        }

        private void GetRoutes()
        {
            ddlDepart.Items.Add(new ListItem("Selecteer", "-1"));


            ddlArrive.Items.Add(new ListItem("Selecteer", "-1"));


            string loc = Server.MapPath("XML/Airports.xml");
            XmlDocument xAirports = new XmlDocument();
            xAirports.Load(loc);

            foreach (XmlNode airport in xAirports.SelectNodes("Airports/Airport"))
            {
                string iata = FlightClient.XML.GetNode(airport, "", "IATA");
                string name = FlightClient.XML.GetNode(airport, "", "Name");

                ListItem li1 = new ListItem(string.Format("{0} - {1}", iata, name), iata);
                ListItem li2 = new ListItem(string.Format("{0} - {1}", iata, name), iata);

                ddlDepart.Items.Add(li1);
                ddlArrive.Items.Add(li2);

            }

        }

        public static DateTime GetRandomDate()
        {
            DateTime dateFrom = DateTime.Today.AddDays(1);
            DateTime dateTo = DateTime.Today.AddMonths(6);

            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));

            TimeSpan range = new TimeSpan(dateTo.Ticks - dateFrom.Ticks);
            return dateFrom + new TimeSpan((long)(range.Ticks * rndNum.NextDouble()));

        }

        private static int RandNumber(int Low, int High)
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber));

            int rnd = rndNum.Next(Low, High);

            return rnd;
        }

        protected void cbMultiple_CheckedChanged(object sender, EventArgs e)
        {
            //phMultiple.Visible = cbMultiple.Checked;
        }

        protected void calDepart_SelectionChanged(object sender, EventArgs e)
        {
            calReturn.VisibleDate = calDepart.SelectedDate.AddDays(1);
            calReturn.SelectedDate = calDepart.SelectedDate.AddDays(1);
        }

        protected void Calendar1_DayRender(object sender, DayRenderEventArgs e)
        {
            e.Day.IsSelectable = e.Day.Date > DateTime.Today;

            if (e.Day.Date.ToString("yyyyMMdd").Equals(calDepart.SelectedDate.ToString("yyyyMMdd")))
            {
                e.Cell.BackColor = System.Drawing.Color.Gray;
                e.Cell.ForeColor = System.Drawing.Color.White;
            }
        }

        protected void Calendar2_DayRender(object sender, DayRenderEventArgs e)
        {
            bool isSelectable = e.Day.Date > DateTime.Today;

            if (calDepart.SelectedDate != DateTime.MinValue)
            {
                isSelectable = e.Day.Date > calDepart.SelectedDate;
            }

            e.Day.IsSelectable = isSelectable;

            if (e.Day.Date.ToString("yyyyMMdd").Equals(calReturn.SelectedDate.ToString("yyyyMMdd")))
            {
                e.Cell.BackColor = System.Drawing.Color.Gray;
                e.Cell.ForeColor = System.Drawing.Color.White;
            }

        }

        protected void ddlCard_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlCardNumbers.Items.Clear();

            string cardNrText = ddlCard.SelectedIndex > 0 ? "Select a cc number" : "Select a creditcard";

            if (ddlCard.SelectedIndex > 0)
            {
                InitScrape(false);

                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_CARD_NAME", ddlCard.SelectedValue, true);

                switch (ddlCard.SelectedItem.Text)
                {
                    case "AMEX":
                        ddlCardNumbers.Items.AddRange(new ListItem[] {
                            new ListItem("374500261001009","374500261001009"),
                            new ListItem("374500262001008","374500261001009"),
                            new ListItem("100010000000006","100010000000006"),
                            new ListItem("376507058418583","376507058418583"),
                            new ListItem("375860706730029","375860706730029")});
                        break;
                    case "MASTERCARD":
                        ddlCardNumbers.Items.AddRange(new ListItem[] {
                            new ListItem("5457210001000050","5457210001000050"),
                            new ListItem("5204730000001003","5457210001000050"),
                            new ListItem("5457210001000035","5457210001000050"),
                            new ListItem("1000120000000004","1000120000000004"),
                            new ListItem("5520357339586202","5520357339586202"),
                            new ListItem("5462032649133728","5462032649133728"),
                            new ListItem("5566257401681516","5566257401681516")});
                        break;
                    case "VISA":
                        ddlCardNumbers.Items.AddRange(new ListItem[] {
                            new ListItem("4005555555000009","4005555555000009"),
                            new ListItem("4123450131000508","4123450131000508"),
                            new ListItem("4123450131001381","4123450131001381"),
                            new ListItem("1000120000000004","1000120000000004"),
                            new ListItem("4015501150000216","4015501150000216"),
                            new ListItem("4444333322221111","4444333322221111"),
                            new ListItem("4539570141492417","4539570141492417")});
                        break;
                }
            }

            ddlCardNumbers.Items.Insert(0, new ListItem(cardNrText, "-1"));

            checkReturnCalendar();


        }

        private void InitScrape()
        {
            InitScrape(true);
        }

        private void InitScrape(bool ResetFlightTable)
        {
            _scrapeInfo = new EAScrape.ScrapeInfo();


            //-- init _scrapeInfo, simulate pax
            _scrapeInfo.InsertVariable("REQ_NUM_ADULT", ddlAdults.SelectedValue, true);
            _scrapeInfo.InsertVariable("REQ_NUM_CHILD", ddlChildren.SelectedValue, true);
            _scrapeInfo.InsertVariable("REQ_NUM_BABY", ddlBabies.SelectedValue, true);

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


            if (ddlChildren.SelectedIndex > 0 && cbUseChildAges.Checked)
            {
                for (int c = 1; c <= ddlChildren.SelectedIndex; c++)
                {
                    int childAge = 10 - c;

                    _scrapeInfo.InsertVariable("REQ_BOO_CHILD_AGE_YEARS" + c.ToString(), childAge.ToString(), true);
                }

            }


            if (ResetFlightTable)
                Session["OutwardFlights"] = null;

            List<string> originList = new List<string>();
            List<string> destinationList = new List<string>();

            if (cbMultiple.Checked)
            {
                if (!string.IsNullOrEmpty(hMultipleRoutes.Value))
                {
                    string[] itemLines = hMultipleRoutes.Value.Split(';');
                    if (itemLines.Length > 0)
                    {
                        for (int i = 0; i < itemLines.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(itemLines[i]))
                            {
                                string[] items = itemLines[i].Split('|');
                                if (items[0].Equals("Out"))
                                {

                                    if (!originList.Contains(items[1]))
                                        originList.Add(items[1]);

                                    if (!destinationList.Contains(items[2]))
                                        destinationList.Add(items[2]);

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                string depart = ddlDepart.SelectedValue;
                string arrive = ddlArrive.SelectedValue;

                if (!string.IsNullOrEmpty(tbDepart.Text) && !string.IsNullOrEmpty(tbArrive.Text))
                {
                    depart = tbDepart.Text;
                    arrive = tbArrive.Text;
                }

                originList.Add(depart);
                destinationList.Add(arrive);
            }

            int ctr = 0;
            string origins = string.Empty;
            for (int i = 0; i < originList.Count; i++)
            {
                if (ctr > 0)
                    origins += ",";
                origins += originList[i];
                ctr++;
            }

            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME", origins, true);
            ctr = 0;
            string destinations = string.Empty;
            for (int i = 0; i < destinationList.Count; i++)
            {
                if (ctr > 0)
                    destinations += ",";
                destinations += destinationList[i];
                ctr++;
            }
            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME", destinations, true);


            _scrapeInfo.InsertVariable("REQ_START_YEAR", calDepart.SelectedDate.ToString("yyyy"), true);
            _scrapeInfo.InsertVariable("REQ_START_MONTH", calDepart.SelectedDate.ToString("MM"), true);
            _scrapeInfo.InsertVariable("REQ_START_DAY", calDepart.SelectedDate.ToString("dd"), true);

            if (!cbOneWay.Checked)
            {
                _scrapeInfo.InsertVariable("REQ_START_YEAR_Outward", calDepart.SelectedDate.ToString("yyyy"), true);
                _scrapeInfo.InsertVariable("REQ_START_MONTH_Outward", calDepart.SelectedDate.ToString("MM"), true);
                _scrapeInfo.InsertVariable("REQ_START_DAY_Outward", calDepart.SelectedDate.ToString("dd"), true);

                _scrapeInfo.InsertVariable("REQ_START_YEAR_Return", calReturn.SelectedDate.ToString("yyyy"), true);
                _scrapeInfo.InsertVariable("REQ_START_MONTH_Return", calReturn.SelectedDate.ToString("MM"), true);
                _scrapeInfo.InsertVariable("REQ_START_DAY_Return", calReturn.SelectedDate.ToString("dd"), true);


            }

            List<ListItem> selectedFareClasses = cblFareClass.Items.Cast<ListItem>().Where(li => li.Selected).ToList();

            string[] selectedClass = new string[4] { "0", "0", "0", "0" };

            for (int i = 0; i < selectedFareClasses.Count; i++)
            {
                switch (selectedFareClasses[i].Value.ToLower())
                {
                    case "economy": selectedClass[3] = "1"; break;
                    case "premium economy": selectedClass[2] = "1"; break;
                    case "business": selectedClass[1] = "1"; break;
                    case "first": selectedClass[0] = "1"; break;
                }
            }
            string MFC = string.Empty;
            for (int i = 0; i < selectedClass.Length; i++)
                MFC += selectedClass[i];
            _scrapeInfo.InsertVariable("REQ_MFC", MFC, true);

            if (!string.IsNullOrEmpty(tbPrefCarriers.Text))
                _scrapeInfo.InsertVariable("SCR_CARLIST", tbPrefCarriers.Text, true);
            else
                _scrapeInfo.InsertVariable("SCR_CARLIST", "", true);

            if (!string.IsNullOrEmpty(tbExclCarriers.Text))
                _scrapeInfo.InsertVariable("SCR_CARLISTEXCL", tbExclCarriers.Text, true);




            //Set User info

            _scrapeInfo.InsertVariable("WS_Username", "PYTON_TEST", true);
            _scrapeInfo.InsertVariable("WS_Password", "PYTO_2016", true);
            _scrapeInfo.InsertVariable("WS_URLPart", "http://productapi-docs.airtrade.com", true);
            _scrapeInfo.InsertVariable("WS_AffiliateCode", "PRDAPI", true);
            _scrapeInfo.InsertVariable("WS_AccountCode", "PRDAPI", true);
            _scrapeInfo.InsertVariable("WS_ApplicationType", "ProductApi", true);
            _scrapeInfo.InsertVariable("WS_Digest", "JcJFfnJVzS9CzNYxRAjcoQ==", true);
            _scrapeInfo.InsertVariable("WS_CultureCode", "nl-NL", true);

            _scrapeInfo.InsertVariable("WS_PageSize", System.Configuration.ConfigurationManager.AppSettings["MaxPageSize"], true);

            if (cbIgnoreCarIATA.Checked)
                _scrapeInfo.InsertVariable("WS_IgnoreCarrier", "true", true);

            //Logging
            _scrapeInfo.lbAutoErrorLog = System.Configuration.ConfigurationManager.AppSettings["LogItems"].Equals("1");

            //Baggage response
            if (tbExtended.Text.Contains("Baggage_response"))
            {
                string search = "\"Baggage_response\":\"";
                string resp = tbExtended.Text.Substring(tbExtended.Text.IndexOf(search) + search.Length);
                search = cbOneWay.Checked ? "\",\"VF_LOG\"" : "\",\"SRF_LOG\"";
                resp = resp.Substring(0, resp.IndexOf(search));
                if (resp.Length > 0)
                    _scrapeInfo.InsertVariable("WS_BagResponse", resp.Replace("\\r\\n", "").Replace("\\", ""), true);
            }
        }


        /************************ Checks ***************************/

        protected void cvCheck_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            if (string.IsNullOrEmpty(tbDepart.Text) && string.IsNullOrEmpty(tbArrive.Text))
            {
                if (ddlDepart.SelectedIndex.Equals(0))
                {
                    cvCheck.ErrorMessage = "Select a departure airport";
                    args.IsValid = false;
                    return;
                }
                if (ddlArrive.SelectedIndex.Equals(0))
                {
                    cvCheck.ErrorMessage = "Select an arrive airport";
                    args.IsValid = false;
                    return;
                }
                if (ddlDepart.SelectedIndex > 0 && ddlArrive.SelectedIndex > 0 && ddlDepart.SelectedValue.Equals(ddlArrive.SelectedValue))
                {
                    cvCheck.ErrorMessage = "Select different arrive airport";
                    args.IsValid = false;
                    return;
                }
            }
            if (ddlDepart.SelectedIndex.Equals(0) && ddlArrive.SelectedIndex.Equals(0))
            {
                if (string.IsNullOrEmpty(tbDepart.Text))
                {
                    cvCheck.ErrorMessage = "Provide a departure airport";
                    args.IsValid = false;
                    return;
                }
                if (string.IsNullOrEmpty(tbArrive.Text))
                {
                    cvCheck.ErrorMessage = "Provide a arrival airport";
                    args.IsValid = false;
                    return;
                }
            }
            if (cbOneWay.Checked)
            {
                if (calDepart.SelectedDate < DateTime.Today)
                {
                    cvCheck.ErrorMessage = "Select a correct departure date";
                    args.IsValid = false;
                    return;
                }
                if (phPayment.Visible && ddlCardNumbers.SelectedValue.Equals("-1"))
                {
                    cvCheck.ErrorMessage = "Select a cc number";
                    args.IsValid = false;
                    return;
                }
            }
            else
            {
                if (calDepart.SelectedDate < DateTime.Today)
                {
                    cvCheck.ErrorMessage = "Select a correct departure date";
                    args.IsValid = false;
                    return;
                }

                if (calDepart.SelectedDate > calReturn.SelectedDate)
                {
                    cvCheck.ErrorMessage = "Select a correct departure and arrival date";
                    args.IsValid = false;
                    return;
                }
            }
        }

        protected void cbOneWay_CheckedChanged(object sender, EventArgs e)
        {
            checkReturnCalendar();

            if (!string.IsNullOrEmpty(hOutwardSelected.Value) && !string.IsNullOrEmpty(hReturnSelected.Value) && !cbOneWay.Checked)
            {

                string[] valOut = hOutwardSelected.Value.Split(',');
                string[] valRet = hReturnSelected.Value.Split(',');

                string oriOut = valOut[9];
                string desOut = valOut[10];
                string oriRet = valRet[9];
                string desRet = valRet[10];

                if (oriOut.Equals(desRet) && desOut.Equals(oriRet))
                {
                    string[] depTimes = valOut[4].Split('~');

                    calDepart.SelectedDate = DateTime.ParseExact(depTimes[0].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    tbDepart.Text = valOut[9];
                    ddlDepart.ClearSelection();
                    if (ddlDepart.Items.FindByValue(valOut[9]) != null)
                        ddlDepart.Items.FindByValue(valOut[9]).Selected = true;

                    tbArrive.Text = valOut[10];
                    ddlArrive.ClearSelection();
                    if (ddlArrive.Items.FindByValue(valOut[10]) != null)
                        ddlArrive.Items.FindByValue(valOut[10]).Selected = true;

                    depTimes = valRet[4].Split('~');

                    calReturn.SelectedDate = DateTime.ParseExact(depTimes[0].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                }

            }
        }

        protected void ddlCardNumbers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //checkReturnCalendar();

            string selectedMonth = string.Empty;
            string selectedYear = string.Empty;
            switch (ddlCardNumbers.SelectedValue)
            {
                case "5457210001000050":
                case "5204730000001003":
                case "5457210001000035": tbCCV.Text = "123"; selectedMonth = "12"; selectedYear = "2017"; break;
                case "4005555555000009":
                case "4123450131000508": tbCCV.Text = "123"; selectedMonth = "05"; selectedYear = "2017"; break;
                case "4123450131001381": tbCCV.Text = "123"; selectedMonth = "12"; selectedYear = "2017"; break;
                case "5566257401681516": tbCCV.Text = "513"; selectedMonth = "03"; selectedYear = "2017"; break;
            }

            if (ddlValidMonth.Items.FindByText(selectedMonth) != null)
            {
                ddlValidMonth.ClearSelection();
                ddlValidMonth.Items.FindByText(selectedMonth).Selected = true;
            }
            if (ddlValidYear.Items.FindByText(selectedYear) != null)
            {
                ddlValidYear.ClearSelection();
                ddlValidYear.Items.FindByText(selectedYear).Selected = true;
            }

        }

        /******************** private methods *****************************/

        private void initPage()
        {
            hOutwardID.Value = string.Empty;
            hSelRowOut.Value = string.Empty;
            hReturnID.Value = string.Empty;
            hSelRowRet.Value = string.Empty;
            hCCFee.Value = "0";

            phError.Visible = false;
            phSummary.Visible = false;
            phOutbound.Visible = false;
            //phReturn.Visible = false;
            //btnPriceDetails.Visible = false;
            btnCheckAvail.Visible = false;
            //btnBook.Visible = false;


            //phMultiple.Visible = false;
            hMultipleRoutes.Value = string.Empty;

            tbReq.Text = string.Empty;
            tbRes.Text = string.Empty;

            phReturnDate.Visible = false;
            //phPayment.Visible = false;
            //phOutFlightSelect.Visible = false;
            //phPNR.Visible = false;
            //tbPNR.Text = string.Empty;

            checkReturnCalendar();

        }

        private void checkReturnCalendar()
        {
            phReturnDate.Visible = !cbOneWay.Checked;

        }

        private void BuildSummary()
        {
            int nrOfAdults = Convert.ToInt32(ddlAdults.SelectedValue);
            int nrOfChildren = Convert.ToInt32(ddlChildren.SelectedValue);
            int nrOfBabies = Convert.ToInt32(ddlBabies.SelectedValue);

            double total = 0d;
            string currencyCode = "EUR";

            StringBuilder sb = new StringBuilder();
            sb.Append("<table>");

            if (hOutwardID.Value.Contains("["))
            {
                string[] itemRows = hOutwardID.Value.Split('[');
                for (int i = 0; i < itemRows.Length; i++)
                {
                    string[] items = itemRows[i].Split(',');

                    double adultIncTaxPP = string.IsNullOrEmpty(items[7]) ? 0 : Convert.ToDouble(items[7].Replace(".", ","));
                    double childIncTaxPP = string.IsNullOrEmpty(items[8]) ? 0 : Convert.ToDouble(items[8].Replace(".", ","));
                    double babyIncTaxPP = string.IsNullOrEmpty(items[9]) ? 0 : Convert.ToDouble(items[9].Replace(".", ","));

                    double totalRow = string.IsNullOrEmpty(items[6]) ? 0 : Convert.ToDouble(items[6].Replace(".", ","));

                    total += totalRow;

                    sb.Append(string.Format("<tr><td colspan='5' style='height:10px'></td></tr><tr><td>{0}:</td><td width='25'></td><td align='right'></td><td width='25'></td><td width='25'></td></tr><tr><td colspan='5' style='height:10px'></td></tr>", "Row select " + i.ToString()));

                    double adults = nrOfAdults * adultIncTaxPP;
                    sb.Append(string.Format("<tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Adults", (decimal)adults, currencyCode));

                    double children = nrOfChildren * childIncTaxPP;
                    sb.Append(string.Format("<tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Children", (decimal)children, currencyCode));

                    double babies = nrOfBabies * babyIncTaxPP;
                    sb.Append(string.Format("<tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Children", (decimal)babies, currencyCode));

                    sb.Append(string.Format("<tr><td colspan='5' style='height:10px'></td></tr><tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Total", (decimal)totalRow, currencyCode));

                }
            }
            else
            {
                string[] items = hOutwardID.Value.Split(',');

                double adultIncTaxPP = string.IsNullOrEmpty(items[7]) ? 0 : Convert.ToDouble(items[7].Replace(".", ","));
                double childIncTaxPP = string.IsNullOrEmpty(items[8]) ? 0 : Convert.ToDouble(items[8].Replace(".", ","));
                double babyIncTaxPP = string.IsNullOrEmpty(items[9]) ? 0 : Convert.ToDouble(items[9].Replace(".", ","));

                double totalRow = string.IsNullOrEmpty(items[6]) ? 0 : Convert.ToDouble(items[6].Replace(".", ","));

                total += totalRow;



                double adults = nrOfAdults * adultIncTaxPP;
                sb.Append(string.Format("<tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Adults", (decimal)adults, currencyCode));

                double children = nrOfChildren * childIncTaxPP;
                sb.Append(string.Format("<tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Children", (decimal)children, currencyCode));

                double babies = nrOfBabies * babyIncTaxPP;
                sb.Append(string.Format("<tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Children", (decimal)babies, currencyCode));

                sb.Append(string.Format("<tr><td colspan='5' style='height:25px'></td></tr><tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Total", (decimal)totalRow, currencyCode));

            }

            if (!string.IsNullOrEmpty(hCheckAvailTotal.Value))
                sb.Append(string.Format("<tr><td colspan='5' style='height:25px'></td></tr><tr><td>{0}:</td><td width='25'></td><td align='right'>{1:0.00}</td><td width='25'></td><td width='25'>{2}</td></tr>", "Check availability Total", hCheckAvailTotal.Value, currencyCode));

            lblSummary.Text = hOutwardTotal.Value = string.Format("{0:0.00}", (decimal)total);

            if (!hCheckAvailTotal.Value.Equals("0") && !Convert.ToDouble(hOutwardTotal.Value).Equals(Convert.ToDouble(hCheckAvailTotal.Value)))
                sb.Append(string.Format("<tr><td colspan='5' style='height:25px'></td></tr><tr><td colspan='5' style='height:25px;color:red;'>Difference in getflight price ({0}) and availability price ({1})!!!</td></tr>", hOutwardTotal.Value, hCheckAvailTotal.Value));

            sb.Append("</table>");

            litSummary.Text = sb.ToString();

            phSummary.Visible = true;


        }

        /****************** flight events ******************************/
        protected void btnFlight_Click(object sender, EventArgs e)
        {
            hOutwardID.Value = string.Empty;
            hSelRowOut.Value = string.Empty;
            hReturnID.Value = string.Empty;
            hSelRowRet.Value = string.Empty;


            Session["AvailResponse"] = null;

            Page.Validate("Check");
            if (Page.IsValid)
            {
                InitScrape();

                if (!cbUsePFPAPI.Checked)
                {

                    _scrapeInfo.SCR_PROCESS_NAME = "SearchFlights";
                    if (!cbOneWay.Checked)
                        _scrapeInfo.InsertVariable("RR search", "true", true);

                    SetStartTime();

                    if (cbJet2.Checked)
                    {
                        _scrapeInfo.InsertVariable("getSession", "true", true);
                        EAScrape.Jet2ApiLocal j2 = new EAScrape.Jet2ApiLocal();
                        tbRes.Text = j2.Jet2(ref _scrapeInfo, ref _foundInfo, _webPage);
                    }
                    else
                    {
                        Airtrade.AitradeMain am = new Airtrade.AitradeMain();
                        tbRes.Text = am.Aitrade(ref _scrapeInfo, ref _foundInfo, _webPage);
                    }

                    SetEndTime();

                    Session["AvailResponse"] = tbRes.Text;
                }
                else
                {

                }
                phOutbound.Visible = true;
                btnCheckAvail.Visible = true;

            }
        }

        protected void btnRnd_Click(object sender, EventArgs e)
        {
            string loc = Server.MapPath("XML/Airports.xml");
            XmlDocument xAirports = new XmlDocument();
            xAirports.Load(loc);

            XmlNodeList airports = xAirports.SelectNodes("Airports/Airport");

            if (airports.Count > 0)
            {
                int rndNumAirport1 = RandNumber(0, airports.Count);
                int rndNumAirport2 = RandNumber(0, airports.Count);

                int adt = RandNumber(1, ddlAdults.Items.Count);
                int chd = RandNumber(1, ddlChildren.Items.Count);
                int inf = RandNumber(1, ddlBabies.Items.Count);

                string air1 = FlightClient.XML.GetNode(airports[rndNumAirport1], "", "IATA");
                string air2 = FlightClient.XML.GetNode(airports[rndNumAirport2], "", "IATA");

                if (!string.IsNullOrEmpty(air1))
                {
                    if (ddlDepart.Items.FindByValue(air1) != null)
                    {
                        ddlDepart.ClearSelection();
                        ddlDepart.Items.FindByValue(air1).Selected = true;

                        tbDepart.Text = air1;
                    }
                }

                if (!string.IsNullOrEmpty(air2))
                {
                    if (ddlArrive.Items.FindByValue(air2) != null)
                    {
                        ddlArrive.ClearSelection();
                        ddlArrive.Items.FindByValue(air2).Selected = true;

                        tbArrive.Text = air2;
                    }
                }

                DateTime date1 = GetRandomDate();
                DateTime date2 = GetRandomDate();

                DateTime cal1 = date1 < date2 ? date1 : date2;
                DateTime cal2 = date1 < date2 ? date2 : date1;

                calDepart.SelectedDate = cal1;
                calDepart.VisibleDate = cal1;

                calReturn.SelectedDate = cal2;
                calReturn.VisibleDate = cal2;

            }
        }

        protected void btnCheckAvail_Click(object sender, EventArgs e)
        {
            InitScrape();

            if (cbOneWay.Checked)
            {
                #region OneWay
                try
                {
                    if (string.IsNullOrEmpty(hOutwardID.Value)) return;

                    string[] valOut = hOutwardID.Value.Split(',');
                    if (valOut.Length < 1) return;

                    string[] keys = valOut[0].Split('|');
                    if (keys.Length < 1) return;

                    string[] carrierCodes = valOut[1].Split('~');
                    string[] flightNumbers = valOut[2].Split('~');
                    string[] origins = valOut[9].Split('~');
                    string[] destinations = valOut[10].Split('~');
                    string[] depTimes = valOut[3].Split('~');
                    string[] arrTimes = valOut[4].Split('~');

                    //_scrapeInfo.InsertVariable("SessionIdentifier", keys[0], true);
                    //_scrapeInfo.InsertVariable("GroupedFlightResultIdentifier", keys[1], true);
                    //_scrapeInfo.InsertVariable("SelectedGroupedFlightOptionIdentifier", keys[2], true);
                    //_scrapeInfo.InsertVariable("SelectedLegOptionsIdentifiers", "\"" + keys[3].Replace("~","\",\"") + "\"", true);

                    string deptime = depTimes[0].Split('T')[1];
                    string arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                    DateTime depDay = DateTime.ParseExact(depTimes[depTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrDay = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


                    _scrapeInfo.InsertVariable("OperatingCarrierIATA", carrierCodes[0], true);

                    _scrapeInfo.InsertVariable("arrival day", arrDay.ToString("dd.MM.yyyy"), true);
                    _scrapeInfo.InsertVariable("departure time", deptime, true);
                    _scrapeInfo.InsertVariable("arrival time", arrtime, true);
                    _scrapeInfo.InsertVariable("flight number", flightNumbers[0], true);
                    _scrapeInfo.SCR_PROCESS_NAME = "SearchFlights";
                    _scrapeInfo.lbIsVF = true;

                    SetStartTime();

                    Airtrade.AitradeMain am = new Airtrade.AitradeMain();
                    tbExtended.Text = am.Aitrade(ref _scrapeInfo, ref _foundInfo, _webPage);

                    SetEndTime();

                    Session["SelectedFlightResponse"] = tbExtended.Text;

                    Newtonsoft.Json.Linq.JObject jObj = Airtrade.General.parseJSON(tbExtended.Text);

                    btnBook.Visible = false;
                    if (checkAvailResponse(jObj))
                    {
                        valOut[0] = jObj.SelectToken("$..SelectedIdentifiers.SessionIdentifier") + "|" + jObj.SelectToken("$..SelectedIdentifiers.GroupedFlightResultIdentifier") + "|" + jObj.SelectToken("$..SelectedIdentifiers.SelectedGroupedFlightOptionIdentifier") + "|" + jObj.SelectToken("$..SelectedIdentifiers.SelectedLegOptionsIdentifiers");

                        StringBuilder sb = new StringBuilder();
                        for (int t = 0; t < valOut.Length; t++)
                        {
                            if (t > 0)
                                sb.Append(",");
                            sb.Append(valOut[t]);
                        }

                        hOutwardID.Value = sb.ToString();

                        btnBook.Visible = true;

                        phPayment.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    phError.Visible = true;

                    litError.Text = ex.Message;
                }
                #endregion
            }
            else
            {
                if (!string.IsNullOrEmpty(hOutwardSelected.Value) && !string.IsNullOrEmpty(hReturnSelected.Value))
                {

                }
                else if (string.IsNullOrEmpty(hOutwardID.Value)) return;

                try
                {
                    int nrOfAdults = Convert.ToInt32(ddlAdults.SelectedValue);
                    int nrOfChildren = Convert.ToInt32(ddlChildren.SelectedValue);
                    int nrOfBabies = Convert.ToInt32(ddlBabies.SelectedValue);

                    if (!string.IsNullOrEmpty(hOutwardSelected.Value) && !string.IsNullOrEmpty(hReturnSelected.Value))
                    {
                        #region 2 single filghts selected

                        string[] valOut = hOutwardSelected.Value.Split(',');
                        if (valOut.Length < 1) return;

                        string[] carrierCodes = valOut[1].Split('~');
                        string[] flightNumbers = valOut[2].Split('~');
                        string[] origins = valOut[9].Split('~');
                        string[] destinations = valOut[10].Split('~');
                        string[] depTimes = valOut[3].Split('~');
                        string[] arrTimes = valOut[4].Split('~');

                        string deptime = depTimes[0].Split('T')[1];
                        string arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                        DateTime depDay = DateTime.ParseExact(depTimes[depTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime arrDay = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        _scrapeInfo.InsertVariable("arrival day_Outward", arrDay.ToString("dd.MM.yyyy"), true);
                        _scrapeInfo.InsertVariable("departure time_Outward", deptime, true);
                        _scrapeInfo.InsertVariable("arrival time_Outward", arrtime, true);
                        _scrapeInfo.InsertVariable("flight number_Outward", flightNumbers[0], true);

                        string carrierCode = valOut[1].Split(']')[0].Contains('~') ? valOut[1].Split(']')[0].Split('~')[0] : valOut[1].Split(']')[0];
                        _scrapeInfo.InsertVariable("CaRrIeR_IaTa_Outward", carrierCode, true);

                        string origin = valOut[9].Split(']')[0].Contains('~') ? valOut[9].Split(']')[0].Split('~')[0] : valOut[9].Split(']')[0];
                        string destination = valOut[10].Split(']')[0].Contains('~') ? valOut[10].Split(']')[0].Split('~')[valOut[10].Split(']')[0].Split('~').Length - 1] : valOut[10].Split(']')[0];

                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME", origin, true);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME", destination, true);

                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Outward", origin, true);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Outward", destination, true);

                        string[] valRet = hReturnSelected.Value.Split(',');
                        if (valRet.Length < 1) return;

                        carrierCodes = valRet[1].Split('~');
                        flightNumbers = valRet[2].Split('~');
                        origins = valRet[9].Split('~');
                        destinations = valRet[10].Split('~');
                        depTimes = valRet[3].Split('~');
                        arrTimes = valRet[4].Split('~');

                        deptime = depTimes[0].Split('T')[1];
                        arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                        depDay = DateTime.ParseExact(depTimes[depTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        arrDay = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        _scrapeInfo.InsertVariable("arrival day_Return", arrDay.ToString("dd.MM.yyyy"), true);
                        _scrapeInfo.InsertVariable("departure time_Return", deptime, true);
                        _scrapeInfo.InsertVariable("arrival time_Return", arrtime, true);
                        _scrapeInfo.InsertVariable("flight number_Return", flightNumbers[0], true);

                        origin = valRet[9].Split(']')[0].Contains('~') ? valRet[9].Split(']')[0].Split('~')[0] : valRet[9].Split(']')[0];
                        destination = valRet[10].Split(']')[0].Contains('~') ? valRet[10].Split(']')[0].Split('~')[valRet[10].Split(']')[0].Split('~').Length - 1] : valRet[10].Split(']')[0];

                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Return", origin, true);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Return", destination, true);

                        #endregion

                    }
                    else if (!string.IsNullOrEmpty(hOutwardID.Value))
                    {

                        #region 1 SRF selected

                        string[] valOut = hOutwardID.Value.Split(',');
                        if (valOut.Length < 1) return;

                        string[] keys = valOut[0].Split('|');
                        if (keys.Length < 1) return;


                        string depTime = valOut[3].Split(']')[0].Contains('~') ? valOut[3].Split(']')[0].Split('~')[0] : valOut[3].Split(']')[0];
                        string depTime_out = depTime.Split('T')[1];
                        depTime = valOut[3].Split(']')[1].Contains('~') ? valOut[3].Split(']')[1].Split('~')[0] : valOut[3].Split(']')[1];
                        string depTime_Ret = depTime.Split('T')[1];

                        string arrTimeOut = valOut[4].Split(']')[0].Contains('~') ? valOut[4].Split(']')[0].Split('~')[valOut[4].Split(']')[0].Split('~').Length - 1] : valOut[4].Split(']')[0];
                        string arrTime_Out = arrTimeOut.Split('T')[1];
                        string arrTimeRet = valOut[4].Split(']')[1].Contains('~') ? valOut[4].Split(']')[1].Split('~')[valOut[4].Split(']')[1].Split('~').Length - 1] : valOut[4].Split(']')[1];
                        string arrTime_Ret = arrTimeRet.Split('T')[1];

                        DateTime arrDay_Out = DateTime.ParseExact(arrTimeOut.Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        DateTime arrDay_Ret = DateTime.ParseExact(arrTimeRet.Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                        _scrapeInfo.InsertVariable("arrival day_Outward", arrDay_Out.ToString("dd.MM.yyyy"), true);
                        _scrapeInfo.InsertVariable("arrival day_Return", arrDay_Ret.ToString("dd.MM.yyyy"), true);
                        _scrapeInfo.InsertVariable("departure time_Outward", depTime_out, true);
                        _scrapeInfo.InsertVariable("departure time_Return", depTime_Ret, true);
                        _scrapeInfo.InsertVariable("arrival time_Outward", arrTime_Out, true);
                        _scrapeInfo.InsertVariable("arrival time_Return", arrTime_Ret, true);

                        string carrierCode_Out = valOut[1].Split(']')[0].Contains('~') ? valOut[1].Split(']')[0].Split('~')[0] : valOut[1].Split(']')[0];
                        string flightNumber_Out = valOut[2].Split(']')[0].Contains('~') ? valOut[2].Split(']')[0].Split('~')[0] : valOut[2].Split(']')[0];

                        string carrierCode_Ret = carrierCode_Out;
                        string flightNumber_Ret = valOut[2].Split(']')[1].Contains('~') ? valOut[2].Split(']')[1].Split('~')[0] : valOut[2].Split(']')[1];

                        _scrapeInfo.InsertVariable("flight number_Outward", flightNumber_Out, true);
                        _scrapeInfo.InsertVariable("flight number_Return", flightNumber_Ret, true);

                        string carrierCode = valOut[1].Split(']')[0].Contains('~') ? valOut[1].Split(']')[0].Split('~')[0] : valOut[1].Split(']')[0];

                        _scrapeInfo.InsertVariable("CaRrIeR_IaTa_Outward", carrierCode, true);

                        string origin = valOut[9].Split(']')[0].Contains('~') ? valOut[9].Split(']')[0].Split('~')[0] : valOut[9].Split(']')[0];
                        string destination = valOut[10].Split(']')[0].Contains('~') ? valOut[10].Split(']')[0].Split('~')[valOut[10].Split(']')[0].Split('~').Length - 1] : valOut[10].Split(']')[0];

                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME", origin, true);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME", destination, true);

                        string origin_Out = valOut[9].Split(']')[0].Contains('~') ? valOut[9].Split(']')[0].Split('~')[0] : valOut[9].Split(']')[0];
                        string destinations_Out = valOut[10].Split(']')[0].Contains('~') ? valOut[10].Split(']')[0].Split('~')[valOut[10].Split(']')[0].Split('~').Length - 1] : valOut[10].Split(']')[0];

                        string origin_Ret = valOut[9].Split(']')[1].Contains('~') ? valOut[9].Split(']')[1].Split('~')[0] : valOut[9].Split(']')[1];
                        string destinations_Ret = valOut[10].Split(']')[1].Contains('~') ? valOut[10].Split(']')[1].Split('~')[valOut[10].Split(']')[1].Split('~').Length - 1] : valOut[10].Split(']')[1];

                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Outward", origin_Out, true);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Outward", destinations_Out, true);

                        _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Return", origin_Ret, true);
                        _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Return", destinations_Ret, true);

                        //_scrapeInfo.InsertVariable("SessionIdentifier", keys[0], true);
                        //_scrapeInfo.InsertVariable("GroupedFlightResultIdentifier", keys[1], true);
                        //_scrapeInfo.InsertVariable("SelectedGroupedFlightOptionIdentifier", keys[2], true);
                        //_scrapeInfo.InsertVariable("SelectedLegOptionsIdentifiers", "\"" + keys[3].Replace("~", "\",\"") + "\"", true);

                        #endregion
                    }

                    _scrapeInfo.InsertVariable("RR search", "false", true);
                    _scrapeInfo.SCR_PROCESS_NAME = "SearchReturnFlight";

                    SetStartTime();

                    Airtrade.AitradeMain am = new Airtrade.AitradeMain();
                    tbExtended.Text = am.Aitrade(ref _scrapeInfo, ref _foundInfo, _webPage);

                    SetEndTime();

                    Session["SelectedFlightResponse"] = tbExtended.Text;

                    Newtonsoft.Json.Linq.JObject jObj = Airtrade.General.parseJSON(tbExtended.Text);

                    btnBook.Visible = false;
                    if (checkAvailResponse(jObj))
                    {
                        string[] valOut = ",,,,,,,,,,".Split(',');

                        valOut[0] = jObj.SelectToken("$..SelectedIdentifiers.SessionIdentifier").ToString() + "|" + jObj.SelectToken("$..SelectedIdentifiers.GroupedFlightResultIdentifier").ToString() + "|" + jObj.SelectToken("$..SelectedIdentifiers.SelectedGroupedFlightOptionIdentifier").ToString() + "|" + jObj.SelectToken("$..SelectedIdentifiers.SelectedLegOptionsIdentifiers").ToString().Replace(",", "~");
                        valOut[1] = jObj.SelectToken("$.Airline.Code").ToString();
                        valOut[2] = jObj.SelectToken("$.SelectedLegs[0].FlightNumber").ToString() + "]" + jObj.SelectToken("$.SelectedLegs[1].FlightNumber").ToString();
                        valOut[3] = Airtrade.General.jSonDate(jObj.SelectToken("$.SelectedLegs[0].DepartureDate")) + "T" + jObj.SelectToken("$.SelectedLegs[0].DepartureTime") + "]" +
                                    Airtrade.General.jSonDate(jObj.SelectToken("$.SelectedLegs[1].DepartureDate")) + "T" + jObj.SelectToken("$.SelectedLegs[1].DepartureTime");
                        valOut[4] = Airtrade.General.jSonDate(jObj.SelectToken("$.SelectedLegs[0].ArrivalDate")) + "T" + jObj.SelectToken("$.SelectedLegs[0].ArrivalTime") + "]" +
                                    Airtrade.General.jSonDate(jObj.SelectToken("$.SelectedLegs[1].ArrivalDate")) + "T" + jObj.SelectToken("$.SelectedLegs[1].ArrivalTime");
                        valOut[5] = jObj.SelectToken("$.PriceBreakdown.Total").ToString().Replace(",", ".");
                        valOut[6] = jObj.SelectToken("$..TicketPrices[?(@.PaxType=='Adult')].TotalPerPax").ToString().Replace(",", ".");
                        valOut[7] = Airtrade.General.getToken(jObj, "", "$..TicketPrices[?(@.PaxType=='Child')].TotalPerPax").Replace(",", ".");
                        valOut[8] = Airtrade.General.getToken(jObj, "", "$..TicketPrices[?(@.PaxType=='Infant')].TotalPerPax").Replace(",", ".");
                        valOut[9] = jObj.SelectToken("$.SelectedLegs[0].DepartureAirport.Code").ToString() + "]" + jObj.SelectToken("$.SelectedLegs[1].DepartureAirport.Code").ToString();
                        valOut[10] = jObj.SelectToken("$.SelectedLegs[0].ArrivalAirport.Code").ToString() + "]" + jObj.SelectToken("$.SelectedLegs[1].ArrivalAirport.Code").ToString();

                        StringBuilder sb = new StringBuilder();
                        for (int t = 0; t < valOut.Length; t++)
                        {
                            if (t > 0)
                                sb.Append(",");
                            sb.Append(valOut[t]);
                        }

                        hOutwardID.Value = sb.ToString();

                        btnBook.Visible = true;

                        phPayment.Visible = true;
                    }

                }
                catch (Exception ex)
                {
                    phError.Visible = true;

                    litError.Text = ex.Message;
                }
            }
        }

        protected void btnBookFlight_Click(object sender, EventArgs e)
        {
            Page.Validate("Check");
            if (Page.IsValid)
            {
                InitScrape();

                if (string.IsNullOrEmpty(hOutwardID.Value)) return;

                string[] valOut = hOutwardID.Value.Split(',');
                if (valOut.Length < 1) return;

                string[] keys = valOut[0].Split('|');
                if (keys.Length < 1) return;

                _scrapeInfo.InsertVariable("SessionIdentifier", keys[0], true);
                _scrapeInfo.InsertVariable("GroupedFlightResultIdentifier", keys[1], true);
                _scrapeInfo.InsertVariable("SelectedGroupedFlightOptionIdentifier", keys[2], true);
                _scrapeInfo.InsertVariable("SelectedLegOptionsIdentifiers", "\"" + keys[3].Replace("~", "\",\"") + "\"", true);

                if (tbExtended.Text.Contains("\"_selectedFlightIdentifier\""))
                {
                    string s = "\"_selectedFlightIdentifier\":\"";
                    string rest = tbExtended.Text.Substring(tbExtended.Text.IndexOf(s) + s.Length);
                    s = "\",\"";
                    string selectedFlightIdentifier = rest.Substring(0, rest.IndexOf(s));
                    _scrapeInfo.InsertVariable("SelectedFlightIdentifier", selectedFlightIdentifier);

                }

                _scrapeInfo.InsertVariable("WS_PaymentOption", "TPINV", true);

                _scrapeInfo.SCR_PROCESS_NAME = cbOneWay.Checked ? "BookFlight" : "BookReturnFlight";
                string search = string.Empty;

                if (cbOneWay.Checked)
                {
                    #region Oneway

                    string[] carrierCodes = valOut[1].Split('~');
                    string[] flightNumbers = valOut[2].Split('~');
                    string[] origins = valOut[9].Split('~');
                    string[] destinations = valOut[10].Split('~');
                    string[] depTimes = valOut[3].Split('~');
                    string[] arrTimes = valOut[4].Split('~');

                    string deptime = depTimes[0].Split('T')[1];
                    string arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                    DateTime depDay = DateTime.ParseExact(depTimes[depTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrDay = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    _scrapeInfo.InsertVariable("OperatingCarrierIATA", carrierCodes[0], true);

                    //_scrapeInfo.InsertVariable("departure day", depDay.ToString("dd.MM.yyyy"), true);
                    _scrapeInfo.InsertVariable("arrival day", arrDay.ToString("dd.MM.yyyy"), true);
                    _scrapeInfo.InsertVariable("departure time", deptime, true);
                    _scrapeInfo.InsertVariable("arrival time", arrtime, true);
                    _scrapeInfo.InsertVariable("flight number", flightNumbers[0], true);

                    SetStartTime();

                    Airtrade.AitradeMain am = new Airtrade.AitradeMain();
                    tbExtended.Text = am.Aitrade(ref _scrapeInfo, ref _foundInfo, _webPage);

                    SetEndTime();

                    Session["SelectedFlightResponse"] = tbExtended.Text;

                    //Newtonsoft.Json.Linq.JObject jObj = Airtrade.General.parseJSON(tbExtended.Text);

                    #endregion
                }
                else
                {
                    if (!string.IsNullOrEmpty(hOutwardSelected.Value) && !string.IsNullOrEmpty(hReturnSelected.Value))
                    {

                    }
                    else if (string.IsNullOrEmpty(hOutwardID.Value)) return;

                    try
                    {
                        int nrOfAdults = Convert.ToInt32(ddlAdults.SelectedValue);
                        int nrOfChildren = Convert.ToInt32(ddlChildren.SelectedValue);
                        int nrOfBabies = Convert.ToInt32(ddlBabies.SelectedValue);

                        if (!string.IsNullOrEmpty(hOutwardSelected.Value) && !string.IsNullOrEmpty(hReturnSelected.Value))
                        {

                            #region 2 single flights selected

                            string[] carrierCodes = valOut[1].Split('~');
                            string[] flightNumbers = valOut[2].Split('~');
                            string[] origins = valOut[9].Split('~');
                            string[] destinations = valOut[10].Split('~');
                            string[] depTimes = valOut[3].Split('~');
                            string[] arrTimes = valOut[4].Split('~');

                            string deptime = depTimes[0].Split('T')[1];
                            string arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                            DateTime depDay = DateTime.ParseExact(depTimes[depTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            DateTime arrDay = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


                            _scrapeInfo.InsertVariable("arrival day_Outward", arrDay.ToString("dd.MM.yyyy"), true);
                            _scrapeInfo.InsertVariable("departure time_Outward", deptime, true);
                            _scrapeInfo.InsertVariable("arrival time_Outward", arrtime, true);
                            _scrapeInfo.InsertVariable("flight number_Outward", flightNumbers[0], true);

                            string carrierCode = valOut[1].Split(']')[0].Contains('~') ? valOut[1].Split(']')[0].Split('~')[0] : valOut[1].Split(']')[0];
                            _scrapeInfo.InsertVariable("CaRrIeR_IaTa_Outward", carrierCode, true);

                            string origin = valOut[9].Split(']')[0].Contains('~') ? valOut[9].Split(']')[0].Split('~')[0] : valOut[9].Split(']')[0];
                            string destination = valOut[10].Split(']')[0].Contains('~') ? valOut[10].Split(']')[0].Split('~')[valOut[10].Split(']')[0].Split('~').Length - 1] : valOut[10].Split(']')[0];

                            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME", origin, true);
                            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME", destination, true);

                            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Outward", origin, true);
                            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Outward", destination, true);

                            string[] valRet = hReturnSelected.Value.Split(',');
                            if (valRet.Length < 1) return;

                            carrierCodes = valRet[1].Split('~');
                            flightNumbers = valRet[2].Split('~');
                            origins = valRet[9].Split('~');
                            destinations = valRet[10].Split('~');
                            depTimes = valRet[3].Split('~');
                            arrTimes = valRet[4].Split('~');

                            deptime = depTimes[0].Split('T')[1];
                            arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                            depDay = DateTime.ParseExact(depTimes[depTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            arrDay = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                            _scrapeInfo.InsertVariable("arrival day_Return", arrDay.ToString("dd.MM.yyyy"), true);
                            _scrapeInfo.InsertVariable("departure time_Return", deptime, true);
                            _scrapeInfo.InsertVariable("arrival time_Return", arrtime, true);
                            _scrapeInfo.InsertVariable("flight number_Return", flightNumbers[0], true);

                            origin = valRet[9].Split(']')[0].Contains('~') ? valRet[9].Split(']')[0].Split('~')[0] : valRet[9].Split(']')[0];
                            destination = valRet[10].Split(']')[0].Contains('~') ? valRet[10].Split(']')[0].Split('~')[valRet[10].Split(']')[0].Split('~').Length - 1] : valRet[10].Split(']')[0];

                            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Return", origin, true);
                            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Return", destination, true);

                            #endregion

                        }
                        else if (!string.IsNullOrEmpty(hOutwardID.Value))
                        {

                            #region 1 SRF selected or after SRF checkavail

                            string depTime = valOut[3].Split(']')[0].Contains('~') ? valOut[3].Split(']')[0].Split('~')[0] : valOut[3].Split(']')[0];
                            string depTime_out = depTime.Split('T')[1];
                            depTime = valOut[3].Split(']')[1].Contains('~') ? valOut[3].Split(']')[1].Split('~')[0] : valOut[3].Split(']')[1];
                            string depTime_Ret = depTime.Split('T')[1];

                            string arrTimeOut = valOut[4].Split(']')[0].Contains('~') ? valOut[4].Split(']')[0].Split('~')[valOut[4].Split(']')[0].Split('~').Length - 1] : valOut[4].Split(']')[0];
                            string arrTime_Out = arrTimeOut.Split('T')[1];
                            string arrTimeRet = valOut[4].Split(']')[1].Contains('~') ? valOut[4].Split(']')[1].Split('~')[valOut[4].Split(']')[1].Split('~').Length - 1] : valOut[4].Split(']')[1];
                            string arrTime_Ret = arrTimeRet.Split('T')[1];

                            DateTime arrDay_Out = DateTime.ParseExact(arrTimeOut.Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            DateTime arrDay_Ret = DateTime.ParseExact(arrTimeRet.Split('T')[0], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                            _scrapeInfo.InsertVariable("arrival day_Outward", arrDay_Out.ToString("dd.MM.yyyy"), true);
                            _scrapeInfo.InsertVariable("arrival day_Return", arrDay_Ret.ToString("dd.MM.yyyy"), true);
                            _scrapeInfo.InsertVariable("departure time_Outward", depTime_out, true);
                            _scrapeInfo.InsertVariable("departure time_Return", depTime_Ret, true);
                            _scrapeInfo.InsertVariable("arrival time_Outward", arrTime_Out, true);
                            _scrapeInfo.InsertVariable("arrival time_Return", arrTime_Ret, true);


                            string carrierCode_Out = valOut[1].Split(']')[0].Contains('~') ? valOut[1].Split(']')[0].Split('~')[0] : valOut[1].Split(']')[0];
                            string flightNumber_Out = valOut[2].Split(']')[0].Contains('~') ? valOut[2].Split(']')[0].Split('~')[0] : valOut[2].Split(']')[0];

                            string carrierCode_Ret = carrierCode_Out;
                            string flightNumber_Ret = valOut[2].Split(']')[1].Contains('~') ? valOut[2].Split(']')[1].Split('~')[0] : valOut[2].Split(']')[1];

                            _scrapeInfo.InsertVariable("flight number_Outward", flightNumber_Out, true);
                            _scrapeInfo.InsertVariable("flight number_Return", flightNumber_Ret, true);

                            string carrierCode = valOut[1].Split(']')[0].Contains('~') ? valOut[1].Split(']')[0].Split('~')[0] : valOut[1].Split(']')[0];

                            _scrapeInfo.InsertVariable("CaRrIeR_IaTa_Outward", carrierCode, true);

                            string origin = valOut[9].Split(']')[0].Contains('~') ? valOut[9].Split(']')[0].Split('~')[0] : valOut[9].Split(']')[0];
                            string destination = valOut[10].Split(']')[0].Contains('~') ? valOut[10].Split(']')[0].Split('~')[valOut[10].Split(']')[0].Split('~').Length - 1] : valOut[10].Split(']')[0];

                            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME", origin, true);
                            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME", destination, true);

                            string origin_Out = valOut[9].Split(']')[0].Contains('~') ? valOut[9].Split(']')[0].Split('~')[0] : valOut[9].Split(']')[0];
                            string destinations_Out = valOut[10].Split(']')[0].Contains('~') ? valOut[10].Split(']')[0].Split('~')[valOut[10].Split(']')[0].Split('~').Length - 1] : valOut[10].Split(']')[0];

                            string origin_Ret = valOut[9].Split(']')[1].Contains('~') ? valOut[9].Split(']')[1].Split('~')[0] : valOut[9].Split(']')[1];
                            string destinations_Ret = valOut[10].Split(']')[1].Contains('~') ? valOut[10].Split(']')[1].Split('~')[valOut[10].Split(']')[1].Split('~').Length - 1] : valOut[10].Split(']')[1];

                            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Outward", origin_Out, true);
                            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Outward", destinations_Out, true);

                            _scrapeInfo.InsertVariable("SCR_ORI_SHORT_NAME_Return", origin_Ret, true);
                            _scrapeInfo.InsertVariable("SCR_DES_SHORT_NAME_Return", destinations_Ret, true);

                            _scrapeInfo.InsertVariable("SessionIdentifier", keys[0], true);
                            _scrapeInfo.InsertVariable("GroupedFlightResultIdentifier", keys[1], true);
                            _scrapeInfo.InsertVariable("SelectedGroupedFlightOptionIdentifier", keys[2], true);
                            _scrapeInfo.InsertVariable("SelectedLegOptionsIdentifiers", "\"" + keys[3].Replace("~", "\",\"") + "\"", true);

                            #endregion
                        }

                        _scrapeInfo.InsertVariable("RR search", "true", true);

                        SetStartTime();

                        Airtrade.AitradeMain am = new Airtrade.AitradeMain();
                        tbExtended.Text = am.Aitrade(ref _scrapeInfo, ref _foundInfo, _webPage);

                        SetEndTime();

                        Session["SelectedFlightResponse"] = tbExtended.Text;

                        Newtonsoft.Json.Linq.JObject jObj = Airtrade.General.parseJSON(tbExtended.Text);


                    }
                    catch (Exception ex)
                    {
                        phError.Visible = true;

                        litError.Text = ex.Message;
                    }

                }
            }
        }

        protected void btnSummary_Click(object sender, EventArgs e)
        {
            BuildSummary();
            ClientScript.RegisterStartupScript(typeof(string), "toggleSummary", "toggleSummary();", true);

        }

        private bool checkAvailResponse(Newtonsoft.Json.Linq.JObject jObj)
        {
            bool isOK = false;

            if (cbOneWay.Checked)
            {
                if (string.IsNullOrEmpty(hOutwardID.Value)) return false;

                string[] valOut = hOutwardID.Value.Split(',');
                if (valOut.Length < 1) return false;

                string[] carrierCodes = valOut[1].Split('~');
                string[] flightNumbers = valOut[2].Split('~');
                string[] origins = valOut[9].Split('~');
                string[] destinations = valOut[10].Split('~');
                string[] depTimes = valOut[3].Split('~');
                string[] arrTimes = valOut[4].Split('~');

                string deptime = depTimes[0].Split('T')[1];
                string arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                DateTime depDay = DateTime.ParseExact(depTimes[0].Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DateTime arrDay = DateTime.ParseExact(arrTimes[0].Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                Newtonsoft.Json.Linq.JArray SelectedLegs = (Newtonsoft.Json.Linq.JArray)jObj.SelectToken("$..SelectedLegs");
                Newtonsoft.Json.Linq.JArray outboundLeg = (Newtonsoft.Json.Linq.JArray)SelectedLegs[0]["Segments"];

                string depNodeOut = Airtrade.General.jSonDate(outboundLeg[0]["DepartureDate"]) + " " + outboundLeg[0]["DepartureTime"].ToString();
                string arrNodeOut = Airtrade.General.jSonDate(outboundLeg[outboundLeg.Count - 1]["ArrivalDate"]) + " " + outboundLeg[outboundLeg.Count - 1]["ArrivalTime"].ToString();

                DateTime deptimeNode = DateTime.ParseExact(depNodeOut, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                DateTime arrtimeNode = DateTime.ParseExact(arrNodeOut, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                TimeSpan depDiff = depDay - deptimeNode;
                TimeSpan arrDiff = arrDay - arrtimeNode;

                bool depDiffOK = Math.Abs(depDiff.TotalMinutes) <= 5;
                bool arrDiffOK = Math.Abs(arrDiff.TotalMinutes) <= 5;

                isOK = depDiffOK && arrDiffOK;

            }
            else
            {
                if (!string.IsNullOrEmpty(hOutwardSelected.Value) && !string.IsNullOrEmpty(hReturnSelected.Value))
                {
                    string[] valOut = hOutwardSelected.Value.Split(',');
                    if (valOut.Length < 1) return false;

                    string[] valRet = hReturnSelected.Value.Split(',');
                    if (valRet.Length < 1) return false;

                    string[] carrierCodes = valOut[1].Split('~');
                    string[] flightNumbers = valOut[2].Split('~');
                    string[] origins = valOut[9].Split('~');
                    string[] destinations = valOut[10].Split('~');
                    string[] depTimes = valOut[3].Split('~');
                    string[] arrTimes = valOut[4].Split('~');

                    string deptime = depTimes[0].Split('T')[1];
                    string arrtime = arrTimes[arrTimes.Length - 1].Split('T')[1];
                    DateTime depDay_Out = DateTime.ParseExact(depTimes[0].Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrDay_Out = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    carrierCodes = valRet[1].Split('~');
                    flightNumbers = valRet[2].Split('~');
                    origins = valRet[9].Split('~');
                    destinations = valRet[10].Split('~');
                    depTimes = valRet[3].Split('~');
                    arrTimes = valRet[4].Split('~');

                    DateTime depDay_Ret = DateTime.ParseExact(depTimes[0].Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrDay_Ret = DateTime.ParseExact(arrTimes[arrTimes.Length - 1].Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    Newtonsoft.Json.Linq.JArray SelectedLegs = (Newtonsoft.Json.Linq.JArray)jObj.SelectToken("$..SelectedLegs");
                    Newtonsoft.Json.Linq.JArray outboundLeg = (Newtonsoft.Json.Linq.JArray)SelectedLegs[0]["Segments"];
                    Newtonsoft.Json.Linq.JArray inboundLeg = (Newtonsoft.Json.Linq.JArray)SelectedLegs[1]["Segments"];

                    DateTime deptimeNode_Out = DateTime.ParseExact(Airtrade.General.jSonDate(outboundLeg[0]["DepartureDate"]) + " " + outboundLeg[0]["DepartureTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrtimeNode_Out = DateTime.ParseExact(Airtrade.General.jSonDate(outboundLeg[outboundLeg.Count - 1]["ArrivalDate"]) + " " + outboundLeg[outboundLeg.Count - 1]["ArrivalTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime deptimeNode_Ret = DateTime.ParseExact(Airtrade.General.jSonDate(inboundLeg[0]["DepartureDate"]) + " " + inboundLeg[0]["DepartureTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrtimeNode_Ret = DateTime.ParseExact(Airtrade.General.jSonDate(inboundLeg[inboundLeg.Count - 1]["ArrivalDate"]) + " " + inboundLeg[inboundLeg.Count - 1]["ArrivalTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    TimeSpan depDiffOut = depDay_Out - deptimeNode_Out;
                    TimeSpan arrDiffOut = arrDay_Out - arrtimeNode_Out;
                    TimeSpan depDiffRet = depDay_Ret - deptimeNode_Ret;
                    TimeSpan arrDiffRet = arrDay_Ret - arrtimeNode_Ret;

                    bool depDiffOKOut = Math.Abs(depDiffOut.TotalMinutes) <= 5;
                    bool arrDiffOKOut = Math.Abs(arrDiffOut.TotalMinutes) <= 5;
                    bool depDiffOKRet = Math.Abs(depDiffRet.TotalMinutes) <= 5;
                    bool arrDiffOKRet = Math.Abs(arrDiffRet.TotalMinutes) <= 5;

                    return depDiffOKOut && arrDiffOKOut && depDiffOKRet && arrDiffOKRet;

                }
                else if (!string.IsNullOrEmpty(hOutwardID.Value))
                {
                    string[] valOut = hOutwardID.Value.Split(',');
                    if (valOut.Length < 1) return false;

                    string depTimeOut = valOut[3].Split(']')[0].Contains('~') ? valOut[3].Split(']')[0].Split('~')[0] : valOut[3].Split(']')[0];
                    string depTime_out = depTimeOut.Split('T')[1];
                    string depTimeRet = valOut[3].Split(']')[1].Contains('~') ? valOut[3].Split(']')[1].Split('~')[0] : valOut[3].Split(']')[1];
                    string depTime_Ret = depTimeRet.Split('T')[1];

                    string arrTimeOut = valOut[4].Split(']')[0].Contains('~') ? valOut[4].Split(']')[0].Split('~')[valOut[4].Split(']')[0].Split('~').Length - 1] : valOut[4].Split(']')[0];
                    string arrTime_Out = arrTimeOut.Split('T')[1];
                    string arrTimeRet = valOut[4].Split(']')[1].Contains('~') ? valOut[4].Split(']')[1].Split('~')[valOut[4].Split(']')[1].Split('~').Length - 1] : valOut[4].Split(']')[1];
                    string arrTime_Ret = arrTimeRet.Split('T')[1];

                    DateTime depDay_Out = DateTime.ParseExact(depTimeOut.Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime depDay_Ret = DateTime.ParseExact(depTimeRet.Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrDay_Out = DateTime.ParseExact(arrTimeOut.Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrDay_Ret = DateTime.ParseExact(arrTimeRet.Replace("T", " "), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    Newtonsoft.Json.Linq.JArray SelectedLegs = (Newtonsoft.Json.Linq.JArray)jObj.SelectToken("$..SelectedLegs");
                    Newtonsoft.Json.Linq.JArray outboundLeg = (Newtonsoft.Json.Linq.JArray)SelectedLegs[0]["Segments"];
                    Newtonsoft.Json.Linq.JArray inboundLeg = (Newtonsoft.Json.Linq.JArray)SelectedLegs[1]["Segments"];

                    DateTime deptimeNode_Out = DateTime.ParseExact(Airtrade.General.jSonDate(outboundLeg[0]["DepartureDate"]) + " " + outboundLeg[0]["DepartureTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrtimeNode_Out = DateTime.ParseExact(Airtrade.General.jSonDate(outboundLeg[outboundLeg.Count - 1]["ArrivalDate"]) + " " + outboundLeg[outboundLeg.Count - 1]["ArrivalTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime deptimeNode_Ret = DateTime.ParseExact(Airtrade.General.jSonDate(inboundLeg[0]["DepartureDate"]) + " " + inboundLeg[0]["DepartureTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    DateTime arrtimeNode_Ret = DateTime.ParseExact(Airtrade.General.jSonDate(inboundLeg[inboundLeg.Count - 1]["ArrivalDate"]) + " " + inboundLeg[inboundLeg.Count - 1]["ArrivalTime"].ToString(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

                    TimeSpan depDiffOut = depDay_Out - deptimeNode_Out;
                    TimeSpan arrDiffOut = arrDay_Out - arrtimeNode_Out;
                    TimeSpan depDiffRet = depDay_Ret - deptimeNode_Ret;
                    TimeSpan arrDiffRet = arrDay_Ret - arrtimeNode_Ret;

                    bool depDiffOKOut = Math.Abs(depDiffOut.TotalMinutes) <= 5;
                    bool arrDiffOKOut = Math.Abs(arrDiffOut.TotalMinutes) <= 5;
                    bool depDiffOKRet = Math.Abs(depDiffRet.TotalMinutes) <= 5;
                    bool arrDiffOKRet = Math.Abs(arrDiffRet.TotalMinutes) <= 5;

                    return depDiffOKOut && arrDiffOKOut && depDiffOKRet && arrDiffOKRet;

                }
            }

            return isOK;
        }


        private void SetStartTime()
        {
            lblTime.Visible = false;
            lblTime.Text = string.Empty;
            _timeBeforeRQ = DateTime.Now;
        }

        private void SetEndTime()
        {
            _timeAfterRQ = DateTime.Now;
            if (!_timeBeforeRQ.Equals(DateTime.MinValue) && !_timeAfterRQ.Equals(DateTime.MinValue))
            {
                TimeSpan Diff = _timeAfterRQ - _timeBeforeRQ;
                _timelapse = Diff.ToString();

                lblTime.Text = "Request sent : " + _timelapse;
                lblTime.Visible = true;
            }
        }


        protected void btnEncryptCC_Click(object sender, EventArgs e)
        {
            if (ddlCardNumbers.SelectedIndex > 0 && !string.IsNullOrEmpty(tbCCV.Text) && ddlValidMonth.SelectedIndex > 0 && ddlValidYear.SelectedIndex > 0)
            {
                InitScrape();

                _scrapeInfo.InsertVariable("action", "EncryptCC", true);
                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_NUMBER", ddlCardNumbers.SelectedValue, true);
                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VERIFICATION_NO", tbCCV.Text, true);
                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_HOLDER", "Anton Adams", true);
                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_MONTH", ddlValidMonth.SelectedValue, true);
                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_YEAR_4D", ddlValidYear.SelectedValue, true);
                _scrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_YEARD", ddlValidYear.SelectedValue.Substring(2, 2), true);
                _scrapeInfo.InsertVariable("WS_ENCRYPT_CER_LOC", System.Configuration.ConfigurationManager.AppSettings["Encrypt.Cer"], true);
                if (cbOverWriteWithContent.Checked)
                {
                    _scrapeInfo.InsertVariable("WS_ENCRYPT_CER_LOC", "_OVERWRITE_", true);
                    _scrapeInfo.InsertVariable("WS_ENCRYPT_CER_VAL", tbReq.Text, true);
                }

                SunExpress.SunExpressMain se = new SunExpress.SunExpressMain();
                NameValueCollection poScrapeData = new NameValueCollection();
                poScrapeData.Add("action", "EncryptCC");
                tbExtended.Text = se.GoNext(poScrapeData, ref _scrapeInfo, ref _foundInfo, _webPage);

            }
        }

        protected void btnEncryptThis_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbExtended.Text))
            {
                if (chkElsyEnc.Checked)
                {
                    tbLFR.Text = new EARoot.Root().EncryptString(tbExtended.Text);
                }
                else
                {
                    InitScrape();
                    _scrapeInfo.InsertVariable("WS_ENCRYPT_STR", tbExtended.Text, true);
                    _scrapeInfo.InsertVariable("WS_ENCRYPT_CER_LOC", System.Configuration.ConfigurationManager.AppSettings["Encrypt.Cer"], true);


                    SunExpress.SunExpressMain se = new SunExpress.SunExpressMain();
                    NameValueCollection poScrapeData = new NameValueCollection();
                    poScrapeData.Add("action", "EncryptItem");
                    tbLFR.Text = se.GoNext(poScrapeData, ref _scrapeInfo, ref _foundInfo, _webPage);
                }
            }
        }

        protected void btnDecryptThis_Click(object sender, EventArgs e)
        {
            if (chkElsyEnc.Checked)
            {
                if (!string.IsNullOrEmpty(tbExtended.Text))
                    tbLFR.Text = new EARoot.Root().DecryptString(tbExtended.Text);
            }
            else
            {
                string delimiter = "%~~`%~~~~~~~%^**(%$#%";
                if (!string.IsNullOrEmpty(tbExtended.Text) && tbExtended.Text.Contains(delimiter))
                {
                    InitScrape();
                    _scrapeInfo.InsertVariable("WS_ENCRYPT_STR", tbExtended.Text, true);
                    _scrapeInfo.InsertVariable("WS_ENCRYPT_CER_LOC", System.Configuration.ConfigurationManager.AppSettings["Encrypt.Cer"], true);


                    SunExpress.SunExpressMain se = new SunExpress.SunExpressMain();
                    NameValueCollection poScrapeData = new NameValueCollection();
                    poScrapeData.Add("action", "DecryptItem");
                    tbLFR.Text = se.GoNext(poScrapeData, ref _scrapeInfo, ref _foundInfo, _webPage);
                }
            }
        }

        protected void btnAmadeusConfig_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbExtended.Text))
            {
                InitScrape();

                AmadeusDll.v1.Amadeus am = new AmadeusDll.v1.Amadeus();
                am.JSON = tbExtended.Text;

                AmadeusDll.v1.AmadeusConfig amc = am.AmadeusConfigItem;

                tbLFR.Text = General.SerializeObject(amc);

                string segmentName = "TK";
                if (amc.TK != null && !string.IsNullOrEmpty(amc.TK))
                {
                    segmentName = amc.TK;
                }

                tbLFR.Text += "\n\rTK: " + segmentName;

                string date = DateTime.Now.AddHours(24).ToString("ddMMyy");

                //Added by JvL on 20170301: Influence TK Date value
                //Overwrite the date to the current date when the TK value = TKTL

                if (amc.dateNow)
                    date = DateTime.Now.ToString("ddMMyy");

                tbLFR.Text += "\n\rdate: " + date;

                if (amc.FP != null && amc.FP.Count > 0)
                {
                    foreach (AmadeusDll.v1.FP loFP in amc.FP)
                    {
                        tbLFR.Text += "\n\rNew FP:\r";

                        string ccNr = new EARoot.Root().DecryptString(loFP.accountNumber);
                        if (loFP.creditCardScrambled)
                        {
                            string pre = "";
                            for (int i = 0; i < ccNr.Length - 4; i++)
                                pre += "x";
                            ccNr = pre + ccNr.Substring(ccNr.Length - 4, 4);
                        }

                        tbLFR.Text += string.Format("accountNumber: {0}\r", ccNr);
                        tbLFR.Text += string.Format("creditCardCode: {0}\r", loFP.creditCardCode);
                        tbLFR.Text += string.Format("expiryDate: {0}\r", loFP.expiryDate);
                        tbLFR.Text += string.Format("identification: {0}\r", loFP.identification);
                        tbLFR.Text += string.Format("fopSequenceNumber: {0}\r", loFP.fopSequenceNumber);
                        tbLFR.Text += string.Format("cvData: {0}\r", loFP.cvData);

                    }
                }
            }
        }

        protected void btnSmartwings_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbExtended.Text))
            {
                string b = tbExtended.Text;

                StringBuilder js = new StringBuilder();
                for (var i = 0; i < b.Length; i += 2)
                    js.Append(char.ConvertFromUtf32(int.Parse(b.Substring(i, 2), System.Globalization.NumberStyles.HexNumber)).ToString());


                tbLFR.Text = js.ToString();

                
            }
            else
                tbLFR.Text = DecrB();
        }

        protected void btnRecaptcha_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbExtended.Text) && !string.IsNullOrEmpty(tbRes.Text))
            {
                try
                {
                    System.Net.ServicePointManager.Expect100Continue = false;

                    System.Net.HttpWebRequest HttpReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://2captcha.com/in.php");
                    HttpReq.Method = "POST";
                    HttpReq.KeepAlive = true;
                    HttpReq.ContentType = "application/x-www-form-urlencoded";
                    HttpReq.Timeout = 30000;

                    string SendString = String.Format("key={0}&method=userrecaptcha&googlekey={1}&pageurl={2}", System.Configuration.ConfigurationManager.AppSettings["2CaptchaAPI_Key"], tbExtended.Text, tbRes.Text);

                    System.IO.StreamWriter sOut = new System.IO.StreamWriter(HttpReq.GetRequestStream());
                    sOut.Write(SendString);
                    sOut.Close();

                    TimeSpan timeDiff;
                    DateTime startTime = DateTime.Now;

                    //Receive Request
                    System.Net.HttpWebResponse HttpResp = (System.Net.HttpWebResponse)HttpReq.GetResponse();

                    System.IO.StreamReader sReader = new System.IO.StreamReader(HttpResp.GetResponseStream());

                    string TheContent = sReader.ReadToEnd();

                    HttpResp.Close();

                    if (TheContent.Contains("OK|"))
                    {
                        tbLFR.Text = TheContent;
                        string[] items = TheContent.Split('|');

                        DateTime firstResponse = DateTime.Now;

                        timeDiff = firstResponse - startTime;
                        tbLFR.Text += string.Format("\nTime after 1st response (in sec): {0}", timeDiff.TotalSeconds);

                        string URL = String.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}", System.Configuration.ConfigurationManager.AppSettings["2CaptchaAPI_Key"], items[1]);

                        int retry = 0;
                        bool found = false;

                        

                        while (retry < 10 && !found)
                        {
                            HttpReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URL);
                            HttpReq.Method = "GET";
                            HttpReq.KeepAlive = true;
                            HttpReq.ContentType = "application/x-www-form-urlencoded";

                            //Receive Request
                            HttpResp = (System.Net.HttpWebResponse)HttpReq.GetResponse();

                            sReader = new System.IO.StreamReader(HttpResp.GetResponseStream());
                            TheContent = sReader.ReadToEnd();

                            found = TheContent.Contains("OK|");

                            retry++;

                            if (!found)
                                System.Threading.Thread.Sleep(5000);

                        }

                        DateTime secondResponse = DateTime.Now;

                        timeDiff = secondResponse - startTime;

                        tbLFR.Text += string.Format("\nTime after 2nd response (in sec): {0}", timeDiff.TotalSeconds);

                        

                        HttpResp.Close();

                        tbLFR.Text += "\n==>\n" + TheContent;
                    }
                    else
                        tbLFR.Text = "Error";

                    /*

                    var request = (HttpWebRequest)WebRequest.Create("http://2captcha.com/in.php");

                    var postData = "key=2captcha API KEY&method=userrecaptcha&googlekey=GOOGLE KEY";//&pageurl=yourpageurl"; ;
                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    //  GET
                    if (responseString.Contains("OK|"))
                    {
                        return responseString.Substring(0, 3);
                    }
                    else
                    {
                        return "Error";
                    }
                    */
                }
                catch (Exception eX)
                {

                    tbLFR.Text = eX.Message;
                }
                
            }
        }

                
        private string DecrB()
        {
            string b = "7472797B766172207868723B76617220743D6E6577204461746528292E67657454696D6528293B766172207374617475733D227374617274223B7661722074696D696E673D6E65772041727261792833293B77696E646F772E6F6E756E6C6F61643D66756E6374696F6E28297B74696D696E675B325D3D22723A222B286E6577204461746528292E67657454696D6528292D74293B646F63756D656E742E637265617465456C656D656E742822696D6722292E7372633D222F5F496E63617073756C615F5265736F757263653F4553324C555243543D363726743D373826643D222B656E636F6465555249436F6D706F6E656E74287374617475732B222028222B74696D696E672E6A6F696E28292B222922297D3B69662877696E646F772E584D4C4874747052657175657374297B7868723D6E657720584D4C48747470526571756573747D656C73657B7868723D6E657720416374697665584F626A65637428224D6963726F736F66742E584D4C4854545022297D7868722E6F6E726561647973746174656368616E67653D66756E6374696F6E28297B737769746368287868722E72656164795374617465297B6361736520303A7374617475733D6E6577204461746528292E67657454696D6528292D742B223A2072657175657374206E6F7420696E697469616C697A656420223B627265616B3B6361736520313A7374617475733D6E6577204461746528292E67657454696D6528292D742B223A2073657276657220636F6E6E656374696F6E2065737461626C6973686564223B627265616B3B6361736520323A7374617475733D6E6577204461746528292E67657454696D6528292D742B223A2072657175657374207265636569766564223B627265616B3B6361736520333A7374617475733D6E6577204461746528292E67657454696D6528292D742B223A2070726F63657373696E672072657175657374223B627265616B3B6361736520343A7374617475733D22636F6D706C657465223B74696D696E675B315D3D22633A222B286E6577204461746528292E67657454696D6528292D74293B6966287868722E7374617475733D3D323030297B706172656E742E6C6F636174696F6E2E72656C6F616428297D627265616B7D7D3B74696D696E675B305D3D22733A222B286E6577204461746528292E67657454696D6528292D74293B7868722E6F70656E2822474554222C222F5F496E63617073756C615F5265736F757263653F535748414E45444C3D333233333633303330313632393332393438312C31303037303138383530383038363839333731322C313834353433383433343235313537393435322C313739333231222C66616C7365293B7868722E73656E64286E756C6C297D63617463682863297B7374617475732B3D6E6577204461746528292E67657454696D6528292D742B2220696E6361705F6578633A20222B633B646F63756D656E742E637265617465456C656D656E742822696D6722292E7372633D222F5F496E63617073756C615F5265736F757263653F4553324C555243543D363726743D373826643D222B656E636F6465555249436F6D706F6E656E74287374617475732B222028222B74696D696E672E6A6F696E28292B222922297D3B";

            StringBuilder js = new StringBuilder();
            for (var i = 0; i < b.Length; i += 2)
                js.Append(char.ConvertFromUtf32(int.Parse(b.Substring(i, 2), System.Globalization.NumberStyles.HexNumber)).ToString());

            return js.ToString();
        }

        protected void btmJSONPathCmd_Click(object sender, EventArgs e)
        {
            FlightClient.App_Backend.JSONPath JPATH = new App_Backend.JSONPath();
            JPATH.JSONStr = tbReq.Text;
            //JPATH.JSONPathCmd = tbRes.Text;

            List<string> cmdList = new List<string>();

            if (tbRes.Text.Contains("|"))
            {
                string[] cmds = tbRes.Text.Split('|');
                for (int i = 0; i < cmds.Length; i++)
                    cmdList.Add(cmds[i]);
            }
            else
                cmdList.Add(tbRes.Text);

            JPATH.JSONPathCmd = cmdList.ToArray();

            tbExtended.Text = JPATH.Result();

            if (!string.IsNullOrEmpty(tbExtended.Text))
                ClientScript.RegisterStartupScript(typeof(string), "JSONBeuatify", string.Format("beautifyJSON('{0}')", tbExtended.ClientID), true);


        }

        protected void btnCheckMoney_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbMoneyValue.Text))
            {
                Double totalPrice = Convert.ToDouble(string.Format("{0:0.00}", tbMoneyValue.Text.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture));
                string price = totalPrice.ToString(); //.Replace(",", ".");
                lblMoneyTrans.Text = price;
            }
        }
    }
}