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
using System.Text;
using System.Threading.Tasks;
using PAI.FRATIS.Wrappers.WebFleet.Mapping;
using PAI.FRATIS.Wrappers.WebFleet.Model;
using PAI.FRATIS.Wrappers.WebFleet.MessagesService;
using PAI.FRATIS.Wrappers.WebFleet.Settings;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public interface IWebFleetMessagesService
    {
        ICollection<WebFleetMessage> GetRecentTextMessages(int hours, int minutes, string objectNo = "");

        bool CreateQueue(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.TEXT);

        bool DeleteQueue(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.TEXT);

        ICollection<WebFleetMessage> GetQueueMessages(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.STATUS,
                         bool markMessagesAsAcknowledged = true);

        bool AcknowledgeQueueMessages(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.STATUS);

        bool SendMessage(string objectNumber, string message);
    }

    public class WebFleetMessagesService : IWebFleetMessagesService
    {
        private readonly IWebFleetMappingService _mappingService;

        public WebFleetMessagesService(IWebFleetMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public AuthenticationParameters GetAuthenticationParameters()
        {
            var auth = new AuthenticationParameters()
            {
                accountName = WebFleetSettings.AccountName,
                userName = WebFleetSettings.UserName,
                password = WebFleetSettings.Password,
                apiKey = WebFleetSettings.ApiKey
            };

            return auth;
        }

        public GeneralParameters GetGeneralParameters()
        {
            return new GeneralParameters
            {
                locale = KnownLocales.US,
                timeZone = KnownTimeZones.America_New_York
            };
        }

        public bool HandleResult(ServiceOpResult result)
        {
            return result.statusCode == 0;
            // TODO - Log Errors to Logger Service
        }



        public ICollection<WebFleetMessage> GetRecentTextMessages(int hours, int minutes, string objectNo = "")
        {
            var result = new List<WebFleetMessage>();

            var webService = new messagesClient();
            var showMessagesParam = new ShowMessagesParameter()
                {
                    messageCategory = MessageCategory.TEXT,
                    messageCategorySpecified = true, 
                    dateRange = new DateRange() { rangePattern = DateRangePattern.UD, rangePatternSpecified = true, from = DateTime.UtcNow.AddHours(-hours).AddMinutes(-minutes), fromSpecified = true, to = DateTime.UtcNow, toSpecified = true },
                    @object = new ObjectIdentityParameter() { objectNo = objectNo }
                };
            var response = webService.showMessages(GetAuthenticationParameters(), GetGeneralParameters(), showMessagesParam);
            
            if (HandleResult(response))
            {
                result.AddRange(response.results.Select(msg => _mappingService.Map(msg as MessageTO)));
            }

            return result;
        }

        /// <summary>
        /// Creates a Messages Queue that is linked to the current API User
        /// </summary>
        /// <param name="queueType"></param>
        /// <returns>
        /// Returns true if operation successful, False if operation failed or queue type already exists
        /// </returns>
        public bool CreateQueue(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.TEXT)
        {
            var webService = new messagesClient();
            var response = webService.createQueueExtern(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      new QueueServiceParameter()
                                                          {
                                                              filter = queueType
                                                          });
            return HandleResult(response);
        }

        public bool DeleteQueue(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.TEXT)
        {
            var webService = new messagesClient();
            var response = webService.deleteQueueExtern(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      new QueueServiceParameter()
                                                      {
                                                          filter = queueType
                                                      });
            return HandleResult(response);
        }

        public bool SendMessage(string message, string objectNo, string externalId = "", string objectUid = "")
        {
            var webService = new messagesClient();
            var response = webService.sendTextMessage(GetAuthenticationParameters(), GetGeneralParameters(),
                                       new SimpleTextMessageParameter()
                                           {
                                               messageText = message,
                                               @object = new ObjectIdentityParameter()
                                                   {
                                                       externalId = externalId,
                                                       objectNo = objectNo,
                                                       objectUid = objectUid
                                                   }
                                           });
            return HandleResult(response);

        }

        public ICollection<WebFleetMessage> GetQueueMessages(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.STATUS, bool markMessagesAsAcknowledged = true)
        {
            var webService = new messagesClient();
            var result = new List<WebFleetMessage>();

            var response = webService.popQueueMessagesExtern(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      new QueueServiceParameter()
                                                      {
                                                          filter = queueType,   
                                                      });

            if (HandleResult(response))
            {
                result.AddRange(response.results.Select(msg => _mappingService.Map(msg as QueueServiceData)));
            }

            if (markMessagesAsAcknowledged)
            {
                AcknowledgeQueueMessages(queueType);    
            }

            return result;
        }

        public bool AcknowledgeQueueMessages(QueueServiceMessageClassFilter queueType = QueueServiceMessageClassFilter.STATUS)
        {
            var webService = new messagesClient();
            var result = new List<WebFleetMessage>();

            var response = webService.ackQueueMessagesExtern(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      new QueueServiceParameter()
                                                      {
                                                          filter = queueType
                                                      });

            return HandleResult(response);
        }

        public bool SendMessage(string objectNumber, string message)
        {
            var result = new List<WebFleetMessage>();
            var webService = new messagesClient();
            var response = webService.sendTextMessage(GetAuthenticationParameters(), GetGeneralParameters(),
                                                      new SimpleTextMessageParameter()
                                                          {
                                                              messageText = message,
                                                              @object = new ObjectIdentityParameter()
                                                                  {
                                                                      objectNo = objectNumber
                                                                  }
                                                          });
            return HandleResult(response);
        }

    }
}
