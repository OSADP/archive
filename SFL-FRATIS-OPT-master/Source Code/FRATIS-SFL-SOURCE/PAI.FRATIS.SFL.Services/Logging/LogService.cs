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
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Logging;
using PAI.FRATIS.SFL.Domain.Users;

namespace PAI.FRATIS.SFL.Services.Logging
{
    public partial class LogService : ILogService
    {
        protected readonly IRepository<LogEntry> _repository;

        public LogService(IRepository<LogEntry> repository)
        {
            _repository = repository;
        }

        public bool IsEnabled(LogLevel level)
        {
            return true;
        }

        public void Delete(LogEntry entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _repository.Delete(entity);
        }

        public LogEntry GetById(int entityId)
        {
            return _repository.GetById(entityId);
        }

        public void Insert(LogEntry entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _repository.Insert(entity);
        }

        public void Update(LogEntry entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _repository.Update(entity);
        }

        public IQueryable<LogEntry> GetAll()
        {
            return _repository.Select();
        }

        public void InsertLog(LogLevel level, string message, string fullMessage, User user)
        {
            var logEntry = new LogEntry()
                {
                    LogLevel = level,
                    Message = message,
                    FullMessage = fullMessage,
                    User = user,
                    AuditDate = DateTime.UtcNow
                };

            Insert(logEntry);
        }
    }
}
