using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenMessage
{
    class Program
    {
        struct Match
        {
            public string Word { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
        }

        static bool ContainsPhrase(string s, string[] p, int start)
        {
            if (start >= p.Length)
                return true;
            string word = p[start];
            int index = s.IndexOf(word);
            return index != -1 && ContainsPhrase(s.Substring(index + 1), p, start + 1);
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

        static int EndIndexOf(string s, string word, int start)
        {
            if (start < 0)
                return -1;
            foreach (char c in word)
            {
                while (start < s.Length && s[start] != c)
                {
                    start++;
                }
            }
            return start < s.Length ? ++start : -1;
        }

        static int GetMinCost(string t, string[] p)
        {
            int unvisited = 0, i = 0, cost = p.Length - 1, start = 0;
            for (int p_i = 0, p_end = p.Length - 1; p_i < p_end; p_i++)
            {
                string word = p[p_i];
                start = t.IndexOf(word[0], i);
                int matched = 0;
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
                i = ContainsPhrase(t.Substring(unvisited), p, p_i + 1) ? unvisited : (i + 1);
            }

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

            return cost + t.Length - unvisited;
        }

        static void Main(String[] args)
        {
            var reader = new StreamReader("../../TestCase.txt");
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
