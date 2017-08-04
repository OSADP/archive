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

namespace PAI.FRATIS.SFL.Domain.Users
{
    public interface IUserContext
    {
        int UserId { get; set; }

        int SubscriberId { get; set; }
    }

    public class UserContext
    {
        public int UserId { get; set; }

        public int SubscriberId { get; set; }

        public UserContext()
        {
            UserId = 1;
            SubscriberId = 1;
        }
    }
}