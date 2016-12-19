//#define useWebTrace
using System;
using System.Net;
using System.Xml;
using System.Data;
using System.Text;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

namespace EAScrape
{


    /// <summary>
	/// Connection to Jet2 API.
	/// </summary>
	public class Jet2ApiLocal
    {
        private static List<Jet2Standard.Country> _countries;
        private static List<Jet2Standard.BaggageType> _baggageTypes;
        private static List<Jet2Standard.CardType> _cardTypes;


        /// <summary>
        /// Constructor only ensures that all SSL certificates will be accepted.
        /// </summary>
        public Jet2ApiLocal()
        {
            //Trust Jet2's certificate (just in case).
            System.Net.ServicePointManager.CertificatePolicy = new WeGoLoPolicy();
        }


        /// <summary>
        /// Creates and sends XML to Jet2. Always includes the complete process flow for a process (i.e. just one call of the method for process, scraping process only has to analyse the final XML response or error message).
        /// </summary>
        /// <param name="pFoundInfo">FoundInfo for current scraping path.</param>
        /// <param name="poWebPage">Current WebPage.</param>
        /// <returns>Response from API or exception text.</returns>
        public string Jet2(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            try
            {
                //Note - unchanged in 20.09, we don't expect int64 prices from Jet2 anytime soon.

                //Trace. Only activate this in SE
#if useWebTrace
                WebPage.TraceWebStart(ref pScrapeInfo);
#endif

                if (pScrapeInfo.GetScrapeInfoValueFromName("getSession") == "true")
                {
                    return new Jet2Standard.Securityservice().GetSessionToken(pScrapeInfo.GetScrapeInfoValueFromName("Username"), pScrapeInfo.GetScrapeInfoValueFromName("Password")).Token;
                }

                switch (pScrapeInfo.SCR_PROCESS_NAME)
                {
                    case "GetRoutes":
                        return GetRoutes(ref pScrapeInfo);

                    case "SearchFlights":
                        //Session may be in pool.
                        pScrapeInfo.InsertVariable("SessionToken", pFoundInfo.GetValueFromVariable("SessionToken"));

                        return SearchFlightsPriceQuote(ref pScrapeInfo, poWebPage.WEB_TIMEOUT);

                    case "SearchReturnFlight":
                        //Session may be in pool.
                        pScrapeInfo.InsertVariable("SessionToken", pFoundInfo.GetValueFromVariable("SessionToken"));

                        return SearchReturnFlights(ref pScrapeInfo, poWebPage.WEB_TIMEOUT);

                    case "BookFlight":
                        return BookFlight(ref pScrapeInfo, ref pFoundInfo, poWebPage);

                    case "BookReturnFlight":
                        return BookReturnFlight(ref pScrapeInfo, ref pFoundInfo, poWebPage);

                    default:
                        return "<b>ERROR - (<font color=\"#CC0000\">" + pScrapeInfo.SCR_PROCESS_NAME + "</font>) not implemented!</b>";
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                //Thread timout: handle as CarrierError.
                pScrapeInfo.HandleThreadTimeout(pFoundInfo, this.ToString());
                throw;
            }
#if useWebTrace
            catch (System.Net.WebException)
            {
                //This one will be handled in the WebPage class.
                throw;
            }
#endif
            catch (Exception leExc)
            {
                return "Oops, API Error: " + leExc.ToString();
            }
        }


        private string GetRoutes(ref ScrapeInfo pScrapeInfo)
        {
            //-- get flightsector
            Jet2Standard.FlightSectorRequest loFlightSectorRequest = new Jet2Standard.FlightSectorRequest();

            //-- get session
            Jet2Standard.Securityservice SecurityService = new Jet2Standard.Securityservice();
            loFlightSectorRequest.Token = new Jet2Standard.Securityservice().GetSessionToken(pScrapeInfo.GetScrapeInfoValueFromName("Username"), pScrapeInfo.GetScrapeInfoValueFromName("Password")).Token;
            Jet2Standard.FlightSectorResponse loFlightSectorResponse = new Jet2Standard.ReservationService().GetFlightSectors(loFlightSectorRequest);

            //-- release session
            SecurityService.ReleaseSessionToken(loFlightSectorRequest.Token);

            //-- to xml 
            return SerializeObject(loFlightSectorResponse);
        }


        private string SearchFlightsPriceQuote(ref ScrapeInfo pScrapeInfo, int piTimeout)
        {
            //As of 19.46 we need to add a price quote for one-ways, as on some (few) routes an additional one-way charge is added.

            Jet2Standard.FlightSearchResponse loResp = SearchFlights(ref pScrapeInfo, piTimeout, "");

            if (loResp.Flights.Length == 0)
            {
                return SerializeObject(loResp);
            }

            bool lbInitBookingData = true;
            iFou loDummy = null;

            FoundInfo loFou;
            DataRow[] lrSelect;

            int flightCtr = 0;

            foreach (Jet2Standard.Flight flight in loResp.Flights)
            {
                //Caching verification pre-check... If we already know that the flight would pass the verification, we skip the quote request for the respective flight.
                if (pScrapeInfo.SCR_CAC_CHECK != "")
                {
                    if (pScrapeInfo.SCR_CAC_FOU.Rows.Count > 0)
                    {
                        //The check in the scraping process includes flight number, departue- and arrival time.
                        lrSelect = pScrapeInfo.SCR_CAC_FOU.Select("CHE = '" + flight.FlightNumber + flight.DepartureTime.ToString("HH:mm") + flight.ArrivalTime.ToString("HH:mm") + "' AND DC = 0");

                        if (lrSelect.Length == 1)
                        {
                            //Check adult fare.
                            loFou = new FoundInfo();
                            loFou.FOU_VARIABLES = lrSelect[0]["VAR"].ToString();
                            loFou.FOU_VALUES = lrSelect[0]["VAL"].ToString();

                            if (flight.BaseFare.AdultFare.ToString("F").Replace(".", "").Replace(",", "") + flight.CurrencyCode == loFou.GetValueFromVariable("price adults_OriginAmount") + loFou.GetValueFromVariable("OriginCurrency"))
                            {
                                //Adult fare unchanged, no need to do the quote request. The actual handling for the caching verification will be made in the RSI as ususal!
                                continue;
                            }
                        }
                    }
                }

                //No reuse possible, get a price quote.
                if (lbInitBookingData)
                {
                    //Booking object needs simulated pax data...
                    pScrapeInfo.SimulatePax();

                    pScrapeInfo.InsertVariable("REQ_BOO_CONTACT_COUNTRY", "Netherlands", true);

                    for (int i = 1; i <= pScrapeInfo.ADT + pScrapeInfo.CHD; i++)
                    {
                        pScrapeInfo.InsertVariable("REQ_BOO_AC_BAG" + i.ToString(), "1");
                    }

                    //... cc data...
                    pScrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VERIFICATION_NO", pScrapeInfo.oReq.EncryptValue("321"));
                    pScrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_CARD_NAME", "MASTERCARD");
                    pScrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_NUMBER", pScrapeInfo.oReq.EncryptValue("1000010000000007"));
                    pScrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_HOLDER", "ANTON ADAMS");
                    pScrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_MONTH", "12");
                    pScrapeInfo.InsertVariable("REQ_BOO_CREDITCARD_VALID_YEAR", (DateTime.Now.Year - 1999).ToString("00"));
                    pScrapeInfo.InsertVariable("OriginCurrency", flight.CurrencyCode);

                    //... and account data:
                    loDummy = new iFou(pScrapeInfo.lbiFouTest);
                    loDummy.InsertVariable("REQ_BOO_PASSWORD", "A1589bUx77", false);
                    loDummy.InsertVariable("REQ_BOO_AUTOGENERATED_MAIL_ADDRESS", "antonadams@hetnet.nl", false);

                    lbInitBookingData = false;
                }

                List<Jet2Standard.Flight> flightList = new List<Jet2Standard.Flight>();
                flightList.Add(flight);

                Jet2Standard.Booking booking = CreateBookingObject(flightList, ref pScrapeInfo, ref loDummy);
                Jet2Standard.QuoteResponse quoteResp = Quote(booking, pScrapeInfo.GetScrapeInfoValueFromName("SessionToken"));

                Jet2Standard.FlightReservation[] frList = quoteResp.Booking.FlightReservations;

                foreach (Jet2Standard.FlightReservation fr in frList)
                {
                    if (fr.Flight.FlightId.Equals(flight.FlightId))
                    {
                        loResp.Flights[flightCtr] = fr.Flight;
                        break;
                    }
                }

                flightCtr++;
            }

            return SerializeObject(loResp);
        }


        /// <summary>
        /// SearchFlights
        /// </summary>
        /// <returns></returns>
        public Jet2Standard.FlightSearchResponse SearchFlights(ref ScrapeInfo pScrapeInfo, int piTimeout, string psFlightType)
        {
            //-- request obj
            Jet2Standard.BasicFlightSearchRequest loBasicFlightSearchRequest = new Jet2Standard.BasicFlightSearchRequest();

            //-- request data
            if (psFlightType == "_Return")
            {
                loBasicFlightSearchRequest.DepartureAirportCode = pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME");
                loBasicFlightSearchRequest.ArrivalAirportCode = pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME");
            }
            else
            {
                loBasicFlightSearchRequest.DepartureAirportCode = pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME");
                loBasicFlightSearchRequest.ArrivalAirportCode = pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME");
            }

            loBasicFlightSearchRequest.SearchDate = new DateTime(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR" + psFlightType), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH" + psFlightType), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY" + psFlightType));
            loBasicFlightSearchRequest.NumberOfAdults = pScrapeInfo.ADT;
            loBasicFlightSearchRequest.NumberOfChildren = pScrapeInfo.CHD;
            loBasicFlightSearchRequest.NumberOfInfants = pScrapeInfo.INF;

            loBasicFlightSearchRequest.CheckInType = Jet2Standard.CheckinType.Online;
            loBasicFlightSearchRequest.CheckInTypeSpecified = true;

            loBasicFlightSearchRequest.NumberOfCarryOnRequired = 0;
            loBasicFlightSearchRequest.NumberOfCarryOnRequiredSpecified = false;

            //In SF/SRF we always search for 1 bag per passenger, for BF/BRF we have to assign exact values.
            if (pScrapeInfo.lbIsBooking)
            {
                loBasicFlightSearchRequest.NumberOfHoldBagsPerPassenger = DetermineNumberOfHoldBagsPerPassenger(ref pScrapeInfo);
            }
            else
            {
                loBasicFlightSearchRequest.NumberOfHoldBagsPerPassenger = 1;
            }

            loBasicFlightSearchRequest.NumberOfHoldBagsPerPassengerSpecified = true;

            //Set currency.
            loBasicFlightSearchRequest.CurrencyCode = pScrapeInfo.GetScrapeInfoValueFromName("OriginCurrency");

            //-- get session, reuse from session pool if applicable
            loBasicFlightSearchRequest.Token = pScrapeInfo.GetScrapeInfoValueFromName("SessionToken");

            if (loBasicFlightSearchRequest.Token == "")
            {
                loBasicFlightSearchRequest.Token = new Jet2Standard.Securityservice().GetSessionToken(pScrapeInfo.GetScrapeInfoValueFromName("Username"), pScrapeInfo.GetScrapeInfoValueFromName("Password")).Token;

                //Remember Session for e.g. BF
                pScrapeInfo.InsertVariable("SessionToken", loBasicFlightSearchRequest.Token, true);
            }

            //Set request timeout.
            Jet2Standard.ReservationService loResa = new Jet2Standard.ReservationService();
            loResa.Timeout = piTimeout;

            //-- perform request
            return loResa.GetBasicFlightSearch(loBasicFlightSearchRequest);
        }


        /// <summary>
        /// SearchReturnFlights
        /// </summary>
        /// <returns></returns>
        public string SearchReturnFlights(ref ScrapeInfo pScrapeInfo, int piTimeout)
        {
            //First get all the outward flights
            Jet2Standard.FlightSearchResponse loOutward = SearchFlights(ref pScrapeInfo, piTimeout / 2, "_Outward");

            //Then get the flight with the Flight Date, Flight Number, Departure time, Arrival time

            //Use the outward date / outward data
            DateTime departureDate = new DateTime(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Outward"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH_Outward"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY_Outward"));

            bool foundOutward = false;
            Jet2Standard.Flight selectedOutwardFlight = new Jet2Standard.Flight();
            foreach (Jet2Standard.Flight flight in loOutward.Flights)
            {
                if (flight.DepartureTime.Date == departureDate
                    && flight.DepartureTime.ToString("HH:mm").Equals(pScrapeInfo.GetScrapeInfoValueFromName("departure time_Outward"))
                    && flight.ArrivalTime.ToString("HH:mm").Equals(pScrapeInfo.GetScrapeInfoValueFromName("arrival time_Outward"))
                    && flight.FlightNumber.Equals(pScrapeInfo.GetScrapeInfoValueFromName("flight number_Outward")))
                {
                    selectedOutwardFlight = flight;
                    foundOutward = true;
                    break;
                }
            }

            //Then we get all the return flights
            Jet2Standard.FlightSearchResponse loReturn = SearchFlights(ref pScrapeInfo, piTimeout / 2, "_Return");

            //Use the return date / return data
            departureDate = new DateTime(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Return"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH_Return"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY_Return"));

            bool foundReturn = false;
            Jet2Standard.Flight selectedReturnFlight = new Jet2Standard.Flight();
            foreach (Jet2Standard.Flight flight in loReturn.Flights)
            {
                if (flight.DepartureTime.Date == departureDate
                    && flight.DepartureTime.ToString("HH:mm").Equals(pScrapeInfo.GetScrapeInfoValueFromName("departure time_Return"))
                    && flight.ArrivalTime.ToString("HH:mm").Equals(pScrapeInfo.GetScrapeInfoValueFromName("arrival time_Return"))
                    && flight.FlightNumber.Equals(pScrapeInfo.GetScrapeInfoValueFromName("flight number_Return")))
                {
                    selectedReturnFlight = flight;
                    foundReturn = true;
                    break;
                }
            }

            if (foundOutward && foundReturn)
            {
                StringBuilder flightsSelected = new StringBuilder("<Flights>");
                flightsSelected.Append(SerializeObject(selectedOutwardFlight));
                flightsSelected.Append(SerializeObject(selectedReturnFlight));

                double price = Convert.ToDouble(selectedOutwardFlight.FareBreakdown.TotalFare, new CultureInfo("de-DE")) + Convert.ToDouble(selectedReturnFlight.FareBreakdown.TotalFare, new CultureInfo("de-DE"));

                flightsSelected.Append("<TotalCost>" + String.Format("{0:0.00}", price) + "</TotalCost>");

                flightsSelected.Append("</Flights>");

                return flightsSelected.ToString();
            }
            else
            {
                return "<Error><Message>No matching flights found.</Message></Error>\r\n\r\n" + SerializeObject(loOutward) + "\r\n\r\n" + SerializeObject(loReturn);
            }
        }


        private void CheckForCarrierError(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, string psPageContent, WebPage poWebPage)
        {
            CERCarrierError loCER = new CERCarrierError();
            loCER.ScanPageForCarrierErrors(pScrapeInfo, ref pFoundInfo, ref psPageContent, poWebPage);
        }


        /// <summary>
        /// BookFlight
        /// </summary>
        /// <returns></returns>
        public string BookFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            //Search flight.
            Jet2Standard.FlightSearchResponse loFlightResult = SearchFlights(ref pScrapeInfo, (poWebPage.WEB_TIMEOUT / 10) * 2, "");

            //Response should be scanned by CarrierError class.
            CheckForCarrierError(ref pScrapeInfo, ref pFoundInfo, SerializeObject(loFlightResult), poWebPage);

            //Get the flight with the Flight Date, Flight Number, Departure time, Arrival time
            DateTime departureDate = new DateTime(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY"));
            string departureTime = pScrapeInfo.GetScrapeInfoValueFromName("departure time");
            string arrivalTime = pScrapeInfo.GetScrapeInfoValueFromName("arrival time");
            string flightNumber = pScrapeInfo.GetScrapeInfoValueFromName("flight number");

            bool found = false;
            Jet2Standard.Flight selectedFlight = new Jet2Standard.Flight();
            foreach (Jet2Standard.Flight flight in loFlightResult.Flights)
            {
                if (flight.DepartureTime.Date == departureDate
                    && flight.DepartureTime.ToString("HH:mm").Equals(departureTime)
                    && flight.ArrivalTime.ToString("HH:mm").Equals(arrivalTime)
                    && flight.FlightNumber.Equals(flightNumber))
                {
                    selectedFlight = flight;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return "<Error><Message>No matching flights found.</Message></Error>" + SerializeObject(loFlightResult);
            }

            List<Jet2Standard.Flight> flights = new List<Jet2Standard.Flight>();
            flights.Add(selectedFlight);

            //Adjusted by JvL on 20151103: Quoteresponse no longer contains sent CardDetails
            Jet2Standard.Booking bookingOriginal = CreateBookingObject(flights, ref pScrapeInfo, ref pFoundInfo);
            Jet2Standard.Booking booking = bookingOriginal;

            //Requote the fare to get the booking and card charges
            Jet2Standard.QuoteResponse quoteResponse = Quote(booking, pScrapeInfo.GetScrapeInfoValueFromName("SessionToken"));

            //PriceCheck. Note - process will be stopped if price check not passed.
            CheckPrice(ref pScrapeInfo, ref pFoundInfo, quoteResponse);

            //Adjusted by JvL on 20151103: Quoteresponse no longer contains sent CardDetails
            booking = quoteResponse.Booking;
            booking.PaymentDetails.Amount = booking.TotalAmount;
            booking.PaymentDetails.CardDetail = bookingOriginal.PaymentDetails.CardDetail;

            //Info trace for booking. Data can be removed once booking is confirmed...
            pFoundInfo.InsertVariable("PageContent BookingRequest", SerializeObject(booking), false);
            pScrapeInfo.SCR_FOU_REMOVE.Add("PageContent BookingRequest");

            //Book the flight
            Jet2Standard.BookingResponse loBookFlightResponse = Book(booking, ref pScrapeInfo, ref pFoundInfo, poWebPage);

            //sb.Append("BookResponse\n\r");
            return pFoundInfo.GetValueFromVariable("PageContent BookingRequest") + "\r\n\r\n" + SerializeObject(loBookFlightResponse);
        }


        /// <summary>
        /// BookReturnFlight
        /// </summary>
        /// <returns></returns>
        public string BookReturnFlight(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            //Search outward.
            Jet2Standard.FlightSearchResponse loOutward = SearchFlights(ref pScrapeInfo, (poWebPage.WEB_TIMEOUT / 10) * 2, "_Outward");

            //Response should be scanned by CarrierError class.
            CheckForCarrierError(ref pScrapeInfo, ref pFoundInfo, SerializeObject(loOutward), poWebPage);

            //Get the flight with the Flight Date, Flight Number, Departure time, Arrival time
            DateTime departureDate = new DateTime(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Outward"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH_Outward"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY_Outward"));
            string departureTime = pScrapeInfo.GetScrapeInfoValueFromName("departure time_Outward");
            string arrivalTime = pScrapeInfo.GetScrapeInfoValueFromName("arrival time_Outward");
            string flightNumber = pScrapeInfo.GetScrapeInfoValueFromName("flight number_Outward");

            bool found = false;
            Jet2Standard.Flight selectedOutwardFlight = new Jet2Standard.Flight();
            foreach (Jet2Standard.Flight flight in loOutward.Flights)
            {
                if (flight.DepartureTime.Date == departureDate
                    && flight.DepartureTime.ToString("HH:mm").Equals(departureTime)
                    && flight.ArrivalTime.ToString("HH:mm").Equals(arrivalTime)
                    && flight.FlightNumber.Equals(flightNumber))
                {
                    selectedOutwardFlight = flight;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return "<Error><Message>No matching flights found.</Message></Error>" + SerializeObject(loOutward);
            }

            //Search inbound flight.
            Jet2Standard.FlightSearchResponse loReturn = SearchFlights(ref pScrapeInfo, (poWebPage.WEB_TIMEOUT / 10) * 2, "_Return");

            //Response should be scanned by CarrierError class.
            CheckForCarrierError(ref pScrapeInfo, ref pFoundInfo, SerializeObject(loReturn), poWebPage);

            //Get the flight with the Flight Date, Flight Number, Departure time, Arrival time
            departureDate = new DateTime(pScrapeInfo.GetScrapeInfoInteger("REQ_START_YEAR_Return"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_MONTH_Return"), pScrapeInfo.GetScrapeInfoInteger("REQ_START_DAY_Return"));
            departureTime = pScrapeInfo.GetScrapeInfoValueFromName("departure time_Return");
            arrivalTime = pScrapeInfo.GetScrapeInfoValueFromName("arrival time_Return");
            flightNumber = pScrapeInfo.GetScrapeInfoValueFromName("flight number_Return");

            found = false;
            Jet2Standard.Flight selectedReturnFlight = new Jet2Standard.Flight();
            foreach (Jet2Standard.Flight flight in loReturn.Flights)
            {
                if (flight.DepartureTime.Date == departureDate
                    && flight.DepartureTime.ToString("HH:mm").Equals(departureTime)
                    && flight.ArrivalTime.ToString("HH:mm").Equals(arrivalTime)
                    && flight.FlightNumber.Equals(flightNumber))
                {
                    selectedReturnFlight = flight;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return "<Error><Message>No matching flights found.</Message></Error>" + SerializeObject(loOutward) + SerializeObject(loReturn);
            }

            List<Jet2Standard.Flight> flights = new List<Jet2Standard.Flight>();
            flights.Add(selectedOutwardFlight);
            flights.Add(selectedReturnFlight);

            //Adjusted by JvL on 20151103: Quoteresponse no longer contains sent CardDetails
            Jet2Standard.Booking bookingOriginal = CreateBookingObject(flights, ref pScrapeInfo, ref pFoundInfo);
            Jet2Standard.Booking booking = bookingOriginal;

            //Requote the fare to get the booking and card charges
            Jet2Standard.QuoteResponse quoteResponse = Quote(booking, pScrapeInfo.GetScrapeInfoValueFromName("SessionToken"));

            //PriceCheck. Note - process will be stopped if price check not passed.
            CheckPrice(ref pScrapeInfo, ref pFoundInfo, quoteResponse);

            //Adjusted by JvL on 20151103: Quoteresponse no longer contains sent CardDetails
            booking = quoteResponse.Booking;
            booking.PaymentDetails.Amount = booking.TotalAmount;
            booking.PaymentDetails.CardDetail = bookingOriginal.PaymentDetails.CardDetail;

            //Info trace for booking. Data can be removed once booking is confirmed...
            pFoundInfo.InsertVariable("PageContent BookingRequest", SerializeObject(booking), false);
            pScrapeInfo.SCR_FOU_REMOVE.Add("PageContent BookingRequest");

            //Book the flight
            Jet2Standard.BookingResponse loBookFlightResponse = Book(booking, ref pScrapeInfo, ref pFoundInfo, poWebPage);

            //sb.Append("BookResponse\n\r");
            return pFoundInfo.GetValueFromVariable("PageContent BookingRequest") + "\r\n\r\n" + SerializeObject(loBookFlightResponse);
        }


        private void CheckPrice(ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, Jet2Standard.QuoteResponse quoteResponse)
        {
            //Execute PriceCheck.
            string lsDummy = SerializeObject(quoteResponse);

            PriceCheck loPri = new PriceCheck();
            loPri.CheckPrice(ref pScrapeInfo, ref pFoundInfo, quoteResponse.Booking.TotalAmount.ToString("F"), quoteResponse.Booking.PaymentDetails.CurrencyCode, ref lsDummy);
        }


        /// <summary>
        /// private Jet2Standard.BookingResponse: returns a book response based on a Jet2Standard.Booking object
        /// </summary>
        /// <param name="Jet2Standard.Booking"></param>
        /// <returns>Jet2Standard.QuoteResponse</returns>
        private Jet2Standard.BookingResponse Book(Jet2Standard.Booking booking, ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            Jet2Standard.ReservationService loProxyStd = new Jet2Standard.ReservationService();

            Jet2Standard.BookingRequest bookRequest = new Jet2Standard.BookingRequest();
            bookRequest.Booking = booking;
            bookRequest.Token = pScrapeInfo.GetScrapeInfoValueFromName("SessionToken");

            //set timeout (note - part of it has already been used, but we assign the full timeout nevertheless)
            loProxyStd.Timeout = poWebPage.WEB_TIMEOUT;

            //In case of a mixed roundtrip we may have to wait for the 2nd segment.
            pScrapeInfo.WaitFor2ndSegment(true, "", ref pFoundInfo, poWebPage.WEB_WAIT, poWebPage.WEB_AUTO_CC);

            //CC data already used?
            pScrapeInfo.lbCcSent = true;

            Jet2Standard.BookingResponse bookResponse = loProxyStd.BookFlights(bookRequest);

            return bookResponse;
        }


        /// <summary>
        /// private helper method to create a Jet2Standard.Booking object filled with scraping info
        /// </summary>
        /// <param name="List<Jet2Standard.Flight>"></param>
        /// <returns>Jet2Standard.Booking</returns>
        private Jet2Standard.Booking CreateBookingObject(List<Jet2Standard.Flight> flights, ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo)
        {
            Jet2Standard.Booking booking = new Jet2Standard.Booking();

            //Get passenger info
            List<Jet2Standard.Passenger> passengers = GetPassengersFromScrapeInfo(ref pScrapeInfo);

            _countries = GetCountries(pScrapeInfo.GetScrapeInfoValueFromName("SessionToken"));
            _baggageTypes = GetBaggageTypes(pScrapeInfo.GetScrapeInfoValueFromName("SessionToken"));
            _cardTypes = GetCardTypes(pScrapeInfo.GetScrapeInfoValueFromName("SessionToken"));

            //Create flight reservation(s)
            List<Jet2Standard.FlightReservation> flightReservations = new List<Jet2Standard.FlightReservation>();

            foreach (Jet2Standard.Flight flight in flights)
            {
                Jet2Standard.FlightReservation fr = new Jet2Standard.FlightReservation();
                fr.Flight = flight;

                List<Jet2Standard.FlightReservationPassenger> frpList = new List<Jet2Standard.FlightReservationPassenger>();

                int passngerCtr = 0;
                int nrOfBagsPerPax;
                foreach (Jet2Standard.Passenger passenger in passengers)
                {
                    passngerCtr++;

                    Jet2Standard.FlightReservationPassenger frp = new Jet2Standard.FlightReservationPassenger();
                    frp.PassengerRPH = passngerCtr;
                    frp.PassengerRPHSpecified = true;
                    frp.CheckinType = Jet2Standard.CheckinType.Online;
                    frp.CheckinTypeSpecified = true;
                    frp.HasCheckedIn = false;
                    frp.HasCheckedInSpecified = true;

                    //Assign Infant to adult
                    if (passenger.Type.Equals(Jet2Standard.PassengerType.Infant))
                    {
                        for (int i = 0; i < passengers.Count; i++)
                        {
                            if (passengers[i].Type.Equals(Jet2Standard.PassengerType.Adult) && frpList[i].DependantPassengerRPH != i + 1)
                            {
                                frp.DependantPassengerRPH = i + 1;
                                frp.DependantPassengerRPHSpecified = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        //Assign baggage to AC
                        nrOfBagsPerPax = pScrapeInfo.GetScrapeInfoInteger("REQ_BOO_AC_BAG" + passngerCtr.ToString());

                        if (nrOfBagsPerPax > 0)
                        {
                            AddBaggage(frp, FindTypeByValue<Jet2Standard.BaggageType>(_baggageTypes, "HoldBaggage"), nrOfBagsPerPax);
                        }
                    }

                    frpList.Add(frp);
                }

                fr.Passengers = frpList.ToArray();
                flightReservations.Add(fr);
            }

            //Set subscriber details
            Jet2Standard.SubscriberDetails subscriber = new Jet2Standard.SubscriberDetails();
            subscriber.Code = pFoundInfo.GetValueFromVariable("REQ_BOO_PASSWORD");
            subscriber.Name = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_FIRSTNAME") + " " + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_LASTNAME");
            subscriber.Email = pFoundInfo.GetValueFromVariable("REQ_BOO_AUTOGENERATED_MAIL_ADDRESS");
            subscriber.IsATOLBooking = false;
            subscriber.IsATOLBookingSpecified = true;

            //Set payment details
            Jet2Standard.PaymentDetails paymentDetails = new Jet2Standard.PaymentDetails();
            paymentDetails.PaymentType = Jet2Standard.PaymentType.Card;
            paymentDetails.CurrencyCode = pScrapeInfo.GetScrapeInfoValueFromName("OriginCurrency");

            Jet2Standard.CreditCardDetails creditCardDetail = new Jet2Standard.CreditCardDetails();
            creditCardDetail.HolderName = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_HOLDER");

            string cardValue = string.Empty;
            switch (pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_CARD_NAME").ToUpper())
            {
                case "VISA": cardValue = "Visa Credit"; break;
                case "MASTERCARD": cardValue = "Mastercard"; break;
                case "AMEX": cardValue = "American Express"; break;
                case "MASTERDEBIT": cardValue = "Mastercard Debit"; break;
                case "VISADEBIT": cardValue = "Visa Debit"; break;
            }
            creditCardDetail.CardType = FindTypeByValue<Jet2Standard.CardType>(_cardTypes, cardValue);

            creditCardDetail.Number = Convert.ToInt64(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_NUMBER"));

            creditCardDetail.CCV = Convert.ToInt16(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_VERIFICATION_NO"));

            creditCardDetail.NumberSpecified = true;
            creditCardDetail.CCVSpecified = true;

            creditCardDetail.ValidToMonth = Convert.ToInt16(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_VALID_MONTH"));
            creditCardDetail.ValidToMonthSpecified = true;

            creditCardDetail.ValidToYear = Convert.ToInt16("20" + pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_VALID_YEAR"));
            creditCardDetail.ValidToYearSpecified = true;

            paymentDetails.CardDetail = creditCardDetail;

            Jet2Standard.Address address = new Jet2Standard.Address();
            //HouseNameNumber is obliged; so if no houseNameNumber is available we add '- House' by default
            string houseNameNumber = string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_HOUSENO")) ? pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_STREET_LINE2") : pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_HOUSENO");
            address.HouseNameNumber = string.IsNullOrEmpty(houseNameNumber) ? " - House" : houseNameNumber;

            address.StreetAddress = string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_STREET")) ? pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_STREET_LINE1") : pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_STREET");
            address.PostCode = string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_ZIP")) ? pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_ZIP") : pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_ZIP");
            address.City = string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_TOWN")) ? pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_TOWN") : pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_BILLING_TOWN");
            address.Country = FindTypeByValue<Jet2Standard.Country>(_countries, pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_COUNTRY"));

            paymentDetails.BillingAddress = address;

            //Add all items to booking object
            booking.FlightReservations = flightReservations.ToArray();
            booking.Passengers = passengers.ToArray();
            booking.SubscriberDetails = subscriber;
            booking.PaymentDetails = paymentDetails;

            return booking;
        }


        /// <summary>
        /// private Jet2Standard.QuoteResponse: returns a quote response based on a Jet2Standard.Booking object
        /// </summary>
        /// <param name="Jet2Standard.Booking"></param>
        /// <returns>Jet2Standard.QuoteResponse</returns>
        private Jet2Standard.QuoteResponse Quote(Jet2Standard.Booking booking, string psSessionToken)
        {

            Jet2Standard.ReservationService loProxyStd = new Jet2Standard.ReservationService();

            Jet2Standard.QuoteRequest quoteRequest = new Jet2Standard.QuoteRequest();
            quoteRequest.Booking = booking;
            quoteRequest.Token = psSessionToken;

            Jet2Standard.QuoteResponse quoteResponse = loProxyStd.QuoteBooking(quoteRequest);

            return quoteResponse;

        }


        private List<Jet2Standard.Passenger> GetPassengersFromScrapeInfo(ref ScrapeInfo pScrapeInfo)
        {
            List<Jet2Standard.Passenger> loPassengers = new List<Jet2Standard.Passenger>();

            //-- get data for adt, chd, inf
            int ctr = 1;
            for (int i = 1; i <= pScrapeInfo.ADT; i++)
                loPassengers.Add(GetPassengerFromScrapeInfoByTypeAndNumber(Jet2Standard.PassengerType.Adult, i.ToString(), ctr++, ref pScrapeInfo));

            for (int i = 1; i <= pScrapeInfo.CHD; i++)
                loPassengers.Add(GetPassengerFromScrapeInfoByTypeAndNumber(Jet2Standard.PassengerType.Child, i.ToString(), ctr++, ref pScrapeInfo));

            for (int i = 1; i <= pScrapeInfo.INF; i++)
                loPassengers.Add(GetPassengerFromScrapeInfoByTypeAndNumber(Jet2Standard.PassengerType.Infant, i.ToString(), ctr++, ref pScrapeInfo));

            return loPassengers;
        }


        /// <summary>
        /// private helper method to fill passenger object with scrapeinfo passenger data
        /// </summary>
        /// <param name="Jet2Standard.PassengerType pPaxtype"></param>
        /// <param name="string pNumber"></param>
        /// <returns>Jet2Standard.Passenger</returns>
        private Jet2Standard.Passenger GetPassengerFromScrapeInfoByTypeAndNumber(Jet2Standard.PassengerType pPaxtype, string pNumber, int passengerRPH, ref ScrapeInfo pScrapeInfo)
        {
            Jet2Standard.Passenger loPassenger = new Jet2Standard.Passenger();
            string VarName = "REQ_BOO_";

            Jet2Standard.Title title;

            //-- paxtype
            switch (pPaxtype)
            {
                case Jet2Standard.PassengerType.Adult:
                    VarName += "ADULT";
                    title = ((pScrapeInfo.GetScrapeInfoValueFromName(VarName + "_SEX" + pNumber) == "male")) ? Jet2Standard.Title.Mr : Jet2Standard.Title.Mrs;
                    break;
                case Jet2Standard.PassengerType.Child:
                    VarName += "CHILD";
                    title = ((pScrapeInfo.GetScrapeInfoValueFromName(VarName + "_SEX" + pNumber) == "male")) ? Jet2Standard.Title.Master : Jet2Standard.Title.Miss;
                    break;
                case Jet2Standard.PassengerType.Infant:
                    VarName += "INFANT";
                    title = ((pScrapeInfo.GetScrapeInfoValueFromName(VarName + "_SEX" + pNumber) == "male")) ? Jet2Standard.Title.Master : Jet2Standard.Title.Miss;
                    break;
                default:
                    return null;
            }

            //-- fill object
            loPassenger.DateOfBirth = new DateTime(pScrapeInfo.GetScrapeInfoInteger(VarName + "_AGE_BIRTHYEAR" + pNumber), pScrapeInfo.GetScrapeInfoInteger(VarName + "_AGE_BIRTHMONTH" + pNumber), pScrapeInfo.GetScrapeInfoInteger(VarName + "_AGE_BIRTHDAY" + pNumber));
            loPassenger.DateOfBirthSpecified = true;
            loPassenger.FirstName = pScrapeInfo.GetScrapeInfoValueFromName(VarName + "_FIRSTNAME" + pNumber);
            loPassenger.Surname = pScrapeInfo.GetScrapeInfoValueFromName(VarName + "_LASTNAME" + pNumber);
            loPassenger.Gender = (pScrapeInfo.GetScrapeInfoValueFromName(VarName + "_SEX" + pNumber) == "male") ? Jet2Standard.Gender.Male : Jet2Standard.Gender.Female; loPassenger.GenderSpecified = true;
            loPassenger.Title = title;
            loPassenger.TitleSpecified = true;
            loPassenger.Type = pPaxtype;
            loPassenger.TypeSpecified = true;
            loPassenger.PassengerRPH = passengerRPH;
            loPassenger.PassengerRPHSpecified = true;

            loPassenger.EmailAddress = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_MAIL_ADDRESS");
            loPassenger.MobileNumber = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_MOBILE");
            loPassenger.PhoneNumber = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CONTACT_PHONE");

            return loPassenger;
        }


        /// <summary>
        /// private Jet2Standard.Baggage: assigns baggage type and quantity to Jet2Standard.FlightReservationPassenger
        /// </summary>
        /// <param name="Jet2Standard.FlightReservationPassenger"></param>
        /// <param name="Jet2Standard.BaggageType"></param>
        /// <returns>Jet2Standard.Baggage</returns>
        private Jet2Standard.Baggage AddBaggage(Jet2Standard.FlightReservationPassenger frp, Jet2Standard.BaggageType type, int qty)
        {
            Jet2Standard.Baggage baggage = null;

            if (qty > 0 && frp != null)
            {
                if (frp.SpecialRequestDetails == null)
                {
                    frp.SpecialRequestDetails = new Jet2Standard.SpecialRequestDetails();
                }

                //Consolidate existing baggage with new baggage requests
                List<Jet2Standard.Baggage> bags = new List<Jet2Standard.Baggage>();
                if (frp.SpecialRequestDetails.Baggage != null)
                {
                    bags.AddRange(frp.SpecialRequestDetails.Baggage);
                    baggage = FindBaggage(frp, type);
                }

                if (baggage == null)
                {
                    baggage = new Jet2Standard.Baggage();
                    baggage.BaggageType = type;
                    baggage.Quantity = qty;
                    baggage.QuantitySpecified = true;
                    bags.Add(baggage);
                }
                else
                {
                    baggage.Quantity += qty;
                }

                frp.SpecialRequestDetails.Baggage = bags.ToArray();
            }

            return baggage;
        }


        private Jet2Standard.Baggage FindBaggage(Jet2Standard.FlightReservationPassenger frp, Jet2Standard.BaggageType type)
        {
            Jet2Standard.Baggage baggage = null;

            if (frp != null &&
                frp.SpecialRequestDetails != null &&
                frp.SpecialRequestDetails.Baggage != null &&
                type != null
            )
            {

                foreach (Jet2Standard.Baggage bag in frp.SpecialRequestDetails.Baggage)
                {
                    if (bag.BaggageType.Key.ToLower().Equals(type.Key.ToLower()))
                    {
                        baggage = bag;
                        break;
                    }
                }
            }
            return baggage;
        }


        /// <summary>
        /// private helper method to create a baggage type list
        /// </summary>
        /// <returns>List<Jet2Standard.BaggageType></returns>
        private List<Jet2Standard.BaggageType> GetBaggageTypes(string psSessionToken)
        {
            if (_baggageTypes != null && _baggageTypes.Count > 0)
                return _baggageTypes;
            else
            {
                List<Jet2Standard.BaggageType> baggageTypes = new List<Jet2Standard.BaggageType>();

                Jet2Standard.ReservationService loProxyStd = new Jet2Standard.ReservationService();
                Jet2Standard.BaggageTypeRequest req = new Jet2Standard.BaggageTypeRequest();
                req.Token = psSessionToken;
                Jet2Standard.BaggageTypeResponse loBaggageTypeResponse = loProxyStd.GetBaggageTypes(req);

                if (loBaggageTypeResponse.BaggageTypes != null)
                {
                    foreach (Jet2Standard.BaggageType loBaggageType in loBaggageTypeResponse.BaggageTypes)
                        baggageTypes.Add(loBaggageType);
                }

                return baggageTypes;
            }
        }


        /// <summary>
        /// private helper method to create a country list
        /// </summary>
        /// <returns>List<Jet2Standard.Country></returns>
        private List<Jet2Standard.Country> GetCountries(string psSessionToken)
        {
            if (_countries != null && _countries.Count > 0)
                return _countries;
            else
            {

                List<Jet2Standard.Country> countries = new List<Jet2Standard.Country>();
                Jet2Standard.ReservationService loProxyStd = new Jet2Standard.ReservationService();
                Jet2Standard.CountriesRequest req = new Jet2Standard.CountriesRequest();
                req.Token = psSessionToken;
                Jet2Standard.CountriesResponse loCountriesResponse = loProxyStd.GetCountries(req);

                if (loCountriesResponse.Countries != null)
                {
                    foreach (Jet2Standard.Country loCountry in loCountriesResponse.Countries)
                        countries.Add(loCountry);
                }

                return countries;
            }
        }


        /// <summary>
        /// private helper method to create a payment/card type list
        /// </summary>
        /// <returns>List<Jet2Standard.CardType></returns>
        private List<Jet2Standard.CardType> GetCardTypes(string psSessionToken)
        {
            if (_cardTypes != null && _cardTypes.Count > 0)
                return _cardTypes;
            else
            {
                List<Jet2Standard.CardType> cardTypes = new List<Jet2Standard.CardType>();

                Jet2Standard.ReservationService loProxyStd = new Jet2Standard.ReservationService();
                Jet2Standard.CardTypeRequest req = new Jet2Standard.CardTypeRequest();
                req.Token = psSessionToken;

                Jet2Standard.CardTypeResponse loCardTypeResponse = loProxyStd.GetCardTypes(req);

                if (loCardTypeResponse.CardTypes != null)
                {
                    foreach (Jet2Standard.CardType loCardType in loCardTypeResponse.CardTypes)
                        cardTypes.Add(loCardType);
                }

                return cardTypes;
            }
        }


        /// <summary>
        /// private helper method to determine the bags per pax. Value is assigned to SearchFlights_Input.NumberOfHoldBagsPerPassenger
        /// </summary>
        private int DetermineNumberOfHoldBagsPerPassenger(ref ScrapeInfo pScrapeInfo)
        {
            //For the time being max 1 bag pp, so let's try this:
            if (pScrapeInfo.GetScrapeInfoValueFromName("SCR_NUM_BAG") == "0")
            {
                return 0;
            }

            return 1;

            /*

            //The passenger with the highest amount of bags decides for all
            //SearchFlights_Input.NumberOfHoldBagsPerPassenger = 0;
            //for (int i = 0; i < nrOfPax; i++)
            //{
            //    if (pScrapeInfo.GetScrapeInfoInteger("REQ_BOO_AC_BAG" + i.ToString()) > SearchFlights_Input.NumberOfHoldBagsPerPassenger)
            //        SearchFlights_Input.NumberOfHoldBagsPerPassenger = pScrapeInfo.GetScrapeInfoInteger("REQ_BOO_AC_BAG" + i.ToString());
            //}

            //It is decided what the average amount of bags p.p.is; if the decimal < 0.5 then the bags are rounded down
            //Otherwise if the decimal >= 0.5 then the the number is rounded up

            decimal avg = (decimal)pScrapeInfo.GetScrapeInfoInteger("SCR_NUM_BAG") / pScrapeInfo.GetScrapeInfoInteger("SCR_NUM_PERSON");
            decimal fraction = avg - (int)avg;

            return (fraction < (decimal)0.5) ? (int)avg : (int)avg + 1;
			*/
        }


        /// <summary>
        /// private helper method to serialize each given object into XML string without namespaces
        /// </summary>
        /// <param name="object"></param>
        /// <returns>string</returns>
        private string SerializeObject(object obj)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            StringBuilder writer = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            using (XmlWriter stringWriter = XmlWriter.Create(writer, settings))
            {
                serializer.Serialize(stringWriter, obj, ns);
            }
            return StripXmlNameSpaces(writer.ToString());
        }


        private static string StripXmlNameSpaces(string xml)
        {
            const string strXMLPattern = @"(p[0-9]:nil=""true"")?(\ )?xmlns(:\w+)?="".+""";
            return Regex.Replace(xml, strXMLPattern, "");
        }


        public static T FindTypeByValue<T>(List<T> types, string value) where T : Jet2Standard.KeyValuePair
        {
            foreach (T item in types)
            {
                if (item.Value.ToLower().Equals(value.ToLower()))
                    return item;
            }
            return null;
        }
    }

}