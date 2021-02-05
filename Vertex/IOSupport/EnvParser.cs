using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Vertex.Kernel;
using Environment = Vertex.Kernel.Environment;
//using NLog;
//using Vertex.Resources;

namespace Vertex.IOSupport
{
    static class EnvParser
    {
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void ParseMetaData(ref Environment env, in XmlNode node)
        {
            foreach (XmlNode metadataNode in node.ChildNodes)
            {
                if (metadataNode.Name != "Meta")
                {
                    throw new FormatException($"In section Metadata: name of tag {metadataNode.Name} is not \"Meta\"");
                }

                switch (metadataNode.Attributes["key"].Value)
                {
                    case "VertexVersion":
                        if (float.Parse(metadataNode.Attributes["val"].Value) >= 0.3)
                        {
                            throw new NotSupportedException($"This environment profile requires Vertex version {metadataNode.Attributes["val"].Value} or higher.");
                        }
                        break;

                    case "Visibility":
                    case "Networking":
                    case "Terminal":
                        //Logger.Warn("This version doesn't support networking. This line of configuration is ignored.");
                        ResourceHelper.Log("NoNetworkingFeatureWarn");
                        break;

                    case "CMSize":
                        string toParse = metadataNode.Attributes["val"].Value;
                        string[] xAndY = toParse.Split('x');
                        env.Matrix = new Matrix(uint.Parse(xAndY[0]), uint.Parse(xAndY[1]))
                        {
                            // = new List<Cell>(Convert.ToInt32((env.Matrix.Columns * env.Matrix.Rows) - 1));
                            rawSize = toParse
                        };
                        break;

                    case "ShowRealTimeMatrix":
                        if (env.Matrix == null)
                        {
                            throw new FormatException("ShowRealTimeMatrix property must be placed after CMSize property!");
                        }
                        env.Matrix.ShowRealTimeMatrix = bool.Parse(metadataNode.Attributes["val"].Value);
                        break;

                    case "MultithreadingExecution":
                        if (env.Matrix == null)
                        {
                            throw new FormatException("MultithreadingExecution property must be placed after CMSize property!");
                        }
                        env.Matrix.UseMultiThreading = bool.Parse(metadataNode.Attributes["val"].Value);
                        break;

                    case "MutationMethod":
                        if (env.RulePool == null)
                        {
                            env.RulePool = new RulePool(512);   // fixed constant 512
                        }
                        env.RulePool.MutationMethod = metadataNode.Attributes["val"].Value.Trim().ToLower() switch
                        {
                            "flip" => MutationMethod.Flip,
                            "random" => MutationMethod.Random,
                            _ => throw new FormatException($"Unexpected MutationMethod \"{metadataNode.Attributes["val"].Value.Trim().ToLower()}\"")
                        };

                        break;

                    case "MutationCount":
                        if (env.RulePool == null)
                        {
                            throw new FormatException("MutationCount property must be placed after MutationMethod property!");
                        }
                        env.RulePool.MutationCount = ushort.Parse(metadataNode.Attributes["val"].Value);
                        break;

                    case "SVGOutputFile":
                        env.SVGProperty.Whether = true;
                        env.SVGProperty.Where = metadataNode.Attributes["val"].Value;
                        break;

                    case "SVGSnapshot":
                        env.SVGProperty.Frequency = ushort.Parse(metadataNode.Attributes["val"].Value);
                        break;

                    case "EnableDynamicViewer":
                        env.SVGProperty.UseDynamicViewer = bool.Parse(metadataNode.Attributes["val"].Value);
                        break;

                    case "ResultedRuleOutputFile":
                        env.ResultedRuleOutputFile = metadataNode.Attributes["val"].Value;
                        break;

                    case "Language":
                        env.Language = metadataNode.Attributes["val"].Value;
                        break;

                    default:
                        throw new FormatException($"Undefined metadata configuration key \"{metadataNode.Attributes["key"].Value}\".");
                }
            }
        }

