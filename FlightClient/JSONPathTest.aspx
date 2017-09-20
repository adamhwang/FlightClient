<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JSONPathTest.aspx.cs" Inherits="FlightClient.JSONPathTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script language="javascript" type="text/javascript">
        function beautifyJSON(x) {
            try {
                var txt = document.getElementById(x);
                if (txt) {
                    var jsonContent = txt.innerHTML;
                    var jsonBeautified = JSON.stringify(JSON.parse(jsonContent), null, 2);
                    txt.innerHTML = jsonBeautified;
                }
            }
            catch(ex)
            {
                alert('Unable to beautify. Invalid JSON!');
            }
        }
   </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table>
    <tr>
        <td><asp:TextBox ID="tbReq" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
        <td valign="top">
            <br /><br /><br /><a href="javascript:beautifyJSON('<%=tbReq.ClientID %>')">Beautify this</a>
        </td>
       
    </tr>
    <tr>
        <td><asp:TextBox ID="tbCmd" runat="server" TextMode="MultiLine" Columns="100" Rows="5" /></td> 
        <td valign="top">
            <asp:CheckBox ID="cbBeautify" runat="server" Text="Beautified result" />
            <br /><asp:Button ID="btmJSONPathCmd" runat="server" Text="Check JsonPathCmd" OnClick="btmJSONPathCmd_Click" />
        </td>
    </tr>    
    <tr>
        <td><asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
        <td valign="top"><br />
            <br /><br /><a href="javascript:beautifyJSON('<%=tbRes.ClientID %>')">Beautify this</a>
        </td>
    </tr>
    </table>
    </div>
    </form>
</body>
</html>
