//#define __USE_ASPOSE_API__

#if __USE_ASPOSE_API__
using Aspose.Svg;
using Aspose.Svg.Dom;
using Aspose.Svg.Paths;
#else
using Svg;  // opensource freeware SVG
using System.Drawing;
#endif

//using NLog;
using System.Linq;
using Vertex.Kernel;

namespace Vertex.IOSupport
{
    static class SVGExporter
    {
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

#if __USE_ASPOSE_API__
        public void ExportEnvToSVG(ref SVGDocument document, in string SVGNamespace, in Environment env)
        {
            Logger.Info("Starting export function...");
            //SVGElement root = document.RootElement;

            // Get maxRank
            short maxRank = env.Evaluator.RankingHistory.Max();

            // to draw: env.Evaluator.RankingHistory
            float lastX = 100F;
            float lastY = yConvert(maxRank, env.Evaluator.RankingHistory[0], 100F);
            //Logger.Debug("The initial spot of SVG graph refers to rank " + env.Evaluator.RankingHistory[0]);

            for (int i = 1; i < env.Evaluator.RankingHistory.Count; i++, lastX += 10F)
            {
                //Logger.Debug($"The {i + 1}th spot of SVG graph refers to rank {env.Evaluator.RankingHistory[i]}");
                float y = yConvert(maxRank, env.Evaluator.RankingHistory[i], 100F);

                var conn = (SVGLineElement)document.CreateElementNS(SVGNamespace, "line");
                conn.X1.BaseVal.Value = lastX;
                conn.Y1.BaseVal.Value = lastY;
                conn.X2.BaseVal.Value = lastX + 10F;
                conn.Y2.BaseVal.Value = y;
                Logger.Debug($"Connecting SVG line from spot ({lastX},{lastY}) to ({lastX + 10F},{y})");

                conn.SetAttribute("stroke", "black");
                conn.SetAttribute("stroke-width", "3");
                document.RootElement.InsertBefore(conn, document.RootElement.FirstChild);

                lastY = y;
            }
        }
#else
        public static void ExportEnvToSVG(ref SvgDocument document, in Environment env)
        {
            //Logger.Info("Starting SVG export function...");
            ResourceHelper.Log(VxLogLevel.Verbose, "StartSVGExportInfo");

            // Get max rank
            short maxRank = env.Evaluator.RankingHistory.Max();

            float lastX = 10F;
            float lastY = YConvert(maxRank, env.Evaluator.RankingHistory[0], 10F);

            for (int i = 1; i < env.Evaluator.RankingHistory.Count; i++, lastX += 5F)
            {
                float nowY = YConvert(maxRank, env.Evaluator.RankingHistory[i], 10F);

                SvgLine conn = new SvgLine
                {
                    StartX = lastX,
                    StartY = lastY,
                    EndX = lastX + 5F,
                    EndY = nowY,
                    StrokeWidth = 3,
                    Stroke = new SvgColourServer(Color.Black)
                };

                document.Children.Add(conn);

                lastY = nowY;
            }

        }

#endif

        private static float YConvert(short maxRank, short rank, float topMargin)
        {
            /*
            Mechanism:
            y = 0      ------------------------------------------------------
            y = TOPM   ------------------------------------------------------   maxRank
            y = res                                                             rank

            res = 0 + topMargin + (maxRank - rank)
            */
            float res = topMargin + (maxRank - rank) * 5;
            return res;
        }
    }
}