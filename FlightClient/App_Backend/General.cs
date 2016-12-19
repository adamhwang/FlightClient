using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Text;

namespace FlightClient
{
    public class General
    {
        public General()
        { }

        public static string SerializeObject(object obj)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            StringBuilder writer = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            using (XmlWriter stringWriter = XmlWriter.Create(writer, settings))
            {
                serializer.Serialize(stringWriter, obj, ns);
            }
            return StripXmlNameSpaces(writer.ToString());
        }

        private static string StripXmlNameSpaces(string xml)
        {
            const string strXMLPattern = @"(p[0-9]:nil=""true"")?(\ )?xmlns(:\w+)?="".+""";
            return Regex.Replace(xml, strXMLPattern, "");
        }

        public static string gethhmm(string time)
        {
            if (time.Contains(":") && time.Split(':').Length > 1)
            {
                string[] items = time.Split(':');
                time = items[0] + ":" + items[1];
            }
            return time;
        }

        public static bool useXMLFile()
        {
            return System.Configuration.ConfigurationManager.AppSettings["UseXmlFileResponse"].Equals("1");
        }

        public static string getXmlFileText()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(HttpContext.Current.Server.MapPath("XML/LowFare.xml"));

            return xdoc.OuterXml;
        }


        public static string createExtendedResponse(XmlDocument response)
        {
            string responseStr = response.OuterXml;

            StringBuilder sb = new StringBuilder();

            sb.Append(responseStr.Substring(0, responseStr.IndexOf(">") + 1).Replace("LowFareSearchRsp", "LowFareSearchExtendedRsp"));

            foreach (XmlNode AirPricingSolution in response.SelectNodes("//AirPricingSolution"))
            {
                sb.Append(CreateTotalAirPricingSolution(response, AirPricingSolution).OuterXml);
            }

            sb.Append("</LowFareSearchExtendedRsp>");

            return sb.ToString();
        }

        public static XmlDocument CreateTotalAirPricingSolution(XmlDocument response, XmlNode AirPricingSolution)
        {
            XmlDocument xdoc = new XmlDocument();

            List<string> AirSegmentKeyList = new List<string>();
            List<XmlNode> AirSegmentList = new List<XmlNode>();
            string search = string.Empty;

            XmlNodeList journeys = AirPricingSolution.SelectNodes("Journey/AirSegmentRef");

            foreach (XmlNode AirSegmentRef in journeys)
            {
                string a_key = XML.GetNode(AirSegmentRef, "", "@Key");

                XmlNode AirSegment = response.SelectSingleNode(string.Format("//AirSegmentList/AirSegment[@Key[contains(.,'{0}')]]", a_key));

                if (AirSegment != null)
                {
                    if (!AirSegmentKeyList.Contains(a_key))
                    {
                        AirSegmentKeyList.Add(a_key);

                        //Add the flightdetail(s) instead of only the reference
                        List<string> FlightDetailsRefKeyList = new List<string>();
                        XmlNodeList FlightDetailsRefList = AirSegment.SelectNodes("FlightDetailsRef");

                        string flightDetailStr = string.Empty;

                        foreach (XmlNode FlightDetailsRef in FlightDetailsRefList)
                        {
                            string f_key = XML.GetNode(FlightDetailsRef, "", "@Key");

                            XmlNode flightDetail = response.SelectSingleNode(string.Format("//FlightDetails[@Key[contains(.,'{0}')]]", f_key));

                            if (flightDetail != null && !FlightDetailsRefKeyList.Contains(f_key))
                            {
                                FlightDetailsRefKeyList.Add(f_key);
                                flightDetailStr += flightDetail.OuterXml;
                            }
                        }

                        //Remove FlightDetailsRef
                        search = "<FlightDetailsRef";
                        string pre = AirSegment.OuterXml.Substring(0, AirSegment.OuterXml.IndexOf(search));
                        search = "</AirSegment";
                        string post = AirSegment.OuterXml.Substring(AirSegment.OuterXml.IndexOf(search));

                        string AirSegmentStr = pre + flightDetailStr + post;

                        XmlDocument xtemp = new XmlDocument();
                        xtemp.LoadXml(AirSegmentStr);

                        AirSegmentList.Add(xtemp);

                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            search = "<AirPricingSolution ";
            string restStr = AirPricingSolution.OuterXml.Substring(search.Length);
            search = ">";
            string AirPricingSolutionAttr = restStr.Substring(0, restStr.IndexOf(search));

            sb.Append("<AirPricingSolution " + AirPricingSolutionAttr + ">");

            foreach (XmlNode journey in AirPricingSolution.SelectNodes("Journey"))
            {
                sb.Append("<Journey TravelTime='" + XML.GetNode(journey, "", "@TravelTime") + "'>");

                foreach (XmlNode airSegmentRef in journey.SelectNodes("AirSegmentRef"))
                {
                    XmlNode airsegment = GetAirSegment<XmlNode>(AirSegmentList, XML.GetNode(airSegmentRef, "", "@Key"));
                    if (airsegment != null)
                        sb.Append(airsegment.OuterXml);
                }
                sb.Append("</Journey>");
            }

            //sb.Append("<Journey TravelTime='" + XML.GetNode(AirPricingSolution, "", "Journey/@TravelTime") + "'>");
            //for (int i = 0; i < AirSegmentList.Count; i++)
            //{
            //    sb.Append(AirSegmentList[i].OuterXml);
            //}
            //sb.Append("</Journey>");


            foreach (XmlNode legRef in AirPricingSolution.SelectNodes("LegRef"))
            {
                string LegRef_key = XML.GetNode(legRef, "", "@Key");
                XmlNode Leg = response.SelectSingleNode(string.Format("//Leg[@Key[contains(.,'{0}')]]", LegRef_key));
                if (Leg != null)
                    sb.Append(Leg.OuterXml);
            }

            //string LegRef_key = XML.GetNode(AirPricingSolution, "", "LegRef/@Key");
            //XmlNode Leg = response.SelectSingleNode(string.Format("//Leg[@Key[contains(.,'{0}')]]", LegRef_key));
            //if (Leg != null)
            //    sb.Append(Leg.OuterXml);

            foreach (XmlNode AirPricingInfo in AirPricingSolution.SelectNodes("AirPricingInfo"))
            {
                List<string> FareInfoKeyList = new List<string>();

                string fareInfoStr = string.Empty;

                foreach (XmlNode FareInfoRef in AirPricingInfo.SelectNodes("FareInfoRef"))
                {
                    string fareInfoRef_key = XML.GetNode(FareInfoRef, "", "@Key");

                    if (!FareInfoKeyList.Contains(fareInfoRef_key))
                    {
                        FareInfoKeyList.Add(fareInfoRef_key);
                        XmlNode fareInfo = response.SelectSingleNode(string.Format("//FareInfo[@Key[contains(., '{0}')]]", fareInfoRef_key));
                        if (fareInfo != null)
                            fareInfoStr += fareInfo.OuterXml;
                    }
                }
                search = "<FareInfoRef";
                string pre = AirPricingInfo.OuterXml.Substring(0, AirPricingInfo.OuterXml.IndexOf(search));
                search = "<BookingInfo";
                string post = AirPricingInfo.OuterXml.Substring(AirPricingInfo.OuterXml.IndexOf(search));

                string airPricingInfoStr = pre + fareInfoStr + post;

                sb.Append(airPricingInfoStr);
            }

            search = "</AirPricingInfo>";

            restStr = AirPricingSolution.OuterXml.Substring(AirPricingSolution.OuterXml.LastIndexOf(search) + search.Length);

            restStr = restStr.Replace("</AirPricingSolution>", "");

            sb.Append(restStr);

            sb.Append("</AirPricingSolution>");

            xdoc.LoadXml(sb.ToString());

            return xdoc;
        }

        public static T GetAirSegment<T>(List<XmlNode> AirSegmentList, string key) where T : XmlNode
        {
            T item = null;

            XmlNode airSegment = AirSegmentList.SingleOrDefault(x => x.SelectSingleNode(string.Format("AirSegment[@Key[contains(.,'{0}')]]", key)) != null);

            if (airSegment != null)
                item = (T)airSegment;

            return item;
        }
    }

    public class XML
    {
        public XML()
        { }

        public static string GetNode(XmlNode x, string defaultValue, params string[] xPath)
        {
            string Value = "";
            if (x != null)
            {
                foreach (string s in xPath)
                {
                    if (x.SelectSingleNode(s) != null)
                    {
                        Value = x.SelectSingleNode(s).InnerText;
                        break;
                    }
                }
            }

            if (Value == "")
                Value = defaultValue;

            return Value;
        }
    }
}

    