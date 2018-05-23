using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace Generator
{
    public class DocHandler
    {
        public Dictionary<string, string> map = new Dictionary<string, string>();
        private HashSet<string> gl4Files = new HashSet<string>();
        public class FunctionPrototype {
            public string Name;
            public List<string> Params = new List<string>();
        }
        public Dictionary<string, FunctionPrototype> Prototypes = new Dictionary<string, FunctionPrototype>();

        private Regex indexRegex = new Regex("<li><a href=\"(.+?).xhtml\" target=\"pagedisplay\">(.+?)</a></li>", RegexOptions.Compiled);
        private Regex functionsRegex = new Regex("<funcsynopsis>\\s*?<funcprototype>(.*?)</funcprototype>\\s*?</funcsynopsis>", RegexOptions.Compiled | RegexOptions.Singleline);
        private Regex functionNameRegex = new Regex("<funcdef>.*?<function>(.+?)</function></funcdef>", RegexOptions.Compiled);
        private Regex functionParamRegex = new Regex("<paramdef>.*?<parameter>(.+?)</parameter></paramdef>", RegexOptions.Compiled);
        public void DownloadGL4() {
            if(!Directory.Exists("cache")) Directory.CreateDirectory("cache");

            var index = "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl4/html/indexflat.php";
            var contents = string.Empty;
            var webRequest = WebRequest.Create(index);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            {
                using (var reader = new StreamReader(content))
                {
                    contents = reader.ReadToEnd();
                }
            }

            var matches = indexRegex.Matches(contents);
            foreach(Match match in matches) {
                var name = match.Groups[1].Value;
                gl4Files.Add(name);
                Console.WriteLine("DocHandler - GL4 - Parsing: {0}", name);
                var xmlContents = string.Empty;
                if(!File.Exists("cache/" + name + ".xml")) {
                    try {
                        var xmlRequest = WebRequest.Create("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl4/" + name + ".xml");

                        using (var response = xmlRequest.GetResponse())
                        using (var content = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(content))
                            {
                                xmlContents = reader.ReadToEnd();
                                File.WriteAllText("cache/" + name + ".xml", xmlContents);
                            }
                        }
                    } catch {
                        //System.Console.WriteLine("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl4/" + name + ".xml");
                    }
                } else {
                    xmlContents = File.ReadAllText("cache/" + name + ".xml");
                }


                var functions = functionsRegex.Matches(xmlContents);
                foreach(Match function in functions) {
                    var nameMatch = functionNameRegex.Match(function.Groups[1].Value);
                    var functionName = nameMatch.Groups[1].Value;
                    var paramsMatches = functionParamRegex.Matches(function.Groups[1].Value);
                    var f = new FunctionPrototype();
                    f.Name = functionName;
                    foreach(Match param in paramsMatches) {
                        f.Params.Add(param.Groups[1].Value);
                    }
                    Prototypes[functionName] = f;

                    if(!map.ContainsKey(functionName)) {
                        map[functionName] = name;
                    }
                }
            }
        }

        private Regex summaryRegex = new Regex("<refpurpose>(.+?)</refpurpose>", RegexOptions.Compiled);
        private Regex descriptionRegex = new Regex("id=\"description\"><title>Description</title>\\s*?<para>(.*?)</para>", RegexOptions.Singleline | RegexOptions.Compiled);
        public void WriteMainDoc(IndentedStringBuilder sb, string name) {
            sb.AppendLine("/// <summary>");
            if(map.ContainsKey(name)) {
                if(File.Exists("cache/" + map[name] + ".xml")) {
                    var contents = File.ReadAllText("cache/" + map[name] + ".xml");
                    var match1 = summaryRegex.Match(contents);
                    if(match1.Success) {
                        var summary = match1.Groups[1].Value;
                        summary = StripHTML(summary);
                        sb.AppendLine("/// " + summary);
                    } else {
                        sb.AppendLine("/// ");
                    }

                    var match2 = descriptionRegex.Match(contents);
                    if(match2.Success) {
                        var description = match2.Groups[1].Value;
                        description = StripHTML(description);
                        description = description.Replace('\n', ' ').Trim(new char[] { ' ', '\t', '.' });
                        while (description.Contains("  ")) description = description.Replace("  ", " ");

                        sb.AppendLine("/// <para>");
                        if (description.StartsWith("Gl")) description = description.Replace("Gl", "gl");
                        WriteMultiLine(sb, description);
                        sb.AppendLine("/// </para>");
                    }
                }
            }
            sb.AppendLine("/// </summary>");
        }

        private static void WriteMultiLine(IndentedStringBuilder sb, string text, int maxLine = 100) {
            while (text.Length > maxLine)
            {
                int i = maxLine;
                for (; i > 0; i--) if (text[i] == ' ') break;
                sb.AppendLine("/// " + text.Substring(0, i).Trim());
                text = text.Substring(i + 1).Trim();
            }
            sb.AppendLine("/// " + text.Trim());
        }

        public void WriteParameter(IndentedStringBuilder sb, string function, string parameter, string parameterOrig) {
            if(map.ContainsKey(function)) {
                if(File.Exists("cache/" + map[function] + ".xml")) {
                    var contents = File.ReadAllText("cache/" + map[function] + ".xml");
                    var regex = new Regex("<term>.*?<parameter>"+parameterOrig+"</parameter>.*?</term>\\s*?<listitem>\\s*?<para>(.*?)</para>", RegexOptions.Singleline);
                    var matches = regex.Matches(contents);
                    if(matches.Count >= 1) {
                        Match match = matches[0];
                        var parameterText = match.Groups[1].Value;
                        parameterText = parameterText.Replace('\n', ' ').Trim(new char[] { '\r', '\n', '\t', ' ' });
                        parameterText = StripHTML(parameterText);
                        while (parameterText.Contains("  ")) parameterText = parameterText.Replace("  ", " ");

                        sb.AppendLine("/// <param name=\"{0}\">", parameter);
                        WriteMultiLine(sb, parameterText);
                        sb.AppendLine("/// </param>");
                        return;
                    }
                    //System.Console.WriteLine("{0}, {1}, {2}, {3}", function, map[function], parameter, parameterOrig);
                }
            }
            sb.AppendLine("/// <param name=\"{0}\"> </param>", parameter);
        }

        private static string StripHTML(string data)
        {
            return Regex.Replace(data, @"<(.|\n)*?>", string.Empty);
        }

        private Regex indexRegex2 = new Regex("<tr><td><a target=\"pagedisp\" href=\"(.+?).xml\">\\1</a></td></tr>", RegexOptions.Compiled);
        public void DownloadGL2() {
            if(!Directory.Exists("cache")) Directory.CreateDirectory("cache");

            var index = "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl2.1/xhtml/index.html";
            var contents = string.Empty;
            var webRequest = WebRequest.Create(index);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            {
                using (var reader = new StreamReader(content))
                {
                    contents = reader.ReadToEnd();
                }
            }

            var matches = indexRegex2.Matches(contents);
            foreach(Match match in matches) {
                var name = match.Groups[1].Value;
                if(gl4Files.Contains(name)) continue;
                Console.WriteLine("DocHandler - GL2 - Parsing: {0}", name);

                var xmlContents = string.Empty;
                if(!File.Exists("cache/" + name + ".xml")) {
                    try {
                        var xmlRequest = WebRequest.Create("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl2.1/" + name + ".xml");

                        using (var response = xmlRequest.GetResponse())
                        using (var content = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(content))
                            {
                                xmlContents = reader.ReadToEnd();
                                File.WriteAllText("cache/" + name + ".xml", xmlContents);
                            }
                        }
                    } catch {
                        System.Console.WriteLine("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl4/" + name + ".xml");
                    }
                } else {
                    xmlContents = File.ReadAllText("cache/" + name + ".xml");
                }

                var functions = functionsRegex.Matches(xmlContents);
                foreach(Match function in functions) {
                    var nameMatch = functionNameRegex.Match(function.Groups[1].Value);
                    var functionName = nameMatch.Groups[1].Value;
                    var paramsMatches = functionParamRegex.Matches(function.Groups[1].Value);
                    var f = new FunctionPrototype();
                    f.Name = functionName;
                    foreach(Match param in paramsMatches) {
                        f.Params.Add(param.Groups[1].Value);
                    }
                    Prototypes[functionName] = f;

                    if(!map.ContainsKey(functionName)) {
                        map[functionName] = name;
                    }
                }
            }
        }
    }
}