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

namespace PAI.FRATIS.SFL.Infrastructure
{
    public class NullLogger : ILogger
    {
        public bool IsDebugEnabled { get; private set; }
        public bool IsInfoEnabled { get; private set; }
        public bool IsTraceEnabled { get; private set; }
        public bool IsWarnEnabled { get; private set; }
        public bool IsErrorEnabled { get; private set; }
        public bool IsFatalEnabled { get; private set; }
        public void Debug(string message)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
        }

        public void DebugException(string message, Exception exception)
        {
        }

        public void Info(string message)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Info(Exception exception, string format, params object[] args)
        {
        }

        public void InfoException(string message, Exception exception)
        {
        }

        public void Trace(string message)
        {
        }

        public void Trace(string format, params object[] args)
        {
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
        }

        public void TraceException(string message, Exception exception)
        {
        }

        public void Warn(string message)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
        }

        public void WarnException(string message, Exception exception)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Error(Exception exception, string format, params object[] args)
        {
        }

        public void ErrorException(string message, Exception exception)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Fatal(string format, params object[] args)
        {
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
        }

        public void FatalException(string message, Exception exception)
        {
        }
    }
}