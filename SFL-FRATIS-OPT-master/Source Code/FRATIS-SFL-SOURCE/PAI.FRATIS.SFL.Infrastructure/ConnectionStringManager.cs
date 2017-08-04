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

using System.Configuration;

namespace PAI.FRATIS.SFL.Infrastructure
{
    /// <summary>Enum representing the SQL server connection string to use</summary>
    public enum DbDestination
    {
        Unspecified,
        Development,
        TestCloud
    }

    /// <summary>The connection string manager returns the appropriate
    /// connection string based upon the active DbDestination.</summary>
    public static class ConnectionStringManager
    {
        /// <summary>Initializes static members of the <see cref="ConnectionStringManager"/> class.</summary>
        static ConnectionStringManager()
        {
            DbDestination = DbDestination.TestCloud;       // set the default DbDestination
        }

        public static void UpdateConnectionString(DbDestination destinationConnectionString)
        {
            ActiveConnectionString = string.Empty;
            DbDestination = destinationConnectionString;
        }

        /// <summary>Gets or sets the active connection string.</summary>
        private static string ActiveConnectionString { get; set; }

        /// <summary>Gets the production connection string.</summary>
        private static string DevelopmentConnectionString
        {
            get
            {
                return @"Server=54.221.219.71;Integrated Security=false;Database=pai_fratis_sfl_dev;User Id=fratismiami;Password=miamifratis2";
            }
        }


        private static string TestCloudConnectionString
        {
            get
            {
                return @"Server=54.221.219.71;Integrated Security=false;Database=pai_fratis_sfl_testing;User Id=fratismiami;Password=miamifratis2";
            }
        }

        /// <summary>Gets or sets the db destination.</summary>
        public static DbDestination DbDestination { get; set; }

        /// <summary>The get connection string for.</summary>
        /// <param name="sqlDestination">The sql destination.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetConnectionStringFor(DbDestination sqlDestination)
        {
            if (sqlDestination > 0)
            {
                switch (sqlDestination)
                {
                    case DbDestination.Development:
                        return DevelopmentConnectionString;
                    case DbDestination.TestCloud:
                        return TestCloudConnectionString;
                }
            }

            return ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        }

        /// <summary>Gets the connection string.</summary>
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(ActiveConnectionString))
                {
                    ActiveConnectionString = GetConnectionStringFor(DbDestination);
                }

                return ActiveConnectionString;
            }
        }
    }
}
