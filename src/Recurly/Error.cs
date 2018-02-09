using System;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    /// <summary>
    /// An individual error message.
    /// For more information, please visit https://dev.recurly.com/docs/api-validation-errors
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Field causing the error, if appropriate.
        /// </summary>
        public string Field { get; internal set; }

        /// <summary>
        /// Error code set for certain transaction failures.
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// Error symbol
        /// </summary>
        public string Symbol { get; internal set; }

        /// <summary>
        /// Error details
        /// </summary>
        public string Details { get; internal set; }

        internal static async Task<Error> ReadFromXmlAsync(XmlReader reader, bool fromList)
        {
            var error = new Error();
            if (fromList)
            {
                // list of errors returned
                // <errors>
                //    <error field="model_name.field_name" symbol="not_a_number" lang="en-US">is not a number</error>
                // </errors>
                if (reader.HasAttributes)
                {
                    error.Field = ReadAttr("field", reader);
                    error.Code = ReadAttr("code", reader);
                    error.Symbol = ReadAttr("symbol", reader);
                }

                error.Message = await reader.ReadElementContentAsStringAsync();
                return error;
            }

            // single error returned
            // <error>
            //    <symbol>asdf</symbol>
            //    <description>asdfasdf </symbol>
            // </error>
            while (await reader.ReadAsync())
            {
                switch (reader.Name)
                {
                    case "symbol":
                        error.Symbol = reader.ReadElementContentAsString();
                        break;
                    case "description":
                        error.Message = reader.ReadElementContentAsString();
                        break;
                    case "details":
                        error.Details = reader.ReadElementContentAsString();
                        break;
                }

                if (reader.Name == "error" && reader.NodeType == XmlNodeType.EndElement)
                    break;
            }

            return error;
        }

        public override string ToString()
        {
            return string.Format("{0} | Field: \"{1}\" Code: \"{2}\" Symbol: \"{3}\" Details: \"{4}\""
                , Message, Field, Code, Symbol, Details);
        }

        /// <summary>
        /// Reads the attribute with the given `name` from the `reader` without throwing
        /// </summary>
        /// <param name="name"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static string ReadAttr(string name, XmlReader reader)
        {
            try
            {
                return reader.GetAttribute(name);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "";
            }
        }

    }
}
