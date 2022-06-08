using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AAH_Call_Api.Models;
using Newtonsoft.Json;

namespace AAH_Call_Api
{
    internal class Program
    {
        //public static InitialValues initialValues;
        //public static CardJSON cardJSON;
        //public static SessionJSON sessionJSON;
        //public static SessionResponse sessionResponse;
        //public static KeyResponse keyResponse;
        static void Main(string[] args)
        {
            GetSession();

            Console.ReadLine();
        }

        public static void GetSession()
        {
            try
            {
                InitialValues initialValues = new InitialValues();
                CardJSON cardJSON = new CardJSON();
                SessionJSON sessionJSON = new SessionJSON();
                cardJSON.endPointID = initialValues.EndpointId;
                cardJSON.expireTime = initialValues.KeyExpireTime;

                Encoding encoding = Encoding.UTF8;

                HttpWebRequest request = WebRequest.Create(new Uri($"{initialValues.VisionlineIP}{initialValues.ApiSession}")) as HttpWebRequest;
                // Unable to check SSL secure
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };

                request.Method = "POST";
                request.ContentType = initialValues.ContentType;
                request.Date = DateTime.Now;
                request.Headers["Content-MD5"] = MD5Hash(JsonConvert.SerializeObject(sessionJSON,Formatting.None));

                try
                {
                    var jsonBody = encoding.GetBytes(JsonConvert.SerializeObject(sessionJSON,Formatting.None));
                    request.ContentLength = jsonBody.Length;
                    using (Stream s = request.GetRequestStream())
                    {
                        s.Write(jsonBody, 0, jsonBody.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    return;
                }

                // Get response
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (System.IO.Stream s = response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                    {
                        var jsonResponse = sr.ReadToEnd();
                        Console.WriteLine("==================================================");
                        Console.WriteLine("SESSION REQUEST");
                        Console.WriteLine("==================================================");

                        //Save accessKey & sessionId for later use when we are creating key
                        var sessionResponse = JsonConvert.DeserializeObject<SessionResponse>(jsonResponse);
                        Console.WriteLine(String.Format("Response: {0}", JsonConvert.SerializeObject(sessionResponse)));

                       
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("---------------------------Exception----------------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                displayError(e as WebException);
            }
        }

        public static void CreateKey()
        {

        }

        public static void DeleteSession()
        {

        }

        private static string MD5Hash(string input)
        {
            System.Security.Cryptography.MD5 hs = System.Security.Cryptography.MD5.Create();
            byte[] bytes = Encoding.Default.GetBytes(input);
            string result = Convert.ToBase64String(hs.ComputeHash(bytes));
            return result;
        }

        private static string HmacSha1Hash(string stringToSign, string accessKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(accessKey);
            var inputBytes = Encoding.UTF8.GetBytes(stringToSign);
            var hmac = new HMACSHA1(keyBytes);
            var bits = hmac.ComputeHash(inputBytes);
            var result = Convert.ToBase64String(hmac.ComputeHash(inputBytes));
            return result;
        }

        private static void displayError(WebException e)
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
