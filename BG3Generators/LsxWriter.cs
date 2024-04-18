using System.Xml;

namespace BG3Common
{
    /// <summary>
    /// Helper functions and extension for LSX files.
    /// </summary>
    public static class LsxWriter
    {
        /// <summary>
        /// Writes a generic LSX attribute.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="attributeName">The name of the attribute to write.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <param name="attributeValue">The value of the attribute.</param>
        public static void WriteLsxAttribute(this XmlWriter writer, string attributeName, string attributeType, string attributeValue)
        {
            using (var attribute = writer.WriteScopedElement("attribute"))
            {
                writer.WriteAttributeString("id", attributeName);
                writer.WriteAttributeString("type", attributeType);
                writer.WriteAttributeString("value", attributeValue);
            }
        }

        /// <summary>
        /// Writes an int32 LSX attribute.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="attributeName">The name of the attribute to write.</param>
        /// <param name="attributeValue">The value of the attribute.</param>
        public static void WriteLsxAttribute(this XmlWriter writer, string attributeName, int attributeValue)
        {
            using (var attribute = writer.WriteScopedElement("attribute"))
            {
                writer.WriteAttributeString("id", attributeName);
                writer.WriteAttributeString("type", "int32");
                writer.WriteAttributeString("value", attributeValue.ToString());
            }
        }
        /// <summary>
        /// Writes a float LSX attribute.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="attributeName">The name of the attribute to write.</param>
        /// <param name="attributeValue">The value of the attribute.</param>
        public static void WriteLsxAttribute(this XmlWriter writer, string attributeName, float attributeValue)
        {
            using (var attribute = writer.WriteScopedElement("attribute"))
            {
                writer.WriteAttributeString("id", attributeName);
                writer.WriteAttributeString("type", "float");
                writer.WriteAttributeString("value", attributeValue.ToString("G"));
            }
        }
        /// <summary>
        /// Writes a boolean LSX attribute.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="attributeName">The name of the attribute to write.</param>
        /// <param name="attributeValue">The value of the attribute.</param>
        public static void WriteLsxAttribute(this XmlWriter writer, string attributeName, bool attributeValue)
        {
            using (var attribute = writer.WriteScopedElement("attribute"))
            {
                writer.WriteAttributeString("id", attributeName);
                writer.WriteAttributeString("type", "bool");
                writer.WriteAttributeString("value", attributeValue.ToString());
            }
        }
    }
}
