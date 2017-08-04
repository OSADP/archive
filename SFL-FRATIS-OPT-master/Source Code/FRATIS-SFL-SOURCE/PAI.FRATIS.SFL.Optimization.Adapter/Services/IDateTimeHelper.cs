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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using PAI.Drayage.Optimization.Model;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Services;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    public interface IOptimizationDateTimeHelper
    {
        void UpdateDateTimeToLocal(object o);

        void UpdateDateTimeToUtc(object o);

        void UpdateDateTimeToUtc<T>(IList<T> items) where T : class, new();

        void UpdateDateTimeToLocal<T>(IList<T> items) where T : class, new();
    }

    public class OptimizationDateTimeHelper : IOptimizationDateTimeHelper
    {
        public IDateTimeHelper _dateTimeHelper;

        public OptimizationDateTimeHelper(IDateTimeHelper dateTimeHelper)
        {
            _dateTimeHelper = dateTimeHelper;
        }

        public void UpdateDateTimeToLocal(object o)
        {
            if (o == null) return;

            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                Type t = p.PropertyType;
                if (t == typeof(DateTime))
                {
                    p.SetValue(o, _dateTimeHelper.ConvertUtcToLocalTime((DateTime)p.GetValue(o)));
                }
                else if (t == typeof(DateTime?))
                {
                    var existingValue = (DateTime?)p.GetValue(o);
                    if (existingValue.HasValue)
                    {
                        p.SetValue(o, _dateTimeHelper.ConvertUtcToLocalTime(existingValue.Value));
                    }
                }
                else if (t.IsGenericType && t.GetGenericTypeDefinition() != null
                    && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable))
                    && t.IsGenericType && t.GetGenericTypeDefinition() != null
                    && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var listItems = p.GetValue(o);
                    if (listItems != null)
                    {
                        var listItem = p.GetValue(o);
                        if (listItem != null)
                        {
                            foreach (var oo in (IEnumerable<object>)listItem)
                            {
                                UpdateDateTimeToLocal(oo);
                            }
                        }
                    }
                }
                else if (t.IsSubclassOf(typeof(EntityBase)) || t.IsSubclassOf(typeof(ModelBase)))
                {
                    if (t == typeof(Drayage.Optimization.Model.Orders.RouteStop))
                    {
                        UpdateDateTimeToLocal(p.GetValue(o));
                    }
                }
            }
        }

        public void UpdateDateTimeToUtc(object o)
        {
            if (o == null) return;

            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                var t = p.PropertyType;
                if (t == typeof(DateTime))
                {
                    p.SetValue(o, _dateTimeHelper.ConvertLocalToUtcTime((DateTime)p.GetValue(o)));
                }
                else if (t == typeof(DateTime?))
                {
                    var existingValue = (DateTime?)p.GetValue(o);
                    if (existingValue.HasValue)
                    {
                        p.SetValue(o, _dateTimeHelper.ConvertLocalToUtcTime(existingValue.Value));
                    }
                }
                else if (t.IsGenericType && t.GetGenericTypeDefinition() != null
                    && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable))
                    && t.IsGenericType && t.GetGenericTypeDefinition() != null
                    && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var listItems = p.GetValue(o);
                    if (listItems != null)
                    {
                        var listItem = p.GetValue(o);
                        if (listItem != null)
                        {
                            foreach (var oo in (IEnumerable<object>)listItem)
                            {
                                UpdateDateTimeToLocal(oo);
                            }
                        }
                    }
                }
                else if (t.IsSubclassOf(typeof(EntityBase)) || t.IsSubclassOf(typeof(ModelBase)))
                {
                    UpdateDateTimeToLocal(p.GetValue(o));
                }
            }
        }

        public void UpdateDateTimeToUtc<T>(IList<T> items) where T : class, new()
        {
            if (items != null && items.Any())
            {
                var item = items.FirstOrDefault();
                foreach (PropertyInfo p in item.GetType().GetProperties())
                {
                    var t = p.PropertyType;
                    if (t == typeof(DateTime))
                    {
                        foreach (var i in items)
                        {
                            p.SetValue(p, _dateTimeHelper.ConvertLocalToUtcTime((DateTime)p.GetValue(i)));
                        }
                    }
                    else if (t == typeof(DateTime?))
                    {
                        foreach (var i in items)
                        {
                            var existingValue = (DateTime?)p.GetValue(i);
                            if (existingValue.HasValue)
                            {
                                p.SetValue(p, _dateTimeHelper.ConvertLocalToUtcTime(existingValue.Value));
                            }
                        }
                    }
                    else if (t.IsGenericType && t.GetGenericTypeDefinition() != null
                        && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable))
                        && t.IsGenericType && t.GetGenericTypeDefinition() != null
                        && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        foreach (var o in items)
                        {
                            var listItems = p.GetValue(o);
                            if (listItems != null)
                            {
                                var listItem = p.GetValue(o);
                                if (listItem != null)
                                {
                                    foreach (var oo in (IEnumerable<object>)listItem)
                                    {
                                        UpdateDateTimeToLocal(oo);
                                    }
                                }
                            }
                        }
                    }
                    else if (t.IsSubclassOf(typeof(EntityBase)) || t.IsSubclassOf(typeof(ModelBase)))
                    {
                        foreach (var o in items)
                        {
                            UpdateDateTimeToLocal(p.GetValue(o));
                        }
                    }
                }
            }
        }

        public void UpdateDateTimeToLocal<T>(IList<T> items) where T : class, new()
        {
            if (items != null && items.Any())
            {
                var item = items.FirstOrDefault();
                foreach (PropertyInfo p in item.GetType().GetProperties())
                {
                    var t = p.PropertyType;
                    if (p.PropertyType == typeof(DateTime))
                    {
                        foreach (var i in items)
                        {
                            p.SetValue(p, _dateTimeHelper.ConvertUtcToLocalTime((DateTime)p.GetValue(i)));
                        }
                    }
                    else if (p.PropertyType == typeof(DateTime?))
                    {
                        foreach (var i in items)
                        {
                            var existingValue = (DateTime?)p.GetValue(i);
                            if (existingValue.HasValue)
                            {
                                p.SetValue(p, _dateTimeHelper.ConvertUtcToLocalTime(existingValue.Value));
                            }
                        }
                    }
                    else if (t.IsGenericType && t.GetGenericTypeDefinition() != null
                        && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable))
                        && t.IsGenericType && t.GetGenericTypeDefinition() != null
                        && t.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        foreach (var o in items)
                        {
                            var listItems = p.GetValue(o);
                            if (listItems != null)
                            {
                                var listItem = p.GetValue(o);
                                if (listItem != null)
                                {
                                    foreach (var oo in (IEnumerable<object>)listItem)
                                    {
                                        UpdateDateTimeToLocal(oo);
                                    }
                                }
                            }
                        }
                    }
                    else if (t.IsSubclassOf(typeof(EntityBase)) || t.IsSubclassOf(typeof(ModelBase)))
                    {
                        foreach (var o in items)
                        {
                            UpdateDateTimeToLocal(p.GetValue(o));
                        }
                    }
                }
            }
        }


    }
}
