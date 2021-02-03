using System.Collections.Generic;

namespace Vertex
{
    class Environment
    {
        // Environment identifier
        public string ID;

        // Metadata Dictionary
        // Includes: VertexVersion Visibility CMMaxSize Terminal Networking VCIONum VCIOLocation
        public Dictionary<string, string> Metadata = new Dictionary<string, string>();

        // Evaluator
        public Evaluator Evaluator;

        // IO definition
        public IO IO;

        // Rule Pool definition
        public RulePool RulePool;

        // Matrix definition
        public Matrix Matrix;

        // Whether to use SVG export
        public (bool Whether, string Where, ushort Frequency) SVGProperty = (false, "", 0);

        // Where to export resulted rule
        public string ResultedRuleOutputFile = "";

        // Language
        public string Language = "en-US";

        // Construction function
        public Environment(string id, Dictionary<string, string> metadata, Evaluator ev, IO io)
        {
            ID = id;
            Metadata = metadata;
            Evaluator = ev;
            IO = io;
        }

        public Environment()
        {
            //Console.WriteLine("[WARNING] Creating a new Environment without parameters.");
        }
    }
}
