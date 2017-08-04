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

using PAI.FRATIS.SFL.Domain.Subscribers;

namespace PAI.FRATIS.SFL.Domain.Users
{
    /// <summary>
    /// Represents an user
    /// </summary>
    public sealed class User : EntitySubscriberBase, IDatedEntity
    {
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the password salt
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Gets or sets the password format
        /// </summary>
        public PasswordFormat PasswordFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the consecutive failed logins
        /// </summary>
        public int FailedLogins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the timestamp they they are locked until
        /// </summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// Gets or sets whether the User is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the User has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        /// Gets or sets the TimeZone of the user
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the IsAdmin
        /// </summary>
        public bool IsAdmin { get; set; }

        public User()
        {
            LastActivityDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the modified date
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}