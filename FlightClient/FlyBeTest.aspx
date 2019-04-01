<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FlyBeTest.aspx.cs" Inherits="FlightClient.FlyBeTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript">
        function hex2s(c) {
            var b = ""; if (c.indexOf("0x") == 0 || c.indexOf("0X") == 0) {
                c = c.substr(2);
            }
            if (c.length % 2) {
                c += "0";
            }
            for (var a = 0; a < c.length; a += 2) {
                b += String.fromCharCode(parseInt(c.slice(a, a + 2), 16))
            }
            return b;
        }



        var blockSizeInBits; var keySizeInBits;

        function rijndaelEncrypt(b, h, f) {
            var e, j; var a = blockSizeInBits / 8; var g; if (!b || !h) {
                return;
            }
            if (h.length * 8 != keySizeInBits) {
                return;
            }
            if (f == "ECB"; g = new Array();
            }
            b = formatPlaintext(b);
            var c = new keyExpansion(h);
            for (var d = 0; d < b.length / a; d++) {
                j = b.slice(d * a, (d + 1) * a);
                if (f == "CBC") {
                    for (var e = 0; e < a; e++) {
                        j[e] ^= g[d * a + e];
                    }
                }
                g = g.concat(AESencrypt(j, c));
            }
            return g;
        }

        function Go()
        {
            var val2Enc = document.getElementById("<%=tbReq.ClientID %>").value;
            var key = document.getElementById("<%=tbKey.ClientID %>").value;

            var tbRes = document.getElementById("<%=tbRes.ClientID %>");

            tbRes.value = hex2s(key);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="tbReq" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />  
        <br />
        <asp:TextBox ID="tbKey" runat="server" Columns="100" Rows="25" Width="900px" />
        <asp:Button ID="btnEncr" runat="server" Text="Encrypt" OnClick="btnEncr_Click" />
        <a href="javascript:Go()">js encrypt</a>
        <asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
    </div>
    </form>
</body>
</html>
