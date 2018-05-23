using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace Generator
{
    public class XMLParser
    {
        private static readonly XmlSerializer ReferencePageSerializer = new XmlSerializer(typeof(OpenGLSpec.Registry));
        
        public static void CacheUrl(string url, string file) {
            if (!File.Exists(file)) {
                var webRequest = WebRequest.Create(url);

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(content))
                    {
                        File.WriteAllText(file, reader.ReadToEnd());
                    }
                }
            }
        }
        
        public OpenGLSpec.Registry Parse(string url, string file) {
            OpenGLSpec.Registry result;

            CacheUrl(url, file);
            using(var reader = File.OpenText(file)){
                result = (OpenGLSpec.Registry)ReferencePageSerializer.Deserialize(reader);
            }

            return result;
        }
    }
}