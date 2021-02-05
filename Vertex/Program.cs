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
//#define __SHOW_COMPLETE_XML__
//#define __FORCE_TO_FAIL__
//#define __PRINT_UNSUCCESSFUL_CURRULE__
//#define __SAVE_EVERY_STEP_SVG__
//#define __USE_ASPOSE_API__
//#define __READKEY_AFTER_INITIALIZING_DYNAMICVIEWER__

using System;
using System.Xml;
// using System.Xml.Schema;
using System.IO;
using NLog;
using Vertex.Kernel;
using Environment = Vertex.Kernel.Environment;
using ResourceHelper = Vertex.IOSupport.ResourceHelper;
using VxLogLevel = Vertex.IOSupport.VxLogLevel;

#if __USE_ASPOSE_API__
using Aspose.Svg;
#else
using Svg;
#endif

namespace Vertex
{
    class Program
    {
        private const int RuleLength = 512;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.Title = "Vertex v0.2";
            Console.WriteLine("Vertex (Vertex Evaluator-based Recursive automata Technology for EXpress cellular calculation) Version 0.2");
            Console.WriteLine("Copyright 2021 Zijian Wang. All rights reserved.");
            Console.WriteLine("The Vertex software and its resource files are protected by the copyright and other pending or existing intellectual property rights in the P.R.China and other countries/regions.");

            //// Configures NLog
            //var config = new NLog.Config.LoggingConfiguration();
            //// Targets where to log to: File and Console
            //var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.txt" };
            //var logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole");
            //// Rules for mapping loggers to targets            
            //config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            //config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            //// Apply config           
            //LogManager.LoadConfiguration(@"C:\Users\felix\Documents\projects\Vertex\Vertex\");

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
                //Console.Error.WriteLine("[FATAL] Environment file cannot be loaded.");
                Logger.Error("Environment file cannot be loaded.");
                //Environment.Exit(-1);
                System.Environment.Exit(-1);
            }
            catch (ArgumentException)
            {
                //Console.Error.WriteLine("[FATAL] Environment file cannot be loaded.");
                Logger.Error("Environment file cannot be loaded.");
                //Environment.Exit(-1);
                System.Environment.Exit(-1);
            }

#if __DEBUG_MODE__ && __SHOW_COMPLETE_XML__
            //Console.WriteLine(document.ToString());
            Logger.Debug(document.ToString());
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
                //Console.WriteLine("Exception: " + e.Message);
                Logger.Error("Exception: " + e.Message);
            }
            finally
            {
                Logger.Debug("Debug option exec done.");
            }
#endif

            // Parse document
            // document: <environment> - <metadata> ; <evaluator> ; <io>
            env = IOSupport.EnvParser.ParseFromXML(document);

            // Now document has been parsed, can use localized output now.
            // Setup resource helper
            ResourceHelper.DefaultCulture = env.Language;

            //Logger.Info("Environment file parsing done successfully.");
            ResourceHelper.Log(VxLogLevel.Info,"EnvFileParsingDoneInfo");

            //Logger.Info($"Environment profile: {env.ID}");
            ResourceHelper.Log(VxLogLevel.Info, "EnvFileHint", env.ID);

            // Now starts the cellular automation...
            // Before doing so, we need to summarize what the Evaluator does because I've forgot it.
            #region
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
            #endregion

            // Initialize
            if (env.RulePool == null)
            {
                env.RulePool = new RulePool(RuleLength);   // Sorry but this is a fixed value.
            }
            env.Evaluator.ConfigCurrentRanking(0);
            env.Evaluator.ConfigMaxRanking((short)(env.Evaluator.AnswerLists[0].Parameters.Count - 1));

            env.Evaluator.RankingHistory.Add(0);
            env.Evaluator.RankingHistory.Add(0);

            // Initialize DynamicViewer
            if (env.SVGProperty.UseDynamicViewer)
            {
                // DynamicViewer enabled.
                //DynamicViewerHelper.ConfigureDynamicViewer(ViewerFilename, SVGSnapshotFilename)
                // ToWrite: content of: Resources.Static.SVG_Lastest_Dynamic_Viewer
                try
                {
                    string path = env.SVGProperty.Location;
                    using StreamWriter sw = new StreamWriter(path);
                    Console.WriteLine(Resources.Static.SVG_Lastest_Dynamic_Viewer);
                    sw.WriteLine(Resources.Static.SVG_Lastest_Dynamic_Viewer);
                    sw.Close();

                    (bool success, string detail) = IOSupport.DynamicViewerHelper.ConfigureDynamicViewer(path, env.SVGProperty.Where + "_latest.svg");
                    if (success)
                    {
                        ResourceHelper.Log("DynamicViewerSuccessHint", path);
                    }
                    else
                    {
                        ResourceHelper.Log("DynamicViewerErrorHint", detail);
                    }
                }
                catch (Exception exp)
                {
                    ResourceHelper.Log("DynamicViewerErrorHint", exp.Message);
                }

#if __READKEY_AFTER_INITIALIZING_DYNAMICVIEWER__ && __DEBUG_MODE__
                Console.ReadKey();
#endif
            }

            //Logger.Info("Enter main loop...");
            ResourceHelper.Log(VxLogLevel.Info, "EnterMainLoopHint");

