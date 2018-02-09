using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    internal static class XmlReaderExtensions
    {
        public static async Task<bool> ReadElementContentAsBooleanAsync(this XmlReader reader)
        {
            var elementContent = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
            var result = XmlConvert.ToBoolean(elementContent);
            return result;
        }

        public static async Task<TEnum> ReadElementContentAsEnumAsync<TEnum>(this XmlReader reader, bool ignoreCase = true) where TEnum : struct
        {
            var elementContent = await reader.ReadElementContentAsStringAsync().ConfigureAwait(true);
            var success = Enum.TryParse<TEnum>(elementContent.ToPascalCase(), ignoreCase, out var result);
            if(!success)
                throw new ArgumentException($"Unable to parse {elementContent} as {typeof(TEnum).Name}");

            return result;
        }

        public static async Task<DateTime> ReadElementContentAsDateTimeAsync(this XmlReader reader)
        {
            var elementContent = await reader.ReadElementContentAsStringAsync().ConfigureAwait(true);
            var result = XmlConvert.ToDateTime(elementContent, XmlDateTimeSerializationMode.RoundtripKind);
            return result;
        }
    }

    internal static class XmlWriterExtensions
    {
        /// <summary>
        /// Convenience implementation of <see cref="T:System.Xml.XmlWriter.WriteElementString(string, string)"/> that guards it with
        /// a check if <paramref name="value"/> is null or empty, writing the value if it is not null or empty.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> that will be written to.</param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void WriteStringIfValid(this XmlWriter writer, string localName, string value)
        {
            if(!value.IsNullOrEmpty())
                writer.WriteElementString(localName, value);
        }

        /// <summary>
        /// If the given collection <paramref name="items"/> has any elements, writes the contents to the <see cref="T:System.Xml.XmlTextWriter"/> <paramref name="writer"/>
        /// using the provided <paramref name="localName"/> and <paramref name="stringValue"/> <see cref="T:System.Func{T,TResult}"/>s.
        /// </summary>
        /// <typeparam name="T">The type of elements in <paramref name="items"/>.</typeparam>
        /// <param name="writer">The <see cref="T:System.Xml.XmlTextWriter"/> to write to.</param>
        /// <param name="collectionName">The value to use for the encompassing XML tag if the collection is written.</param>
        /// <param name="items">The collection to test and then write if it has any elements</param>
        /// <param name="localName">A <see cref="T:System.Func{T,TResult}"/> that provides the localName for the XML element written for each item.</param>
        /// <param name="stringValue">A <see cref="T:System.Func{T,TResult}"/> that provides the value for the XML element written for each item.</param>
        public static void WriteIfCollectionHasAny<T>(this XmlWriter writer, string collectionName, IEnumerable<T> items,
                                                      Func<T, string> localName, Func<T, string> stringValue)
        {
            if(!items.Any())
                return;
            writer.WriteStartElement(collectionName);
            foreach(var item in items)
            {
                writer.WriteElementString(localName(item), stringValue(item));
            }

            writer.WriteEndElement();
        }
    }
}