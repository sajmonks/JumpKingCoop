using JumpKingCoop.Networking.Messages.Attributes;
using JumpKingCoop.Networking.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking.Serialization
{
    public static class MessageSerializer
    {
        private static bool isDictionaryBuilt = false;
        private static readonly Dictionary<int, Type> typeDictionary = new Dictionary<int, Type>();

        static MessageSerializer()
        {
            BuildDeserializationDictionary();
        }

        private static void BuildDeserializationDictionary()
        {
            if (!isDictionaryBuilt)
            {
                var assembly = Assembly.GetExecutingAssembly();
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute(typeof(MessageAttribute), true) != null)
                    {
                        var hashCode = type.Name.GetHashCode();
                        typeDictionary.Add(hashCode, type);
                    }
                }
                isDictionaryBuilt = true;
            }
        }

        public static byte[] SerializeMessage<T>(T toSerialize)
        {
            var xmlResult = string.Empty;
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize, namespaces);
                xmlResult = textWriter.ToString();
            }
            return Encoding.ASCII.GetBytes(xmlResult); ;
        }

        public static object DeserializeMessage(byte[] toDeserialize)
        {
            var xmlInput = Encoding.ASCII.GetString(toDeserialize);
            var document = XDocument.Parse(xmlInput);
            var typeHashCode = document.Root.Name.LocalName.GetHashCode();

            Type typeToDeserialize = null;
            if (!typeDictionary.TryGetValue(typeHashCode, out typeToDeserialize))
                return null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeToDeserialize);
            StringReader streamReader = new StringReader(xmlInput);
            var result = xmlSerializer.Deserialize(streamReader);
            streamReader.Close();
            return result;
        }
    }
}
