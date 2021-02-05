using System;
using System.Linq;
using System.Xml.Linq;

namespace Vertex.IOSupport
{
    static class DynamicViewerHelper
    {
        public static (bool success, string detail) ConfigureDynamicViewer(string ViewerFilename, string SVGSnapshotFilename)
        {
            // Load HTML
            XDocument xml;
            try
            {
                xml = XDocument.Load(ViewerFilename);
            }
            catch (Exception exp)
            {
                return (false, "Failed to load " + ViewerFilename + " Details: " + exp.GetType().ToString() + "\t" + exp.Message);
            }

            // Configure HTML
            System.Collections.Generic.IEnumerable<XElement> query = from p in xml.Descendants("div")
                                                                     select p;

            if (query.Any())
            {
                XElement element = query.First();
                element.SetAttributeValue("name", SVGSnapshotFilename);
            }
            else
            {
                //return (false, "XML query returned null");
            }

            // Saves HTML
            xml.Save(ViewerFilename);
            return (true, "Success");
        }
    }
}
