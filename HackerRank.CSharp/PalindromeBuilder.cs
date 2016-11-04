using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HackerRank
{
    public class PalindromeBuilder
    {
        static string Reverse(string s)
        {
            char[] rev = new char[s.Length];
            for (int i = 0, j = rev.Length - 1; i < s.Length; i++)
            {
                rev[j] = s[i];
                j--;
            }
            return new String(rev);
        }

        static void TryAddPalindrome(List<Tuple<int, int>> list, int suffix, int lcp, int[] p, int pIndex, ref int maxLength)
        {
            int len = lcp * 2 + PalindromeLengthAt(p, pIndex);
            if (len > maxLength)
            {
                maxLength = len;
                list.Clear();
            }
            if (len == maxLength)
                list.Add(Tuple.Create(suffix, lcp));
        }

        static string LongestPalindrome(string a, string b, string s, int[] pa, int[] pb, List<Tuple<int, int>> longest)
        {
            string p = string.Empty;
            foreach (Tuple<int, int> item in longest)
            {
                int i = item.Item1, len = item.Item2;

                // get remainder palindromes
                string rem = string.Empty;
                int[] arr = pa;
                if (i > pa.Length)
                {
                    i -= pa.Length + 1;
                    arr = pb;
                }
                i += len;
                if (i < arr.Length)
                {
                    int plen = arr[i];
                    if (plen > 0)
                    {
                        rem = (item.Item1 > a.Length ? b : a).Substring(i, plen);
                    }
                }

                string sp = s.Substring(item.Item1, len);
                sp += rem + Reverse(sp);
                p = PreferredPalindrome(p, sp);
            }
            return p;
        }

        static int PalindromeLength(string s, int midLeft, int midRight)
        {
            bool odd = midLeft == midRight;
            int len = 0;
            while (midLeft >= 0 && midRight < s.Length && s[midLeft] == s[midRight])
            {
                len++;
                midLeft--;
                midRight++;
            }
            len *= 2;
            return odd ? len - 1 : len;
        }

        static int PalindromeLength(string s, int mid)
        {
            int even = PalindromeLength(s, mid - 1, mid), odd = PalindromeLength(s, mid, mid);
            return Math.Max(even, odd);
        }

        static bool IsPreferred(string s1, string s2)
        {
            return s1.Length > s2.Length || (s1.Length == s2.Length && s1.CompareTo(s2) < 0);
        }

        static string PreferredPalindrome(string a, string b)
        {
            return IsPreferred(a, b) ? a : b;
        }

        static int PalindromeLengthAt(int[] p, int index)
        {
            return index >= 0 && index < p.Length ? p[index] : 0;
        }

        static void FillStartLengths(int[] lengths, int index, int baseLength, int maxLength)
        {
            for (; baseLength <= maxLength; index--)
            {
                FillLength(lengths, index, baseLength);
                baseLength += 2;
            }
        }

        static void FillLength(int[] lengths, int i, int l)
        {
            if (lengths[i] < l)
                lengths[i] = l;
        }

        #region By LCP Array

        static int[] Manacher(string s)
        {
            int[] startLengths = new int[s.Length], midLengths = new int[s.Length];
            int i = s.Length - 1, cutoff = s.Length, mid = i;
            bool evenMirror = false;
            while (i >= 0)
            {
                // get palindrome length centered at i
                bool skipMirror = i <= cutoff;
                if (!skipMirror)
                {
                    // get mirror val
                    int mirror = 2 * mid - i;
                    if (evenMirror)
                        mirror--;
                    int len = midLengths[mirror];

                    // look ahead for even (asymmetric) mirror length and swap
                    if (++mirror >= 0 && mirror < midLengths.Length && midLengths[mirror] % 2 == 0)
                        len = midLengths[mirror];

                    int mirrorCutoff = i - len / 2 + 1;
                    if (mirrorCutoff <= cutoff)
                        skipMirror = true;
                    else
                        midLengths[i] = len;
                }

                // (re)calculate palindrome and fill in starting points
                if (skipMirror)
                {
                    int len = PalindromeLength(s, i), iCutoff = i - len / 2;
                    midLengths[i] = len;
                    evenMirror = len % 2 == 0;
                    if (iCutoff < cutoff)
                    {
                        // new palindrome left bound found, so fill in lengths
                        cutoff = iCutoff;
                        mid = i;
                        FillStartLengths(startLengths, i, 1, 1);
                        FillStartLengths(startLengths, i - 1, evenMirror ? 2 : 3, i - cutoff);
                    }
                }

                i--;
            }
            return startLengths;
        }

        static int[] SuffixArray(string s)
        {
            var suf = Enumerable.Range(0, s.Length).ToArray();
            Array.Sort(suf, (i, j) =>
            {
                // lexicographically compare substrings starting at i and j
                if (i == j)
                    return 0;
                while (i < s.Length && j < s.Length)
                {
                    if (s[i] != s[j])
                        return s[i].CompareTo(s[j]);
                    i++;
                    j++;
                }
                return i < s.Length ? 1 : -1;
            });
            return suf;
        }

        static int[] LCP(string s, int[] suffix, int cutoff)
        {
            int[] arr = new int[suffix.Length];
            for (int i = 1; i < suffix.Length; i++)
            {
                int a = suffix[i], b = suffix[i - 1];
                while (a < s.Length && b < s.Length && s[a] == s[b])
                {
                    a++;
                    b++;
                }
                arr[i] = a - suffix[i];
            }
            return arr;
        }

        static List<Tuple<int, int>> LongestPalindromes(int[] suffix, int[] lcp, int[] pa, int[] pb)
        {
            int max = 0, matched = 0;
            var list = new List<Tuple<int, int>>();
            for (int i = 2; i < lcp.Length; i++)
            {
                // skip non-palindromes
                if (lcp[i] == 0)
                {
                    matched = 0;
                    continue;
                }

                int[] p = suffix[i] < pa.Length ? pa : pb;
                int pi = suffix[i] + lcp[i], j = i - 1;
                if (p == pb)
                {
                    // adjust indexing according to string a or b
                    pi -= pa.Length + 1;
                }

                // check if suffix is from different string than previous
                if (suffix[i] < pa.Length ^ suffix[j] < pa.Length)
                {
                    // different, so valid match
                    int len = lcp[i];
                    TryAddPalindrome(list, suffix[i], len, p, pi, ref max);

                    // add previous unmatched from other string only
                    p = p == pb ? pa : pb;
                    matched = len;
                    len = lcp[i];
                    while (len >= matched && (suffix[j] < pa.Length ^ suffix[i] < pa.Length))
                    {
                        pi = suffix[j] + len;
                        if (p == pb)
                            pi -= pa.Length + 1; // adjust indexing
                        TryAddPalindrome(list, suffix[j], Math.Min(matched, len), p, pi, ref max);
                        len = lcp[j--];
                    }
                }
                else if (matched > 0)
                {
                    // add at most matched length thus far
                    TryAddPalindrome(list, suffix[i], Math.Min(matched, lcp[i]), p, pi, ref max);
                }
            }
            return list;
        }

        static string BuildByLCP(string a, string b)
        {
            b = Reverse(b);
            string s = a + "$" + b;
            int[] suffix = SuffixArray(s);
            int[] lcp = LCP(s, suffix, a.Length);
            int[] pa = Manacher(a), pb = Manacher(b);
            List<Tuple<int, int>> longest = LongestPalindromes(suffix, lcp, pa, pb);
            return LongestPalindrome(a, b, s, pa, pb, longest);
        }

        #endregion

        #region By Prefix Iteration

        static string Prefix(string s, int index)
        {
            return s.Substring(index, 2);
        }

        static Dictionary<string, List<int>> PrefixIndices(string s)
        {
            Dictionary<string, List<int>> ind = new Dictionary<string, List<int>>();
            int last = s.Length - 1;
            for (int i = 0; i < last; i++)
            {
                string key = Prefix(s, i);
                List<int> list;
                if (!ind.TryGetValue(key, out list))
                {
                    list = new List<int>();
                    ind.Add(key, list);
                }
                list.Add(i);
            }
            return ind;
        }

        static int[] FindAllPalindromes(string s, HashSet<char> letters)
        {
            int[] p = new int[s.Length];
            for (int i = p.Length - 1; i >= 0; i--)
            {
                letters.Add(s[i]);
                int len = PalindromeLength(s, i);
                bool even = len % 2 == 0;
                FillStartLengths(p, i, 1, 1);
                FillStartLengths(p, i - 1, even ? 2 : 3, len);
            }
            return p;
        }

        static string BuildByPrefixIteration(string a, string b)
        {
            b = Reverse(b);
            HashSet<char> la = new HashSet<char>(), lb = new HashSet<char>(); // cache distinct chars in strings
            int[] pa = FindAllPalindromes(a, la), pb = FindAllPalindromes(b, lb); // lengths of in-string palindromes by start position
            Dictionary<string, List<int>> bIndices = PrefixIndices(b);
            bool[] cka = new bool[a.Length], ckb = new bool[b.Length]; // avoid repeat computing same index
            List<Tuple<int, int>> list = new List<Tuple<int, int>>();

            int max = 0;
            // process palindromes with prefixes of length >= 2
            int last = a.Length - 1;
            for (int ai = 0; ai < last; ai++)
            {
                string key = Prefix(a, ai);
                List<int> indices;
                if (bIndices.TryGetValue(key, out indices))
                {
                    cka[ai] = true;
                    foreach (int bi in indices)
                    {
                        ckb[bi] = true;
                        // skip substrings of previously checked palindromes
                        if (ai > 0 && bi > 0 && a[ai - 1] == b[bi - 1])
                            continue;

                        // longest palindrome possible at current indices
                        int ma = ai + 1, mb = bi + 1;
                        while (ma < a.Length && mb < b.Length && a[ma] == b[mb])
                        {
                            ma++;
                            mb++;
                            if (ma < a.Length)
                                cka[ma] = true;
                            if (mb < b.Length)
                                ckb[mb] = true;
                        }
                        int len = ma - ai;
                        TryAddPalindrome(list, ai, len, pa, ma, ref max);
                        TryAddPalindrome(list, bi + a.Length + 1, len, pb, mb, ref max);
                    }
                }
            }

            // process palindromes with single char prefixes
            Action<string, int, int[], bool[], HashSet<char>> tryAddSingleCharPalindromes = (s, offset, p, ck, l) =>
            {
                for (int i = 0; i < p.Length; i++)
                {
                    if (!ck[i] && l.Contains(s[i]))
                        TryAddPalindrome(list, offset + i, 1, p, i + 1, ref max);
                }
            };
            tryAddSingleCharPalindromes(a, 0, pa, cka, lb);
            tryAddSingleCharPalindromes(b, a.Length + 1, pb, ckb, la);

            return LongestPalindrome(a, b, a + "$" + b, pa, pb, list);
        }

        #endregion

        public static void Run(String[] args)
        {
            using (var reader = new StreamReader("../../PalindromeBuilder.txt"))
            {
                int q = int.Parse(reader.ReadLine());
                while (q-- > 0)
                {
                    string a = reader.ReadLine();
                    string b = reader.ReadLine();
                    string s = BuildByPrefixIteration(a, b);
                    Console.WriteLine(string.IsNullOrEmpty(s) ? "-1" : s);
                }
            }
        }
    }
}