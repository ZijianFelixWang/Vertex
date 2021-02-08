/*

___ __                 _____               
__ |  / /_____ __________  /______ ____  __
__ | / / _  _ \__  ___/_  __/_  _ \__  |/_/
__ |/ /  /  __/_  /    / /_  /  __/__>  <  
_____/   \___/ /_/     \__/  \___/ /_/|_|  

        === PROJECT VERTEX ===

      Copyright 2021 Zijian Wang.
         All rights reserved.

*/

// Debug tabs... DISABLE MASTER TAB WHEN PACKAGING...
//Master Tab
#if !DEBUG
#define DEBUG
#endif

using System;
using McMaster.Extensions.CommandLineUtils;
using ResourceHelper = Vertex.IOSupport.ResourceHelper;

namespace Vertex
{
    class Program
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        [Option(CommandOptionType.NoValue, Description = "Verbose mode")]
        public bool VerboseMode { get; }

        [Option(CommandOptionType.SingleValue, Description = "Action to do with Vertex: Evolve, Execute, Help or Version")]
        public string Feature { get; }

        [Option(CommandOptionType.SingleValue, Description = "Source file to evolve or execute")]
        public string Source { get; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Whether to use Drag&Drop mode")]
        public bool DragAndDrop { get; }

        public void OnExecute()
        {
            Console.Title = "Vertex v0.2";
            Console.WriteLine("Vertex (Vertex Evaluator-based Recursive automata Technology for EXpress cellular calculation) Version 0.2");
            Console.WriteLine("Copyright 2021 Zijian Wang. All rights reserved.");
            Console.WriteLine("The Vertex software and its resource files are protected by the copyright and other pending or existing intellectual property rights in the P.R.China and other countries/regions.");

            string EnvProfile = Source;
            if (DragAndDrop)
            {
                // No specified environment file
                ResourceHelper.Log("DragAndDropInfo");

                EnvProfile = Console.ReadLine();
            }

            ResourceHelper.VerboseMode = VerboseMode;

            if (string.IsNullOrEmpty(Feature))
            {
                ResourceHelper.Log(IOSupport.VxLogLevel.Fatal, "NoValidParamError");
                Environment.Exit(-1);
            }

            Features.FeatureLoader.LoadFeature(Feature, EnvProfile);
        }
    }
}
