using System.Xml;

namespace BG3Common
{
    /// <summary>
    /// Extensions to facilitate working with Xml.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Helper class to create a scoped 'using' statement close the end element after writing the start element.
        /// </summary>
        class ElementEnder : IDisposable
        {
            private readonly XmlWriter mWriter;

            public ElementEnder(XmlWriter writer)
            {
                mWriter = writer;
            }

            public void Dispose()
            {
                mWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes a scoped start element that will automatically write the end element when the 'using' statement completes.
        /// </summary>
        public static IDisposable WriteScopedElement(this XmlWriter writer, string elementName)
        {
            writer.WriteStartElement(elementName);
            return new ElementEnder(writer);
        }
    }
}
