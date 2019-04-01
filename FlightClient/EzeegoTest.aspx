<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EzeegoTest.aspx.cs" Inherits="FlightClient.EzeegoTest" ValidateRequest="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Ezeego Test</title>
    <script src="Scripts/CryptoJS.js" type="text/javascript"></script>
    <script type="text/javascript">
        var iterationCount = <%=iterationCount%>;
        var keySize = <%=keySize%>;
        var passPhrase = "<%=passPhrase%>";

        function performJsEncryption()
        {
            var objIV = document.getElementById("<%= tbIV.ClientID%>");
            var objSalt = document.getElementById("<%= tbSalt.ClientID%>")
            var objData = document.getElementById("<%= tbData.ClientID%>")

            if (objIV.value == "")
                objIV.value = CryptoJS.lib.WordArray.random(128/8).toString(CryptoJS.enc.Hex);;

            if (objSalt.value == "")
                objSalt.value = CryptoJS.lib.WordArray.random(128/8).toString(CryptoJS.enc.Hex);

            if (objData.value == "")
                objData.value = document.getElementById("mainData").innerText;
            
            var aesUtil = new AesUtil(keySize, iterationCount);
            var ciphertext = aesUtil.encrypt(objSalt.value, objIV.value, passPhrase, objData.value);

            var objCypherText = document.getElementById("<%= tbCypherText.ClientID%>")
            objCypherText.value = ciphertext;

        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="mainData" style="display:none">5566257401681516|Anton Adams|03|2020|513</div>
    <div>
        <table>
            <tr>
                <td valign="top">IV</td>
                <td><asp:TextBox ID="tbIV" runat="server" Width="250px" /></td>
            </tr>
            <tr>
                <td valign="top">Salt</td>
                <td><asp:TextBox ID="tbSalt" runat="server" Width="250px" /></td>
            </tr>
            <tr>
                <td valign="top">Data</td>
                <td valign="top"><asp:TextBox ID="tbData" runat="server" TextMode="MultiLine" Width="500px" Height="200px" /></td>
            </tr>
            <tr>
                <td valign="top">CipherText</td>
                <td valign="top"><asp:TextBox ID="tbCypherText" runat="server" TextMode="MultiLine" Width="500px" Height="200px" /></td>
            </tr>
            <tr>
                <td></td>
                <td align="right"><a href="javascript:performJsEncryption()">JS Encrypt</a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<asp:Button ID="btnEnc" runat="server" Text="C# encrypt" OnClick="btnEnc_Click" Width="100px" Height="30px" /></td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
