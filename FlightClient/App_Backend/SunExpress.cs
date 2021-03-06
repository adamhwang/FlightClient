﻿#define useCertLoc
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.IO;
using System.Text;

using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using System.Xml;

using EAScrape;

namespace SunExpress
{
    /*      22-11-2016  : JvL has created this class

    */
    public class SunExpressMain
    {
        public SunExpressMain()
        {
            System.Net.ServicePointManager.CertificatePolicy = new WeGoLoPolicy();
        }

        public string GoNext(NameValueCollection poScrapeData, ref ScrapeInfo pScrapeInfo, ref iFou pFoundInfo, WebPage poWebPage)
        {
            try
            {
                switch (poScrapeData["action"])
                {
                    case "EncryptItem": return EncryptItem(pScrapeInfo);
                    case "EncryptCC": return EncryptCC(pScrapeInfo);
                    case "DecryptItem": return DecryptItem(pScrapeInfo);

                    case "GetFlights": return GetFlights(pScrapeInfo);
                    case "GetRRCombis": return GetCombis(pScrapeInfo);

                    default: return "";
                }
                return "";
            }
            catch (System.Threading.ThreadAbortException)
            {
                //Thread timout: handle as CarrierError.
                pScrapeInfo.HandleThreadTimeout(pFoundInfo, this.ToString());
                throw;
            }
            catch (System.Net.WebException)
            {
                //This one will be handled in the WebPage class.
                throw;
            }
            catch (Exception leExc)
            {
                return "Unexpected Error: " + leExc.ToString();
            }
        }

        private string EncryptItem(ScrapeInfo pScrapeInfo)
        {
            StringBuilder sb = new StringBuilder();

            bool gotStr = pScrapeInfo.CheckIfVariableExists("WS_ENCRYPT_STR");
            bool gotCertLocat = pScrapeInfo.CheckIfVariableExists("WS_ENCRYPT_CER_LOC");

            sb.Append("<ENCRYPTEDINFO>");
            try
            {
                if (gotStr && gotCertLocat)
                {
                    string str = pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_STR");
                    string locat = pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_CER_LOC");

                    if (locat.ToUpper().Contains("_PYM_") || locat.ToUpper().Contains("_PYMTEST_") || locat.ToUpper().Contains("_FRMK_") || locat.ToUpper().Contains("_OVERWRITE_"))
                    //if (File.Exists(locat))
                    {
                        sb.Append(string.Format("<ITEM>{0}</ITEM>", cipherRequest(str, locat, pScrapeInfo)));
                    }
                    else
                        sb.Append("<ERROR>No proper certificate name</ERROR>");
                }
                else

                    sb.Append(string.Format("<ERROR>Not all necessary data available: {0}, {1}</ERROR>", gotStr, gotCertLocat));
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                sb.Append(string.Format("<ERROR>{0}</ERROR>", e.Message));
            }
            sb.Append("</ENCRYPTEDINFO>");

            return sb.ToString();
        }

