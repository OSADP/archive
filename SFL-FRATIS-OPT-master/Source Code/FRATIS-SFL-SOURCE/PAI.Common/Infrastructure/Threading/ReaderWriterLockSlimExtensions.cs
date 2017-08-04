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
using System;
using System.Threading;

namespace PAI.FRATIS.SFL.Common.Infrastructure.Threading
{
    public static class ReaderWriterLockSlimExtensions
    {
        /// <summary>
        /// Starts thread safe read write code block.
        /// </summary>
        /// <param name="rwLock">The rwLock.</param>
        /// <returns></returns>
        public static IDisposable ReadAndWrite(this ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterUpgradeableReadLock();
            return new DisposableCodeBlock(rwLock.ExitUpgradeableReadLock);
        }

        /// <summary>
        /// Starts thread safe read code block.
        /// </summary>
        /// <param name="rwLock">The rwLock.</param>
        /// <returns></returns>
        public static IDisposable Read(this ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterReadLock();
            return new DisposableCodeBlock(rwLock.ExitReadLock);
        }

        /// <summary>
        /// Starts thread safe write code block.
        /// </summary>
        /// <param name="rwLock">The rwLock.</param>
        /// <returns></returns>
        public static IDisposable Write(this ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterWriteLock();
            return new DisposableCodeBlock(rwLock.ExitWriteLock);
        }

        private sealed class DisposableCodeBlock : IDisposable
        {
            private readonly Action _action;

            public DisposableCodeBlock(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}