using System;

namespace LeetCode
{
    public class TreeNode
    {
        public int val;
        public TreeNode left;
        public TreeNode right;

        public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null)
        {
            this.val = val;
            this.left = left;
            this.right = right;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private int ans = Int32.MinValue;

        public int MaxPathSum(TreeNode root)
        {
            if (root == null)
            {
                return 0;
            }

            int left = System.Math.Max(0, MaxPathSum(root.left));
            int right = System.Math.Max(0, MaxPathSum(root.right));

            ans = System.Math.Max(ans, left + right + root.val);
            return System.Math.Max(left, right) + root.val;
        }
    }
}