        private string EncryptCC(ScrapeInfo pScrapeInfo)
        {
            StringBuilder sb = new StringBuilder();

            bool gotCCNr = pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_NUMBER");
            bool gotCCV = pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_VERIFICATION_NO");
            bool gotCCHolder = pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_HOLDER");
            bool gotExpMonth = pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_VALID_MONTH");
            bool gotExpYY = pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_VALID_YEAR");
            bool gotCertLocat = pScrapeInfo.CheckIfVariableExists("WS_ENCRYPT_CER_LOC");

            sb.Append("<ENCRYPTEDINFO>");
            try
            {
                if (gotCCNr && gotCCV && gotCCHolder && gotExpMonth && gotExpYY && gotCertLocat)
                {
                    string CCNR = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_NUMBER");
                    string CCV = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_VERIFICATION_NO");
                    string CCHolder = pScrapeInfo.GetScrapeInfoValueFromName("REQ_BOO_CREDITCARD_HOLDER");
                    string ExpDate = pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_VALID_MONTH") + "/" + pScrapeInfo.CheckIfVariableExists("REQ_BOO_CREDITCARD_VALID_YEAR");
                    string locat = pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_CER_LOC");

                    if (locat.ToUpper().Contains("_PYM_") || locat.ToUpper().Contains("_PYMTEST_") || locat.ToUpper().Contains("_FRMK_") || locat.ToUpper().Contains("_OVERWRITE_"))
                    //if (File.Exists(locat))
                    {
                        sb.Append(string.Format("<CCNR>{0}</CCNR>", cipherRequest(CCNR, locat, pScrapeInfo)));
                        sb.Append(string.Format("<CCV>{0}</CCV>", cipherRequest(CCV, locat, pScrapeInfo)));
                        sb.Append(string.Format("<CCHolder>{0}</CCHolder>", cipherRequest(CCHolder, locat, pScrapeInfo)));
                        sb.Append(string.Format("<ExpDate>{0}</ExpDate>", cipherRequest(ExpDate, locat, pScrapeInfo)));
                    }
                    else
                        sb.Append("<ERROR>No proper certificate name</ERROR>");

                }
                else
                    sb.Append(string.Format("<ERROR>Not all necessary data available: CCNR>{0}, CCV>{1}, CCHolder>{2}, ExpMonth>{3}, ExpYY>{4}, CertLocat>{5}</ERROR>", gotCCNr, gotCCV, gotCCHolder, gotExpMonth, gotExpYY, gotCertLocat));
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                sb.Append(string.Format("<ERROR>{0}</ERROR>", e.Message));
            }
            sb.Append("</ENCRYPTEDINFO>");


            return sb.ToString();
        }


        

