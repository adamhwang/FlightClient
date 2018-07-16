using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.IO;
using System.Text;

using System.Collections.Specialized;
using System.Xml;

using EAScrape;

namespace Condor
{
    public class CondorMain
    {
        public CondorMain()
        {
            System.Net.ServicePointManager.CertificatePolicy = new WeGoLoPolicy();
        }

        public string GoNext(NameValueCollection poScrapeData, ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            try
            {
                switch (poScrapeData["action"])
                {
                    case "GetFlights": return GetFlights(pScrapeInfo);
                    case "GetRRCombis": return GetCombis(pScrapeInfo);
                    default: return "";
                }
                return "";
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
                return "Unexpected Error: " + leExc.ToString();
            }
        }

        private string GetFlights(ScrapeInfo pScrapeInfo)
        {
            bool gotDepDateOut = pScrapeInfo.CheckIfVariableExists("DepDate_Out");
            bool gotFlightContent = pScrapeInfo.CheckIfVariableExists("FlightContent");

            string response = string.Empty;
            if (gotDepDateOut && gotFlightContent)
            {
                Condor c = new Condor();
                c.DepartureDateOut = pScrapeInfo.GetScrapeInfoValueFromName("DepDate_Out");
                c.XmlResponse = pScrapeInfo.GetScrapeInfoValueFromName("FlightContent");
                response = c.getFlights();
            }
            else if (!gotDepDateOut)
                response = "<ERROR>No outbound departure date</ERROR>";
            else if (!gotFlightContent)
                response = "<ERROR>No Xml response</ERROR>";

            return response;
        }

        private string GetCombis(ScrapeInfo pScrapeInfo)
        {
            bool gotDepDateOut = pScrapeInfo.CheckIfVariableExists("DepDate_Out");
            bool gotDepDateRet = pScrapeInfo.CheckIfVariableExists("DepDate_Ret");
            bool gotFlightContent = pScrapeInfo.CheckIfVariableExists("FlightContent");

            string response = string.Empty;
            if (gotDepDateOut && gotDepDateRet && gotFlightContent)
            {
                Condor c = new Condor();
                c.DepartureDateOut = pScrapeInfo.GetScrapeInfoValueFromName("DepDate_Out");
                c.DepartureDateRet = pScrapeInfo.GetScrapeInfoValueFromName("DepDate_Ret");
                c.XmlResponse = pScrapeInfo.GetScrapeInfoValueFromName("FlightContent");
                c.ORI = pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME");
                c.DES = pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME");
                response = c.getRRCombis();
            }
            else if (!gotDepDateOut)
                response = "<ERROR>No outbound departure date</ERROR>";
            else if (!gotDepDateRet)
                response = "<ERROR>No inbound departure date</ERROR>";
            else if (!gotFlightContent)
                response = "<ERROR>No Xml response</ERROR>";

            return response;
        }
    }

    public class Condor
    {
        private string _xmlContent = string.Empty;
        private string _depDateOut = string.Empty;
        private string _depDateRet = string.Empty;
        private string _ORI = string.Empty;
        private string _DES = string.Empty;

        public string XmlResponse
        {
            //Must be valid XML
            get { return _xmlContent; }
            set { _xmlContent = value; }
        }

        public string DepartureDateOut
        {
            //Date in yyyy-MM-dd format
            get { return _depDateOut; }
            set { _depDateOut = value; }
        }

        public string DepartureDateRet
        {
            //Date in yyyy-MM-dd format
            get { return _depDateRet; }
            set { _depDateRet = value; }
        }

        public string ORI
        {
            set { _ORI = value; }
        }

        public string DES
        {
            set { _DES = value; }
        }

        public Condor()
        {}

        public string getRRCombis()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(_xmlContent)) return "<ERROR>No Xml response</ERROR>";
            if (string.IsNullOrEmpty(_depDateOut)) return "<ERROR>No outbound departure date</ERROR>";
            if (string.IsNullOrEmpty(_depDateRet)) return "<ERROR>No inbound departure date</ERROR>";

            string errorMess = string.Empty;
            if (!gotAllElements(ref errorMess)) return errorMess;

            sb.Append("<COMBIS>");
            try
            {
                RemoveItems();

                string content = _xmlContent;

                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(content);
                }
                catch
                {
                    return "<ERROR>No valid XML response</ERROR>";
                }

                XmlNodeList AirlineOfferList = xdoc.SelectNodes("//AirlineOffer");
                XmlNodeList FlightList = xdoc.SelectNodes("//FlightList/Flight");
                XmlNodeList FlightSegmentList = xdoc.SelectNodes("//FlightSegmentList/FlightSegment");
                XmlNodeList FlightReferenceList = xdoc.SelectNodes("//OriginDestination/FlightReferences");

