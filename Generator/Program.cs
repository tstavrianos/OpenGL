using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;

namespace Generator
{
    class Program
    {
        private static string Namespace = "OpenGL";
        private static string FilenamePrefix = "OpenGL.";
        private static string path = "../../../../Bindings/";

        private static void BuildClass(IndentedStringBuilder sb, string name, Action<IndentedStringBuilder> acn) {
            sb.AppendLine("public static class {0} {{", name);
            sb.Indent();

            acn(sb);

            sb.Outdent();
            sb.AppendLine("}");
        }

        static void Main(string[] args)
        {
            var docHandler = new DocHandler();
            docHandler.DownloadGL4();
            //docHandler.DownloadGL2();

            var parser = new XMLParser();
            var def = parser.Parse(@"https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/master/xml/gl.xml");
            var consts = new IndentedStringBuilder();
            consts.AppendLine("namespace {0} {{", Namespace);
            consts.Indent();
            BuildClass(consts, "Constants", (sb) => {
                foreach(var e1 in def.Enums) {
                    foreach(var e2 in e1.Enum) {
                        e2.ConstantDeclaration(sb);
                    }
                }
            });
            consts.Outdent();
            consts.AppendLine("}");

            var enums = new IndentedStringBuilder();
            enums.AppendLine("namespace {0} {{", Namespace);
            enums.Indent();
            BuildClass(enums, "Enums", (sb) => {
                var enumsStart = true;
                foreach(var e1 in def.Enums) {
                    if(!string.IsNullOrEmpty(e1.Group) && e1.Group != "SpecialNumbers") {
                        if(enumsStart == true) {
                            enumsStart = false;
                        } else {
                            enums.AppendLine("");
                        }
                        e1.StronglyTypedDeclaration(sb);
                    }
                }
            });
            enums.Outdent();
            enums.AppendLine("}");

            var features = new IndentedStringBuilder();
            features.AppendLine("using System.Collections.Generic;");
            features.AppendLine("");
            features.AppendLine("namespace {0} {{", Namespace);
            features.Indent();
            BuildClass(features, "Features", (sb) => {
                sb.AppendLine("internal static HashSet<string> ExtensionsGPU = new HashSet<string>();");
                sb.AppendLine("");
                sb.AppendLine("public static bool IsExtensionSupported(string ExtensionName) => ExtensionsGPU.Contains(ExtensionName);");
                sb.AppendLine("");
                foreach(var feature in def.Feature) {
                    feature.BuildDeclaration(sb);
                }
            });
            features.Outdent();
            features.AppendLine("}");

            var loader = new IndentedStringBuilder();
            loader.AppendLine("using System;");
            loader.AppendLine("using System.Runtime.InteropServices;");
            loader.AppendLine("using System.Collections.Generic;");
            loader.AppendLine("");
            loader.AppendLine("namespace {0} {{", Namespace);
            loader.Indent();
            loader.AppendLine("public static class Loader {");
            loader.Indent();
            loader.AppendLine("public static void Init(Func<string, IntPtr> loadProc, bool disableRemovedProcs = false, bool loadExtensions = true, bool loadExtensionProcs = true, bool loadLeftovers = false) {");
            loader.Indent();
            loader.AppendLine("var proc = IntPtr.Zero;");
            OpenGLSpec.Command.BuildLoader(loader, "glGetIntegerv");
            loader.AppendLine("");
            loader.AppendLine("var int1 = new int[1];");
            loader.AppendLine("var versionMajor = 0;");
            loader.AppendLine("var versionMinor = 0;");
            loader.AppendLine("if(Pointers.glGetIntegerv != null) {");
            loader.Indent();
            loader.AppendLine("Wrappers.glGetIntegerv(Constants.GL_MAJOR_VERSION, int1);");
            loader.AppendLine("versionMajor = int1[0];");
            loader.AppendLine("Wrappers.glGetIntegerv(Constants.GL_MINOR_VERSION, int1);");
            loader.AppendLine("versionMinor = int1[0];");
            loader.Outdent();
            loader.AppendLine("}");
            loader.AppendLine("");
            loader.AppendLine("var version = (byte)(versionMajor * 10 + versionMinor);");
            foreach(var feat in def.Feature.Where(x => x.Api == "gl")) {
                feat.BuildAssign(loader);
            }
            loader.AppendLine("");

            var delegates = new IndentedStringBuilder();
            delegates.AppendLine("using System;");
            delegates.AppendLine("using System.Text;");
            delegates.AppendLine("");
            delegates.AppendLine("namespace {0} {{", Namespace);
            delegates.Indent();
            delegates.AppendLine("internal unsafe static class Delegates {");
            delegates.Indent();
            var wrappers = new IndentedStringBuilder();
            wrappers.AppendLine("using System;");
            wrappers.AppendLine("using System.Text;");
            wrappers.AppendLine("using System.Runtime.InteropServices;");
            wrappers.AppendLine("using System.Collections.Generic;");
            wrappers.AppendLine("");
            wrappers.AppendLine("namespace {0} {{", Namespace);
            wrappers.Indent();
            wrappers.AppendLine("public unsafe static partial class Wrappers {");
            wrappers.Indent();
            wrappers.AppendLine("private static string PtrToStringUTF8(IntPtr ptr) {");
            wrappers.Indent();
            wrappers.AppendLine("if (ptr == IntPtr.Zero) return null;");
            wrappers.AppendLine("var buff = new List<byte>();");
            wrappers.AppendLine("var offset = 0;");
            wrappers.AppendLine("for (; ; offset++) {");
            wrappers.Indent();
            wrappers.AppendLine("var currentByte = Marshal.ReadByte(ptr, offset);");
            wrappers.AppendLine("if (currentByte == 0) break;");
            wrappers.AppendLine("buff.Add(currentByte);");
            wrappers.Outdent();
            wrappers.AppendLine("}");
            wrappers.AppendLine("return Encoding.UTF8.GetString(buff.ToArray());");
            wrappers.Outdent();
            wrappers.AppendLine("}");
            wrappers.AppendLine("");

            var pointers = new IndentedStringBuilder();
            pointers.AppendLine("using System;");
            pointers.AppendLine("");
            pointers.AppendLine("namespace {0} {{", Namespace);
            pointers.Indent();
            pointers.AppendLine("internal static class Pointers {");
            pointers.Indent();

            var hashlist = new HashSet<string>();
            hashlist.Add("glGetIntegerv");
            def.Commands.Build(delegates, pointers, wrappers, def, hashlist, docHandler);

            foreach(var feature in def.Feature.Where(x => x.Api == "gl")) {
                feature.BuildLoader(loader, hashlist);
            }

            loader.AppendLine("");
            loader.AppendLine("if(loadExtensions) {");
            loader.Indent();
            loader.AppendLine("if(Pointers.glGetString != null) {");
            loader.Indent();
            loader.AppendLine("var extStd = Wrappers.glGetString(Constants.GL_EXTENSIONS).Trim();");
            loader.AppendLine("foreach (string extension in extStd.Split(' ')) {");
            loader.Indent();
            loader.AppendLine("if (!Features.ExtensionsGPU.Contains(extension) && !String.IsNullOrEmpty(extension)) {");
            loader.Indent();
            loader.AppendLine("Features.ExtensionsGPU.Add(extension);");
            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");
            loader.AppendLine("");
            loader.AppendLine("if(versionMajor >= 3) {");
            loader.Indent();
            loader.AppendLine("Wrappers.glGetIntegerv(Constants.GL_NUM_EXTENSIONS, int1);");
            loader.AppendLine("var exts = int1[0];");
            loader.AppendLine("for(uint cnt = 0; cnt < exts; cnt++) {");
            loader.Indent();
            loader.AppendLine("var extIdx = Wrappers.glGetStringi(Constants.GL_EXTENSIONS, cnt).Trim();");
            loader.AppendLine("if (!Features.ExtensionsGPU.Contains(extIdx) && !String.IsNullOrEmpty(extIdx)) {");
            loader.Indent();
            loader.AppendLine("Features.ExtensionsGPU.Add(extIdx);");
            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");
            loader.AppendLine("");

            loader.AppendLine("if(loadExtensions && loadExtensionProcs) {");
            loader.Indent();
            def.Extensions.BuildLoader(loader, "gl", hashlist);
            loader.Outdent();
            loader.AppendLine("}");

            if(hashlist.Count > 0) {
                loader.AppendLine("");
                loader.AppendLine("if(loadLeftovers) {");
                loader.Indent();
                foreach(var name in hashlist) {
                    OpenGLSpec.Command.BuildLoader(loader, name);
                }
                loader.Outdent();
                loader.AppendLine("}");
            }

            delegates.Outdent();
            delegates.AppendLine("}");
            delegates.Outdent();
            delegates.AppendLine("}");
            
            pointers.Outdent();
            pointers.AppendLine("}");
            pointers.Outdent();
            pointers.AppendLine("}");

            wrappers.Outdent();
            wrappers.AppendLine("}");
            wrappers.Outdent();
            wrappers.AppendLine("}");

            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");
            loader.Outdent();
            loader.AppendLine("}");

            Senzible.WriteStringBuilderToFile(consts, path, FilenamePrefix + "Constants.cs");
            Senzible.WriteStringBuilderToFile(enums, path, FilenamePrefix + "Enums.cs");
            Senzible.WriteStringBuilderToFile(features, path, FilenamePrefix + "Features.cs");
            Senzible.WriteStringBuilderToFile(delegates, path, FilenamePrefix + "Delegates.cs");
            Senzible.WriteStringBuilderToFile(pointers, path, FilenamePrefix + "Pointers.cs");
            Senzible.WriteStringBuilderToFile(wrappers, path, FilenamePrefix + "Wrappers.cs");
            Senzible.WriteStringBuilderToFile(loader, path, FilenamePrefix + "Loader.cs");
        }
    }
}