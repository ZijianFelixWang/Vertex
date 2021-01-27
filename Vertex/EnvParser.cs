using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Vertex
{
    static class EnvParser
    {
        public static void ParseMetaData(ref Environment env, in XmlNode node)
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
                        if (float.Parse(metadataNode.Attributes["val"].Value) >= 0.2)
                        {
                            throw new NotSupportedException($"This environment profile requires Vertex version {metadataNode.Attributes["val"].Value} or higher.");
                        }
                        break;

                    case "Visibility":
                    case "Networking":
                    case "Terminal":
                        Console.Error.WriteLine("[WARNING] This version doesn't support networking. This line of configuration is ignored.");
                        break;

                    case "CMSize":
                        string toParse = metadataNode.Attributes["val"].Value;
                        string[] xAndY = toParse.Split('x');
                        env.Matrix = new Matrix(uint.Parse(xAndY[0]), uint.Parse(xAndY[1]));
                        // = new List<Cell>(Convert.ToInt32((env.Matrix.Columns * env.Matrix.Rows) - 1));
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

                    default:
                        throw new FormatException($"Undefined metadata configuration key \"{metadataNode.Attributes["key"].Value}\".");
                }
            }
        }

        public static void ParseEvaluator(ref Environment env, in XmlNode node)
        {
            env.Evaluator = new Evaluator();
            foreach (XmlNode evNode in node.ChildNodes)
            {
                switch (evNode.Name)
                {
                    case "ParameterList":
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
                        break;

                    case "AnswerList":
                        env.Evaluator.AnswerList.For = evNode.Attributes["for"].Value;

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

                                    if (env.Evaluator.AnswerList.Parameters == null)
                                    {
                                        env.Evaluator.AnswerList.Parameters = new List<ParameterItem>();
                                    }

                                    env.Evaluator.AnswerList.Parameters.Add(temp);
                                    break;

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

                                default:
                                    throw new FormatException($"Undefined tag met in answer section: {alChild.Name}");
                            }
                        }
                        break;

                    case "RIBinder":
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
                        break;
                }
            }
        }

        public static void ParseIO(ref Environment env, in XmlNode node)
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
                        env.IO.Inputs.Add(ioNode.Attributes["id"].Value, new VCIOCell
                        {
                            Type = VCIOType.Input,
                            ID = default,   // Here should not be default, should be its location in the grand list of Matrix.Cells
                            Name = ioNode.Attributes["name"].Value,
                            Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
                        });
                        break;

                    case "Output":
                        env.IO.Outputs.Add(ioNode.Attributes["id"].Value, new VCIOCell
                        {
                            Type = VCIOType.Output,
                            ID = default,
                            Name = ioNode.Attributes["name"].Value,
                            Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
                        });
                        break;

                    case "ReadyIndicator":
                        env.IO.ReadyIndicators.Add(ioNode.Attributes["id"].Value, new VCIOCell
                        {
                            Type = VCIOType.ReadyIndicator,
                            ID = default,
                            Name = ioNode.Attributes["name"].Value,
                            Execute = bool.Parse(ioNode.Attributes["execute"].Value.Trim())
                        });

                        break;

                    case "VCIODistribution":
                        // Configure the matrix now based on this property...
                        switch (ioNode.Attributes["method"].Value)
                        {
                            case "random":
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

                                for (int index = 0; index < env.IO.ReadyIndicators.Count; index++)
                                {
                                    do
                                    {
                                        int loc = rand.Next(env.Matrix.Cells.Capacity - 1);
                                        if (usedLocations[loc] == false)
                                        {
                                            // Location usable
                                            env.IO.ReadyIndicators[env.IO.ReadyIndicators.Keys.ToArray()[index]].ID = (uint)loc;
                                            env.Matrix.Cells[loc] = env.IO.ReadyIndicators[env.IO.ReadyIndicators.Keys.ToArray()[index]];
                                            break;
                                        }
                                    } while (true);
                                }

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
                                break;

                            default:
                                throw new FormatException($"Unknown VCIODistribution: {ioNode.Attributes["method"].Value}. Try: \'Random\'");
                        }
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
                Console.Error.WriteLine("[FATAL] XML Syntax error");
                Console.Error.WriteLine(ex.ToString());
                System.Environment.Exit(-1);
            }
            catch (NotSupportedException ex)
            {
                Console.Error.WriteLine("[FATAL] Some operations are not supported now...");
                Console.Error.WriteLine(ex.ToString());
                System.Environment.Exit(-1);
            }

            return env;
        }
    }
}
