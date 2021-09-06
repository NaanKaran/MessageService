using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MessageService.InfraStructure.Helpers
{
    public static class ExtensionHelper
    {

        public static T ToEnum<T>(this string value)
        {
            var data = Enum.TryParse(typeof(T), value, true, out object result);
            if (result == null)
            {
                return default(T);
            }
            return (T)result;
        }

        public static int ToInt(this string value)
        {
            int.TryParse(value, out var result);
            return result;
        }

        public static long ToLong(this string value)
        {
            long.TryParse(value, out var result);
            return result;
        }
        public static string ToJsonString(this object obj)
        {
            return obj != null ? JsonConvert.SerializeObject(obj) : string.Empty;
        }

        public static DateTime ToChinaTime(this DateTime obj)
        {
            return obj.AddHours(8);
        }

        /// <summary>
        /// "yyyy-MM-dd HH:mm:ss tt" -- 2019-05-21 12:53:09 PM
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToDateTimeString(this DateTime obj)
        {
            return obj.ToString("yyyy-MM-dd hh:mm:ss tt");
        }

        /// <summary>
        /// "yyyy-MM-dd HH:mm:ss tt" -- 2019-05-21 12:53:09 PM
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToDateTimeString(this DateTime? obj)
        {
            return obj?.ToString("yyyy-MM-dd hh:mm:ss tt");
        }
        /// <summary>
        /// like: JAN_2019-MAR_2019
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetQuadrantMonthInfo(this DateTime obj)
        {

            var month = obj.Month;

            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return $@"JAN_{obj.Year}-MAR_{obj.Year}";
                case 4:
                case 5:
                case 6:
                    return $@"APR_{obj.Year}-JUN_{obj.Year}";
                case 7:
                case 8:
                case 9:
                    return $@"JUL_{obj.Year}-NOV_{obj.Year}";
                case 10:
                case 11:
                case 12:
                    return $@"OCT_{obj.Year}-DEC_{obj.Year}";
                default:
                    break;
            }

            return "";
        }
        /// <summary>
        /// like: 2019_Q1, 2019_Q2, 2019_Q3
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetQuadrantInfo(this DateTime obj)
        {

            var month = obj.Month;

            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return $@"{obj.Year}_Q1";
                case 4:
                case 5:
                case 6:
                    return $@"{obj.Year}_Q2";
                case 7:
                case 8:
                case 9:
                    return $@"{obj.Year}_Q3";
                case 10:
                case 11:
                case 12:
                    return $@"{obj.Year}_Q4";
                default:
                    break;
            }

            return "";
        }

        public static DateTime GetLastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }
        public static int GetQuadrantNumberInfo(this DateTime obj)
        {

            var month = obj.Month;

            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return 1;
                case 4:
                case 5:
                case 6:
                    return 2;
                case 7:
                case 8:
                case 9:
                    return 3;
                case 10:
                case 11:
                case 12:
                    return 4;
                default:
                    break;
            }

            return 0;
        }
        /// <summary>
        /// like: MMSLog_2019_Q1
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetQuadrantMMSLogTableName(this DateTime obj)
        {

            var month = obj.Month;

            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return $@"MMSLog_{obj.Year}_Q1";
                case 4:
                case 5:
                case 6:
                    return $@"MMSLog_{obj.Year}_Q2";
                case 7:
                case 8:
                case 9:
                    return $@"MMSLog_{obj.Year}_Q3";
                case 10:
                case 11:
                case 12:
                    return $@"MMSLog_{obj.Year}_Q4";
                default:
                    break;
            }

            return "";
        }


        /// <summary>
        /// like: SMSLog_2019_Q1
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetQuadrantSMSLogTableName(this DateTime obj)
        {

            var month = obj.Month;

            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return $@"SMSLog_{obj.Year}_Q1";
                case 4:
                case 5:
                case 6:
                    return $@"SMSLog_{obj.Year}_Q2";
                case 7:
                case 8:
                case 9:
                    return $@"SMSLog_{obj.Year}_Q3";
                case 10:
                case 11:
                case 12:
                    return $@"SMSLog_{obj.Year}_Q4";
                default:
                    break;
            }

            return "";
        }

        public static T ConvertToModel<T>(this string obj)
        {
            try
            {
                return obj != null ? JsonConvert.DeserializeObject<T>(obj) : default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // ignored
            }

            return default(T);
        }

        public static string ToFileExtension(this string obj)
        {
            var mapping = Registry.ClassesRoot.GetSubKeyNames()
                      .Select(key => new
                      {
                          Key = key,
                          ContentType = Registry.ClassesRoot.OpenSubKey(key).GetValue("Content Type")
                      })
                       .Where(x => x.ContentType != null)
                       .ToLookup(x => x.ContentType, x => x.Key);
            return mapping[obj].FirstOrDefault();
        }


        public static bool IsNullOrDefault(this int? obj)
        {
            return (obj == null || default(int) == obj);
        }

        public static bool IsNullOrDefault(this long? obj)
        {
            return (obj == null || default(long) == obj);
        }

        /// <summary>
        /// True when this has value (ie) its not equal to '0'
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNotDefault(this long obj)
        {
            return (default(long) != obj);
        }

        /// <summary>
        /// True when this has value (ie) its not equal to '0'
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNotDefault(this int obj)
        {
            return (default(int) != obj);
        }

        public static bool IsNullOrWhiteSpace(this string obj)
        {
            return string.IsNullOrWhiteSpace(obj);
        }
        /// <summary>
        /// indicate this string is not null, empty or whitespace
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNotNullOrWhiteSpace(this string obj)
        {
            return !string.IsNullOrWhiteSpace(obj);
        }


        public static Dictionary<string, string> ToDictionary(this JArray data)
        {
            if (data == null)
            {
                return new Dictionary<string, string>();
            }
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();

            foreach (JObject content in data.Children<JObject>())
            {
                foreach (JProperty prop in content.Properties())
                {
                    myDictionary.Add(prop.Name, prop.Value?.ToString());
                }
            }

            return myDictionary;
            //return data.ToDictionary(k => ((JObject) k).Properties().First().Name,
            //    v => v.Values().First().Value<string>());
        }

        public static Dictionary<string, string> ToDictionary(this JObject data)
        {
            if (data == null)
            {
                return new Dictionary<string, string>();
            }
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();

            foreach (var content in data)
            {
                myDictionary.Add(content.Key, content.Value?.ToString());
            }

            return myDictionary;
        }

        public static bool IsNullOrEmpty(this string obj)
        {
            return string.IsNullOrEmpty(obj);
        }

        public static bool IsNotNullOrEmpty(this string obj)
        {
            return !string.IsNullOrEmpty(obj);
        }

        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsNotNull(this object obj)
        {
            return obj != null;
        }

        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static XDocument RemoveCDATAFromXDocument(this XDocument xDocument)
        {
            var nodes = xDocument.DescendantNodes().OfType<XCData>().ToList();
            foreach (var node in nodes)
            {
                node.ReplaceWith(new XText(node.Value));
            }
            return xDocument;
        }

        /// <summary>
        /// Converts the timestamp(1357290913) to UTC date time(1/4/2013 9:15:13 AM)
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string timeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Convert.ToInt64(timeStamp));
            return dtDateTime;
        }

        public static string ToTimestamp(this DateTime value)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var elapsedTime = value - dateTime;
            return ((long)elapsedTime.TotalSeconds).ToString();
        }

        public static bool IsValidGuid(this string val)
        {
            var isValid = Guid.TryParseExact(val, "D", out var cc);

            return cc != new Guid() && isValid;
        }

        public static byte[] ToByteArray(this Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            ms.Position = 0;
            return ms.ToArray();
        }

        public static DataTable CreateDataTable<T>(this IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        //public static byte[] CreateExcelDocument<T>(this IEnumerable<T> list)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        ds.Tables.Add(ListToDataTable(list.ToList()));
        //        var strm = CreateExcelDocumentAsStream(ds);
        //        return strm;
        //    }
        //    catch 
        //    {
        //        //Trace.WriteLine("Failed, exception thrown: " + ex.Message);
        //        return null;
        //    }
        //}

        public static DataTable ListToDataTable<T>(List<T> list)
        {
            DataTable dt = new DataTable();

            foreach (PropertyInfo info in typeof(T).GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
            }
            foreach (T t in list)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo info in typeof(T).GetProperties())
                {
                    if (!IsNullableType(info.PropertyType))
                        row[info.Name] = info.GetValue(t, null);
                    else
                        row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
        public static string GetLast(this string source, int tailLength)
        {
            if (source == null) return source;
            if (tailLength >= source.Length)
                return source;
            return source.Substring(source.Length - tailLength);
        }
        private static Type GetNullableType(Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }
        private static bool IsNullableType(Type type)
        {
            return (type == typeof(string) ||
                    type.IsArray ||
                    (type.IsGenericType &&
                     type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        public static void ConvertAsStream(this DataSet ds, MemoryStream memoryStream)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook, true))
                {
                    WriteExcelFile(ds, document);
                }
                memoryStream.Position = 0;
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.ToString());
            }
        }

        public static void ConvertAsStream(this DataTable dt, MemoryStream memoryStream)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook, true))
                {
                    WriteExcelFile(dt, document);
                }

                memoryStream.Position = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
        {
            //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
            //  to a file, or writing to a MemoryStream.
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            Stylesheet stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;


            //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
            uint worksheetNumber = 1;
            Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            foreach (DataTable dt in ds.Tables)
            {
                //  For each worksheet you want to create
                string worksheetName = dt.TableName;

                //  Create worksheet part, and add it to the sheets collection in workbook
                WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                Sheet sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart), SheetId = worksheetNumber, Name = worksheetName };

                // If you want to define the Column Widths for a Worksheet, you need to do this *before* appending the SheetData
                // http://social.msdn.microsoft.com/Forums/en-US/oxmlsdk/thread/1d93eca8-2949-4d12-8dd9-15cc24128b10/

                sheets.Append(sheet);

                //  Append this worksheet's data to our Workbook, using OpenXmlWriter, to prevent memory problems
                WriteDataTableToExcelWorksheet(dt, newWorksheetPart);

                worksheetNumber++;
            }

            spreadsheet.WorkbookPart.Workbook.Save();
        }

        private static void WriteExcelFile(DataTable dt, SpreadsheetDocument spreadsheet)
        {
            //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
            //  to a file, or writing to a MemoryStream.
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

            //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
            spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

            //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
            WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            Stylesheet stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;

            //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
            uint worksheetNumber = 1;
            Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            //  For each worksheet you want to create
            string worksheetName = dt.TableName;

            //  Create worksheet part, and add it to the sheets collection in workbook
            WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
            Sheet sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart), SheetId = worksheetNumber, Name = worksheetName };

            sheets.Append(sheet);

            //  Append this worksheet's data to our Workbook, using OpenXmlWriter, to prevent memory problems
            WriteDataTableToExcelWorksheet(dt, newWorksheetPart);

            spreadsheet.WorkbookPart.Workbook.Save();
        }


        private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart)
        {
            OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart, Encoding.ASCII);
            writer.WriteStartElement(new Worksheet());
            writer.WriteStartElement(new SheetData());

            string cellValue = "";

            //  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
            //
            //  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
            //  cells of data, we'll know if to write Text values or Numeric cell values.
            int numberOfColumns = dt.Columns.Count;
            bool[] IsNumericColumn = new bool[numberOfColumns];
            bool[] IsDateColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            //
            //  Create the Header row in our Excel Worksheet
            //
            uint rowIndex = 1;

            writer.WriteStartElement(new Row { RowIndex = rowIndex });
            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, ref writer);
                IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32") || (col.DataType.FullName == "System.Double") || (col.DataType.FullName == "System.Single");
                IsDateColumn[colInx] = (col.DataType.FullName == "System.DateTime");
            }
            writer.WriteEndElement();   //  End of header "Row"

            //
            //  Now, step through each row of data in our DataTable...
            //
            double cellNumericValue = 0;
            foreach (DataRow dr in dt.Rows)
            {
                // ...create a new row, and append a set of this row's data to it.
                ++rowIndex;

                writer.WriteStartElement(new Row { RowIndex = rowIndex });

                for (int colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    cellValue = dr.ItemArray[colInx].ToString();
                    cellValue = ReplaceHexadecimalSymbols(cellValue);

                    // Create cell with data
                    if (IsNumericColumn[colInx])
                    {
                        //  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
                        //  If this numeric value is NULL, then don't write anything to the Excel file.
                        cellNumericValue = 0;
                        if (double.TryParse(cellValue, out cellNumericValue))
                        {
                            cellValue = cellNumericValue.ToString();
                            AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, ref writer);
                        }
                    }
                    else if (IsDateColumn[colInx])
                    {
                        //  This is a date value.
                        DateTime dtValue;
                        string strValue = "";
                        if (DateTime.TryParse(cellValue, out dtValue))
                            strValue = dtValue.ToShortDateString();
                        AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), strValue, ref writer);
                    }
                    else
                    {
                        //  For text cells, just write the input data straight out to the Excel file.
                        AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, ref writer);
                    }
                }
                writer.WriteEndElement(); //  End of Row
            }
            writer.WriteEndElement(); //  End of SheetData
            writer.WriteEndElement(); //  End of worksheet

            writer.Close();
        }

        private static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        private static void AppendTextCell(string cellReference, string cellStringValue, ref OpenXmlWriter writer)
        {
            //  Add a new Excel Cell to our Row 
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.String
            });
        }

        private static void AppendNumericCell(string cellReference, string cellStringValue, ref OpenXmlWriter writer)
        {
            //  Add a new Excel Cell to our Row 
            writer.WriteElement(new Cell
            {
                CellValue = new CellValue(cellStringValue),
                CellReference = cellReference,
                DataType = CellValues.Number
            });
        }

        public static string GetExcelColumnName(int columnIndex)
        {
            //  eg  (0) should return "A"
            //      (1) should return "B"
            //      (25) should return "Z"
            //      (26) should return "AA"
            //      (27) should return "AB"
            //      ..etc..
            char firstChar;
            char secondChar;
            char thirdChar;

            if (columnIndex < 26)
            {
                return ((char)('A' + columnIndex)).ToString();
            }

            if (columnIndex < 702)
            {
                firstChar = (char)('A' + (columnIndex / 26) - 1);
                secondChar = (char)('A' + (columnIndex % 26));

                return string.Format("{0}{1}", firstChar, secondChar);
            }

            int firstInt = columnIndex / 26 / 26;
            int secondInt = (columnIndex - firstInt * 26 * 26) / 26;
            if (secondInt == 0)
            {
                secondInt = 26;
                firstInt = firstInt - 1;
            }
            int thirdInt = (columnIndex - firstInt * 26 * 26 - secondInt * 26);

            firstChar = (char)('A' + firstInt - 1);
            secondChar = (char)('A' + secondInt - 1);
            thirdChar = (char)('A' + thirdInt);

            return string.Format("{0}{1}{2}", firstChar, secondChar, thirdChar);
        }

        public static string ToCSV(this DataTable table)
        {
            var result = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
            }

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(row[i].ToString());
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ",");
                }
            }
            string s = result.ToString(); s = s.TrimEnd(new char[] { '\r', '\n' });
            return s;

        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                var columnName = prop.DisplayName ?? prop.Name;
                table.Columns.Add(columnName, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    var columnName = prop.DisplayName ?? prop.Name;
                    row[columnName] = prop.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }
            return table;

        }
        public static DataTable ToDataTable<T>(this List<T> iList)
        {
            DataTable dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection =
                TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);


                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }
            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        public static byte[] ReadToEnd(this Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }


        public static StringBuilder ConvertDataTableToCsvFile(this DataTable dtData)
        {
            StringBuilder data = new StringBuilder();

            //Taking the column names.
            for (int column = 0; column < dtData.Columns.Count; column++)
            {
                //Making sure that end of the line, shoould not have comma delimiter.
                if (column == dtData.Columns.Count - 1)
                    data.Append(dtData.Columns[column].ColumnName.ToString().Replace(",", ";"));
                else
                    data.Append(dtData.Columns[column].ColumnName.ToString().Replace(",", ";") + ',');
            }

            data.Append(Environment.NewLine);//New line after appending columns.

            for (int row = 0; row < dtData.Rows.Count; row++)
            {
                for (int column = 0; column < dtData.Columns.Count; column++)
                {
                    ////Making sure that end of the line, shoould not have comma delimiter.
                    if (column == dtData.Columns.Count - 1)
                        data.Append(dtData.Rows[row][column].ToString().Replace(",", ";"));
                    else
                        data.Append(dtData.Rows[row][column].ToString().Replace(",", ";") + ',');
                }

                //Making sure that end of the file, should not have a new line.
                if (row != dtData.Rows.Count - 1)
                    data.Append(Environment.NewLine);
            }
            return data;
        }
    }

}
