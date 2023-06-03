using Laboratorium3.Interfaces;
using Laboratorium3.Models;
using Newtonsoft.Json;
using System.Net;

namespace Laboratorium3
{
    public class NBPGoldRateWebClient : NBPBaseWebClient, IGoldRateWebClient
    {

        public NBPGoldRateWebClient(string address) : base(address)
        {
        }
        #region Constructors
        public GoldDTO GetLastGoldRate()
        {
            GoldDTO wynik = new GoldDTO();
            var response = _webClient.DownloadString("cenyzlota");

            wynik = JsonConvert.DeserializeObject<GoldDTO[]>(response)!.First();
            return wynik;
        }
        #endregion
        #region Public Methods
        public GoldDTO[] GetLastGoldRates(int count)
        {
            try
            {
                return Get<GoldDTO[]>($"cenyzlota/last/{count}");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public GoldDTO GetGoldRateInSpecificDate(DateTime date)
        {
            var wynik = new GoldDTO();
            try
            {
                wynik = Get<GoldDTO[]>($"cenyzlota/{date.ToString(_dateFormat)}").First();
                return wynik;
            }
            catch (WebException ex)
            {
                if(ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response =ex.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null!;
                    }
                }
                else
                    Console.WriteLine($"Nieznany błąd :{ex.Message}");
            }

            return wynik;
        }

        public bool IsGoldRatePublishedToday()
        {
            var wynik = false;

            try
            {
                var response = _webClient.DownloadString($"cenyzlota/today");
                var dzisiejszeNotowanie = JsonConvert.DeserializeObject<GoldDTO[]>(response).First();
                wynik = dzisiejszeNotowanie != null;
                return wynik;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                       return false;
                    }
                }
                else
                    Console.WriteLine($"Nieznany błąd :{ex.Message}");
            }

            return wynik;
        }

        public IList<GoldDTO> GetGoldRatesInSpecificPeriod(DateTime startDate, DateTime endDate)
        {
            var wynik = new List<GoldDTO>();

            var response = _webClient.DownloadString($"cenyzlota/{startDate.ToString(_dateFormat)}" +
                $"/{endDate.ToString(_dateFormat)}");
            wynik = JsonConvert.DeserializeObject<List<GoldDTO>>(response); 

            return wynik!;
        }

        public GoldDTO? GetTodayGoldRate()
        {
            var wynik = new GoldDTO();
            
            try
            {
                var response = _webClient.DownloadString("cenyzlota/today");
                wynik = JsonConvert.DeserializeObject<GoldDTO>(response);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var exResponse = ex.Response as HttpWebResponse;
                    if (exResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                }
                else
                    Console.WriteLine($"Nieznany błąd :{ex.Message}");
            }
            return wynik;
        }
        #endregion
        #region Private methods

        #endregion
    }
}
