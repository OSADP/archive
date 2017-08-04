//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using DataStreams.Xls;
using DataStreams.Xlsx;

namespace PAI.FRATIS.SFL.Services.Integration
{
    public abstract class ImportServiceBase : IImportService
    {
        #region Public Methods and Operators

        public FileInfo GetFileInfo(string filePath)
        {
            return new FileInfo(filePath);
        }

        public ImportResult Read(string filePath, int sheetIndex = 0, int maxRecords = 0)
        {
            return this.Read(this.GetFileInfo(filePath), sheetIndex, maxRecords);
        }

        public ImportResult Read(FileInfo finfo, int sheetIndex = 0, int maxRecords = 0)
        {
            var result = new ImportResult();

            if (finfo.Exists == false)
            {
                result.StatusMessage = "File does not exist";
            }
            else if (finfo.Extension.ToLower().EndsWith("xls"))
            {
                result = this.ReadXls(finfo.FullName, sheetIndex, maxRecords); // xls
            }
            else if (finfo.Extension.ToLower().EndsWith("xlsx"))
            {
                result = this.ReadXlsx(finfo.FullName, sheetIndex, maxRecords); // xlsx
            }
            else //if (finfo.Extension.ToLower().EndsWith("csv") || finfo.Extension.ToLower().EndsWith("txt"))
            {
                result = this.ReadCsv(finfo.FullName); // csv plaintext
            }
            //else
            //{
            //    result.StatusMessage = "Valid XLS or XLSX spreadsheet file not detected.";
            //}
            return result;
        }

        public bool VerifyFileExists(string filePath)
        {
            return this.GetFileInfo(filePath).Exists;
        }

        #endregion

        #region Methods

        public ImportResult ReadCsv(string filePath, char delimeter = ',')
        {
            var result = new ImportResult()
            {
                Values = new List<string[]>()
            };
            
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                var lineContents = new List<string>();
                var items = line.Split(delimeter);
                var isOpenQuote = false;
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (!isOpenQuote)
                    {
                        // see if line contains open quote
                        var quoteIndex = item.IndexOf('\"');
                        if (quoteIndex != -1)
                        {
                            var endQuoteIndex = item.IndexOf('\"', quoteIndex + 1);
                            if (endQuoteIndex != -1)
                            {
                                // end quote found, just add line - no delimterer found
                                lineContents.Add(item.Replace("\"", ""));
                                continue;
                            }

                            isOpenQuote = true;
                        }

                        lineContents.Add(item.Replace("\"", ""));
                    }
                    else
                    {
                        // open quote, check for close
                        var endQuoteIndex = item.IndexOf('\"');
                        if (endQuoteIndex >= 0)
                        {
                            isOpenQuote = false;
                            item = item.Replace("\"", "");
                        }

                        //append to last line
                        lineContents[lineContents.Count - 1] = lineContents[lineContents.Count - 1] + item;
                    }
                }

                for (int q = 0; q < lineContents.Count; q++)
                {
                    lineContents[q] = lineContents[q].Trim();
                }

                result.Values.Add(lineContents.ToArray());
            }
            return result;
        }

        private ImportResult ReadXls(string filePath, int sheetIndex = 0, int maxRows = 0)
        {
            var result = new ImportResult();

            IList<string[]> lstValues = new List<string[]>();
            try
            {
                var reader = new XlsReader(filePath) { CurrentSheet = sheetIndex };
                reader.ReadRecord();

                for (int i = 0; i < reader.RecordCount && (maxRows <= 0 || i < maxRows); i++)
                {
                    bool record = reader.ReadRecord();
                    lstValues.Add(reader.Values);
                }

                result.StatusMessage = string.Format(
                    "Operation completed on {0} record(s).  {1} Columns Detected",
                    reader.RecordCount,
                    reader.ColumnCount);
            }
            catch (Exception ex)
            {
                result.StatusMessage = string.Format("Exception thrown: {0}", ex.Message);
            }

            result.Values = lstValues;
            return result;
        }

        private ImportResult ReadXlsx(string filePath, int sheetIndex = 0, int maxRows = 0, bool headerRow = true)
        {
            var result = new ImportResult();

            IList<string[]> lstValues = new List<string[]>();
            try
            {
                var reader = new XlsxReader(filePath) { CurrentSheet = sheetIndex };
                reader.ReadRecord();                    
                result.ColumnCount = reader.ColumnCount;

                if (headerRow && reader.RecordCount > 0)
                {
                    result.Columns = reader.Values;
                    

                    for (int i = 1; i < reader.RecordCount && (maxRows <= 0 || i-1 < maxRows); i++)
                    {
                        reader.ReadRecord(); 
                        lstValues.Add(reader.Values);
                    }

                }
                else if (!headerRow)
                {
                    for (int i = 0; i < reader.RecordCount && (maxRows <= 0 || i < maxRows); i++)
                    {
                        bool record = reader.ReadRecord();
                        lstValues.Add(reader.Values);
                    }
                }

                result.StatusMessage = string.Format(
                    "Operation completed on {0} record(s).  {1} Columns Detected",
                    reader.RecordCount,
                    reader.ColumnCount);
            }
            catch (Exception ex)
            {
                result.StatusMessage = string.Format("Exception thrown: {0}", ex.Message);
            }

            result.Values = lstValues;
            return result;
        }

        #endregion
    }
}