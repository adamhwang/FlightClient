﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FlyBeTest.aspx.cs" Inherits="FlightClient.FlyBeTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    
    <script type="text/javascript">
        

        if (!String.prototype.padStart) {
            String.prototype.padStart = function padStart(targetLength, padString) {
                targetLength = targetLength >> 0; //truncate if number, or convert non-number to 0;
                padString = String(typeof padString !== 'undefined' ? padString : ' ');
                if (this.length >= targetLength) {
                    return String(this);
                } else {
                    targetLength = targetLength - this.length;
                    if (targetLength > padString.length) {
                        padString += padString.repeat(targetLength / padString.length); //append to original to ensure we are longer than needed
                    }
                    return padString.slice(0, targetLength) + String(this);
                }
            };
        }

        function showTravellers() {
            var tbRes = document.getElementById("<%=tbRes.ClientID %>");
            tbRes.value = createTravellers();
        }

        function showTimeStamp() {
            var encTime = document.getElementById("<%=ENC_TIME.ClientID %>");
            encTime.value = createTimeStamp();
        }

        function showString() {
            var tbReq = document.getElementById("<%=tbReq.ClientID %>");
            tbReq.value = createString2Encrypt();
        }

        function createTimeStamp() {
            
            var d = new Date();
            //Apparently the datetimestamp FlyBe is using, is 2 hours less
            d.setHours(d.getHours() - 2);
            var timestamp = d.getFullYear().toString()
                            + String(d.getMonth() + 1).padStart(2, '0')
                            + String(d.getDate()).padStart(2, '0')
                            + String(d.getHours()).padStart(2, '0')
                            + String(d.getMinutes()).padStart(2, '0')
                            + String(d.getSeconds()).padStart(2, '0');
            

            return timestamp;
        }

        function createTravellers() {
 
            var nrOfADT = parseInt(document.getElementById("<%=ddlADT.ClientID %>").options[document.getElementById("<%=ddlADT.ClientID %>").selectedIndex].text, 10);
            var nrOfCHD = parseInt(document.getElementById("<%=ddlCHD.ClientID %>").options[document.getElementById("<%=ddlCHD.ClientID %>").selectedIndex].text, 10);
            var nrOfINF = parseInt(document.getElementById("<%=ddlINF.ClientID %>").options[document.getElementById("<%=ddlINF.ClientID %>").selectedIndex].text, 10);
            
            var travStr = "";
            var t = 0;
            for (a = 1; a <= nrOfADT; a++) {
                t++;
                travStr += "&TRAVELLER_TYPE_" + t.toString() + "=ADT";
            }
            for (c = 1; c <= nrOfCHD; c++) {
                t++;
                travStr += "&TRAVELLER_TYPE_" + t.toString() + "=CHD";
            }
            for (i = 1; i <= nrOfINF; i++) {
                travStr += "&HAS_INFANT_" + i.toString() + "=TRUE";
            }

            return travStr;
        }

        function createORIDES() {
            var ORI = document.getElementById("<%=ORI.ClientID %>").value;
            var DES = document.getElementById("<%=DES.ClientID %>").value;

            return "&B_LOCATION_1=" + ORI + "&E_LOCATION_2=" + ORI + "&E_LOCATION_1=" + DES + "&B_LOCATION_2=" + DES;
        }

        function createString2Encrypt()
        {
            var string = "ENC_TIME=" + createTimeStamp() + "&TRIP_FLOW=YES&BOOKING_FLOW=REVENUE&B_ANY_TIME_1=TRUE&B_ANY_TIME_2=TRUE&EXTERNAL_ID=BOOKING&PRICING_TYPE=O&DISPLAY_TYPE=2&ARRANGE_BY=R&COMMERCIAL_FARE_FAMILY_1=BECFF&DATE_RANGE_VALUE_1=3&DATE_RANGE_VALUE_2=3&DATE_RANGE_QUALIFIER_1=C&DATE_RANGE_QUALIFIER_2=C&EMBEDDED_TRANSACTION=FlexPricerAvailability";
            string += "&B_DATE_1=" + document.getElementById("<%=DepDate.ClientID %>").value + "&TRIP_TYPE=O";
            string += createORIDES();
            string += createTravellers();
            string += "&CONFIG_CHECKIN_URL=https://www.flybe.com/en/info-link/check-in&";

            return string;
        }

        

        /****************************** FlyBe js code copied ***************************************/

        
        var Rcon = [1, 2, 4, 8, 16, 32, 64, 128, 27, 54, 108, 216, 171, 77, 154, 47, 94, 188, 99, 198, 151, 53, 106, 212, 179, 125, 250, 239, 197, 145];
        var S = [99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22];
        var T1 = [2774754246, 2222750968, 2574743534, 2373680118, 234025727, 3177933782, 2976870366, 1422247313, 1345335392, 50397442, 2842126286, 2099981142, 436141799, 1658312629, 3870010189, 2591454956, 1170918031, 2642575903, 1086966153, 2273148410, 368769775, 3948501426, 3376891790, 200339707, 3970805057, 1742001331, 4255294047, 3937382213, 3214711843, 4154762323, 2524082916, 1539358875, 3266819957, 486407649, 2928907069, 1780885068, 1513502316, 1094664062, 49805301, 1338821763, 1546925160, 4104496465, 887481809, 150073849, 2473685474, 1943591083, 1395732834, 1058346282, 201589768, 1388824469, 1696801606, 1589887901, 672667696, 2711000631, 251987210, 3046808111, 151455502, 907153956, 2608889883, 1038279391, 652995533, 1764173646, 3451040383, 2675275242, 453576978, 2659418909, 1949051992, 773462580, 756751158, 2993581788, 3998898868, 4221608027, 4132590244, 1295727478, 1641469623, 3467883389, 2066295122, 1055122397, 1898917726, 2542044179, 4115878822, 1758581177, 0, 753790401, 1612718144, 536673507, 3367088505, 3982187446, 3194645204, 1187761037, 3653156455, 1262041458, 3729410708, 3561770136, 3898103984, 1255133061, 1808847035, 720367557, 3853167183, 385612781, 3309519750, 3612167578, 1429418854, 2491778321, 3477423498, 284817897, 100794884, 2172616702, 4031795360, 1144798328, 3131023141, 3819481163, 4082192802, 4272137053, 3225436288, 2324664069, 2912064063, 3164445985, 1211644016, 83228145, 3753688163, 3249976951, 1977277103, 1663115586, 806359072, 452984805, 250868733, 1842533055, 1288555905, 336333848, 890442534, 804056259, 3781124030, 2727843637, 3427026056, 957814574, 1472513171, 4071073621, 2189328124, 1195195770, 2892260552, 3881655738, 723065138, 2507371494, 2690670784, 2558624025, 3511635870, 2145180835, 1713513028, 2116692564, 2878378043, 2206763019, 3393603212, 703524551, 3552098411, 1007948840, 2044649127, 3797835452, 487262998, 1994120109, 1004593371, 1446130276, 1312438900, 503974420, 3679013266, 168166924, 1814307912, 3831258296, 1573044895, 1859376061, 4021070915, 2791465668, 2828112185, 2761266481, 937747667, 2339994098, 854058965, 1137232011, 1496790894, 3077402074, 2358086913, 1691735473, 3528347292, 3769215305, 3027004632, 4199962284, 133494003, 636152527, 2942657994, 2390391540, 3920539207, 403179536, 3585784431, 2289596656, 1864705354, 1915629148, 605822008, 4054230615, 3350508659, 1371981463, 602466507, 2094914977, 2624877800, 555687742, 3712699286, 3703422305, 2257292045, 2240449039, 2423288032, 1111375484, 3300242801, 2858837708, 3628615824, 84083462, 32962295, 302911004, 2741068226, 1597322602, 4183250862, 3501832553, 2441512471, 1489093017, 656219450, 3114180135, 954327513, 335083755, 3013122091, 856756514, 3144247762, 1893325225, 2307821063, 2811532339, 3063651117, 572399164, 2458355477, 552200649, 1238290055, 4283782570, 2015897680, 2061492133, 2408352771, 4171342169, 2156497161, 386731290, 3669999461, 837215959, 3326231172, 3093850320, 3275833730, 2962856233, 1999449434, 286199582, 3417354363, 4233385128, 3602627437, 974525996];
        var T2 = [1667483301, 2088564868, 2004348569, 2071721613, 4076011277, 1802229437, 1869602481, 3318059348, 808476752, 16843267, 1734856361, 724260477, 4278118169, 3621238114, 2880130534, 1987505306, 3402272581, 2189565853, 3385428288, 2105408135, 4210749205, 1499050731, 1195871945, 4042324747, 2913812972, 3570709351, 2728550397, 2947499498, 2627478463, 2762232823, 1920132246, 3233848155, 3082253762, 4261273884, 2475900334, 640044138, 909536346, 1061125697, 4160222466, 3435955023, 875849820, 2779075060, 3857043764, 4059166984, 1903288979, 3638078323, 825320019, 353708607, 67373068, 3351745874, 589514341, 3284376926, 404238376, 2526427041, 84216335, 2593796021, 117902857, 303178806, 2155879323, 3806519101, 3958099238, 656887401, 2998042573, 1970662047, 151589403, 2206408094, 741103732, 437924910, 454768173, 1852759218, 1515893998, 2694863867, 1381147894, 993752653, 3604395873, 3014884814, 690573947, 3823361342, 791633521, 2223248279, 1397991157, 3520182632, 0, 3991781676, 538984544, 4244431647, 2981198280, 1532737261, 1785386174, 3419114822, 3200149465, 960066123, 1246401758, 1280088276, 1482207464, 3486483786, 3503340395, 4025468202, 2863288293, 4227591446, 1128498885, 1296931543, 859006549, 2240090516, 1162185423, 4193904912, 33686534, 2139094657, 1347461360, 1010595908, 2678007226, 2829601763, 1364304627, 2745392638, 1077969088, 2408514954, 2459058093, 2644320700, 943222856, 4126535940, 3166462943, 3065411521, 3671764853, 555827811, 269492272, 4294960410, 4092853518, 3537026925, 3452797260, 202119188, 320022069, 3974939439, 1600110305, 2543269282, 1145342156, 387395129, 3301217111, 2812761586, 2122251394, 1027439175, 1684326572, 1566423783, 421081643, 1936975509, 1616953504, 2172721560, 1330618065, 3705447295, 572671078, 707417214, 2425371563, 2290617219, 1179028682, 4008625961, 3099093971, 336865340, 3739133817, 1583267042, 185275933, 3688607094, 3772832571, 842163286, 976909390, 168432670, 1229558491, 101059594, 606357612, 1549580516, 3267534685, 3553869166, 2896970735, 1650640038, 2442213800, 2509582756, 3840201527, 2038035083, 3890730290, 3368586051, 926379609, 1835915959, 2374828428, 3587551588, 1313774802, 2846444000, 1819072692, 1448520954, 4109693703, 3941256997, 1701169839, 2054878350, 2930657257, 134746136, 3132780501, 2021191816, 623200879, 774790258, 471611428, 2795919345, 3031724999, 3334903633, 3907570467, 3722289532, 1953818780, 522141217, 1263245021, 3183305180, 2341145990, 2324303749, 1886445712, 1044282434, 3048567236, 1718013098, 1212715224, 50529797, 4143380225, 235805714, 1633796771, 892693087, 1465364217, 3115936208, 2256934801, 3250690392, 488454695, 2661164985, 3789674808, 4177062675, 2560109491, 286335539, 1768542907, 3654920560, 2391672713, 2492740519, 2610638262, 505297954, 2273777042, 3924412704, 3469641545, 1431677695, 673730680, 3755976058, 2357986191, 2711706104, 2307459456, 218962455, 3216991706, 3873888049, 1111655622, 1751699640, 1094812355, 2576951728, 757946999, 252648977, 2964356043, 1414834428, 3149622742, 370551866];
        var T3 = [1673962851, 2096661628, 2012125559, 2079755643, 4076801522, 1809235307, 1876865391, 3314635973, 811618352, 16909057, 1741597031, 727088427, 4276558334, 3618988759, 2874009259, 1995217526, 3398387146, 2183110018, 3381215433, 2113570685, 4209972730, 1504897881, 1200539975, 4042984432, 2906778797, 3568527316, 2724199842, 2940594863, 2619588508, 2756966308, 1927583346, 3231407040, 3077948087, 4259388669, 2470293139, 642542118, 913070646, 1065238847, 4160029431, 3431157708, 879254580, 2773611685, 3855693029, 4059629809, 1910674289, 3635114968, 828527409, 355090197, 67636228, 3348452039, 591815971, 3281870531, 405809176, 2520228246, 84545285, 2586817946, 118360327, 304363026, 2149292928, 3806281186, 3956090603, 659450151, 2994720178, 1978310517, 152181513, 2199756419, 743994412, 439627290, 456535323, 1859957358, 1521806938, 2690382752, 1386542674, 997608763, 3602342358, 3011366579, 693271337, 3822927587, 794718511, 2215876484, 1403450707, 3518589137, 0, 3988860141, 541089824, 4242743292, 2977548465, 1538714971, 1792327274, 3415033547, 3194476990, 963791673, 1251270218, 1285084236, 1487988824, 3481619151, 3501943760, 4022676207, 2857362858, 4226619131, 1132905795, 1301993293, 862344499, 2232521861, 1166724933, 4192801017, 33818114, 2147385727, 1352724560, 1014514748, 2670049951, 2823545768, 1369633617, 2740846243, 1082179648, 2399505039, 2453646738, 2636233885, 946882616, 4126213365, 3160661948, 3061301686, 3668932058, 557998881, 270544912, 4293204735, 4093447923, 3535760850, 3447803085, 202904588, 321271059, 3972214764, 1606345055, 2536874647, 1149815876, 388905239, 3297990596, 2807427751, 2130477694, 1031423805, 1690872932, 1572530013, 422718233, 1944491379, 1623236704, 2165938305, 1335808335, 3701702620, 574907938, 710180394, 2419829648, 2282455944, 1183631942, 4006029806, 3094074296, 338181140, 3735517662, 1589437022, 185998603, 3685578459, 3772464096, 845436466, 980700730, 169090570, 1234361161, 101452294, 608726052, 1555620956, 3265224130, 3552407251, 2890133420, 1657054818, 2436475025, 2503058581, 3839047652, 2045938553, 3889509095, 3364570056, 929978679, 1843050349, 2365688973, 3585172693, 1318900302, 2840191145, 1826141292, 1454176854, 4109567988, 3939444202, 1707781989, 2062847610, 2923948462, 135272456, 3127891386, 2029029496, 625635109, 777810478, 473441308, 2790781350, 3027486644, 3331805638, 3905627112, 3718347997, 1961401460, 524165407, 1268178251, 3177307325, 2332919435, 2316273034, 1893765232, 1048330814, 3044132021, 1724688998, 1217452104, 50726147, 4143383030, 236720654, 1640145761, 896163637, 1471084887, 3110719673, 2249691526, 3248052417, 490350365, 2653403550, 3789109473, 4176155640, 2553000856, 287453969, 1775418217, 3651760345, 2382858638, 2486413204, 2603464347, 507257374, 2266337927, 3922272489, 3464972750, 1437269845, 676362280, 3752164063, 2349043596, 2707028129, 2299101321, 219813645, 3211123391, 3872862694, 1115997762, 1758509160, 1099088705, 2569646233, 760903469, 253628687, 2960903088, 1420360788, 3144537787, 371997206];
        var T4 = [3332727651, 4169432188, 4003034999, 4136467323, 4279104242, 3602738027, 3736170351, 2438251973, 1615867952, 33751297, 3467208551, 1451043627, 3877240574, 3043153879, 1306962859, 3969545846, 2403715786, 530416258, 2302724553, 4203183485, 4011195130, 3001768281, 2395555655, 4211863792, 1106029997, 3009926356, 1610457762, 1173008303, 599760028, 1408738468, 3835064946, 2606481600, 1975695287, 3776773629, 1034851219, 1282024998, 1817851446, 2118205247, 4110612471, 2203045068, 1750873140, 1374987685, 3509904869, 4178113009, 3801313649, 2876496088, 1649619249, 708777237, 135005188, 2505230279, 1181033251, 2640233411, 807933976, 933336726, 168756485, 800430746, 235472647, 607523346, 463175808, 3745374946, 3441880043, 1315514151, 2144187058, 3936318837, 303761673, 496927619, 1484008492, 875436570, 908925723, 3702681198, 3035519578, 1543217312, 2767606354, 1984772923, 3076642518, 2110698419, 1383803177, 3711886307, 1584475951, 328696964, 2801095507, 3110654417, 0, 3240947181, 1080041504, 3810524412, 2043195825, 3069008731, 3569248874, 2370227147, 1742323390, 1917532473, 2497595978, 2564049996, 2968016984, 2236272591, 3144405200, 3307925487, 1340451498, 3977706491, 2261074755, 2597801293, 1716859699, 294946181, 2328839493, 3910203897, 67502594, 4269899647, 2700103760, 2017737788, 632987551, 1273211048, 2733855057, 1576969123, 2160083008, 92966799, 1068339858, 566009245, 1883781176, 4043634165, 1675607228, 2009183926, 2943736538, 1113792801, 540020752, 3843751935, 4245615603, 3211645650, 2169294285, 403966988, 641012499, 3274697964, 3202441055, 899848087, 2295088196, 775493399, 2472002756, 1441965991, 4236410494, 2051489085, 3366741092, 3135724893, 841685273, 3868554099, 3231735904, 429425025, 2664517455, 2743065820, 1147544098, 1417554474, 1001099408, 193169544, 2362066502, 3341414126, 1809037496, 675025940, 2809781982, 3168951902, 371002123, 2910247899, 3678134496, 1683370546, 1951283770, 337512970, 2463844681, 201983494, 1215046692, 3101973596, 2673722050, 3178157011, 1139780780, 3299238498, 967348625, 832869781, 3543655652, 4069226873, 3576883175, 2336475336, 1851340599, 3669454189, 25988493, 2976175573, 2631028302, 1239460265, 3635702892, 2902087254, 4077384948, 3475368682, 3400492389, 4102978170, 1206496942, 270010376, 1876277946, 4035475576, 1248797989, 1550986798, 941890588, 1475454630, 1942467764, 2538718918, 3408128232, 2709315037, 3902567540, 1042358047, 2531085131, 1641856445, 226921355, 260409994, 3767562352, 2084716094, 1908716981, 3433719398, 2430093384, 100991747, 4144101110, 470945294, 3265487201, 1784624437, 2935576407, 1775286713, 395413126, 2572730817, 975641885, 666476190, 3644383713, 3943954680, 733190296, 573772049, 3535497577, 2842745305, 126455438, 866620564, 766942107, 1008868894, 361924487, 3374377449, 2269761230, 2868860245, 1350051880, 2776293343, 59739276, 1509466529, 159418761, 437718285, 1708834751, 3610371814, 2227585602, 3501746280, 2193834305, 699439513, 1517759789, 504434447, 2076946608, 2835108948, 1842789307, 742004246];
        function B0(a) {
            return (a & 255);
        }
        function B1(a) {
            return ((a >> 8) & 255);
        }
        function B2(a) {
            return ((a >> 16) & 255);
        }
        function B3(a) {
            return ((a >> 24) & 255);
        }
        function F1(d, c, b, a) {
            return B1(T1[d & 255]) | (B1(T1[(c >> 8) & 255]) << 8) | (B1(T1[(b >> 16) & 255]) << 16) | (B1(T1[a >>> 24]) << 24);
        }
        function packBytes(e) {
            var f, d;
            var c = e.length;
            var a = new Array(c / 4);
            if (!e || c % 4) {
                return;
            }
            for (f = 0,
            d = 0; d < c; d += 4) {
                a[f++] = e[d] | (e[d + 1] << 8) | (e[d + 2] << 16) | (e[d + 3] << 24);
            }
            return a;
        }
        function unpackBytes(b) {
            var c;
            var d = 0
              , a = b.length;
            var e = new Array(a * 4);
            for (c = 0; c < a; c++) {
                e[d++] = B0(b[c]);
                e[d++] = B1(b[c]);
                e[d++] = B2(b[c]);
                e[d++] = B3(b[c]);
            }
            return e;
        }
 
        function hex2s(c) {
            var b = "";
            if (c.indexOf("0x") == 0 || c.indexOf("0X") == 0) {
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
        var encryptURL = function (key, dec) {
            switch (key.length) {
                case 32:
                    blockSizeInBits = key.length * 4;
                    keySizeInBits = key.length * 4;
                    break;
                case 64:
                    blockSizeInBits = key.length * 2;
                    keySizeInBits = key.length * 4;
                    break;
                default:
                    blockSizeInBits = key.length * 2;
                    keySizeInBits = key.length * 4;
                    break;
            }
        };

        function formatPlaintext(b) {
            var c = blockSizeInBits / 8;
            var a;
            if (typeof b == "string" || b.indexOf) {
                b = b.split("");
                for (a = 0; a < b.length; a++) {
                    b[a] = b[a].charCodeAt(0) & 255;
                }
            }
            for (a = c - (b.length % c) ;
            a > 0 && a < c; a--) {
                b[b.length] = 0;
            }
            return b;
        }

        var maxkc = 8;
        var maxrk = 14;
        function keyExpansion(h) {
            var g, f, e, b, o; var n; var l = new Array(maxrk + 1);
            var a = h.length; var d = new Array(maxkc);
            var p = new Array(maxkc);
            var c = 0; if (a == 16) {
                n = 10; g = 4;
            } else {
                if (a == 24) {
                    n = 12; g = 6;
                } else {
                    if (a == 32) {
                        n = 14; g = 8;
                    } else {
                        alert("Invalid key length " + a);
                        return;
                    }
                }
            }
            for (f = 0; f < maxrk + 1; f++) {
                l[f] = new Array(4);
            }
            for (f = 0, e = 0; e < a; e++, f += 4) {
                d[e] = h.charCodeAt(f) | (h.charCodeAt(f + 1) << 8) | (h.charCodeAt(f + 2) << 16) | (h.charCodeAt(f + 3) << 24)
            }
            for (e = g - 1; e >= 0; e--) {
                p[e] = d[e];
            }
            b = 0; o = 0; for (e = 0; (e < g) && (b < n + 1) ;) {
                for (; (e < g) && (o < 4) ;
                e++, o++) {
                    l[b][o] = p[e];
                }
                if (o == 4) {
                    b++; o = 0;
                }
            }
            while (b < n + 1) {
                var m = p[g - 1]; p[0] ^= S[B1(m)] | (S[B2(m)] << 8) | (S[B3(m)] << 16) | (S[B0(m)] << 24);
                p[0] ^= Rcon[c++]; if (g != 8) {
                    for (e = 1; e < g; e++) {
                        p[e] ^= p[e - 1];
                    }
                } else {
                    for (e = 1; e < g / 2; e++) {
                        p[e] ^= p[e - 1];
                    }
                    m = p[g / 2 - 1]; p[g / 2] ^= S[B0(m)] | (S[B1(m)] << 8) | (S[B2(m)] << 16) | (S[B3(m)] << 24);
                    for (e = g / 2 + 1; e < g; e++) {
                        p[e] ^= p[e - 1];
                    }
                }
                for (e = 0; (e < g) && (b < n + 1) ;) {
                    for (; (e < g) && (o < 4) ;
                    e++, o++) {
                        l[b][o] = p[e];
                    }
                    if (o == 4) {
                        b++; o = 0;
                    }
                }
            }
            this.rounds = n; this.rk = l; return this;
        }

        function AESencrypt(c, o) {
            var a; var g, f, e, d;
            var k = packBytes(c);
            var n = o.rounds;
            var m = k[0];
            var l = k[1];
            var j = k[2];
            var h = k[3];
            for (a = 0; a < n - 1; a++) {
                g = m ^ o.rk[a][0];
                f = l ^ o.rk[a][1];
                e = j ^ o.rk[a][2];
                d = h ^ o.rk[a][3];
                m = T1[g & 255] ^ T2[(f >> 8) & 255] ^ T3[(e >> 16) & 255] ^ T4[d >>> 24];
                l = T1[f & 255] ^ T2[(e >> 8) & 255] ^ T3[(d >> 16) & 255] ^ T4[g >>> 24];
                j = T1[e & 255] ^ T2[(d >> 8) & 255] ^ T3[(g >> 16) & 255] ^ T4[f >>> 24];
                h = T1[d & 255] ^ T2[(g >> 8) & 255] ^ T3[(f >> 16) & 255] ^ T4[e >>> 24];
            }
            a = n - 1;
            g = m ^ o.rk[a][0];
            f = l ^ o.rk[a][1];
            e = j ^ o.rk[a][2];
            d = h ^ o.rk[a][3];
            k[0] = F1(g, f, e, d) ^ o.rk[n][0];
            k[1] = F1(f, e, d, g) ^ o.rk[n][1];
            k[2] = F1(e, d, g, f) ^ o.rk[n][2];
            k[3] = F1(d, g, f, e) ^ o.rk[n][3];
            return unpackBytes(k);
        }

        function byteArrayToHex(b) {
            var a = ""; if (!b) {
                return
            }
            for (var c = 0; c < b.length; c++) {
                a += ((b[c] < 16) ? "0" : "") + b[c].toString(16)
            }
            return a;
        }

        function rijndaelEncrypt(b, h, f) {
            var e, j; var a = blockSizeInBits / 8; var g; if (!b || !h) {
                return;
            }
            if (h.length * 8 != keySizeInBits) {
                return;
            }
            if (f == "CBC") {
                g = getRandomBytes(a);
            } else {
                f = "ECB"; g = new Array();
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
        
        /***************************************** End FlyBe original js copy ********************/

        function encryptLocal(valueToEncrypt) {
            var encryptionKey = document.getElementById("<%=tbKey.ClientID %>").value;
 
            encryptURL(encryptionKey, valueToEncrypt);
            var ENCRYPTED_VALUE = byteArrayToHex(rijndaelEncrypt(valueToEncrypt, hex2s(encryptionKey), "ECB")).toUpperCase();
            return ENCRYPTED_VALUE;
        }

        function Go()
        {
            var tbReq = document.getElementById("<%=tbReq.ClientID %>");

            tbReq.value = createString2Encrypt();

            var val2Enc = tbReq.value;
            
            var tbRes = document.getElementById("<%=tbRes.ClientID %>");
            
            tbRes.value = encryptLocal(val2Enc);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="tbReq" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />  
        <br />
        
        <table>
            <tr>
                <td>ENC_TIME</td><td><asp:TextBox ID="ENC_TIME" runat="server"></asp:TextBox><a href="javascript:showTimeStamp()">timestamp</a></td>
            </tr>
            <tr>
                <td colspan="2"">
                ADT 
                <asp:DropDownList ID="ddlADT" runat="server">
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                    <asp:ListItem>6</asp:ListItem>
                    <asp:ListItem>7</asp:ListItem>
                    <asp:ListItem>8</asp:ListItem>
                </asp:DropDownList>
                CHD 
                <asp:DropDownList ID="ddlCHD" runat="server">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                </asp:DropDownList>
                INF 
                <asp:DropDownList ID="ddlINF" runat="server">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                </asp:DropDownList>    
                &nbsp;<a href="javascript:showTravellers()">Travellers</a>
                </td>
            </tr>
            <tr>
                <td>DEP Date</td><td><asp:TextBox ID="DepDate" runat="server"></asp:TextBox></td>
            </tr>
            <tr>
                <td>ORI</td><td><asp:TextBox ID="ORI" runat="server"></asp:TextBox></td>
            </tr>
             <tr>
                <td>DES</td><td><asp:TextBox ID="DES" runat="server"></asp:TextBox>&nbsp;&nbsp; <a href="javascript:showString()">Create string</a></td>
            </tr>
        </table>
        <asp:TextBox ID="tbKey" runat="server" Columns="100" Rows="25" Width="800px" />
        <a href="javascript:Go()">js encrypt</a><br />
        <asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" />
    </div>
    </form>
</body>
</html>
