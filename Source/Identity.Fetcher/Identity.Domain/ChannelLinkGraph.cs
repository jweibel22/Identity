using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Events;

namespace Identity.Domain
{
    public class ChannelLinkEdge
    {
        public ChannelLinkNode From { get; set; }

        public ChannelLinkNode To { get; set; }
    }

    public enum NodeType { Channel, User }

    public enum CycleVisitState { White, Gray, Black }

    public class ChannelLinkNode
    {
        public static ChannelLinkNode NewUserNode(long id)
        {
            return new ChannelLinkNode
            {
                Id = id,
                NodeType = NodeType.User
            };
        }

        public static ChannelLinkNode NewChannelNode(long id)
        {
            return new ChannelLinkNode
            {
                Id = id,
                NodeType = NodeType.Channel
            };
        }

        public long Id { get; set; }

        public bool IsDirty { get; set; }

        public NodeType NodeType { get; set; }
    }

    public class DirtyUserChannels
    {
        public IEnumerable<ChannelLinkEdge> Channels { get; set; }
    }
        

    public class ChannelLinkGraph
    {
        private readonly IList<ChannelLinkEdge> edges;
        private readonly IList<ChannelLinkNode> nodes;

        public ChannelLinkGraph(IEnumerable<ChannelLinkNode> nodes, IEnumerable<ChannelLinkEdge> edges)
        {
            this.nodes = nodes.ToList();
            this.edges = edges.ToList();

            if (IsCyclic())
            {
                throw new ApplicationException("Graph contains cycles, this is not allowed!");
            }
        }

        public DirtyUserChannels ApplyChanges(IList<IChannelLinkEvent> events)
        {
            lock (this)
            {
                foreach (var e in events)
                {
                    e.Apply(this);
                }

                var dirtyUserChannels = new DirtyUserChannels { Channels = DirtyUserChannels.ToList() };

                foreach (var n in nodes)
                {
                    n.IsDirty = false;
                }

                return dirtyUserChannels;
            }
        }

        public bool IntroducesCycle(ChannelLinkEdge edge)
        {
            try
            {
                edges.Add(edge);
                return IsCyclic();
            }
            finally
            {
                edges.Remove(edge);
            }
        }

        public void MarkAsDirty(long channelId)
        {
            var node = nodes.SingleOrDefault(n => n.Id == channelId && n.NodeType == NodeType.Channel);

            if (node == null)
            {
                throw new ApplicationException(String.Format("Node with channel Id {0} was not found", channelId));
            }

            MarkAsDirtyRec(node);
        }

        private void MarkAsDirtyRec(ChannelLinkNode node)
        {
            if (node.IsDirty)
            {
                return;
            }

            node.IsDirty = true;

            foreach (var outEdge in edges.Where(e => e.From == node))
            {
                MarkAsDirtyRec(outEdge.To);
            }
        }

        public void AddEdge(ChannelLinkEdge edge)
        {
            edges.Add(edge);
        }

        public void RemoveEdge(ChannelLinkEdge edge)
        {
            edges.Add(edge);
        }

        public void AddNode(ChannelLinkNode node)
        {
            nodes.Add(node);
        }

        public void RemoveNode(ChannelLinkNode node)
        {
            var toRemove = edges.Where(e => e.From == node || e.To == node);
            foreach (var x in toRemove)
            {
                edges.Remove(x);
            }

            nodes.Remove(node);
        }

        public IEnumerable<ChannelLinkNode> VisitAllDownstreams(ChannelLinkNode node)
        {
            yield return node;

            foreach (var outEdge in edges.Where(e => e.From == node))
            {
                foreach (var x in VisitAllDownstreams(outEdge.To))
                {
                    yield return x;
                }                
            }
        }

        private IEnumerable<ChannelLinkEdge> DirtyUserChannels
        {
            get
            {
                var userNodes = nodes.Where(n => n.IsDirty && n.NodeType == NodeType.User);

                foreach (var userNode in userNodes)
                {
                    foreach (var outEdge in edges.Where(e => e.To == userNode && e.From.IsDirty))
                    {
                        yield return outEdge;
                    }
                }
            }
        }

        private bool dfsHasBackEdge(ChannelLinkNode n, IDictionary<ChannelLinkNode, CycleVisitState> colors)
        {
            colors[n] = CycleVisitState.Gray;

            foreach (var outEdge in edges.Where(e => e.From == n))
            {
                var v = outEdge.To;

                if (colors[v] == CycleVisitState.Gray)
                {
                    return true;
                }

                if (colors[v] == CycleVisitState.White && dfsHasBackEdge(v, colors))
                {
                    return true;
                }
            }

            colors[n] = CycleVisitState.Black;

            return false;
        }

        public bool IsCyclic()
        {
            var states = nodes.ToDictionary(n => n, n => CycleVisitState.White);

            foreach (var node in nodes)
            {
                if (states[node] == CycleVisitState.White)
                {
                    if (dfsHasBackEdge(node, states))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ChannelLinkEdge GetUserEdge(long userId, long channelId)
        {
            var result = edges.SingleOrDefault(n => n.From.Id == channelId && n.From.NodeType == NodeType.Channel
                                                    && n.To.Id == userId && n.To.NodeType == NodeType.User);

            if (result == null)
            {
                throw new ApplicationException("User edge was not found");
            }

            return result;
        }

        public ChannelLinkEdge GetChannelEdge(long upstreamChannelId, long downstreamChannelId)
        {
            var result = edges.SingleOrDefault(n => n.From.Id == upstreamChannelId && n.From.NodeType == NodeType.Channel 
                                                    && n.To.Id == downstreamChannelId && n.To.NodeType == NodeType.Channel);

            if (result == null)
            {
                throw new ApplicationException("Channel edge was not found");
            }

            return result;
        }

        public ChannelLinkNode GetChannelNode(long channelId)
        {
            var result = nodes.SingleOrDefault(n => n.Id == channelId && n.NodeType == NodeType.Channel);

            if (result == null)
            {
                throw new ApplicationException("Channel node with id " + channelId + " was not found");
            }

            return result;
        }

        public ChannelLinkNode GetUserNode(long userId)
        {
            var result = nodes.SingleOrDefault(n => n.Id == userId && n.NodeType == NodeType.User);

            if (result == null)
            {
                throw new ApplicationException("User node with id " + userId + " was not found");
            }

            return result;
        }
    }

}
