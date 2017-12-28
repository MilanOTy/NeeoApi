using Home.Neeo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Home.Neeo.Device
{
    internal class TokenSearch<T> : IComparer<ITokenSearch> where T : class, ITokenSearch
    {
        internal class Options
        {
            internal char[]                                 Delimiter;
            internal double                                 Threshold;
            internal int                                    MaxFilterTokenEntries;
            internal string[]                               CollectionKeys;
            internal bool                                   Unique;
            internal Func<T, bool>                          PreprocessCheck;
            internal Func<string, List<string>, int>        SearchAlgorithm;
            internal Func<List<T>, List<T>>                 SortAlgorithm;
            internal Func<List<T>, double, double, List<T>> PostProcessAlgorithm;
        }

        private Options _options;
        private List<T> _collection;
        private Options _defaultOptions;

        internal TokenSearch (List<T> collection, Options options)
        {
            _collection = collection;
            _defaultOptions = new Options
            {
                Delimiter = new char[] { '[', ' ', '-', '_', ']', '+' },
                Threshold = 0.7,
                MaxFilterTokenEntries = 5,
                CollectionKeys = new string[0],
                Unique = false,
                PreprocessCheck = (e) => { return true; },
                SearchAlgorithm = DefaultSearchAlgorithm,
                SortAlgorithm = DefaultSortAlgorithm,
                PostProcessAlgorithm = DefaultPostprocessAlgorithm
            };
            _options = options;
            if (_options.Delimiter == null)
                _options.Delimiter = _defaultOptions.Delimiter;
            if (_options.Threshold == 0.0)
                _options.Threshold = _defaultOptions.Threshold;
            if (_options.MaxFilterTokenEntries == 0)
                _options.MaxFilterTokenEntries = _defaultOptions.MaxFilterTokenEntries;
            if (_options.PreprocessCheck == null)
                _options.PreprocessCheck = _defaultOptions.PreprocessCheck;
            if (_options.SearchAlgorithm == null)
                _options.SearchAlgorithm = _defaultOptions.SearchAlgorithm;
            if (_options.SortAlgorithm == null)
                _options.SortAlgorithm = _defaultOptions.SortAlgorithm;
            if (_options.PostProcessAlgorithm == null)
                _options.PostProcessAlgorithm = _defaultOptions.PostProcessAlgorithm;

            foreach (var entry in _collection)
            {
                entry.DataEntryTokens = new HashSet<string>();
                foreach (var key in _options.CollectionKeys)
                {
                    string token = entry.GetKeyItem(key);
                    if (token != null)
                    {
                        var splitToken = token.Trim().ToLower().Split(_options.Delimiter);
                        foreach (var str in splitToken)
                        {
                            entry.DataEntryTokens.Add(str);
                        }
                    }
                }
            }
        }
        internal List<T> Search (string token, Options options = null)
        {

            List<string> searchTokens = new List<string>();
            var threshold = _defaultOptions.Threshold;
            var preprocessCheck = _defaultOptions.PreprocessCheck;

            if (options != null)
            {
                if (options.Threshold != 0.0)
                    threshold = options.Threshold;
                if (options.PreprocessCheck != null)
                    preprocessCheck = options.PreprocessCheck;
            }

            List<string> tmpsplit = new List<string> (token.Trim().Split(_options.Delimiter));
            List<string> tmp = new List<string> (tmpsplit.Where((item,index) => {
                  return tmpsplit.IndexOf(item) == index;
             }));

            for (int i = 0, len = Math.Min(tmp.Count, _options.MaxFilterTokenEntries); i < len; i++)
            {
                searchTokens.Add(tmp[i].ToLower());
            }

            List<T> resultTmp = new List<T>();
            double maxScore = 0;
            foreach (var entry in _collection)
            {
                if (_options.PreprocessCheck != null && !_options.PreprocessCheck(entry))
                {
                    continue;
                }
                double score = entry.DataEntryTokens.Aggregate(0, (scor, dataEntryToken) => {
                    return scor + _options.SearchAlgorithm(dataEntryToken, searchTokens);
                });
                if (score != 0)
                {
                    if (score > maxScore)
                    {
                        maxScore = score;
                    }
                    entry.Score = score;
                    resultTmp.Add (entry);
                }
            }
            resultTmp = _options.PostProcessAlgorithm(resultTmp, maxScore, threshold);
            resultTmp = _options.SortAlgorithm(resultTmp);
            return resultTmp;
        }
        internal T FindFirstExactMatch (Func<T,bool> match)
        {
            foreach (var item in _collection)
            {
                if (match(item))
                    return item;
            }
            return null;
        }

        private int DefaultSearchAlgorithm (string haystack, List<string> needles)
        {
            return needles.Aggregate(0, (score, needle) =>
            {
                int stringPos = haystack.IndexOf(needle);
                if (stringPos == -1)
                {
                    return score;
                }
                if (needle.Length < 2)
                {
                    return score + 1;
                }
                if (haystack == needle)
                {
                    return score + 6;
                }
                if (stringPos == 0)
                {
                    return score + 2;
                }
                return score + 1;
            });
        }
        private List<T> DefaultPostprocessAlgorithm( List<T> collection, double maxScore, double threshold)
        {
            double normalizedScore = 1.0 / maxScore;
            List<T> result = new List<T>();
            List<string> ids = new List<string>();
            foreach (var e in collection)
            {
                e.Score = 1 - e.Score * normalizedScore;
                if (e.Score <= threshold)
                {
                    e.MaxScore = maxScore;
                    if (_options.Unique)
                    {
                        StringBuilder idsb = new StringBuilder();
                        foreach (string key in _options.CollectionKeys)
                        {
                            idsb.Append(e.GetKeyItem(key));
                        }
                        string id = idsb.ToString();
                        if (ids.IndexOf(id) < 0)
                        {
                            ids.Add(id);
                            result.Add(e);
                        }
                    }
                    else
                    {
                        result.Add(e);
                    }
                }
            }
            return result;
        }
        private List<T> DefaultSortAlgorithm (List<T> array)
        {
            array.Sort(this);
            return array;
        }

        int IComparer<ITokenSearch>.Compare(ITokenSearch a, ITokenSearch b)
        {
            if (a.Score != b.Score)
            {
                return a.Score < b.Score ? -1 : +1;
            }
            int cmp = string.CompareOrdinal (a.Name, b.Name);
            return cmp;
        }
    }
}

