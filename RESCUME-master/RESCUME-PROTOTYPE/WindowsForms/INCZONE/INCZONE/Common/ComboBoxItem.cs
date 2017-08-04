using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class ComboBoxItem
    {

        public ComboBoxItem() {}

        public ComboBoxItem(string Id, string Name)
        {
            // TODO: Complete member initialization
            this.Id = Id;
            this.Name = Name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
