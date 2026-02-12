using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TextFileParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            basePath = Directory.GetParent(basePath).Parent.Parent.FullName;

            string incomingPath = Path.Combine(basePath, "Incoming");
            string outgoingPath = Path.Combine(basePath, "Outgoing");
            string backupPath = Path.Combine(basePath, "Backup");

            string inputFile = Path.Combine(incomingPath, "InputData.txt");
            string outputFile = Path.Combine(outgoingPath, "OutputData.txt");

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("Archivo de entrada no encontrado.");
                return;
            }

            var lines = File.ReadAllLines(inputFile).ToList();

            List<string> processedLines = new List<string>();
            string currentLine = "";

            foreach (var line in lines.Skip(1))
            {
                if (!line.StartsWith("|"))
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        processedLines.Add(currentLine);
                    }

                    currentLine = line;
                }
                else
                {
                    currentLine += line;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                processedLines.Add(currentLine);
            }
            if (lines.Count <= 1)
            {
                Console.WriteLine("No hay datos para procesar.");
                return;
            }

            List<string> outputLines = new List<string>();
           
            //HEADER_RECORD
            var groupedCustomers = processedLines
                .Select(line => line.Split('|'))
                .GroupBy(fields => fields[0]);

            int totalCustomers = groupedCustomers.Count();
            decimal grandTotal = 0;

            foreach (var line in processedLines)
            {
                var fields = line.Split('|');

                for (int i = 11; i < fields.Length; i += 2)
                {
                    string value = fields[i].Trim();

                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                    {
                        grandTotal += amount;
                    }
                }
            }
            string formattedGrandTotal = grandTotal.ToString("N2", CultureInfo.InvariantCulture);

            DateTime now = DateTime.Now;
            string fileName = "InputData.txt";
            string todayDate = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string timeStamp = now.ToString("hh:mm:ss tt", new CultureInfo("en-US"));

            outputLines.Add(
                $"\"HEADER_RECORD\",\"{fileName}\",{totalCustomers},\"{formattedGrandTotal}\",\"{todayDate}\",\"{timeStamp}\""
            );

            //CUSTOMER_RECORD
            foreach (var customerGroup in groupedCustomers)
            {
                var firstRecord = customerGroup.First();

                int counter = int.Parse(firstRecord[0]);
                string firstName = firstRecord[1];
                string lastName = firstRecord[2];
                string addr1 = firstRecord[4];
                string addr2 = firstRecord[5];
                string city = firstRecord[6];
                string state = firstRecord[7];
                string zipcode = firstRecord[8];
                string accountNumber = firstRecord[9];

                outputLines.Add(
                    $"\"CUSTOMER_RECORD\",{counter},\"{firstName}\",\"{lastName}\",\"{addr1}\",\"{addr2}\",\"{city}\",\"{state}\",{zipcode},{accountNumber}"
                );


                //DETAILS_RECORD
                decimal customerTotal = 0;

                var record = customerGroup.First();

                for (int i = 10; i < record.Length; i += 2)
                {
                    string description = record[i];

                    if (decimal.TryParse(record[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                    {
                        customerTotal += amount;

                        string code = GetCode(amount);

                        string formattedAmount = "$" + amount.ToString("N2", CultureInfo.InvariantCulture);

                        outputLines.Add(
                            $"\"DETAILS_RECORD\",\"{description}\",\"{code}\",\"{formattedAmount}\""
                        );
                    }
                }
                string formattedCustomerTotal = "$" + customerTotal.ToString("N2", CultureInfo.InvariantCulture);

                //TOTAL DETAILS_RECORD
                outputLines.Add(
                    $"\"DETAILS_RECORD\",\"TOTAL\",\"{formattedCustomerTotal}\""
                );
            }
            File.WriteAllLines(outputFile, outputLines);

            File.Copy(inputFile, Path.Combine(backupPath, "InputData.txt"), true);
            File.Copy(outputFile, Path.Combine(backupPath, "OutputData.txt"), true);

            Console.WriteLine("Proceso completado exitosamente.");
        }
        static string GetCode(decimal amount)
        {
            if (amount < 500) return "N";
            if (amount > 500 && amount < 1000) return "A";
            if (amount > 1000 && amount < 1500) return "C";
            if (amount > 1500 && amount < 2000) return "L";
            if (amount > 2000 && amount < 2500) return "P";
            if (amount > 2500 && amount < 3000) return "X";
            if (amount > 3000 && amount < 5000) return "T";
            if (amount > 5000 && amount < 10000) return "S";
            if (amount > 10000 && amount < 20000) return "U";
            if (amount > 20000 && amount < 30000) return "R";
            if (amount > 30000) return "V";
            return "N";
        }
    }
}
