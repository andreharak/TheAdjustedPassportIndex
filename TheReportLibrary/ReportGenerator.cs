using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace TheReportLibrary
{
    public class ReportGenerator
    {
        public int HdrYear { get; set; }

        private string CsvFilePath1 { get; set; }   // Path for CSV 1
        private string CsvFilePath2 { get; set; }   // Path for CSV 2

        public readonly double accessLevel1 = 1;    // Visa free
        public readonly double accessLevel2 = 0.9;  // Visa on arrival
        public readonly double accessLevel3 = 0.8;  // eTA required

        public readonly int weight1 = 5;    // Human Development Index
        public readonly int weight2 = 1;    // Economical size
        public readonly int weight3 = 1;    // Geographical size
        public readonly int weight4 = 5;    // WEF Travel & Tourism Competitiveness Index

        // List of ISO.3 countries codes [for Passports and Destinations]
        public List<string> CountriesIso3Codes { get; set; }

        // string: code - string: full_name
        public Dictionary<string, string> CountriesFullNames { get; set; }

        // Records from the CSV source files
        public List<Dataset1> RecordsFromFile1 { get; set; }
        public List<Dataset2> RecordsFromFile2 { get; set; }

        // string: passport_code - List<string>: destination_code [with visa free access]
        public Dictionary<string, List<string>> PassportVisaFreeDestinations { get; set; }
        public Dictionary<string, List<string>> PassportVisaOnArrivalDestinations { get; set; }
        public Dictionary<string, List<string>> PassporteTArequiredDestinations { get; set; }

        // string: destination_code - List<double>: [value1, value2, value3, value4, FinalScore]
        public Dictionary<string, List<double>> DestinationGrades { get; set; }

        // string: passport_code - double: passport_score
        public Dictionary<string, double> PassportFinalScores { get; set; }

        public ReportGenerator(string filePath1, string filePath2, int hdrYear)
        {
            CsvFilePath1 = filePath1;
            CsvFilePath2 = filePath2;
            HdrYear = hdrYear;

            CountriesIso3Codes = new List<string>();
            CountriesFullNames = new Dictionary<string, string>();
            RecordsFromFile1 = new List<Dataset1>();
            RecordsFromFile2 = new List<Dataset2>();
            PassportVisaFreeDestinations = new Dictionary<string, List<string>>();
            PassportVisaOnArrivalDestinations = new Dictionary<string, List<string>>();
            PassporteTArequiredDestinations = new Dictionary<string, List<string>>();
            DestinationGrades = new Dictionary<string, List<double>>();
            PassportFinalScores = new Dictionary<string, double>();
        }

        #region Load CSV
        public void LoadDatasetCsv1()
        {
            using (StreamReader reader = new StreamReader(CsvFilePath1))
            using (CsvReader csv = new CsvReader(reader))
            {
                var records = csv.GetRecords<Dataset1>();
                foreach (Dataset1 r in records)
                {
                    RecordsFromFile1.Add(r);
                }
            }

            // Fill Countries Iso3 codes:
            // Prepare all levels of Passport Destinations:
            foreach (Dataset1 d in RecordsFromFile1)
            {
                if (!CountriesIso3Codes.Contains(d.Passport))
                {
                    CountriesIso3Codes.Add(d.Passport);
                    PassportVisaFreeDestinations.Add(d.Passport, new List<string>());
                    PassportVisaOnArrivalDestinations.Add(d.Passport, new List<string>());
                    PassporteTArequiredDestinations.Add(d.Passport, new List<string>());
                }
            }

            // Fill all levels of Passport Destinations:
            foreach (Dataset1 d in RecordsFromFile1)
            {
                string passport_code = d.Passport;
                string destination_code = d.Destination;

                if (d.Value == 3)
                {
                    PassportVisaFreeDestinations[passport_code].Add(destination_code);
                }
                if (d.Value == 1)
                {
                    PassportVisaOnArrivalDestinations[passport_code].Add(destination_code);
                }
                if (d.Value == 2)
                {
                    PassporteTArequiredDestinations[passport_code].Add(destination_code);
                }
            }
        }

        public void LoadDatasetCsv2()
        {
            using (StreamReader reader = new StreamReader(CsvFilePath2))
            using (CsvReader csv = new CsvReader(reader))
            {
                var records = csv.GetRecords<Dataset2>();
                foreach (Dataset2 r in records)
                {
                    // Only take: WEF Travel & Tourism Competitiveness Index
                    if (string.Equals(r.Indicator_Id, ConstAndParams.WEFTTCIcode))
                    {
                        RecordsFromFile2.Add(r);
                    }
                }
            }
        }
        #endregion

        #region Calculate
        public bool CalculateDestinationGrades()
        {
            try
            {
                // By default, all grades are = 0 and FinalScore = 0:
                foreach (string code in CountriesIso3Codes)
                {
                    DestinationGrades.Add(code, new List<double>() { 0, 0, 0, 0, 0 });
                }

                string year = HdrYear.ToString();

                // Value1: Human Development Index
                if (FetchHDIfromHumanDevelopmentReport(year))
                {
                    // Value2: Geographical and Economical size
                    if (FetchGDPfromHumanDevelopmentReport(year) && FetchCountriesAreasFromRestCountriesApi())
                    {
                        // Value3: WEF Travel & Tourism Competitiveness Index
                        if (FillWefTourismIndexValues())
                        {
                            // Success!
                            CalculateDestinationFinalScores();
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void CalculateDestinationFinalScores()
        {
            int denominator = weight1 + weight2 + weight3 + weight4;
            foreach (KeyValuePair<string, List<double>> s in DestinationGrades)
            {
                s.Value[4] = ((weight1 * s.Value[0]) + (weight2 * s.Value[1]) + (weight3 * s.Value[2]) + (weight4 * s.Value[3])) / denominator;
            }
        }

        public void CalculatePassportFinalScores()
        {
            foreach (string passport in CountriesIso3Codes)
            {
                // Include the points for self access [a Passport having access to its home country as destination]
                double score = accessLevel1 * DestinationGrades[passport][4];

                // Only include one level for a [Passport <-> Destination] in the score
                foreach (string destination in CountriesIso3Codes)
                {
                    if (PassportVisaFreeDestinations[passport].Contains(destination))
                    {
                        score += accessLevel1 * DestinationGrades[destination][4];
                    }
                    else if (PassportVisaOnArrivalDestinations[passport].Contains(destination))
                    {
                        score += accessLevel2 * DestinationGrades[destination][4];
                    }
                    else if (PassporteTArequiredDestinations[passport].Contains(destination))
                    {
                        score += accessLevel3 * DestinationGrades[destination][4];
                    }
                }
                PassportFinalScores.Add(passport, score);
            }
        }
        #endregion

        #region Fetch Data
        private bool FetchHDIfromHumanDevelopmentReport(string hdrYear)
        {
            if ((!string.IsNullOrEmpty(hdrYear)) && CountriesIso3Codes.Count > 0)
            {
                Dictionary<string, double> HDIperCountry = ApiProcessorHDR.LoadHdrHDI(CountriesIso3Codes, hdrYear);

                if ((HDIperCountry != null) && (HDIperCountry.Count > 0))
                {
                    foreach (var hdi in HDIperCountry)
                    {
                        DestinationGrades[hdi.Key][0] = 100 * hdi.Value;
                    }
                    return true;
                }
            }
            return false;
        }

        private bool FetchGDPfromHumanDevelopmentReport(string hdrYear)
        {
            if ((!string.IsNullOrEmpty(hdrYear)) && CountriesIso3Codes.Count > 0)
            {
                Dictionary<string, double> GDPperCountry = ApiProcessorHDR.LoadHdrGDP(CountriesIso3Codes, hdrYear);

                if ((GDPperCountry != null) && (GDPperCountry.Count > 0))
                {
                    double WorldMaxGdp = GDPperCountry.Values.Max() * 1.01;

                    foreach (var gdp in GDPperCountry)
                    {
                        DestinationGrades[gdp.Key][1] = 100 * gdp.Value / WorldMaxGdp;
                    }
                    return true;
                }
            }
            return false;
        }

        private bool FetchCountriesAreasFromRestCountriesApi()
        {
            if (CountriesIso3Codes.Count > 0)
            {
                (Dictionary<string, string>, Dictionary<string, double>) result = ApiProcessorRestCountries.LoadCountriesFullNameAndArea(CountriesIso3Codes);

                CountriesFullNames = result.Item1;
                Dictionary<string, double> CountriesAreas = result.Item2;

                if ((CountriesAreas != null) && (CountriesAreas.Count > 0))
                {
                    double WorldMaxArea = CountriesAreas.Values.Max() * 1.01;

                    foreach (var c in CountriesAreas)
                    {
                        DestinationGrades[c.Key][2] = 100 * c.Value / WorldMaxArea;
                    }
                    return true;
                }
            }
            return false;
        }

        private bool FillWefTourismIndexValues()
        {
            if (RecordsFromFile2.Count > 0)
            {
                // Calculate the MaxScore
                List<double> Scores = new List<double>();
                foreach (Dataset2 r in RecordsFromFile2)
                {
                    double.TryParse(r.Y2017, out double IndexValue);
                    Scores.Add(IndexValue);
                }
                double MaxScore = Scores.Max() * 1.01;
                if (Math.Abs(MaxScore) < double.Epsilon) MaxScore = 6;

                // Calculate the % score per destination
                foreach (Dataset2 r in RecordsFromFile2)
                {
                    if (DestinationGrades.Keys.Contains(r.Country_ISO3))
                    {
                        double IndexValue = 0;
                        if (!string.IsNullOrEmpty(r.Y2017))
                        {
                            double.TryParse(r.Y2017, out IndexValue);
                        }
                        else
                        {
                            double.TryParse(r.Y2015, out IndexValue);
                        }
                        DestinationGrades[r.Country_ISO3][3] = 100 * IndexValue / MaxScore;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Export results
        public void ExportDestinationsFinalGrades(string filePath)
        {
            var DisplayList = DestinationGrades.OrderByDescending(o => o.Value[4]).ToList();

            var records = new List<DestinationFinalGrades>();

            int rank = 1;
            foreach (var x in DisplayList)
            {
                string fullName = CountriesFullNames[x.Key];

                double hdi = Math.Round(x.Value[0], 1);
                double gdp = Math.Round(x.Value[1], 1);
                double area = Math.Round(x.Value[2], 1);
                double tourism = Math.Round(x.Value[3], 1);
                double score = Math.Round(x.Value[4], 2);

                records.Add(new DestinationFinalGrades(rank, x.Key, fullName, hdi, gdp, area, tourism, score));
                rank++;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            using (CsvWriter csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }
        }

        public void ExportPassportsFinalScores(string filePath)
        {
            var DisplayList = PassportFinalScores.OrderByDescending(o => o.Value).ToList();

            var records = new List<PassportFinalScore>();

            int rank = 1;
            int nbSameRank = 1;
            double previous_score = PassportFinalScores.Values.Max();
            foreach (var x in DisplayList)
            {
                string fullName = CountriesFullNames[x.Key];
                double score = Math.Round(x.Value, 2);
                int nbVisaFree = PassportVisaFreeDestinations[x.Key].Count;
                int nbVisaOnArrival = PassportVisaOnArrivalDestinations[x.Key].Count;
                int nbETArequired = PassporteTArequiredDestinations[x.Key].Count;

                // Increase rank
                if (score < previous_score)
                {
                    previous_score = score;
                    rank += nbSameRank;
                    nbSameRank = 1;
                }
                // Maintain same rank
                else if (Math.Abs(score - previous_score) < double.Epsilon)
                {
                    nbSameRank++;
                }

                records.Add(new PassportFinalScore(rank, x.Key, fullName, score, nbVisaFree, nbVisaOnArrival, nbETArequired));
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            using (CsvWriter csv = new CsvWriter(writer))
            {
                csv.WriteRecords(records);
            }
        }
        #endregion
    }
}