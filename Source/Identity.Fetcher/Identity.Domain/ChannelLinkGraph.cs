using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IEnumerable<ChannelLinkEdge> DirtyUserChannels
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
    }

}
