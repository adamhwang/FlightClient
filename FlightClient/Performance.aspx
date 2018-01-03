<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Performance.aspx.cs" Inherits="FlightClient.Performance" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Performance</title>
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
	<script type="text/javascript">
	    //See https://google-developers.appspot.com/chart/interactive/docs/gallery/linechart
	    
	     google.load("visualization", "1", {packages:["corechart"]});
	     google.setOnLoadCallback(drawChart);

        function drawChart() 
        {
            <%if( draw == "Y" ) { %>
            drawGlobal();

            <% if (Request.QueryString["type"] == "sites") { %>
                drawHome();
                drawSearch();
                drawAcco();
            <%} %>

            <% if (Request.QueryString["type"] == "media") { %>
                drawHome();
                drawSearch();
            <%} %>

            <%} %>
        }

        function drawGlobal()
        {
            var data = google.visualization.arrayToDataTable
            ([
                <%=chartData_Global %>
            ]);
            var options = 
            {
                title: '<%= Request.QueryString["type"] == "sites" ? "Performance All pages" : "Peformance mediacache" %>'
            };
            
            var chart = new google.visualization.LineChart(document.getElementById('chart_global'));
            chart.draw(data, options);
           
            google.visualization.events.addListener
            (
                chart, 
                'select', 
                function()
                {
                    var selection = chart.getSelection();
                    var date      = '';
                    var site      = '';
                    for (var i = 0; i < selection.length; i++) 
                    {
                        var item = selection[i];
                        if( item.row != null && item.column != null )
                        {
                            date     = data.getFormattedValue(item.row, 0);
                            site     = data.getColumnLabel(item.column);
                        }
                    }
                    ShowDetailsOverlay(date, site, 'globall');
                }
            );
        }
                
        function drawHome()
        {
            var data = google.visualization.arrayToDataTable
            ([
                <%=chartData_Home %>
            ]);
            var options = 
            {
                title: '<%= Request.QueryString["type"] == "sites" ? "Performance Homepage" : "Peformance Teksten" %>'
            };
            
            var chart = new google.visualization.LineChart(document.getElementById('chart_home'));
            chart.draw(data, options);

            google.visualization.events.addListener
            (
                chart, 
                'select', 
                function()
                {
                    var selection = chart.getSelection();
                    var date      = '';
                    var site      = '';
                    for (var i = 0; i < selection.length; i++) 
                    {
                        var item = selection[i];
                        if( item.row != null && item.column != null )
                        {
                            date     = data.getFormattedValue(item.row, 0);
                            site     = data.getColumnLabel(item.column);
                        }
                    }
                    ShowDetailsOverlay(date, site, 'home');
                }
            );
        }
        
        function drawSearch()
        {
            var data = google.visualization.arrayToDataTable
            ([
                <%=chartData_Search %>
            ]);
            var options = 
            {
                title: '<%= Request.QueryString["type"] == "sites" ? "Performance Searchresult" : "Peformance Plaatjes" %>'
            };
            
            var chart = new google.visualization.LineChart(document.getElementById('chart_search'));
            chart.draw(data, options);

            google.visualization.events.addListener
            (
                chart, 
                'select', 
                function()
                {
                    var selection = chart.getSelection();
                    var date      = '';
                    var site      = '';
                    for (var i = 0; i < selection.length; i++) 
                    {
                        var item = selection[i];
                        if( item.row != null && item.column != null )
                        {
                            date     = data.getFormattedValue(item.row, 0);
                            site     = data.getColumnLabel(item.column);
                        }
                    }
                    ShowDetailsOverlay(date, site, 'search');
                }
            );
        }
        
        function drawAcco()
        {
            var data = google.visualization.arrayToDataTable
            ([
                <%=chartData_Acco %>
            ]);
            var options = 
            {
                title: 'Performance Accopage'
            };
            
            var chart = new google.visualization.LineChart(document.getElementById('chart_acco'));
            chart.draw(data, options);

            google.visualization.events.addListener
            (
                chart, 
                'select', 
                function()
                {
                    var selection = chart.getSelection();
                    var date      = '';
                    var site      = '';
                    for (var i = 0; i < selection.length; i++) 
                    {
                        var item = selection[i];
                        if( item.row != null && item.column != null )
                        {
                            date     = data.getFormattedValue(item.row, 0);
                            site     = data.getColumnLabel(item.column);
                        }
                    }
                    ShowDetailsOverlay(date, site, 'acco');
                }
            );
        }
        
        function ShowDetailsOverlay(date, site, type)
        {
			var h,w,l,t;
			
			h = 768;
			w = 1024;
			l = (screen.width - w) / 2;
			t = (screen.height - h) / 2;

			newwind = window.open( "PerformanceDetails.aspx?date=" + date + "&site=" + site + "&type=" + type ,"PerformanceDetails", "scrollbars=no,height="+h+",width="+w+",left="+l+",top="+t);
			newwind.focus();
        }
	</script>
