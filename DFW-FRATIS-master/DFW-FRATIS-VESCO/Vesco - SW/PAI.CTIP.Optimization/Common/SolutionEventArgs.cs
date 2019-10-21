using System;
using System.Linq;
using PAI.CTIP.Optimization.Model.Node;

namespace PAI.CTIP.Optimization.Common
{
    public class SolutionEventArgs : EventArgs
    {
        public Solution Solution { get; set; }
    }
}