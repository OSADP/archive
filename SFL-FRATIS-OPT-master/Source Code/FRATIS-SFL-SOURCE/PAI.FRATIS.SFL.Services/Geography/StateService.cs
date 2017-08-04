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
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;

namespace PAI.FRATIS.SFL.Services.Geography
{
    public interface IStateService : IEntityServiceBase<State>, IInstallableEntity
    {
        ICollection<State> GetStates();
        State GetByNameOrAbbreviation(string value);
    }

    public class StateService : EntityServiceBase<State>, IStateService
    {
        public StateService(IRepository<State> repository, ICacheManager cacheManager) : base(repository, cacheManager)
        {
        }

        public ICollection<State> GetStates()
        {
            return InternalSelect().Where(p => p.Id > 0).ToList();
        }

        public State GetByNameOrAbbreviation(string value)
        {
            return Select().FirstOrDefault(f => f.Abbreviation == value || f.Name == value);
        }

        public void Install(int subscriberId = 0)
        {
            var existingStates = GetStates();
            var isChanged = false;

            foreach (var state in GetStatesList())
            {
                var x = existingStates.FirstOrDefault(p => p.Name.ToLower() == state.Name.ToLower());
                if (x == null || x.Id == 0)
                {
                    // add new record
                    isChanged = true;
                    Insert(state, false);
                }
            }

            if (isChanged)
                _repository.SaveChanges();
        }

        private IEnumerable<State> GetStatesList(int subscriberId = 0)
        {
            var states = new List<State>
                {
                    new State()
                        {
                            Name = "Alabama",
                            Abbreviation = "AL",
                        },
                    new State()
                        {
                            Name = "Alaska",
                            Abbreviation = "AK",
                        },
                    new State()
                        {
                            Name = "Arizona",
                            Abbreviation = "AZ",
                        },
                    new State()
                        {
                            Name = "Arkansas",
                            Abbreviation = "AR",
                        },
                    new State()
                        {
                            Name = "California",
                            Abbreviation = "CA",
                        },
                    new State()
                        {
                            Name = "Colorado",
                            Abbreviation = "CO",
                        },
                    new State()
                        {
                            Name = "Connecticut",
                            Abbreviation = "CT",
                        },
                    new State()
                        {
                            Name = "Delaware",
                            Abbreviation = "DE",
                        },
                    new State()
                        {
                            Name = "District of Columbia",
                            Abbreviation = "DC",
                        },
                    new State()
                        {
                            Name = "Florida",
                            Abbreviation = "FL",
                        },
                    new State()
                        {
                            Name = "Georgia",
                            Abbreviation = "GA",
                        },
                    new State()
                        {
                            Name = "Hawaii",
                            Abbreviation = "HI",
                        },
                    new State()
                        {
                            Name = "Idaho",
                            Abbreviation = "ID",
                        },
                    new State()
                        {
                            Name = "Illinois",
                            Abbreviation = "IL",
                        },
                    new State()
                        {
                            Name = "Indiana",
                            Abbreviation = "IN",
                        },
                    new State()
                        {
                            Name = "Iowa",
                            Abbreviation = "IA",
                        },
                    new State()
                        {
                            Name = "Kansas",
                            Abbreviation = "KS",
                        },
                    new State()
                        {
                            Name = "Kentucky",
                            Abbreviation = "KY",
                        },
                    new State()
                        {
                            Name = "Louisiana",
                            Abbreviation = "LA",
                        },
                    new State()
                        {
                            Name = "Maine",
                            Abbreviation = "ME",
                        },
                    new State()
                        {
                            Name = "Maryland",
                            Abbreviation = "MD",
                        },
                    new State()
                        {
                            Name = "Massachusetts",
                            Abbreviation = "MA",
                        },
                    new State()
                        {
                            Name = "Michigan",
                            Abbreviation = "MI",
                        },
                    new State()
                        {
                            Name = "Minnesota",
                            Abbreviation = "MN",
                        },
                    new State()
                        {
                            Name = "Mississippi",
                            Abbreviation = "MS",
                        },
                    new State()
                        {
                            Name = "Missouri",
                            Abbreviation = "MO",
                        },
                    new State()
                        {
                            Name = "Montana",
                            Abbreviation = "MT",
                        },
                    new State()
                        {
                            Name = "Nebraska",
                            Abbreviation = "NE",
                        },
                    new State()
                        {
                            Name = "Nevada",
                            Abbreviation = "NV",
                        },
                    new State()
                        {
                            Name = "New Hampshire",
                            Abbreviation = "NH",
                        },
                    new State()
                        {
                            Name = "New Jersey",
                            Abbreviation = "NJ",
                        },
                    new State()
                        {
                            Name = "New Mexico",
                            Abbreviation = "NM",
                        },
                    new State()
                        {
                            Name = "New York",
                            Abbreviation = "NY",
                        },
                    new State()
                        {
                            Name = "North Carolina",
                            Abbreviation = "NC",
                        },
                    new State()
                        {
                            Name = "North Dakota",
                            Abbreviation = "ND",
                        },
                    new State()
                        {
                            Name = "Ohio",
                            Abbreviation = "OH",
                        },
                    new State()
                        {
                            Name = "Oklahoma",
                            Abbreviation = "OK",
                        },
                    new State()
                        {
                            Name = "Oregon",
                            Abbreviation = "OR",
                        },
                    new State()
                        {
                            Name = "Pennsylvania",
                            Abbreviation = "PA",
                        },
                    new State()
                        {
                            Name = "Rhode Island",
                            Abbreviation = "RI",
                        },
                    new State()
                        {
                            Name = "South Carolina",
                            Abbreviation = "SC",
                        },
                    new State()
                        {
                            Name = "South Dakota",
                            Abbreviation = "SD",
                        },
                    new State()
                        {
                            Name = "Tennessee",
                            Abbreviation = "TN",
                        },
                    new State()
                        {
                            Name = "Texas",
                            Abbreviation = "TX",
                        },
                    new State()
                        {
                            Name = "Utah",
                            Abbreviation = "UT",
                        },
                    new State()
                        {
                            Name = "Vermont",
                            Abbreviation = "VT",
                        },
                    new State()
                        {
                            Name = "Virginia",
                            Abbreviation = "VA",
                        },
                    new State()
                        {
                            Name = "Washington",
                            Abbreviation = "WA",
                        },
                    new State()
                        {
                            Name = "West Virginia",
                            Abbreviation = "WV",
                        },
                    new State()
                        {
                            Name = "Wisconsin",
                            Abbreviation = "WI",
                        },
                    new State()
                        {
                            Name = "Wyoming",
                            Abbreviation = "WY",
                        }
                };

            return states;
        }
    }
}
