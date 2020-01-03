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
                    <asp:DropDownList ID="ddlSHAType" runat="server" OnSelectedIndexChanged="ddlSHAType_SelectedIndexChanged" AutoPostBack="true">
                        <asp:ListItem>Select a SHA type</asp:ListItem>
                        <asp:ListItem>SHA1</asp:ListItem>
                        <asp:ListItem>SHA256</asp:ListItem>
                        <asp:ListItem>SHA384</asp:ListItem>
                        <asp:ListItem>SHA512</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Button ID="Button1" Text="SHA Encode" runat="server" OnClick="Button1_Click" />
                    <br />
                    <asp:Label ID="lblMess" runat="server" Text="Provide a valid text to encode" Visible="false" ForeColor="Red" />
                    
                    <asp:RadioButtonList ID="rblSHA1List" runat="server" Visible="false" RepeatDirection="Vertical">
                        <asp:ListItem Selected="True">SHA1</asp:ListItem>
                        <asp:ListItem>HMACSHA1</asp:ListItem>
                        <asp:ListItem>SHA1Managed</asp:ListItem>
                        <asp:ListItem>SHA1CryptoServiceProvider</asp:ListItem>
                    </asp:RadioButtonList>
                     <br />
                    <asp:CheckBox ID="cbHash2Lower" runat="server" Text="Hash 2 Lower" />
                    <br />
                    <asp:CheckBox ID="cbUseFile" runat="server" Text="Use js file first" />
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
                    <asp:TextBox ID="tbEnc" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
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
        <a href="https://www.aerlingus.com//ahktqsewxjhguuxe.js">Download file</a>
    </form>
</body>
</html>
