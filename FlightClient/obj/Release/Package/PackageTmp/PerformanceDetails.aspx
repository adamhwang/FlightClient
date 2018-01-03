<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PerformanceDetails.aspx.cs" Inherits="FlightClient.PerformanceDetails" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Performance details</title>
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
	<script type="text/javascript">
	    //See https://google-developers.appspot.com/chart/interactive/docs/gallery/linechart
	    
	     google.load("visualization", "1", {packages:["corechart"]});
	     google.setOnLoadCallback(drawChart);
	     
        function drawChart()
        {
            var data = google.visualization.arrayToDataTable
            ([
                <%=chartData %>
            ]);
            var options = 
            {
                title: '<%=chartTitle %>'
            };
            
            var chart = new google.visualization.LineChart(document.getElementById('chart'));
            chart.draw(data, options);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
     <div id="chart" style="width: 1000px; height: 500px;"></div>
    </form>
</body>
</html>
