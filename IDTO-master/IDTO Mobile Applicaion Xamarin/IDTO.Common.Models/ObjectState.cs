namespace IDTO.Common.Models
{
    using System.Collections.Generic;
    using System;

    public class ObjectState
    {
        public enum Type
        {
            Unchanged = 0,
            Added = 1,
            Modified = 2,
            Deleted = 3
        }
    }
}