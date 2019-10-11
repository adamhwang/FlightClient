<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BlueAirEnc.aspx.cs" Inherits="FlightClient.BlueAirEnc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Blue Air Enc</title>
   <script src="Scripts/BlueAirVE.js" type="text/javascript"></script>
    <script>
        !function (r) {
            var n = window.webpackJsonp;
            window.webpackJsonp = function (e, u, c) {
                for (var f, i, p, a = 0, l = []; a < e.length; a++)
                    i = e[a],
                        o[i] && l.push(o[i][0]),
                        o[i] = 0;
                for (f in u)
                    Object.prototype.hasOwnProperty.call(u, f) && (r[f] = u[f]);
                for (n && n(e, u, c); l.length;)
                    l.shift()();
                if (c)
                    for (a = 0; a < c.length; a++)
                        p = t(t.s = c[a]);
                return p
            }
                ;
            var e = {}
                , o = {
                    4: 0
                };
            function t(n) {
                if (e[n])
                    return e[n].exports;
                var o = e[n] = {
                    i: n,
                    l: !1,
                    exports: {}
                };
                return r[n].call(o.exports, o, o.exports, t),
                    o.l = !0,
                    o.exports
            }
            t.m = r,
                t.c = e,
                t.d = function (r, n, e) {
                    t.o(r, n) || Object.defineProperty(r, n, {
                        configurable: !1,
                        enumerable: !0,
                        get: e
                    })
                }
                ,
                t.n = function (r) {
                    var n = r && r.__esModule ? function () {
                        return r.default
                    }
                        : function () {
                            return r
                        }
                        ;
                    return t.d(n, "a", n),
                        n
                }
                ,
                t.o = function (r, n) {
                    return Object.prototype.hasOwnProperty.call(r, n)
                }
                ,
                t.p = "",
                t.oe = function (r) {
                    throw console.error(r),
                    r
                }
        }([]);
    </script>
    <script src="Scripts/BlueAirPolyFills.js" type="text/javascript"></script>
    <script src="Scripts/BlueAirStyles.js" type="text/javascript"></script>
    <script src="Scripts/BlueAirVendor.js" type="text/javascript"></script>
    <script src="Scripts/BlueAirMain.js" type="text/javascript"></script>    
</head>
<body>
    <form id="form1" runat="server">
        
            <div _ngcontent-c9="" class="checkin-cart-footer">
    
    <button _ngcontent-c9="" class="btn btn-primary" type="button" aria-label="Click to continue">
      Continue
    </button>
  </div>
        
    </form>
</body>
</html>
