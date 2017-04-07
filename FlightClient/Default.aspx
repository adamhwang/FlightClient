<%@ Page Title="" Language="C#" MasterPageFile="~/Masterpages/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FlightClient.Default" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
    .AirportSelect 
    {
        width:200px;    
    }
    .ErrMess
    {
        color:#FF0000;    
    }
    </style>
    <script src="../Scripts/Request.js" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">
        var searchAirportTimeout;
        function Trim(str) {
            return str.replace(/^\s+|\s+$/g, '');
        }

        function ShowSelectedOutFlight() {
            var hOutwardSelected = document.getElementById('<%=hOutwardSelected.ClientID %>');
            alert(hOutwardSelected.value);
        }

        function ShowSelectedRetFlight() {
            var hReturnSelected = document.getElementById('<%=hReturnSelected.ClientID %>');
            alert(hReturnSelected.value);
        }

        function GetOutwardFlight() {
            var hOutwardID = document.getElementById('<%=hOutwardID.ClientID %>');
            alert(hOutwardID.value);

            if (hOutwardID.value != '') {
                var total = document.getElementById('<%=lblSummary.ClientID %>');
                if (total)
                    total.innerHTML = hOutwardID.value.split(',')[6];
                showSummary();
            }
        }

        function delaySelectOut() {
            var hSelRowOut = document.getElementById('<%=hSelRowOut.ClientID %>');
            if (hSelRowOut && hSelRowOut.value != '')
                window.frames[0].HighLite(hSelRowOut.value)

        }

        function SelectOutwardFlight() {
            setTimeout("delaySelectOut()", 1000)
        }

        function GetReturnFlight() {
            var hReturnID = document.getElementById('<%=hReturnID.ClientID %>');
            alert(hReturnID.value);
        }

        function delaySelectRet() {
            var hSelRowRet = document.getElementById('<%=hSelRowRet.ClientID %>')
            if (hSelRowRet && hSelRowRet.value != '')
                window.frames[1].HighLite(hSelRowRet.value)
        }

        function SelectReturnFlight() {
            setTimeout("delaySelectRet()", 1000)
        }

        function SelectArriveAirport(x) {
            if (x.value != "") {
                var depart = document.getElementById('<%=ddlDepart.ClientID %>');
                var arrive = document.getElementById('<%=ddlArrive.ClientID %>');

                var obj = airports[x.value];

                arrive.options.length = 0
                var opt = document.createElement('option');
                opt.value = "";
                opt.innerHTML = "Select airport";
                arrive.appendChild(opt);

                for (var i = 0; i < obj.length; i++) {
                    var opt = document.createElement('option');

                    opt.value = obj[i];
                    opt.innerHTML = obj[i];

                    for (var j = 0; j < depart.length; j++) {
                        if (depart.options[j].value == obj[i]) {
                            opt.innerHTML = depart.options[j].text;
                            break;
                        }
                    }

                    arrive.appendChild(opt);
                }
            }
        }

        function toggleSummary() {
            var summary = document.getElementById("Summary");
            summary.style.display = summary.style.display == "" ? "none" : "";
        }

        function toggleErrors() {
            var error = document.getElementById("Errors");
            error.style.display = error.style.display == "" ? "none" : "";
        }

        function toggleLogging() {
            var logging = document.getElementById("Logging");
            logging.style.display = logging.style.display == "" ? "none" : "";
        }

        function SelectRetBag(outID) {
            var retID = outID.replace("repServices", "repServicesReturn");
            var ddl1 = document.getElementById(outID);
            var ddl2 = document.getElementById(retID);
            if (ddl1 && ddl2) {
                ddl2.selectedIndex = ddl1.selectedIndex;
            }
        }

        function formatXML(xml) {
            var reg = /(>)\s*(<)(\/*)/g;
            var wsexp = / *(.*) +\n/g;
            var contexp = /(<.+>)(.+\n)/g;
            xml = xml.replace(reg, '$1\n$2$3').replace(wsexp, '$1\n').replace(contexp, '$1\n$2');
            var pad = 0;
            var formatted = '';
            var lines = xml.split('\n');
            var indent = 0;
            var lastType = 'other';
            // 4 types of tags - single, closing, opening, other (text, doctype, comment) - 4*4 = 16 transitions 
            var transitions = {
                'single->single': 0,
                'single->closing': -1,
                'single->opening': 0,
                'single->other': 0,
                'closing->single': 0,
                'closing->closing': -1,
                'closing->opening': 0,
                'closing->other': 0,
                'opening->single': 1,
                'opening->closing': 0,
                'opening->opening': 1,
                'opening->other': 1,
                'other->single': 0,
                'other->closing': -1,
                'other->opening': 0,
                'other->other': 0
            };

            for (var i = 0; i < lines.length; i++) {
                var ln = lines[i];
                var single = Boolean(ln.match(/<.+\/>/)); // is this line a single tag? ex. <br />
                var closing = Boolean(ln.match(/<\/.+>/)); // is this a closing tag? ex. </a>
                var opening = Boolean(ln.match(/<[^!].*>/)); // is this even a tag (that's not <!something>)
                var type = single ? 'single' : closing ? 'closing' : opening ? 'opening' : 'other';
                var fromTo = lastType + '->' + type;
                lastType = type;
                var padding = '';

                indent += transitions[fromTo];
                for (var j = 0; j < indent; j++) {
                    padding += '    ';
                }

                formatted += padding + ln + '\n';
            }

            return formatted;
        };

        function replaceXML(xml) {
            var txt = xml;
            while (txt.indexOf('&lt;') >= 0)
                txt = txt.replace('&lt;', '<')
            while (txt.indexOf('&gt;') >= 0)
                txt = txt.replace('&gt;', '>')
            while (txt.indexOf('&quot;') >= 0)
                txt = txt.replace('&quot;', '\'')

            return txt;
        }

        function niceXML() {
            var obj1 = document.getElementById('<%=tbReq.ClientID %>');
            var obj2 = document.getElementById('<%=tbRes.ClientID %>');
            obj1.value = formatXML(replaceXML(obj1.value));
            obj2.value = formatXML(replaceXML(obj2.value));
        }

        function showSummary() {
            var summary = document.getElementById("Summary");
            summary.style.display = "";
        }

        function clearSelection() {
            var hOutwardID = document.getElementById('<%=hOutwardID.ClientID %>');
            var hOutwardSelected = document.getElementById('<%=hOutwardSelected.ClientID %>');
            var hReturnSelected = document.getElementById('<%=hReturnSelected.ClientID %>');

            hOutwardID.value = "";
            hOutwardSelected.value = "";
            hReturnSelected.value = "";

            copyValues();

        }

        function clearOutward() {
            var hOutwardID = document.getElementById('<%=hOutwardID.ClientID %>');
            hOutwardID.value = "";
            copyValues();
        }

        function clearOutwardSelected() {
            var hOutwardSelected = document.getElementById('<%=hOutwardSelected.ClientID %>');
            hOutwardSelected.value = "";
            copyValues();
        }

        function clearReturnSelected() {
            var hReturnSelected = document.getElementById('<%=hReturnSelected.ClientID %>');
            hReturnSelected.value = "";
            copyValues();
        }

        function copyValues() {
            var hOutwardSelected = document.getElementById('<%=hOutwardSelected.ClientID %>');
            var hReturnSelected = document.getElementById('<%=hReturnSelected.ClientID %>');
            var div1 = document.getElementById('outFlightDetails');
            var div2 = document.getElementById('retFlightDetails');

            if (div1 && div2) {
                div1.innerHTML = hOutwardSelected.value;
                div2.innerHTML = hReturnSelected.value;
            }
        }

        function CloseAirportSearchRes() {
            var pop = document.getElementById("searchAirportRes");
            pop.style.visibility = "hidden";
        }

        function SearchAirport(searchText) {
            clearTimeout(searchAirportTimeout);
            searchAirportTimeout = setTimeout("SearchAirportAfterTimeout('" + searchText + "')", 500);
        }
        function SearchAirportAfterTimeout(searchText) {
            var d = document.getElementById("searchAirportRes");
            var dt = document.getElementById("searchAirportText");
            searchText = Trim(searchText);
            if (searchText.length < 3) {
                d.style.visibility = "hidden";
                return;
            }

            var div = document.getElementById("searchAirportResText")
            if (div)
                div.innerHTML = "";
            var url = "AirportSearch.aspx?val=" + searchText;
            var req = new Request();
            req.GetNoCache(url, SearchAirportResult);
        }
        function SearchAirportResult(result) {
            if (result.readyState != ReadyState.Complete) {
                return;
            }
            if (result.status == HttpStatus.OK && result.responseText != "") {
                var dt = document.getElementById("searchAirportResText");
                if (dt == null) {
                    return;
                }
                dt.innerHTML = result.responseText;
                document.getElementById("searchAirportRes").style.visibility = "visible";
            }
        }

        function SelectAirport(IATA, AirportName) {
            var depart = document.getElementById("<%=tbDepart.ClientID %>");
            var arrive = document.getElementById("<%=tbArrive.ClientID %>");
            var ddlDepart = document.getElementById("<%=ddlDepart.ClientID %>");
            var ddlArrive = document.getElementById("<%=ddlArrive.ClientID %>");

            var rbSelect = getCheckedRadio(document.getElementsByName("rbAirport"));

            if (rbSelect) {
                switch (rbSelect.value) {
                    case "Depart": depart.value = IATA;
                        setSelectedValue(ddlDepart, IATA);
                        break;
                    case "Arrive": arrive.value = IATA;
                        setSelectedValue(ddlArrive, IATA);
                        break;
                }
            }

            CloseAirportSearchRes();
        }

        function getCheckedRadio(radio_group) {
            for (var i = 0; i < radio_group.length; i++) {
                var button = radio_group[i];
                if (button.checked) {
                    return button;
                }
            }
            return undefined;
        }

        function setSelectedValue(selectObj, valueToSet) {
            for (var i = 0; i < selectObj.options.length; i++) {
                if (selectObj.options[i].value == valueToSet) {
                    selectObj.options[i].selected = true;
                    return;
                }
            }
        }

        function checkDepart(searchText) {
            searchText = Trim(searchText);
            if (searchText.length == 3) {
                var ddlDepart = document.getElementById("<%=ddlDepart.ClientID %>");
                setSelectedValue(ddlDepart, searchText.toUpperCase());
            }
        }

        function checkArrive(searchText) {
            searchText = Trim(searchText);
            if (searchText.length == 3) {
                var ddlArrive = document.getElementById("<%=ddlArrive.ClientID %>");
                setSelectedValue(ddlArrive, searchText.toUpperCase());
            }
        }

        function SetNrOfFlights(x) {
            var lblNrOfFlights = document.getElementById("<%=lblNrOfFlights.ClientID %>");
            if (lblNrOfFlights) {
                lblNrOfFlights.innerHTML = 'Number of flights found: ' + x.toString();
            }
        }

        function effeChecke()
        { }

    
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div align="left">

    <table cellpadding="0" cellspacing="0" border="0">
    <tr>
        <td colspan="2"><asp:CustomValidator ID="cvCheck" runat="server" Display="Dynamic" onservervalidate="cvCheck_ServerValidate" CssClass="ErrMess" ValidationGroup="Check" /></td>
    </tr>
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td colspan="2">
        Departure airport&nbsp;<asp:DropDownList ID="ddlDepart" runat="server" CssClass="AirportSelect" AutoPostBack="false" DataTextField="Text" DataValueField="Value" >

        </asp:DropDownList>
        &nbsp;&nbsp;Arrive airport&nbsp;<asp:DropDownList ID="ddlArrive" runat="server" CssClass="AirportSelect" DataTextField="Text" DataValueField="Value">

        </asp:DropDownList>
        &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbOneWay" runat="server" Checked="true" Text="One way" oncheckedchanged="cbOneWay_CheckedChanged" AutoPostBack="true" />
        <asp:PlaceHolder ID="phSummary" runat="server" Visible="false">
            &nbsp;&nbsp;&nbsp;<a href="javascript:toggleSummary()">Summary</a>
            &nbsp;&nbsp;&nbsp;<asp:Label ID="lblCurrency" runat="server" />&nbsp;&nbsp;<asp:Label ID="lblSummary" runat="server" />
        </asp:PlaceHolder>
         </td>
    </tr>
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td colspan="2" align="right">

            <table cellpadding="0" cellspacing="0">
            <tr>
               
                <td valign="top">
                Depart <asp:TextBox ID="tbDepart" runat="server" Width="60px" /><br />
                Arrive <asp:TextBox ID="tbArrive" runat="server" Width="60px" /><br /><br />
                
                
                 <div style="position:relative; width:250px; z-index:100">
                Find airport: <input type="radio" id="rbDepart" name="rbAirport" value="Depart" checked="checked" /><label for="rbDepart">Depart</label>
                <input type="radio" id="rbArrive" name="rbAirport" value="Arrive" /><label for="rbArrive">Arrive</label>
                </div>
                <input type="text" value="find airport" class="Searchbar"
			        onfocus="javascript:if(this.value == 'find airport') {this.value = '';}"
			        onkeyup="javascript:SearchAirport(this.value);"
                    oninit="javascript:this.value = 'find airport';" />  
                </td>
                <td style="width:20px"></td>
                <td valign="top">
                    <asp:TextBox ID="tbPrefCarriers" runat="server" TextMode="MultiLine" Columns="10" Rows="5" /><br />
                    Preferred carriers
                </td>
                <td style="width:20px"></td>
                <td valign="top">
                    <asp:TextBox ID="tbExclCarriers" runat="server" TextMode="MultiLine" Columns="10" Rows="5" /><br />
                    Excluded carriers
                </td>
                <td style="width:20px"></td>
                <td valign="top">
               
                <asp:CheckBoxList ID="cblFareClass" runat="server" RepeatDirection="Vertical">
                <asp:ListItem Selected="True">Economy</asp:ListItem>
                <asp:ListItem>Premium Economy</asp:ListItem>
                <asp:ListItem>Business</asp:ListItem>
                <asp:ListItem>First</asp:ListItem>
                <%-- 
                <asp:ListItem>Premium First</asp:ListItem>
                --%>
                </asp:CheckBoxList>
                </td>
                <td style="width:20px"></td>
                <td valign="top">
                <asp:CheckBox ID="cbLowFare" runat="server" Text="Low fare" Checked="true" />
                &nbsp;&nbsp;&nbsp;<asp:CheckBox ID="cbMultiple" runat="server" Text="Multiple" oncheckedchanged="cbMultiple_CheckedChanged" AutoPostBack="true"/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <asp:CheckBox ID="cbIgnoreCarIATA"  runat="server" Text="Ignore Carrier IATA" />
                </td>
            </tr>
            <tr>
                <td>
                 <div style="position:relative; width:328px;" >
                <div id="searchAirportRes" class="searchres-div">
                    <div class="tip-midden">
                        <div class="searchres-text" id="searchAirportResText"></div>
                    </div>
                    <div class="close-button"><a href="javascript:CloseAirportSearchRes();">Close</a></div>
	            </div>
                </div>
                </td>
                <td style="width:20px"></td>
                <td></td>
                <td style="width:20px"></td>
                <td></td>
                <td style="width:20px"></td>
                <td></td>
                <td style="width:20px"></td>
                <td></td>
            </tr>
            </table>
        </td>
    </tr>

    <asp:PlaceHolder ID="phError" runat="server" >
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td colspan="2"><a href="javascript:toggleErrors()" style="color:red">Error</a></td>
    </tr>
    </asp:PlaceHolder>


    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td colspan="2">Adults&nbsp;<asp:DropDownList ID="ddlAdults" runat="server">
        <asp:ListItem>1</asp:ListItem>
        <asp:ListItem>2</asp:ListItem>
        <asp:ListItem>3</asp:ListItem>
        <asp:ListItem>4</asp:ListItem>
        <asp:ListItem>5</asp:ListItem>
        <asp:ListItem>6</asp:ListItem>
        <asp:ListItem>7</asp:ListItem>
        <asp:ListItem>8</asp:ListItem>
        </asp:DropDownList>
        &nbsp;&nbsp;Children&nbsp;<asp:DropDownList ID="ddlChildren" runat="server">
        <asp:ListItem>0</asp:ListItem>
        <asp:ListItem>1</asp:ListItem>
        <asp:ListItem>2</asp:ListItem>
        <asp:ListItem>3</asp:ListItem>
        <asp:ListItem>4</asp:ListItem>
        <asp:ListItem>5</asp:ListItem>
        <asp:ListItem>6</asp:ListItem>
        <asp:ListItem>7</asp:ListItem>
        <asp:ListItem>8</asp:ListItem>
        </asp:DropDownList>
        &nbsp;&nbsp;Babies&nbsp;<asp:DropDownList ID="ddlBabies" runat="server">
        <asp:ListItem>0</asp:ListItem>
        <asp:ListItem>1</asp:ListItem>
        <asp:ListItem>2</asp:ListItem>
        <asp:ListItem>3</asp:ListItem>
        <asp:ListItem>4</asp:ListItem>
        <asp:ListItem>5</asp:ListItem>
        <asp:ListItem>6</asp:ListItem>
        <asp:ListItem>7</asp:ListItem>
        <asp:ListItem>8</asp:ListItem>
        </asp:DropDownList>
        &nbsp;&nbsp;
        <asp:CheckBox ID="cbUseChildAges" runat="server" Text="Use ChildAges" />
        </td>
    </tr>
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td colspan="2">
        <table cellpadding="0" cellspacing="0">
        <tr>
            <td valign="top">Departure date&nbsp;</td>
            <td valign="top"><asp:Calendar ID="calDepart" runat="server" onselectionchanged="calDepart_SelectionChanged" OnDayRender="Calendar1_DayRender"></asp:Calendar></td>
            <asp:PlaceHolder ID="phReturnDate" runat="server" Visible="false">
            <td valign="top" style="width:20px"></td>
            <td valign="top">Return date&nbsp;</td>
            <td valign="top"><asp:Calendar ID="calReturn" runat="server" OnDayRender="Calendar2_DayRender"></asp:Calendar></td>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phPayment" runat="server" Visible="false">
            <td valign="top" style="width:20px"></td>
            <td valign="top">Creditcard&nbsp;&nbsp;<br /><br /><br /><br /><br />CCV&nbsp;&nbsp;</td>
            <td valign="top"><asp:DropDownList ID="ddlCard" runat="server" onselectedindexchanged="ddlCard_SelectedIndexChanged" AutoPostBack="true">
                <asp:ListItem Value="-1">Select a creditcard</asp:ListItem>
                <asp:ListItem Value="AC">AMEX</asp:ListItem>
                <asp:ListItem Value="EC">MASTERCARD</asp:ListItem>
                <asp:ListItem Value="VI">VISA</asp:ListItem>
            </asp:DropDownList>
            <br /><br />

            <table cellpadding="0" cellspacing="0">
            <tr>
                <td valign="top">
                    <asp:DropDownList ID="ddlCardNumbers" runat="server" onselectedindexchanged="ddlCardNumbers_SelectedIndexChanged" AutoPostBack="true">
                    <asp:ListItem Value="-1">Select a creditcard</asp:ListItem>
                    </asp:DropDownList>
                </td>
                <td style="width:25px"></td>
                <td valign="top">
                    <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td valign="top" nowrap="nowrap">Valid until: <asp:DropDownList ID="ddlValidMonth" runat="server">
                        <asp:ListItem>01</asp:ListItem>
                        <asp:ListItem>02</asp:ListItem>
                        <asp:ListItem>03</asp:ListItem>
                        <asp:ListItem>04</asp:ListItem>
                        <asp:ListItem>05</asp:ListItem>
                        <asp:ListItem>06</asp:ListItem>
                        <asp:ListItem>07</asp:ListItem>
                        <asp:ListItem>08</asp:ListItem>
                        <asp:ListItem>09</asp:ListItem>
                        <asp:ListItem>10</asp:ListItem>
                        <asp:ListItem>11</asp:ListItem>
                        <asp:ListItem>12</asp:ListItem>
                        </asp:DropDownList></td>
                        <td valign="top"><asp:DropDownList ID="ddlValidYear" runat="server"></asp:DropDownList></td>
                    </tr>
                    </table>
                </td>
            </tr>
            </table>
            
            <br /><br /><asp:TextBox ID="tbCCV" runat="server" Text="321" />&nbsp;&nbsp;&nbsp;<asp:Button ID="btnEncryptCC" runat="server" Text="Encrypt CC" OnClick="btnEncryptCC_Click" />
            </td>
            </asp:PlaceHolder>
            
        </tr>
        </table>
        </td>
    </tr>
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td valign="top">
            <table cellpadding="0" cellspacing="0">
            <tr>
                <td><asp:CheckBox ID="cbJet2" runat="server" Text="Check Jet2" /></td>
            </tr>
            <tr>
                <td valign="top"><asp:Button ID="btnRnd" runat="server" Text="Create random flight" OnClick="btnRnd_Click"/>
                &nbsp;&nbsp;&nbsp;<asp:Button ID="btnFlight" runat="server" Text="Get Flights" onclick="btnFlight_Click" CausesValidation="false" />
                &nbsp;&nbsp;&nbsp;<asp:Button ID="btnCheckAvail" runat="server" Text="Check Flight avail/price" OnClick="btnCheckAvail_Click" Visible="false"/>
                &nbsp;&nbsp;&nbsp;<asp:Button ID="btnBook" runat="server" Text="Book Flight" OnClick="btnBookFlight_Click" Visible="false"/>

                </td>
            </tr>
           
            </table>
        </td>
        <td valign="top">
           
        </td>
    </tr>
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td valign="top">

        </td>
        <td valign="top"></td>
    </tr>
    <tr>
        <td valign="top">
        </td>
        <td valign="top"></td>
    </tr>
     <tr>
        <td valign="top">
        </td>
        <td valign="top"></td>
    </tr><tr>
        <td valign="top">
        </td>
        <td valign="top"></td>
    </tr>
    <tr>
        <td valign="top">
            <asp:Label ID="lblNrOfFlights" runat="server" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Label ID="lblTime" runat="server" />
        </td>
        <td valign="top"></td>
    </tr>
     <tr>
        <td valign="top"></td>
        <td valign="top"></td>
    </tr>

    <asp:PlaceHolder ID="phOutbound" runat="server" Visible="false">
    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td valign="top">
        Outward flights<br />
        <iframe id="ifOutbound" name="ifOutbound" src="Outward.aspx<%=qstring %>" frameborder="1" width="820px" height="400px" scrolling="auto" ></iframe>
        </td>
        <td valign="top">&nbsp;&nbsp;<a href="javascript:GetOutwardFlight()">Get outward flight details</a><br /><br />
        
        <a href="javascript:ShowSelectedOutFlight()">Show selected out flight</a>
        <br /><br />
        <a href="javascript:ShowSelectedRetFlight()">Show Selected ret flight</a>
        <br /><br />
        <a href="javascript:clearSelection()">Clear all selections</a>
        <br /><br />
        <a href="javascript:clearOutward()">Clear outward flight only</a>
        <br /><br />
        <a href="javascript:clearOutwardSelected()">Clear selected out flightt only</a>
        <br /><br />
        <a href="javascript:clearReturnSelected()">Clear selected ter flightt only</a>
        
        </td>
    </tr>
    <tr>
        <td valign="top">
        <div id="outFlightDetails"></div>
        <div id="retFlightDetails"></div>
        </td>
    </tr>
    </asp:PlaceHolder>


    <tr><td colspan="2" style="height:10px"></td></tr>
    <tr>
        <td><asp:TextBox ID="tbReq" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
        <td valign="top"></td>
    </tr>
    <tr>
        <td><asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
        <td valign="top"></td>
    </tr>
    
    <tr>
        <td><asp:TextBox ID="tbExtended" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
        <td valign="top"><br />
            <asp:CheckBox ID="chkElsyEnc" runat="server" Text="Elsy Encrypt" />
             <br /><asp:Button ID="btnEncryptThis" Text="Encrypt this" OnClick="btnEncryptThis_Click" runat="server" />
            <br /><asp:Button ID="Button1" Text="Decrypt this" OnClick="btnDecryptThis_Click" runat="server" />
            <br /><asp:Button ID="Button2" Text="Amadeus this" OnClick="btnAmadeusConfig_Click" runat="server" />
            <br /><asp:Button ID="Button3" Text="Smartwing this" OnClick="btnSmartwings_Click" runat="server" />
        </td>
    </tr>

    <tr>
        <td>
        
            <asp:TextBox ID="tbLFR" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
        
        </td>
        <td valign="top"></td>
    </tr>
    </table>

    <div id="AirPricingResultDiv" style="display:none">
        <table cellpadding="0" cellspacing="0" style="width:400px; height:600px; border:1px solid black; background-color:#c0c0c0">
        <tr>
            <td valign="top">
            <asp:TextBox ID="tbAirPriceRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
        </tr>
        </table>
    </div>

  
    <div id="Summary" style="display:none; position:absolute; top:0px;">
        <table cellpadding="0" cellspacing="0" style="width:400px; height:600px; border:1px solid black; background-color:#c0c0c0">
        <tr>
            <td valign="top" style="height:30px; width:380px"><b>Summary</b></td>
            <td align="right" style="height:30px"><asp:Button ID="btnSummary" runat="server" Text="Request summary" OnClick="btnSummary_Click" />&nbsp;&nbsp;&nbsp;<a href="javascript:toggleSummary()">x</a>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" valign="top" id="SumContent" style="height:570px;"><asp:Literal ID="litSummary" runat="server" /></td>
        </tr>
        </table>
    </div>
    

    <div id="Errors" style="display:none; position:absolute; top:0px;">
        <table cellpadding="0" cellspacing="0" style="width:600px; height:600px; border:1px solid black; background-color:#F0F0F0" border="1">
        <tr>
            <td valign="top" style="height:30px; width:580px"><b>Error</b></td>
            <td align="right" style="height:30px"><a href="javascript:toggleErrors()">x</a>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" valign="top" id="ErrorContent" style="color:red; height:570px"><asp:Literal ID="litError" runat="server" /></td>
        </tr>
        </table>
    </div>

     <div id="Logging" style="display:none; position:absolute; top:0px;">
        <table cellpadding="0" cellspacing="0" style="width:600px; height:600px; border:1px solid black; background-color:#F0F0F0" border="1">
        <tr>
            <td valign="top" style="height:30px; width:580px"><b>Logging</b></td>
            <td align="right" style="height:30px"><a href="javascript:toggleLogging()">x</a>&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" valign="top" id="Td1" style="color:red; height:570px"><asp:Literal ID="Literal1" runat="server" /></td>
        </tr>
        </table>
    </div>


    <asp:HiddenField ID="hOutwardID" runat="server" />
    <asp:HiddenField ID="hSelRowOut" runat="server" />
    <asp:HiddenField ID="hReturnID" runat="server" />
    <asp:HiddenField ID="hSelRowRet" runat="server" />
    <asp:HiddenField ID="hCCFee" runat="server" Value="0" />
    <asp:HiddenField ID="hOutwardTotal" runat="server" Value="0" />
    <asp:HiddenField ID="hReturnTotal" runat="server" Value="0"  />
    <asp:HiddenField ID="hMultipleRoutes" runat="server" />
    <asp:HiddenField ID="hCheckAvailTotal" runat="server" Value="0" />

    <asp:HiddenField ID="hOutwardSelected" runat="server" />
    <asp:HiddenField ID="hReturnSelected" runat="server" />

    </div>
</asp:Content>
