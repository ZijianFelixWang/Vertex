using Vertex.Resources;
using System.Security.Cryptography;
using System.Xml.Linq;
using System;
using System.Collections.Generic;

namespace Vertex
{
    static class RuleExporter
    {
        public static void ExportRuleToXML(in Environment env, in string filename)
        {
            /*
             * Example XML output file content:
             * 
             * <?xml version="1.0" encoding="utf-8" ?>
             * <RuleDef VxVer="0.2">
             *  <Matrix size="32x32"/>
             *  <Language culture="zh-CN"/>
             *  <Rule sha384="c2d213b9cdb8a26ce9244161557ddd70e74d7aa736727438856e93800d6dfdcdd16b89626de92585e7c7ea9557ded782">
             *      0011111001001011001110010010001001101011001110110010010110101010100111010110001011110100100110011011011010001110100001
             *      1111000010001001110111010110011110110111000100001100111111011001101100000100010000001011011011111011100001001111101110
             *      1111010000111001101011101011010001001101011011101001010101110001110101010001100011111110001001010101011001110111000001
             *      0011000101111011011111011101001111010001010010111101111011101011000110100001100111111010110001010100100100111110100011
             *      1001111101001100110010000010100010110001
             *  </Rule>
             *  <VCIODistribution>
             *      <VCIO type="in" location="358" execute="false"/>
             *      <VCIO type="out" location="906" execute="true"/>
             *      <VCIO type="ri" location="107" on="ok" off="continue" timeout="1"/>
             *  </VCIODistribution>
             *  <Input from=""/>    <!-- User should complete this configuration -->
             *  <Output to=""/> <!-- User should complete this configuration -->
             * </RuleDef>
             */

            ResourceHelper.Log("BeginConstructXMLInfo");

            XDocument document = new XDocument();
            document.Add(new XElement("RuleDef", new XAttribute("VxVer", "0.2"),
                new XElement("Matrix", new XAttribute("size", env.Matrix.rawSize)),
                new XElement("Language", new XAttribute("culture", ResourceHelper.DefaultCulture)),
                new XElement("Rule", new XAttribute("sha384", GetRuleHash(env.RulePool.GetLatest())), GetRuleString(env.RulePool.GetLatest())),
                new XElement("VCIODistribution", GenerateVCIODistributionContent(env)),
                new XElement("Input", new XAttribute("from", ""), new XComment(ResourceHelper.GetContentByKey("ExportRuleXMLCommentContent"))),
                new XElement("Output", new XAttribute("to", ""), new XComment(ResourceHelper.GetContentByKey("ExportRuleXMLCommentContent")))
                ));

            ResourceHelper.Log("EndXMLConstructionInfo");

            document.Save(filename);

            ResourceHelper.Log("RuleXMLSaveHint", filename);
        }

        static private XElement[] GenerateVCIODistributionContent(in Environment env)
        {
            //XElement[] elements = new XElement[env.IO.GetVCIOCount()];
            List<XElement> elements = new List<XElement>(env.IO.GetVCIOCount());

            // For inputs
            foreach (VCIOCell vcio in env.IO.Inputs.Values)
            {
                elements.Add(new XElement("VCIO",
                    new XAttribute("type", "in"),
                    new XAttribute("location", vcio.ID),
                    new XAttribute("execute", vcio.Execute.ToString().ToLower())
                    ));
            }

            // For outputs
            foreach (VCIOCell vcio in env.IO.Outputs.Values)
            {
                elements.Add(new XElement("VCIO",
                    new XAttribute("type", "out"),
                    new XAttribute("location", vcio.ID),
                    new XAttribute("execute", vcio.Execute.ToString().ToLower())
                    ));
            }

            // For RIs
            // < VCIO type = "ri" location = "107" on = "OK" off = "Continue" timeout = "1" />

            //foreach (VCIOCell vcio in env.IO.ReadyIndicators.Values)
            //{
            //    ri = vcio;  // There should be only one ReadyIndicator in the env.
            //}

            VCIOCell ri = env.IO.ReadyIndicator.content;

            // Get riba string
            string RIBA_on = "", RIBA_off = "";
            try
            {
                RIBA_on = RIBAction2String(env.Evaluator.RIBinder.On);
                RIBA_off = RIBAction2String(env.Evaluator.RIBinder.Off);
            }
            catch (ArgumentOutOfRangeException exp)
            {
                ResourceHelper.Log(VxLogLevel.Error, exp.Message);
            }

            elements.Add(new XElement("VCIO",
                new XAttribute("type", "ri"),
                new XAttribute("location", ri.ID),
                new XAttribute("on", RIBA_on),
                new XAttribute("off", RIBA_off),
                new XAttribute("timeout", env.Evaluator.RIBinder.Timeout.ToString())
                ));

            return elements.ToArray();
        }

        private static string RIBAction2String(RIBAction action) => action switch
        {
            RIBAction.Break => "break",
            RIBAction.Continue => "continue",
            RIBAction.ToShiftNext => "toshiftnext",
            RIBAction.OK => "ok",
            _ => throw new ArgumentOutOfRangeException($"RIBAction {Convert.ToInt32(action)} is invalid.")
        };

        private static string GetRuleHash(bool[] rule)
        {
            using SHA384 shaM = new SHA384Managed();

            byte[] converted = new byte[rule.Length];
            for (int i = 0; i < rule.Length; i++)
            {
                converted[i] = Convert.ToByte(rule[i]);
            }

            byte[] result = shaM.ComputeHash(converted);
            shaM.Clear();
            shaM.Dispose();

            // convert byte[] to string
            return Convert.ToBase64String(result);
        }

        private static string GetRuleString(bool[] rule)
        {
            string str = "";
            for (int i = 0; i < rule.Length; i++)
            {
                str += rule[i] ? "1" : "0";
            }

            return str;
        }
    }
}
