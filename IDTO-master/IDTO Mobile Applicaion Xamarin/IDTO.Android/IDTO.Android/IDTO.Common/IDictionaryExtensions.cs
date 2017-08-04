using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace IDTO.Common
{

    public static class IDictionaryExtensions
    {
        public static System.Collections.Generic.Dictionary<string,string> ToDictionary(this System.Collections.IDictionary build)
        {
            var build2 = new Dictionary<string, string>();

            foreach (var key in build.Keys)
            {
                build2.Add(key.ToString(), build[key].ToString());
            }

            return build2;
        }
    }
    
}