using System;
using System.Linq;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Services
{
    public interface INodeConnectionFactory
    {
        /// <summary>
        /// Creates a <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        NodeConnection CreateNodeConnection(INode startNode, INode endNode);
    }
}