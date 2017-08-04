using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace WebRole1
{
	public static class Utility
	{
		public static ObjectContext ObjectContext(this DbContext context)
		{
			return ((IObjectContextAdapter)context).ObjectContext;
		}
 
	}
}