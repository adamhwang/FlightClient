<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SHA.aspx.cs" Inherits="FlightClient.SHA" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <table>
            <tr>
                <td valign="top">
                    <asp:TextBox ID="tbContent" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
                </td>
                <td valign="top">
                    <asp:DropDownList ID="ddlSHAType" runat="server">
                        <asp:ListItem>Select a SHA type</asp:ListItem>
                        <asp:ListItem>SHA1</asp:ListItem>
                        <asp:ListItem>SHA256</asp:ListItem>
                        <asp:ListItem>SHA384</asp:ListItem>
                        <asp:ListItem>SHA512</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Button ID="Button1" Text="SHA Encode" runat="server" OnClick="Button1_Click" />
                    <br />
                    <asp:Label ID="lblMess" runat="server" Text="Provide a valid text to encode" Visible="false" ForeColor="Red" />
                </td>
            </tr>
            <tr>
                <td valign="top">
                     <asp:TextBox ID="tbKey" runat="server" TextMode="MultiLine" Columns="100" Rows="5" />
                </td>
                <td valign="top">
                    </td>
            </tr>

            <tr>
                <td valign="top">
                    <asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
               </td>
                <td valign="top">
                    
                </td>
            </tr>
       </table>
    </form>
</body>
</html>
