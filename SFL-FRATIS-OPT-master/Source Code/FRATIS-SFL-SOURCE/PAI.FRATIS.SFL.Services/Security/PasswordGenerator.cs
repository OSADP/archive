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

namespace PAI.FRATIS.SFL.Services.Security
{
    public class PasswordGenerator : IPasswordGenerator
    {
        public string GeneratePassword(int length)
        {
            string result = string.Empty;
            var r = new Random();
            r.Next();

            for (int i = 0; i < length; i++)
            {
                r.Next();
                if (i == 0)
                {
                    result += GetRandomPrintableLetter(r);
                }
                else
                {
                    result += GetRandomPrintableCharacter(r);
                }
            }

            return result;
        }

        private string GetRandomPrintableCharacter(Random r)
        {
            const string passwordCharacters = "abcdefghijkmnopqrstuvwxyz23456789";
            int location = r.Next(passwordCharacters.Length - 1);
            return passwordCharacters.Substring(location, 1);
        }

        private string GetRandomPrintableLetter(Random r)
        {
            const string passwordCharacters = "abcdefghijkmnopqrstuvwxyz";
            int location = r.Next(passwordCharacters.Length - 1);
            return passwordCharacters.Substring(location, 1);
        }

    }
}