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
using PAI.FRATIS.SFL.Domain.Users;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Security;

namespace PAI.FRATIS.SFL.Services.Authentication
{
    public interface IUserService : IEntitySubscriberServiceBase<User>
    {
        User GetUserByEmail(string email);

        void ForgotPassword(string email);

        bool ValidateCustomer(string email, string password);

        bool IsEmailUnique(string email);

        void SetUserPassword(User user, PasswordFormat passwordFormat, string password);
    }

    public class UserService : EntitySubscriberServiceBase<User>, IUserService
    {
        /// <summary>
        /// The encryption service.
        /// </summary>
        private readonly IEncryptionService _encryptionService;

        /// <summary>
        /// The password generator.
        /// </summary>
        private readonly IPasswordGenerator _passwordGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="cacheManager">
        /// The cache manager.
        /// </param>
        /// <param name="encryptionService">
        /// The encryption service.
        /// </param>
        /// <param name="passwordGenerator">
        /// The password generator.
        /// </param>
        public UserService(
            IRepository<User> repository,
            ICacheManager cacheManager,
            IEncryptionService encryptionService,
            IPasswordGenerator passwordGenerator)
            : base(repository, cacheManager)
        {
            _encryptionService = encryptionService;
            _passwordGenerator = passwordGenerator;
        }

        /// <summary>
        /// The get user by email.
        /// </summary>
        /// <param name="email">
        /// The email address.
        /// </param>
        /// <returns>
        /// The <see cref="User"/>.
        /// </returns>
        public User GetUserByEmail(string email)
        {
            return InternalSelect().FirstOrDefault(u => u.Username == email);
        }

        /// <summary>
        /// The is email unique.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsEmailUnique(string email)
        {
            return !InternalSelect().Any(u => u.Username == email);
        }

        /// <summary>
        /// Resets the user's password and email's them a new password
        /// </summary>
        /// <param name="email">email address</param>
        public void ForgotPassword(string email)
        {
            var user = GetUserByEmail(email);
            if (user == null || user.Deleted || !user.Active)
            {
                return;
            }

            var password = _passwordGenerator.GeneratePassword(10);

            SetUserPassword(user, user.PasswordFormat, password);
        }

        /// <summary>
        /// The validate customer.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public virtual bool ValidateCustomer(string email, string password)
        {
            var user = GetUserByEmail(email);

            if (user == null || user.Deleted)
            {
                return false;
            }

            var isValid = false;

            switch (user.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    isValid = password == user.Password;
                    break;

                case PasswordFormat.Encrypted:
                    var pwd = _encryptionService.EncryptText(password);

                    isValid = pwd == user.Password;
                    break;

                case PasswordFormat.Hashed:
                    var pwdhash = _encryptionService.CreatePasswordHash(password, user.PasswordSalt);
                    isValid = pwdhash == user.Password;
                    break;
            }

            // save last login date
            if (isValid)
            {
                user.LastLoginDate = DateTime.UtcNow;
                Update(user);
            }

            return isValid;
        }

        /// <summary>
        /// The set user password.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="passwordFormat">
        /// The password format.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public void SetUserPassword(User user, PasswordFormat passwordFormat, string password)
        {
            user.PasswordFormat = passwordFormat;

            switch (user.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    {
                        user.Password = password;
                    }

                    break;
                case PasswordFormat.Encrypted:
                    {
                        user.Password = _encryptionService.EncryptText(password);
                    }

                    break;
                case PasswordFormat.Hashed:
                    {
                        string saltKey = _encryptionService.CreateSaltKey(5);
                        user.PasswordSalt = saltKey;
                        user.Password = _encryptionService.CreatePasswordHash(password, saltKey);
                    }

                    break;
            }
        }
    }
}