<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Outward.aspx.cs" Inherits="FlightClient.Outward" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
      Table
      {
        font-size: .80em;
    font-family: "Helvetica Neue", "Lucida Grande", "Segoe UI", Arial, Helvetica, Verdana, sans-serif;    
      }
    .HiLite
    {
        background-color: Blue;  
        color:White;  
    
    }
    .Normal
    {
        background-color: White; 
        color:Black;
    }
    .link
    {
    font-family: "Helvetica Neue", "Lucida Grande", "Segoe UI", Arial, Helvetica, Verdana, sans-serif; 
    text-decoration: none;   
    }
   </style>
    <script language="javascript">
        var arItems = [];
        var arXml = [];
        <asp:Repeater ID="repOutItems" runat="server">
        <ItemTemplate>
        arItems.push("<%# DataBinder.Eval(Container.DataItem, "FlightID")%>,<%# DataBinder.Eval(Container.DataItem, "Carrier")%>,<%# DataBinder.Eval(Container.DataItem, "FlightNr")%>,<%# DataBinder.Eval(Container.DataItem, "DepTime")%>,<%# DataBinder.Eval(Container.DataItem, "ArrTime")%>,<%# DataBinder.Eval(Container.DataItem, "Total")%>,<%# DataBinder.Eval(Container.DataItem, "AdultTaxIncl")%>,<%# DataBinder.Eval(Container.DataItem, "ChildTaxIncl")%>,<%# DataBinder.Eval(Container.DataItem, "InfantTaxIncl")%>,<%# DataBinder.Eval(Container.DataItem, "Depart")%>,<%# DataBinder.Eval(Container.DataItem, "Arrive")%>,R<%=ctr2 %>");
        arXml.push("<%# DataBinder.Eval(Container.DataItem, "GroupedFlightOption")%>");
        <%ctr2++; %>
        </ItemTemplate>
        </asp:Repeater>

        var arSortItem = [];

        var show = true;
        function Normalize() {
            var nlist = document.getElementsByTagName("TD");
            for (x = 0; x < nlist.length; x++) {
                nlist[x].className = "Normal";
            }
        }

        function HighLite(x) {
            Normalize();

            var items = x.split('~');

            for (t = 0; t < items.length; t++) {

                var row = document.getElementById(items[t]);
                if (row) {
                    var cols = row.childNodes;
                    if (cols) {
                        for (i = 0; i < cols.length; i++) {
                            cols[i].className = "HiLite";
                            if (i == 0) {
                                row.click();
                            }
                        }
                    }
                }
            }
        }

        var arSelect = [];

        function RemoveArrayItem(arItem) {
            if (arItem >= 0) {
                if (arItem == 0)
                    arSelect.shift();
                else if (arItem == arSelect.length - 1)
                    arSelect.pop();
                else arSelect.splice(arItem, 1);
            }
        }

        function GetArrayItem(x, car, f, t1, t2, total, at, ct, it, arr, dep, r) {
            var select = -1;
            for (var c = 0; c < arSelect.length; c++) {
                if (arSelect[c] == x + "," + car + "," + f + "," + t1 + "," + t2 + "," + total.replace(',', '.') + "," + at.replace(',', '.') + "," + ct.replace(',', '.') + "," + it.replace(',', '.') + ',' + arr + ',' + dep + ',' + r  ) {
                    select = c;
                    break;
                }
            }
            return select;
        }

        function HighLite1(x) {
            var row = document.getElementById(x);
            if (row) {
                var cols = row.childNodes;
                if (cols) {
                    for (i = 0; i < cols.length; i++) {
                        cols[i].className = "HiLite";
//                        if (i == 0) {
//                            row.click();
//                        }
                    }
                }
            }
        }

        function UnHighLite(x) {
            var row = document.getElementById(x);
            if (row) {
                var cols = row.childNodes;
                if (cols) {
                    for (i = 0; i < cols.length; i++) {
                        cols[i].className = "Normal";
//                        if (i == 0) {
//                            row.click();
//                        }
                    }

                }
            }
        }

        function SelectOutwardFlight(x, car, f, t1, t2, total, at, ct, it, arr, dep, r) {

            var itemNr = GetArrayItem(x, car, f, t1, t2, total, at, ct, it, arr, dep, r);
            RemoveArrayItem(itemNr);
            
            /*
            HighLite(r)
            */

            if (itemNr >= 0)
                UnHighLite(r);
            else {
                HighLite1(r);
                arSelect.push(x + "," + car + "," + f + "," + t1 + "," + t2 + "," + total.replace(',', '.') + "," + at.replace(',', '.') + "," + ct.replace(',', '.') + "," + it.replace(',', '.') + ',' + arr + ',' + dep + ','  + r);
            }

            var outward = window.top.document.getElementById("MainContent_hOutwardID");
            var outwardRow = window.top.document.getElementById("MainContent_hSelRowOut");
            //outward.value = x + "," + y + "," + f + "," + t1 + "," + t2 + "," + total.replace(',', '.') + "," + at.replace(',', '.') + "," + ct.replace(',', '.') + "," + it.replace(',', '.');
            //outwardRow.value = r;

            var content = "";
            var contentRows = "";
            for (i = 0; i < arSelect.length; i++) {
                if (i > 0) {
                    content += '[';
                    contentRows += '~';
                }
                var value = arSelect[i]
                content += value;

                var values = value.split(',')
                contentRows += values[values.length-1];
            }
            
            outward.value = content;
            outwardRow.value = contentRows;

            
        }

        function SelectRowOut() {
            window.top.delaySelectOut();
        }

        window.onscroll = function () {
            if (parent.document.frames["ifReturn"])
                parent.document.frames["ifReturn"].document.body.scrollLeft = document.body.scrollLeft;
        }
        
        function SelectOutwardFlightAr(x)
        {
            var items = arItems[x].split(",")
            SelectOutwardFlight(items[0], items[1], items[2], items[3], items[4], items[5], items[6], items[7], items[8], items[9], items[10], items[11] );
        }

        function ShowContent(x)
        {
            var url = "ShowContent.aspx?content=" + x
            var theWindow = window.open(url, "myWindow", "width=1200, height=600")
            theWindow.focus();
        }

        function GetContent(x)
        {
            return arXml[x]
        }

        function StoreOutFlight(x)
        {
            var outwardSel = window.top.document.getElementById("MainContent_hOutwardSelected");
            outwardSel.value = arItems[x];

            window.top.copyValues();
        }

        function StoreRetFlight(x)
        {
            var returnSel = window.top.document.getElementById("MainContent_hReturnSelected");
            returnSel.value = arItems[x];

            window.top.copyValues();
        }

        /************************** Sorting ************************************************/

        function addElement() {

          var ni = document.getElementById('sortDiv');
          var numi = document.getElementById('sortVal');
          var num = (document.getElementById('sortVal').value -1)+ 2;

          numi.value = num;
          var newdiv = document.createElement('div');
          var divIdName = 'sort'+num;

          newdiv.setAttribute('id',divIdName);
          
          var textIdName = 'txtSort' + num;
          var rbIdName = 'rbSort' + num;

          newdiv.innerHTML = "<input type='text' name='" +  textIdName + "' style='width:200px'> <input type='radio' name='" +  rbIdName + "' id='" + rbIdName + "1' value='AND' checked='checked'/><label for='" + rbIdName + "1'>AND</label> / <input type='radio' name='" +  rbIdName + "' id='" + rbIdName + "2' value='OR'><label for='" + rbIdName + "2'>OR<label> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <a href=\"javascript:addElement()\" class=\"link\">Add</a> / <a href=\"javascript:removeElement(\'" +divIdName+ "\')\" class=\"link\">Remove</a>";

          arSortItem.push("" + divIdName+ ",'','AND'");

          ni.appendChild(newdiv);

        }

    function removeElement(divNum) {

        var d = document.getElementById('sortDiv');
        var olddiv = document.getElementById(divNum);
        var numi = document.getElementById('sortVal');
      
        if (arSortItem.length>1)
        {
            var itemNr = getSortArrayItem(divNum)
            removeSortArrayItem(itemNr);

            d.removeChild(olddiv);
        }
    }

    function getSortArrayItem(x)
    {
        var select = -1;
        for (var c = 0; c < arSortItem.length; c++) {
            if (arSortItem[c].split(',')[0] == x ) {
                select = c;
                break;
            }
        }
        return select;
    }

    function removeSortArrayItem(arItem) {
            if (arItem >= 0) {
                if (arItem == 0)
                    arSortItem.shift();
                else if (arItem == arSortItem.length - 1)
                    arSortItem.pop();
                else arSortItem.splice(arItem, 1);
            }
        }

    function SortContent()
    {
        if (arSortItem.length>0)
        {
            var sortText = '';
            var andTexts = '';
            var orTexts = '';
            for (var c = 0; c < arSortItem.length; c++)
            {
                var num = arSortItem[c].split(',')[0].replace('sort','');
                var sortTextObj = document.getElementById('txtSort' + num);
                var andOrObj = document.getElementsByName('rbSort' + num);
                if (sortTextObj && andOrObj && sortTextObj.value!='')
                {
                    if (andOrObj[0].checked)
                    {
                        if (andTexts!='') andTexts += '|';
                        andTexts += sortTextObj.value
                    }
                    else
                    {
                        if (orTexts!='') orTexts += '|';
                        orTexts += sortTextObj.value;
                    }
                   
                    if (sortText!='') sortText += '|';
                    sortText += sortTextObj.value;
                }
            }

            if (sortText!='')
            {
                var andText = [];
                var orText = [];
                        
                if (andTexts!='')
                {
                    if (andTexts.indexOf('|')>0)
                        andText = andTexts.split('|')
                    else
                        andText.push(andTexts);
                 }

                if (orTexts!='')
                {
                    if (orTexts.indexOf('|')>0)
                        orText = orTexts.split('|')
                    else
                        orText.push(orTexts);
                }

                var nrOfFlights = 0;
                for (var c = 0; c < arItems.length; c++)
                {
                    var rowObj = document.getElementById("R" + c);
                    if (rowObj )
                    {   
                        var gotAllANDs = true;
                        for (var i = 0; i< andText.length; i++)
                        {
                            if (gotAllANDs && rowObj.innerHTML.indexOf(andText[i])>=0)
                                gotAllANDs = true;
                            else
                                gotAllANDs = false;
                        }

                        var gotORs = false;
                        for (var i = 0; i< orText.length; i++)
                        {
                            if (rowObj.innerHTML.indexOf(orText[i])>=0)
                                gotORs = !gotORs? true : gotORs;
                        }

                        if ( gotAllANDs || gotORs)
                        {
                            rowObj.style.display = "";
                            nrOfFlights++;
                        }
                        else
                            rowObj.style.display = "none";
                    }
                }

                window.top.SetNrOfFlights(nrOfFlights);
            }
        }
    }

    function unSort()
    {
        for (var c = 0; c < arItems.length; c++)
        {
            var obj = document.getElementById("R" + c);
            if (obj )
                    obj.style.display = "";
        }
        window.top.SetNrOfFlights(arItems.length);
    }

    

    </script>
