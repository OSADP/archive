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

using PAI.FRATIS.SFL.Domain;
using PAI.Drayage.Optimization.Model;


using ModelBase = PAI.Drayage.Optimization.Model.ModelBase;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Mapping
{
    
    public class DomainToModelValueInjection : LoopValueInjection, IValueInjection
    {
        public bool EnableCollectionInjection { get; set; }
        public bool EnableNoNullsInjection { get; set; }

        public DomainToModelValueInjection()
        {
            EnableCollectionInjection = true;
            EnableNoNullsInjection = true;
        }

        protected override bool TypesMatch(Type sourceType, Type targetType)
        {
            var result = base.TypesMatch(sourceType, targetType);

            if (!result)
            {
                if (sourceType.IsSubclassOf(typeof (EntityBase)) && targetType.IsSubclassOf(typeof (ModelBase)) ||
                    sourceType.IsSubclassOf(typeof (ModelBase)) && targetType.IsSubclassOf(typeof (EntityBase)))
                {
                    result = true;
                }
            }

            if (EnableCollectionInjection && !result)
            {
                if (targetType.IsGenericType &&
                    targetType.GetGenericTypeDefinition() != null &&
                    targetType.GetGenericTypeDefinition().GetInterfaces()
                              .Contains(typeof(IEnumerable)) &&
                    sourceType.IsGenericType &&
                    sourceType.GetGenericTypeDefinition() != null &&
                    sourceType.GetGenericTypeDefinition().GetInterfaces()
                              .Contains(typeof(IEnumerable)))
                {
                    result = true;
                }
            }

            if (EnableNoNullsInjection && !result)
            {
                var snt = Nullable.GetUnderlyingType(sourceType);
                var tnt = Nullable.GetUnderlyingType(targetType);

                result = sourceType == targetType
                          || sourceType == tnt
                          || targetType == snt
                          || (snt == tnt && snt != null);
            }

            return result;
        }

        protected override object SetValue(object sourcePropertyValue)
        {

            if (this.SourcePropType.IsSubclassOf(typeof(EntityBase)) && TargetPropType.IsSubclassOf(typeof(ModelBase)) ||
                this.SourcePropType.IsSubclassOf(typeof(ModelBase)) && TargetPropType.IsSubclassOf(typeof(EntityBase)))
            {
                if (sourcePropertyValue != null)
                {
                    var target = Activator.CreateInstance(TargetPropType);

                    target.InjectFrom<DomainToModelValueInjection>(sourcePropertyValue);

                    return target;
                }

                return null;
            }

            if (EnableCollectionInjection)
            {
                if (TargetPropType.IsGenericType &&
                   TargetPropType.GetGenericTypeDefinition() != null &&
                   TargetPropType.GetGenericTypeDefinition().GetInterfaces()
                                 .Contains(typeof(IEnumerable)) &&
                   SourcePropType.IsGenericType &&
                   SourcePropType.GetGenericTypeDefinition() != null &&
                   SourcePropType.GetGenericTypeDefinition().GetInterfaces()
                                 .Contains(typeof(IEnumerable)))
                {
                    var t = TargetPropType.GetGenericArguments()[0];
                    var tlist = typeof(List<>).MakeGenericType(t);
                    var addMethod = tlist.GetMethod("Add");

                    var sourceItems = sourcePropertyValue as IEnumerable;

                    var sourceT = SourcePropType.GetGenericArguments()[0];
                    if (sourceItems != null && typeof(ISortableEntity).IsAssignableFrom(sourceT))
                    {
                        sourceItems = sourceItems.Cast<ISortableEntity>().OrderBy(f => f.SortOrder);
                    }

                    if (sourceItems != null)
                    {
                        var target = Activator.CreateInstance(tlist);

                        foreach (var sourceItem in sourceItems)
                        {
                            var e = Activator.CreateInstance(t);

                            e.InjectFrom<DomainToModelValueInjection>(sourceItem);

                            addMethod.Invoke(target, new[] { e });
                        }

                        return target;
                    }
                }
            }


            return base.SetValue(sourcePropertyValue);
        }

        protected override bool AllowSetValue(object value)
        {
            if (EnableNoNullsInjection)
            {
                if (value == null)
                    return false;

                return !(value is DateTime) || (DateTime)value != default(DateTime);
            }

            return base.AllowSetValue(value);
        }
    }  
}