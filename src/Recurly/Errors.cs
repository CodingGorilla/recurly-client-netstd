using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    /// <summary>
    /// An internal class for parsing and handling errors
    /// </summary>
    internal class Errors
    {
        /// <summary>
        /// Error objects message
        /// </summary>
        public Error[] ValidationErrors { get; internal set; }

        /// <summary>
        /// Transaction error if set
        /// </summary>
        public TransactionError TransactionError { get; internal set; }

        internal Errors() { }

        internal static async Task<Errors> ReadFromXmlAsync(XmlReader xmlReader)
        {
            var list = false;
            var validationErrors = new List<Error>();

            var errors = new Errors();
            while (await xmlReader.ReadAsync().ConfigureAwait(false))
            {
            
                if ((xmlReader.Name == "errors" || xmlReader.Name == "error") &&
                    xmlReader.NodeType == XmlNodeType.EndElement)
                    break;

                if (xmlReader.Name == "errors" && xmlReader.NodeType == XmlNodeType.Element)
                    list = true;

                if (xmlReader.Name == "error" && xmlReader.NodeType == XmlNodeType.Element)
                    validationErrors.Add(await Error.ReadFromXmlAsync(xmlReader, list).ConfigureAwait(false));

                if (xmlReader.Name == "transaction_error" && xmlReader.NodeType == XmlNodeType.Element)
                    errors.TransactionError = await TransactionError.ReadFromXmlAsync(xmlReader).ConfigureAwait(false);
            }

            errors.ValidationErrors = validationErrors.ToArray();
            return errors;
        }
    }
}