                for (int t = 0; t < FlightReferenceList.Count; t++)
                {
                    string depAirportOut = FlightReferenceList[t].ParentNode.SelectSingleNode("DepartureCode").InnerText;
                    if (depAirportOut == _ORI)
                    { 
                        string selectedFlightSegment = SelectedFlightSegment(xdoc, FlightReferenceList, t);
                        XmlNode selectedAirlineOffer = AirlineOfferList[t];

                        bool gotDapDateOut = selectedFlightSegment.Contains(_depDateOut);

                        if (gotDapDateOut)
                        {
                            string selectedFlightSegment_Out = selectedFlightSegment;
                            string selectedAirlineOffer_Out = selectedAirlineOffer.OuterXml;

                            //Find all returnflight segments

                            for (int s = t + 1; s < FlightReferenceList.Count; s++)
                            {
                                string depAirportRet = FlightReferenceList[s].ParentNode.SelectSingleNode("DepartureCode").InnerText;
                                if (depAirportRet == _DES)
                                {
                                    selectedFlightSegment = SelectedFlightSegment(xdoc, FlightReferenceList, s);
                                    selectedAirlineOffer = AirlineOfferList[s];

                                    bool gotDapDateRet = selectedFlightSegment.Contains(_depDateRet);

                                    if (gotDapDateRet)
                                    {
                                        string selectedFlightSegment_Ret = selectedFlightSegment;
                                        string selectedAirlineOffer_Ret = selectedAirlineOffer.OuterXml;

                                        sb.Append("<COMBI><OUT>");
                                        sb.Append(selectedFlightSegment_Out);
                                        sb.Append(selectedAirlineOffer_Out);
                                        sb.Append("</OUT><RET>");
                                        sb.Append(selectedFlightSegment_Ret);
                                        sb.Append(selectedAirlineOffer_Ret);
                                        sb.Append("</RET></COMBI>");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return "<ERROR>" + e.Message + "</ERROR>";
            }

            sb.Append("</COMBIS>");

            return sb.ToString();

        }

        public string getFlights()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(_xmlContent)) return "<ERROR>No Xml response</ERROR>";
            if (string.IsNullOrEmpty(_depDateOut)) return "<ERROR>No outbound departure date</ERROR>";

            string errorMess = string.Empty;
            if (!gotAllElements(ref errorMess)) return errorMess;

            sb.Append("<FLIGHTS>");
            try
            {
                RemoveItems();

                string content = _xmlContent;

                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(content);
                }
                catch
                {
                    return "<ERROR>No valid XML response</ERROR>";
                }

                XmlNodeList AirlineOfferList = xdoc.SelectNodes("//AirlineOffer");
                XmlNodeList FlightList = xdoc.SelectNodes("//FlightList/Flight");
                XmlNodeList FlightSegmentList = xdoc.SelectNodes("//FlightSegmentList/FlightSegment");
                XmlNodeList FlightReferenceList = xdoc.SelectNodes("//OriginDestination/FlightReferences");

                for (int t = 0; t < FlightReferenceList.Count; t++)
                {
                    string selectedFlightSegment = SelectedFlightSegment(xdoc, FlightReferenceList, t);
                    XmlNode selectedAirlineOffer = AirlineOfferList[t];

                    bool gotDapDateOut = selectedFlightSegment.Contains(_depDateOut);

                    if (gotDapDateOut)
                    {
                        string selectedFlightSegment_Out = selectedFlightSegment;
                        string selectedAirlineOffer_Out = selectedAirlineOffer.OuterXml;

                        sb.Append("<FLIGHT>");
                        sb.Append(selectedFlightSegment_Out);
                        sb.Append(selectedAirlineOffer_Out);
                        sb.Append("</FLIGHT>");
                    }
                }
            }
            catch (Exception e)
            {
                return "<ERROR>" + e.Message + "</ERROR>";
            }

            sb.Append("</FLIGHTS>");

            return sb.ToString();
        }


        /// <summary>
        /// //////////////////////////////////// private functions /////////////////////////////////
        /// </summary>
        /// 

        private void RemoveItems()
        {
            string content = _xmlContent;
            string search = "xmlns=\"";

            //Remove all xmlns references
            while (content.Contains(search))
            {
                int pos = content.IndexOf(search);

                if (pos > 0)
                {
                    string pre = content.Substring(0, pos);
                    string post = content.Substring(pos + search.Length);
                    string search2 = "\"";
                    int pos2 = post.IndexOf(search2) + search2.Length;
                    post = post.Substring(pos2);
                    content = pre + post;
                }
            }

            /* 
             * remove all[x] from airlineoffers (is actually not necessary)
             * 

            search = "<AirlineOffer>[";
            while (content.Contains(search))
            {
                int pos = content.IndexOf(search) + search.Length - 1;
                if (pos > 0)
                {
                    string pre = content.Substring(0, pos);
                    string post = content.Substring(pos);
                    string search2 = "]<OfferID ";
                    int pos2 = post.IndexOf(search2) + 1;
                    post = post.Substring(pos2);
                    content = pre + post;
                }
            }

            search = "</PricedOffer>[";
            while (content.Contains(search))
            {
                int pos = content.IndexOf(search) + search.Length - 1;
                if (pos > 0)
                {
                    string pre = content.Substring(0, pos);
                    string post = content.Substring(pos);
                    string search2 = "]</AirlineOffer>";
                    int pos2 = post.IndexOf(search2) + 1;
                    post = post.Substring(pos2);
                    content = pre + post;
                }
            }
            */

            _xmlContent = content;
        }
        
        private bool gotAllElements(ref string ErrorMessage)
        {
            bool allOK = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("<ERROR>");
            allOK = _xmlContent.Contains("<AirlineOffer>");
            if (!allOK) sb.Append("No AirlineOffers");

            if (!_xmlContent.Contains("<FlightSegment SegmentKey="))
            {
                if (sb.ToString().Contains("No")) sb.Append(" / ");
                sb.Append("No FlightSegments");
                allOK = false;
            }

            if (!_xmlContent.Contains("<Flight FlightKey="))
            {
                if (sb.ToString().Contains("No")) sb.Append(" / ");
                sb.Append("No Flights");
                allOK = false;
            }

            if (!_xmlContent.Contains("<OriginDestination OriginDestinationKey="))
            {
                if (sb.ToString().Contains("No")) sb.Append(" / ");
                sb.Append("No OriginDestinations");
                allOK = false;
            }


            sb.Append("</ERROR>");

            ErrorMessage = sb.ToString();


            return allOK;
        }

        private string SelectedFlightSegment(XmlDocument xdoc, XmlNodeList FlightReferenceList, int startItem)
        {
            string selectedFlightSegment = string.Empty;

            XmlNodeList FlightSegmentList = xdoc.SelectNodes("//FlightSegmentList/FlightSegment");

            XmlNode flightReference = FlightReferenceList[startItem];
            string flightReferences = flightReference.InnerText;
            bool gotMultipleFlightReferences = flightReferences.Contains(" ");

            string firstFlightReference = gotMultipleFlightReferences ? flightReferences.Split(' ')[0] : flightReferences;
            string lastFlightReference = gotMultipleFlightReferences ? flightReferences.Split(' ')[flightReferences.Split(' ').Length - 1] : flightReferences;
            int fromSegNr = Convert.ToInt32(firstFlightReference.ToUpper().Replace("FL", ""));
            int toSegNr = Convert.ToInt32(lastFlightReference.ToUpper().Replace("FL", ""));

            XmlNode flight = xdoc.SelectSingleNode(String.Format("//FlightList/Flight[@FlightKey='{0}']", firstFlightReference));
            string segmentReferences = flight != null ? flight.SelectSingleNode("SegmentReferences").InnerText : string.Empty;

            bool gotMultipleSegmentReferences = segmentReferences.Contains(" ");

            if (gotMultipleFlightReferences)
            {
                fromSegNr = Convert.ToInt32(segmentReferences.Split(' ')[0].ToUpper().Replace("SEG", ""));
                toSegNr = Convert.ToInt32(flightReferences.Split(' ')[flightReferences.Split(' ').Length - 1].ToUpper().Replace("FL", ""));

                if (firstFlightReference != lastFlightReference)
                {
                    flight = xdoc.SelectSingleNode(String.Format("//FlightList/Flight[@FlightKey='{0}']", lastFlightReference));
                    segmentReferences = flight != null ? flight.SelectSingleNode("SegmentReferences").InnerText : string.Empty;

                    toSegNr = Convert.ToInt32(segmentReferences.Split(' ')[segmentReferences.Split(' ').Length - 1].ToUpper().Replace("SEG", ""));

                }

                toSegNr++;
            }
            else
            {
                if (gotMultipleSegmentReferences)
                {
                    fromSegNr = Convert.ToInt32(segmentReferences.Split(' ')[0].ToUpper().Replace("SEG", ""));
                    toSegNr = Convert.ToInt32(segmentReferences.Split(' ')[segmentReferences.Split(' ').Length - 1].ToUpper().Replace("SEG", ""));
                    toSegNr++;
                }
                else
                {
                    flightReferences = segmentReferences;
                    fromSegNr = Convert.ToInt32(segmentReferences.ToUpper().Replace("SEG", "").Replace("FL", ""));
                    toSegNr = fromSegNr + 1;

                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<FlightSegmentList>");
            for (int j = 0; j < FlightSegmentList.Count; j++)
                sb.Append(FlightSegmentList[j].OuterXml);
            sb.Append("</FlightSegmentList>");


            string fromSegStr = string.Format("<FlightSegment SegmentKey=\"SEG{0}\">", fromSegNr);
            string toSegSr = sb.ToString().Contains(string.Format("<FlightSegment SegmentKey=\"SEG{0}\">", toSegNr)) ? string.Format("<FlightSegment SegmentKey=\"SEG{0}\">", toSegNr) : "</FlightSegmentList>";

            selectedFlightSegment = string.Empty;
            int pos = sb.ToString().IndexOf(fromSegStr);
            int pos2 = sb.ToString().IndexOf(toSegSr);
            if (pos >= 0 && pos2 > pos)
                selectedFlightSegment = sb.ToString().Substring(pos, pos2 - pos);

            return selectedFlightSegment;
        }
    }

}