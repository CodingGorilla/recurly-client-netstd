using System;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    public class Address
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }

        internal async Task ReadXmlAsync(XmlReader reader)
        {
            while(await reader.ReadAsync().ConfigureAwait(false))
            {
                if(reader.Name == "address" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                if(reader.NodeType != XmlNodeType.Element)
                    continue;

                switch(reader.Name)
                {
                    case "address1":
                        Address1 = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "address2":
                        Address2 = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "city":
                        City = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "state":
                        State = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "zip":
                        Zip = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "country":
                        Country = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;

                    case "phone":
                        Phone = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        break;
                }
            }
        }

        internal static async Task<Address> CreateFromReaderAsync(XmlReader reader)
        {
            var address = new Address();
            await address.ReadXmlAsync(reader).ConfigureAwait(false);
            return address;
        }

        internal void WriteToXml(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("address");

            xmlWriter.WriteElementString("address1", Address1);
            xmlWriter.WriteElementString("address2", Address2);
            xmlWriter.WriteElementString("city", City);
            xmlWriter.WriteElementString("state", State);
            xmlWriter.WriteElementString("zip", Zip);
            xmlWriter.WriteElementString("country", Country);
            xmlWriter.WriteElementString("phone", Phone);

            xmlWriter.WriteEndElement();
        }
    }
}