using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerRank
{
    public class HiddenMessage
    {
        struct Match
        {
            public string Word { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
        }

        static bool ContainsPhrase(string s, string[] p, int start)
        {
            int pos = 0;
            while (start < p.Length)
            {
                string word = p[start++];
                int index = s.IndexOf(word, pos++);
                if (index == -1)
                    return false;
            }
            return true;
        }

        static bool ContainsLetters(string s, int s_start, char[] letters, int l_start)
        {
            while (l_start < letters.Length)
            {
                if (s_start >= s.Length)
                    return false;
                s_start = s.IndexOf(letters[l_start], s_start);
                if (s_start < 0)
                    return false;
                l_start++;
            }
            return true;
        }

        static int GetMinCost(string t, string[] p)
        {
            int unvisited = 0, i = 0, cost = p.Length - 1, start = 0;

            // process one word at a time, resetting position to start of word + 1 each time
            for (int p_i = 0, p_end = p.Length - 1; p_i < p_end; p_i++)
            {
                string word = p[p_i];
                start = t.IndexOf(word[0], i);
                int matched = 0;

                // process each letter of word
                while (matched < word.Length)
                {
                    int next = t.IndexOf(word[matched], start);
                    start = next + 1;
                    if (next < 0) // letter not found, should not occur
                        return 0;
                    if (next < unvisited) // duplicate letter
                        cost++;
                    else
                    { // new letter, delete/skip
                        cost += next - unvisited;
                        unvisited = next + 1;
                    }
                    matched++;
                }
                // minimize cost by avoiding reuse of visited letters if rest of p is in rest of t
                i = ContainsPhrase(t.Substring(unvisited), p, p_i + 1) ? unvisited : (i + 1);
            }

            // process last word by allowing skipping ahead mid-word to unvisited segment if match found
            char[] letters = p[p.Length - 1].ToCharArray();
            for (int c_i = 0, c_end = letters.Length; c_i < c_end; c_i++)
            {
                if (ContainsLetters(t, unvisited, letters, c_i))
                    start = unvisited;
                int next = t.IndexOf(letters[c_i], start);
                start = next + 1;
                if (next < unvisited)
                    cost++;
                else
                {
                    cost += next - unvisited;
                    unvisited = next + 1;
                }
            }

            // delete remaining unvisited letters
            return cost + t.Length - unvisited;
        }

        public static void Run(String[] args)
        {
            using (var reader = new StreamReader("../../HiddenMessageTest.txt"))
            {
                string t = reader.ReadLine();
                string[] p = reader.ReadLine().Split(' ');

                List<Match> matches = new List<Match>();
                int matched = 0, skipped = 0, noskip = -1;
                string next = p[0];
                for (int i = 0, tlen = t.Length; i < tlen; i++)
                {
                    if (next == null || i + next.Length > tlen)
                        break;
                    if (t.Substring(i, next.Length) == next)
                    {
                        matches.Add(new Match
                        {
                            Word = next,
                            Start = i,
                            End = noskip = i + next.Length - 1
                        });
                        next = ++matched < p.Length ? p[matched] : null;
                    }
                    else if (i > noskip)
                    {
                        skipped++;
                    }
                }
                skipped += t.Length - noskip - 1;

                bool allMatch = matches.Count == p.Length;
                Console.WriteLine(allMatch ? "YES" : "NO");

                if (matched > 0)
                {
                    foreach (Match m in matches)
                    {
                        Console.Write(string.Format("{0} {1} {2} ", m.Word, m.Start, m.End));
                    }
                }
                else
                {
                    Console.Write(0);
                }
                Console.WriteLine();

                Console.WriteLine(allMatch ? GetMinCost(t, p) : 0);
            }
        }
    }
}
