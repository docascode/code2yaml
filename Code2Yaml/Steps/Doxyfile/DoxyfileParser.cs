namespace Microsoft.Content.Build.Code2Yaml.Doxyfile
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    public class DoxyfileParser
    {
        private static readonly Regex MetadataRegex = new Regex(@"^\s*(?!#)((\w*)\s*(=?)\s*(.*?))(\\?)$", RegexOptions.Compiled);

        public static Dictionary<string, object> ParseDoxyfile(Stream doxyfileStream)
        {
            var metadata = new Dictionary<string, object>();
            using (var reader = new StreamReader(doxyfileStream))
            {
                string lastKey = null;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    var match = MetadataRegex.Match(line);
                    if (!match.Success)
                    {
                        continue;
                    }
                    bool equalSign = match.Groups[3].Value == "=";
                    bool appendSign = match.Groups[5].Value == "\\";
                    if (!equalSign)
                    {
                        Debug.Assert(lastKey != null);
                        Debug.Assert(metadata.ContainsKey(lastKey));
                        var value = metadata[lastKey] as List<string>;
                        Debug.Assert(value != null);
                        value.Add(match.Groups[1].Value);
                        metadata[lastKey] = value;
                        if (!appendSign)
                        {
                            lastKey = null;
                        }
                    }
                    else
                    {
                        var key = match.Groups[2].Value;
                        var value = match.Groups[4].Value;
                        if (appendSign)
                        {
                            metadata[key] = new List<string> { value };
                            lastKey = key;
                        }
                        else
                        {
                            metadata[key] = value;
                            lastKey = null;
                        }
                    }
                }
            }
            return metadata;
        }
    }
}
