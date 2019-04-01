<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeEncode.aspx.cs" Inherits="FlightClient.DeEncode" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>DeEncode</title>
    <script type="text/javascript">
        function ClearTxt()
        {
            var tbReq = document.getElementById("<%= tbReq.ClientID%>");
            var tbRes = document.getElementById("<%= tbRes.ClientID%>");

            tbReq.value = "";
            tbRes.value = "";
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
        </table>
        <asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
        <a href="javascript:ClearTxt()">Clear text</a>&nbsp;&nbsp;<a href="javascript:Copy2Clipboard()">Copy2Clipboard</a> <input type="checkbox" id="cbClear" checked="checked" />
    </div>
    </form>
</body>
</html>
