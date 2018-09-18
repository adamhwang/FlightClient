using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using EAScrape;

namespace FlightClient.App_Backend
{
    public class PFAPI
    {
        public PFAPI()
        {

        }

        public void getFlightDetails(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo)
        {
            ElsyAPI.GetFlightDetails gfd = new ElsyAPI.GetFlightDetails();
            

        }
    }
}