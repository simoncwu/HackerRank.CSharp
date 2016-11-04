using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HackerRank
{
    class City
    {
        public int ID { get; set; }
        public long Population { get; set; }
        public IEnumerable<City> Neighbors { get; set; }
        public City(int id, long pop)
        {
            ID = id;
            Population = pop;
        }
    }

    public class PresidentialGala
    {
        #region By Largest Neighbor (Recursive Traversal)

        static int GetLargestNeighbor(int[] pops, HashSet<int> neighbors)
        {
            int largest = -1;
            if (neighbors.Count > 0)
                largest = neighbors.OrderByDescending(n => pops[n]).First();
            return largest;
        }

        private static int LargestConnected(int[] pops, HashSet<int>[] neighbors, int city)
        {
            int largest = GetLargestNeighbor(pops, neighbors[city]);
            if (largest != -1 && pops[largest] > pops[city])
            {
                city = LargestConnected(pops, neighbors, largest);
            }
            return city;
        }

        private static long MaxAttendeesByNeighbor(int[] pops, HashSet<int>[] neighbors)
        {
            long count = 0;
            for (int city = 0, n = pops.Length; city < n; city++)
            {
                while (pops[city] > 0)
                {
                    int largest = LargestConnected(pops, neighbors, city);
                    count += pops[largest];
                    pops[largest] = 0;
                    foreach (int nb in neighbors[largest])
                    {
                        pops[nb] = 0;
                    }
                }
            }
            return count;
        }

        #endregion

        #region By Largest Population Order

        static int CountNeighborsLost(HashSet<int>[] neighbors, Dictionary<int, int> pops, int n)
        {
            return neighbors[n].Sum(o => pops[o]);
        }

        static int CountUnvisitedNeighbors(HashSet<int>[] neighbors, Dictionary<int, int> pops, int n)
        {
            return neighbors[n].Count(o => pops[o] > 0);
        }

        private static long MaxAttendeesByPopulation(int[] popsarr, HashSet<int>[] neighbors)
        {
            int n = popsarr.Length;
            Dictionary<int, int> pops = new Dictionary<int, int>(n);
            for (int i = 0; i < popsarr.Length; i++)
            {
                pops[i] = popsarr[i];
            }
            long count = 0;
            var sorted = pops.OrderByDescending(p => p.Value).ThenBy(p => CountNeighborsLost(neighbors, pops, p.Key)).Select(p => p.Key).ToArray();
            int visited = 0;
            foreach (var i in sorted)
            {
                int pop = pops[i];
                if (pop > 0)
                {
                    visited++;
                    if (visited + CountUnvisitedNeighbors(neighbors, pops, i) == n && CountNeighborsLost(neighbors, pops, i) > pop)
                    {
                        // maximize last set of connected cities
                        pops[i] = 0;
                    }
                    else
                    {
                        count += pop;
                        foreach (var j in neighbors[i])
                        {
                            visited++;
                            pops[j] = 0;
                        }
                    }
                }
            }
            return count;
        }

        #endregion

        #region By Graph Coloring

        private static Tuple<City, bool> QItem(City c, bool b)
        {
            return Tuple.Create<City, bool>(c, b);
        }

        private static long TotalPopByColor(IEnumerable<City> cities, IDictionary<int, bool> colors, bool color)
        {
            long sum = 0;
            foreach (City city in cities)
            {
                if (colors[city.ID] == color)
                    sum += city.Population;
            }
            return sum;
        }

        private static IEnumerable<City> ColorNeighbors(City city, IDictionary<int, bool> colors)
        {
            return city.Neighbors.Where(_ => colors[_.ID] == colors[city.ID] && _.Population > 0).ToList();
        }

        private static IEnumerable<City> ExtractColorNetwork(City city, IDictionary<int, bool> colors)
        {
            Dictionary<int, City> result = new Dictionary<int, City>();
            Queue<City> q = new Queue<City>();
            q.Enqueue(city);
            while (q.Count > 0)
            {
                city = q.Dequeue();
                if (!result.ContainsKey(city.ID) && city.Population > 0)
                {
                    City copy = new City(city.ID, city.Population);
                    copy.Neighbors = ColorNeighbors(city, colors);
                    result.Add(copy.ID, copy);
                    foreach (City nb in copy.Neighbors)
                    {
                        q.Enqueue(nb);
                    }
                }
            }

            // replace neighbor references with copies stored in result
            foreach (City c in result.Values)
            {
                c.Neighbors = c.Neighbors.Select(_ => result[_.ID]);
                c.Population = 0;
            }

            return result.Values;
        }

        private static long MaxAttendeesByColoring(int[] pops, HashSet<int>[] neighbors)
        {
            var cities = ToCityList(pops, neighbors);
            return MaxAttendeesByColoring(cities);
        }

        private static long MaxAttendeesByColoring(IEnumerable<City> cities)
        {
            int n = cities.Count();

            // alternately color graph starting from largest city
            Dictionary<int, bool> colors = new Dictionary<int, bool>(n);
            Queue<Tuple<City, bool>> q = new Queue<Tuple<City, bool>>();
            q.Enqueue(QItem(cities.First(), false));
            while (q.Count > 0)
            {
                var i = q.Dequeue();
                City c = i.Item1;
                bool color = i.Item2;
                if (!colors.ContainsKey(c.ID))
                    colors[c.ID] = color;
                foreach (City nb in c.Neighbors)
                {
                    if (!colors.ContainsKey(nb.ID))
                    {
                        q.Enqueue(QItem(nb, !color));
                    }
                }
            }

            // same-color neighbor case, recursively process subgraph
            foreach (var city in cities.Where(_ => _.Population > 0))
            {
                var cn = ColorNeighbors(city, colors);
                if (cn.Any())
                {
                    var sub = MaxAttendeesByColoring(ExtractColorNetwork(city, colors));
                    city.Population = sub;
                }
            }

            // find max sum of each color
            return Math.Max(TotalPopByColor(cities, colors, true), TotalPopByColor(cities, colors, false));
        }

        #endregion

        #region By Neighbor Population

        private static long MaxAttendeesByNeighborPopulation(int[] pops, HashSet<int>[] neighbors)
        {
            int n = pops.Length;
            var cities = ToCityList(pops, neighbors);
            long count = 0;
            bool?[] invite = new bool?[n];

            // cycle detection
            EraseCycles(cities);

            // visit cities, higher population preference
            foreach (City city in cities.OrderByDescending(_ => _.Population))
            {
                if (invite[city.ID].HasValue)
                    continue;
                if (city.Neighbors.Any(_ => invite[_.ID] == true))
                {
                    // skip if a neighbor has been visited
                    invite[city.ID] = false;
                }
                else if (city.Neighbors.All(_ => invite[_.ID] == false))
                {
                    // visit if all neighbors were skipped
                    invite[city.ID] = true;
                    count += city.Population;
                }
                else
                {
                    // visit neighbors or city, depending on which has higher total pop
                    var undecided = city.Neighbors.Where(_ => !invite[_.ID].HasValue);
                    bool visit = city.Population >= undecided.Sum(_ => _.Population);
                    invite[city.ID] = visit;
                    if (visit)
                        count += city.Population;
                    foreach (City nb in undecided)
                    {
                        invite[nb.ID] = !visit;
                        if (!visit)
                            count += nb.Population;
                    }
                }
            }
            return count;
        }

        private static void EraseCycles(IEnumerable<City> cities)
        {
            int[] cycleCount = new int[cities.Count()];
            foreach (City city in cities)
            {
                if (cycleCount[city.ID] > 0 || city.Neighbors.Count() < 2)
                    continue;
                int totalShared = 0;
                foreach (City nb in city.Neighbors.Where(_ => _.Neighbors.Count() > 1))
                {
                    // each shared neighbor with city represents another cycle
                    int shared = nb.Neighbors.Intersect(city.Neighbors).Count();
                    if (shared > 0)
                    {
                        cycleCount[nb.ID] += shared;
                        totalShared += shared;
                    }
                }

                // city will appear twice in each cycle as a neighbor
                cycleCount[city.ID] += totalShared / 2;
            }

            // erase cycle cities, highest cycle membership & lowest pop order
            var cycleCities = from c in cities
                              where cycleCount[c.ID] > 0
                              group c by cycleCount[c.ID] into _
                              orderby _.Key descending
                              select _;
            foreach (var group in cycleCities)
            {
                foreach (var city in group.OrderBy(_ => _.Population))
                {
                    if (cycleCount[city.ID] > 0)
                    {
                        // erase city and decrement neighbor cycles
                        city.Population = 0;
                        foreach (var nb in city.Neighbors)
                        {
                            if (cycleCount[nb.ID] > 0)
                                cycleCount[nb.ID]--;
                        }
                    }
                }
            }
        }

        #endregion

         private static IEnumerable<City> ToCityList(int[] pops, HashSet<int>[] neighbors)
        {
            int n = pops.Length;
            Dictionary<int, City> cities = new Dictionary<int, City>(n);
            for (int i = 0; i < n; i++)
            {
                cities.Add(i, new City(i, pops[i]));
            }
            foreach (City city in cities.Values)
            {
                city.Neighbors = neighbors[city.ID].Select(_ => cities[_]);
            }

            return cities.Values;
        }

       public static void Run(String[] args)
        {
            using (var reader = new StreamReader("../../PresidentialGalaTest.txt"))
            {
                int[] nm = Array.ConvertAll(reader.ReadLine().Split(' '), int.Parse);
                int n = nm[0], m = nm[1];

                int[] pops = Array.ConvertAll(reader.ReadLine().Split(' '), int.Parse);

                HashSet<int>[] neighbors = pops.Select(o => new HashSet<int>()).ToArray();
                for (int i = 0; i < m; i++)
                {
                    int[] c = Array.ConvertAll(reader.ReadLine().Split(' '), int.Parse);
                    c[0]--;
                    c[1]--;
                    neighbors[c[0]].Add(c[1]);
                    neighbors[c[1]].Add(c[0]);
                }

                long num = MaxAttendeesByNeighborPopulation(pops, neighbors);

                Console.WriteLine(num);
            }
        }

    }
}