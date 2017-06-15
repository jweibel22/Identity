using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Helpers
{
    public class SubstringLookup
    {
        public class TreeNode
        {
            public TreeNode(char c, string substring)
            {
                Char = c;
                Substring = substring;
                Children = new List<TreeNode>();
            }

            public char Char { get; set; }

            public string Substring { get; set; }

            public IList<TreeNode> Children { get; set; }
        }

        public static TreeNode BuildPrefixTree(IList<string> substrings)
        {
            char c = '$';
            var tree = new TreeNode(c, null);
            tree.Children = BuildRec(tree, substrings, 0).ToList();
            return tree;
        }

        private static IEnumerable<TreeNode> BuildRec(TreeNode parent, IEnumerable<string> substrings, int i)
        {
            var cs = substrings.Where(ss => ss.Length > i).Select(ss => ss[i]).Distinct().ToList();

            foreach (var c in cs)
            {
                var match = substrings.SingleOrDefault(ss => ss.Length == i + 1 && ss[i] == c);
                var child = new TreeNode(c, match);
                child.Children = BuildRec(child, substrings.Where(ss => ss.Length > i && ss[i] == c), i + 1).ToList();
                yield return child;
            }
        }

        private static IEnumerable<string> RecX(TreeNode node, string s, int i)
        {
            if (i == s.Length)
                yield break;

            var next = node.Children.SingleOrDefault(c => c.Char == s[i]);

            if (next == null)
                yield break;

            if (next.Substring != null)
                yield return next.Substring;

            foreach (var x in RecX(next, s, i + 1))
            {
                yield return x;
            }
        }

        public static IEnumerable<string> FindSubstrings(string s, TreeNode prefixTree)
        {
            for (int i = 0; i < s.Length; i++)
            {
                foreach (var x in RecX(prefixTree, s.Substring(i), 0))
                    yield return x;
            }
        }

        public static IEnumerable<string> FindSubstrings(string s, IList<string> substrings)
        {
            var prefixTree = BuildPrefixTree(substrings.Select(ss => ss).ToList());
            return FindSubstrings(s, prefixTree);
        }
    }

    
}