</head>
<body onload="javascript:SelectRowOut();addElement();">
    <form id="form1" runat="server">
    <div>
    <table cellpadding="1" cellspacing="1" border="1" style="width:1500px">
    <tr>
        <td>Carrier</td>
        <td>Flight Nr</td>
        <td>Depart</td>
        <td>Arrive</td>
        <td>Dept. time</td>
        <td>Arr. time</td>
        <td>Total fare (incl tax)</td>
        <td>Adult</td>
        <td>Adult (incl tax)</td>
        <td>Child</td>
        <td>Child (incl tax)</td>
        <td>Infant</td>
        <td>Infant (incl tax)</td>
        <td>Taxes</td>
        <td>Baggage</td>
        <td>Fees</td>
        <td>IsStopover</td>
        <td>Leg Arrive time</td>
        <td>Leg Depart time</td>
        <td>Leg Flight Nr</td>
        <td>StopOverAirport</td>
        <td>GroupedFlightOption</td>
    </tr>
    <asp:Repeater ID="repOutboundFlights" runat="server">
    <ItemTemplate>
    <tr id="R<%=ctr %>" onclick="javascript:<%# DataBinder.Eval(Container.DataItem, "JScript")%>" onmouseover="this.style.cursor='pointer'">
        <td><%# DataBinder.Eval(Container.DataItem, "Carrier")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "FlightNr")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Depart")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Arrive")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "DepTime")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "ArrTime")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Total")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Adult")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "AdultTaxIncl")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Child")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "ChildTaxIncl")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Infant")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "InfantTaxIncl")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Taxes")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Baggage")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "Fees")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "IsStopOver")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "LegArrTime")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "LegDepTime")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "LegFlightNr")%></td>
        <td><%# DataBinder.Eval(Container.DataItem, "StopOverAirport")%></td>
        <td><a href="javascript:ShowContent('<%=ctr %>');" style="color:#FF0000" >ShowContent </a>
        <div style="display:<%# DataBinder.Eval(Container.DataItem, "IsRoundtrip").Equals("False")?"":"none"%>">
            <a href="javascript:StoreOutFlight('<%=ctr %>')">Store as outward flight</a>
        </div>
        <div style="display:<%# DataBinder.Eval(Container.DataItem, "IsRoundtrip").Equals("False")?"":"none"%>">
            <a href="javascript:StoreRetFlight('<%=ctr %>')">Store as return flight</a>
        </div>
        
        </td>
    </tr>
    <%ctr++; %>
    </ItemTemplate>
    </asp:Repeater>
    </table>
    </div>
    <br /><br />
    <input type="hidden" value="0" id="sortVal" />
    <div id="sortDiv"></div>
    <br />
    <a href="javascript:unSort()" class="link">UnSort >></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="javascript:SortContent()" class="link">Sort >></a>
    </div>
    <script language="javascript" type="text/javascript">
        function CountFlights() {
            var nrOfFl = '<%=ctr %>';
            if (nrOfFl != '')
                window.top.SetNrOfFlights(nrOfFl);
        }
    </script>
    </form>
</body>
</html>
