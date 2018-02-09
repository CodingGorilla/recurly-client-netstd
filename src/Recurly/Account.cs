using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    public class Account : RecurlyEntity
    {
        public Account(string accountCode) : base(Accounts.MakeAccountUri(accountCode))
        {
        }

        private Account(Uri entityUri) : base(entityUri)
        {
        }

        /// <summary>
        /// Account Code or unique ID for the account in Recurly
        /// </summary>
        public string AccountCode { get; private set; }

        public AccountState State { get; private set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string VatNumber { get; set; }
        public bool? TaxExempt { get; set; }
        public string EntityUseCode { get; set; }
        public string AcceptLanguage { get; set; }
        public string CcEmails { get; set; }
        public string HostedLoginToken { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool VatLocationValid { get; private set; }
        public Address Address { get; set; }
        public bool HasLiveSubscription { get; private set; }
        public bool HasActiveSubscription { get; private set; }
        public bool HasFutureSubscription { get; private set; }
        public bool HasCanceledSubscription { get; private set; }
        public bool HasPastDueInvoice { get; private set; }

        internal async Task ReadXmlAsync(XmlReader reader, string xmlName)
        {
            while(await reader.ReadAsync())
            {
                if(reader.Name == xmlName && reader.NodeType == XmlNodeType.EndElement)
                    break;

                if(reader.NodeType != XmlNodeType.Element)
                    continue;

                switch(reader.Name)
                {
                    case "account_code":
                        AccountCode = await reader.ReadElementContentAsStringAsync();
                        break;

                    case "state":
                        // TODO investigate in case of incoming data representing multiple states, as https://dev.recurly.com/docs/get-account says is possible
                        State = await reader.ReadElementContentAsEnumAsync<AccountState>();
                        break;

                    case "username":
                        Username = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "email":
                        Email = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "first_name":
                        FirstName = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "last_name":
                        LastName = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "company_name":
                        CompanyName = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "vat_number":
                        VatNumber = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "tax_exempt":
                        TaxExempt = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        break;

                    case "entity_use_code":
                        EntityUseCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "accept_language":
                        AcceptLanguage = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "cc_emails":
                        CcEmails = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "hosted_login_token":
                        HostedLoginToken = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "created_at":
                        CreatedAt = await reader.ReadElementContentAsDateTimeAsync().ConfigureAwait(false);
                        break;

                    case "updated_at":
                        UpdatedAt = await reader.ReadElementContentAsDateTimeAsync().ConfigureAwait(false);
                        break;

                    case "address":
                        Address = await Address.CreateFromReaderAsync(reader);
                        break;

                    case "vat_location_valid":
                        if(reader.GetAttribute("nil") == null)
                        {
                            VatLocationValid = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        }

                        break;

                    case "has_live_subscription":
                        HasLiveSubscription = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        break;

                    case "has_active_subscription":
                        HasActiveSubscription = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        break;

                    case "has_future_subscription":
                        HasFutureSubscription = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        break;

                    case "has_canceled_subscription":
                        HasCanceledSubscription = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        break;

                    case "has_past_due_invoice":
                        HasPastDueInvoice = await reader.ReadElementContentAsBooleanAsync().ConfigureAwait(false);
                        break;
                }
            }
        }

        internal static async Task<Account> CreateFromReaderAsync(XmlReader reader, Uri entityUri)
        {
            var finalUri = entityUri.IsAbsoluteUri ? new Uri(entityUri.PathAndQuery, UriKind.Relative) : entityUri;
            var account = new Account(finalUri);
            await account.ReadXmlAsync(reader, "account").ConfigureAwait(false);
            return account;
        }

        internal void WriteXml(XmlWriter xmlWriter, string xmlName)
        {
            xmlWriter.WriteStartElement(xmlName); // Start: account

            xmlWriter.WriteElementString("account_code", AccountCode);
            xmlWriter.WriteStringIfValid("username", Username);
            xmlWriter.WriteStringIfValid("email", Email);
            xmlWriter.WriteStringIfValid("first_name", FirstName);
            xmlWriter.WriteStringIfValid("last_name", LastName);
            xmlWriter.WriteStringIfValid("company_name", CompanyName);
            xmlWriter.WriteStringIfValid("accept_language", AcceptLanguage);
            xmlWriter.WriteStringIfValid("vat_number", VatNumber);
            xmlWriter.WriteStringIfValid("entity_use_code", EntityUseCode);
            xmlWriter.WriteStringIfValid("cc_emails", CcEmails);

            //TODO: xmlWriter.WriteIfCollectionHasAny("shipping_addresses", ShippingAddresses);

            if(TaxExempt.HasValue)
                xmlWriter.WriteElementString("tax_exempt", TaxExempt.Value.AsString());

            // TODO
            //if(_billingInfo != null)
            //    _billingInfo.WriteXml(xmlWriter);

            Address?.WriteToXml(xmlWriter);

            xmlWriter.WriteEndElement(); // End: account
        }


        public static readonly Account NotFound = CreateNotFoundEntity<Account>();
    }
}