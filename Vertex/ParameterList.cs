using System.Collections.Generic;

namespace Vertex
{
    class ParameterList
    {
        public string For;
        public List<ParameterItem> Parameters;
    }

    class ParameterItem
    {
        public uint Index;
        public string Type;
        public bool Value;
    }
}