        private static void ParseParameterList(ref Environment env, XmlNode evNode)
        {
            //_ = env.Evaluator.ParameterLists.FirstOrDefault(pl => pl.For == evNode.Attributes["for"].Value);
            //_ = env.Evaluator.ParameterLists.FirstOrDefault(pl => pl.For == evNode.Attributes["for"].Value).Parameters;

            // Now parses the parameters inside the list
            foreach (XmlNode plChild in evNode.ChildNodes)
            {
                if (plChild.Name != "Value")
                {
                    throw new FormatException($"Undefined tag met in parameter section: {plChild.Name}");
                }

                ParameterItem temp = new ParameterItem
                {
                    Index = uint.Parse(plChild.Attributes["index"].Value),
                    Type = plChild.Attributes["type"].Value,
                    Value = bool.Parse(plChild.InnerText.Trim())
                };

                if (temp.Type != "Bool")
                {
                    throw new NotSupportedException("This version of Vertex only supports Boolean-typed cellular automations");
                }

                // DEBUG
                //for (int d = 0; d < plChild.Attributes.Count; d++)
                //{
                //    Console.Write(plChild.Attributes[d].Value + " ");
                //}
                //Console.WriteLine("");
                // DEBUG END

                if (env.Evaluator.ParameterLists?.FirstOrDefault(pl => pl.For == plChild.ParentNode.Attributes["for"].Value)?.For == null)
                {
                    // required pl does not exist
                    // so will create it!
                    env.Evaluator.ParameterLists.Add(new ParameterList
                    {
                        For = evNode.Attributes["for"].Value,
                        Parameters = new List<ParameterItem>()
                    });
                }

                env.Evaluator.ParameterLists.FirstOrDefault(pl => pl.For == evNode.Attributes["for"].Value).Parameters.Add(temp);
            }
        }

        private static void ParseAnswerList(ref Environment env, in XmlNode evNode)
        {
            //env.Evaluator.AnswerList.For = evNode.Attributes["for"].Value;

            // Add this answerList later.
            ParameterList apl = new ParameterList
            {
                For = evNode.Attributes["for"].Value
            };

            foreach (XmlNode alChild in evNode.ChildNodes)
            {
                switch (alChild.Name)
                {
                    case "Value":
                        ParameterItem temp = new ParameterItem
                        {
                            Index = uint.Parse(alChild.Attributes["index"].Value),
                            Type = alChild.Attributes["type"].Value,
                            Value = bool.Parse(alChild.InnerText.Trim())
                        };

                        if (temp.Type != "Bool")
                        {
                            throw new NotSupportedException("This version of Vertex only supports Boolean-typed cellular automations");
                        }

                        if (apl.Parameters == null)
                        {
                            apl.Parameters = new List<ParameterItem>();
                        }

                        apl.Parameters.Add(temp);
                        break;

                    #region Commented code about Calculator tag.
                    //case "Calculator":
                    //    // To implement logic for parsing calculator!
                    //    // In this version 'for' attribute is ignored.
                    //    Console.WriteLine("[WARNING] Only one calculator is supported in this version of Vertex.");
                    //    foreach (XmlNode calcNode in alChild.ChildNodes)
                    //    {
                    //        if (calcNode.Name != "Calculation")
                    //        {
                    //            throw new FormatException($"Undefined tag met in calculator definition section: {calcNode.Name}");
                    //        }

                    //        CalculationType toPerform = calcNode.Attributes["perform"].Value switch
                    //        {
                    //            "AND" => CalculationType.AND,
                    //            "OR" => CalculationType.OR,
                    //            "XOR" => CalculationType.XOR,
                    //            "NOT" => CalculationType.NOT,
                    //            "NAND" => CalculationType.NAND,
                    //            "ADD" => CalculationType.ADD,
                    //            "SUB" => CalculationType.SUB,
                    //            "MUL" => CalculationType.MUL,
                    //            "DIV" => CalculationType.DIV,
                    //            _ => throw new NotSupportedException($"Unsupported calculation type: {calcNode.Attributes["perform"].Value}")
                    //        };

                    //        Console.WriteLine("[WARNING] In this experimental very first version, opr propery is ignored...");
                    //        Console.WriteLine("[WARNING] Only one calculation is supported in this calculator version now...");

                    //        Calculation tmp = new Calculation
                    //        {
                    //            ToPerform = toPerform
                    //        };
                    //    }
                    //    break;
                    #endregion

                    default:
                        throw new FormatException($"Undefined tag met in answer section: {alChild.Name}");
                }
            }

            env.Evaluator.AnswerLists.Add(apl);
        }

        private static void ParseRIBinder(ref Environment env, in XmlNode evNode)
        {
            RIBinder rIBinder = new RIBinder
            {
                Indicator = evNode.Attributes["indicator"].Value,
                Timeout = uint.Parse(evNode.Attributes["timeout"].Value),
                On = evNode.Attributes["on"].Value switch
                {
                    "ToShiftNext" => RIBAction.ToShiftNext,
                    "Continue" => RIBAction.Continue,
                    "Break" => RIBAction.Break,
                    "OK" => RIBAction.OK,
                    _ => throw new NotSupportedException($"Unsupported RIBAction type: {evNode.Attributes["on"].Value}")
                },
                Off = evNode.Attributes["off"].Value switch
                {
                    "ToShiftNext" => RIBAction.ToShiftNext,
                    "Continue" => RIBAction.Continue,
                    "Break" => RIBAction.Break,
                    "OK" => RIBAction.OK,
                    _ => throw new NotSupportedException($"Unsupported RIBAction type: {evNode.Attributes["off"].Value}")
                }
            };

            env.Evaluator.RIBinder = rIBinder;
        }

