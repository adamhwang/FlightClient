<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CondorTest.aspx.cs" Inherits="FlightClient.CondorTest" ValidateRequest="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
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
                </td>
            </tr>

        </table>
        
        
        <br />
        
    </div>
    </form>
</body>
</html>
