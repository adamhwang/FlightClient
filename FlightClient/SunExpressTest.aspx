<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SunExpressTest.aspx.cs" Inherits="FlightClient.SunExpressTest" ValidateRequest="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="/Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        function formatXml(xml) {
            var formatted = '';
            var reg = /(>)(<)(\/*)/g;
            xml = xml.replace(reg, '$1\r\n$2$3');
            var pad = 0;
            jQuery.each(xml.split('\r\n'), function (index, node) {
                var indent = 0;
                if (node.match(/.+<\/\w[^>]*>$/)) {
                    indent = 0;
                } else if (node.match(/^<\/\w/)) {
                    if (pad != 0) {
                        pad -= 1;
                    }
                } else if (node.match(/^<\w[^>]*[^\/]>.*$/)) {
                    indent = 1;
                } else {
                    indent = 0;
                }

                var padding = '';
                for (var i = 0; i < pad; i++) {
                    padding += '  ';
                }

                formatted += padding + node + '\r\n';
                pad += indent;
            });

            return formatted;
        }

        function prettyPrint()
        {
            var src = document.getElementById("<%= tb2.ClientID%>").value;
            
            var dest = document.getElementById("prettyXML");
            xml_formatted = formatXml(src);
            xml_escaped = xml_formatted.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/ /g, '&nbsp;').replace(/\n/g, '<br />');
            dest.innerHTML = xml_escaped;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table>
            <tr>
                <td valign="top"><asp:TextBox ID="tb1" runat="server" TextMode="MultiLine" Width="1000px" Height="200px" /></td>
                <td valign="top">
                    <asp:DropDownList ID="ddl1" runat="server">
                        <asp:ListItem Text="SF" />
                        <asp:ListItem Text="SF RR" />
                    </asp:DropDownList>
                    <br /><br />
                    ORI <asp:TextBox ID="ORI" runat="server" width="50px" /> DES <asp:TextBox ID="DES" runat="server" width="50px" /><br />
                    Depdate out (yyyy-mm-dd)<br />
                    <asp:TextBox ID="date1" runat="server" Width="100px" /><br />
                    Depdate return (yyyy-mm-dd)<br />
                    <asp:TextBox ID="date2" runat="server" Width="100px" /><br /><br />
                    <asp:Button ID="btn1" runat="server" Text="Execute" OnClick="btn1_Click" />
                   
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <asp:TextBox ID="tb2" runat="server" TextMode="MultiLine" Width="1000px" Height="200px" />
                </td>
                <td valign="top">
                    <asp:Label ID="lb1" runat="server" /> 
                    <br /><br />
                    <a href="javascript:prettyPrint()">Pretty print XML</a>
                </td>
            </tr>

        </table>
        <div id="prettyXML"></div>
        
        <br />
        
    </div>
    </form>
</body>
</html>
