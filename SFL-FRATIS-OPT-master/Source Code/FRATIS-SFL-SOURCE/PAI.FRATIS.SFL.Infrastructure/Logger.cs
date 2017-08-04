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
    public class Logger : ILogger
    {
        private readonly Ninject.Extensions.Logging.ILogger _logger;

        public Logger(Ninject.Extensions.Logging.ILogger logger)
        {
            this._logger = logger;
        }

        public bool IsDebugEnabled { get { return this._logger.IsDebugEnabled; } }

        public bool IsInfoEnabled { get { return this._logger.IsInfoEnabled; } }

        public bool IsTraceEnabled { get { return this._logger.IsTraceEnabled; } }

        public bool IsWarnEnabled { get { return this._logger.IsWarnEnabled; } }

        public bool IsErrorEnabled { get { return this._logger.IsErrorEnabled; } }

        public bool IsFatalEnabled { get { return this._logger.IsFatalEnabled; } }

        public void Debug(string message)
        {
            this._logger.Debug(message);
        }

        public void Debug(string format, params object[] args)
        {
            this._logger.Debug(format, args);
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            this._logger.Debug(exception, format, args);
        }

        public void DebugException(string message, Exception exception)
        {
            this._logger.Debug(message, exception);
        }

        public void Info(string message)
        {
            this._logger.Info(message);
        }

        public void Info(string format, params object[] args)
        {
            this._logger.Info(format, args);
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            this._logger.Info(exception, format, args);
        }

        public void InfoException(string message, Exception exception)
        {
            this._logger.Info(message, exception);
        }

        public void Trace(string message)
        {
            this._logger.Trace(message);
        }

        public void Trace(string format, params object[] args)
        {
            this._logger.Trace(format, args);
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
            this._logger.Trace(exception, format, args);
        }

        public void TraceException(string message, Exception exception)
        {
            this._logger.Trace(message, exception);
        }

        public void Warn(string message)
        {
            this._logger.Warn(message);
        }

        public void Warn(string format, params object[] args)
        {
            this._logger.Warn(format, args);
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            this._logger.Warn(exception, format, args);
        }

        public void WarnException(string message, Exception exception)
        {
            this._logger.Warn(message, exception);
        }

        public void Error(string message)
        {
            this._logger.Error(message);
        }

        public void Error(string format, params object[] args)
        {
            this._logger.Error(format, args);
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            this._logger.Error(exception, format, args);
        }

        public void ErrorException(string message, Exception exception)
        {
            this._logger.Error(message, exception);
        }

        public void Fatal(string message)
        {
            this._logger.Fatal(message);
        }

        public void Fatal(string format, params object[] args)
        {
            this._logger.Fatal(format, args);
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            this._logger.Fatal(exception, format, args);
        }

        public void FatalException(string message, Exception exception)
        {
            this._logger.Fatal(message, exception);
        }
    }
}
