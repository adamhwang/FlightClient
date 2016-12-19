using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using Newtonsoft.Json.Linq;

namespace FlightClient
{
    public partial class Outward : System.Web.UI.Page
    {
        protected int ctr = 0;
        protected int ctr2 = 0;

        private int adt = 1;
        private int chd = 0;
        private int inf = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["A"]))
                adt = Convert.ToInt32(Request.QueryString["A"]);

            if (!string.IsNullOrEmpty(Request.QueryString["C"]))
                chd = Convert.ToInt32(Request.QueryString["C"]);

            if (!string.IsNullOrEmpty(Request.QueryString["I"]))
                inf = Convert.ToInt32(Request.QueryString["I"]);

            FillContent();

        }

        private void FillContent()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("FlightID");
            dt.Columns.Add("Carrier");
            dt.Columns.Add("FlightNr");
            dt.Columns.Add("Depart");
            dt.Columns.Add("Arrive");
            dt.Columns.Add("DepTime");
            dt.Columns.Add("ArrTime");
            dt.Columns.Add("Total");
            dt.Columns.Add("Adult");
            dt.Columns.Add("AdultTaxIncl");
            dt.Columns.Add("Child");
            dt.Columns.Add("ChildTaxIncl");
            dt.Columns.Add("Infant");
            dt.Columns.Add("InfantTaxIncl");
            dt.Columns.Add("Taxes");
            dt.Columns.Add("Baggage");
            dt.Columns.Add("Fees");
            dt.Columns.Add("IsStopOver");
            dt.Columns.Add("LegArrTime");
            dt.Columns.Add("LegDepTime");
            dt.Columns.Add("LegFlightNr");
            dt.Columns.Add("StopOverAirport");
            dt.Columns.Add("JScript");
            dt.Columns.Add("GroupedFlightOption");
            dt.Columns.Add("IsRoundtrip");



            JObject jObj = null;

            if (Session["AvailResponse"] != null)
                jObj = Airtrade.General.parseJSON(Session["AvailResponse"].ToString());

            int refCtr = 0;

            //Response of SF
            if (jObj != null && jObj["_embedded"] != null && jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
            {
                #region SF request
                string pagingResultlistHref = jObj.SelectToken("$._links.paging:resultlist.href").ToString();
                string SessionIdentifier = pagingResultlistHref.Substring(1, pagingResultlistHref.IndexOf("/FlightOptions") - 1);

                string GroupedFlightResultIdentifier = jObj["Identifier"].ToString();

                JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                for (int i = 0; i < groupedFlightOptionsList.Count; i++)
                {
                    JObject groupedFlightOption = (JObject)groupedFlightOptionsList[i];

                    string SelectedGroupedFlightOptionIdentifier = groupedFlightOption["Identifier"].ToString();
                    string airlineCode = groupedFlightOption["Airline"]["Code"].ToString();
                    string airlineDescr = groupedFlightOption["Airline"]["DisplayName"].ToString();
                    string totalPrice = groupedFlightOption["PriceBreakdown"]["Total"].ToString().Replace(',', '.');

                    string adultPrice = groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Adult')].TotalPerPax").ToString().Replace(',', '.');
                    string adultPriceNoTax = groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Adult')].PriceElements[?(@.Description=='Ticket price')].Amount").ToString().Replace(',', '.');

                    string childPrice = "0";
                    string childPriceNoTax = "0";
                    if (groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Child')]") != null)
                    {
                        childPrice = groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Child')].TotalPerPax").ToString().Replace(',', '.');
                        childPriceNoTax = groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Child')].PriceElements[?(@.Description=='Ticket price')].Amount").ToString().Replace(',', '.');
                    }

                    string infantPrice = "0";
                    string infantPriceNoTax = "0";
                    if (groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Infant')]") != null)
                    {
                        infantPrice = groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Infant')].TotalPerPax").ToString().Replace(',', '.');
                        infantPriceNoTax = groupedFlightOption.SelectToken("$.PriceBreakdown.TicketPrices[?(@.PaxType=='Infant')].PriceElements[?(@.Description=='Ticket price')].Amount").ToString().Replace(',', '.');
                    }

                    double totalTaxes = (adt * ((Convert.ToDouble(adultPrice)) - (Convert.ToDouble(adultPriceNoTax)))) + (chd * ((Convert.ToDouble(childPrice)) - (Convert.ToDouble(childPriceNoTax)))) + (inf * ((Convert.ToDouble(infantPrice)) - (Convert.ToDouble(infantPriceNoTax))));

                    bool isOneWay = groupedFlightOption["LegOptionInfoLists"] != null && (((JArray)groupedFlightOption["LegOptionInfoLists"]).Count.Equals(1));
                    //For oneways

                    if (isOneWay)
                    {
                        #region Oneways
                        if (groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"] != null && ((JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"]).Count > 0)
                        {
                            JArray legOptionInfoListsOutbound = (JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"];
                            for (int j = 0; j < legOptionInfoListsOutbound.Count; j++)
                            {
                                JObject legOptionInfo = (JObject)legOptionInfoListsOutbound[j];
                                int nrOfStops = (int)legOptionInfo["NumberOfStops"];
                                if (nrOfStops > 1) continue;

                                string legOptionInfoListsOutboundIdentifier = legOptionInfo["Identifier"].ToString();
                                string flightID = SessionIdentifier + "|" + GroupedFlightResultIdentifier + "|" + SelectedGroupedFlightOptionIdentifier + "|" + legOptionInfoListsOutboundIdentifier;

                                string flightNr = legOptionInfo["FlightNumber"].ToString();
                                string departCode = legOptionInfo["DepartureAirport"]["Code"].ToString();
                                string departDescr = legOptionInfo["DepartureAirport"]["DisplayNameShort"].ToString();
                                string arriveCode = legOptionInfo["ArrivalAirport"]["Code"].ToString();
                                string arriveDescr = legOptionInfo["ArrivalAirport"]["DisplayNameShort"].ToString();
                                string departureDate = DateTime.Parse(legOptionInfo["DepartureDate"].ToString()).ToString("yyyy-MM-dd");
                                string departureTime = legOptionInfo["DepartureTime"].ToString();
                                string arrivalDate = DateTime.Parse(legOptionInfo["ArrivalDate"].ToString()).ToString("yyyy-MM-dd");
                                string arrivalTime = legOptionInfo["ArrivalTime"].ToString();

                                string legArrTime = string.Empty;
                                string legDepTime = string.Empty;
                                string legFlightNr = string.Empty;

                                bool isStopOver = nrOfStops > 0;
                                string stopOverAirport = isStopOver ? legOptionInfo["StopOvers"][0]["Airport"]["Code"].ToString() : "";

                                if (isStopOver)
                                {
                                    legArrTime = legOptionInfo.SelectToken("$.Segments[0].ArrivalTime") != null ? legOptionInfo.SelectToken("$.Segments[0].ArrivalTime").ToString() : string.Empty;
                                    legFlightNr = legOptionInfo.SelectToken("$.Segments[1].FlightNumber") != null ? legOptionInfo.SelectToken("$.Segments[1].FlightNumber").ToString() : string.Empty;
                                    legDepTime = legOptionInfo.SelectToken("$.Segments[1].DepartureTime") != null ? legOptionInfo.SelectToken("$.Segments[1].DepartureTime").ToString() : string.Empty;
                                }

                                string jscript = "SelectOutwardFlightAr(" + refCtr.ToString() + ");";

                                dt.Rows.Add(new object[] { flightID, 
                                                               //string.Format("{0} - ({1})", airlineDescr, airlineCode), 
                                                               airlineCode,
                                                               flightNr, 
                                                               //string.Format("{0} - ({1})", departDescr,  departCode),
                                                               departCode,
                                                               //string.Format("{0} - ({1})", arriveDescr,  arriveCode),
                                                               arriveCode,
                                                               string.Format("{0}T{1}", departureDate, departureTime),
                                                               string.Format("{0}T{1}", arrivalDate, arrivalTime),
                                                               totalPrice,
                                                               adultPriceNoTax,
                                                               adultPrice,
                                                               childPriceNoTax,
                                                               childPrice,
                                                               infantPriceNoTax,
                                                               infantPrice,
                                                               totalTaxes.ToString(),
                                                               "",
                                                               "",
                                                               isStopOver.ToString(),
                                                               legArrTime,
                                                               legDepTime,
                                                               legFlightNr,
                                                               stopOverAirport,
                                                               jscript,
                                                               System.Web.HttpUtility.UrlEncode(groupedFlightOption.ToString()),
                                                               "False"});

                                refCtr++;


                            }
                        }

                        if (dt.Rows.Count > 0)
                        {
                            repOutboundFlights.DataSource = dt;
                            repOutboundFlights.DataBind();

                            repOutItems.DataSource = dt;
                            repOutItems.DataBind();

                        }

                        #endregion
                    }
                    //For RR
                    else
                    {
                        #region RR

                        if (groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"] != null && ((JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"]).Count > 0 && groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"] != null && ((JArray)groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"]).Count > 0)
                        {
                            JArray legOptionInfoListsOutbound = (JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"];
                            JArray legOptionInfoListsInbound = (JArray)groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"];

                            for (int j = 0; j < legOptionInfoListsOutbound.Count; j++)
                            {
                                JObject legOptionInfoOut = (JObject)legOptionInfoListsOutbound[j];
                                int nrOfStopsOut = (int)legOptionInfoOut["NumberOfStops"];
                                if (nrOfStopsOut > 1) continue;

                                for (int t = 0; t < legOptionInfoListsInbound.Count; t++)
                                {
                                    JObject legOptionInfoRet = (JObject)legOptionInfoListsInbound[t];
                                    int nrOfStopsRet = (int)legOptionInfoRet["NumberOfStops"];
                                    if (nrOfStopsRet > 1) continue;

                                    string legOptionInfoListsOutboundIdentifier = legOptionInfoOut["Identifier"].ToString();
                                    string legOptionInfoListsInboundIdentifier = legOptionInfoRet["Identifier"].ToString();
                                    string flightID = SessionIdentifier + "|" + GroupedFlightResultIdentifier + "|" + SelectedGroupedFlightOptionIdentifier + "|" + legOptionInfoListsOutboundIdentifier + "~" + legOptionInfoListsInboundIdentifier;

                                    string flightNr = legOptionInfoOut["FlightNumber"].ToString() + "]" + legOptionInfoRet["FlightNumber"].ToString();
                                    string departCode = legOptionInfoOut["DepartureAirport"]["Code"].ToString() + "]" + legOptionInfoRet["DepartureAirport"]["Code"].ToString();
                                    string departDescr = legOptionInfoOut["DepartureAirport"]["DisplayNameShort"].ToString() + "]" + legOptionInfoRet["DepartureAirport"]["DisplayNameShort"].ToString();
                                    string arriveCode = legOptionInfoOut["ArrivalAirport"]["Code"].ToString() + "]" + legOptionInfoRet["ArrivalAirport"]["Code"].ToString();
                                    string arriveDescr = legOptionInfoOut["ArrivalAirport"]["DisplayNameShort"].ToString() + "]" + legOptionInfoRet["ArrivalAirport"]["DisplayNameShort"].ToString();
                                    string departureDateOut = DateTime.Parse(legOptionInfoOut["DepartureDate"].ToString()).ToString("yyyy-MM-dd");
                                    string departureDateRet = DateTime.Parse(legOptionInfoRet["DepartureDate"].ToString()).ToString("yyyy-MM-dd");
                                    string departureTimeOut = legOptionInfoOut["DepartureTime"].ToString();
                                    string departureTimeret = legOptionInfoRet["DepartureTime"].ToString();
                                    string arrivalDateOut = DateTime.Parse(legOptionInfoOut["ArrivalDate"].ToString()).ToString("yyyy-MM-dd");
                                    string arrivalDateRet = DateTime.Parse(legOptionInfoRet["ArrivalDate"].ToString()).ToString("yyyy-MM-dd");
                                    string arrivalTimeOut = legOptionInfoOut["ArrivalTime"].ToString();
                                    string arrivalTimeRet = legOptionInfoRet["ArrivalTime"].ToString();

                                    bool isStopOverOut = nrOfStopsOut > 0;
                                    string stopOverAirportOut = isStopOverOut ? legOptionInfoOut["StopOvers"][0]["Airport"]["Code"].ToString() : "";
                                    bool isStopOverRet = nrOfStopsRet > 0;
                                    string stopOverAirportRet = isStopOverRet ? legOptionInfoRet["StopOvers"][0]["Airport"]["Code"].ToString() : "";

                                    string jscript = "SelectOutwardFlightAr(" + refCtr.ToString() + ");";

                                    dt.Rows.Add(new object[] { flightID,
                                                               airlineCode,
                                                               flightNr,
                                                               departCode,
                                                               arriveCode,
                                                               string.Format("{0}T{1}]{2}T{3}", departureDateOut, departureTimeOut, departureDateRet, departureTimeret),
                                                               string.Format("{0}T{1}]{2}T{3}", arrivalDateOut, arrivalTimeOut, arrivalDateRet, arrivalTimeRet),
                                                               totalPrice,
                                                               adultPriceNoTax,
                                                               adultPrice,
                                                               childPriceNoTax,
                                                               childPrice,
                                                               infantPriceNoTax,
                                                               infantPrice,
                                                               totalTaxes.ToString(),
                                                               "",
                                                               "",
                                                               string.Format("{0}]{1}",isStopOverOut.ToString(),isStopOverRet.ToString()),
                                                               "",
                                                               "",
                                                               "",
                                                               string.Format("{0}]{1}",stopOverAirportOut, stopOverAirportRet),

                                                               jscript,
                                                               System.Web.HttpUtility.UrlEncode(groupedFlightOption.ToString()),
                                                               "False"});



                                    refCtr++;

                                }
                            }
                        }

                        if (dt.Rows.Count > 0)
                        {
                            repOutboundFlights.DataSource = dt;
                            repOutboundFlights.DataBind();

                            repOutItems.DataSource = dt;
                            repOutItems.DataBind();

                        }

                        #endregion
                    }
                }
                #endregion

                ClientScript.RegisterStartupScript(typeof(string), "CountFlights", "CountFlights();", true);
            }

            //Response of VF
            if (jObj != null  /* && jObj["_embedded"]!= null && jObj["_embedded"]["Passengers"]!=null */ && jObj["SelectedLegs"] != null && jObj.SelectToken("$..SelectedLegs..Segments") != null && jObj["PriceBreakdown"] != null)
            {

            }

        }


    }
}