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
using System.Collections.ObjectModel;
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Equipment
{
    public interface IContainerService : IEntityServiceBase<Container>, IInstallableEntity
    {
        IQueryable<Container> GetContainers(int subscriberId, bool? isDomestic = null);

        Container GetByIdWithAll(int id);
    }

    public class ContainerService : EntityServiceBase<Container>, IContainerService
    {
        private readonly IChassisService _chassisService;

        public ContainerService(IRepository<Container> repository, ICacheManager cacheManager, IChassisService chassisService)
            : base(repository, cacheManager)
        {
            _chassisService = chassisService;
        }

        public IQueryable<Container> GetContainers(int subscriberId, bool? isDomestic = null)
        {
            var query = SelectWithAll().Where(p => p.SubscriberId == subscriberId);
            if (isDomestic.HasValue)
            {
                query = query.Where(p => p.IsDomestic == isDomestic.Value);
            }
            return query.OrderBy(p => p.DisplayName);
        }

        public void RemoveChassis(Container container, Chassis chassisToRemove, bool saveNow = true)
        {
            if (container != null && chassisToRemove != null && chassisToRemove.Id > 0)
            {
                var chassisFound = container.AllowedChassis.FirstOrDefault(m => m.Id == chassisToRemove.Id);
                if (chassisFound != null)
                {
                    container.AllowedChassis.Remove(chassisFound);
                    InsertOrUpdate(container, saveNow);
                }
            }
        }

        public Container GetByIdWithAll(int id)
        {
            return SelectWithAll().FirstOrDefault(m => m.Id == id);
        }

        public IQueryable<Container> SelectWithAll(int subscriberId)
        {
            return _repository.SelectWith("AllowedChassis").Where(p => p.SubscriberId == subscriberId);
        }

        public void Install(int subscriberId)
        {
            var existingContainers = GetContainers(subscriberId);
            var existingChassis = _chassisService.GetChassis(subscriberId);
            var isChanged = false;

            if (existingChassis.Count == 0)
            {
                throw new Exception("No Chassis Exist - Chassis should be initialized before Containers");
            }

            foreach (var container in GetContainerList(subscriberId))
            {
                var x = existingContainers.FirstOrDefault(p => p.DisplayName.ToLower() == container.DisplayName.ToLower());
                if (x == null || x.Id == 0)
                {
                    // add new record
                    isChanged = true;
                    container.AllowedChassis = new List<Chassis>();
                    foreach (var chassisToAdd in
                        _chassisService.GetCompatibleChassisForContainer(subscriberId, container.DisplayName).ToList())
                    {
                        container.AllowedChassis.Add(chassisToAdd);
                    }
                    Insert(container, false);
                }
                else
                {
                    if (x.AllowedChassis != null)
                    {
                        x.AllowedChassis.Clear();
                    }
                    else
                    {
                        x.AllowedChassis = new Collection<Chassis>();
                    }

                    foreach (var chassisToAdd in _chassisService.GetCompatibleChassisForContainer(subscriberId, container.DisplayName).ToList())
                    {
                        x.AllowedChassis.Add(chassisToAdd);
                    }
                    Update(x, false);
                }
            }

            if (isChanged)
            {
                _repository.SaveChanges();
            }
        }


        private IEnumerable<Container> GetContainerList(int subscriberId)
        {
            var containers = new List<Container>
                {
                    new Container()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Container 20",
                            Enabled = true,
                            IsDomestic = false,
                        },
                    new Container()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Container 40",
                            Enabled = true,
                            IsDomestic = false
                        },
                    new Container()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Container 40 HC",
                            Enabled = true,
                            IsDomestic = false,
                        },
                    new Container()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = "Container 45 HC",
                            Enabled = true,
                            IsDomestic = false,
                        },
                    new Container()
                    {
                        SubscriberId = subscriberId,
                        DisplayName = "Container 48",
                        Enabled = true,
                        IsDomestic = true
                    },
                    new Container()
                    {
                        SubscriberId = subscriberId,
                        DisplayName = "Container 53",
                        Enabled = true,
                        IsDomestic = true
                    },
                };

            return containers;
        }

    }
}