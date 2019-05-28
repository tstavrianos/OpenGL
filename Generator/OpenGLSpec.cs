/* 
 Built using XML2Sharp
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using Generator;
using System.Linq;

namespace OpenGLSpec
{
    [XmlRoot(ElementName = "type")]
    public class Type
    {
        [XmlAttribute(AttributeName = "name")]
        public string _Name { get; set; }
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlAttribute(AttributeName = "requires")]
        public string Requires { get; set; }
        [XmlElement(ElementName = "apientry")]
        public string Apientry { get; set; }
        [XmlAttribute(AttributeName = "api")]
        public string Api { get; set; }
    }

    [XmlRoot(ElementName = "types")]
    public class Types
    {
        [XmlElement(ElementName = "type")]
        public List<Type> Type { get; set; }
    }

    [XmlRoot(ElementName = "enum")]
    public class Enum
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlAttribute(AttributeName = "alias")]
        public string Alias { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "api")]
        public string Api { get; set; }

        public void ConstantDeclaration(IndentedStringBuilder sb)
        {
            if (Name == "GL_ACTIVE_PROGRAM_EXT" && Api == "gles2") return;
            sb.AppendLine(luca_piccioni.ConstantDeclaration(Name, Value));
        }
    }

    [XmlRoot(ElementName = "group")]
    public class Group
    {
        [XmlElement(ElementName = "enum")]
        public List<Enum> Enum { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
    }

    [XmlRoot(ElementName = "groups")]
    public class Groups
    {
        [XmlElement(ElementName = "group")]
        public List<Group> Group { get; set; }
    }

    [XmlRoot(ElementName = "enums")]
    public class Enums
    {
        [XmlElement(ElementName = "enum")]
        public List<Enum> Enum { get; set; }
        [XmlAttribute(AttributeName = "namespace")]
        public string Namespace { get; set; }
        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "unused")]
        public List<Unused> Unused { get; set; }
        [XmlAttribute(AttributeName = "vendor")]
        public string Vendor { get; set; }
        [XmlAttribute(AttributeName = "start")]
        public string Start { get; set; }
        [XmlAttribute(AttributeName = "end")]
        public string End { get; set; }


        private static string FixName(string value)
        {
            var output = string.Empty;
            if (value.StartsWith("GL_")) value = value.Substring(3);
            else if (value.StartsWith("WGL_")) value = value.Substring(4);

            for (var i = 0; i < value.Length; i++)
            {
                if (i == 0) output += char.ToUpper(value[i]);
                else if (value[i] == '_') continue;
                else if (value[i - 1] == '_') output += char.ToUpper(value[i]);
                else output += char.ToLower(value[i]);
            }
            if (output[0] >= '0' && output[0] <= '9') output = '_' + output;
            return output;
        }
        public void StronglyTypedDeclaration(IndentedStringBuilder sb)
        {
            if (!string.IsNullOrEmpty(Comment)) sb.AppendLine("// {0}", Comment);
            if (!string.IsNullOrEmpty(Group))
            {
                sb.AppendLine("public enum {0}{1} {{", Group, !string.IsNullOrEmpty(Type) && Type == "bitmask" ? ": uint" : "");
            }
            else if (!string.IsNullOrEmpty(Namespace) && Namespace != "GL" && Namespace != "WGL" && Namespace != "EGL")
            {
                sb.AppendLine("public enum {0}{1} {{", Namespace, !string.IsNullOrEmpty(Type) && Type == "bitmask" ? ": uint" : "");
            }
            sb.Indent();
            var enumHash = new HashSet<string>();
            foreach (var e1 in Enum)
            {
                var name = FixName(e1.Name);
                if (enumHash.Contains(name)) continue;
                enumHash.Add(name);
                if (!string.IsNullOrEmpty(e1.Comment)) sb.AppendLine("// {0}", e1.Comment);
                sb.AppendLine("{0} = {1},", name, e1.Value);
            }
            sb.Outdent();
            sb.AppendLine("}");
        }
    }

    [XmlRoot(ElementName = "unused")]
    public class Unused
    {
        [XmlAttribute(AttributeName = "start")]
        public string Start { get; set; }
        [XmlAttribute(AttributeName = "end")]
        public string End { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlAttribute(AttributeName = "vendor")]
        public string Vendor { get; set; }
    }

    [XmlRoot(ElementName = "proto")]
    public class Proto
    {
        [XmlText()]
        public string ReturnTypeText;

        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "ptype")]
        public string Ptype { get; set; }
        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }
        public string ReturnType;
        public bool MarshalString;
    }

    [XmlRoot(ElementName = "param")]
    public class Param
    {
        [XmlText()]
        public List<string> TypeDecorators = new List<string>();

        [XmlElement(ElementName = "ptype")]
        public string Ptype { get; set; }
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }
        [XmlAttribute(AttributeName = "len")]
        public string Len { get; set; }
        public string Translated;
        public bool NeedsFixed;
        public bool NeedsOut;
        public string EnumTranslated;
        public bool HasEnum;
        public string OrigName;

        public string Declaration(bool std = true)
        {
            if (std)
            {
                return Translated + " " + Name;
            }
            else if (NeedsFixed && !NeedsOut)
            {
                return Translated.Substring(0, Translated.Length - 1) + "[] " + Name;
            }
            else if (NeedsOut)
            {
                return "out " + Translated.Substring(0, Translated.Length - 1) + " " + Name;
            }
            return string.Empty;
        }

        public string EnumDeclaration(bool std = true)
        {
            if (std)
            {
                return EnumTranslated + " " + Name;
            }
            else if (NeedsFixed && !NeedsOut)
            {
                return EnumTranslated.Substring(0, EnumTranslated.Length - 1) + "[] " + Name;
            }
            else if (NeedsOut)
            {
                return "out " + EnumTranslated.Substring(0, EnumTranslated.Length - 1) + " " + Name;
            }
            return string.Empty;
        }
    }

    [XmlRoot(ElementName = "glx")]
    public class Glx
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "opcode")]
        public string Opcode { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
    }

    [XmlRoot(ElementName = "command")]
    public class Command
    {
        [XmlElement(ElementName = "proto")]
        public Proto Proto { get; set; }
        [XmlElement(ElementName = "param")]
        public List<Param> Param { get; set; }
        [XmlElement(ElementName = "glx")]
        public List<Glx> Glx { get; set; }
        [XmlElement(ElementName = "alias")]
        public Alias Alias { get; set; }
        [XmlElement(ElementName = "vecequiv")]
        public Vecequiv Vecequiv { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        public void FixParams(Registry reg)
        {
            if (Param != null)
            {
                foreach (var param in Param)
                {
                    param.OrigName = param.Name;
                    param.Name = Senzible.CleanUpName(param.Name);
                    param.Translated = Senzible.GetReturnType(param);
                    param.NeedsFixed = param.Translated.EndsWith("*");
                    param.HasEnum = !string.IsNullOrEmpty(param.Group) && reg.Enums.Any(x => x.Group == param.Group);
                    param.EnumTranslated = !param.HasEnum ? param.Translated : ("Enums." + param.Group + (param.NeedsFixed ? "*" : ""));
                    param.NeedsOut =
                        param.NeedsFixed && !string.IsNullOrEmpty(param.Len) && param.Len == "1";
                }
            }
        }

        public void BuildDelegate(IndentedStringBuilder sb)
        {
            sb.AppendLine("internal delegate {0} {1}({2});",
                Proto.ReturnType,
                Proto.Name,
                Param != null ? string.Join(", ", Param.Select(x => x.Translated + " " + x.Name)) : "");

        }

        public void BuildPointer(IndentedStringBuilder sb)
        {
            sb.AppendLine("internal static Delegates.{0} {0} = null;", Proto.Name);
        }

        private struct FixedParam
        {
            public string OrigName;
            public string Name;
            public string Type;
            public bool IsFixed;
            public bool IsOut;
            public bool IsEnum;
            public string EnumType;

            public string DeclareType;
            public string FixedType;
            public string Param;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == null)
            {
                return null;
            }

            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) => accumulator.SelectMany(
                    accseq => sequence,
                    (accseq, item) => accseq.Concat(new[] { item })));
        }

        private string ParamAlias(int i, DocHandler doc)
        {
            var name = Proto.Name;
            if (!doc.Prototypes.ContainsKey(name))
            {
                if (Alias != null && !string.IsNullOrEmpty(Alias.Name))
                {
                    if (doc.Prototypes.ContainsKey(Alias.Name))
                    {
                        name = Alias.Name;
                    }
                }
            }
            if (!doc.Prototypes.ContainsKey(name)) return Param[i].OrigName;

            if (doc.Prototypes[name].Params.Count <= i) return Param[i].OrigName;
            return doc.Prototypes[name].Params[i];
        }

        private IEnumerable<IEnumerable<FixedParam>> GetParamVariations()
        {
            var res = new List<List<FixedParam>>();
            foreach (var param in Param)
            {
                var pList = new List<FixedParam>();
                pList.Add(new FixedParam()
                {
                    OrigName = param.OrigName,
                    Name = param.Name,
                    Type = param.Translated,
                    IsFixed = false,
                    IsOut = false,
                    IsEnum = false,
                    EnumType = null,
                    DeclareType = param.Translated,
                    FixedType = param.Translated,
                    Param = param.Name
                });
                if (param.NeedsFixed && !param.NeedsOut && !param.HasEnum)
                {
                    pList.Add(new FixedParam()
                    {
                        OrigName = param.OrigName,
                        Name = param.Name,
                        Type = param.Translated,
                        IsFixed = true,
                        IsOut = false,
                        IsEnum = false,
                        EnumType = null,
                        DeclareType = param.Translated.Substring(0, param.Translated.Length - 1) + "[]",
                        FixedType = param.Translated,
                        Param = param.Name + "_"
                    });
                }
                else if (param.NeedsOut && !param.HasEnum)
                {
                    pList.Add(new FixedParam()
                    {
                        OrigName = param.OrigName,
                        Name = param.Name,
                        Type = param.Translated,
                        IsFixed = false,
                        IsOut = true,
                        IsEnum = false,
                        EnumType = null,
                        DeclareType = "out " + param.Translated.Substring(0, param.Translated.Length - 1),
                        FixedType = param.Translated,
                        Param = param.Name + "_"
                    });
                }
                else if (!param.NeedsFixed && param.HasEnum)
                {
                    pList.Add(new FixedParam()
                    {
                        OrigName = param.OrigName,
                        Name = param.Name,
                        Type = param.Translated,
                        IsFixed = false,
                        IsOut = false,
                        IsEnum = true,
                        EnumType = param.EnumTranslated,
                        DeclareType = param.EnumTranslated,
                        Param = "(" + param.Translated + ")" + param.Name
                    });
                }
                else if (param.NeedsFixed && !param.NeedsOut && param.HasEnum)
                {
                    pList.Add(new FixedParam()
                    {
                        OrigName = param.OrigName,
                        Name = param.Name,
                        Type = param.Translated,
                        IsFixed = true,
                        IsOut = false,
                        IsEnum = true,
                        EnumType = param.EnumTranslated,
                        DeclareType = param.EnumTranslated.Substring(0, param.EnumTranslated.Length - 1) + "[]",
                        FixedType = param.EnumTranslated,
                        Param = "(" + param.Translated + ")" + param.Name
                    });
                }
                else if (param.NeedsOut && param.HasEnum)
                {
                    pList.Add(new FixedParam()
                    {
                        OrigName = param.OrigName,
                        Name = param.Name,
                        Type = param.Translated,
                        IsFixed = false,
                        IsOut = true,
                        IsEnum = true,
                        EnumType = param.EnumTranslated,
                        DeclareType = "out " + param.EnumTranslated.Substring(0, param.EnumTranslated.Length - 1),
                        FixedType = param.EnumTranslated,
                        Param = "(" + param.Translated + ")" + param.Name + "_"
                    });
                }
                res.Add(pList);
            }
            return CartesianProduct(res);
        }

        public string Filename(DocHandler doc, Registry reg)
        {
            if (!doc.map.ContainsKey(Proto.Name))
            {
                if (Alias != null && !string.IsNullOrEmpty(Alias.Name))
                {
                    if (doc.map.ContainsKey(Alias.Name))
                    {
                        return doc.map[Alias.Name];
                    }
                    else
                    {
                        return reg.Commands.Command.First(x => x.Proto.Name == Alias.Name).Filename(doc, reg);
                    }
                }
                return string.Empty;
            }
            else
            {
                return doc.map[Proto.Name];
            }
        }

        public void BuildWrappers(IndentedStringBuilder sb, DocHandler doc, Registry reg)
        {
            var p1 = GetParamVariations();
            var first = true;
            foreach (var p2 in p1)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine("");
                }

                var filename = Filename(doc, reg);
                if (filename != string.Empty)
                {
                    doc.map[Proto.Name] = filename;
                    doc.WriteMainDoc(sb, Proto.Name);
                    var i = 0;
                    foreach (var param in p2)
                    {
                        doc.WriteParameter(sb, Proto.Name, param.Name, ParamAlias(i, doc));
                        i++;
                    }
                }

                sb.AppendLine("public static {0} {1}({2}) {{",
                    Proto.MarshalString ? "string" : Proto.ReturnType,
                    Proto.Name,
                    string.Join(", ", p2.Select(x => x.DeclareType + " " + x.Name))
                );
                sb.Indent();
                var fxd = false;
                foreach (var param in p2)
                {
                    if (param.IsFixed)
                    {
                        fxd = true;
                        sb.AppendLine("fixed({0} {1}_ = &{1}[0])", param.FixedType, param.Name);
                    }
                    else if (param.IsOut)
                    {
                        fxd = true;
                        sb.AppendLine("fixed({0} {1}_ = &{1})", param.FixedType, param.Name);
                    }
                }
                if (fxd) sb.Indent();
                sb.AppendLine("{0}{2}Pointers.{1}({3}){4};",
                    Proto.ReturnType == "void" ? "" : "return ",
                    Proto.Name,
                    Proto.MarshalString ? "PtrToStringUTF8(" : "",
                    string.Join(", ", p2.Select(x => x.Param)),
                    Proto.MarshalString ? ")" : ""
                );
                if (fxd) sb.Outdent();
                sb.Outdent();
                sb.AppendLine("}");
            }

            if (
                (Proto.Name.StartsWith("glGen") || Proto.Name.StartsWith("glCreate")) &&
                Proto.ReturnType == "void" &&
                Param.Count == 2 &&
                Param.Any(x => x.NeedsFixed && !string.IsNullOrEmpty(x.Len) && Param.Any(y => y.Name == x.Len && y.Name != x.Name))
            )
            {
                var param = Param.First(x => !string.IsNullOrEmpty(x.Len));
                sb.AppendLine("");
                sb.AppendLine("public static {0} {1}() {{",
                    param.Translated.Substring(0, param.Translated.Length - 1),
                    Proto.Name.EndsWith("s") ? Proto.Name.Substring(0, Proto.Name.Length - 1) : Proto.Name
                );
                sb.Indent();
                sb.AppendLine("var {0}_ = new {1}[1];", param.Name, param.Translated.Substring(0, param.Translated.Length - 1));
                sb.AppendLine("{0}(1, {1}_);", Proto.Name, param.Name);
                sb.AppendLine("return {0}_[0];", param.Name);
                sb.Outdent();
                sb.AppendLine("}");
            }

            if (new[] { "glGetIntegerv", "glGetFloatv", "glGetDoublev", "glGetBooleanv" }.Contains(Proto.Name))
            {
                sb.AppendLine("");
                var param = Param.First(x => x.NeedsFixed);
                sb.AppendLine("public static {0} {1}({2}) {{",
                    param.Translated.Substring(0, param.Translated.Length - 1),
                    Proto.Name,
                    string.Join(", ", Param.Where(x => x != param).Select(x => x.Translated + " " + x.Name))
                );
                sb.Indent();
                sb.AppendLine("{0} {1};", param.Translated.Substring(0, param.Translated.Length - 1), param.Name);
                sb.AppendLine("{0}({1});",
                    Proto.Name,
                    string.Join(", ", Param.Select(x => (x == param ? "&" : "") + x.Name))
                );
                sb.AppendLine("return {0};", param.Name);
                sb.Outdent();
                sb.AppendLine("}");
            }
        }

        public static void BuildLoader(IndentedStringBuilder sb, string name)
        {
            sb.AppendLine("if(Pointers.{0} == null) {{", name);
            sb.Indent();
            sb.AppendLine("if((proc = loadProc(\"{0}\")) != IntPtr.Zero) {{", name);
            sb.Indent();
            sb.AppendLine("Pointers.{0} = (Delegates.{0})Marshal.GetDelegateForFunctionPointer(proc, typeof(Delegates.{0}));", name);
            sb.Outdent();
            sb.AppendLine("}");
            sb.Outdent();
            sb.AppendLine("}");
        }

        public void BuildLoader(IndentedStringBuilder sb)
        {
            BuildLoader(sb, Name);
        }
    }

    [XmlRoot(ElementName = "alias")]
    public class Alias
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "vecequiv")]
    public class Vecequiv
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "commands")]
    public class Commands
    {
        [XmlElement(ElementName = "command")]
        public List<Command> Command { get; set; }
        [XmlAttribute(AttributeName = "namespace")]
        public string Namespace { get; set; }

        public void Build(IndentedStringBuilder delegates, IndentedStringBuilder pointers, IndentedStringBuilder wrappers, Registry reg, HashSet<string> commands, DocHandler doc)
        {
            var wrapperStart = true;
            foreach (var c in Command)
            {
                c.Proto.ReturnType = Senzible.GetReturnType(c.Proto);
                c.FixParams(reg);
                c.BuildDelegate(delegates);
                c.BuildPointer(pointers);
                if (!wrapperStart)
                {
                    wrappers.AppendLine("");
                }
                else
                {
                    wrapperStart = false;
                }
                c.BuildWrappers(wrappers, doc, reg);
                commands.Add(c.Proto.Name);
            }
        }
    }

    [XmlRoot(ElementName = "require")]
    public class Require
    {
        [XmlElement(ElementName = "type")]
        public List<Type> Type { get; set; }
        [XmlElement(ElementName = "enum")]
        public List<Enum> Enum { get; set; }
        [XmlElement(ElementName = "command")]
        public List<Command> Command { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlAttribute(AttributeName = "profile")]
        public string Profile { get; set; }
        [XmlAttribute(AttributeName = "api")]
        public string Api { get; set; }

        public void BuildLoader(IndentedStringBuilder sb, HashSet<string> commands)
        {
            if (Command == null || Command.Count == 0) return;
            foreach (var cmd in Command)
            {
                cmd.BuildLoader(sb);
                commands.Remove(cmd.Name);
            }
        }
    }

    [XmlRoot(ElementName = "feature")]
    public class Feature
    {
        [XmlElement(ElementName = "require")]
        public List<Require> Require { get; set; }
        [XmlAttribute(AttributeName = "api")]
        public string Api { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public string Number { get; set; }
        [XmlElement(ElementName = "remove")]
        public List<Remove> Remove { get; set; }

        public void BuildDeclaration(IndentedStringBuilder sb)
        {
            sb.AppendLine("public static bool {0} {{get; internal set;}} = false;", Name);
        }

        public void BuildAssign(IndentedStringBuilder sb)
        {
            sb.AppendLine("if(version >= {0}) Features.{1} = true;", Number.Replace(".", ""), Name);
        }

        public void BuildLoader(IndentedStringBuilder sb, HashSet<string> commands)
        {
            if (Require == null || !Require.Any(x => x.Command != null && x.Command.Count > 0)) return;
            sb.AppendLine("if(Features.{0}) {{", Name);
            sb.Indent();
            foreach (var require in Require)
            {
                require.BuildLoader(sb, commands);
            }
            if (Remove != null && Remove.Any(x => x.Command != null && x.Command.Count > 0))
            {
                sb.AppendLine("if(disableRemovedProcs) {");
                sb.Indent();
                foreach (var remove in Remove.Where(x => x.Command != null && x.Command.Count > 0))
                {
                    remove.BuildLoader(sb);
                }
                sb.Outdent();
                sb.AppendLine("}");
            }
            sb.Outdent();
            sb.AppendLine("}");
        }
    }

    [XmlRoot(ElementName = "remove")]
    public class Remove
    {
        [XmlElement(ElementName = "command")]
        public List<Command> Command { get; set; }
        [XmlAttribute(AttributeName = "profile")]
        public string Profile { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "enum")]
        public List<Enum> Enum { get; set; }

        public void BuildLoader(IndentedStringBuilder sb)
        {
            if (Command == null || Command.Count == 0) return;
            foreach (var cmd in Command)
            {
                sb.AppendLine("Pointers.{0} = null;", cmd.Name);
            }
        }
    }

    [XmlRoot(ElementName = "extension")]
    public class Extension
    {
        [XmlElement(ElementName = "require")]
        public List<Require> Require { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "supported")]
        public string Supported { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string Comment { get; set; }

        public void BuildLoader(IndentedStringBuilder sb, HashSet<string> commands)
        {
            if (Require == null) return;
            sb.AppendLine("if(Features.ExtensionsGPU.Contains(\"{0}\")) {{", Name);
            sb.Indent();
            foreach (var req in Require)
            {
                req.BuildLoader(sb, commands);
            }
            sb.Outdent();
            sb.AppendLine("}");
        }
    }

    [XmlRoot(ElementName = "extensions")]
    public class Extensions
    {
        [XmlElement(ElementName = "extension")]
        public List<Extension> Extension { get; set; }

        public void BuildLoader(IndentedStringBuilder sb, string api, HashSet<string> commands)
        {
            foreach (var ext in Extension.Where(x => x.Supported == api && x.Require != null && x.Require.Any(y => y.Command != null && y.Command.Count > 0)))
            {
                ext.BuildLoader(sb, commands);
            }
        }
    }

    [XmlRoot(ElementName = "registry")]
    public class Registry
    {
        [XmlElement(ElementName = "comment")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "types")]
        public Types Types { get; set; }
        [XmlElement(ElementName = "groups")]
        public Groups Groups { get; set; }
        [XmlElement(ElementName = "enums")]
        public List<Enums> Enums { get; set; }
        [XmlElement(ElementName = "commands")]
        public Commands Commands { get; set; }
        [XmlElement(ElementName = "feature")]
        public List<Feature> Feature { get; set; }
        [XmlElement(ElementName = "extensions")]
        public Extensions Extensions { get; set; }
    }
}
