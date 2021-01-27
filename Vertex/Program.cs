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
#define __DEBUG_MODE__  // Master tab
// #define __SHOW_COMPLETE_XML__

using System;
using System.Xml;
// using System.Xml.Schema;
using System.IO;

namespace Vertex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Vertex (Vertex Evaluator-based Recursive automata Technology for EXpress cellular calculation) Version 0.1");
            Console.WriteLine("Copyright 2021 Zijian Wang. All rights reserved.");
            Console.WriteLine("The Vertex software and its resource files are protected by the copyright and other pending or existing intellectual property rights in the P.R.China and other countries/regions.");

            //Console.Write("Environment file:");
            //string EnvConf = Console.ReadLine();
            string EnvConf;
            if (args.Length == 0)
            {
                // No specified environment file
                Console.Write("Environment file:");
                EnvConf = Console.ReadLine();
            }
            else
            {
                EnvConf = args[0];
            }

            XmlDocument document = new XmlDocument();

            // Declare the Environment class entity now...
            Environment env = new Environment();

            try
            {
                //document.Load(EnvConf);
                XmlReaderSettings readerSettings = new XmlReaderSettings
                {
                    IgnoreComments = true
                };
                using XmlReader reader = XmlReader.Create(EnvConf, readerSettings);
                document.Load(reader);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("[FATAL] Environment file cannot be loaded.");
                //Environment.Exit(-1);
                System.Environment.Exit(-1);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine("[FATAL] Environment file cannot be loaded.");
                //Environment.Exit(-1);
                System.Environment.Exit(-1);
            }

#if __DEBUG_MODE__ && __SHOW_COMPLETE_XML__
            Console.WriteLine(document.ToString());
            string line;
            try
            {
                StreamReader sr = new StreamReader(EnvConf);
                do
                {

                    line = sr.ReadLine();
                    Console.WriteLine(line);
                } 
                while (line != null);
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Debug option exec done.");
            }
#endif

            // Parse document
            // document: <environment> - <metadata> ; <evaluator> ; <io>
            env = EnvParser.ParseFromXML(document);

            Console.WriteLine("[INFO] Environment file parsed done successfully.");
            Console.WriteLine($"[INFO] Environment profile: {env.ID}");

            // Now starts the cellular automation...
            // Before doing so, we need to summarize what the Evaluator does because I've forgot it.
            /*
             * About the Evaluator
             * ----------------------------------------------------------
             * The evaluator is made up of 4 functions. All of the public
             * variables have been set up in the parser's procedure. All
             * of the four functions are public. They're void 
             * ConfigCurrentRanking(ushort CurrentRanking), void
             * ConfigMaxRanking(ushort MaxRanking), void Execute(ref IO
             * IODefinition, ref Matrix MatrixDefinition, ref RulePool
             * RulePoolDefinition) and void Evaluate(ref IO
             * IODefinition, ref Matrix MatrixDefinition, ref RulePool
             * RulePoolDefinition).
             * 
             * So what to do to execute the automation actually?
             * It examines the ParameterLists to find out whether the 
             * current rule can work out the tests of configured by the
             * parser. It will return a boolean to represent whether the
             * rule is successful (typically not).
             */

            // Initialize 
            env.RulePool = new RulePool(512);   // Sorry but this is a fixed value.
            env.Evaluator.ConfigCurrentRanking(0);
            env.Evaluator.ConfigMaxRanking((short)(env.Evaluator.AnswerList.Parameters.Count - 1));

            env.Evaluator.RankingHistory.Add(0);
            env.Evaluator.RankingHistory.Add(0);

            Console.WriteLine("Enter main loop...");

            while (env.Evaluator.Evaluate(ref env.IO, ref env.Matrix, ref env.RulePool) != true)
            {
                // Not successful yet...
                Console.Write($"-> [{env.Evaluator.RankingHistory[^1]}] Unsuccessful CurRule= ");
                for (int i = 0; i < env.RulePool.RuleLength; i++)
                {
                    Console.Write(env.RulePool.GetLatest()[i] ? "1" : "0");
                }
                Console.WriteLine();
                _ = env.RulePool.Produce(env.Evaluator.RankingHistory);
                env.Evaluator.ConfigCurrentRanking(0);

                // reset cells in the matrix
                env.Matrix.ResetCells();
            }

            Console.WriteLine("-> Success!");
            Console.Beep();
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------------");
            Console.Write("Resulted rule= ");
            for (int i = 0; i < env.RulePool.RuleLength; i++)
            {
                Console.Write(env.RulePool.GetLatest()[i] ? "1" : "0");
            }
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------------");
        }
    }
}
