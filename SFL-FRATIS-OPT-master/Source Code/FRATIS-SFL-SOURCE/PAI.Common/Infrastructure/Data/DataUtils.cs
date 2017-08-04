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

using System.Collections;
using System.Data;
using System.Linq;
using System;

namespace PAI.FRATIS.SFL.Common.Infrastructure.Data
{
    public static class DataUtils
    {
        public static T ToFields<T>(this DataRow row) where T : class, new()
        {
            T target = new T();
            AssignDataRowToFields(row, target);
            return target;
        }

        public static void ToFields<T>(this DataRow row, T target)
        {
            AssignDataRowToFields(row, target);
        }
        

        /// <summary>
        /// Initializes a DataTable for the referenced Target
        /// Columns are created for the DataTable based on
        /// Fields of the Target, via Reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="target"></param>
        public static void InitializeFor<T>(this DataTable table, T target)
            where T : class
        {
            table = new DataTable();

            if (target is IEnumerable)
            {
                var lst = target as IEnumerable;
                int count = 0;
                foreach (object o in lst)
                {
                    if (count == 0)
                    {
                        var type = o.GetType();
                        foreach (var field in type.GetFields())
                        {
                            table.Columns.Add(field.Name);
                        }
                    }
                    break;
                }                
            }
        }

        /// <summary>
        /// Initializes a DataTable with columns for the Fields of the Target,
        /// and uses reflection to copy Field Values to the resulting Rows
        /// created within the DataTable
        /// 
        /// Used for export functions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="target"></param>
        public static void FillWith<T>(this DataTable table, T target)
            where T : class
        {
            table.InitializeFor(target);
            if (target is IEnumerable)
            {
                var lst = target as IEnumerable;
                foreach (T o in lst)
                {
                    table.Rows.Add();
                    table.Rows[table.Rows.Count - 1].FillWith(o);                    
                }
            }

        }

        /// <summary>
        /// Using reflection, adds the Field values of the Target
        /// to the referenced DataRow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="target"></param>
        public static void FillWith<T>(this DataRow row, T target)
            where T : class
        {
            var type = target.GetType();
            if (type.GetFields().Count() != row.ItemArray.Count())
                throw new Exception("Column Count Mismatch between DataRow and Object Field Count");

            int column = 0;
            foreach (var field in type.GetFields())
            {
                row[column] = field.GetValue(target).ToString();
                column++;
            }
        }
       
        public static void AssignDataRowToFields<T>(DataRow row, T target)
        {
            var type = target.GetType();
            int column = 0;

            try
            {
                foreach (var field in type.GetFields())
                {
                    object value = null;

                    if (field.FieldType == typeof(string))
                    {
                        value = row[column].ToString();
                    }

                    if (field.FieldType == typeof(int))
                    {
                        value = int.Parse(row[column].ToString());
                    }

                    if (field.FieldType == typeof(double))
                    {
                        value = double.Parse(row[column].ToString());
                    }

                    field.SetValue(target, value);

                    column++;
                }
            }
            catch (Exception ex)
            {
                // eh, just swallow it   
            }

            

        }
    }
}