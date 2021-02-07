using System;
using FeatureEvolve = Vertex.Features.Evolve;
using ResourceHelper = Vertex.IOSupport.ResourceHelper;

namespace Vertex.Features
{
    static class FeatureLoader
    {
        static private FeatureType Str2ft(string feature)
            => feature.ToLower() switch
            {
                "evolve" => FeatureType.Evolve,
                "execute" => FeatureType.Execute,
                "help" => FeatureType.Help,
                "version" => FeatureType.Version,
                _ => throw new NotImplementedException("Feature " + feature.ToLower() + " is not implemented now.")
            };

        static public void LoadFeature(string feature, string param)
        {
            try
            {
                switch (Str2ft(feature))
                {
                    case FeatureType.Evolve:
                        ResourceHelper.Log(IOSupport.VxLogLevel.Verbose, "StartEvolveVerbose");
                        FeatureEvolve.StartEvolve(param);
                        break;
                    case FeatureType.Help:
                        ResourceHelper.Log(IOSupport.VxLogLevel.Info, "ReferToHelpIndication");
                        Environment.Exit(0);
                        break;
                    case FeatureType.Version:
                        // Version information was shown on application startup...
                        Environment.Exit(0);
                        break;
                    default:
                        throw new NotImplementedException("Feature " + feature.ToLower() + " is not implemented now.");
                        //break;
                }
            }
            catch (NotImplementedException exp)
            {
                ResourceHelper.Log(IOSupport.VxLogLevel.Fatal, "NotImplementedHint", exp.Message);
            }
        }
    }

    enum FeatureType
    {
        Evolve,
        Execute,
        Help,
        Version
    }
}