        private static string cipherRequest(string stringToEncrypt, string locat, ScrapeInfo pScrapeInfo)
        {
            try
            {
                string certPubKey = string.Empty;

                //Creating the public key from the certificate
                //X509Certificate2 certificate = new X509Certificate2(locat);
                //RSACryptoServiceProvider rsaTemp = certificate.PublicKey.Key as RSACryptoServiceProvider;
                //certPubKey = certificate.PublicKey.Key.ToXmlString(false);

                //Using the public keys instead of the cetificate
                //Depending from the certificate name the public key is selected

                if (locat.ToUpper().Contains("_PYMTEST_"))
                    //aiRES_PYM_Cert.cer
                    //*** Test cert ***//
                    certPubKey = "<RSAKeyValue><Modulus>hrNq3NsJqKAUz+CG4PJNzN6QOLjUTlT5ZFCvWL+BWGIBKEvPJadUB/4CGftMgDIqC48QJlA5TekGsnj9hb1oNd7vtxMU9nr01ygduCpDeTz4kaN5Hlef/luYFsPN3rIMeExCyH6NqLccLhTdWHBt/zqSEtCVH4lqgp+BW1ouCNRw7D3eK5CqY0+XtGT3YOF9szCBFKpWjJMMQumx36sPvZDF6IHZxUE3MasfK/ihO226pK/ncgzonmA40OMCMsP3xsOB/uhX0X4bnIbrgpLytSuhU2Me8fLzlu7Ngluo0oQRmhj+vlVNjoJmJSMl1SKV83vUcPI54KWKPykU0a3bnQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                else if (locat.ToUpper().Contains("_PYM_"))
                    //*** Prod cert ***//
                    certPubKey = "<RSAKeyValue><Modulus>w5S4HLdgiR9t5APAmO6/yQLB1SfFFjnMe3i0nI1WW7dlUuCIJcvYkC94Eb4LP8g7vcbJlGvx0ZwQ/8HvIV4l7FwMMJfEC3PdscRkXXdsYcdaICkRmI/Zzsp4Ux3fSrJ/HXeRd/LbNzyuLellDU/Ul2SIIm4F1yodGjzRVBBfRCWxcCrk5GniunO49NL3jkPi3GONeK4bKr+24T++4OGs9tDy9jBDmxlLVkPXujtrTM8mJwBmfzc7VU6qU3bnP4yYjs7Q2zmPPHbzQk1mD5Y62kQ6G3kLVR1rcHjG6VO4hqcXPBl1j/D4XqRAPPGt4JbmpCA9wxceCQlMDmcEKXZnwId1p1K4AxFueJEC+TNZc4DF1LEv3xDUbpmaKygzcblZ4rXFFPJio3m9dvPBz2xHyr0pBsUMuYI5Xu/eZvQUsMFVxtz2hKnGKLEaDQMlB/DX48qY5Yaq+CDLWnHrat4fJk37VOvhqHOevSaBgviwTr0+EVqlI1JIfDORDScYfSPL</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                else if (locat.ToUpper().Contains("_FRMK_"))
                    //aiRES_FRMK_Cert.cer
                    certPubKey = "<RSAKeyValue><Modulus>rFxUl9/aGuybV3EIdudy2dWY1xB2NmBf/qdFLfa5d44xVNgLONGMB3rD+UrlzCNuLyJor3z0nf9qg50Ob2quKPlv2XhYKVu3aJl+uh2SXZ4BVwIYKIvvxTH7eKGbj3UdkrVCw9oQs/AClpK4MaHXzBe3t4YzC77DJykM2Ih27Q2bMwU2XvZhBhq6Zz2N717fPi+Md3Pyack0revPtZ+bNfSMjV+M8+poV9rBBGGZoAF1vt8hJIXyg9hZaWZzPUAdiYP1wvSgaJpoNKyL5/smFxq8sQjeEB8zpP9kfb/3gU7aRvM5YJTGhzrnEF16ADenL9+uOyBkIBFX0wKIiFSmSQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                else if (locat.ToUpper().Contains("_OVERWRITE_"))
                {
                    //Overwrite the certPubKey with given value
                    if (pScrapeInfo.CheckIfVariableExists("WS_ENCRYPT_CER_VAL") && !string.IsNullOrEmpty(pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_CER_VAL")))
                    {
                        try
                        {
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_CER_VAL"));
                            if (xdoc != null && xdoc.SelectSingleNode("RSAKeyValue/Modulus") != null)
                                certPubKey = pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_CER_VAL");
                        }
                        catch
                        {
                            return "ERROR: No proper certificate value";
                        }
                    }
                    

                }


                if (string.IsNullOrEmpty(certPubKey))
                    return "ERROR: No proper certificate name";

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(certPubKey);

                ///////////////////////////////////////////////////////////////////////////////

                //Create an asymmetric encrypted nonce using the public key 
                string rndStr = RandomString(40);
                //string rndStr = "1234567890123456789012345678901234567890";

                UTF8Encoding UTF8 = new UTF8Encoding();
                byte[] nonceByte = UTF8.GetBytes(rndStr);
                byte[] encNonceBytes = rsa.Encrypt(nonceByte, false);

                ///////////////////////////////////////////////////////////////////////////////

                //Encrypt the given text using the generated nonce
                SHA256 messageDigest = System.Security.Cryptography.SHA256.Create("SHA-256");
                byte[] key = messageDigest.ComputeHash(nonceByte);
                byte[] shortKey = new byte[16];
                Array.Copy(key, shortKey, 16);


                AesManaged tdes = new AesManaged();
                tdes.Key = shortKey;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform crypt = tdes.CreateEncryptor();

                UTF8 = new UTF8Encoding();
                byte[] plain = Encoding.UTF8.GetBytes(stringToEncrypt);
                byte[] cryptedData = crypt.TransformFinalBlock(plain, 0, plain.Length);


                ///////////////////////////////////////////////////////////////////////////////

                //Create the final string
                string delimiter = "%~~`%~~~~~~~%^**(%$#%";
                StringBuilder sb = new StringBuilder();
                sb.Append(Convert.ToBase64String(cryptedData)).Append(delimiter).Append(Convert.ToBase64String(encNonceBytes));

                return sb.ToString();
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                return string.Format("ERROR : {0}", e.Message);
            }
        }

        private static string RandomString(int Size)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            string input = "0123456789abcdefghijklmnopqrstuvwxyz";

            var chars = Enumerable.Range(0, Size).Select(x => input[rnd.Next(0, input.Length)]);
            string rndStr = new string(chars.ToArray());

            return rndStr;
        }

        private string DecryptItem(ScrapeInfo pScrapeInfo)
        {
            bool gotStr = pScrapeInfo.CheckIfVariableExists("WS_ENCRYPT_STR");
            bool gotCertLocat = pScrapeInfo.CheckIfVariableExists("WS_ENCRYPT_CER_LOC");

            if (gotStr && gotCertLocat)
            {
                string str = pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_STR");
                string locat = pScrapeInfo.GetScrapeInfoValueFromName("WS_ENCRYPT_CER_LOC");

                X509Certificate2 certificate = new X509Certificate2(locat);
                RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PublicKey.Key;

                /*
                string certPubKey = string.Empty;

                if (locat.ToUpper().Contains("_PYM_"))
                    //aiRES_PYM_Cert.cer
                    certPubKey = "<RSAKeyValue><Modulus>hrNq3NsJqKAUz+CG4PJNzN6QOLjUTlT5ZFCvWL+BWGIBKEvPJadUB/4CGftMgDIqC48QJlA5TekGsnj9hb1oNd7vtxMU9nr01ygduCpDeTz4kaN5Hlef/luYFsPN3rIMeExCyH6NqLccLhTdWHBt/zqSEtCVH4lqgp+BW1ouCNRw7D3eK5CqY0+XtGT3YOF9szCBFKpWjJMMQumx36sPvZDF6IHZxUE3MasfK/ihO226pK/ncgzonmA40OMCMsP3xsOB/uhX0X4bnIbrgpLytSuhU2Me8fLzlu7Ngluo0oQRmhj+vlVNjoJmJSMl1SKV83vUcPI54KWKPykU0a3bnQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                else if (locat.ToUpper().Contains("_FRMK_"))
                    //aiRES_FRMK_Cert.cer
                    certPubKey = "<RSAKeyValue><Modulus>rFxUl9/aGuybV3EIdudy2dWY1xB2NmBf/qdFLfa5d44xVNgLONGMB3rD+UrlzCNuLyJor3z0nf9qg50Ob2quKPlv2XhYKVu3aJl+uh2SXZ4BVwIYKIvvxTH7eKGbj3UdkrVCw9oQs/AClpK4MaHXzBe3t4YzC77DJykM2Ih27Q2bMwU2XvZhBhq6Zz2N717fPi+Md3Pyack0revPtZ+bNfSMjV+M8+poV9rBBGGZoAF1vt8hJIXyg9hZaWZzPUAdiYP1wvSgaJpoNKyL5/smFxq8sQjeEB8zpP9kfb/3gU7aRvM5YJTGhzrnEF16ADenL9+uOyBkIBFX0wKIiFSmSQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(certPubKey);
                
                
                RSAParameters RSAParams = rsa.ExportParameters(false);

                */

                string delimiter = "%~~`%~~~~~~~%^**(%$#%";

                string[] res = str.Split(new string[] { delimiter }, StringSplitOptions.None);

                string encVal = res[0];
                string encNonce = res[1];

                UTF8Encoding UTF8 = new UTF8Encoding();
                byte[] nonceByte = Convert.FromBase64String(encNonce);
                byte[] decNonceBytes = rsa.Decrypt(nonceByte, false);




                /*
                 
                byte[] plainEncVal = Convert.FromBase64String(encVal);

                SHA256 messageDigest = System.Security.Cryptography.SHA256.Create("SHA-256");
                byte[] key = messageDigest.ComputeHash(Convert.FromBase64String(encNonce));
                byte[] shortKey = new byte[16];
                Array.Copy(key, shortKey, 16);

                AesManaged tdes = new AesManaged();
                tdes.Key = shortKey;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform crypt = tdes.CreateDecryptor();
                byte[] plain = Encoding.UTF8.GetBytes(encVal);
                byte[] decryptedData = crypt.TransformFinalBlock(plain, 0, plain.Length);


                return Convert.ToBase64String(decryptedData);

                */

                /*
                using (AesManaged aes = new AesManaged())
                {
                    byte[] encrypted = Convert.FromBase64String(encVal);
                    aes.Key = RSAParams.Modulus;                                 
                    
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;


                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (ICryptoTransform decryptor = aes.CreateDecryptor())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                            {
                                // Decrypt through stream.
                                cs.Write(encrypted, 0, encrypted.Length);
                                cs.FlushFinalBlock();

                                // Remove from stream and convert to string.
                                byte[] decrypted = ms.ToArray();
                                return Encoding.UTF8.GetString(decrypted);
                            }
                        }
                    }
                }
                */

                return Encoding.UTF8.GetString(decNonceBytes);


            }
            else return "";
        }


        

        private string GetFlights(ScrapeInfo pScrapeInfo)
        {
            bool gotDepDateOut = pScrapeInfo.CheckIfVariableExists("DepDate_Out");
            bool gotFlightContent = pScrapeInfo.CheckIfVariableExists("FlightContent");

            string response = string.Empty;
            if (gotDepDateOut && gotFlightContent)
            {
                SunExpressFlights sef = new SunExpressFlights();
                sef.DepartureDateOut = pScrapeInfo.GetScrapeInfoValueFromName("DepDate_Out");
                sef.XmlResponse = pScrapeInfo.GetScrapeInfoValueFromName("FlightContent");
                sef.ORI = pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME");
                sef.DES = pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME");
                sef.MaxNrOfPax = pScrapeInfo.GetScrapeInfoInteger("SCR_NUM_PERSON");
                response = sef.getFlights();
            }
            else if (!gotDepDateOut)
                response = "<ERROR>No outbound departure date</ERROR>";
            else if (!gotFlightContent)
                response = "<ERROR>No Xml response</ERROR>";

            return response;
        }

        private string GetCombis(ScrapeInfo pScrapeInfo)
        {
            bool gotDepDateOut = pScrapeInfo.CheckIfVariableExists("DepDate_Out");
            bool gotDepDateRet = pScrapeInfo.CheckIfVariableExists("DepDate_Ret");
            bool gotFlightContent = pScrapeInfo.CheckIfVariableExists("FlightContent");

            string response = string.Empty;
            if (gotDepDateOut && gotDepDateRet && gotFlightContent)
            {
                SunExpressFlights sef = new SunExpressFlights();
                sef.DepartureDateOut = pScrapeInfo.GetScrapeInfoValueFromName("DepDate_Out");
                sef.DepartureDateRet = pScrapeInfo.GetScrapeInfoValueFromName("DepDate_Ret");
                sef.XmlResponse = pScrapeInfo.GetScrapeInfoValueFromName("FlightContent");
                sef.ORI = pScrapeInfo.GetScrapeInfoValueFromName("SCR_ORI_SHORT_NAME");
                sef.DES = pScrapeInfo.GetScrapeInfoValueFromName("SCR_DES_SHORT_NAME");
                sef.MaxNrOfPax = pScrapeInfo.GetScrapeInfoInteger("SCR_NUM_PERSON");
                response = sef.getRRCombis();
            }
            else if (!gotDepDateOut)
                response = "<ERROR>No outbound departure date</ERROR>";
            else if (!gotDepDateRet)
                response = "<ERROR>No inbound departure date</ERROR>";
            else if (!gotFlightContent)
                response = "<ERROR>No Xml response</ERROR>";

            return response;
        }
    }