</head>
<body>
    <form id="form1" runat="server">
    <a href="../Main.aspx">Home</a> &gt;&gt; <strong>Snelheid sites</strong>
	    <br />
	    <hr color="#000000" SIZE="1"/>
	    Op deze pagina kunt u de snelheid van verschillende sites bekijken.
	    <br />
	    <br />
	    <table id="Table2" cellSpacing="0" cellPadding="0" width="1%" border="0">
			<tr>
				<td noWrap>Vanaf datum (DD-MM-JJJJ)&nbsp;&nbsp;</td>
				<td noWrap>&nbsp;:&nbsp;</td>
				<td noWrap><asp:textbox id="txtFromDate" runat="server" Columns="10" MaxLength="10" CssClass="StandaardText"></asp:textbox>&nbsp;&nbsp;<A href="Javascript:GetDate('txtFromDate')">Kalender</A>
				</td>
			</tr>
			<tr>
				<td noWrap>T/m datum (DD-MM-JJJJ)&nbsp;&nbsp;</td>
				<td noWrap>&nbsp;:&nbsp;</td>
				<td noWrap><asp:textbox id="txtTillDate" runat="server" Columns="10" MaxLength="10" CssClass="StandaardText"></asp:textbox>&nbsp;&nbsp;<A href="Javascript:GetDate('txtTillDate')">Kalender</A>
				</td>
			</tr>
			<tr>
				<td noWrap>Negeer Min & Max&nbsp;&nbsp;</td>
				<td noWrap>&nbsp;:&nbsp;</td>
				<td noWrap><asp:CheckBox ID="chkIgnoreMinMax" runat="Server" Text="Ja" /></A>
				</td>
			</tr>
			<tr>
				<td noWrap>Toon gemiddelde&nbsp;&nbsp;</td>
				<td noWrap>&nbsp;:&nbsp;</td>
				<td noWrap><asp:CheckBox ID="chkAverage" runat="Server" Text="Ja" /></A>
				</td>
			</tr>
			<tr>
			    <td noWrap valign="top">Sites</td>
				<td noWrap valign="top">&nbsp;:&nbsp;</td>
			    <td noWrap valign="top">
			        <asp:CheckBoxList ID="chkSites" runat="server" CellPadding="0" CellSpacing="0" BorderWidth="0" RepeatColumns="3" Width="400">
			        </asp:CheckBoxList>
			    </td>
			</tr>
			<tr>
				<td noWrap colSpan="3" height="3">&nbsp;</td>
			</tr>
			<tr>
				<td noWrap align="right" colSpan="3"><BR>
					<asp:linkbutton id="btnShow" runat="server" OnClick="btnShow_Click">Toon Resultaten&nbsp;&gt;&gt;</asp:linkbutton>
				</td>
			</tr>
	    </table>
	    <br />
	    <hr />
        <div id="chart_global" style="width: 1000px; height: 500px;"></div>
        <div id="chart_home" style="width: 1000px; height: 500px;"></div>
        <div id="chart_search" style="width: 1000px; height: 500px;"></div>
        <div id="chart_acco" style="width: 1000px; height: 500px;"></div>
    </form>
</body>
</html>
