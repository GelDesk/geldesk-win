using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public sealed class ComponentFrameReader : IEnumerator<JToken>, IEnumerator, IDisposable
    {
        public ComponentFrameReader(JArray data)
        {
            _frame = data;
            _enumerator = _frame.GetEnumerator();
        }

        readonly JArray _frame;
        readonly IEnumerator<JToken> _enumerator;

        public JToken Current
        {
            get { return _enumerator.Current; }
        }

        object IEnumerator.Current
        {
            get { return _enumerator.Current; }
        }

        public bool HasChildren
        {
            get
            {
                // A frame: ['header', {props}, [child], [child], ...]
                return _frame.Count > 2;
            }
        }
        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}
