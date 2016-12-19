//#define enablePriceCheck
using System;
using System.Collections.Generic;

using System.Net;
using System.IO;
using System.Text;

using Newtonsoft.Json.Linq;

using EAScrape;

namespace Airtrade
{

    //******************* Remark: enable PriceCheck before sending this file to ScrapeEngine *******************************************

    /*      11-08-2016  : JvL has created this class
     *      16-08-2016  : LEG_FCL info added to SF and SF RR
     *      18-08-2016  : removed the en-US culture; createAPIDate method added
     *      19-08-2016  : additional logging added for VF
     *      23-08-2016  : VF check without flight number just for depDateTime / arrDateTime
     *      24-08-2016  : SRF added
     *      25-08-2016  : General DateTime parse / jSonDate added for JSON date
     *      02-09-2016  : BF added
     *      06-09-2016  : BRF added
     *      20-09-2016  : Baggage request re-enabled / selected baggage added to ADT and/or CHD 
     *      11-10-2016  : Pre-zeros removed at flightnumber at all responses
     *      19-10-2016  : Added the possibility to retrieve the selected flight in BF/BRF using a SelectedFlightIdentifier
    */


    public class General
    {
        public General()
        { }

        public static JObject parseJSON(string json)
        {
            //Parse JSON with use of offset times

            JObject tmpObj = new JObject();

            if (!string.IsNullOrEmpty(json))
            {
                //Transpose JSON text to stream
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;

                //Parse stream to JObject
                using (var streamReader = new StreamReader(stream))
                {
                    Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(streamReader);
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    serializer.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset;
                    serializer.Culture = System.Globalization.CultureInfo.InvariantCulture;
                    tmpObj = serializer.Deserialize<JObject>(reader);
                }
            }

            return tmpObj;
        }

        public static string getToken(JObject x, string defaultValue, params string[] xPath)
        {
            string Value = "";
            if (x != null)
            {
                foreach (string s in xPath)
                {
                    if (x.SelectToken(s) != null)
                    {
                        Value = x.SelectToken(s).ToString();
                        break;
                    }
                }
            }

            if (Value == "")
                Value = defaultValue;

            return Value;
        }