    public class SunExpressFlights
    {
        private string _xmlContent = string.Empty;
        private string _depDateOut = string.Empty;
        private string _depDateRet = string.Empty;
        private string _ORI = string.Empty;
        private string _DES = string.Empty;
        private int _maxnrofpax = Int32.MinValue;

        public string XmlResponse
        {
            //Must be valid XML
            get { return _xmlContent; }
            set { _xmlContent = value; }
        }

        public string DepartureDateOut
        {
            //Date in yyyy-MM-dd format
            get { return _depDateOut; }
            set { _depDateOut = value; }
        }

        public string DepartureDateRet
        {
            //Date in yyyy-MM-dd format
            get { return _depDateRet; }
            set { _depDateRet = value; }
        }

        public string ORI
        {
            set { _ORI = value; }
        }

        public string DES
        {
            set { _DES = value; }
        }

        public int MaxNrOfPax
        {
            set { _maxnrofpax = value; }
        }

        public SunExpressFlights()
        {}

        private void RemoveItems()
        {
            string content = _xmlContent;
            string search = "xmlns=\"";

            //Remove all xmlns references
            while (content.Contains(search))
            {
                int pos = content.IndexOf(search);

                if (pos > 0)
                {
                    string pre = content.Substring(0, pos);
                    string post = content.Substring(pos + search.Length);
                    string search2 = "\"";
                    int pos2 = post.IndexOf(search2) + search2.Length;
                    post = post.Substring(pos2);
                    content = pre + post;
                }
            }

            /* 
             * remove all[x] from airlineoffers (is actually not necessary)
             * 

            search = "<AirlineOffer>[";
            while (content.Contains(search))
            {
                int pos = content.IndexOf(search) + search.Length - 1;
                if (pos > 0)
                {
                    string pre = content.Substring(0, pos);
                    string post = content.Substring(pos);
                    string search2 = "]<OfferID ";
                    int pos2 = post.IndexOf(search2) + 1;
                    post = post.Substring(pos2);
                    content = pre + post;
                }
            }

            search = "</PricedOffer>[";
            while (content.Contains(search))
            {
                int pos = content.IndexOf(search) + search.Length - 1;
                if (pos > 0)
                {
                    string pre = content.Substring(0, pos);
                    string post = content.Substring(pos);
                    string search2 = "]</AirlineOffer>";
                    int pos2 = post.IndexOf(search2) + 1;
                    post = post.Substring(pos2);
                    content = pre + post;
                }
            }
            */

            _xmlContent = content;
        }

