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

using System.Data.Entity.ModelConfiguration;
using PAI.FRATIS.SFL.Domain.Information;

namespace PAI.FRATIS.SFL.Data.Mappings
{
    /// <summary>The weather city map.</summary>
    public class QueueDeviceSegmentMap : EntityTypeConfiguration<QueueDeviceSegment>
    {
        public QueueDeviceSegmentMap()
        {
            Property(p => p.Device1Identifier).HasMaxLength(25);
            Property(p => p.Device2Identifier).HasMaxLength(25);
            Ignore(p => p.TimeSegmentStart);
            Ignore(p => p.TimeSegmentEnd);
        }
    }
}