        public static string jSonDate(object date)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(date).Replace("\"", "").Substring(0, 10);
        }
        
    }

    public class AitradeMain
    {
        public AitradeMain()
        {
            System.Net.ServicePointManager.CertificatePolicy = new WeGoLoPolicy();
        }

        public string Aitrade(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            try
            {
                AirtradeAPI api = new AirtradeAPI();

                switch (pScrapeInfo.SCR_PROCESS_NAME)
                {
                    case "SearchFlights":
                        return api.SearchFlights(ref pScrapeInfo, ref pFoundInfo, poWebPage.WEB_TIMEOUT);

                    case "BookFlight":
                        return api.BookFlight(ref pScrapeInfo, ref pFoundInfo, poWebPage.WEB_TIMEOUT);

                    case "SearchReturnFlight":
                        return api.SearchReturnFlight(ref pScrapeInfo, ref pFoundInfo, poWebPage.WEB_TIMEOUT);

                    case "BookReturnFlight":
                        return api.BookReturnFlight(ref pScrapeInfo, ref pFoundInfo, poWebPage.WEB_TIMEOUT);

                    default:
                        return "<b>ERROR - (<font color=\"#CC0000\">" + pScrapeInfo.SCR_PROCESS_NAME + "</font>) not implemented!</b>";

                }

                //Trace - Activate this when using it in the SE
#if enablePriceCheck
                WebPage.TraceWebStart(ref pScrapeInfo);
#endif


            }
            catch (System.Threading.ThreadAbortException)
            {
                //Thread timout: handle as CarrierError.
                pScrapeInfo.HandleThreadTimeout(pFoundInfo, this.ToString());

                throw;
            }
            catch (System.Net.WebException)
            {
                //This one will be handled in the WebPage class.
                throw;
            }
            catch (Exception leExc)
            {
                return "Oops, API Error: " + leExc.ToString();
            }
        }
    }

    public class AirtradeAPI
    {
        private JObject _jObject;
        private string _json;

        public AirtradeAPI()
        {
            _json = null;
            _jObject = null;

            _urlPart = null;
            _affiliateCode = null;
            _accountCode = null;
            _applicationType = null;
            _userName = null;
            _userPassword = null;
            _digest = null;
            _cultureCode = null;
            _pageSize = int.MinValue;

            _sessionIdentifier = string.Empty;
            _flightResultUrl = string.Empty;
            _groupedResultIdentifier = string.Empty;
            _selectedGroupedFlightOptionIdentifier = string.Empty;
            _selectedLegOptionsIdentifiers = string.Empty;
            _selectedFlightIdentifier = string.Empty;
            _orderIdentifier = string.Empty;
        }

        public string Json
        {
            get { return _json; }
            set
            {
                _json = value;
                _jObject = Airtrade.General.parseJSON(_json);
            }
        }

        //Basic properties
        private string _urlPart;
        private string _affiliateCode;
        private string _accountCode;
        private string _applicationType;
        private string _userName;
        private string _userPassword;
        private string _digest;
        private string _cultureCode;
        private int _pageSize;
        private string _currencyName = "EUR";

        //Process properties
        private string _sessionIdentifier;

        public string SessionIdentifier
        {
            get { return _sessionIdentifier; }
            set { _sessionIdentifier = value; }
        }

        private string _flightResultUrl;
        public string FlightResultUrl
        {
            get { return _flightResultUrl; }
            set { _flightResultUrl = value; }
        }

        private string _groupedResultIdentifier;
        public string GroupedResultIdentifier
        {
            get { return _groupedResultIdentifier; }
            set { _groupedResultIdentifier = value; }
        }

        private string _selectedGroupedFlightOptionIdentifier;
        public string SelectedGroupedFlightOptionIdentifier
        {
            get { return _selectedGroupedFlightOptionIdentifier; }
            set { _selectedGroupedFlightOptionIdentifier = value; }
        }

        private string _selectedLegOptionsIdentifiers;
        public string SelectedLegOptionsIdentifiers
        {
            get { return _selectedLegOptionsIdentifiers; }
            set { _selectedLegOptionsIdentifiers = value; }
        }

        private string _selectedFlightIdentifier;
        public string SelectedFlightIdentifier
        {
            get { return _selectedFlightIdentifier; }
            set { _selectedFlightIdentifier = value; }
        }

        private string _orderIdentifier;

        public string OrderIdentifier
        {
            get { return _orderIdentifier; }
            set { _orderIdentifier = value; }
        }

        public string SearchFlights(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout)
        {
            StringBuilder sbLog = new StringBuilder();

            try
            {
                JObject jObj = null;

                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("SearchFlights method started;\n");
                    sbLog.Append("RealRoundtrip check started;\n");
                    sbLog.Append(string.Format("RR search value = {0};\n ", pScrapeInfo.GetScrapeInfoValueFromName("RR search")));
                }

                #region SF RR request
                if (!string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("RR search")) && pScrapeInfo.GetScrapeInfoValueFromName("RR search").ToLower().Equals("true"))
                {

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting with authentication\n");

                    jObj = Authentcate(pScrapeInfo, piTimeout);

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("Authentication ended\n");
                        if (jObj != null)
                            sbLog.Append(string.Format("sendRequest_Auth:{0}\n", (string)jObj["sendrequest_Auth"]));
                    }

                    if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/usersession/"))
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Authentication successful\n");

                        string data = (string)jObj["Data"];
                        string search = "/usersession/";
                        _sessionIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Sending request for RR flights;\n");

                        jObj = getFlights(ref pScrapeInfo, ref pFoundInfo, piTimeout, true, ref sbLog);

                        if (pScrapeInfo.lbAutoErrorLog)
                        {
                            sbLog.Append("Sending request for flights performed;\n");
                            if (jObj != null)
                                sbLog.Append(string.Format("searchflightRequestSendRR:{0}\n", (string)jObj["searchflightRequestSend"]));
                        }

                        if (jObj != null)
                        {
                            //We have got a valid response
                            if (jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/GroupedResultList/"))
                            {
                                #region Successful SF RR send

                                data = (string)jObj["Data"];
                                search = _urlPart;
                                _flightResultUrl = data.Substring(data.IndexOf(search) + search.Length);

                                search = "GroupedResultList/";
                                _groupedResultIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                                Airtrade.IConnection ai;

                                //Only perform a preffered carrier request if more then 1 carrier is preferred
                                //If only 1 carrier is preferred we can send it in the SF request
                                bool gotPreferred = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST"));
                                //gotPreferred = gotPreferred ? pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(",") : gotPreferred;

                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append(string.Format("Got preffered carriers:{0};\n", gotPreferred.ToString()));

                                bool gotFlights = false;
                                string lastError = null;

                                #region prefferred carriers

                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Starting preffered carriers:;\n");

                                StringBuilder sb = new StringBuilder();

                                sb.Append("{\"filterCriteria\":{");

                                if (gotPreferred)
                                {
                                    sb.Append("\"Airline\":{\"SelectedAirlineCodes\":[");
                                    int ctr = 0;
                                    if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(","))
                                    {
                                        string[] items = pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Split(',');
                                        for (int c = 0; c < items.Length; c++)
                                        {
                                            if (!string.IsNullOrEmpty(items[c].Trim()) && !pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL").Contains(items[c].Trim()))
                                            {
                                                if (ctr > 0)
                                                    sb.Append(",");
                                                sb.Append(string.Format("\"{0}\"", items[c].Trim()));

                                                ctr++;
                                            }
                                        }
                                    }
                                    else
                                        sb.Append(string.Format("\"{0}\"", pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST")));
                                    sb.Append("]}");
                                }

                                sb.Append("},\"sortingCriteria\":{\"Code\":\"Default\",\"Direction\":\"Ascending\"}");

                                if (!_pageSize.Equals(int.MinValue))
                                    sb.Append(",\"PagingCriteria\":{\"PageSize\":" + _pageSize.ToString() + "}");

                                sb.Append("}");

                                ai = new Airtrade.Connection();

                                ai.setContentType(contentType.JSON);
                                ai.setReqType(requestType.PUT);
                                ai.setAuthType(authenticationType.NONE);
                                ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/FlightOptions/GroupedResultList/" + _groupedResultIdentifier);
                                ai.requestTimeOut = piTimeout;

                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append(string.Format("Sending preffered carriers request to {0};\n", ai.Uri));

                                jObj = ai.SendRequest(sb.ToString());

                                if (jObj != null && (JArray)jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
                                {
                                    if (pScrapeInfo.lbAutoErrorLog)
                                    {
                                        sbLog.Append("preferred_carriers_request." + ai.requestSend + "\n");
                                        sbLog.Append("Sending preffered carriers request performed;\n");
                                    }
                                }
                                else
                                {
                                    if (pScrapeInfo.lbAutoErrorLog)
                                        sbLog.Append(string.Format("Preffered carriers request performed incorrectly:{0};\n", ai.requestSend));

                                    lastError = ai.LastError;
                                }

                                #endregion

                                if (!gotFlights)
                                {
                                    if (pScrapeInfo.lbAutoErrorLog)
                                        sbLog.Append("Retrieving actual RR flight data;\n");

                                    ai = new Airtrade.Connection();

                                    ai.setContentType(contentType.FORM);
                                    ai.setReqType(requestType.GET);
                                    ai.setAuthType(authenticationType.NONE);
                                    ai.Uri = new Uri(_urlPart + "/" + _flightResultUrl);
                                    ai.requestTimeOut = piTimeout;

                                    jObj = ai.SendRequest();

                                    if (pScrapeInfo.lbAutoErrorLog)
                                        sbLog.Append("Retrieving actual RR flight data performed;\n");

                                    if (jObj == null)
                                        lastError = ai.LastError;
                                }

                                if (jObj != null)
                                {
                                    #region removing excluded carriers
                                    //Since there is no excluding request in the API, the result has to be filtered

                                    bool gotProhibited = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL"));
                                    if (gotProhibited)
                                    {
                                        JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                                        for (int i = groupedFlightOptionsList.Count; i > 0; i--)
                                        {
                                            if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL").Contains(groupedFlightOptionsList[i - 1]["Airline"]["Code"].ToString()))
                                                jObj["_embedded"]["GroupedFlightOptions"][i - 1].Remove();
                                        }
                                    }

                                    #endregion
                                }
                                else
                                {
                                    if (pScrapeInfo.lbAutoErrorLog)
                                        sbLog.Append(string.Format("Retrieving actual flight data has failed: {0};\n", lastError));

                                    //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);

                                    return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                                }

                                #endregion
                            }
                        }

                        CreateFareDetail(ref jObj, pScrapeInfo);

                        //Remove '0' before flightnumbers
                        normalizeFlightNumbers(ref jObj);

                        jObj.Add("SF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Authentication unsuccessful\n");

                        jObj.Add("SF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }
                }
                #endregion

                #region VF Request
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("No RealRoundtrip performed;\nVerifyFlight check started;\n");
                    sbLog.Append(string.Format("SCR_IS_VF value = {0};\n", pScrapeInfo.lbIsVF.ToString()));
                }

                if (pScrapeInfo.lbIsVF)
                {
                    jObj = getSelectedFlight(ref pScrapeInfo, ref pFoundInfo, piTimeout, "VF", ref sbLog);

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("VerifyFlight performed;\n");
                        //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);

                    }

                    return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                }

                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("No VerifyFlight performed;\nSearchFlights started;\n");
                    //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);

                }
                #endregion

                #region SF Request
                //Sending the actual SF request
                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append("Starting with authentication\n");

                jObj = Authentcate(pScrapeInfo, piTimeout);

                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("Authentication ended\n");
                    if (jObj != null)
                        sbLog.Append(string.Format("sendRequest_Auth:{0}\n", (string)jObj["sendrequest_Auth"]));
                }

                if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/usersession/"))
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Authentication successful\n");

                    string data = (string)jObj["Data"];
                    string search = "/usersession/";
                    _sessionIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                    jObj = retrieveSingleFlights(ref pScrapeInfo, ref pFoundInfo, piTimeout, ref sbLog);

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);
                    }

                    return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);

                }
                else
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Authentication unsuccessful\n");

                    jObj.Add("SF_LOG", sbLog.ToString());

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("SearchFlights performed unsuccessfully;\n");
                        //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);
                    }

                    return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                }
                #endregion


            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("\nException on SF;\n");
                    //pFoundInfo.InsertVariable("PageContent SearchFlightLogging", sbLog.ToString(), true);
                }
                return e.Message;
            }

            return "";
        }

        public string BookFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout)
        {
            StringBuilder sbLog = new StringBuilder();

            try
            {
                JObject jObj = null;

                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append("BookFlight method started;\n");

                jObj = getSelectedFlight(ref pScrapeInfo, ref pFoundInfo, piTimeout, "BF", ref sbLog);

                Airtrade.Connection ai;

                if (jObj != null && jObj.SelectToken("$.PriceBreakdown.Total") != null)
                {
                    Double totalPrice = Convert.ToDouble(string.Format("{0:0.00}", jObj.SelectToken("$.PriceBreakdown.Total").ToString().Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture));

                    int nrOfAdults = pScrapeInfo.ADT;
                    int nrOfChildren = pScrapeInfo.CHD;
                    int nrOfBabies = pScrapeInfo.INF;
                    int nrOfPax = nrOfAdults + nrOfChildren + nrOfBabies;

                    string airlineCode = jObj.SelectToken("$.Airline.Code").ToString();
                    _sessionIdentifier = jObj.SelectToken("$..SelectedIdentifiers.SessionIdentifier").ToString();
                    string search = "/SelectedFlight/";
                    string href = jObj.SelectToken("$..validate:selectedflight.href").ToString();
                    _selectedFlightIdentifier = href.Substring(href.IndexOf(search) + search.Length).Replace("/Validation", "");

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Sending passenger data;\n");

                    bool gotPassngerError = false;
                    JObject bagResponse = jObj.SelectToken("$..Baggage_response") != null ? General.parseJSON(jObj.SelectToken("$..Baggage_response").ToString()) : null;

                    int aCtr = 0;
                    foreach (JToken adt in jObj.SelectTokens("$..Passengers[?(@.PaxType=='Adult')]"))
                    {
                        aCtr++;
                        if (aCtr <= nrOfAdults)
                        {
                            string gender = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_SEX" + aCtr.ToString()).ToUpper().Substring(0, 1).Equals("M") ? "Male" : "Female";
                            string dob = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_AGE_BIRTHYEAR" + aCtr.ToString()) + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_AGE_BIRTHMONTH" + aCtr.ToString()).PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_AGE_BIRTHDAY" + aCtr.ToString()).PadLeft(2, '0');
                            string firstName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_FIRSTNAME" + aCtr.ToString());
                            string lastName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_LASTNAME" + aCtr.ToString());
                            string identifier = adt["Identifier"].ToString();

                            string selectedBagService = string.Empty;
                            if (!string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_BAG" + aCtr.ToString())))
                            {
                                int adultBag = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_BAG" + aCtr.ToString()));
                                int adultBagKG = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_KG" + aCtr.ToString()));
                                if (adultBag > 0 && bagResponse != null)
                                {
                                    try
                                    {
                                        if (bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", adultBag.ToString(), adultBagKG.ToString())) != null)
                                            selectedBagService = bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", adultBag.ToString(), adultBagKG.ToString())).ToString();
                                    }
                                    //We don't want the scrape engine to quit if no BaggageServiceOptions can be found in the response or if the JSONPATH command might crash
                                    //catch (System.Threading.ThreadAbortException)
                                    //{
                                    //    throw;
                                    //}
                                    catch { }
                                }
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{\"identifier\":\"" + identifier + "\",\"PersonData\":{");
                            sb.Append(string.Format("\"GenderType\":\"{0}\",\"FirstName\":\"{1}\",\"LastName\":\"{2}\",\"DateOfBirth\":\"{3}\"", gender, firstName, lastName, dob));
                            sb.Append("},\"FrequentFlyer\":{\"AirlineCode\":\"" + airlineCode + "\",\"FrequentFlyerNumber\":\"\"},");
                            sb.Append("\"SelectedBaggageService\":{\"Code\":\"" + selectedBagService + "\"},");
                            sb.Append("\"SelectedSeatPreference\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedMealPreference\":{\"Code\":\"\"}}");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.PUT);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Passengers/" + identifier);
                            ai.requestTimeOut = piTimeout;

                            JObject passengerObj = ai.SendRequest(sb.ToString());
                            if (passengerObj != null && passengerObj.SelectToken("$.Identifier") != null)
                                continue;
                            else
                            {
                                gotPassngerError = true;
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Passenger ADT data sending failed;\n");

                                break;
                            }
                        }
                    }

                    if (gotPassngerError)
                    {
                        if (jObj.SelectToken("$..BF_LOG") != null)
                        {
                            //replace value
                            jObj.Remove("BF_LOG");
                            jObj.Add("BF_LOG", sbLog.ToString());
                        }
                        else
                            jObj.Add("BF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    int cCtr = 0;
                    foreach (JToken chd in jObj.SelectTokens("$..Passengers[?(@.PaxType=='Child')]"))
                    {
                        cCtr++;
                        if (cCtr <= nrOfChildren)
                        {
                            string gender = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_SEX" + cCtr.ToString()).ToUpper().Substring(0, 1).Equals("M") ? "Male" : "Female";
                            string dob = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_AGE_BIRTHYEAR" + cCtr.ToString()) + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_AGE_BIRTHMONTH" + cCtr.ToString()).PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_AGE_BIRTHDAY" + cCtr.ToString()).PadLeft(2, '0');
                            string firstName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_FIRSTNAME" + cCtr.ToString());
                            string lastName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_LASTNAME" + cCtr.ToString());
                            string identifier = chd["Identifier"].ToString();

                            string selectedBagService = string.Empty;
                            if (!string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_BAG" + cCtr.ToString())))
                            {
                                int childBag = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_BAG" + cCtr.ToString()));
                                int childBagKG = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_KG" + cCtr.ToString()));
                                if (childBag > 0 && bagResponse != null)
                                    try
                                    {
                                        if (bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", childBag.ToString(), childBagKG.ToString())) != null)
                                            selectedBagService = bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", childBag.ToString(), childBagKG.ToString())).ToString();
                                    }
                                    //We don't want the scrape engine to quit if no BaggageServiceOptions can be found in the response or if the JSONPATH command might crash
                                    //catch (System.Threading.ThreadAbortException)
                                    //{
                                    //    throw;
                                    //}
                                    catch { }
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{\"identifier\":\"" + identifier + "\",\"PersonData\":{");
                            sb.Append(string.Format("\"GenderType\":\"{0}\",\"FirstName\":\"{1}\",\"LastName\":\"{2}\",\"DateOfBirth\":\"{3}\"", gender, firstName, lastName, dob));
                            sb.Append("},\"FrequentFlyer\":{\"AirlineCode\":\"" + airlineCode + "\",\"FrequentFlyerNumber\":\"\"},");
                            sb.Append("\"SelectedBaggageService\":{\"Code\":\"" + selectedBagService + "\"},");
                            sb.Append("\"SelectedSeatPreference\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedMealPreference\":{\"Code\":\"\"}}");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.PUT);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Passengers/" + identifier);
                            ai.requestTimeOut = piTimeout;

                            JObject passengerObj = ai.SendRequest(sb.ToString());

                            if (passengerObj != null && passengerObj.SelectToken("$.Identifier") != null)
                                continue;
                            else
                            {
                                gotPassngerError = true;
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Passenger CHD data sending failed;\n");

                                break;
                            }
                        }
                    }

                    if (gotPassngerError)
                    {
                        jObj.Add("BF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    int iCtr = 0;
                    foreach (JToken inf in jObj.SelectTokens("$..Passengers[?(@.PaxType=='Infant')]"))
                    {
                        iCtr++;
                        if (iCtr <= nrOfBabies)
                        {
                            string gender = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_SEX" + iCtr.ToString()).ToUpper().Substring(0, 1).Equals("M") ? "Male" : "Female";
                            string dob = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_AGE_BIRTHYEAR" + iCtr.ToString()) + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_AGE_BIRTHMONTH" + iCtr.ToString()).PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_AGE_BIRTHDAY" + iCtr.ToString()).PadLeft(2, '0');
                            string firstName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_FIRSTNAME" + iCtr.ToString());
                            string lastName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_LASTNAME" + iCtr.ToString());
                            string identifier = inf["Identifier"].ToString();

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{\"identifier\":\"" + identifier + "\",\"PersonData\":{");
                            sb.Append(string.Format("\"GenderType\":\"{0}\",\"FirstName\":\"{1}\",\"LastName\":\"{2}\",\"DateOfBirth\":\"{3}\"", gender, firstName, lastName, dob));
                            sb.Append("},\"FrequentFlyer\":{\"AirlineCode\":\"" + airlineCode + "\",\"FrequentFlyerNumber\":\"\"},");
                            sb.Append("\"SelectedBaggageService\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedSeatPreference\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedMealPreference\":{\"Code\":\"\"}}");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.PUT);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Passengers/" + identifier);
                            ai.requestTimeOut = piTimeout;

                            JObject passengerObj = ai.SendRequest(sb.ToString());

                            if (passengerObj != null && passengerObj.SelectToken("$.Identifier") != null)
                                continue;
                            else
                            {
                                gotPassngerError = true;
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Passenger INF data sending failed;\n");

                                break;
                            }
                        }
                    }

                    if (gotPassngerError)
                    {
                        jObj.Add("BF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Passenger data sent performed. Retrieving selected flight;\n");

                    ai = new Airtrade.Connection();

                    ai.setContentType(contentType.FORM);
                    ai.setReqType(requestType.GET);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                    ai.requestTimeOut = piTimeout;

                    jObj = ai.SendRequest();

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Retrieving selected flight performed;\n");

                    string paymentOption = pScrapeInfo.CheckIfVariableExists("WS_PaymentOption") ? pScrapeInfo.GetScrapeInfoValueFromName("WS_PaymentOption") : "TPINV";

                    //Not sure if we need to send for payment options since we're using invoice anyhow
                    //if (pScrapeInfo.lbAutoErrorLog)
                    //    sbLog.Append("Retrieving payment options;\n");

                    //ai = new Airtrade.Connection();

                    //ai.setContentType(contentType.FORM);
                    //ai.setReqType(requestType.GET);
                    //ai.setAuthType(authenticationType.NONE);
                    //ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/PaymentOptions");
                    //ai.requestTimeOut = piTimeout;

                    //jObj = ai.SendRequest();

                    //if (jObj != null && jObj.SelectToken("$..PaymentOptions[?(@.DisplayName=='Invoice')].Code") != null)
                    //    paymentOption = jObj.SelectToken("$..PaymentOptions[?(@.DisplayName=='Invoice')].Code").ToString();

                    //if (pScrapeInfo.lbAutoErrorLog)
                    //    sbLog.Append("Payment options received;\n");

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Sending selected flight with paymenoption Invoice\n");

                    StringBuilder sbPayment = new StringBuilder();

                    sbPayment.Append("{\"Identifier\":\"" + _selectedFlightIdentifier + "\",\"TicketInsuranceOption\":{\"Code\":\"\",\"Amount\":0,\"IsSelected\":null},");
                    sbPayment.Append("\"SelectedPriorityBoardingService\":{\"Code\":\"\"},");
                    sbPayment.Append("\"SelectedPaymentOption\":{\"Code\":\"" + paymentOption + "\"}}");

                    ai.setContentType(contentType.JSON);
                    ai.setReqType(requestType.PUT);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                    ai.requestTimeOut = piTimeout;

                    jObj = ai.SendRequest(sbPayment.ToString());

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Selected Payment option Invoice sent;\n");

                    if (jObj == null || jObj.SelectToken("$.PriceBreakdown.Total") == null)
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("No valid validation quote recieved;\n");

                        jObj.Add("BF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Validation quote performed; Checking quoted price\n");

                    Double quotePrice = Convert.ToDouble(string.Format("{0:0.00}", jObj.SelectToken("$.PriceBreakdown.Total").ToString().Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture));

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Checking found price ({0}) and quoted price ({1});\n", totalPrice.ToString(), quotePrice.ToString()));

                    CheckPrice(ref pScrapeInfo, ref pFoundInfo, jObj);

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Sending final flight validation;\n");

                    ai = new Airtrade.Connection();

                    ai.setContentType(contentType.FORM);
                    ai.setReqType(requestType.GET);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Validation");
                    ai.requestTimeOut = piTimeout;

                    jObj = ai.SendRequest();

                    if (jObj != null && jObj["Status"] != null && jObj["Status"].ToString().ToLower().Equals("valid"))
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Creating a productorder;\n");

                        ai.setContentType(contentType.JSON);
                        ai.setReqType(requestType.POST);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/ProductOrder");
                        ai.requestTimeOut = piTimeout;

                        StringBuilder sbOrder = new StringBuilder();

                        sbOrder.Append("{\"SelectedProductIdentifier\":\"" + _selectedFlightIdentifier + "\",");
                        sbOrder.Append("\"OrderRecapFields\":{\"CustomParameters\":[");
                        //Don't know yet where to get the clients name from
                        //Or to remove them
                        //sbOrder.Append("{\"name\":\"test0\",");
                        //sbOrder.Append("\"value\":\"testvalue0\"}");
                        //sbOrder.Append(",")
                        //sbOrder.Append("{\"name\":\"test1\",");
                        //sbOrder.Append("\"value\":\"testvalue1\"}");
                        ////
                        sbOrder.Append("]}}");

                        jObj = ai.SendRequest(sbOrder.ToString());

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Productorder received;\n");

                        if (jObj != null && jObj["Data"] != null)
                        {
                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving order confirmation;\n");

                            string productOrderUrl = jObj["Data"].ToString();

                            ai.setContentType(contentType.FORM);
                            ai.setReqType(requestType.GET);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(productOrderUrl);
                            ai.requestTimeOut = piTimeout;

                            jObj = ai.SendRequest();

                            if (jObj != null && jObj.SelectToken("$.Identifier") != null)
                            {
                                _orderIdentifier = jObj.SelectToken("$.Identifier").ToString();
                                addIdentifiers(ref jObj);
                            }

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Order confirmation performed;\n");

                            jObj.Add("BF_LOG", sbLog.ToString());

                            //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                            return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                        }
                        else
                        {
                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("No valid productorder created;\n");

                            jObj.Add("BF_LOG", sbLog.ToString());

                            //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                            return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                        }
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("No valid final flight validation ");

                        if (jObj.SelectToken("$..BF_LOG") != null)
                        {
                            //replace value
                            jObj.Remove("BF_LOG");
                            jObj.Add("BF_LOG", sbLog.ToString());
                        }
                        else
                            jObj.Add("BF_LOG", sbLog.ToString());

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }
                }
                else
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("No match found on BF;\n");

                    if (jObj.SelectToken("$..BF_LOG") != null)
                    {
                        //replace value
                        jObj.Remove("BF_LOG");
                        jObj.Add("BF_LOG", sbLog.ToString());
                    }
                    else
                        jObj.Add("BF_LOG", sbLog.ToString());

                    //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("\nException on BF;\n");
                    //pFoundInfo.InsertVariable("PageContent BookFlightLogging", sbLog.ToString(), true);
                }
                return e.Message;
            }

            return "";
        }

        public string SearchReturnFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout)
        {
            StringBuilder sbLog = new StringBuilder();

            try
            {
                JObject jObj = getSelectedSRFlight(ref pScrapeInfo, ref pFoundInfo, piTimeout, "SRF", ref sbLog);

                //pFoundInfo.InsertVariable("PageContent SearchReturnFlightLogging", sbLog.ToString(), true);

                return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("\nException on SRF;\n");
                    //pFoundInfo.InsertVariable("PageContent SearchReturnFlightLogging", sbLog.ToString(), true);
                }
                return e.Message;
            }

            return "";
        }

        public string BookReturnFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout)
        {
            StringBuilder sbLog = new StringBuilder();

            try
            {
                JObject jObj = null;

                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append("BookReturnFlight method started;\n");

                jObj = getSelectedSRFlight(ref pScrapeInfo, ref pFoundInfo, piTimeout, "BRF", ref sbLog);

                Airtrade.Connection ai;

                if (jObj != null && jObj.SelectToken("$.PriceBreakdown.Total") != null)
                {
                    #region Selected flight found

                    Double totalPrice = Convert.ToDouble(string.Format("{0:0.00}", jObj.SelectToken("$.PriceBreakdown.Total").ToString().Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture));

                    int nrOfAdults = pScrapeInfo.ADT;
                    int nrOfChildren = pScrapeInfo.CHD;
                    int nrOfBabies = pScrapeInfo.INF;
                    int nrOfPax = nrOfAdults + nrOfChildren + nrOfBabies;

                    string airlineCode = jObj.SelectToken("$.Airline.Code").ToString();
                    _sessionIdentifier = jObj.SelectToken("$..SelectedIdentifiers.SessionIdentifier").ToString();
                    string search = "/SelectedFlight/";
                    string href = jObj.SelectToken("$..validate:selectedflight.href").ToString();
                    _selectedFlightIdentifier = href.Substring(href.IndexOf(search) + search.Length).Replace("/Validation", "");

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Sending passenger data;\n");

                    bool gotPassngerError = false;
                    JObject bagResponse = jObj.SelectToken("$..Baggage_response") != null ? General.parseJSON(jObj.SelectToken("$..Baggage_response").ToString()) : null;

                    int aCtr = 0;
                    foreach (JToken adt in jObj.SelectTokens("$..Passengers[?(@.PaxType=='Adult')]"))
                    {
                        aCtr++;
                        if (aCtr <= nrOfAdults)
                        {
                            string gender = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_SEX" + aCtr.ToString()).ToUpper().Substring(0, 1).Equals("M") ? "Male" : "Female";
                            string dob = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_AGE_BIRTHYEAR" + aCtr.ToString()) + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_AGE_BIRTHMONTH" + aCtr.ToString()).PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_AGE_BIRTHDAY" + aCtr.ToString()).PadLeft(2, '0');
                            string firstName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_FIRSTNAME" + aCtr.ToString());
                            string lastName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_LASTNAME" + aCtr.ToString());
                            string identifier = adt["Identifier"].ToString();

                            string selectedBagService = string.Empty;
                            if (!string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_BAG" + aCtr.ToString())))
                            {
                                int adultBag = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_BAG" + aCtr.ToString()));
                                int adultBagKG = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_ADULT_KG" + aCtr.ToString()));
                                if (adultBag > 0 && bagResponse != null)
                                {
                                    try
                                    {
                                        if (bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", adultBag.ToString(), adultBagKG.ToString())) != null)
                                            selectedBagService = bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", adultBag.ToString(), adultBagKG.ToString())).ToString();
                                    }
                                    //We don't want the scrape engine to quit if no BaggageServiceOptions can be found in the response or if the JSONPATH command might crash
                                    //catch (System.Threading.ThreadAbortException)
                                    //{
                                    //    throw;
                                    //}
                                    catch { }

                                }
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{\"identifier\":\"" + identifier + "\",\"PersonData\":{");
                            sb.Append(string.Format("\"GenderType\":\"{0}\",\"FirstName\":\"{1}\",\"LastName\":\"{2}\",\"DateOfBirth\":\"{3}\"", gender, firstName, lastName, dob));
                            sb.Append("},\"FrequentFlyer\":{\"AirlineCode\":\"" + airlineCode + "\",\"FrequentFlyerNumber\":\"\"},");
                            sb.Append("\"SelectedBaggageService\":{\"Code\":\"" + selectedBagService + "\"},");
                            sb.Append("\"SelectedSeatPreference\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedMealPreference\":{\"Code\":\"\"}}");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.PUT);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Passengers/" + identifier);
                            ai.requestTimeOut = piTimeout;

                            JObject passengerObj = ai.SendRequest(sb.ToString());

                            if (passengerObj != null && passengerObj.SelectToken("$.Identifier") != null)
                                continue;
                            else
                            {
                                gotPassngerError = true;
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Passenger ADT data sending failed;\n");

                                break;
                            }
                        }
                    }

                    if (gotPassngerError)
                    {
                        jObj.Add("BRF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    int cCtr = 0;
                    foreach (JToken chd in jObj.SelectTokens("$..Passengers[?(@.PaxType=='Child')]"))
                    {
                        cCtr++;
                        if (cCtr <= nrOfChildren)
                        {
                            string gender = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_SEX" + cCtr.ToString()).ToUpper().Substring(0, 1).Equals("M") ? "Male" : "Female";
                            string dob = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_AGE_BIRTHYEAR" + cCtr.ToString()) + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_AGE_BIRTHMONTH" + cCtr.ToString()).PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_AGE_BIRTHDAY" + cCtr.ToString()).PadLeft(2, '0');
                            string firstName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_FIRSTNAME" + cCtr.ToString());
                            string lastName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_LASTNAME" + cCtr.ToString());
                            string identifier = chd["Identifier"].ToString();

                            string selectedBagService = string.Empty;
                            if (!string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_BAG" + cCtr.ToString())))
                            {
                                int childBag = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_BAG" + cCtr.ToString()));
                                int childBagKG = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CHILD_KG" + cCtr.ToString()));
                                if (childBag > 0 && bagResponse != null)
                                {
                                    try
                                    {
                                        if (bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", childBag.ToString(), childBagKG.ToString())) != null)
                                            selectedBagService = bagResponse.SelectToken(string.Format("$..BaggageServiceOptions[?(@.Pieces == {0} && @.Weight == {1}.0)].Code", childBag.ToString(), childBagKG.ToString())).ToString();
                                    }
                                    //We don't want the scrape engine to quit if no BaggageServiceOptions can be found in the response or if the JSONPATH command might crash//We don't want the scrape engine to quit if no BaggageServiceOptions can be found in the response or if the JSONPATH command might crash
                                    //catch (System.Threading.ThreadAbortException)
                                    //{
                                    //    throw;
                                    //}
                                    catch { }
                                }
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{\"identifier\":\"" + identifier + "\",\"PersonData\":{");
                            sb.Append(string.Format("\"GenderType\":\"{0}\",\"FirstName\":\"{1}\",\"LastName\":\"{2}\",\"DateOfBirth\":\"{3}\"", gender, firstName, lastName, dob));
                            sb.Append("},\"FrequentFlyer\":{\"AirlineCode\":\"" + airlineCode + "\",\"FrequentFlyerNumber\":\"\"},");
                            sb.Append("\"SelectedBaggageService\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedSeatPreference\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedMealPreference\":{\"Code\":\"\"}}");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.PUT);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Passengers/" + identifier);
                            ai.requestTimeOut = piTimeout;

                            JObject passengerObj = ai.SendRequest(sb.ToString());

                            if (passengerObj != null && passengerObj.SelectToken("$.Identifier") != null)
                                continue;
                            else
                            {
                                gotPassngerError = true;
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Passenger CHD data sending failed;\n");

                                break;
                            }
                        }
                    }

                    if (gotPassngerError)
                    {
                        jObj.Add("BRF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    int iCtr = 0;
                    foreach (JToken inf in jObj.SelectTokens("$..Passengers[?(@.PaxType=='Infant')]"))
                    {
                        iCtr++;
                        if (iCtr <= nrOfBabies)
                        {
                            string gender = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_SEX" + iCtr.ToString()).ToUpper().Substring(0, 1).Equals("M") ? "Male" : "Female";
                            string dob = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_AGE_BIRTHYEAR" + iCtr.ToString()) + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_AGE_BIRTHMONTH" + iCtr.ToString()).PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_AGE_BIRTHDAY" + iCtr.ToString()).PadLeft(2, '0');
                            string firstName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_FIRSTNAME" + iCtr.ToString());
                            string lastName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_INFANT_LASTNAME" + iCtr.ToString());
                            string identifier = inf["Identifier"].ToString();

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{\"identifier\":\"" + identifier + "\",\"PersonData\":{");
                            sb.Append(string.Format("\"GenderType\":\"{0}\",\"FirstName\":\"{1}\",\"LastName\":\"{2}\",\"DateOfBirth\":\"{3}\"", gender, firstName, lastName, dob));
                            sb.Append("},\"FrequentFlyer\":{\"AirlineCode\":\"" + airlineCode + "\",\"FrequentFlyerNumber\":\"\"},");
                            sb.Append("\"SelectedBaggageService\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedSeatPreference\":{\"Code\":\"\"},");
                            sb.Append("\"SelectedMealPreference\":{\"Code\":\"\"}}");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.PUT);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Passengers/" + identifier);
                            ai.requestTimeOut = piTimeout;

                            JObject passengerObj = ai.SendRequest(sb.ToString());

                            if (passengerObj != null && passengerObj.SelectToken("$.Identifier") != null)
                                continue;
                            else
                            {
                                gotPassngerError = true;
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Passenger INF data sending failed;\n");

                                break;
                            }
                        }
                    }

                    if (gotPassngerError)
                    {
                        jObj.Add("BRF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Passenger data sent performed. Retrieving selected flight;\n");

                    ai = new Airtrade.Connection();

                    ai.setContentType(contentType.FORM);
                    ai.setReqType(requestType.GET);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                    ai.requestTimeOut = piTimeout;

                    jObj = ai.SendRequest();

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Retrieving selected flight performed;\n");

                    string paymentOption = pScrapeInfo.CheckIfVariableExists("WS_PaymentOption") ? pScrapeInfo.GetScrapeInfoValueFromName("WS_PaymentOption") : "TPINV";

                    //Not sure if we need to send for payment options since we're using invoice anyhow
                    //if (pScrapeInfo.lbAutoErrorLog)
                    //    sbLog.Append("Retrieving payment options;\n");

                    //ai = new Airtrade.Connection();

                    //ai.setContentType(contentType.FORM);
                    //ai.setReqType(requestType.GET);
                    //ai.setAuthType(authenticationType.NONE);
                    //ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/PaymentOptions");
                    //ai.requestTimeOut = piTimeout;

                    //jObj = ai.SendRequest();

                    //if (jObj != null && jObj.SelectToken("$..PaymentOptions[?(@.DisplayName=='Invoice')].Code") != null)
                    //    paymentOption = jObj.SelectToken("$..PaymentOptions[?(@.DisplayName=='Invoice')].Code").ToString();

                    //if (pScrapeInfo.lbAutoErrorLog)
                    //    sbLog.Append("Payment options received;\n");

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Sending selected flight with paymenoption Invoice\n");

                    StringBuilder sbPayment = new StringBuilder();

                    sbPayment.Append("{\"Identifier\":\"" + _selectedFlightIdentifier + "\",\"TicketInsuranceOption\":{\"Code\":\"\",\"Amount\":0,\"IsSelected\":null},");
                    sbPayment.Append("\"SelectedPriorityBoardingService\":{\"Code\":\"\"},");
                    sbPayment.Append("\"SelectedPaymentOption\":{\"Code\":\"" + paymentOption + "\"}}");

                    ai.setContentType(contentType.JSON);
                    ai.setReqType(requestType.PUT);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                    ai.requestTimeOut = piTimeout;

                    jObj = ai.SendRequest(sbPayment.ToString());

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Selected Payment option Invoice sent;\n");

                    if (jObj == null || jObj.SelectToken("$.PriceBreakdown.Total") == null)
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("No valid validation quote recieved;\n");

                        jObj.Add("BRF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Validation quote performed; Checking quoted price\n");

                    Double quotePrice = Convert.ToDouble(string.Format("{0:0.00}", jObj.SelectToken("$.PriceBreakdown.Total").ToString().Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture));

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Checking found price ({0}) and quoted price ({1});\n", totalPrice.ToString(), quotePrice.ToString()));

                    CheckPrice(ref pScrapeInfo, ref pFoundInfo, jObj);

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Sending final flight validation;\n");

                    ai = new Airtrade.Connection();

                    ai.setContentType(contentType.FORM);
                    ai.setReqType(requestType.GET);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier + "/Validation");
                    ai.requestTimeOut = piTimeout;

                    jObj = ai.SendRequest();

                    if (jObj != null && jObj["Status"] != null && jObj["Status"].ToString().ToLower().Equals("valid"))
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Creating a productorder;\n");

                        ai.setContentType(contentType.JSON);
                        ai.setReqType(requestType.POST);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/ProductOrder");
                        ai.requestTimeOut = piTimeout;

                        StringBuilder sbOrder = new StringBuilder();

                        sbOrder.Append("{\"SelectedProductIdentifier\":\"" + _selectedFlightIdentifier + "\",");
                        sbOrder.Append("\"OrderRecapFields\":{\"CustomParameters\":[");
                        //Don't know yet where to get the clients name from
                        //Or to remove them
                        //sbOrder.Append("{\"name\":\"test0\",");
                        //sbOrder.Append("\"value\":\"testvalue0\"}");
                        //sbOrder.Append(",")
                        //sbOrder.Append("{\"name\":\"test1\",");
                        //sbOrder.Append("\"value\":\"testvalue1\"}");
                        ////
                        sbOrder.Append("]}}");

                        jObj = ai.SendRequest(sbOrder.ToString());

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Productorder received;\n");

                        if (jObj != null && jObj["Data"] != null)
                        {
                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving order confirmation;\n");

                            string productOrderUrl = jObj["Data"].ToString();

                            ai.setContentType(contentType.FORM);
                            ai.setReqType(requestType.GET);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(productOrderUrl);
                            ai.requestTimeOut = piTimeout;

                            jObj = ai.SendRequest();

                            if (jObj != null && jObj.SelectToken("$.Identifier") != null)
                            {
                                _orderIdentifier = jObj.SelectToken("$.Identifier").ToString();
                                addIdentifiers(ref jObj);
                            }

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Order confirmation performed;\n");

                            jObj.Add("BRF_LOG", sbLog.ToString());

                            //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                            return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                        }
                        else
                        {
                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("No valid productorder created;\n");

                            jObj.Add("BRF_LOG", sbLog.ToString());

                            //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                            return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                        }

                        #endregion
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("No valid final flight validation ");

                        if (jObj.SelectToken("$..BRF_LOG") != null)
                        {
                            //replace value
                            jObj.Remove("BRF_LOG");
                            jObj.Add("BRF_LOG", sbLog.ToString());
                        }
                        else
                            jObj.Add("BRF_LOG", sbLog.ToString());

                        //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                        return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                    }
                }
                else
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("No match found on BRF;\n");

                    if (jObj.SelectToken("$..BRF_LOG") != null)
                    {
                        //replace value
                        jObj.Remove("BRF_LOG");
                        jObj.Add("BRF_LOG", sbLog.ToString());
                    }
                    else
                        jObj.Add("BRF_LOG", sbLog.ToString());

                    //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(jObj);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("\nException on BRF;\n");
                    //pFoundInfo.InsertVariable("PageContent BookReturnFlightLogging", sbLog.ToString(), true);
                }
                return e.Message;
            }

            return "";
        }

        /** private methods ****************************************************************************************/

        private JObject Authentcate(ScrapeInfo pScrapeInfo, int piTimeout)
        {
            JObject responseJObj = null;

            _userName = pScrapeInfo.GetScrapeInfoValueFromName("WS_Username");
            _userPassword = pScrapeInfo.GetScrapeInfoValueFromName("WS_Password");
            _urlPart = pScrapeInfo.GetScrapeInfoValueFromName("WS_URLPart");
            _affiliateCode = pScrapeInfo.GetScrapeInfoValueFromName("WS_AffiliateCode");
            _accountCode = pScrapeInfo.GetScrapeInfoValueFromName("WS_AccountCode");
            _applicationType = pScrapeInfo.GetScrapeInfoValueFromName("WS_ApplicationType");
            _digest = pScrapeInfo.GetScrapeInfoValueFromName("WS_Digest");
            _cultureCode = pScrapeInfo.GetScrapeInfoValueFromName("WS_CultureCode");

            if (pScrapeInfo.CheckIfVariableExists("WS_PageSize"))
                _pageSize = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("WS_PageSize"));

            Airtrade.IConnection ai = new Airtrade.Connection();

            ai.setContentType(contentType.JSON);
            ai.setReqType(requestType.POST);
            ai.setAuthType(authenticationType.BASIC);
            ai.Uri = new Uri(_urlPart + "/usersession/");
            ai.requestTimeOut = piTimeout;
            ai.UserName = _userName;
            ai.Password = _userPassword;

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append(string.Format("\"AffiliateCode\":\"{0}\",", _affiliateCode));
            sb.Append(string.Format("\"AccountCode\": \"{0}\",", _accountCode));
            sb.Append(string.Format("\"ApplicationType\": \"{0}\",", _applicationType));
            sb.Append("\"UserCredentials\":{\"UserToken\": \"\",");
            sb.Append(string.Format("\"Name\": \"{0}\",", _userName));
            sb.Append(string.Format("\"Password\": \"{0}\"", _userPassword));
            sb.Append("},\"CookieData\":null,\"Debug\":false,");
            sb.Append(string.Format("\"Digest\":\"{0}\",", _digest));
            sb.Append(string.Format("\"CultureCode\":\"{0}\"", _cultureCode));
            sb.Append("}");

            responseJObj = ai.SendRequest(sb.ToString());

            responseJObj.Add("sendrequest_Auth", ai.requestSend);

            return responseJObj;
        }

        private JObject getFlights(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout, bool isRoundtrip, ref StringBuilder sbLog)
        {
            JObject responseJObj = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"PassengerAndPreferenceCriteria\":{");
                sb.Append(string.Format("\"NumberOfAdults\":\"{0}\",", pScrapeInfo.ADT.ToString()));
                sb.Append(string.Format("\"NumberOfChildren\":\"{0}\",", pScrapeInfo.CHD.ToString()));
                sb.Append(string.Format("\"NumberOfInfants\":\"{0}\",", pScrapeInfo.INF.ToString()));

                string cabinClass = "Economy";
                List<string> selectedFareClasses = MFC2Items(pScrapeInfo);
                if (selectedFareClasses != null && selectedFareClasses.Count > 0)
                {
                    //Get the first available cabin class
                    cabinClass = selectedFareClasses[0].ToString();
                }
                sb.Append(string.Format("\"CabinClass\":\"{0}\",", cabinClass));

                string preferredCarrier = "";
                bool gotPreferred = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST"));
                if (gotPreferred && !pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(","))
                    preferredCarrier = pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST");

                sb.Append(string.Format("\"AirlinePreferenceCode\":\"{0}\",", preferredCarrier));
                sb.Append("\"AlliancePerferenceCode\":\"\",");
                sb.Append("\"NonstopOnly\":\"false\",");
                sb.Append("\"UseRadius\":\"false\"},");
                sb.Append("\"LegCriterias\":[");

                string depDate = null;
                if (!isRoundtrip)
                    depDate = createAPIDate(DateTime.ParseExact(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR") + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_START_MONTH").PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_START_DAY").PadLeft(2, '0'), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None));
                else
                    depDate = createAPIDate(DateTime.ParseExact(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Outward") + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_START_MONTH_Outward").PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_START_DAY_Outward").PadLeft(2, '0'), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None));

                string[] origins;
                string[] destinations;

                if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME").Contains(","))
                    origins = pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME").Split(',');
                else
                    origins = new string[1] { pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME") };

                if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME").Contains(","))
                    destinations = pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME").Split(',');
                else
                    destinations = new string[1] { pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME") };

                int originLength = origins.Length <= 6 ? origins.Length : 6;
                int destinLength = destinations.Length <= 6 ? destinations.Length : 6;

                int ctr = 0;
                for (int i = 0; i < originLength; i++)
                {
                    for (int j = 0; j < destinLength; j++)
                    {
                        if (ctr > 0)
                            sb.Append(",");
                        sb.Append("{");
                        sb.Append(string.Format("\"LegNumber\":\"{0}\",", ctr));
                        sb.Append(string.Format("\"DepartureCode\":\"{0}\",", origins[i]));
                        sb.Append(string.Format("\"ArrivalCode\":\"{0}\",", destinations[j]));
                        sb.Append(string.Format("\"DepartureDate\":\"{0}\",", depDate));
                        sb.Append("\"DepartureTime\":\"11:00\",");
                        sb.Append("\"UseTimePreference\":\"false\"}");

                        ctr++;
                    }
                }

                //This part only for RR
                if (isRoundtrip)
                {
                    string arrDate = createAPIDate(DateTime.ParseExact(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Return") + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_START_MONTH_Return").PadLeft(2, '0') + "-" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_START_DAY_Return").PadLeft(2, '0'), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None));

                    for (int j = 0; j < destinLength; j++)
                    {
                        for (int i = 0; i < originLength; i++)
                        {
                            sb.Append(",{");
                            sb.Append(string.Format("\"LegNumber\":\"{0}\",", ctr));
                            sb.Append(string.Format("\"DepartureCode\":\"{0}\",", destinations[j]));
                            sb.Append(string.Format("\"ArrivalCode\":\"{0}\",", origins[i]));
                            sb.Append(string.Format("\"DepartureDate\":\"{0}\",", arrDate));
                            sb.Append("\"DepartureTime\":\"11:00\",");
                            sb.Append("\"UseTimePreference\":\"false\"}");

                            ctr++;
                        }
                    }
                }

                sb.Append("],\"CustomSearchParams\":[{\"name\":\"CorporateCode\",\"value\":\"\"}]");

                //sb.Append(",\"PagingCriteria\":{\"PageSize\": " + _pageSize.ToString() + "}");

                sb.Append("}");

                Airtrade.IConnection ai = new Airtrade.Connection();

                ai.setContentType(contentType.JSON);
                ai.setReqType(requestType.POST);
                ai.setAuthType(authenticationType.NONE);
                ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/flightoptions/groupedresultlist/");
                ai.requestTimeOut = piTimeout;

                responseJObj = ai.SendRequest(sb.ToString());

                if (responseJObj != null && pScrapeInfo.lbAutoErrorLog)
                {
                    responseJObj.Add("searchflightRequestSend", ai.requestSend);
                }

            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    responseJObj = new JObject();

                    sbLog.Append(string.Format("\nException on getFlights method; Error message:{0}\n", e.Message));
                    responseJObj.Add("getFlights_LOG", sbLog.ToString());
                }

                return responseJObj;
            }

            return responseJObj;
        }

        private JObject retrieveSingleFlights(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout, ref StringBuilder sbLog)
        {
            JObject jObj = null;

            #region Search Flight request
            //Sending the actual SF request
            if (pScrapeInfo.lbAutoErrorLog)
                sbLog.Append("Sending request for flights;\n");

            jObj = getFlights(ref pScrapeInfo, ref pFoundInfo, piTimeout, false, ref sbLog);

            if (pScrapeInfo.lbAutoErrorLog)
            {
                sbLog.Append("Sending request for flights performed;\n");
                if (jObj != null)
                    sbLog.Append(string.Format("searchflightRequestSend:{0}\n", (string)jObj["searchflightRequestSend"]));
            }

            #endregion

            if (jObj != null)
            {
                //We have got a valid response
                if (jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/GroupedResultList/"))
                {
                    #region Successful SF send

                    string data = (string)jObj["Data"];
                    string search = _urlPart;
                    _flightResultUrl = data.Substring(data.IndexOf(search) + search.Length);

                    search = "GroupedResultList/";
                    _groupedResultIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                    Airtrade.IConnection ai;

                    //Only perform a preffered carrier request if more then 1 carrier is preferred
                    //If only 1 carrier is preferred we can send it in the SF request // Allways send thecarrier even if it is 1
                    bool gotPreferred = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST"));
                    //gotPreferred = gotPreferred ? pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(",") : gotPreferred;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Got preffered carriers:{0};\n", gotPreferred.ToString()));

                    bool gotFlights = false;

                    string lastError = null;


                    #region prefferred carriers

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting preffered carriers:;\n");

                    StringBuilder sb = new StringBuilder();

                    sb.Append("{\"filterCriteria\":{");


                    if (gotPreferred)
                    {
                        sb.Append("\"Airline\":{\"SelectedAirlineCodes\":[");
                        int ctr = 0;
                        if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(","))
                        {
                            string[] items = pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Split(',');
                            for (int c = 0; c < items.Length; c++)
                            {
                                if (!string.IsNullOrEmpty(items[c].Trim()) && !pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL").Contains(items[c].Trim()))
                                {
                                    if (ctr > 0)
                                        sb.Append(",");
                                    sb.Append(string.Format("\"{0}\"", items[c].Trim()));

                                    ctr++;
                                }
                            }
                        }
                        else
                            sb.Append(string.Format("\"{0}\"", pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST")));
                        sb.Append("]}");
                    }

                    sb.Append("},\"sortingCriteria\":{\"Code\":\"Default\",\"Direction\":\"Ascending\"}");

                    if (!_pageSize.Equals(int.MinValue))
                        sb.Append(",\"PagingCriteria\":{\"PageSize\":" + _pageSize.ToString() + "}");

                    sb.Append("}");

                    ai = new Airtrade.Connection();
                    ai.setContentType(contentType.JSON);
                    ai.setReqType(requestType.PUT);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/FlightOptions/GroupedResultList/" + _groupedResultIdentifier);
                    ai.requestTimeOut = piTimeout;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Sending preffered carriers request to {0};\n", ai.Uri));

                    jObj = ai.SendRequest(sb.ToString());

                    if (jObj != null && (JArray)jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
                    {
                        gotFlights = true;

                        if (pScrapeInfo.lbAutoErrorLog)
                        {
                            sbLog.Append("preferred_carriers_request." + ai.requestSend + "\n");
                            sbLog.Append("Sending preffered carriers request performed;\n");
                        }
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Preffered carriers request performed incorrectly:{0};\n", ai.requestSend));

                        lastError = ai.LastError;
                    }

                    #endregion


                    int pageSize = jObj.SelectToken("$..PageSize") != null ? Convert.ToInt32(jObj.SelectToken("$..PageSize")) : int.MinValue;


                    if (!gotFlights)
                    {
                        ai = new Airtrade.Connection();

                        ai.setContentType(contentType.FORM);
                        ai.setReqType(requestType.GET);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + "/" + _flightResultUrl);
                        ai.requestTimeOut = piTimeout;

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Retrieving actual flight data from {0};\n", ai.Uri));

                        jObj = ai.SendRequest();

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving actual flight data performed;\n");

                        if (jObj == null)
                            lastError = ai.LastError;
                    }

                    if (jObj != null)
                    {
                        #region removing excluded carriers
                        //Since there is no excluding request in the API, the result has to be filtered

                        bool gotProhibited = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL"));
                        if (gotProhibited)
                        {
                            JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                            for (int i = groupedFlightOptionsList.Count; i > 0; i--)
                            {
                                if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL").Contains(groupedFlightOptionsList[i - 1]["Airline"]["Code"].ToString()))
                                    jObj["_embedded"]["GroupedFlightOptions"][i - 1].Remove();
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Retrieving actual flight data has failed: {0};\n", lastError));
                    }

                    if (jObj != null)
                    {
                        CreateFareDetail(ref jObj, pScrapeInfo);

                        //Remove '0' before flightnumbers
                        normalizeFlightNumbers(ref jObj);
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("SearchFlights performed successfully;\n");


                    jObj.Add("SF_LOG", sbLog.ToString());

                    #endregion
                }
                else
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("No valid request for sending flights performed;\n");

                    jObj.Add("SF_LOG", sbLog.ToString());
                }
            }
            else
            {
                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append("No GroupedResultList url returned;\n");

                jObj.Add("SF_LOG", sbLog.ToString());
            }

            return jObj;
        }

        private JObject getSelectedFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout, string procesType, ref StringBuilder sbLog)
        {
            JObject jObj = null;

            if (pScrapeInfo.lbAutoErrorLog)
                sbLog.Append("Starting with verifyFlight\n");

            try
            {
                //First check if we can still use the session identifiers to get the specific flight
                bool gotSessionIdentifier = pScrapeInfo.CheckIfVariableExists("SessionIdentifier");
                bool gotGroupedFlightResultIdentifier = pScrapeInfo.CheckIfVariableExists("GroupedFlightResultIdentifier");
                bool gotSelectedGroupedFlightOptionIdentifier = pScrapeInfo.CheckIfVariableExists("SelectedGroupedFlightOptionIdentifier");
                bool gotSelectedLegOptionsIdentifiers = pScrapeInfo.CheckIfVariableExists("SelectedLegOptionsIdentifiers");
                bool gotSelectedFlightIdentifier = pScrapeInfo.CheckIfVariableExists("SelectedFlightIdentifier");

                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append(string.Format("Checking session identifiers SF --> SessionIdentifier:{0}, GroupedFlightResultIdentifier:{1}, SelectedGroupedFlightOptionIdentifier:{2}, SelectedLegOptionsIdentifiers:{3}, SelectedFlightIdentifier:{4}\n", gotSessionIdentifier.ToString(), gotGroupedFlightResultIdentifier.ToString(), gotSelectedGroupedFlightOptionIdentifier.ToString(), gotSelectedLegOptionsIdentifiers.ToString(), gotSelectedFlightIdentifier.ToString()));

                bool gotSelectedFlight = false;

                if (!gotSelectedFlight && gotSessionIdentifier && gotSelectedFlightIdentifier)
                {
                    _urlPart = pScrapeInfo.GetScrapeInfoValueFromName("WS_URLPart");
                    _sessionIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SessionIdentifier");
                    _selectedFlightIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SelectedFlightIdentifier");

                    Airtrade.Connection ai = new Airtrade.Connection();

                    ai.setContentType(contentType.FORM);
                    ai.setReqType(requestType.GET);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                    ai.requestTimeOut = piTimeout;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Retrieving selected flight data with selectedFlightIdentifier from {0};\n", ai.Uri));

                    jObj = ai.SendRequest();

                    gotSelectedFlight = true;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Retrieving selected flight data with selectedFlightIdentifier performed;\n");

                    //Retrieveing the baggage
                    if (jObj != null)
                    {
                        string baggageUrl = jObj.SelectToken("$..Passengers[0]._links.lookup:baggageserviceoptions.href").ToString();

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving selected flight baggage;\n");

                        ai = new Airtrade.Connection();

                        ai.setContentType(contentType.FORM);
                        ai.setReqType(requestType.GET);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + baggageUrl);
                        ai.requestTimeOut = piTimeout;

                        JObject baggageJObj = ai.SendRequest();

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving selected flight baggage performed;\n");

                        if (baggageJObj != null)
                            jObj.Add("Baggage_response", baggageJObj.ToString());
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("GetSelectedFlight performed;\n");

                    jObj.Add(procesType + "_LOG", sbLog.ToString());

                    addIdentifiers(ref jObj);

                    normalizeFlightNumbers(ref jObj);

                    return jObj;
                }

                if (!gotSelectedFlight && gotSessionIdentifier && gotGroupedFlightResultIdentifier && gotSelectedGroupedFlightOptionIdentifier && gotSelectedLegOptionsIdentifiers)
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting with using session identifiers\n");

                    _sessionIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SessionIdentifier");
                    _groupedResultIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("GroupedFlightResultIdentifier");
                    _selectedGroupedFlightOptionIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SelectedGroupedFlightOptionIdentifier");
                    _selectedLegOptionsIdentifiers = pScrapeInfo.GetScrapeInfoValueFromName("SelectedLegOptionsIdentifiers");

                    _urlPart = pScrapeInfo.GetScrapeInfoValueFromName("WS_URLPart");

                    if (pScrapeInfo.CheckIfVariableExists("WS_PageSize"))
                        _pageSize = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("WS_PageSize"));

                    Airtrade.Connection ai = new Airtrade.Connection();

                    ai.setContentType(contentType.JSON);
                    ai.setReqType(requestType.POST);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight");
                    ai.requestTimeOut = piTimeout;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append(string.Format("\"GroupedFlightResultIdentifier\":\"{0}\",", _groupedResultIdentifier));
                    sb.Append(string.Format("\"SelectedGroupedFlightOptionIdentifier\":\"{0}\",", _selectedGroupedFlightOptionIdentifier));
                    sb.Append(string.Format("\"SelectedLegOptionsIdentifiers\":[{0}]", _selectedLegOptionsIdentifiers));
                    sb.Append("}");

                    jObj = ai.SendRequest(sb.ToString());

                    if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/SelectedFlight/"))
                    {

                        string search = "/SelectedFlight/";
                        string data = jObj["Data"].ToString();
                        _selectedFlightIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                        ai = new Airtrade.Connection();

                        ai.setContentType(contentType.FORM);
                        ai.setReqType(requestType.GET);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                        ai.requestTimeOut = piTimeout;

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Retrieving selected flight data with usersessions from {0};\n", ai.Uri));

                        jObj = ai.SendRequest();

                        gotSelectedFlight = true;

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving selected flight data with usersessions performed;\n");

                        //Retrieveing the baggage
                        if (jObj != null)
                        {
                            string baggageUrl = jObj.SelectToken("$..Passengers[0]._links.lookup:baggageserviceoptions.href").ToString();

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving selected flight baggage;\n");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.FORM);
                            ai.setReqType(requestType.GET);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + baggageUrl);
                            ai.requestTimeOut = piTimeout;

                            JObject baggageJObj = ai.SendRequest();

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving selected flight baggage performed;\n");

                            if (baggageJObj != null)
                                jObj.Add("Baggage_response", baggageJObj.ToString());
                        }

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("GetSelectedFlight performed;\n");

                        jObj.Add(procesType + "_LOG", sbLog.ToString());

                        addIdentifiers(ref jObj);

                        normalizeFlightNumbers(ref jObj);

                        return jObj;
                    }
                }

                //Otherwise get the selected flight by going through all the requests again
                if (!gotSelectedFlight)
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting with authentication\n");

                    jObj = Authentcate(pScrapeInfo, piTimeout);

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("Authentication ended\n");
                        if (jObj != null)
                            sbLog.Append(string.Format("sendRequest_Auth:{0}\n", (string)jObj["sendrequest_Auth"]));
                    }

                    if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/usersession/"))
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Authentication successful\n");

                        string data = (string)jObj["Data"];
                        string search = "/usersession/";
                        _sessionIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                        jObj = retrieveSingleFlights(ref pScrapeInfo, ref pFoundInfo, piTimeout, ref sbLog);

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Single flight retrieved; Finding the selected flight\n");

                        bool flightFound = false;

                        if (jObj != null && jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
                        {
                            string depDate = string.Format("{0}-{1}-{2}", pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH").ToString().PadLeft(2, '0'), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY").ToString().PadLeft(2, '0'));
                            string arrDate = DateTime.ParseExact(pScrapeInfo.GetScrapeInfoValueFromName("arrival day"), "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                            string depTime = pScrapeInfo.GetScrapeInfoValueFromName("departure time");
                            string arrTime = pScrapeInfo.GetScrapeInfoValueFromName("arrival time");

                            DateTime depDateTime = DateTime.ParseExact(depDate + " " + depTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                            DateTime arrDateTime = DateTime.ParseExact(arrDate + " " + arrTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                            _groupedResultIdentifier = jObj["Identifier"].ToString();

                            string carrierIATA = pScrapeInfo.GetScrapeInfoValueFromName("OperatingCarrierIATA");
                            carrierIATA = string.IsNullOrEmpty(carrierIATA) ? pFoundInfo.GetValueFromVariable("OperatingCarrierIATA") : carrierIATA;
                            carrierIATA = string.IsNullOrEmpty(carrierIATA) ? pScrapeInfo.GetScrapeInfoValueFromName("CaRrIeR_IaTa") : carrierIATA;
                            carrierIATA = string.IsNullOrEmpty(carrierIATA) ? pFoundInfo.GetValueFromVariable("CaRrIeR_IaTa") : carrierIATA;

                            bool ignoreCarrier = pScrapeInfo.CheckIfVariableExists("WS_IgnoreCarrier") ? pScrapeInfo.GetScrapeInfoValueFromName("WS_IgnoreCarrier").ToString().ToUpper().Equals("TRUE") : false;

                            if (pScrapeInfo.lbAutoErrorLog)
                            {
                                sbLog.Append("Starting with finding the selected flight\n");
                                sbLog.Append(string.Format("depDate:{0}, arrDate{1}, depTime{2}, arrTime{3}, --  carrierIATA ({4}) / ignoreCarrier ({5})\n", depDate, arrDate, depTime, arrTime, carrierIATA, ignoreCarrier.ToString()));
                            }

                            JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                            for (int i = 0; i < groupedFlightOptionsList.Count; i++)
                            {
                                JObject groupedFlightOption = (JObject)groupedFlightOptionsList[i];

                                //Only check for the proper IATA code unless we ignore the Carrier Iata code
                                if (!ignoreCarrier && !groupedFlightOption.SelectToken("$.Airline.Code").ToString().Equals(carrierIATA)) continue;

                                _selectedGroupedFlightOptionIdentifier = groupedFlightOption["Identifier"].ToString();

                                //Only for oneways
                                if (groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"] != null && ((JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"]).Count > 0)
                                {
                                    JArray legOptionInfoListsOutbound = (JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"];
                                    for (int j = 0; j < legOptionInfoListsOutbound.Count; j++)
                                    {
                                        JObject legOptionInfo = (JObject)legOptionInfoListsOutbound[j];
                                        int nrOfStops = (int)legOptionInfo["NumberOfStops"];
                                        if (nrOfStops > 1) continue;

                                        _selectedLegOptionsIdentifiers = legOptionInfo["Identifier"].ToString();

                                        string nodeDepartureDate = General.jSonDate(legOptionInfo["DepartureDate"]);
                                        string nodeDepartureTime = legOptionInfo["DepartureTime"].ToString();
                                        string nodeArrivalDate = General.jSonDate(legOptionInfo["ArrivalDate"]);
                                        string nodeArrivalTime = legOptionInfo["ArrivalTime"].ToString();

                                        DateTime nodeDepDateTime = DateTime.ParseExact(nodeDepartureDate + " " + nodeDepartureTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                                        DateTime nodeArrDateTime = DateTime.ParseExact(nodeArrivalDate + " " + nodeArrivalTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                                        TimeSpan depDiff = depDateTime - nodeDepDateTime;
                                        TimeSpan arrDiff = arrDateTime - nodeArrDateTime;

                                        bool depDiffOK = Math.Abs(depDiff.TotalMinutes) <= 5;
                                        bool arrDiffOK = Math.Abs(arrDiff.TotalMinutes) <= 5;

                                        if (depDiffOK && arrDiffOK)
                                        {
                                            _selectedLegOptionsIdentifiers = "\"" + _selectedLegOptionsIdentifiers.Replace("~", "\",\"") + "\"";
                                            flightFound = true;

                                            if (pScrapeInfo.lbAutoErrorLog)
                                            {
                                                sbLog.Append(string.Format("Match found depDateTime / nodeDepDateTime - arrDateTime / nodeArrDateTime : {0} / {1} - {2} / {3} \n", depDateTime, nodeDepDateTime, arrDateTime, nodeArrDateTime));
                                                sbLog.Append("Matched flight content: " + legOptionInfo.ToString() + "\n");
                                                sbLog.Append(string.Format("Matched identifiers--> _sessionIdentifier:{0}, _groupedResultIdentifier:{1}, _selectedGroupedFlightOptionIdentifier:{2}, _selectedLegOptionsIdentifiers: {3}", _sessionIdentifier, _groupedResultIdentifier, _selectedGroupedFlightOptionIdentifier, _selectedLegOptionsIdentifiers));
                                            }

                                            break;
                                        }

                                    }
                                }
                                if (flightFound) break;
                            }

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append(string.Format("Matching flight found: {0} \n", flightFound.ToString()));

                            if (flightFound)
                            {
                                Airtrade.Connection ai = new Airtrade.Connection();

                                ai.setContentType(contentType.JSON);
                                ai.setReqType(requestType.POST);
                                ai.setAuthType(authenticationType.NONE);
                                ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight");
                                ai.requestTimeOut = piTimeout;

                                StringBuilder sb = new StringBuilder();
                                sb.Append("{");
                                sb.Append(string.Format("\"GroupedFlightResultIdentifier\":\"{0}\",", _groupedResultIdentifier));
                                sb.Append(string.Format("\"SelectedGroupedFlightOptionIdentifier\":\"{0}\",", _selectedGroupedFlightOptionIdentifier));
                                sb.Append(string.Format("\"SelectedLegOptionsIdentifiers\":[{0}]", _selectedLegOptionsIdentifiers));
                                sb.Append("}");

                                jObj = ai.SendRequest(sb.ToString());

                                if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/SelectedFlight/"))
                                {
                                    if (pScrapeInfo.lbAutoErrorLog)
                                    {
                                        sbLog.Append(string.Format("Selected flight request details--> URL:{0}; Request send: {1}; Response received: {2} \n", ai.Uri, ai.requestSend, jObj.ToString()));
                                        sbLog.Append("Retrieving selected flight data;\n");
                                    }

                                    search = "/SelectedFlight/";
                                    data = jObj["Data"].ToString();
                                    _selectedFlightIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                                    ai = new Airtrade.Connection();

                                    ai.setContentType(contentType.FORM);
                                    ai.setReqType(requestType.GET);
                                    ai.setAuthType(authenticationType.NONE);
                                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                                    ai.requestTimeOut = piTimeout;

                                    jObj = ai.SendRequest();

                                    if (pScrapeInfo.lbAutoErrorLog)
                                    {
                                        sbLog.Append("Selected flight url: " + ai.Uri + " \n");
                                        sbLog.Append("Retrieving selected flight data performed;\n");
                                    }

                                    //Retrieving the baggage
                                    if (jObj != null)
                                    {
                                        string baggageUrl = jObj.SelectToken("$..Passengers[0]._links.lookup:baggageserviceoptions.href").ToString();

                                        if (pScrapeInfo.lbAutoErrorLog)
                                            sbLog.Append("Retrieving selected flight baggage;\n");

                                        ai = new Airtrade.Connection();

                                        ai.setContentType(contentType.FORM);
                                        ai.setReqType(requestType.GET);
                                        ai.setAuthType(authenticationType.NONE);
                                        ai.Uri = new Uri(_urlPart + baggageUrl);
                                        ai.requestTimeOut = piTimeout;

                                        JObject baggageJObj = ai.SendRequest();

                                        if (pScrapeInfo.lbAutoErrorLog)
                                            sbLog.Append("Retrieving selected flight baggage performed;\n");

                                        if (baggageJObj != null)
                                            jObj.Add("Baggage_response", baggageJObj.ToString());
                                    }

                                    jObj.Add(procesType + "_LOG", sbLog.ToString());

                                    addIdentifiers(ref jObj);

                                    normalizeFlightNumbers(ref jObj);

                                    return jObj;
                                }
                            }
                            else
                            {
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("No matching flight found in " + procesType + " process\n");

                                jObj.Add(procesType + "_LOG", sbLog.ToString());

                                return jObj;
                            }
                        }
                        else
                        {
                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("No SF response found in " + procesType + " process\n");

                            jObj.Add(procesType + "_LOG", sbLog.ToString());

                            return jObj;
                        }
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Authentication unsuccessful\n");

                        jObj.Add(procesType + "_LOG", sbLog.ToString());

                        return jObj;
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append(string.Format("\nException on verifyFlights method; Error message:{0}\n", e.Message));
                    jObj.Add("getSelectedFlight_LOG", sbLog.ToString());
                }
            }

            return jObj;
        }

        private JObject retrieveSingleSRFlights(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout, ref StringBuilder sbLog)
        {
            JObject jObj = null;

            #region Search Flight request
            //Sending the actual SF request
            if (pScrapeInfo.lbAutoErrorLog)
                sbLog.Append("Sending request for SRF;\n");

            jObj = getFlights(ref pScrapeInfo, ref pFoundInfo, piTimeout, true, ref sbLog);

            if (pScrapeInfo.lbAutoErrorLog)
            {
                sbLog.Append("Sending request for SRF performed;\n");
                if (jObj != null)
                    sbLog.Append(string.Format("searchflightRequestSend:{0}\n", (string)jObj["searchflightRequestSend"]));
            }

            #endregion

            if (jObj != null)
            {
                //We have got a valid response
                if (jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/GroupedResultList/"))
                {
                    #region Successful SF send

                    string data = (string)jObj["Data"];
                    string search = _urlPart;
                    _flightResultUrl = data.Substring(data.IndexOf(search) + search.Length);

                    search = "GroupedResultList/";
                    _groupedResultIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                    Airtrade.IConnection ai;

                    //Only perform a preffered carrier request if more then 1 carrier is preferred
                    //If only 1 carrier is preferred we can send it in the SF request // Allways send thecarrier even if it is 1
                    bool gotPreferred = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST"));
                    //gotPreferred = gotPreferred ? pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(",") : gotPreferred;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Got preffered carriers:{0};\n", gotPreferred.ToString()));

                    bool gotFlights = false;

                    string lastError = null;

                    #region prefferred carriers

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting preffered carriers:;\n");

                    StringBuilder sb = new StringBuilder();

                    sb.Append("{\"filterCriteria\":{");

                    if (gotPreferred)
                    {
                        sb.Append("\"Airline\":{\"SelectedAirlineCodes\":[");
                        int ctr = 0;
                        if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Contains(","))
                        {
                            string[] items = pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST").Split(',');
                            for (int c = 0; c < items.Length; c++)
                            {
                                if (!string.IsNullOrEmpty(items[c].Trim()) && !pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL").Contains(items[c].Trim()))
                                {
                                    if (ctr > 0)
                                        sb.Append(",");
                                    sb.Append(string.Format("\"{0}\"", items[c].Trim()));

                                    ctr++;
                                }
                            }
                        }
                        else
                            sb.Append(string.Format("\"{0}\"", pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLIST")));
                        sb.Append("]}");
                    }

                    sb.Append("},\"sortingCriteria\":{\"Code\":\"Default\",\"Direction\":\"Ascending\"}");

                    if (!_pageSize.Equals(int.MinValue))
                        sb.Append(",\"PagingCriteria\":{\"PageSize\":" + _pageSize.ToString() + "}");

                    sb.Append("}");

                    ai = new Airtrade.Connection();

                    ai.setContentType(contentType.JSON);
                    ai.setReqType(requestType.PUT);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/FlightOptions/GroupedResultList/" + _groupedResultIdentifier);
                    ai.requestTimeOut = piTimeout;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Sending preffered carriers request to {0};\n", ai.Uri));

                    jObj = ai.SendRequest(sb.ToString());

                    if (jObj != null && (JArray)jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
                    {
                        gotFlights = true;

                        if (pScrapeInfo.lbAutoErrorLog)
                        {
                            sbLog.Append("preferred_carriers_request." + ai.requestSend + "\n");
                            sbLog.Append("Sending preffered carriers request performed;\n");
                        }
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Preffered carriers request performed incorrectly:{0};\n", ai.requestSend));

                        lastError = ai.LastError;
                    }

                    #endregion


                    int pageSize = jObj.SelectToken("$..PageSize") != null ? Convert.ToInt32(jObj.SelectToken("$..PageSize")) : int.MinValue;


                    if (!gotFlights)
                    {
                        ai = new Airtrade.Connection();

                        ai.setContentType(contentType.FORM);
                        ai.setReqType(requestType.GET);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + "/" + _flightResultUrl);
                        ai.requestTimeOut = piTimeout;

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Retrieving actual flight data from {0};\n", ai.Uri));

                        jObj = ai.SendRequest();

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving actual flight data performed;\n");

                        if (jObj == null)
                            lastError = ai.LastError;
                    }

                    if (jObj != null)
                    {
                        #region removing excluded carriers
                        //Since there is no excluding request in the API, the result has to be filtered

                        bool gotProhibited = !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL"));
                        if (gotProhibited)
                        {
                            JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                            for (int i = groupedFlightOptionsList.Count; i > 0; i--)
                            {
                                if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_CARLISTEXCL").Contains(groupedFlightOptionsList[i - 1]["Airline"]["Code"].ToString()))
                                    jObj["_embedded"]["GroupedFlightOptions"][i - 1].Remove();
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Retrieving actual flight data has failed: {0};\n", lastError));
                    }

                    //CreateFareDetail(ref jObj, pScrapeInfo);

                    //Remove '0' before flightnumbers
                    normalizeFlightNumbers(ref jObj);

                    jObj.Add("SRF_LOG", sbLog.ToString());

                    #endregion
                }
                else
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("No valid request for sending flights performed;\n");

                    jObj.Add("SRF_LOG", sbLog.ToString());
                }
            }
            else
            {
                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append("No GroupedResultList url returned;\n");

                jObj.Add("SRF_LOG", sbLog.ToString());
            }

            return jObj;
        }

        private JObject getSelectedSRFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, int piTimeout, string procesType, ref StringBuilder sbLog)
        {
            JObject jObj = null;

            if (pScrapeInfo.lbAutoErrorLog)
                sbLog.Append("Starting with getSelectedSRFlight\n");

            try
            {
                //First check if we can still use the session identifiers to get the specific flight
                bool gotSessionIdentifier = pScrapeInfo.CheckIfVariableExists("SessionIdentifier");
                bool gotGroupedFlightResultIdentifier = pScrapeInfo.CheckIfVariableExists("GroupedFlightResultIdentifier");
                bool gotSelectedGroupedFlightOptionIdentifier = pScrapeInfo.CheckIfVariableExists("SelectedGroupedFlightOptionIdentifier");
                bool gotSelectedLegOptionsIdentifiers = pScrapeInfo.CheckIfVariableExists("SelectedLegOptionsIdentifiers");
                bool gotSelectedFlightIdentifier = pScrapeInfo.CheckIfVariableExists("SelectedFlightIdentifier");

                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append(string.Format("Checking session identifiers SF --> SessionIdentifier:{0}, GroupedFlightResultIdentifier:{1}, SelectedGroupedFlightOptionIdentifier:{2}, SelectedLegOptionsIdentifiers:{3}, SelectedFlightIdentifier:{4}\n", gotSessionIdentifier.ToString(), gotGroupedFlightResultIdentifier.ToString(), gotSelectedGroupedFlightOptionIdentifier.ToString(), gotSelectedLegOptionsIdentifiers.ToString(), gotSelectedFlightIdentifier.ToString()));
                

                bool gotSelectedFlight = false;

                #region Using Session identifiers
                if (!gotSelectedFlight && gotSessionIdentifier && gotSelectedFlightIdentifier)
                {
                    _urlPart = pScrapeInfo.GetScrapeInfoValueFromName("WS_URLPart");
                    _sessionIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SessionIdentifier");
                    _selectedFlightIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SelectedFlightIdentifier");

                    Airtrade.Connection ai = new Airtrade.Connection();

                    ai.setContentType(contentType.FORM);
                    ai.setReqType(requestType.GET);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                    ai.requestTimeOut = piTimeout;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append(string.Format("Retrieving selected flight data with selectedFlightIdentifier from {0};\n", ai.Uri));

                    jObj = ai.SendRequest();

                    gotSelectedFlight = true;

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Retrieving selected flight data with selectedFlightIdentifier performed;\n");

                    //Retrieveing the baggage
                    if (jObj != null)
                    {
                        string baggageUrl = jObj.SelectToken("$..Passengers[0]._links.lookup:baggageserviceoptions.href").ToString();

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving selected flight baggage;\n");

                        ai = new Airtrade.Connection();

                        ai.setContentType(contentType.FORM);
                        ai.setReqType(requestType.GET);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + baggageUrl);
                        ai.requestTimeOut = piTimeout;

                        JObject baggageJObj = ai.SendRequest();

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving selected flight baggage performed;\n");

                        if (baggageJObj != null)
                            jObj.Add("Baggage_response", baggageJObj.ToString());
                    }

                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("GetSelectedFlight performed;\n");

                    jObj.Add(procesType + "_LOG", sbLog.ToString());

                    addIdentifiers(ref jObj);

                    normalizeFlightNumbers(ref jObj);

                    return jObj;
                }

                if (!gotSelectedFlight && gotSessionIdentifier && gotGroupedFlightResultIdentifier && gotSelectedGroupedFlightOptionIdentifier && gotSelectedLegOptionsIdentifiers)
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting with using session identifiers\n");

                    _sessionIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SessionIdentifier");
                    _groupedResultIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("GroupedFlightResultIdentifier");
                    _selectedGroupedFlightOptionIdentifier = pScrapeInfo.GetScrapeInfoValueFromName("SelectedGroupedFlightOptionIdentifier");
                    _selectedLegOptionsIdentifiers = pScrapeInfo.GetScrapeInfoValueFromName("SelectedLegOptionsIdentifiers");

                    _urlPart = pScrapeInfo.GetScrapeInfoValueFromName("WS_URLPart");

                    if (pScrapeInfo.CheckIfVariableExists("WS_PageSize"))
                        _pageSize = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("WS_PageSize"));

                    Airtrade.Connection ai = new Airtrade.Connection();

                    ai.setContentType(contentType.JSON);
                    ai.setReqType(requestType.POST);
                    ai.setAuthType(authenticationType.NONE);
                    ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight");
                    ai.requestTimeOut = piTimeout;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("{");
                    sb.Append(string.Format("\"GroupedFlightResultIdentifier\":\"{0}\",", _groupedResultIdentifier));
                    sb.Append(string.Format("\"SelectedGroupedFlightOptionIdentifier\":\"{0}\",", _selectedGroupedFlightOptionIdentifier));
                    sb.Append(string.Format("\"SelectedLegOptionsIdentifiers\":[{0}]", _selectedLegOptionsIdentifiers));
                    sb.Append("}");

                    jObj = ai.SendRequest(sb.ToString());

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("Recieving the selected flight data based on session variables:\n");
                        sbLog.Append("Sending request to: " + ai.Uri + " \n");
                        sbLog.Append("Request sent: " + ai.requestSend + " \n");
                        if (jObj != null)
                            sbLog.Append("Response received:" + jObj.ToString() + " \n");
                    }

                    if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/SelectedFlight/"))
                    {
                        string search = "/SelectedFlight/";
                        string data = jObj["Data"].ToString();
                        _selectedFlightIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                        ai = new Airtrade.Connection();

                        ai.setContentType(contentType.FORM);
                        ai.setReqType(requestType.GET);
                        ai.setAuthType(authenticationType.NONE);
                        ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                        ai.requestTimeOut = piTimeout;

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(string.Format("Retrieving selected flight data with usersessions from {0};\n", ai.Uri));

                        jObj = ai.SendRequest();

                        gotSelectedFlight = true;

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Retrieving selected flight data with usersessions performed;\n");

                        //Retrieving the baggage
                        if (jObj != null)
                        {
                            string baggageUrl = jObj.SelectToken("$..Passengers[0]._links.lookup:baggageserviceoptions.href").ToString();

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving selected flight baggage;\n");

                            ai = new Airtrade.Connection();

                            ai.setContentType(contentType.FORM);
                            ai.setReqType(requestType.GET);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + baggageUrl);
                            ai.requestTimeOut = piTimeout;

                            JObject baggageJObj = ai.SendRequest();

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving selected flight baggage performed;\n");

                            if (baggageJObj != null)
                                jObj.Add("Baggage_response", baggageJObj.ToString());
                        }

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("GetSelectedFlight performed;\n");

                        jObj.Add(procesType + "_LOG", sbLog.ToString());

                        addIdentifiers(ref jObj);

                        normalizeFlightNumbers(ref jObj);

                        return jObj;
                    }
                    else
                    {
                        //We havent found the expected result from the selected flight data
                        //We then have to perfrom a regular search which needs the carrier list to be empty
                        pScrapeInfo.InsertVariable("SCR_CARLIST", "", true);
                    }
                }
                #endregion

                //Otherwise get the selected flight by going through all the requests again
                if (!gotSelectedFlight)
                {
                    if (pScrapeInfo.lbAutoErrorLog)
                        sbLog.Append("Starting with authentication\n");

                    jObj = Authentcate(pScrapeInfo, piTimeout);

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("Authentication ended\n");
                        if (jObj != null)
                            sbLog.Append(string.Format("sendRequest_Auth:{0}\n", (string)jObj["sendrequest_Auth"]));
                    }

                    if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/usersession/"))
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Authentication successful\n");

                        string data = (string)jObj["Data"];
                        string search = "/usersession/";
                        _sessionIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Sending request for SRF flights;\n");

                        jObj = retrieveSingleSRFlights(ref pScrapeInfo, ref pFoundInfo, piTimeout, ref sbLog);

                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append(" SRF flights retrieved; Finding the selected flight\n");

                        bool flightFound = false;

                        if (jObj != null && jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
                        {
                            DateTime depDateTime_Out = DateTime.ParseExact(string.Format("{0}-{1}-{2} {3}", pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Outward"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH_Outward").ToString().PadLeft(2, '0'), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY_Outward").ToString().PadLeft(2, '0'), pScrapeInfo.GetScrapeInfoValueFromName("departure time_Outward")), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                            DateTime arrDateTime_Out = DateTime.ParseExact(string.Format("{0}-{1}-{2} {3}", pScrapeInfo.GetScrapeInfoValueFromName("arrival day_Outward").Split('.')[2], pScrapeInfo.GetScrapeInfoValueFromName("arrival day_Outward").Split('.')[1], pScrapeInfo.GetScrapeInfoValueFromName("arrival day_Outward").Split('.')[0], pScrapeInfo.GetScrapeInfoValueFromName("arrival time_Outward")), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                            DateTime depDateTime_Ret = DateTime.ParseExact(string.Format("{0}-{1}-{2} {3}", pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Return"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH_Return").ToString().PadLeft(2, '0'), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY_Return").ToString().PadLeft(2, '0'), pScrapeInfo.GetScrapeInfoValueFromName("departure time_Return")), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                            DateTime arrDateTime_Ret = DateTime.ParseExact(string.Format("{0}-{1}-{2} {3}", pScrapeInfo.GetScrapeInfoValueFromName("arrival day_Return").Split('.')[2], pScrapeInfo.GetScrapeInfoValueFromName("arrival day_Return").Split('.')[1], pScrapeInfo.GetScrapeInfoValueFromName("arrival day_Return").Split('.')[0], pScrapeInfo.GetScrapeInfoValueFromName("arrival time_Return")), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                            string carrierIATA = pScrapeInfo.GetScrapeInfoValueFromName("CaRrIeR_IaTa_Outward");
                            bool ignoreCarrier = pScrapeInfo.CheckIfVariableExists("WS_IgnoreCarrier") ? pScrapeInfo.GetScrapeInfoValueFromName("WS_IgnoreCarrier").ToString().ToUpper().Equals("TRUE") : false;

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append(string.Format("Checking SRF for depDateTime_Out ({0}), arrDateTime_Out ({1}), depDateTime_Ret ({2}) and arrDateTime_Ret ({3}) --  carrierIATA ({4}) / ignoreCarrier ({5})\n;", depDateTime_Out.ToString("yyyy-MM-dd HH:mm"), arrDateTime_Out.ToString("yyyy-MM-dd HH:mm"), depDateTime_Ret.ToString("yyyy-MM-dd HH:mm"), arrDateTime_Ret.ToString("yyyy-MM-dd HH:mm"), carrierIATA, ignoreCarrier.ToString()));

                            JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                            for (int i = 0; i < groupedFlightOptionsList.Count; i++)
                            {
                                JObject groupedFlightOption = (JObject)groupedFlightOptionsList[i];

                                //Only check for the proper IATA code unless we ignore the Carrier Iata code
                                if (!ignoreCarrier && !groupedFlightOption.SelectToken("$.Airline.Code").ToString().Equals(carrierIATA)) continue;

                                _selectedGroupedFlightOptionIdentifier = groupedFlightOption["Identifier"].ToString();

                                JArray legOptionInfoListsOutbound = (JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"];
                                JArray legOptionInfoListsInbound = (JArray)groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"];

                                for (int j = 0; j < legOptionInfoListsOutbound.Count; j++)
                                {
                                    JObject legOptionInfoOut = (JObject)legOptionInfoListsOutbound[j];
                                    int nrOfStopsOut = (int)legOptionInfoOut["NumberOfStops"];
                                    if (nrOfStopsOut > 1) continue;

                                    string legOptionsIdentifierOut = legOptionInfoOut["Identifier"].ToString();

                                    string departureDateOut = General.jSonDate(legOptionInfoOut["DepartureDate"]);
                                    string departureTimeOut = legOptionInfoOut["DepartureTime"].ToString();
                                    string arrivalDateOut = General.jSonDate(legOptionInfoOut["ArrivalDate"]);
                                    string arrivalTimeOut = legOptionInfoOut["ArrivalTime"].ToString();

                                    DateTime nodeDepDateTime_Out = DateTime.ParseExact(departureDateOut + " " + departureTimeOut, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                                    DateTime nodeArrDateTime_Out = DateTime.ParseExact(arrivalDateOut + " " + arrivalTimeOut, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                                    TimeSpan depDiffOut = depDateTime_Out - nodeDepDateTime_Out;
                                    TimeSpan arrDiffOut = arrDateTime_Out - nodeArrDateTime_Out;

                                    bool depDiffOutOK = Math.Abs(depDiffOut.TotalMinutes) <= 5;
                                    bool arrDiffOutOK = Math.Abs(arrDiffOut.TotalMinutes) <= 5;

                                    //if (pScrapeInfo.lbAutoErrorLog)
                                    //{
                                    //    sbLog.Append(string.Format("Checking SRF for nodeDepDateTime_Out ({0}), nodeArrDateTime_Out ({1})\n;", nodeArrDateTime_Out.ToString("yyyy-MM-dd HH:mm"), nodeArrDateTime_Out.ToString("yyyy-MM-dd HH:mm"), depDateTime_Ret.ToString("yyyy-MM-dd HH:mm"), arrDateTime_Ret.ToString("yyyy-MM-dd HH:mm")));
                                    //    sbLog.Append(string.Format("Checking SRF depDiffOutOK ({0}) and arrDiffOutOK ({1});\n", depDiffOutOK.ToString(), arrDiffOutOK.ToString()));
                                    //}

                                    //only look in the inbound nodes if the outbound node has been found
                                    if (depDiffOutOK && arrDiffOutOK)
                                    {
                                        for (int t = 0; t < legOptionInfoListsInbound.Count; t++)
                                        {
                                            JObject legOptionInfoRet = (JObject)legOptionInfoListsInbound[t];
                                            int nrOfStopsRet = (int)legOptionInfoRet["NumberOfStops"];
                                            if (nrOfStopsRet > 1) continue;

                                            string legOptionsIdentifierRet = legOptionInfoRet["Identifier"].ToString();

                                            string departureDateRet = General.jSonDate(legOptionInfoRet["DepartureDate"]);
                                            string departureTimeret = legOptionInfoRet["DepartureTime"].ToString();
                                            string arrivalDateRet = General.jSonDate(legOptionInfoRet["ArrivalDate"]);
                                            string arrivalTimeRet = legOptionInfoRet["ArrivalTime"].ToString();

                                            DateTime nodeDepDateTime_Ret = DateTime.ParseExact(departureDateRet + " " + departureTimeret, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                                            DateTime nodeArrDateTime_Ret = DateTime.ParseExact(arrivalDateRet + " " + arrivalTimeRet, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                                            TimeSpan depDiffRet = depDateTime_Ret - nodeDepDateTime_Ret;
                                            TimeSpan arrDiffRet = arrDateTime_Ret - nodeArrDateTime_Ret;

                                            bool depDiffRetOK = Math.Abs(depDiffRet.TotalMinutes) <= 5;
                                            bool arrDiffRetOK = Math.Abs(arrDiffRet.TotalMinutes) <= 5;

                                            //if (pScrapeInfo.lbAutoErrorLog)
                                            //{
                                            //    sbLog.Append(string.Format("Checking SRF for nodeDepDateTime_Ret ({0}), nodeArrDateTime_Ret ({1})\n;", nodeDepDateTime_Ret.ToString("yyyy-MM-dd HH:mm"), nodeArrDateTime_Ret.ToString("yyyy-MM-dd HH:mm"), depDateTime_Ret.ToString("yyyy-MM-dd HH:mm"), arrDateTime_Ret.ToString("yyyy-MM-dd HH:mm")));
                                            //    sbLog.Append(string.Format("Checking SRF depDiffRetOK ({0}) and arrDiffRetOK ({1});\n", depDiffRetOK.ToString(), arrDiffRetOK.ToString()));
                                            //}

                                            if (depDiffOutOK && arrDiffOutOK && depDiffRetOK && arrDiffRetOK)
                                            {
                                                _selectedLegOptionsIdentifiers = "\"" + legOptionsIdentifierOut + "\",\"" + legOptionsIdentifierRet + "\"";
                                                flightFound = true;

                                                if (pScrapeInfo.lbAutoErrorLog)
                                                {
                                                    sbLog.Append(string.Format("Match found depDateTime_Out / nodeDepDateTime_Out - arrDateTime_Out / nodeArrDateTime_Out - depDateTime_Ret / nodeDepDateTime_Ret - arrDateTime_Ret / nodeArrDateTime_Ret: {0} / {1} - {2} / {3} - {4} / {5} - {6} / {7} \n", depDateTime_Out, nodeDepDateTime_Out, arrDateTime_Out, arrDateTime_Out, depDateTime_Ret, nodeDepDateTime_Ret, arrDateTime_Ret, nodeArrDateTime_Ret));
                                                    sbLog.Append("Matched flight content: " + legOptionInfoOut.ToString() + "---" + legOptionInfoRet.ToString() + "\n");
                                                    sbLog.Append(string.Format("Matched identifiers--> _sessionIdentifier:{0}, _groupedResultIdentifier:{1}, _selectedGroupedFlightOptionIdentifier:{2}, _selectedLegOptionsIdentifiers:{3}", _sessionIdentifier, _groupedResultIdentifier, _selectedGroupedFlightOptionIdentifier, _selectedLegOptionsIdentifiers));
                                                }

                                                break;
                                            }
                                        }
                                    }
                                    if (flightFound) break;
                                }
                                if (flightFound) break;
                            }
                        }

                        if (flightFound)
                        {
                            _urlPart = pScrapeInfo.GetScrapeInfoValueFromName("WS_URLPart");

                            if (pScrapeInfo.CheckIfVariableExists("WS_PageSize"))
                                _pageSize = Convert.ToInt32(pScrapeInfo.GetScrapeInfoValueFromName("WS_PageSize"));

                            Airtrade.Connection ai = new Airtrade.Connection();

                            ai.setContentType(contentType.JSON);
                            ai.setReqType(requestType.POST);
                            ai.setAuthType(authenticationType.NONE);
                            ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight");
                            ai.requestTimeOut = piTimeout;

                            StringBuilder sb = new StringBuilder();
                            sb.Append("{");
                            sb.Append(string.Format("\"GroupedFlightResultIdentifier\":\"{0}\",", _groupedResultIdentifier));
                            sb.Append(string.Format("\"SelectedGroupedFlightOptionIdentifier\":\"{0}\",", _selectedGroupedFlightOptionIdentifier));
                            sb.Append(string.Format("\"SelectedLegOptionsIdentifiers\":[{0}]", _selectedLegOptionsIdentifiers));
                            sb.Append("}");

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append(string.Format("Retrieving matched SRF flight from {0} sending the following request: {1};\n", ai.Uri, sb.ToString()));

                            jObj = ai.SendRequest(sb.ToString());

                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("Retrieving matched SRF flight performed;\n");

                            if (jObj != null && jObj["$id"] != null && jObj["Data"] != null && ((string)jObj["Data"]).Contains("/SelectedFlight/"))
                            {
                                search = "/SelectedFlight/";
                                data = jObj["Data"].ToString();
                                _selectedFlightIdentifier = data.Substring(data.IndexOf(search) + search.Length);

                                ai = new Airtrade.Connection();

                                ai.setContentType(contentType.FORM);
                                ai.setReqType(requestType.GET);
                                ai.setAuthType(authenticationType.NONE);
                                ai.Uri = new Uri(_urlPart + "/" + _sessionIdentifier + "/SelectedFlight/" + _selectedFlightIdentifier);
                                ai.requestTimeOut = piTimeout;

                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append(string.Format("Retrieving selected flight data with usersessions from {0};\n", ai.Uri));

                                jObj = ai.SendRequest();

                                gotSelectedFlight = true;

                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Retrieving selected flight data with usersessions performed;\n");

                                //Retrieving the baggage 
                                if (jObj != null)
                                {
                                    string baggageUrl = jObj.SelectToken("$..Passengers[0]._links.lookup:baggageserviceoptions.href").ToString();

                                    if (pScrapeInfo.lbAutoErrorLog)
                                        sbLog.Append("Retrieving selected flight baggage;\n");

                                    ai = new Airtrade.Connection();

                                    ai.setContentType(contentType.FORM);
                                    ai.setReqType(requestType.GET);
                                    ai.setAuthType(authenticationType.NONE);
                                    ai.Uri = new Uri(_urlPart + baggageUrl);
                                    ai.requestTimeOut = piTimeout;

                                    JObject baggageJObj = ai.SendRequest();

                                    if (pScrapeInfo.lbAutoErrorLog)
                                        sbLog.Append("Retrieving selected flight baggage performed;\n");

                                    if (baggageJObj != null)
                                        jObj.Add("Baggage_response", baggageJObj.ToString());
                                }

                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("GetSelectedFlight performed;\n");

                                jObj.Add(procesType + "_LOG", sbLog.ToString());

                                addIdentifiers(ref jObj);

                                normalizeFlightNumbers(ref jObj);

                                return jObj;
                            }
                            else
                            {
                                if (pScrapeInfo.lbAutoErrorLog)
                                    sbLog.Append("Retrieving matching SRF flight failed\n");

                                jObj = new JObject();

                                jObj.Add(procesType + "_LOG", sbLog.ToString());

                                return jObj;
                            }
                        }
                        else
                        {
                            if (pScrapeInfo.lbAutoErrorLog)
                                sbLog.Append("No matching flight found\n");

                            jObj = new JObject();

                            jObj.Add(procesType + "_LOG", sbLog.ToString());

                            return jObj;
                        }
                    }
                    else
                    {
                        if (pScrapeInfo.lbAutoErrorLog)
                            sbLog.Append("Authentication unsuccessful\n");

                        jObj.Add(procesType + "_LOG", sbLog.ToString());

                        return jObj;
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    jObj = new JObject();

                    sbLog.Append(string.Format("\nException on getSelectedSRFlight method; Error message:{0}\n", e.Message));
                    jObj.Add("getSelectedSRFlight_LOG", sbLog.ToString());
                }
            }

            return jObj;
        }

        private List<string> MFC2Items(ScrapeInfo pScrapeInfo)
        {
            List<string> selectedFareClasses = new List<string>();

            string mfc = pScrapeInfo.GetScrapeInfoValueFromName("REQ_MFC");

            if (!string.IsNullOrEmpty(mfc))
            {
                if (mfc.Substring(3, 1).Equals("1"))
                    selectedFareClasses.Add("Economy");
                if (mfc.Substring(2, 1).Equals("1"))
                    selectedFareClasses.Add("PremiumEconomy");
                if (mfc.Substring(1, 1).Equals("1"))
                    selectedFareClasses.Add("Business");
                if (mfc.Substring(0, 1).Equals("1"))
                    selectedFareClasses.Add("First");
            }

            return selectedFareClasses;
        }

        private JObject CreateFareDetail(ref JObject jObj, ScrapeInfo pScrapeInfo)
        {
            if (jObj != null && jObj["_embedded"] != null && jObj["_embedded"]["GroupedFlightOptions"] != null && ((JArray)jObj["_embedded"]["GroupedFlightOptions"]).Count > 0)
            {
                JArray groupedFlightOptionsList = (JArray)jObj["_embedded"]["GroupedFlightOptions"];
                for (int i = 0; i < groupedFlightOptionsList.Count; i++)
                {
                    JObject groupedFlightOption = (JObject)groupedFlightOptionsList[i];

                    StringBuilder sb = new StringBuilder();

                    sb.Append("{\"LEG_CL_Content\":[");

                    string fareBasis = string.Empty;
                    string[] FareSourceInfos = General.getToken(groupedFlightOption, "", "$.FareSourceInfo").Split('/');
                    if (FareSourceInfos.Length > 0)
                        fareBasis = FareSourceInfos[FareSourceInfos.Length - 1];

                    StringBuilder sbLegOut = new StringBuilder();
                    StringBuilder sbLegRet = new StringBuilder();

                    if (groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"] != null && ((JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"]).Count > 0)
                    {
                        int ctr = 0;
                        JArray legOptionInfoListsOutbound = (JArray)groupedFlightOption["LegOptionInfoLists"][0]["LegOptionInfos"];
                        for (int j = 0; j < legOptionInfoListsOutbound.Count; j++)
                        {
                            JObject legOptionInfo = (JObject)legOptionInfoListsOutbound[j];
                            int nrOfStops = (int)legOptionInfo["NumberOfStops"];
                            if (nrOfStops > 1) continue;

                            if (j > 0)
                                sbLegOut.Append(",");

                            sbLegOut.Append(string.Format("\"LegoptionInfoOUT{0}\":[", j));

                            ctr++;

                            JArray TicketPrices = (JArray)groupedFlightOption["PriceBreakdown"]["TicketPrices"];
                            for (int x = 0; x < TicketPrices.Count; x++)
                            {
                                JObject ticketPrice = (JObject)TicketPrices[x];

                                if (x > 0)
                                    sbLegOut.Append(",");

                                sbLegOut.Append("{");

                                sbLegOut.Append(string.Format("\"BC\":\"{0}\",", General.getToken(legOptionInfo, "", "$.BookingClass")));

                                string fc = string.Empty;

                                string cabinClass = "Economy";
                                List<string> selectedFareClasses = MFC2Items(pScrapeInfo);
                                if (selectedFareClasses != null && selectedFareClasses.Count > 0)
                                {
                                    //Get the first available cabin class
                                    cabinClass = selectedFareClasses[0].ToString();
                                }
                                switch (cabinClass.ToLower())
                                {
                                    case "economy": fc = "0"; break;
                                    case "premiumeconomy": fc = "1"; break;
                                    case "business": fc = "2"; break;
                                    case "first": fc = "3"; break;
                                }
                                sbLegOut.Append(string.Format("\"FC\":{0},", fc));

                                string pt = string.Empty;
                                switch (General.getToken(ticketPrice, "", "$.PaxType"))
                                {
                                    case "Adult": pt = "0"; break;
                                    case "Child": pt = "1"; break;
                                    case "Infant": pt = "2"; break;
                                }

                                sbLegOut.Append(string.Format("\"PT\":{0},", pt));
                                sbLegOut.Append(string.Format("\"SN\":{0}", j));

                                sbLegOut.Append("}");
                            }

                            sbLegOut.Append("]");
                        }
                    }

                    sb.Append("{\"fare_outbound\":{\"FAI\":\"");
                    sb.Append(fareBasis);
                    sb.Append("\",\"LEG_FCL\":{");
                    sb.Append(sbLegOut.ToString());
                    sb.Append("}}}");

                    bool isOneWay = groupedFlightOption["LegOptionInfoLists"] != null && (((JArray)groupedFlightOption["LegOptionInfoLists"]).Count.Equals(1));

                    if (!isOneWay)
                    {
                        if (groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"] != null && ((JArray)groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"]).Count > 0)
                        {
                            int ctr = 0;
                            JArray legOptionInfoListsOutbound = (JArray)groupedFlightOption["LegOptionInfoLists"][1]["LegOptionInfos"];
                            for (int j = 0; j < legOptionInfoListsOutbound.Count; j++)
                            {
                                JObject legOptionInfo = (JObject)legOptionInfoListsOutbound[j];
                                int nrOfStops = (int)legOptionInfo["NumberOfStops"];
                                if (nrOfStops > 1) continue;

                                if (j > 0)
                                    sbLegRet.Append(",");

                                sbLegRet.Append(string.Format("\"LegoptionInfoRET{0}\":[", j));

                                ctr++;

                                JArray TicketPrices = (JArray)groupedFlightOption["PriceBreakdown"]["TicketPrices"];
                                for (int x = 0; x < TicketPrices.Count; x++)
                                {
                                    JObject ticketPrice = (JObject)TicketPrices[x];

                                    if (x > 0)
                                        sbLegRet.Append(",");

                                    sbLegRet.Append("{");

                                    sbLegRet.Append(string.Format("\"BC\":\"{0}\",", General.getToken(legOptionInfo, "", "$.BookingClass")));

                                    string fc = string.Empty;

                                    string cabinClass = "Economy";
                                    List<string> selectedFareClasses = MFC2Items(pScrapeInfo);
                                    if (selectedFareClasses != null && selectedFareClasses.Count > 0)
                                    {
                                        //Get the first available cabin class
                                        cabinClass = selectedFareClasses[0].ToString();
                                    }
                                    switch (cabinClass.ToLower())
                                    {
                                        case "economy": fc = "0"; break;
                                        case "premiumeconomy": fc = "1"; break;
                                        case "business": fc = "2"; break;
                                        case "first": fc = "3"; break;
                                    }
                                    sbLegRet.Append(string.Format("\"FC\":{0},", fc));

                                    string pt = string.Empty;
                                    switch (General.getToken(ticketPrice, "", "$.PaxType"))
                                    {
                                        case "Adult": pt = "0"; break;
                                        case "Child": pt = "1"; break;
                                        case "Infant": pt = "2"; break;
                                    }

                                    sbLegRet.Append(string.Format("\"PT\":{0},", pt));
                                    sbLegRet.Append(string.Format("\"SN\":{0}", j));

                                    sbLegRet.Append("}");
                                }

                                sbLegRet.Append("]");
                            }
                        }

                        sb.Append(",{\"fare_inbound\":{\"FAI\":\"");
                        sb.Append(fareBasis);
                        sb.Append("\",\"LEG_FCL\":{");
                        sb.Append(sbLegRet.ToString());
                        sb.Append("}}}");

                    }

                    sb.Append("]}");

                    var legCLContent = JObject.Parse(sb.ToString());

                    groupedFlightOption.Property("TotalNumberOfInfants").AddAfterSelf(new JProperty("AdditionalLegInfo", legCLContent));

                    //Added this to create an endnode for the additional leg info
                    groupedFlightOption.Property("AdditionalLegInfo").AddAfterSelf(new JProperty("EndAdditionalLegInfo", ""));

                }
            }

            return jObj;
        }

        private void addIdentifiers(ref JObject jObj)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{\"SelectedIdentifiers\":{");
            sb.Append(string.Format("\"SessionIdentifier\":\"{0}\",\"GroupedFlightResultIdentifier\":\"{1}\",\"SelectedGroupedFlightOptionIdentifier\":\"{2}\",\"SelectedLegOptionsIdentifiers\":\"{3}\", \"_selectedFlightIdentifier\":\"{4}\", \"ProductOrderIdentifier\":\"{5}\"", _sessionIdentifier, _groupedResultIdentifier, _selectedGroupedFlightOptionIdentifier, _selectedLegOptionsIdentifiers.Replace("\"", "").Replace("\",\"", "~"), _selectedFlightIdentifier, _orderIdentifier));
            sb.Append("}}");

            var newContent = JObject.Parse(sb.ToString());

            jObj.Last.AddAfterSelf(new JProperty("", newContent));
        }

        private string createAPIDate(DateTime date)
        {
            string dateAPI = date.ToString("dd MMM yyyy");

            string dd = date.ToString("dd");
            string yyyy = date.ToString("yyyy");
            string MMM = date.ToString("MMM");
            switch (Convert.ToInt32(date.ToString("MM")))
            {
                case 1: MMM = "Jan"; break;
                case 2: MMM = "Feb"; break;
                case 3: MMM = "Mar"; break;
                case 4: MMM = "Apr"; break;
                case 5: MMM = "May"; break;
                case 6: MMM = "Jun"; break;
                case 7: MMM = "Jul"; break;
                case 8: MMM = "Aug"; break;
                case 9: MMM = "Sep"; break;
                case 10: MMM = "Oct"; break;
                case 11: MMM = "Nov"; break;
                case 12: MMM = "Dec"; break;
            }

            return string.Format("{0} {1} {2}", dd, MMM, yyyy);
        }

        private void CheckPrice(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, JObject jObj)
        {
            StringBuilder sbLog = new StringBuilder();

            if (pScrapeInfo.lbAutoErrorLog)
                sbLog.Append("CheckPrice started\n");

            try
            {

#if enablePriceCheck

                if (jObj != null && jObj.SelectToken("$.PriceBreakdown.Total") != null)
                {
                    Double totalPrice = Convert.ToDouble(string.Format("{0:0.00}", jObj.SelectToken("$.PriceBreakdown.Total").ToString().Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture));

                    string lsDummy = jObj.SelectToken("$.PriceBreakdown").ToString();

                    PriceCheck loPri = new PriceCheck();

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append("Retrieving price\n");
                    }
                    string price = totalPrice.ToString().Replace(",", ".");

                    if (pScrapeInfo.lbAutoErrorLog)
                    {
                        sbLog.Append(string.Format("Price found: {0}\n", price));
                        sbLog.Append("Checking price...\n");
                    }

                    loPri.CheckPrice(ref pScrapeInfo, ref pFoundInfo, price, _currencyName, ref lsDummy);
                }

#endif
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (pScrapeInfo.lbAutoErrorLog)
                    sbLog.Append(string.Format("CheckPrice ended with exception: {0}\n", e.Message));
            }
            finally
            {
                if (pScrapeInfo.lbAutoErrorLog)
                {
                    sbLog.Append("CheckPrice ended\n");
                    //pFoundInfo.InsertVariable("PageContent CheckPrice", sbLog.ToString(), false);
                }
            }
        }

        private void normalizeFlightNumbers(ref JObject jObj)
        {
            try
            {
                if (jObj.SelectTokens("$..FlightNumber") != null)
                {
                    foreach (JToken flightNr in jObj.SelectTokens("$..FlightNumber"))
                    {
                        string flightNr_Original = flightNr.ToString();
                        
                        //Only remove '0' if the flightnumber contains an IATA code and pre-zeros like 'KL00316';
                        //Airtrade removes the '0' in the selected flight so KL00316 becomes KL316 so we can't find a match in VF
                        //Therefor we remove the pre-zeros from the flightnumber
                        
                        int isNumVal = int.MinValue;
                        if (!string.IsNullOrEmpty(flightNr_Original) && flightNr_Original.Length > 3 && !int.TryParse(flightNr_Original.Substring(0, 2), out isNumVal))
                        {
                            string flightNr_Start = flightNr_Original.Substring(0, 2);
                            string flightNr_Rest = flightNr_Original.Substring(2).TrimStart('0');
                            string newFlightNr = flightNr_Start + flightNr_Rest;
                            ((Newtonsoft.Json.Linq.JValue)flightNr).Value = newFlightNr;
                        }
                    }
                }
            }
            //We don't want the scrape engine to quit if Flightnumbers aren't adjusted properly
            //catch (System.Threading.ThreadAbortException)
            //{
            //    throw;
            //}
            catch { }
        }

    }

    public enum requestType { GET, POST, PUT };
    public enum authenticationType { NONE, BASIC, BEARER };
    public enum contentType { JSON, FORM };

    public interface IConnection
    {
        void setContentType(contentType cType);
        void setAuthType(authenticationType aType);
        void setReqType(requestType rType);

        /// <SUMMARY>         
        /// Contains the last error received from the HttpWebRequest call         
        /// </SUMMARY>         
        string LastError { get; set; }
        /// <SUMMARY>         
        /// used for the credentials         
        /// </SUMMARY>         
        string Password { get; set; }
        /// <SUMMARY>         
        /// Sends a request to the Airtrade APIWeb Service.         
        /// </SUMMARY>         
        /// <PARAM name="request"></PARAM>         
        /// <RETURNS></RETURNS>         
        JObject SendRequest();
        JObject SendRequest(string request);
        /// <SUMMARY>         
        /// End point for the Travelport Universal API (tm) web service         
        /// </SUMMARY>         
        Uri Uri { get; set; }
        /// <SUMMARY>         
        /// username for access credentials         
        /// </SUMMARY>         
        string UserName { get; set; }

        string requestSend { get; set; }
        string sessionID { get; set; }

        int requestTimeOut { get; set; }
    }

    public sealed class Connection : Airtrade.IConnection
    {
        private contentType conType;
        private requestType reqType;
        private authenticationType authType;

        public void setContentType(contentType cType)
        {
            conType = cType;
        }

        public void setAuthType(authenticationType aType)
        {
            authType = aType;
        }

        public void setReqType(requestType rType)
        {
            reqType = rType;
        }

        public string LastError { get; set; }
        public string Password { get; set; }
        public Uri Uri { get; set; }
        public string UserName { get; set; }
        public string requestSend { get; set; }
        public string sessionID { get; set; }
        public int requestTimeOut { get; set; }

        /// <SUMMARY>        
        /// Send a request to the Airtrade API Web Service.        
        /// </SUMMARY>        
        /// <PARAM name="request">JSON request to be sent</PARAM>        
        /// <RETURNS>JSON Response</RETURNS>         
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions are turned into strings for this sample")]

        public JObject SendRequest()
        {
            return SendRequest(null);
        }

        public JObject SendRequest(string request)
        {
            JObject jObj = new JObject();

            HttpWebRequest serverRequest = this.CreateRequestObject(requestTimeOut);
            byte[] requestBytes = null;

            if (!string.IsNullOrEmpty(request))
            {
                requestBytes = new UTF8Encoding().GetBytes(request);
                requestSend = System.Text.Encoding.UTF8.GetString(requestBytes);

                // Send request to the server             
                Stream stream = serverRequest.GetRequestStream();
                if (!string.IsNullOrEmpty(request))
                    stream.Write(requestBytes, 0, requestBytes.Length);
                stream.Close();
            }
            // Receive response             
            Stream receiveStream = null;
            HttpWebResponse webResponse = null;
            try
            {
                webResponse = (HttpWebResponse)serverRequest.GetResponse();
                receiveStream = webResponse.GetResponseStream();
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (WebException exception)
            {
                this.SetErrorMessage(exception);
                if (exception.Response != null)
                {
                    // Although the request failed, we can still get a response that might                     
                    // contain a better error message.                     
                    receiveStream = exception.Response.GetResponseStream();
                }
                else
                {
                    return null;
                }
            }
            // Read output stream             
            StreamReader streamReader = new StreamReader(receiveStream, Encoding.UTF8);
            string result = streamReader.ReadToEnd();

            streamReader.Close();
            if (webResponse != null)
                webResponse.Close();

            jObj = GetResponseJObject(result);


            return jObj;
        }

        private HttpWebRequest CreateRequestObject(int timeOut)
        {
            HttpWebRequest serverRequest = (HttpWebRequest)WebRequest.Create(this.Uri);

            serverRequest.Method = reqType.ToString();
            serverRequest.ContentType = conType.Equals(contentType.FORM) ? "application/x-www-form-urlencoded" : conType.Equals(contentType.JSON) ? "application/json" : "";

            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            serverRequest.AutomaticDecompression = DecompressionMethods.GZip;

            // authentication.             
            if (authType.Equals(authenticationType.BASIC))
            {
                byte[] authBytes = Encoding.UTF8.GetBytes((this.UserName + ":" + this.Password).ToCharArray());
                serverRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);
            }
            if (authType.Equals(authenticationType.BEARER))
            {
                serverRequest.Headers["Authorization"] = "Baerer " + sessionID;
            }
            return serverRequest;
        }

        private void SetErrorMessage(WebException exception)
        {
            if (exception.Response != null && ((HttpWebResponse)exception.Response).StatusCode == HttpStatusCode.Unauthorized)
            {
                this.LastError = "The server returned Unauthorized. Please ensure that you are using the correct user name and password.";
            }
            else if (exception.Response != null && ((HttpWebResponse)exception.Response).StatusCode == HttpStatusCode.NotFound)
            {
                this.LastError = "The service could not be found on the server. Please check that you are using the correct URL.";
                this.LastError += Environment.NewLine + Environment.NewLine;
                this.LastError += "The URL will vary depending on the service you want to access.";
            }
            else
            {
                this.LastError = exception.Message;
            }
        }

        /// <SUMMARY>         
        /// Extracts the JSON response from the HTTP  Response         
        /// </SUMMARY>         
        /// <PARAM name="result"></PARAM>         
        /// <RETURNS></RETURNS>       
        private JObject GetResponseJObject(string result)
        {
            JObject jObj = null;

            try
            {
                jObj = Airtrade.General.parseJSON(result);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(this.LastError))
                {
                    this.LastError = ex.Message + "; result:" + result;
                }
            }

            return jObj;
        }
    }

}