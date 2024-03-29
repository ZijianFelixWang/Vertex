﻿using System;
using System.Collections.Generic;
using System.Text;

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
