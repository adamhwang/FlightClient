<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImageTest.aspx.cs" Inherits="FlightClient.ImageTest" ValidateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="txtBinData" runat="server" Width="400px" Height="200px" TextMode="MultiLine" ></asp:TextBox>
        &nbsp;<asp:Button ID="btnCheckData" Text="CheckData" runat="server" OnClick="btnCheckData_Click" />
        <br /><br />
        <asp:PlaceHolder ID="phImg" runat="server" Visible="false">
            <table>
                <tr>
                    <td valign="top"><asp:Image ID="imgPNG" runat="server" /></td>
                    <td valign="top"><asp:TextBox ID="tbImgTxt" width="200px" runat="server" /></td>
                </tr>
            </table>
           

        </asp:PlaceHolder>
        
    
    </div>
    </form>
</body>
</html>
