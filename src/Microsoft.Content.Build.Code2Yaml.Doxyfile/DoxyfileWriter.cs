namespace Microsoft.Content.Build.Code2Yaml.Doxyfile
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DoxyfileWriter : IDisposable
    {
        private StreamWriter _inner;

        public DoxyfileWriter(StreamWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            _inner = writer;
        }

        public void Write(Dictionary<string, object> metadata)
        {
            if (metadata == null || metadata.Count == 0)
            {
                return;
            }

            foreach (var pair in metadata)
            {
                _inner.Write($"{pair.Key} = ");
                if (!(pair.Value is List<string>))
                {
                    _inner.WriteLine(pair.Value);
                }
                else
                {
                    var valueList = pair.Value as List<string>;
                    for(int i = 0; i < valueList.Count -1; i++)
                    {
                        _inner.WriteLine($"{valueList[i]} \\");
                    }
                    if (valueList.Count > 0)
                    {
                        _inner.WriteLine(valueList[valueList.Count - 1]);
                    }
                }
            }
        }

        public void Dispose()
        {
            _inner.Close();
        }
    }
}
