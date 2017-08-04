/*!
    @file         InfloCommon/Models/CustomAttributes.cs
    @author       Luke Kucalaba

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/
using System;
using System.ComponentModel.DataAnnotations;

namespace InfloCommon.Models
{
    public class BsmBundleTypeIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string strValue = (value as string);
            
            // Return true if no data present in string
            //    (allows "Required" attribute to deal with this issue of lack of data)
            if(string.IsNullOrEmpty(strValue))
            {
                return true;
            }

            return strValue.Equals("BSB", StringComparison.Ordinal);
        }
    }

    public class BsmMessageTypeIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string strValue = (value as string);
            
            // Return true if no data present in string
            //    (allows "Required" attribute to deal with this issue of lack of data)
            if(string.IsNullOrEmpty(strValue))
            {
                return true;
            }

            return strValue.Equals("BSM", StringComparison.Ordinal);
        }
    }

    public class TimTypeIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string strValue = (value as string);

            // Return true if no data present in string
            //    (allows "Required" attribute to deal with this issue of lack of data)
            if (string.IsNullOrEmpty(strValue))
            {
                return true;
            }

            return strValue.Equals("TIM", StringComparison.Ordinal);
        }
    }

    public class ISO8601TimeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string strValue = (value as string);

            // Return true if no data present in string
            //    (allows "Required" attribute to deal with this issue of lack of data)
            if (string.IsNullOrEmpty(strValue))
            {
                return true;
            }

            DateTime rDateTime;
            bool results = DateTime.TryParse(strValue, out rDateTime);
            return results;
        }
    }
    
    public class Base16StringAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string strValue = (value as string);

            // Return true if no data present in string
            //    (allows "Required" attribute to deal with this issue of lack of data)
            if(string.IsNullOrEmpty(strValue))
            {
                return true;
            }
            
            // Return false if any chars are not hex digits
            int strLen = strValue.Length;
            for(int i=0; i < strLen; ++i)
            {
                char ch = strValue[i];

                bool valid = 
                    (((ch >= '0') && (ch <= '9')) ||
                     ((ch >= 'A') && (ch <= 'F')) ||
                     ((ch >= 'a') && (ch <= 'f')));

                if(!valid)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
