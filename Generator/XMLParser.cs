using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace Generator
{
    public class XMLParser
    {
        private static readonly XmlSerializer ReferencePageSerializer = new XmlSerializer(typeof(OpenGLSpec.Registry));
        
        public OpenGLSpec.Registry Parse(string url) {
            OpenGLSpec.Registry result;

            var webRequest = WebRequest.Create(url);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            {
                using (var reader = new StreamReader(content))
                {
                    result = (OpenGLSpec.Registry)ReferencePageSerializer.Deserialize(reader);
                }
            }

            return result;
        }
    }
}