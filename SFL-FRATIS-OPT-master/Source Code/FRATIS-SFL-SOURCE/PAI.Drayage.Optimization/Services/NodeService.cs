// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeService.cs" company="">
//   
// </copyright>
// <summary>
//   The node service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Infrastructure.Threading;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// The node service.
    /// </summary>
    public class NodeService : INodeService
    {
        /// <summary>
        /// The _node connection cache.
        /// </summary>
        readonly IDictionary<Tuple<INode, INode>, NodeConnection> _nodeConnectionCache;

        /// <summary>
        /// The _rw lock.
        /// </summary>
        readonly ReaderWriterLockSlim _rwLock;

        /// <summary>
        /// The _configuration.
        /// </summary>
        readonly OptimizerConfiguration _configuration;

        /// <summary>
        /// The _node connection factory.
        /// </summary>
        readonly INodeConnectionFactory _nodeConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeService"/> class.
        /// </summary>
        /// <param name="nodeConnectionFactory">
        /// The node connection factory.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public NodeService(INodeConnectionFactory nodeConnectionFactory, OptimizerConfiguration configuration)
        {
            _configuration = configuration;

            _nodeConnectionFactory = nodeConnectionFactory;

            _nodeConnectionCache = new Dictionary<Tuple<INode, INode>, NodeConnection>();
            _rwLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Gets the <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode">
        /// </param>
        /// <param name="endNode">
        /// </param>
        /// <returns>
        /// The <see cref="NodeConnection"/>.
        /// </returns>
        public virtual NodeConnection GetNodeConnection(INode startNode, INode endNode)
        {
            NodeConnection nodeConnection = null;

            var key = new Tuple<INode, INode>(startNode, endNode);

            using (_rwLock.Read())
            {
                if (_nodeConnectionCache.TryGetValue(key, out nodeConnection))
                {
                    return nodeConnection;
                }
            }

            // not in cache, create a new connection
            nodeConnection = _nodeConnectionFactory.CreateNodeConnection(startNode, endNode);

            // cache
            using (_rwLock.Write())
            {
                _nodeConnectionCache[key] = nodeConnection;
            }

            return nodeConnection;
        }

    }
}