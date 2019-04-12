using System;
using System.IO;
using TheReportLibrary;

namespace TheReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Data used to generate the 2019 report:
            // --> Data on passport and destinations access levels : 2019   [from: https://github.com/ilyankou/passport-index-dataset]
            // --> WEF Travel & Tourism Competitiveness Index : 2017        [from: https://tcdata360.worldbank.org]
            // --> Human Development Index : 2017 [HdrYear]                 [from: Human Development Report Office Statistical Data API]
            // --> Gross Domestic Product : 2017 [HdrYear]                  [from: Human Development Report Office Statistical Data API]

            // And remove spaces from headers of passport-index-iso3-tidy!

            int HdrYear = 2017;

            string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string resourcesPath = projectPath + @"/Resources/";
            string resultsPath = projectPath + @"/Results/";

            string csvFilePath1 = resourcesPath + @"passport-index-iso3-tidy.csv";
            string csvFilePath2 = resourcesPath + @"WEF Travel & Tourism Competitiveness Index 2017.csv";

            string DestinationsFinalGradesPath = resultsPath + "DestinationsFinalGrades.csv";
            string DestinationPassportsFinalScoresPath = resultsPath + "PassportsFinalScores.csv";

            ReportGenerator newOperation = new ReportGenerator(csvFilePath1, csvFilePath2, HdrYear);

            newOperation.LoadDatasetCsv1();
            newOperation.LoadDatasetCsv2();

            if (newOperation.CalculateDestinationGrades())
            {
                newOperation.CalculatePassportFinalScores();

                newOperation.ExportDestinationsFinalGrades(filePath: DestinationsFinalGradesPath);
                newOperation.ExportPassportsFinalScores(filePath: DestinationPassportsFinalScoresPath);
            }
        }
    }
}