using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace BlockFlash
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            double sVolume = 0.3;
            double dtComssion = 0.001;
            double buyVolume = sVolume + dtComssion;

            Console.WriteLine("- BitCoin Arbitrage System by Julian - julianmb@gmail.com");
            Console.WriteLine("- Not Buying Real Bitcoin, but checking current prices for "+buyVolume.ToString("F3")+" Volume in the markets with comissions included -");

            Console.WriteLine("====================================================================================================================");

           
            Console.WriteLine();

            //Init
            var api = new ExmoAPI.ExmoApi("K-762b56b6c172f39da2bd10f5b3a08d08fe933a4e", "S-98baf899fcab15a345e8d79a086bca413560e042");

            bool done = false;
            double dProfit = 0;

            while (!done)
            {
  
                if (Console.KeyAvailable)
                    done = true;

                Console.WriteLine("-------------- NEW OPERATION -----------------------------------------------------------------------------------");
                var result = api.ApiQuery("order_book", new Dictionary<string, string> { { "pair", "BTC_USD" }, { "limit", "50" } });

                ExmoApi.OrderBook ob = ExmoApi.OrderBook.FromJson(result);


                Console.WriteLine("Searching in (A) OrderBook for a Sell Volume of more than "+buyVolume.ToString("F4")+" Btc");

                double aBTC = 0;
                double aVolume = 0;
 
                for (int i = 0; i < ob.BTCUSD.Ask.Length; i++){
                    
                    aVolume = aVolume + Double.Parse(ob.BTCUSD.Ask[i][1]);

                    if(aVolume>buyVolume){
                        aBTC = Double.Parse(ob.BTCUSD.Ask[i][0]);
                        break;
                    }
                }
                double aBTCwc = aBTC * 1.0015;

                Console.Write(DT() + "(A) Btc Buy Price: " + aBTC.ToString("N") + " Volume: " + aVolume.ToString("N") + " Top Ask: " + ob.BTCUSD.AskTop);


                Console.Write(" <-> ");
                //GatecoinServiceInterface.Client.ServiceClient sc = new GatecoinServiceInterface.Client.ServiceClient();
                //sc.Get(new IReturn<void>)
                //GatecoinServiceInterface.Request.OrderBook obd = new GatecoinServiceInterface.Request.OrderBook();
                //obd.CurrencyPair = "BTC/USD";

                //GatecoinServiceInterface.Client.ServiceClient sc = new GatecoinServiceInterface.Client.ServiceClient();
                //sc.Get(obd);

                string jsMD = GetGCMarketDepth();
                Providers.Gatecoin.MarketDepth gcmd = Providers.Gatecoin.MarketDepth.FromJson(jsMD);
                double bBTC = gcmd.Bids[0].Price;
                double bVolume = gcmd.Bids[0].Volume;


                Console.WriteLine("(B) Btc Sell Price: " + bBTC.ToString("N") + " Volume: " + bVolume.ToString("N") + " Top Bid: " + gcmd.Bids[0].Price.ToString("N"));
                Console.Write(DT() + "Price diff w/o commisions: " + (bBTC - aBTC).ToString("F2"));

                double oDiff = bBTC - aBTC;
                double obBTC = bBTC;
                double obBTCwc = obBTC / 1.003;
                Console.WriteLine(" [] Price diff w/ commisions: " + (obBTCwc - aBTC).ToString("N"));

                Console.WriteLine(DT() + "Buying from (A) at price: " + aBTCwc.ToString("N") + " Comissions Included (" + (aBTC * 0.0015).ToString("N") + ")");
                double aCost = buyVolume * aBTCwc;
                Console.WriteLine(DT() + "Bought " +buyVolume+ " BTC Total Costs " + aCost.ToString("N"));
                Console.Write("Transfering to B (Wait 30m): ");

                //DEMO: transfering BitCoin from A to B
                int m = 0;
                while (m <= 1)
                {
                    System.Threading.Thread.Sleep(1000 * 60);
                    Console.Write(m.ToString() + ".");
                    m++;
                    if (Console.KeyAvailable){
                        done = true;
                        break;
                    }
                }
                Console.WriteLine();

                Console.WriteLine(DT() + sVolume.ToString("N") + "Bitcoin Transfered to B. Transfer Costs: "+dtComssion.ToString("N3")+" "+(gcmd.Bids[0].Price * 0.001).ToString("N")+" USD");

                jsMD = GetGCMarketDepth();
                gcmd = Providers.Gatecoin.MarketDepth.FromJson(jsMD);


                Console.WriteLine("Searching in (B) OrderBook for a Buy Volume of more than "+sVolume.ToString("N")+" Btc");


                for (int i = 0; i <= gcmd.Bids.Length; i++)
                {
                    double tVol =+ gcmd.Bids[i].Volume;

                    if (tVol > sVolume)
                    {
                        bBTC = gcmd.Bids[i].Price;
                        bVolume = tVol;
                        break;
                    }
                }
                double bBTCwc = bBTC / 1.0030;
                double FinalPriceDiff = bBTCwc - aBTCwc;


                Console.WriteLine(DT() + "Original Buying (A) Price: " + aBTCwc.ToString("N") + " PriceDiff: " + FinalPriceDiff.ToString("N") + " Expected Diff: " + (oDiff.ToString("N")));
                Console.WriteLine(DT() + "Selling " + sVolume.ToString("N3") + "BTC at current (B) price: " + bBTCwc.ToString("N") + " Comissions Included (" + (bBTC * 0.003).ToString("N") + ")");

                double bIncome = bBTCwc * sVolume;
                double abProfit = (bIncome - aCost);
                Console.WriteLine(DT() + "Got " + bIncome.ToString("N") + " With a Profit of " + abProfit.ToString("N") + " Original Cost "+ aCost.ToString("N"));

                dProfit += abProfit;
                Console.WriteLine(DT() + "Accumulated profit: "+dProfit.ToString());

                Console.WriteLine();

            }   

        }
        public static string GetGCMarketDepth()
        {

            string responseMessage = string.Empty;
            string unixdate = GetUnixDateNow();
            string baseAddress = "https://api.gatecoin.com/";
            string apiPath = "Public/MarketDepth/BTCUSD";
            var request = (HttpWebRequest)WebRequest.Create(baseAddress + apiPath);
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "";
            string message = request.Method + baseAddress + apiPath + unixdate;

            request.Headers["API_REQUEST_DATE"] = unixdate;
            WebResponse response = request.GetResponse();

            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Read the content.
            if (((HttpWebResponse)response).StatusDescription == "OK")
            {
                using (var stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseMessage = reader.ReadToEnd();
                }
            }
            return responseMessage;
        }


        private static string CreateToken(string message, string secretKey)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        private static string GetUnixDateNow()
        {
            DateTime datetime = DateTime.UtcNow;
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return ((decimal)(datetime - sTime).TotalSeconds).ToString();
        }
        private static string DT(){
            return DateTime.Now.ToString("[hh:mm:ss] ");
        }

    }
}
