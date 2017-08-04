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

namespace PAI.Drayage.Optimization.Common
{
    /// <summary>
    /// Wrapper for generating random numbers
    /// </summary>
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Reseeds the specified seed.
        /// </summary>
        /// <param name="seed">The seed.</param>
        void Reseed(int seed);
        /// <summary>
        /// Nexts the double.
        /// </summary>
        /// <returns>a random number between 0 and 1</returns>
        double NextDouble();
        /// <summary>
        /// Nexts this instance.
        /// </summary>
        /// <returns>a random integer greater than -1</returns>
        int Next();
        /// <summary>
        /// Nexts the specified maximum value.
        /// </summary>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>a rnadom integer greater than -1 and less tha maxValue</returns>
        int Next(int maxValue);
        /// <summary>
        /// Nexts the specified minimum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>a random integer between min and max value inclusive</returns>
        int Next(int minValue, int maxValue);
    }
}