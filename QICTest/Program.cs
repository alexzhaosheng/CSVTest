using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QICTest
{

    //This solution uses a "heavy" way to deal with CSV for test purpose.
    //In the real case, with the given requirements, we should deal with CSV with several simple lines of code.
    //If there comes more requirements, we can refactor the current code to add more features.


    //This solution is just for the relatively smaller amount of the data. If we have a huge amount of data, it's much more complicated.
    //We may be able to use the idea of radix sort to get the median number, here is how:
    //1. Check the top bit of the numbers, if 0 is more, then the median number is started with 0
    //2. Check the 2nd top bit of the numbers, if 0 is more, then the median number is started with 00
    //3. Repeat until the whole median number is found out.

    class Program
    {
        static void Main(string[] args)
        {
            var fileFolder = args.FirstOrDefault();
            if(fileFolder == null)
            {
                Console.WriteLine("Please input the file folder.");
                return;
            }

            try
            {
                //the result will be put into this CSVFile object and be wrote to the disk eventually.
                var resultCSV = new CSVFile(',');
                resultCSV.SetColums(new[] { "file name", "date", "value", "median value" });

                var files = Directory.GetFiles(fileFolder, "*.csv");
                foreach(var file in files)
                {
                    if(Path.GetFileName(file) == "result.csv")
                    {
                        continue;
                    }

                    ProcessCSV(file, resultCSV);
                }

                //write the result to a CSV file in target folder.
                using(FileStream fs = new FileStream(Path.Combine(fileFolder, "result.csv"), FileMode.Create))
                {
                    resultCSV.WriteToStream(fs);
                }                
            }
            catch(Exception err)
            {
                Console.WriteLine("Error detected. Check the exception details for more information.");
                Console.WriteLine(err.ToString());
                Console.ReadKey();
            }
        }

        private static void ProcessCSV(string csvFilePath, CSVFile resultCSV)
        {
            var csv = new CSVFile(',');
            using (FileStream fs = new FileStream(csvFilePath, FileMode.Open))
            {                
                csv.Load(fs);
            }

            //if there's only one record, simply ignore this file.
            if(csv.Length <= 1)
            {
                return;
            }

            string fileName = Path.GetFileName(csvFilePath);

            //the value column name is depended on the name of CSV file.
            string valueColumName;
            if (fileName.StartsWith("comm"))
            {
                valueColumName = "Price SOD";
            }
            else
            {
                valueColumName = "MOD Duration";
            }


            //to get the median value, we need the sorted array for values in the data.
            double[] values = new double[csv.Length];
            for(var i=0; i< csv.Length; i++)
            {
                values[i] = csv.GetDataInCell<double>(i, valueColumName);
            }
            
            Array.Sort(values);

            //now calculate the median value
            double median;
            if (values.Length % 2 == 0)
            {
                median = (values[(int) (values.Length / 2)] + values[(int) (values.Length / 2 - 1)] )/2;
            }
            else
            {
                median = values[(int)Math.Floor((double)values.Length / 2)];
            }
             

            //now we can work on the result
            for(var i = 0; i< csv.Length; i++)
            {
                var dataValue = csv.GetDataInCell<double>(i, valueColumName);

                //only print the abnormal values (20% above or below the median) 
                //and we need make sure there's no divde by zero problem
                if (median == dataValue)
                {
                    continue;
                }
                else if(median != 0)
                {
                    if (Math.Abs(median - dataValue) / median < 0.2d) 
                    {
                        continue;
                    }
                }
                else
                {
                    if (Math.Abs(median - dataValue) / dataValue < 0.2d)
                    {
                        continue;
                    }

                }
                
                resultCSV.AddData(new[]
                {
                    fileName,
                    csv.GetDataInCell<string>(i, "Date"),
                    csv.GetDataInCell<string>(i, valueColumName),
                    median.ToString()
                });
            }
        }
    }
}
