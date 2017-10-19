<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JSONPathTest.aspx.cs" Inherits="FlightClient.JSONPathTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="Scripts/JSONPath.js" type="text/javascript" ></script>
    <script language="javascript" type="text/javascript">
        var jsonID = '<%=tbReq.ClientID %>';
        function beautifyJSON(x) {
            jsonID = x;
            var collapse = document.getElementById('ControlsRow');

            collapse.style.display = "none";
            try {
                var txt = document.getElementById(x);
                if (txt) {
                    var jsonContent = txt.innerHTML;
                    var jsonBeautified = JSON.stringify(JSON.parse(jsonContent), null, 2);
                    txt.innerHTML = jsonBeautified;

                    collapse.style.display = "";

                    Process();
                }
            }
            catch(ex)
            {
                alert('Unable to beautify. Invalid JSON!');
            }
        }

        function goJP() {
            try {
                var txt = document.getElementById('<%=tbReq.ClientID %>');
                var cmd = document.getElementById('<%=tbCmd.ClientID %>');
                var res = document.getElementById('<%=tbRes.ClientID %>');
                var cbBeatyfy = document.getElementById('<%=cbBeautify.ClientID %>');

                

                if (txt && cmd)
                {
                    if (txt.innerHTML == '') return alert('Empty JSON');
                    if (cmd.innerHTML == '') return alert('Empty JSONPath Cmd');
                    if (!cmd.innerHTML.indexOf("$")<0) return alert('Invalid JSONPath Cmd')


                    var json;
                    try{
                        json = JSON.parse(txt.innerHTML);
                    }
                    catch(ex)
                    {
                        return alert("Invalid JSON: unable to parse JSON")
                    }

                    
                    /*************/
                    var result = jsonPath(json, cmd.innerHTML);
                    /**************/

                    
                    /*******************/
                    //var result = JSONPath(null, eval(cmd.innerHTML), json, null, null)
                    /********************/
                    res.innerHTML = JSON.stringify(result, null, 2);

                    
                }

            }
            catch (ex)
            {
                alert('Unable to perform JSONPath')
            }
        }
        // we need tabs as spaces and not CSS magin-left 
