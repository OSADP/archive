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
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNumberGenerator"/> class.
        /// </summary>
        public RandomNumberGenerator()
        {
            this._random = new Random();
        }

        /// <summary>
        /// Reseeds the specified seed.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public void Reseed(int seed)
        {
            this._random = seed == 0 ? new Random() : new Random(seed);
        }

        /// <summary>
        /// Nexts the double.
        /// </summary>
        /// <returns>a random number between 0 and 1</returns>
        public double NextDouble()
        {
            return this._random.NextDouble();
        }

        /// <summary>
        /// Nexts this instance.
        /// </summary>
        /// <returns>a random integer greater than -1</returns>
        public int Next()
        {
            return this._random.Next();
        }

        /// <summary>
        /// Nexts the specified maximum value.
        /// </summary>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>a rnadom integer greater than -1 and less tha maxValue</returns>
        public int Next(int maxValue)
        {
            return this._random.Next(maxValue);
        }

        /// <summary>
        /// Nexts the specified minimum value.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns>a random integer between min and max value inclusive</returns>
        public int Next(int minValue, int maxValue)
        {
            return this._random.Next(minValue, maxValue);
        }
    }
}