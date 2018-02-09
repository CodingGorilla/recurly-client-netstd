using System;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    /// <summary>
    /// Transaction Errors
    /// </summary>
    public class TransactionError
    {
        private TransactionError()
        {
        }

        /// <summary>
        /// Transaction Error Code
        /// </summary>
        public string ErrorCode { get; internal set; }

        /// <summary>
        /// Category of error
        /// </summary>
        public string ErrorCategory { get; internal set; }

        /// <summary>
        /// A localized message you can show your customer
        /// </summary>
        public string CustomerMessage { get; internal set; }

        /// <summary>
        /// English advice for the merchant on how to resolve the transaction error
        /// </summary>
        public string MerchantAdvice { get; internal set; }

        /// <summary>
        /// The error code given by the gateway
        /// </summary>
        public string GatewayErrorCode { get; internal set; }

        internal static async Task<TransactionError> ReadFromXmlAsync(XmlReader reader)
        {
            var transactionError = new TransactionError();
            while(await reader.ReadAsync().ConfigureAwait(false))
            {
                if(reader.Name == "transaction_error" &&
                   reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch(reader.Name)
                {
                    case "error_code":
                        transactionError.ErrorCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;
                    case "error_category":
                        transactionError.ErrorCategory = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;
                    case "customer_message":
                        transactionError.CustomerMessage = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;
                    case "merchant_advice":
                        transactionError.MerchantAdvice = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;
                    case "gateway_error_code":
                        transactionError.GatewayErrorCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;
                }
            }

            return transactionError;
        }

        public override string ToString()
        {
            return string.Format("Code: \"{0}\" Category: \"{1}\" CustomerMessage: \"{2}\" MerchantAdvice: \"{3}\" GatewayCode: \"{4}\""
                , ErrorCode, ErrorCategory, CustomerMessage, MerchantAdvice, GatewayErrorCode);
        }
    }
}