// in order to ratain format when coping and pasing the code
window.SINGLE_TAB = "  ";
window.ImgCollapsed = "images/Collapsed.gif";
window.ImgExpanded = "images/Expanded.gif";
window.QuoteKeys = true;
function $id(id){ return document.getElementById(id); }
function IsArray(obj) {
  return obj && 
          typeof obj === 'object' && 
          typeof obj.length === 'number' &&
          !(obj.propertyIsEnumerable('length'));
}
function Process(){
  SetTab();
  window.IsCollapsible = $id("CollapsibleView").checked;
  var json = $id(jsonID).value;
  var html = "";
  try{
    if(json == "") json = "\"\"";
    var obj = eval("["+json+"]");
    html = ProcessObject(obj[0], 0, false, false, false);
    $id("Canvas").innerHTML = "<PRE class='CodeContainer'>"+html+"</PRE>";
  }catch(e){
    alert("JSON is not well formated:\n"+e.message);
    $id("Canvas").innerHTML = "";
  }
}
window._dateObj = new Date();
window._regexpObj = new RegExp();
function ProcessObject(obj, indent, addComma, isArray, isPropertyContent){
  var html = "";
  var comma = (addComma) ? "<span class='Comma'>,</span> " : ""; 
  var type = typeof obj;
  var clpsHtml ="";
  if(IsArray(obj)){
    if(obj.length == 0){
      html += GetRow(indent, "<span class='ArrayBrace'>[ ]</span>"+comma, isPropertyContent);
    }else{
      clpsHtml = window.IsCollapsible ? "<span><img src=\""+window.ImgExpanded+"\" onClick=\"ExpImgClicked(this)\" /></span><span class='collapsible'>" : "";
      html += GetRow(indent, "<span class='ArrayBrace'>[</span>"+clpsHtml, isPropertyContent);
      for(var i = 0; i < obj.length; i++){
        html += ProcessObject(obj[i], indent + 1, i < (obj.length - 1), true, false);
      }
      clpsHtml = window.IsCollapsible ? "</span>" : "";
      html += GetRow(indent, clpsHtml+"<span class='ArrayBrace'>]</span>"+comma);
    }
  }else if(type == 'object'){
    if (obj == null){
        html += FormatLiteral("null", "", comma, indent, isArray, "Null");
    }else if (obj.constructor == window._dateObj.constructor) { 
        html += FormatLiteral("new Date(" + obj.getTime() + ") /*" + obj.toLocaleString()+"*/", "", comma, indent, isArray, "Date"); 
    }else if (obj.constructor == window._regexpObj.constructor) {
        html += FormatLiteral("new RegExp(" + obj + ")", "", comma, indent, isArray, "RegExp"); 
    }else{
      var numProps = 0;
      for(var prop in obj) numProps++;
      if(numProps == 0){
        html += GetRow(indent, "<span class='ObjectBrace'>{ }</span>"+comma, isPropertyContent);
      }else{
        clpsHtml = window.IsCollapsible ? "<span><img src=\""+window.ImgExpanded+"\" onClick=\"ExpImgClicked(this)\" /></span><span class='collapsible'>" : "";
        html += GetRow(indent, "<span class='ObjectBrace'>{</span>"+clpsHtml, isPropertyContent);
        var j = 0;
        for(var prop in obj){
          var quote = window.QuoteKeys ? "\"" : "";
          html += GetRow(indent + 1, "<span class='PropertyName'>"+quote+prop+quote+"</span>: "+ProcessObject(obj[prop], indent + 1, ++j < numProps, false, true));
        }
        clpsHtml = window.IsCollapsible ? "</span>" : "";
        html += GetRow(indent, clpsHtml+"<span class='ObjectBrace'>}</span>"+comma);
      }
    }
  }else if(type == 'number'){
    html += FormatLiteral(obj, "", comma, indent, isArray, "Number");
  }else if(type == 'boolean'){
    html += FormatLiteral(obj, "", comma, indent, isArray, "Boolean");
  }else if(type == 'function'){
    if (obj.constructor == window._regexpObj.constructor) {
        html += FormatLiteral("new RegExp(" + obj + ")", "", comma, indent, isArray, "RegExp"); 
    }else{
        obj = FormatFunction(indent, obj);
        html += FormatLiteral(obj, "", comma, indent, isArray, "Function");
    }
  }else if(type == 'undefined'){
    html += FormatLiteral("undefined", "", comma, indent, isArray, "Null");
  }else{
    html += FormatLiteral(obj.toString().split("\\").join("\\\\").split('"').join('\\"'), "\"", comma, indent, isArray, "String");
  }
  return html;
}
function FormatLiteral(literal, quote, comma, indent, isArray, style){
  if(typeof literal == 'string')
    literal = literal.split("<").join("&lt;").split(">").join("&gt;");
  var str = "<span class='"+style+"'>"+quote+literal+quote+comma+"</span>";
  if(isArray) str = GetRow(indent, str);
  return str;
}
function FormatFunction(indent, obj){
  var tabs = "";
  for(var i = 0; i < indent; i++) tabs += window.TAB;
  var funcStrArray = obj.toString().split("\n");
  var str = "";
  for(var i = 0; i < funcStrArray.length; i++){
    str += ((i==0)?"":tabs) + funcStrArray[i] + "\n";
  }
  return str;
}
function GetRow(indent, data, isPropertyContent){
  var tabs = "";
  for(var i = 0; i < indent && !isPropertyContent; i++) tabs += window.TAB;
  if(data != null && data.length > 0 && data.charAt(data.length-1) != "\n")
    data = data+"\n";
  return tabs+data;                       
}
function CollapsibleViewClicked(){
  $id("CollapsibleViewDetail").style.visibility = $id("CollapsibleView").checked ? "visible" : "hidden";
  Process();
}
 
function QuoteKeysClicked(){
  window.QuoteKeys = $id("QuoteKeys").checked;
  Process();
}
 
