using System;
using System.Collections.Generic;
using System.Linq;

using System.Globalization;
using System.IO;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;


namespace Landis.Extension.SOSIELHuman.Helpers
{
    class EnumerableConverter<T> : DefaultTypeConverter
    {
        public override string ConvertToString(TypeConverterOptions options, object value)
        {
            IEnumerable<T> values = value as IEnumerable<T>;

            return string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, values);
        }
    }



    public static class WriteToCSVHelper
    {
        private static CsvConfiguration configuration = new CsvConfiguration() { CultureInfo = CultureInfo.CurrentCulture, HasHeaderRecord = false };


        static WriteToCSVHelper()
        {
            TypeConverterFactory.AddConverter<string[]>(new EnumerableConverter<string>());
        }



        /// <summary>
        /// Appends record to the file end or creates new file and writes record there.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="record"></param>
        public static void AppendTo<T>(string filePath, T record)
        {
            bool isFileExist = File.Exists(filePath);

            using (FileStream fs = File.Open(filePath, FileMode.Append))
            using (StreamWriter sw = new StreamWriter(fs))
            using (CsvWriter csv = new CsvWriter(sw, configuration))
            {
                if (!isFileExist)
                {
                    configuration.HasHeaderRecord = true;
                    csv.WriteHeader<T>();
                    configuration.HasHeaderRecord = false;
                }

                csv.WriteRecord(record);
            }
        }


        /// <summary>
        /// Appends records to the file end or creates new file and writes records there.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="records"></param>
        public static void AppendTo<T>(string filePath, IEnumerable<T> records)
        {
            bool isFileExist = File.Exists(filePath);

            using (FileStream fs = File.Open(filePath, FileMode.Append))
            using (StreamWriter sw = new StreamWriter(fs))
            using (CsvWriter csv = new CsvWriter(sw, configuration))
            {
                //it writes header by default
                if (!isFileExist)
                {
                    configuration.HasHeaderRecord = true;
                }
                else
                {
                    configuration.HasHeaderRecord = false;
                }

                csv.WriteRecords(records);
            }
        }


    }
}