        private static void ParseEvaluator(ref Environment env, in XmlNode node)
        {
            env.Evaluator = new Evaluator();
            foreach (XmlNode evNode in node.ChildNodes)
            {
                switch (evNode.Name)
                {
                    case "ParameterList":
                        ParseParameterList(ref env, evNode);
                        break;

                    case "AnswerList":
                        ParseAnswerList(ref env, evNode);
                        break;

                    case "RIBinder":
                        ParseRIBinder(ref env, evNode);
                        break;
                }
            }
        }

        private static void ParseInput(ref Environment env, in XmlNode ioNode)
        {
            env.IO.Inputs.Add(ioNode.Attributes["name"].Value, new VCIOCell
            {
                Type = VCIOType.Input,
                ID = default,   // Here should not be default, should be its location in the grand list of Matrix.Cells
                Name = ioNode.Attributes["name"].Value,
                Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
            });
        }

        private static void ParseOutput(ref Environment env, in XmlNode ioNode)
        {
            env.IO.Outputs.Add(ioNode.Attributes["name"].Value, new VCIOCell
            {
                Type = VCIOType.Output,
                ID = default,
                Name = ioNode.Attributes["name"].Value,
                Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
            });
        }

        private static void ParseRI(ref Environment env, in XmlNode ioNode)
        {
            //env.IO.ReadyIndicators.Add(ioNode.Attributes["id"].Value, new VCIOCell
            //{
            //    Type = VCIOType.ReadyIndicator,
            //    ID = default,
            //    Name = ioNode.Attributes["name"].Value,
            //    Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
            //});

            env.IO.ReadyIndicator.name = ioNode.Attributes["name"].Value;
            env.IO.ReadyIndicator.content = new VCIOCell
            {
                Type = VCIOType.ReadyIndicator,
                ID = default,
                Name = ioNode.Attributes["name"].Value,
                Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
            };
        }

        private static void RandomVCIODistribution(ref Environment env)
        {
            List<bool> usedLocations = new List<bool>(env.Matrix.Cells.Capacity);

            // Initialize usedLocations array
            for (int k = 0; k < usedLocations.Capacity; k++)
            {
                usedLocations.Add(false);
            }

            Random rand = new Random();

            // ToDo: Convert to 'for'
            for (int index = 0; index < env.IO.Inputs.Count; index++)
            {
                // env.IO.Inputs[env.IO.Inputs.Keys.ToArray()[index]].ID
                do
                {
                    int loc = rand.Next(env.Matrix.Cells.Capacity);
                    if (usedLocations[loc] == false)
                    {
                        // Location usable
                        //Console.WriteLine("inputs -> location = " + loc);
                        env.IO.Inputs[env.IO.Inputs.Keys.ToArray()[index]].ID = (uint)loc;
                        env.Matrix.Cells[loc] = env.IO.Inputs[env.IO.Inputs.Keys.ToArray()[index]];
                        break;
                    }
                } while (true);
            }

            //foreach (var one in env.IO.Inputs)
            //{
            //    do
            //    {
            //        int loc = rand.Next(env.Matrix.Cells.Capacity - 1);
            //        if (usedLocations[loc] == false)
            //        {
            //            // Location usable
            //            one.Value.ID = (uint)loc;
            //            env.Matrix.Cells[loc] = one.Value;
            //            break;
            //        }
            //    } while (true);
            //}

            for (int index = 0; index < env.IO.Outputs.Count; index++)
            {
                do
                {
                    int loc = rand.Next(env.Matrix.Cells.Capacity - 1);
                    if (usedLocations[loc] == false)
                    {
                        // Location usable
                        env.IO.Outputs[env.IO.Outputs.Keys.ToArray()[index]].ID = (uint)loc;
                        env.Matrix.Cells[loc] = env.IO.Outputs[env.IO.Outputs.Keys.ToArray()[index]];
                        break;
                    }
                } while (true);
            }

            //foreach (var one in env.IO.Outputs)
            //{
            //    do
            //    {
            //        int loc = rand.Next(0, env.Matrix.Cells.Capacity - 1);
            //        if (usedLocations[loc] == false)
            //        {
            //            // Location usable
            //            one.Value.ID = (uint)loc;
            //            env.Matrix.Cells[loc] = one.Value;
            //            break;
            //        }
            //    } while (true);
            //}

            //for (int index = 0; index < env.IO.ReadyIndicators.Count; index++)
            //{
            //    do
            //    {
            //        int loc = rand.Next(env.Matrix.Cells.Capacity - 1);
            //        if (usedLocations[loc] == false)
            //        {
            //            // Location usable
            //            env.IO.ReadyIndicators[env.IO.ReadyIndicators.Keys.ToArray()[index]].ID = (uint)loc;
            //            env.Matrix.Cells[loc] = env.IO.ReadyIndicators[env.IO.ReadyIndicators.Keys.ToArray()[index]];
            //            break;
            //        }
            //    } while (true);
            //}

            // distribute ready indicator (only one)
            do
            {
                int loc = rand.Next(env.Matrix.Cells.Capacity - 1);
                if (usedLocations[loc] == false)
                {
                    // Location usable
                    env.IO.ReadyIndicator.content.ID = (uint)loc;
                    env.Matrix.Cells[loc] = env.IO.ReadyIndicator.content;
                    break;
                }
            } while (true);

            //foreach (var one in env.IO.ReadyIndicators)
            //{
            //    do
            //    {
            //        int loc = rand.Next(0, env.Matrix.Cells.Capacity - 1);
            //        if (usedLocations[loc] == false)
            //        {
            //            // Location usable
            //            one.Value.ID = (uint)loc;
            //            env.Matrix.Cells[loc] = one.Value;
            //            break;
            //        }
            //    } while (true);
            //}
        }

