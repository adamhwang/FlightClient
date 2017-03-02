using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmadeusDll.v1
{
    public class Amadeus
    {
        private AmadeusConfig _AmadeusConfig;
        private string _json;

        public string JSON
        {
            get { return this._json; }
            set { _json = value; }
        }

        public Amadeus()
        {

        }

        public AmadeusConfig AmadeusConfigItem
        {
            get
            {
                if (this._AmadeusConfig != null)
                {
                    return this._AmadeusConfig;
                }

                try
                {
                    if (!String.IsNullOrEmpty(_json))
                    {
                        this._AmadeusConfig = (AmadeusConfig)ElsyArres.Global.Serializing.DeSerializeJSON2Object(typeof(AmadeusConfig), this._json);
                    }

                    if (this._AmadeusConfig == null)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    this._AmadeusConfig = new AmadeusConfig();
                }

                return this._AmadeusConfig;
            }
        }
    }

    #region AmadeusConfig

    public class OSI
    {
        public string companyId;
        public string longFreetext;
    }

    public class FP
    {
        public string accountNumber;
        public string creditCardCode;
        public string expiryDate;
        public string identification;
        public string cvData;
        public int fopSequenceNumber = 1;
    }

    public class AmadeusConfig
    {
        //public string CID;//Customer ID;
        //public string COI;//Corporate Online Identifier
        //public string BOID;//BOOKING Office ID (Use to start Booking Session)

        public List<FP> FP;//Forms Of Payment
        public List<OSI> OSI;//Carrier Kickback
        public List<string> RM;//Remarks
        public string AP;//Phone
        public string APE;//Email
        public int MHPD = 96;//Min Hours Prior Departure;

        //Added by JvL on 20170302: Using TKTL instead of TK
        public string TK;
    }

    #endregion
}