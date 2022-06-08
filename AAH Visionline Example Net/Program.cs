/*
 *  Visionline Web Api Connector Example .Net
 *  V1.1
 *  2017-06-08
 * 
 * */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AAH_Visionline_Example_Net
{
    // Object for holding initial values for the request
    // Ip address for visionline and some additional required values such as endpointId and key expire time.
    public class InitialValues
    {
        public InitialValues()
        {

            // Values you need to provide based on your environment

            visionlineIP = "https://127.0.0.1"; //Example : https://127.0.0.1
            endpointId = "JILLY44"; //Example : JILLY44
            keyExpireTime = DateTime.Now.AddDays(1).ToString("yyyyMMddTHHmm"); //Default 1 day (24h) valid key, modify if required


            // Static values, should not be modified
            contentType = "application/json; charset=utf-8";
            apiSession = "/api/v1/sessions";
            apiCard = "/api/v1/cards?action=mobileAccess&override=true";
        }

        public string visionlineIP { get; set; }
        public string apiSession { get; set; }
        public string apiCard { get; set; }
        public string contentType { get; set; }
        public string keyExpireTime { get; set; }
        public string endpointId { get; set; }
    }

    // JSON object for session query
    public class SessionJSON
    {
        public SessionJSON()
        {
            username = "sym";
            password = "sym";
        }

        public string username { get; set; }
        public string password { get; set; }
    }

    // JSON object for card/key query
    public class CardJSON
    {
        public CardJSON()
        {
            var doorOp = new CardJSONDoorOperations();
            doorOp.doors = new List<string> { "101" };
            doorOp.operation = "guest";
    
            doorOperations = new List<CardJSONDoorOperations> { doorOp };
            expireTime = DateTime.Now.AddDays(1).ToString("yyyyMMddTHHmm");
            format = "rfid48";
            label = "%ROOMRANGE%:%UUID%:%CARDNUM%";
            description = "Hotel California";
        }

        public string expireTime { get; set; }
        public string format { get; set; }
        public string endPointID { get; set; }
        public string label { get; set; }
        public string description { get; set; }

        public IList<CardJSONDoorOperations> doorOperations { get; set; }
    }

    // JSON object needed for card object
    public class CardJSONDoorOperations
    {
        public string operation { get; set; }
        public IList<string> doors { get; set; }
    }

    // JSON Response object for session query
    public class SessionResponse
    {
        public string accessKey { get; set; }
        public string id { get; set; }
    }

    // JSON Response object for card/key query
    public class KeyResponse
    {
        public string credentialId { get; set; }
    }

    class Program
    {
        /*
        * Helper classes for Newtonsoft JSON and default values
        * 
        * */
        public static InitialValues initialValues;
        public static CardJSON cardJSON;
        public static SessionJSON sessionJSON;
        public static SessionResponse sessionResponse;
        public static KeyResponse keyResponse;

        /*
        * Main console application flow 
        * 
        * */

        static void Main(string[] args)
        {
            setupInitialValues();
            

            if (initialValues.visionlineIP.Length > 0 && initialValues.endpointId.Length > 0)
            {
                getSession();
            }
            else
            {
                Console.WriteLine("Please check your initial values for visionlineIP IP & endpointID...");
            }
            
           
            Console.ReadLine();
        }

        public static void setupInitialValues()
        {
            initialValues = new InitialValues();
            cardJSON = new CardJSON();

            sessionJSON = new SessionJSON();

            cardJSON.endPointID = initialValues.endpointId;
            cardJSON.expireTime = initialValues.keyExpireTime;
        }

        public static void getSession()
        {
            string strJsonBody = JsonConvert.SerializeObject(sessionJSON);
                
            string contentMd5 = MD5Hash(strJsonBody);
            UTF8Encoding encoding = new UTF8Encoding();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(initialValues.visionlineIP + initialValues.apiSession); // create web request 
            //Disable certificate validity check
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };

            // REQUEST
            request.Method = "POST";
            request.ContentType = initialValues.contentType;
            request.Date = DateTime.Now;
            request.Headers["Content-MD5"] = contentMd5;

            try
            {
                byte[] bodyData = encoding.GetBytes(strJsonBody);
                request.ContentLength = bodyData.Length;
                using (System.IO.Stream s = request.GetRequestStream())
                {
                    s.Write(bodyData, 0, bodyData.Length);
                }
            }
            catch (WebException e)
            {
                displayError(e);
                return;
            }

            // RESPONSE
            HttpWebResponse response = null;
            string jsonResponse = "";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (System.IO.Stream s = response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        jsonResponse = sr.ReadToEnd();
                        Console.WriteLine("==================================================");
                        Console.WriteLine("SESSION REQUEST");
                        Console.WriteLine("==================================================");

                        //Save accessKey & sessionId for later use when we are creating key
                        sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(jsonResponse);
                        Console.WriteLine(String.Format("Response: {0}", JsonConvert.SerializeObject(sessionResponse)));

                        //Create key
                        createKey();
                    }
                }
            }
            catch (WebException e)
            {
                displayError(e);
            }
        }

        public static void createKey()
        {
            string strJsonBody = JsonConvert.SerializeObject(cardJSON, Formatting.Indented);

            string contentMd5 = MD5Hash(strJsonBody);
            UTF8Encoding encoding = new UTF8Encoding();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(initialValues.visionlineIP + initialValues.apiCard);
            request.Method = "POST";
            request.Headers["Content-MD5"] = contentMd5;
            request.Date = DateTime.Now;
            request.UserAgent = "MobileKeyOperator dev/0.16 (iPhone; iOS 9.2; Scale/2.00)";

            //Disable certificate validity check
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };

            // REQUEST
            //Sign the request
            string newLine = "\n";
            string line1 = string.Format("POST{0}", newLine);
            string line2 = string.Format("{0}{1}", contentMd5, newLine);
            string line3 = string.Format("{0}{1}", initialValues.contentType, newLine);
            string line4 = string.Format("{0}{1}", request.Headers.Get("Date"), newLine);
            string line5 = string.Format("{0}", initialValues.apiCard);
            string toSign = line1 + line2 + line3 + line4 + line5;

            //String to be signed with access key received from the /sessions call
            //Console.WriteLine(toSign);

            string hashed = HmacSha1Hash(toSign, sessionResponse.accessKey);
            string authorisationHeader = "AWS " + sessionResponse.id + ":" + hashed;

            Console.WriteLine(authorisationHeader);
            request.Headers["Authorization"] = authorisationHeader;
            
            request.ContentType = initialValues.contentType;

            try
            {
                byte[] bodyData = encoding.GetBytes(strJsonBody);
                request.ContentLength = bodyData.Length;
                using (System.IO.Stream s = request.GetRequestStream())
                {
                    s.Write(bodyData, 0, bodyData.Length);
                }
            }
            catch (WebException e)
            {
                displayError(e);
                return;
            }

            // RESPONSE
            HttpWebResponse response = null;
            string jsonResponse = "";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                using (System.IO.Stream s = response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        jsonResponse = sr.ReadToEnd();
                        Console.WriteLine("==================================================");
                        Console.WriteLine("CREATE KEY REQUEST");
                        Console.WriteLine("==================================================");

                        keyResponse = JsonConvert.DeserializeObject<KeyResponse>(jsonResponse);
                        Console.WriteLine(String.Format("Response: {0}", JsonConvert.SerializeObject(keyResponse)));

                    }
                }
            }
            catch (WebException e)
            {
                displayError(e);
            }

            deleteSession(sessionResponse.id);
        }

        public static void deleteSession(string sessionId)
        {
           
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(initialValues.visionlineIP + initialValues.apiSession + "/" + sessionId);
            //Disable certificate validity check
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };

            // REQUEST
            request.Method = "DELETE";
            //request.ContentType = initialValues.contentType;
            request.Date = DateTime.Now;
            

            //Sign the request
            string newLine = "\n";
            string line1 = string.Format("DELETE{0}", newLine);
            string line2 = string.Format("{0}", newLine);
            string line3 = string.Format("{0}", newLine);
            string line4 = string.Format("{0}{1}", request.Headers.Get("Date"), newLine);
            string line5 = string.Format("{0}", initialValues.apiSession +"/"+ sessionId);
            string toSign = line1 + line2 + line3 + line4 + line5;

            //String to be signed with access key received from the /sessions call
            //Console.WriteLine(toSign);

            string hashed = HmacSha1Hash(toSign, sessionResponse.accessKey);
            string authorisationHeader = "AWS " + sessionResponse.id + ":" + hashed;

            Console.WriteLine(authorisationHeader);
            request.Headers["Authorization"] = authorisationHeader;


            // RESPONSE
            HttpWebResponse response = null;

            Console.WriteLine("==================================================");
            Console.WriteLine("DELETE SESSION");
            Console.WriteLine("==================================================");

            try
            {
                response = (HttpWebResponse)request.GetResponse();

              
                if((int)response.StatusCode == 201)
                {
                    Console.WriteLine("Session deleted...");
                }
                else
                {
                    Console.WriteLine("Error deleting session...");
                }

            }
            catch (WebException e)
            {
                displayError(e);
            }
        }
        /*
         * Helper methods 
         * 
         * */

        public static string HmacSha1Hash(string stringToSign, string accessKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(accessKey);
            var inputBytes = Encoding.UTF8.GetBytes(stringToSign);
            var hmac = new HMACSHA1(keyBytes);
            var bits = hmac.ComputeHash(inputBytes);
            var result = Convert.ToBase64String(hmac.ComputeHash(inputBytes));
            return result;
        }

        public static string MD5Hash(string input)
        {
            System.Security.Cryptography.MD5 hs = System.Security.Cryptography.MD5.Create();
            byte[] bytes = Encoding.Default.GetBytes(input);
            string result = Convert.ToBase64String(hs.ComputeHash(bytes));
            return result;
        }

        public static void displayError(WebException e)
        {
            if (e.Status == WebExceptionStatus.ProtocolError)
            {
                Console.WriteLine("");
                Console.WriteLine("ERROR:");
                Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                using (Stream data = e.Response.GetResponseStream())
                using (var reader = new StreamReader(data))
                {
                    string text = reader.ReadToEnd();
                    Console.WriteLine(text);
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            else
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

}