function CollapseAllClicked(){
  EnsureIsPopulated();
  TraverseChildren($id("Canvas"), function(element){
    if(element.className == 'collapsible'){
      MakeContentVisible(element, false);
    }
  }, 0);
}
function ExpandAllClicked(){
  EnsureIsPopulated();
  TraverseChildren($id("Canvas"), function(element){
    if(element.className == 'collapsible'){
      MakeContentVisible(element, true);
    }
  }, 0);
}
function MakeContentVisible(element, visible){
  var img = element.previousSibling.firstChild;
  if(!!img.tagName && img.tagName.toLowerCase() == "img"){
    element.style.display = visible ? 'inline' : 'none';
    element.previousSibling.firstChild.src = visible ? window.ImgExpanded : window.ImgCollapsed;
  }
}
function TraverseChildren(element, func, depth){
  for(var i = 0; i < element.childNodes.length; i++){
    TraverseChildren(element.childNodes[i], func, depth + 1);
  }
  func(element, depth);
}
function ExpImgClicked(img){
  var container = img.parentNode.nextSibling;
  if(!container) return;
  var disp = "none";
  var src = window.ImgCollapsed;
  if(container.style.display == "none"){
      disp = "inline";
      src = window.ImgExpanded;
  }
  container.style.display = disp;
  img.src = src;
}
function CollapseLevel(level){
  EnsureIsPopulated();
  TraverseChildren($id("Canvas"), function(element, depth){
    if(element.className == 'collapsible'){
      if(depth >= level){
        MakeContentVisible(element, false);
      }else{
        MakeContentVisible(element, true);  
      }
    }
  }, 0);
}
function TabSizeChanged(){
  Process();
}
function SetTab(){
  var select = $id("TabSize");
  window.TAB = MultiplyString(parseInt(select.options[select.selectedIndex].value), window.SINGLE_TAB);
}
function EnsureIsPopulated(){
  if(!$id("Canvas").innerHTML && !!$id(jsonID).value) Process();
}
function MultiplyString(num, str){
  var sb =[];
  for(var i = 0; i < num; i++){
    sb.push(str);
  }
  return sb.join("");
}
function SelectAllClicked(){
 
  if(!!document.selection && !!document.selection.empty) {
    document.selection.empty();
  } else if(window.getSelection) {
    var sel = window.getSelection();
    if(sel.removeAllRanges) {
      window.getSelection().removeAllRanges();
    }
  }
 
  var range = 
      (!!document.body && !!document.body.createTextRange)
          ? document.body.createTextRange()
          : document.createRange();
  
  if(!!range.selectNode)
    range.selectNode($id("Canvas"));
  else if(range.moveToElementText)
    range.moveToElementText($id("Canvas"));
  
  if(!!range.select)
    range.select($id("Canvas"));
  else
    window.getSelection().addRange(range);
}
function LinkToJson(){
  var val = $id(jsonID).value;
  val = escape(val.split('/n').join(' ').split('/r').join(' '));
  $id("InvisibleLinkUrl").value = val;
  $id("InvisibleLink").submit();
}
</script> 
<style> 
*{
	font-family: Georgia;
}
.Canvas
{
	font: 10pt Georgia;
	background-color:#ECECEC;
	color:#000000;
	border:solid 1px #CECECE;
}
.ObjectBrace
{
	color:#00AA00;
	font-weight:bold;
}
.ArrayBrace
{
	color:#0033FF;
	font-weight:bold;
}
.PropertyName
{
	color:#CC0000;
	font-weight:bold;
}
.String
{
	color:#007777;
}
.Number
{
	color:#AA00AA;
}
.Boolean
{
  color:#0000FF;
}
.Function
{
  color:#AA6633;
  text-decoration:italic;
}
.Null
{
  color:#0000FF;
}
.Comma
{
  color:#000000;
  font-weight:bold;
}
PRE.CodeContainer{
  margin-top:0px;
  margin-bottom:0px;
}
PRE.CodeContainer img{
  cursor:pointer;
  border:none;
  margin-bottom:-1px;
}
#CollapsibleViewDetail a{
  padding-left:10px;
}
#ControlsRow{
  white-space:nowrap;
  font: 11px Georgia;
}
#TabSizeHolder{
  padding-left:10px;
  padding-right:10px;
}
#HeaderTitle{
  text-align:right;
  font-size:11px;
}
#HeaderSubTitle{
  margin-bottom:2px;
  margin-top:0px
}
#RawJson{
  width:99%;
  height:130px;
}
A.OtherToolsLink { color:#555;text-decoration:none; }
A.OtherToolsLink:hover { text-decoration:underline; }
</style> 

</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table>
        <tr>
            <td valign="top">
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
                        <!--
                        <br /><br /><a href="javascript:goJP()">JS JSONPath Check</a>
                        -->
                    </td>
                </tr>    
                <tr>
                    <td><asp:TextBox ID="tbRes" runat="server" TextMode="MultiLine" Columns="100" Rows="25" /></td>
                    <td valign="top"><br />
                        <br /><br /><a href="javascript:beautifyJSON('<%=tbRes.ClientID %>')">Beautify this</a>
                    </td>
                </tr>
                </table>
            </td>    
             <td valign="top">
               <div id="ControlsRow" style="display:none"> 
                  <input type="Button" value="Format" onClick="Process()"/> 
                  <span id="TabSizeHolder"> 
                    tab size: 
                    <select id="TabSize" onChange="TabSizeChanged()"> 
                      <option value="1">1</option> 
                      <option value="2">2</option> 
                      <option value="3" selected="true">3</option> 
                      <option value="4">4</option> 
                      <option value="5">5</option> 
                      <option value="6">6</option> 
                    </select> 
                  </span> 
                  <label for="QuoteKeys"> 
                    <input type="checkbox" id="QuoteKeys" onClick="QuoteKeysClicked()" checked="true" /> 
                    Keys in Quotes
                  </label>&nbsp; 
                  <a href="javascript:void(0);" onClick="SelectAllClicked()">select all</a> 
                  &nbsp;
                  <span id="CollapsibleViewHolder" > 
                      <label for="CollapsibleView"> 
                        <input type="checkbox" id="CollapsibleView" onClick="CollapsibleViewClicked()" checked="true" /> 
                        Collapsible View
                      </label> 
                  </span> 
                  <span id="CollapsibleViewDetail"> 
                    <a href="javascript:void(0);" onClick="ExpandAllClicked()">expand all</a> 
                    <a href="javascript:void(0);" onClick="CollapseAllClicked()">collapse all</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(3)">level 2+</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(4)">level 3+</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(5)">level 4+</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(6)">level 5+</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(7)">level 6+</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(8)">level 7+</a> 
                    <a href="javascript:void(0);" onClick="CollapseLevel(9)">level 8+</a> 
                  </span> 
                </div> 
               <div id="Canvas" class="Canvas"></div> 
            </td>
        </tr>
    </table>        
    </div>
    </form>
</body>
</html>
