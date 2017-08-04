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

using System.Linq;
using PAI.FRATIS.SFL.Domain.Logging;
using PAI.FRATIS.SFL.Domain.Users;

namespace PAI.FRATIS.SFL.Services.Logging
{
    /// <summary>
    /// Logger interface
    /// </summary>
    public partial interface ILogService
    {
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">entity</param>
        void Delete(LogEntry entity);

        /// <summary>
        /// Gets an entity 
        /// </summary>
        /// <param name="entityId">entity identifier</param>
        /// <returns>Entity</returns>
        LogEntry GetById(int entityId);

        /// <summary>
        /// Inserts an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Insert(LogEntry entity);

        /// <summary>
        /// Updates the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Update(LogEntry entity);

        /// <summary>
        /// Gets an <see cref="IQueryable"/> of LogEntry
        /// </summary>
        /// <returns><see cref="IQueryable"/> of LogEntry</returns>
        IQueryable<LogEntry> GetAll();

        void InsertLog(LogLevel level, string message, string fullMessage, User user);
    }
}