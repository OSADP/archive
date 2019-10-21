//    Copyright 2013 Productivity Apex Inc.
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
using System.Linq;

namespace PAI.CTIP.Optimization.Model
{
    public abstract class ModelBase
    {
        /// <summary>
        /// Gets or sets the Identifier for this entity
        /// </summary>
        public virtual int Id { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ModelBase);
        }

        public virtual bool Equals(ModelBase other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(Id, other.Id))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Equals(Id, default(int)) ? base.GetHashCode() : Id.GetHashCode();
        }

        public static bool operator ==(ModelBase x, ModelBase y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(ModelBase x, ModelBase y)
        {
            return !(x == y);
        }

        private static bool IsTransient(ModelBase obj)
        {
            return obj != null && Equals(obj.Id, default(int));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }   

    }
}