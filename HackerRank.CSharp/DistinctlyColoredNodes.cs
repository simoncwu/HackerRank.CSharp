using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HackerRank
{
    public class DistinctlyColoredNodes
    {
        const int MAX_NODES = 100000;

        #region By Crawling Each Tree Segment

        static int ColorsFrom(int[] colors, HashSet<int>[] edges, int start, int removed)
        {
            HashSet<int> distinct = new HashSet<int>();
            bool[] visited = new bool[colors.Length];
            Queue<int> q = new Queue<int>();
            distinct.Add(colors[start]);
            visited[start] = true;
            foreach (int next in edges[start].Where(_ => _ != removed))
            {
                q.Enqueue(next);
            }
            while (q.Count > 0)
            {
                int cur = q.Dequeue();
                distinct.Add(colors[cur]);
                visited[cur] = true;
                foreach (int next in edges[cur])
                {
                    if (!visited[next])
                        q.Enqueue(next);
                }
            }
            return distinct.Count;
        }

        static long SumDistinctColors(int[] colors, HashSet<int>[] edges, int[][] edges_raw)
        {
            long sum = 0;
            foreach (int[] uv in edges_raw)
            {
                int u = uv[0], v = uv[1];
                sum += ColorsFrom(colors, edges, u, v) * ColorsFrom(colors, edges, v, u);
            }
            return sum;
        }

        #endregion

        static long SumDistinctColors(int[] colors, HashSet<int>[] edges, Dictionary<int, int> total)
        {
            long sum = 0;
            int n = colors.Length;
            var counts = new Dictionary<int, int>[n]; // for tracking color counts at each node
            Stack<int> s = new Stack<int>(); // to recurse through iteration
            s.Push(0);
            while (s.Count > 0)
            {
                int i = s.Peek();
                var ct = counts[i] = new Dictionary<int, int>();
                var unvisited = edges[i].Where(_ => counts[_] == null);
                if (unvisited.Any())
                {
                    foreach (int e in unvisited)
                    {
                        s.Push(e);
                    }
                }
                else
                {
                    s.Pop();
                    ct[colors[i]] = 1;
                    // aggregate color counts from child trees
                    foreach (int e in edges[i].Where(_ => counts[_].Any()))
                    {
                        foreach (var c in counts[e])
                        {
                            IncDict(ct, c.Key, c.Value);
                        }
                        counts[e] = null; // subtree no longer used, free up memory
                    }
                    long root = CountWithout(total, ct), disconnected = CountDistinct(ct);
                    sum += root * disconnected;

                }
            }
            return sum;
        }

        static void IncDict(Dictionary<int, int> d, int k, int v = 1)
        {
            d[k] = (d.ContainsKey(k) ? d[k] : 0) + v;
        }

        static Dictionary<int, int> CalcTotalCounts(int[] colors)
        {
            var result = new Dictionary<int, int>();
            foreach (int c in colors)
            {
                IncDict(result, c);
            }
            return result;
        }

        static int CountDistinct(Dictionary<int, int> counts)
        {
            return counts.Count;
        }

        static int CountWithout(Dictionary<int, int> counts1, Dictionary<int, int> counts2)
        {
            int removed = 0;
            foreach (int c in counts2.Keys)
            {
                if (counts1[c] <= counts2[c])
                    removed++;
            }
            return _maxColors - removed;
        }

        static int[] RemapColors(int[] colors)
        {
            Dictionary<int, int> map = new Dictionary<int, int>();
            _maxColors = 0;
            foreach (int color in colors)
            {
                if (!map.ContainsKey(color))
                    map[color] = _maxColors++;
            }
            return colors.Select(_ => map[_]).ToArray();
        }

        static StreamReader _reader;
        static int _maxColors;

        static string ReadLine()
        {
            return _reader.ReadLine();
        }

        public static void Run(String[] args)
        {
            int n;
            int[] colors;
            HashSet<int>[] edges;
            int[][] edges_raw;
            using (_reader = new StreamReader("../../DistinctlyColoredNodesTest.txt"))
            {
                n = int.Parse(ReadLine());
                colors = Array.ConvertAll(ReadLine().Split(' '), int.Parse);
                edges = colors.Select(_ => new HashSet<int>()).ToArray();
                edges_raw = new int[n - 1][];
                for (int i = 1; i < n; i++)
                {
                    int[] uv = Array.ConvertAll(ReadLine().Split(' '), int.Parse);
                    int u = --uv[0], v = --uv[1];
                    edges[u].Add(v);
                    edges[v].Add(u);
                    edges_raw[i - 1] = uv;
                }
            }
            colors = RemapColors(colors);

            var total = CalcTotalCounts(colors);
            long sum = SumDistinctColors(colors, edges, total);
            Console.WriteLine(sum);
        }
    }
}