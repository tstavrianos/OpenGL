/* 
MIT License

Copyright (c) 2017 Senzible

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO;
using System.Linq;

namespace Generator
{
    //copied from https://github.com/Senzible/senzible-opengl
    public static class Senzible
    {
        private static readonly string[] keywords = new string[] {
            "abstract", "event", "new", "struct",
            "as", "explicit", "null", "switch",
            "base", "extern", "object", "this",
            "bool", "false", "operator", "throw",
            "break", "finally", "out", "true",
            "byte",  "fixed", "override", "try",
            "case", "float", "params", "typeof",
            "catch", "for", "private", "uint",
            "char", "foreach", "protected", "ulong",
            "checked", "goto", "public", "unchecked",
            "class", "if", "readonly", "unsafe",
            "const", "implicit", "ref", "ushort",
            "continue", "in", "return", "using",
            "decimal", "int", "sbyte", "virtual",
            "default", "interface", "sealed", "volatile",
            "delegate", "internal", "short", "void",
            "do", "is", "sizeof", "while",
            "double", "lock", "stackalloc",
            "else", "long", "static",
            "enum", "namespace", "string",
        };
        
        public static void WriteStringBuilderToFile(IndentedStringBuilder sb, string path, string fileName)
        {
            using (var sw = new StreamWriter(path + fileName))
            {
                sw.Write(sb.ToString());
                sw.Close();
            }
        }

        public static string MapToCSharpType(string type) {
            type = type.Trim().ToLower();
            switch (type) {
                case "void":
                    return "void";
                case "glboolean":
                    return "bool";
                case "glboolean const *":
                case "glboolean *":
                    return "bool *";
                case "glboolean **":
                case "glboolean const **":
                    return "bool[]";
                case "glint":
                case "glenum":
                case "glsizei":
                case "glclampx":
                    return "int";
                case "glint const *":
                case "glsizei const *":
                case "glsizei *":
                case "glint *":
                case "glclampx const *":
                case "glenum *":
                    return "int*";
                case "gluint":
                case "glhandlearb":
                case "glbitfield":
                case "glsizeiptr":
                case "glsizeiptrarb":
                case "glintptr":
                    return "uint";
                case "glenum const *":
                case "gluint const *":
                case "gluint *":
                case "glhandlearb const *":
                case "glhandlearb *":
                case "glbitfield const *":
                case "glsizeiptr const *":
                case "glsizeiptrarb const *":
                case "glintptr const *":
                    return "uint*";
                case "gluint64":
                case "gluint64ext":
                    return "ulong";
                case "glint64":
                case "glint64ext":
                    return "long";
                case "gluint64 *":
                case "gluint64 const *":
                case "gluint64ext const *":
                case "gluint64ext *":
                    return "ulong*";
                case "glint64 *":
                case "glint64 const *":
                case "glint64ext const *":
                case "glint64ext *":
                    return "long*";
                case "glfloat":
                case "glclampf":
                    return "float";
                case "glfloat const *":
                case "glclampf const *":
                case "glfloat *":
                    return "float*";
                case "glubyte":
                    return "byte";
                case "glbyte":
                    return "sbyte";
                case "glushort":
                    return "ushort";
                case "glubyte const *":
                case "glubyte *":
                    return "byte*";
                case "glbyte const *":
                case "glbyte *":
                    return "sbyte*";
                case "glushort const *":
                case "glushort *":
                    return "ushort*";
                case "glvdpausurfacenv":
                case "glvulkanprocnv":
                case "void const *":
                case "void *":
                case "const void *":
                case "glfixed":
                case "gldebugproc":
                case "gldebugprocamd":
                case "gldebugprocarb":
                case "gldebugprockhr":
                case "gleglimageoes":
                case "gleglclientbufferext":
                case "glintptrarb":
                case "glsync":
                case "struct _cl_context *":
                case "struct _cl_event *":
                    return "IntPtr";
                case "const void **":
                case "void **":
                case "const void *const*":
                case "glfixed const *":
                case "glfixed *":
                case "glvdpausurfacenv const *":
                    return "IntPtr*";
                case "glshort":
                case "glhalfnv":
                    return "short";
                case "glshort const *":
                case "glhalfnv const *":
                    return "short*";
                case "gldouble":
                case "glclampd":
                    return "double";
                case "gldouble const *":
                case "gldouble *":
                case "glclampd const *":
                    return "double*";
                case "glchar":
                case "glchararb":
                    return "char";
                case "glchar *":
                case "glchararb *":
                    return "StringBuilder";
                case "const glchar *":
                case "glchar const *":
                case "glchararb const *":
                    return "string";
                case "glchar const *const*":
                case "glchararb **":
                case "glchararb const **":
                case "glchar **":
                case "glchar const **":
                    return "string[]";
                case "gluint [2]":
                    return "uint[]";
            }
            System.Console.WriteLine(type);
            return type;
        }

        public static string GetReturnType(OpenGLSpec.Param parameter) {
            string returnTypeText = null;
            if (parameter.TypeDecorators.Count > 0)
            {
                returnTypeText = string.Join(" ", parameter.TypeDecorators.Where(x => x.Trim() != "").Select(x => x.Trim())).Trim();
            }
            string ptype = parameter.Ptype != null ? parameter.Ptype.Trim() : null;

            if (ptype != null)
            {
                ptype = ptype.Trim();
            }

            return GetCSharpReturnType(returnTypeText, ptype);
        }

        public static string GetReturnType(OpenGLSpec.Proto prototype)
        {
            if (prototype.Group != null && prototype.Group == "String")
            {
                prototype.MarshalString = true;
                return "IntPtr";
            }

            string returnTypeText = prototype.ReturnTypeText != null ? prototype.ReturnTypeText.Trim() : null;
            string ptype = prototype.Ptype?.Trim();
            return GetCSharpReturnType(returnTypeText, ptype);
        }

        public static string GetCSharpReturnType(string returnTypeText, string ptype)
        {
            if ((ptype != null) && returnTypeText != null)
            {
                return MapToCSharpType(ptype + " " + returnTypeText);
            }
            else if (ptype != null)
            {
                return MapToCSharpType(ptype);
            }
            else if (returnTypeText != null)
            {
                return MapToCSharpType(returnTypeText);
            }
            else
            {
                return "IntPtr";
            }
        }

        public static string CleanUpName(string input)
        {
            if (IsCsKeyword(input))
            {
                return "_" + input;
            }
            return input;
        }

        public static bool IsCsKeyword(string input)
        {
            foreach (string keyword in keywords)
                if (keyword == input) return (true);
            return false;
        }
    }
}