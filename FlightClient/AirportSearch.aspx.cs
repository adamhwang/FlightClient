#define useXQString
//#define useLogging
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;
using System.Text;
using System.IO;



namespace FlightClient
{
    public partial class AirportSearch : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["val"]))
            {
                Response.Write("ERROR");
                return;
            }

            string loc = Server.MapPath("XML/Airports.xml");
            XmlDocument xAirports = new XmlDocument();
            xAirports.Load(loc);

            StringBuilder sb = new StringBuilder();
            XmlNode res = null;

#if useLogging
            StringBuilder sbLog = new StringBuilder();
#endif

#if useXQString

#if useLogging
            sbLog.Append(string.Format("{0}: XQuery method with string started;\n", DateTime.Now.ToString("dd.MM.yyyy T HH:mm:ss.fff")));
#endif

            sb.Append("<Airports>{ let $doc := .\n");
            sb.Append(string.Format("let $search := '{0}'\n", Request.QueryString["val"]));
            sb.Append("for $y in $doc//Airport\n");
            sb.Append("where $y/Name[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), translate($search,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'))]\n");
            sb.Append("or $y/IATA[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), translate($search,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'))]\n");
            sb.Append("return $y\n");
            sb.Append("}</Airports>");

            //xQueryProcessor saxonXQuery = new xQueryProcessor();
            //saxonXQuery.Load(sb.ToString());
            //res = saxonXQuery.RunQuery(xAirports);

            res = xQueryProcessor.RunQuery(xAirports, sb.ToString());

#if useLogging
            sbLog.Append(string.Format("{0}: XQuery method with string ended;\n", DateTime.Now.ToString("dd.MM.yyyy T HH:mm:ss.fff")));
#endif

#else

#if useLogging
            sbLog.Append(string.Format("{0}: XQuery method with file started;\n", DateTime.Now.ToString("dd.MM.yyyy T HH:mm:ss.fff")));
#endif

            string xqFile = Server.MapPath("XML/Airportsearch.xq");
            if (File.Exists(xqFile))
            {
                StreamReader sr = new StreamReader(xqFile);
                string req = sr.ReadToEnd().Replace("<#SEARCH#>",Request.QueryString["val"]) ;
                res = xQueryProcessor.RunQuery(xAirports, req);
            }

#if useLogging
            sbLog.Append(string.Format("{0}: XQuery method with file ended;\n", DateTime.Now.ToString("dd.MM.yyyy T HH:mm:ss.fff")));
#endif

#endif


#if useLogging

            string FileName = Server.MapPath("Content_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            StreamWriter writer1 = new StreamWriter(FileName, true);
            writer1.Write(sbLog.ToString(), Encoding.GetEncoding("iso-8859-1"));
            writer1.Close();
#endif

            sb = new StringBuilder();

            foreach (XmlNode airport in res.SelectNodes("Airports/Airport"))
                sb.Append(string.Format("<a href=\"javascript:SelectAirport('{0}','{1}');\">{1}</a><br/>\n", XML.GetNode(airport, "", "IATA"), XML.GetNode(airport, "", "Name")));

            if (string.IsNullOrEmpty(sb.ToString()))
                sb.Append("No items found");

            Response.Write(sb.ToString());
        }
    }
}