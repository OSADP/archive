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

using System.Collections.Generic;

namespace PAI.FRATIS.SFL.Services.Integration
{
    /// <summary>
    /// Represents imported columns and values from a CSV/XLS/XLSX import
    /// </summary>
    public class ImportResult
    {
        #region Public Properties

        public int ColumnCount { get; set; }

        public string[] Columns { get; set; }

        public string StatusMessage { get; set; }

        public IList<string[]> Values { get; set; }

        private Dictionary<string, int> _columnIndex = null;

        public Dictionary<string, int> ColumnIndex
        {
            get
            {
                if (_columnIndex == null)
                {
                    _columnIndex = new Dictionary<string, int>();
                    for (int i = 0; i < Columns.Length; i++)
                    {

                        _columnIndex[Columns[i].ToLower()] = i;
                        //_columnIndex.Add(Columns[i].ToLower(), i);
                    }
                }
                return _columnIndex;
            }
        }
 
        #endregion

        #region Methods

        public int GetColumnIndex(string name)
        {
            int result = -1;
            ColumnIndex.TryGetValue(name.ToLower(), out result);
            return result;
        }


        #endregion
    }
}