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
    // Copied from https://github.com/giawa/opengl4csharp/blob/master/BindingsGen/CreateDocumentation/Program.cs
    public class DocHandler
    {
        public Dictionary<string, string> map = new Dictionary<string, string>();

        public void DownloadGL4() {
            if(!Directory.Exists("cache")) Directory.CreateDirectory("cache");

            var regex = new Regex(
                "<li><a href=\"(.*?).xhtml\" target=\"pagedisplay\">(.*?)</a></li>",
                RegexOptions.Multiline | RegexOptions.Compiled
            );
            var functionRegex = new Regex("<code class=\"funcdef\">.*?<strong class=\"fsfunc\">(.*?)</strong>", RegexOptions.Multiline | RegexOptions.Compiled);

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

            var matches = regex.Matches(contents);
            foreach(Match match in matches) {
                var name = match.Groups[1].Value;
                var xmlContents = string.Empty;
                if(!File.Exists("cache/" + name + ".xhtml")) {
                    try {
                        var xmlRequest = WebRequest.Create("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl4/html/" + name + ".xhtml");

                        using (var response = xmlRequest.GetResponse())
                        using (var content = response.GetResponseStream())
                        {
                            using (var reader = new StreamReader(content))
                            {
                                xmlContents = reader.ReadToEnd();
                                File.WriteAllText("cache/" + name + ".xhtml", xmlContents);
                            }
                        }
                    } catch {
                        System.Console.WriteLine("https://raw.githubusercontent.com/KhronosGroup/OpenGL-Refpages/master/gl4/html/" + name + ".xhtml");
                    }
                } else {
                    xmlContents = File.ReadAllText("cache/" + name + ".xhtml");
                }
                var functions = functionRegex.Matches(xmlContents);
                foreach(Match function in functions) {
                    if(!map.ContainsKey(function.Groups[1].Value)) {
                        map[function.Groups[1].Value] = name;
                    }
                }
            }
        }
        
        public void WriteMainDoc(IndentedStringBuilder sb, string name) {
            if(map.ContainsKey(name)) {
                if(File.Exists("cache/" + map[name] + ".xhtml")) {
                    var contents = File.ReadAllText("cache/" + map[name] + ".xhtml");

                    string summary = contents.Substring(contents.IndexOf("<p>") + 3);
                    summary = summary.Substring(0, summary.IndexOf("</p>"));
                    //string names = summary.Substring(0, summary.IndexOf((char)226)).Trim();
                    summary = summary.Substring(summary.IndexOf((char)8221) + 1);
                    summary = summary.Replace('\n', ' ').Trim(new char[] { ' ', '\t', '.' });
                    while (summary.Contains("  ")) summary = summary.Replace("  ", " ");
                    var Summary = string.Format("{0}{1}.", char.ToUpper(summary[0]), summary.Substring(1));
                    Summary = StripHTML(Summary);

                    string description = contents.Substring(contents.IndexOf("<h2>Description</h2>") + 20);
                    description = description.Substring(0, description.IndexOf("</p>"));
                    description = StripHTML(description);
                    description = description.Replace('\n', ' ').Trim(new char[] { ' ', '\t', '.' });
                    while (description.Contains("  ")) description = description.Replace("  ", " ");

                    // special case for math formatting from the man pages
                    if (description.Contains("clamped to the range 0 "))
                        description = description.Replace("clamped to the range 0 ", "clamped to the range [0, ").Replace("1", "1]");

                    var Description = string.Format("{0}{1}.", char.ToUpper(description[0]), description.Substring(1));

                    sb.AppendLine("/// <summary>");
                    sb.AppendLine("/// " + Summary);
                    sb.AppendLine("/// <para>");
                    string text = Description;
                    if (text.StartsWith("Gl")) text = text.Replace("Gl", "gl");
                    WriteMultiLine(sb, text);
                    sb.AppendLine("/// </para>");
                    sb.AppendLine("/// </summary>");
                }
            }
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
                if(File.Exists("cache/" + map[function] + ".xhtml")) {
                    var contents = File.ReadAllText("cache/" + map[function] + ".xhtml");

                    if (contents.IndexOf("id=\"parameters\"") > 0)
                    {
                        contents = contents.Substring(contents.IndexOf("id=\"parameters\""));

                        // check if multiple methods are in here - if so, move to the correct method
                        if (contents.Contains("parameters2"))
                        {
                            if (contents.Contains(map[function] + "</code></h2>"))
                            {
                                contents = contents.Substring(contents.IndexOf(map[function] + "</code></h2>"));
                            }
                            else
                            {
                                contents = contents.Substring(contents.IndexOf(map[function]));
                            }
                        }

                        contents = contents.Substring(0, contents.IndexOf("div class=\"refsect1\""));

                        var found = false;
                        while (contents.Contains("<span class=\"term\">"))
                        {
                            contents = contents.Substring(contents.IndexOf("<em class=\"parameter\"") + 17);

                            //List<string> parameterNames = new List<string>();

                            while (contents.IndexOf("<code>") > 0 && contents.IndexOf("<code>") < contents.IndexOf("<p>"))
                            {
                                string parameterName = contents.Substring(contents.IndexOf("<code>") + 6);
                                parameterName = parameterName.Substring(0, parameterName.IndexOf("</code>")).Trim(new char[] { '\r', '\n', '\t', ' ' });

                                //parameterNames.Add(parameterName);
                                if(parameterName == parameterOrig) found = true;

                                contents = contents.Substring(contents.IndexOf("</code>") + 7);
                            }
                            if(found) {
                                string parameterText = contents.Substring(contents.IndexOf("<p>") + 3);
                                parameterText = parameterText.Substring(0, parameterText.IndexOf("</dd>"));
                                parameterText = parameterText.Replace('\n', ' ').Trim(new char[] { '\r', '\n', '\t', ' ' });
                                parameterText = StripHTML(parameterText);
                                while (parameterText.Contains("  ")) parameterText = parameterText.Replace("  ", " ");

                                // special case for math formatting from the man pages
                                if (parameterText.Contains("clamped to the range 0 "))
                                    parameterText = parameterText.Replace("clamped to the range 0 ", "clamped to the range [0, ").Replace("1 .", "1].").Replace("2 n - 1 ,", "2^n - 1],");
                                sb.AppendLine("/// <param name=\"{0}\">", parameter);
                                WriteMultiLine(sb, parameterText);
                                sb.AppendLine("/// </param>");
                                return;
                            }
                        }
                    }
                }
            }
        }

        private static string StripHTML(string data)
        {
            var sb = new StringBuilder();

            char[] chars = data.ToCharArray();
            for (int i = 0; i < data.Length; i++)
            {
                if (chars[i] == '<')
                {
                    while (chars[i] != '>') i++;
                    continue;
                }

                sb.Append(chars[i]);
            }

            return sb.ToString();
        }
    }
}