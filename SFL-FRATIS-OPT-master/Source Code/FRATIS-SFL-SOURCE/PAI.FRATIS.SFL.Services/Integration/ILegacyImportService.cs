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
using System.Data.Entity.Migrations.Model;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PAI.FRATIS.SFL.Domain.Geography;

namespace PAI.FRATIS.SFL.Services.Integration
{
    public interface ILegacyImportService
    {
        void ImportDrivers(string filePath, int subscriberId, int defaultLocationId = 0);

        ImportJobResult Process(string filePath, int subscriberId, bool syncLocations, bool syncJobs);

        ImportJobResult Process(string filePath, int subscriberId, bool syncLocations, bool syncJobs, bool saveToDatabase);

        //void ImportLocations(string filePath);

        //ImportJobResult ImportJobs(string filePath, int subscriberId);

        //IList<Location> SyncLocations(ImportJobResult importJobResult, int subscriberId);
    }
}
