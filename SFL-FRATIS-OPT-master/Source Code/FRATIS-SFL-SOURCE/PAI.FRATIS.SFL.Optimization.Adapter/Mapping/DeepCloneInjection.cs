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

using Omu.ValueInjecter;

using PAI.FRATIS.SFL.Optimization.Adapter.Mapping.SmartConvention;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Mapping
{
    public interface IInjectionFactory
    {
        IValueInjection GetDeepCloneInjection();
    }
    
    public class DeepCloneInjection : SmartConventionInjection
    {
        private readonly IInjectionFactory _injectionFactory;

        public DeepCloneInjection(IInjectionFactory injectionFactory)
        {
            _injectionFactory = injectionFactory;
        }
        
        protected override bool Match(SmartConventionInfo c)
        {
            return c.SourceProp.Name == c.TargetProp.Name;
        }

        protected override void ExecuteMatch(SmartMatchInfo mi)
        {
            var sourceVal = GetValue(mi.SourceProp, mi.Source);
            if (sourceVal == null) return;

            //for value types and string just return the value as is
            if (mi.SourceProp.PropertyType.IsValueType || mi.SourceProp.PropertyType == typeof(string))
            {
                SetValue(mi.TargetProp, mi.Target, sourceVal);
                return;
            }

            //handle arrays
            if (mi.SourceProp.PropertyType.IsArray)
            {
                var arr = sourceVal as Array;
                var arrayClone = arr.Clone() as Array;

                for (var index = 0; index < arr.Length; index++)
                {
                    var arriVal = arr.GetValue(index);
                    if (arriVal.GetType().IsValueType || arriVal.GetType() == typeof(string)) continue;

                    var injection = _injectionFactory.GetDeepCloneInjection();

                    arrayClone.SetValue(Activator.CreateInstance(arriVal.GetType()).InjectFrom(injection, arriVal), index);
                }
                SetValue(mi.TargetProp, mi.Target, arrayClone);
                return;
            }

            if (mi.SourceProp.PropertyType.IsGenericType)
            {
                //handle IEnumerable<> also ICollection<> IList<> List<>
                if (mi.SourceProp.PropertyType.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var genericArgument = mi.TargetProp.PropertyType.GetGenericArguments()[0];

                    var tlist = typeof(List<>).MakeGenericType(genericArgument);

                    var list = Activator.CreateInstance(tlist);

                    if (genericArgument.IsValueType || genericArgument == typeof(string))
                    {
                        var addRange = tlist.GetMethod("AddRange");
                        addRange.Invoke(list, new[] { sourceVal });
                    }
                    else
                    {
                        var addMethod = tlist.GetMethod("Add");
                        foreach (var o in sourceVal as IEnumerable)
                        {
                            var injection = _injectionFactory.GetDeepCloneInjection();
                            addMethod.Invoke(list, new[] { Activator.CreateInstance(genericArgument).InjectFrom(injection, o) });
                        }
                    }
                    SetValue(mi.TargetProp, mi.Target, list);
                    return;
                }

                throw new NotImplementedException(string.Format("deep clonning for generic type {0} is not implemented", mi.SourceProp.Name));
            }

            //for simple object types create a new instace and apply the clone injection on it
            var injection1 = _injectionFactory.GetDeepCloneInjection();
            SetValue(mi.TargetProp, mi.Target, Activator.CreateInstance(mi.TargetProp.PropertyType).InjectFrom(injection1, sourceVal));
        }
    }
}