        private bool gotAllElements(ref string ErrorMessage)
        {
            bool allOK = true;

            StringBuilder sb = new StringBuilder();
            sb.Append("<ERROR>");
            allOK = _xmlContent.Contains("<AirlineOffer>");
            if (!allOK) sb.Append("No AirlineOffers");

            if (!_xmlContent.Contains("<FlightSegment SegmentKey="))
            {
                if (sb.ToString().Contains("No")) sb.Append(" / ");
                sb.Append("No FlightSegments");
                allOK = false;
            }

            if (!_xmlContent.Contains("<Flight FlightKey="))
            {
                if (sb.ToString().Contains("No")) sb.Append(" / ");
                sb.Append("No Flights");
                allOK = false;
            }

            if (!_xmlContent.Contains("<OriginDestination OriginDestinationKey="))
            {
                if (sb.ToString().Contains("No")) sb.Append(" / ");
                sb.Append("No OriginDestinations");
                allOK = false;
            }


            sb.Append("</ERROR>");

            ErrorMessage = sb.ToString();


            return allOK;
        }

        public string getFlights()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(_xmlContent)) return "<ERROR>No Xml response</ERROR>";
            if (string.IsNullOrEmpty(_depDateOut)) return "<ERROR>No outbound departure date</ERROR>";

