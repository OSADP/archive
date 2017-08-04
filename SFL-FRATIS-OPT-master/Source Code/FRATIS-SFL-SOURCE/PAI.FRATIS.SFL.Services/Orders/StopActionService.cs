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
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Orders
{
    public interface IStopActionService : IEntityServiceBase<StopAction>, IInstallableEntity
    {
        ICollection<StopAction> GetStopActions();
        StopAction GetByShortName(string shortName);
        ICollection<StopAction> StopActions { get; }
    }

    public class StopActionService : EntityServiceBase<StopAction>, IStopActionService
    {
        public StopActionService(IRepository<StopAction> repository, ICacheManager cacheManager)
            : base(repository, cacheManager)
        {
        }

        public ICollection<StopAction> GetStopActions()
        {
            return InternalSelect().OrderBy(m => m.Name).ToList();
        }

        public StopAction GetByShortName(string shortName)
        {
            return Select().FirstOrDefault(f => f.ShortName == shortName);
        }

        private ICollection<StopAction> _stopActions = null;
        public ICollection<StopAction> StopActions
        {
            get
            {
                return _stopActions ?? (_stopActions = InternalSelect().ToList());
            }
        }

        public void Install(int subscriberId = 0)
        {
            var stopActions = new List<StopAction>
                {
                    //new StopAction() {Id = 0, Name = "No Action", ShortName = "NA"},
                    //new StopAction() {Id = 1, Name = "Pickup Chassis", ShortName = "PC", },
                    //new StopAction() {Id = 2, Name = "Drop Off Chassis", ShortName = "DC", },
                    //new StopAction() {Id = 3, Name = "Pickup Empty", ShortName = "PE", },
                    //new StopAction() {Id = 4, Name = "Drop Off Empty", ShortName = "DE", },
                    //new StopAction() {Id = 5, Name = "Pickup Loaded", ShortName = "PL", },
                    //new StopAction() {Id = 6, Name = "Drop Off Loaded", ShortName = "DL", },
                    //new StopAction() {Id = 7, Name = "Pickup Empty With Chassis", ShortName = "PEWC", },
                    //new StopAction() {Id = 8, Name = "Drop Off Empty With Chassis", ShortName = "DEWC", },
                    //new StopAction() {Id = 9, Name = "Pickup Loaded With Chassis", ShortName = "PLWC", },
                    //new StopAction() {Id = 10, Name = "Drop Off Loaded With Chassis", ShortName = "DLWC", },
                    //new StopAction() {Id = 11, Name = "Live Loading", ShortName = "LL", },
                    //new StopAction() {Id = 12, Name = "Live Unloading", ShortName = "LU", }
                };

            var existingStopActions = Select().OrderBy(p => p.Id);
            foreach (var stopAction in stopActions)
            {
                var existingAction = existingStopActions.FirstOrDefault(p => p.ShortName == stopAction.ShortName);
                if (existingAction == null)
                {
                    Insert(stopAction);
                }
            }

            // delete duplicates
            var hsStopActions = new HashSet<string>();
            foreach (var stopAction in stopActions)
            {
                if (hsStopActions.Contains(stopAction.ShortName))
                {
                    Delete(stopAction, false);
                }
                else
                {
                    hsStopActions.Add(stopAction.ShortName);
                }
            }

            SaveChanges();
        }
    }
}