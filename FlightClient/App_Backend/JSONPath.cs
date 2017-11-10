using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FlightClient.App_Backend
{

    public class JSONPath
    {


        public static JObject parseJSON(string json)
        {
            //Parse JSON with use of offset times

            JObject tmpObj = new JObject();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    //Transpose JSON text to stream
                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(json);
                    writer.Flush();
                    stream.Position = 0;

                    //Parse stream to JObject
                    using (var streamReader = new StreamReader(stream))
                    {
                        Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(streamReader);
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        serializer.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset;
                        serializer.Culture = System.Globalization.CultureInfo.InvariantCulture;
                        tmpObj = serializer.Deserialize<JObject>(reader);
                    }
                }
                catch
                {
                    return null;
                }
            }

            return tmpObj;
        }

        public static string jSonDate(object date)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(date).Replace("\"", "").Substring(0, 10);
        }

        public enum jsonFormat { None, Indented }
        public string JSONStr;
        public string[] JSONPathCmd;

        private Newtonsoft.Json.Formatting _jsonformat;
        private jsonFormat _format;

        public jsonFormat Format
        {
            get { return _format; }
            set
            {
                _format = value;
                switch (_format)
                {
                    case jsonFormat.Indented: _jsonformat = Newtonsoft.Json.Formatting.Indented; break;
                    case jsonFormat.None: _jsonformat = Newtonsoft.Json.Formatting.None; break;
                }
            }
        }



        public JSONPath()
        {
            _jsonformat = Newtonsoft.Json.Formatting.None;
        }

        public string Result()
        {
            if (string.IsNullOrEmpty(JSONStr)) return "Invalid JSON string provided";
            if (JSONPathCmd != null && JSONPathCmd.Length > 0) { }
            else return "Invalid JSONPath command provided";
            //if (string.IsNullOrEmpty(JSONPathCmd)) return "Invalid JSONPath command provided";
            try
            {
                string _result = "No values found";

                JObject json;

                //First see if we can create a JObject
                try
                {
                    JSONStr = JSONStr.Replace("\n\r  ", "").Replace("\n\r ", "").Replace("\n\r", "").Replace("\r\n  ", "").Replace("\r\n ", "").Replace("\r\n", "");

                    if (JSONStr.StartsWith("[[{"))
                    {
                        JSONStr = "{\"items\" : "  + JSONStr + "}"; ;
                    }
                    else if (JSONStr.StartsWith("[{"))
                    {
                        JSONStr = "{\"items\":" + JSONStr + "}";
                    }

                    

                    //json = JObject.Parse(JSONStr);
                    json = parseJSON(JSONStr);
                }
                catch (Newtonsoft.Json.JsonReaderException jex)
                {
                    return string.Format("Not able to parse JSON: {0}", jex.Message);
                }

                if (json == null)
                    return "Not able to parse JSON";

                try
                {
                    string tmpRes = string.Empty;

                    foreach (string cmd in JSONPathCmd)
                    {
                        try
                        {
                            if (json.SelectTokens(cmd) != null)
                                //tmpRes += Newtonsoft.Json.JsonConvert.SerializeObject(json.SelectTokens(cmd), Newtonsoft.Json.Formatting.Indented);
                                tmpRes += Newtonsoft.Json.JsonConvert.SerializeObject(json.SelectTokens(cmd), _jsonformat);
                        }
                        catch
                        {

                            tmpRes += "[Unable to perform JSONPath command]";
                        }
                    }


                    _result = tmpRes;


                    //if (json.SelectTokens(JSONPathCmd) != null)
                    //    _result = Newtonsoft.Json.JsonConvert.SerializeObject(json.SelectTokens(JSONPathCmd));


                }
                catch
                {
                    _result = "Something went wrong";
                }

                return _result;
            }
            catch (Newtonsoft.Json.JsonReaderException jex)
            {
                return string.Format("Invalid JSON string provided: {0}", jex.Message);
            }

        }
    }
}