            string errorMess = string.Empty;
            if (!gotAllElements(ref errorMess)) return errorMess;

            sb.Append("<FLIGHTS>");
            try
            {
                RemoveItems();

                string content = _xmlContent;

                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(content);
                }
                catch
                {
                    return "<ERROR>No valid XML response</ERROR>";
                }

                XmlNodeList AirlineOfferList = xdoc.SelectNodes("//AirlineOffer");
                XmlNodeList FlightList = xdoc.SelectNodes("//FlightList/Flight");
                XmlNodeList FlightSegmentList = xdoc.SelectNodes("//FlightSegmentList/FlightSegment");
                XmlNodeList FlightReferenceList = xdoc.SelectNodes("//OriginDestination/FlightReferences");

                foreach (XmlNode airlineOffer in AirlineOfferList)
                {

                }

            }
            catch (Exception e)
            {
                return "<ERROR>" + e.Message + "</ERROR>";
            }

            sb.Append("</FLIGHTS>");

            return sb.ToString();
        }

        public string getRRCombis()
        {
            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(_xmlContent)) return "<ERROR>No Xml response</ERROR>";
            if (string.IsNullOrEmpty(_depDateOut)) return "<ERROR>No outbound departure date</ERROR>";
            if (string.IsNullOrEmpty(_depDateRet)) return "<ERROR>No inbound departure date</ERROR>";

            string errorMess = string.Empty;
            if (!gotAllElements(ref errorMess)) return errorMess;

            sb.Append("<COMBIS>");
            try
            {
                RemoveItems();

                string content = _xmlContent;

                XmlDocument xdoc = new XmlDocument();
                try
                {
                    xdoc.LoadXml(content);
                }
                catch
                {
                    return "<ERROR>No valid XML response</ERROR>";
                }

                XmlNodeList AirlineOfferList = xdoc.SelectNodes("//AirlineOffer");
                XmlNodeList FlightList = xdoc.SelectNodes("//FlightList/Flight");
                XmlNodeList FlightSegmentList = xdoc.SelectNodes("//FlightSegmentList/FlightSegment");
                XmlNodeList FlightReferenceList = xdoc.SelectNodes("//OriginDestination/FlightReferences");

                //for (int t = 0; t < FlightReferenceList.Count; t++)
                //{
                //    string depAirportOut = FlightReferenceList[t].ParentNode.SelectSingleNode("DepartureCode").InnerText;
                //    if (depAirportOut == _ORI)
                //    {
                //        string selectedFlightSegment = SelectedFlightSegment(xdoc, FlightReferenceList, t);
                //        XmlNode selectedAirlineOffer = AirlineOfferList[t];

                //        bool gotDapDateOut = selectedFlightSegment.Contains(_depDateOut);

                //        if (gotDapDateOut)
                //        {
                //            string selectedFlightSegment_Out = selectedFlightSegment;
                //            string selectedAirlineOffer_Out = selectedAirlineOffer.OuterXml;

                //            //Find all returnflight segments

                //            for (int s = t + 1; s < FlightReferenceList.Count; s++)
                //            {
                //                string depAirportRet = FlightReferenceList[s].ParentNode.SelectSingleNode("DepartureCode").InnerText;
                //                if (depAirportRet == _DES)
                //                {
                //                    selectedFlightSegment = SelectedFlightSegment(xdoc, FlightReferenceList, s);
                //                    selectedAirlineOffer = AirlineOfferList[s];

                //                    bool gotDapDateRet = selectedFlightSegment.Contains(_depDateRet);

                //                    if (gotDapDateRet)
                //                    {
                //                        string selectedFlightSegment_Ret = selectedFlightSegment;
                //                        string selectedAirlineOffer_Ret = selectedAirlineOffer.OuterXml;

                //                        sb.Append("<COMBI><OUT>");
                //                        sb.Append(selectedFlightSegment_Out);
                //                        sb.Append(selectedAirlineOffer_Out);
                //                        sb.Append("</OUT><RET>");
                //                        sb.Append(selectedFlightSegment_Ret);
                //                        sb.Append(selectedAirlineOffer_Ret);
                //                        sb.Append("</RET></COMBI>");
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                foreach (XmlNode airlineOffer in AirlineOfferList)
                {
                    XmlNodeList referencesList = airlineOffer.SelectNodes("//FlightSegmentReference");
                    int seatAvailOut = int.MinValue;
                    int seatAvailRet = int.MinValue;
                    if (referencesList != null && referencesList.Count > 1)
                    {
                        seatAvailOut = Convert.ToInt32(referencesList[0].SelectSingleNode("//Code/@SeatsLeft").InnerText);
                        seatAvailRet = Convert.ToInt32(referencesList[1].SelectSingleNode("//Code/@SeatsLeft").InnerText);
                    }
                }
            }
            catch (Exception e)
            {
                return "<ERROR>" + e.Message + "</ERROR>";
            }

            sb.Append("</COMBIS>");

            return sb.ToString();

        }
    }
}