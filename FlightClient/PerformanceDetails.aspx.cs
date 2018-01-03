using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;

using SQLDatabase;

namespace FlightClient
{
    public partial class PerformanceDetails : System.Web.UI.Page
    {
        protected string chartData;
        protected string chartTitle;

        protected void Page_Load(object sender, EventArgs e)
        {
            ShowData
            (
                Request.QueryString["type"].ToUpper(),
                Request.QueryString["date"],
                Request.QueryString["site"],
                ref chartData
            );

            switch (Request.QueryString["type"].ToUpper())
            {
                case "GLOBALL":
                    chartTitle = "Performance all pages for " + Request.QueryString["site"];
                    break;
                case "HOME":
                    chartTitle = "Performance homepage for " + Request.QueryString["site"];
                    break;
                case "SEARCH":
                    chartTitle = "Performance searchresult for " + Request.QueryString["site"];
                    break;
                case "ACCO":
                    chartTitle = "Performance accopage for " + Request.QueryString["site"];
                    break;
            }
        }

        private void ShowData(string code, string date, string site, ref string item)
        {
            DataSet ds = GetData(code, date, site);

            DataTable dt = new DataTable();
            dt.Columns.Add("Date", typeof(string));
            if (ds.Tables.Count > 1 /*&& chkAverage.Checked*/)
            {
                DataColumn dc = new DataColumn("Gemiddeld", typeof(int));
                dc.DefaultValue = 0;
                dt.Columns.Add(dc);
            }
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                if (!dt.Columns.Contains(r["Site"].ToString()))
                {
                    DataColumn dc = new DataColumn(r["Site"].ToString(), typeof(int));
                    dc.DefaultValue = 0;
                    dt.Columns.Add(dc);
                }
            }

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                if (dt.Select("Date='" + r["Date"].ToString() + "'").Length == 0)
                {
                    dt.Rows.Add(new object[] { r["Date"].ToString() });
                }
                dt.Select("Date='" + r["Date"].ToString() + "'")[0][r["Site"].ToString()] = r["AvgDuration"].ToString();
            }
            if (ds.Tables.Count > 1 /*&& chkAverage.Checked*/)
            {
                foreach (DataRow r in ds.Tables[1].Rows)
                {
                    if (dt.Select("Date='" + r["Date"].ToString() + "'").Length == 0)
                    {
                        dt.Rows.Add(new object[] { r["Date"].ToString() });
                    }
                    dt.Select("Date='" + r["Date"].ToString() + "'")[0]["Gemiddeld"] = r["AvgDuration"].ToString();
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("['Date', ");
            for (int t = 1; t < dt.Columns.Count; t++)
                sb.Append("'" + dt.Columns[t].ColumnName + "', ");
            sb = sb.Remove(sb.Length - 2, 2);
            sb.Append("],"); sb.Append(Environment.NewLine);

            DataView dv = dt.DefaultView;
            dv.Sort = "Date";
            foreach (DataRowView dr in dv)
            {
                sb.Append("['");
                sb.Append(Pyton.General.Date.Convert(dr["Date"].ToString(), "yyyyMMdd", "dd-MM-yyyy"));
                sb.Append("', ");

                for (int t = 1; t < dt.Columns.Count; t++)
                    sb.Append(dr[dt.Columns[t].ColumnName] + ", ");
                sb = sb.Remove(sb.Length - 2, 2);
                sb.Append("],"); sb.Append(Environment.NewLine);
            }
            sb = sb.Remove(sb.Length - 3, 3);

            item = sb.ToString();
        }

        private DataSet GetData(string code, string date, string site)
        {
            cDbase myDB = new cDbase();
            DataSet ds = null;

            //Query
            StringBuilder query = new StringBuilder();
            if (code == "GLOBALL")
            {
                query.Append("SELECT ");
                query.Append("	CONVERT(VARCHAR, m.DateTime, 108) AS [Date], ");
                query.Append("	w.SiteName AS Site, ");
                query.Append("	SUM(m.TotalDuration) / COUNT(m.TotalDuration) AS AvgDuration ");
                query.Append("FROM Measurement m WITH (NOLOCK) ");
                query.Append("	INNER JOIN Website w WITH (NOLOCK) ");
                query.Append("		ON m.WebsiteId = w.WebsiteId ");
                if (site != null && site != "")
                    query.Append("		AND w.SiteName = '" + site + "' ");
                query.Append("	INNER JOIN Url u WITH (NOLOCK) ");
                query.Append("		ON m.UrlId = u.UrlId ");
                query.Append("	INNER JOIN PageType pt WITH (NOLOCK) ");
                query.Append("		ON u.PageTypeId = pt.PageTypeID ");
                query.Append("WHERE CONVERT(VARCHAR, m.DateTime, 112) = '" + Pyton.General.Date.Convert(date, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("GROUP BY CONVERT(VARCHAR, m.DateTime, 108), w.SiteName ");
                query.Append("ORDER BY CONVERT(VARCHAR, m.DateTime, 108), Site ");
            }
            else
            {
                query.Append("SELECT ");
                query.Append("	CONVERT(VARCHAR, m.DateTime, 108) AS [Date], ");
                query.Append("	w.SiteName AS Site, ");
                query.Append("	pt.Code AS PageCode, pt.Description AS Page, ");
                query.Append("	SUM(m.TotalDuration) / COUNT(m.TotalDuration) AS AvgDuration ");
                query.Append("FROM Measurement m WITH (NOLOCK) ");
                query.Append("	INNER JOIN Website w WITH (NOLOCK) ");
                query.Append("		ON m.WebsiteId = w.WebsiteId ");
                if (site != null && site != "")
                    query.Append("		AND w.SiteName = '" + site + "' ");
                query.Append("	INNER JOIN Url u WITH (NOLOCK) ");
                query.Append("		ON m.UrlId = u.UrlId ");
                query.Append("	INNER JOIN PageType pt WITH (NOLOCK) ");
                query.Append("		ON u.PageTypeId = pt.PageTypeID ");
                query.Append("WHERE CONVERT(VARCHAR, m.DateTime, 112) = '" + Pyton.General.Date.Convert(date, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND pt.Code = '" + code + "' ");
                query.Append("GROUP BY CONVERT(VARCHAR, m.DateTime, 108), w.SiteName, pt.Code, pt.PageTypeId, pt.Description ");
                query.Append("ORDER BY CONVERT(VARCHAR, m.DateTime, 108), Site, pt.PageTypeId ");

            }

            myDB.ConnectString = ConfigurationManager.AppSettings["Performance.DbConn"];
            myDB.DbOpen();

            myDB.CreateCommand(query.ToString());

            ds = myDB.ExecuteFill();
            myDB.DbClose();

            return ds;
        }
    }
}