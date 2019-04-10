using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace TheReportLibrary
{
    public static class ApiProcessorHDR
    {
        public const string HDIcode = ConstAndParams.HDIcode;
        public const string GDPcode = ConstAndParams.GDPcode;

        public static Dictionary<string, double> LoadHdrHDI(List<string> CountriesIso3Codes, string HdrYear)
        {
            string url = $"http://ec2-54-174-131-205.compute-1.amazonaws.com/API/HDRO_API.php/indicator_id={ HDIcode }/year={ HdrYear }";

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            try
            {
                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    string JsonContent = response.Content;

                    var NewObject = JsonConvert.DeserializeObject<dynamic>(JsonContent.ToString());

                    if (NewObject.ContainsKey("indicator_value"))
                    {
                        var Results = JsonConvert.DeserializeObject<dynamic>(NewObject.indicator_value.ToString());

                        Dictionary<string, double> HDIperCountry = new Dictionary<string, double>();

                        foreach (string code in CountriesIso3Codes)
                        {
                            if (Results.ContainsKey(code))
                            {
                                foreach (var i in Results.Children())
                                {
                                    if (string.Equals(i.Path, code))
                                    {
                                        foreach (var j in i.Children())
                                        {
                                            foreach (var x in j.Children())
                                            {
                                                foreach (var y in x.Children())
                                                {
                                                    foreach (var z in y.Children())
                                                    {
                                                        if (string.Equals(z.Name, HdrYear))
                                                        {
                                                            string IndexText = z.Value;
                                                            double.TryParse(IndexText, out double IndexValue);

                                                            HDIperCountry.Add(code, IndexValue);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                HDIperCountry.Add(code, 0);
                            }
                        }
                        return HDIperCountry;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, double> LoadHdrGDP(List<string> CountriesIso3Codes, string HdrYear)
        {
            string url = $"http://ec2-54-174-131-205.compute-1.amazonaws.com/API/HDRO_API.php/indicator_id={ GDPcode }/year={ HdrYear }";

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            try
            {
                IRestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    string JsonContent = response.Content;

                    var NewObject = JsonConvert.DeserializeObject<dynamic>(JsonContent.ToString());

                    if (NewObject.ContainsKey("indicator_value"))
                    {
                        var Results = JsonConvert.DeserializeObject<dynamic>(NewObject.indicator_value.ToString());

                        Dictionary<string, double> GDPperCountry = new Dictionary<string, double>();

                        foreach (string code in CountriesIso3Codes)
                        {
                            if (Results.ContainsKey(code))
                            {
                                foreach (var i in Results.Children())
                                {
                                    if (string.Equals(i.Path, code))
                                    {
                                        foreach (var j in i.Children())
                                        {
                                            foreach (var x in j.Children())
                                            {
                                                foreach (var y in x.Children())
                                                {
                                                    foreach (var z in y.Children())
                                                    {
                                                        if (string.Equals(z.Name, HdrYear))
                                                        {
                                                            string IndexText = z.Value;
                                                            double.TryParse(IndexText, out double IndexValue);

                                                            GDPperCountry.Add(code, IndexValue);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                GDPperCountry.Add(code, 0);
                            }
                        }
                        return GDPperCountry;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