#if !__FORCE_TO_FAIL__
            uint count = 0;
            while (env.Evaluator.Evaluate(ref env.IO, ref env.Matrix, ref env.RulePool) != true)
            {
                // Not successful yet...
                Logger.Info($"-> [{count}] <{env.Evaluator.RankingHistory[^1]}> Unsuccessful");
#if __PRINT_UNSUCCESSFUL_CURRULE__ && __DEBUG_MODE__
                Logger.Debug("CurRule= ");
                for (int i = 0; i < env.RulePool.RuleLength; i++)
                {
                    Console.Write(env.RulePool.GetLatest()[i] ? "1" : "0");
                }
                Console.WriteLine();
#endif

#if __SAVE_EVERY_STEP_SVG__ && __DEBUG_MODE__ && __USE_ASPOSE_API__
                // Will export SVG file to destination
                Logger.Info($"Exporting SVG file to " + env.SVGProperty.Where);
                SVGDocument svgDoc = new SVGDocument();
                const string SVGNamespace = "http://www.w3.org/2000/svg";

                // Work around the svg document
                SVGExporter exporter = new SVGExporter();
                exporter.ExportEnvToSVG(ref svgDoc, SVGNamespace, env);

                // Export now
                svgDoc.Save(env.SVGProperty.Where + $"_{count}.svg");
                Logger.Info("SVG file exported successfully.");
#endif
                // SVGSnapshot execution
                //env.SVGProperty.Frequency
#if !__USE_ASPOSE_API__
                // export logic here
                if ((count + 1) % env.SVGProperty.Frequency == 0)
                {
                    //Logger.Info("Exporting SVG Snapshot to " + env.SVGProperty.Where + $"_{count}.svg");
                    ResourceHelper.Log(VxLogLevel.Info, "ExportSVGSnapshotHead", env.SVGProperty.Where + $"_{count}.svg");
                    SvgDocument svgDocument = new SvgDocument();
                    //SVGExporter exporter = new SVGExporter();
                    //exporter.ExportEnvToSVG(ref svgDocument, env);
                    IOSupport.SVGExporter.ExportEnvToSVG(ref svgDocument, env);
                    svgDocument.Write(env.SVGProperty.Where + $"_{count}.svg");
                    svgDocument.Write(env.SVGProperty.Where + "_latest.svg");
                    //Logger.Info("SVGSnapshot file exported successfully.");
                    ResourceHelper.Log("SVGExportSuccessInfo");
                }

#else
#error Implementation for SVGSnapshot logic is incomplete when using Aspose.SVG API.
#endif

                _ = env.RulePool.Produce(env.Evaluator.RankingHistory);
                env.Evaluator.ConfigCurrentRanking(0);

                // reset cells in the matrix
                env.Matrix.ResetCells();

                // count cfg
                count++;
            }

            //Console.WriteLine("-> Success!");
            //Logger.Info("Has successfully generated the executable rule.");
            ResourceHelper.Log("FinalSuccessInfo");
#region
            Console.Beep();
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------------");
            //ConsoleColor fgBak = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("Resulted rule= ");
            string resRule = "";
            for (int i = 0; i < env.RulePool.RuleLength; i++)
            {
                resRule += env.RulePool.GetLatest()[i] ? "1" : "0";
            }
            ResourceHelper.Log("ResultedRuleHint", resRule);

            // Export resultedReult to file
            ResourceHelper.Log("ExportRuleHint", env.ResultedRuleOutputFile);
            try
            {
                //using (StreamWriter sw = new StreamWriter(env.ResultedRuleOutputFile))
                //{
                //    sw.WriteLine(resRule);
                //    sw.Close();
                //}

                //Here should export XML formatted.
                IOSupport.RuleExporter.ExportRuleToXML(env, env.ResultedRuleOutputFile);

                //Logger.Info("Exported resulted rule to file successfully.");
                ResourceHelper.Log("ExportRuleSuccessInfo");
            }
            catch (Exception exp)
            {
                //Logger.Error("Failed to open export file to save resRule: " + exp.Message);
                ResourceHelper.Log("ExportRuleErrorHint", exp.Message);
            }

            //Console.ForegroundColor = fgBak;
            //Console.WriteLine();
            //Console.ForegroundColor = fgBak;
            Console.WriteLine("-------------------------------------------------------------------");
            //Logger.Info("Press ENTER key to close this window.");
            ResourceHelper.Log("ExitConfirm");
#endregion
#endif

#if __FORCE_TO_FAIL__ && __DEBUG_MODE__
            env.Evaluator.RankingHistory.Add(1024);
            env.Evaluator.RankingHistory.Add(-999);
            env.RulePool.ruleHistory.Add(new bool[512]);
            env.RulePool.ruleHistory.Add(new bool[512]);
            env.RulePool.Produce(env.Evaluator.RankingHistory);
#endif
            if (env.SVGProperty.Whether == true)
            {
#if __USE_ASPOSE_API__
#region
#warning Multiculture resx file not implemented warning.
                // Will export SVG file to destination
                Logger.Info($"Exporting SVG file to " + env.SVGProperty.Where);
                SVGDocument svgDoc = new SVGDocument();
                const string SVGNamespace = "http://www.w3.org/2000/svg";

                // Work around the svg document
                SVGExporter exporter = new SVGExporter();
                exporter.ExportEnvToSVG(ref svgDoc, SVGNamespace, env);

                // Export now
                svgDoc.Save(env.SVGProperty.Where);
                Logger.Info("SVG file exported successfully.");
#endregion
#else
                // Use SVGLib now...
                //Logger.Info($"Exporting SVG file to " + env.SVGProperty.Where);
                ResourceHelper.Log("ExportSVGHint", env.SVGProperty.Where);

                SvgDocument svgDocument = new SvgDocument();
                IOSupport.SVGExporter.ExportEnvToSVG(ref svgDocument, env);
                svgDocument.Write(env.SVGProperty.Where);
                //Logger.Info("SVG file exported successfully.");
                ResourceHelper.Log("ExportSVGSuccessInfo");
#endif
            }

            Console.ReadLine();
        }
    }
}
