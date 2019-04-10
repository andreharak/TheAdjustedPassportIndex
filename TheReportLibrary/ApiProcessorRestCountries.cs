using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace TheReportLibrary
{
    public static class ApiProcessorRestCountries
    {
        public static (Dictionary<string, string>, Dictionary<string, double>) LoadCountriesFullNameAndArea(List<string> CountriesIso3Codes)
        {
            Dictionary<string, string> CountriesFullNames = new Dictionary<string, string>();
            Dictionary<string, double> CountriesAreas = new Dictionary<string, double>();

            foreach (string c in CountriesIso3Codes)
            {
                CountriesAreas.Add(c, 0);
                CountriesFullNames.Add(c, "");
            }

            try
            {
                string url = $"https://restcountries.eu/rest/v2/all";

                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);

                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    string JsonContent = response.Content;

                    var NewObject = JsonConvert.DeserializeObject<dynamic>(JsonContent.ToString());

                    foreach (var i in NewObject.Children())
                    {
                        if (i.ContainsKey("alpha3Code"))
                        {
                            string code = i.alpha3Code.Value;

                            if (CountriesAreas.Keys.Contains(code))
                            {
                                double area = 0;
                                try
                                {
                                    area = i.area.Value;
                                }
                                catch { }
                                CountriesAreas[code] = area;
                            }

                            if (i.ContainsKey("name"))
                            {
                                string fullName = i.name.Value;

                                if (CountriesFullNames.Keys.Contains(code))
                                {
                                    CountriesFullNames[code] = fullName;
                                }
                            }
                        }
                    }
                }
                // Manually filling Kosovo's full name:
                if (CountriesFullNames.Keys.Contains("XKX")) CountriesFullNames["XKX"] = "Kosovo";
                if (CountriesFullNames.Keys.Contains("CIV")) CountriesFullNames["CIV"] = "Cote d'Ivoire";

                return (CountriesFullNames, CountriesAreas);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
