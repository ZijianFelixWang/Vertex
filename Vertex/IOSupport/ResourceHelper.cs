using System;
using Vertex.Resources;
using NLog;

namespace Vertex.IOSupport
{
    static class ResourceHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string DefaultCulture = "en-US";

        public static string GetContentByKeyCulture(string key, string culture = "en-US") => culture.ToLower() switch
        {
            "en-us" => en_US.ResourceManager.GetString(key),
            "zh-cn" => zh_CN.ResourceManager.GetString(key),
            _ => throw new ArgumentException("Cannot find specified culture: " + culture, nameof(culture))
        };

        public static string GetContentByKey(string key) => GetContentByKeyCulture(key, DefaultCulture);

        public static void Log(string messageKey, string addition = "")
        {
            // Default loglv is Info
            Log(VxLogLevel.Info, messageKey, addition);
        }

        public static void Log(VxLogLevel level, string messageKey, string addition = "")
        {
            switch (level)
            {
                case VxLogLevel.Debug:
                    Logger.Debug(GetContentByKey(messageKey) + addition);
                    break;
                case VxLogLevel.Info:
                    Logger.Info(GetContentByKey(messageKey) + addition);
                    break;
                case VxLogLevel.Warn:
                    Logger.Warn(GetContentByKey(messageKey) + addition);
                    break;
                case VxLogLevel.Error:
                    Logger.Error(GetContentByKey(messageKey) + addition);
                    break;
                case VxLogLevel.Fatal:
                    Logger.Fatal(GetContentByKey(messageKey) + addition);
                    break;
            }
        }
    }

    enum VxLogLevel
    {
        Debug, Info, Warn, Error, Fatal
    }
}