        private static void ParseAndDistributeVCIO(ref Environment env, in XmlNode ioNode)
        {
            // Configure the matrix now based on this property...
            switch (ioNode.Attributes["method"].Value)
            {
                case "random":
                    RandomVCIODistribution(ref env);
                    break;

                default:
                    throw new FormatException($"Unknown VCIODistribution: {ioNode.Attributes["method"].Value}. Try: \'Random\'");
            }
        }

        private static void ParseIO(ref Environment env, in XmlNode node)
        {
            if (env.IO == null)
            {
                //Console.Error.WriteLine("Warning null env.IO");
                env.IO = new IO();
            }
            foreach (XmlNode ioNode in node.ChildNodes)
            {
                switch (ioNode.Name)
                {
                    case "Input":
                        ParseInput(ref env, ioNode);
                        break;

                    case "Output":
                        ParseOutput(ref env, ioNode);
                        break;

                    case "ReadyIndicator":
                        ParseRI(ref env, ioNode);
                        break;

                    case "VCIODistribution":
                        ParseAndDistributeVCIO(ref env, ioNode);
                        break;

                    default:
                        throw new FormatException($"Undefined tag \"{ioNode.Name}\"");
                }
            }
        }

        public static Environment ParseFromXML(XmlDocument document)
        {
            Environment env = new Environment();
            try
            {
                int rootSearcher = 0;
                XmlNode root = document.ChildNodes[rootSearcher];
                while (root.Name == "xml")
                {
                    root = document.ChildNodes[++rootSearcher];
                }

                if (root.Name != "Environment")
                {
                    // Oops! It is not an Environment configuration.
                    throw new FormatException($"Root tag \"{root.Name}\"is not Environment");
                }

                env.ID = root.Attributes["id"].Value;

                // Now begins parsing
                foreach (XmlNode node in root.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Metadata":
                            // Now parses metadata...
                            ParseMetaData(ref env, node);
                            break;

                        case "Evaluator":
                            // Now parses the Evaluator...
                            ParseEvaluator(ref env, node);
                            break;

                        case "IO":
                            // Now parses the IODefinition...
                            ParseIO(ref env, node);
                            break;

                        default:
                            throw new FormatException($"Undefined tag \"{node.Name}\"");
                            //break;
                    }
                }
            }
            catch (FormatException ex)
            {
                //Logger.Error("XML Syntax error");
                ResourceHelper.Log(VxLogLevel.Fatal, "EnvSyntaxError", ex.Message);
                System.Environment.Exit(-1);
            }
            catch (NotSupportedException ex)
            {
                //Logger.Warn("Something is not supported now...");
                //Logger.Warn(ex.Message);
                ResourceHelper.Log(VxLogLevel.Fatal, "NotSupportedError", ex.Message);

                System.Environment.Exit(-1);
            }

            return env;
        }
    }
}
