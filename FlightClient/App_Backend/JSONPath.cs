
using Newtonsoft.Json.Linq;

namespace FlightClient.App_Backend
{
    
    public class JSONPath
    {

        public string JSONStr;
        public string JSONPathCmd;

        public JSONPath()
        { }

        public string Result()
        {
            if (string.IsNullOrEmpty(JSONStr)) return "Invalid JSON string provided";
            if (string.IsNullOrEmpty(JSONPathCmd)) return "Invalid JSONPath command provided";
            try
            {
                string _result = "No values found";

                JObject json;

                //First see if we can create a JObject
                try 
                {
                    json = JObject.Parse(JSONStr);
                }
                catch
                {
                    return "Not able to parse JSON";
                }
               
                try
                {
                    if (json.SelectTokens(JSONPathCmd) != null)
                        _result = Newtonsoft.Json.JsonConvert.SerializeObject(json.SelectTokens(JSONPathCmd));
                }
                catch
                {
                    _result = "Something went wrong";
                }

                return _result;
            }
            catch
            {
                return "Invalid JSON string provided";
            }
            
        }
    }
}