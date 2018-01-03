using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


using System.Data;
using System.Configuration;
using System.Collections;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;

using SQLDatabase;

namespace FlightClient
{
    public partial class Performance : System.Web.UI.Page
    {
        public string draw;
        public string chartData_Global;
        public string chartData_Home;
        public string chartData_Search;
        public string chartData_Acco;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                txtFromDate.Text = DateTime.Now.AddDays(-10).ToString("dd-MM-yyyy");
                txtTillDate.Text = DateTime.Now.AddDays(-1).Day.ToString().PadLeft(2, '0') + "-" + DateTime.Now.AddDays(-1).Month.ToString().PadLeft(2, '0') + "-" + DateTime.Now.AddDays(-1).Year.ToString();
                chkAverage.Checked = (Request.QueryString["type"] == "sites");
                draw = "N";

                ShowSites();

                if (Request.QueryString["type"] == "media")
                    btnShow_Click(new object(), new EventArgs());
            }
        }

        #region ShowSites
        private void ShowSites()
        {
            cDbase myDB = new cDbase();
            DataSet ds = null;

            myDB.ConnectString = ConfigurationManager.AppSettings["Performance.DbConn"];
            myDB.DbOpen();

            //myDB.CreateCommand("SELECT WebsiteId, SiteName FROM Website ORDER BY SiteName");
            //adjusted by JvL on 20131021: remove OAD, Globe, Vacancia and Primavera
            if (Request.QueryString["type"] == "sites")
                myDB.CreateCommand("SELECT WebsiteId, SiteName FROM Website where WebsiteId not in (3,10,12,14,15) ORDER BY SiteName");
            else
                myDB.CreateCommand("SELECT WebsiteId, SiteName FROM Website where WebsiteId = 15 ORDER BY SiteName");


            ds = myDB.ExecuteFill();
            myDB.DbClose();

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                ListItem li = new ListItem(r["SiteName"].ToString(), r["WebsiteId"].ToString());
                li.Selected = true;
                chkSites.Items.Add(li);
            }
        }
        #endregion

        #region GetData
        private DataSet GetData(string code)
        {
            cDbase myDB = new cDbase();
            DataSet ds = null;

            StringBuilder sites = new StringBuilder();
            foreach (ListItem li in chkSites.Items)
                if (li.Selected)
                    sites.Append(li.Value + ",");
            sites = sites.Remove(sites.Length - 1, 1);

            //Query
            StringBuilder query = new StringBuilder();
            if (code == "GLOBALL")
            {
                query.Append("SELECT ");
                query.Append("	CONVERT(VARCHAR, m.DateTime, 112) AS [Date], ");
                query.Append("	w.SiteName AS Site, ");
                if (chkIgnoreMinMax.Checked)
                    query.Append("	(SUM(TotalDuration)-MAX(TotalDuration)-MIN(TotalDuration)) / (COUNT(TotalDuration)-2) AS AvgDuration ");
                else
                    query.Append("	SUM(m.TotalDuration) / COUNT(m.TotalDuration) AS AvgDuration ");
                query.Append("FROM Measurement m WITH (NOLOCK) ");
                query.Append("	INNER JOIN Website w WITH (NOLOCK) ");
                query.Append("		ON m.WebsiteId = w.WebsiteId ");
                query.Append("	INNER JOIN Url u WITH (NOLOCK) ");
                query.Append("		ON m.UrlId = u.UrlId ");
                query.Append("	INNER JOIN PageType pt WITH (NOLOCK) ");
                query.Append("		ON u.PageTypeId = pt.PageTypeID ");
                query.Append("WHERE m.WebsiteId IN (" + sites.ToString() + ") ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) >= '" + Pyton.General.Date.Convert(txtFromDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) <= '" + Pyton.General.Date.Convert(txtTillDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("GROUP BY CONVERT(VARCHAR, m.DateTime, 112), w.SiteName ");
                query.Append("ORDER BY CONVERT(VARCHAR, m.DateTime, 112), Site ");
                query.Append(" ; ");
                query.Append("SELECT ");
                query.Append("	CONVERT(VARCHAR, m.DateTime, 112) AS [Date],  ");
                query.Append("	SUM(m.TotalDuration) / COUNT(m.TotalDuration) AS AvgDuration ");
                query.Append("FROM Measurement m WITH (NOLOCK) ");
                query.Append("	INNER JOIN Website w WITH (NOLOCK) ");
                query.Append("		ON m.WebsiteId = w.WebsiteId ");
                query.Append("	INNER JOIN Url u WITH (NOLOCK) ");
                query.Append("		ON m.UrlId = u.UrlId ");
                query.Append("	INNER JOIN PageType pt WITH (NOLOCK) ");
                query.Append("		ON u.PageTypeId = pt.PageTypeID ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) >= '" + Pyton.General.Date.Convert(txtFromDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) <= '" + Pyton.General.Date.Convert(txtTillDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("GROUP BY CONVERT(VARCHAR, m.DateTime, 112) ");
                query.Append("ORDER BY CONVERT(VARCHAR, m.DateTime, 112) ");
            }
            else
            {
                query.Append("SELECT ");
                query.Append("	CONVERT(VARCHAR, m.DateTime, 112) AS [Date], ");
                query.Append("	w.SiteName AS Site, ");
                query.Append("	pt.Code AS PageCode, pt.Description AS Page, ");
                if (chkIgnoreMinMax.Checked)
                    query.Append("	(SUM(TotalDuration)-MAX(TotalDuration)-MIN(TotalDuration)) / (COUNT(TotalDuration)-2) AS AvgDuration ");
                else
                    query.Append("	SUM(m.TotalDuration) / COUNT(m.TotalDuration) AS AvgDuration ");
                query.Append("FROM Measurement m WITH (NOLOCK) ");
                query.Append("	INNER JOIN Website w WITH (NOLOCK) ");
                query.Append("		ON m.WebsiteId = w.WebsiteId ");
                query.Append("	INNER JOIN Url u WITH (NOLOCK) ");
                query.Append("		ON m.UrlId = u.UrlId ");
                query.Append("	INNER JOIN PageType pt WITH (NOLOCK) ");
                query.Append("		ON u.PageTypeId = pt.PageTypeID ");
                query.Append("WHERE m.WebsiteId IN (" + sites.ToString() + ") ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) >= '" + Pyton.General.Date.Convert(txtFromDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) <= '" + Pyton.General.Date.Convert(txtTillDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND pt.Code = '" + code + "' ");
                query.Append("GROUP BY CONVERT(VARCHAR, m.DateTime, 112), w.SiteName, pt.Code, pt.PageTypeId, pt.Description ");
                query.Append("ORDER BY CONVERT(VARCHAR, m.DateTime, 112), Site, pt.PageTypeId ");
                query.Append(" ; ");
                query.Append("SELECT ");
                query.Append("	CONVERT(VARCHAR, m.DateTime, 112) AS [Date],  ");
                query.Append("	pt.Code AS PageCode, pt.Description AS Page,  ");
                query.Append("	SUM(m.TotalDuration) / COUNT(m.TotalDuration) AS AvgDuration ");
                query.Append("FROM Measurement m WITH (NOLOCK) ");
                query.Append("	INNER JOIN Website w WITH (NOLOCK) ");
                query.Append("		ON m.WebsiteId = w.WebsiteId ");
                query.Append("	INNER JOIN Url u WITH (NOLOCK) ");
                query.Append("		ON m.UrlId = u.UrlId ");
                query.Append("	INNER JOIN PageType pt WITH (NOLOCK) ");
                query.Append("		ON u.PageTypeId = pt.PageTypeID ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) >= '" + Pyton.General.Date.Convert(txtFromDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND CONVERT(VARCHAR, m.DateTime, 112) <= '" + Pyton.General.Date.Convert(txtTillDate.Text, "dd-MM-yyyy", "yyyyMMdd") + "' ");
                query.Append("AND pt.Code = '" + code + "' ");
                query.Append("GROUP BY CONVERT(VARCHAR, m.DateTime, 112), pt.Code, pt.PageTypeId, pt.Description ");
                query.Append("ORDER BY CONVERT(VARCHAR, m.DateTime, 112), pt.PageTypeId ");
            }

            myDB.ConnectString = ConfigurationManager.AppSettings["Performance.DbConn"];
            myDB.DbOpen();

            myDB.CreateCommand(query.ToString());

            ds = myDB.ExecuteFill();
            myDB.DbClose();

            return ds;
        }
        #endregion

        #region ShowData
        private void ShowData(string code, ref string item)
        {
            DataSet ds = GetData(code);

            DataTable dt = new DataTable();
            dt.Columns.Add("Date", typeof(string));
            if (ds.Tables.Count > 1 && chkAverage.Checked)
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
            if (ds.Tables.Count > 1 && chkAverage.Checked)
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
        #endregion

        protected void btnShow_Click(object sender, EventArgs e)
        {
            ShowData("GLOBALL", ref chartData_Global);

            if (Request.QueryString["type"] == "sites")
            {
                ShowData("HOME", ref chartData_Home);
                ShowData("SEARCH", ref chartData_Search);
                ShowData("ACCO", ref chartData_Acco);
            }
            else
            {
                ShowData("Get Text", ref chartData_Home);
                ShowData("Get Images", ref chartData_Search);
            }
            draw = "Y";
        }

    }
}