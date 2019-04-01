<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeEncode.aspx.cs" Inherits="FlightClient.DeEncode" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>DeEncode</title>
    <script src="/Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        function ClearTxt()
        {
            var tbReq = document.getElementById("<%= tbReq.ClientID%>");
            var tbRes = document.getElementById("<%= tbRes.ClientID%>");
            var dest = document.getElementById("pretty");

            tbReq.value = "";
            tbRes.value = "";
            dest.innerHTML = "";
        }

        function Copy2Clipboard()
        {
            var tbRes = document.getElementById("<%= tbRes.ClientID%>");
            tbRes.focus();
            tbRes.select();
            document.execCommand('copy');

            if (document.getElementById("cbClear").checked)
                ClearTxt();
        }

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

        function prettyPrintXML()
        {
            var src = document.getElementById("<%= tbRes.ClientID%>").value;
            
            xml_formatted = formatXml(src);
            xml_escaped = xml_formatted.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/ /g, '&nbsp;').replace(/\n/g, '<br />');

            if (document.getElementById("cbInTxt").checked)
                storeToPretty(xml_formatted);
            else {
                var dest = document.getElementById("pretty");
                dest.innerHTML = xml_escaped;
            }

        }

        function beautifyJSON(x) {
            
            try {
                var txt = document.getElementById("<%= tbRes.ClientID%>");

                if (txt) {
                    var jsonContent = txt.innerHTML == "" ? txt.value : txt.innerHTML;
                    var jsonBeautified = JSON.stringify(JSON.parse(jsonContent), null, 2);

                    
                    if (document.getElementById("cbInTxt").checked)
                        storeToPretty(jsonBeautified);
                    else
                    {
                        var dest = document.getElementById("pretty");
                        dest.innerHTML = jsonBeautified;
                    }
                   
                   
                    
                }
            }
            catch (ex) {
                alert('Unable to beautify. Invalid JSON!');
            }
        }

        function storeToPretty(x)
        {
            var dest = document.getElementById("pretty");
            dest.innerHTML = "";

            var input = document.createElement("textarea");
            input.name = "post"; input.maxLength = "5000";
            input.cols = "150"; input.rows = "50";
            input.value = x;
            dest.appendChild(input);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <table>
            <tr>
                <td valign="top">
                    <asp:TextBox ID="tbReq" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
                </td>
                <td valign="top">
                    <asp:Button ID="btnURLDecode" Text="URL Decode" runat="server" OnClick="btnURLDecode_Click" />
                    <br /><br />
                    <asp:Button ID="Button1" Text="HTML Decode" runat="server" OnClick="Button1_Click" />
                    <br /><br />
                    <asp:Button ID="Button3" Text="Base64 Decode" runat="server" OnClick="Button3_Click" />
                </td>
                <td valign="top">
                    <asp:Button ID="btnURLEncode" Text="URL Encode" runat="server" OnClick="btnURLEncode_Click" />
                    <br /><br />
                    <asp:Button ID="Button2" Text="HTML Encode" runat="server" OnClick="Button2_Click" />
                    <br /><br />
                    <asp:Button ID="Button4" Text="Base64 Encode" runat="server" OnClick="Button4_Click" />
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
               </td>
               <td colspan="2" valign="bottom">
                    <a href="javascript:ClearTxt()">Clear text</a>&nbsp;&nbsp;<a href="javascript:Copy2Clipboard()">Copy2Clipboard</a> <input type="checkbox" id="cbClear" checked="checked" > Clear text?
                    <br /><br />Beautify &nbsp;&nbsp;<a href="javascript:prettyPrintXML()">XML</a> / <a href="javascript:beautifyJSON()">JSON</a> <input type="checkbox" id="cbInTxt" checked="checked" /> in textarea?
               </td>
            </tr>
        </table>
        
        
    </div>
    <br />
    <div id="pretty" style="border: 1px solid black; padding:10px"></div>
    </form>
</body>
</html>
