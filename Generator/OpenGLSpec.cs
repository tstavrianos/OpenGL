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
	[XmlRoot(ElementName="type")]
	public class Type {
		[XmlAttribute(AttributeName="name")]
		public string _Name { get; set; }
		[XmlElement(ElementName="name")]
		public string Name { get; set; }
		[XmlText]
		public string Text { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlAttribute(AttributeName="requires")]
		public string Requires { get; set; }
		[XmlElement(ElementName="apientry")]
		public string Apientry { get; set; }
		[XmlAttribute(AttributeName="api")]
		public string Api { get; set; }
	}

	[XmlRoot(ElementName="types")]
	public class Types {
		[XmlElement(ElementName="type")]
		public List<Type> Type { get; set; }
	}

	[XmlRoot(ElementName="enum")]
	public class Enum {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="value")]
		public string Value { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlAttribute(AttributeName="alias")]
		public string Alias { get; set; }
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName="api")]
		public string Api { get; set; }

		public void ConstantDeclaration(IndentedStringBuilder sb) {
			if(Name == "GL_ACTIVE_PROGRAM_EXT" && Api == "gles2") return;
			sb.AppendLine(luca_piccioni.ConstantDeclaration(Name, Value));
		}
	}

	[XmlRoot(ElementName="group")]
	public class Group {
		[XmlElement(ElementName="enum")]
		public List<Enum> Enum { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
	}

	[XmlRoot(ElementName="groups")]
	public class Groups {
		[XmlElement(ElementName="group")]
		public List<Group> Group { get; set; }
	}

	[XmlRoot(ElementName="enums")]
	public class Enums {
		[XmlElement(ElementName="enum")]
		public List<Enum> Enum { get; set; }
		[XmlAttribute(AttributeName="namespace")]
		public string Namespace { get; set; }
		[XmlAttribute(AttributeName="group")]
		public string Group { get; set; }
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlElement(ElementName="unused")]
		public List<Unused> Unused { get; set; }
		[XmlAttribute(AttributeName="vendor")]
		public string Vendor { get; set; }
		[XmlAttribute(AttributeName="start")]
		public string Start { get; set; }
		[XmlAttribute(AttributeName="end")]
		public string End { get; set; }


        private static string FixName(string value) {
            var output = string.Empty;
            if(value.StartsWith("GL_")) value = value.Substring(3);

            for(var i = 0; i < value.Length; i++) {
                if(i == 0) output += char.ToUpper(value[i]);
                else if(value[i] == '_') continue;
                else if(value[i-1] == '_') output += char.ToUpper(value[i]);
                else output += char.ToLower(value[i]);
            }
            if(output[0] >= '0' && output[0] <= '9') output = '_' + output;
            return output;
        }
		public void StronglyTypedDeclaration(IndentedStringBuilder sb) {
			if(!string.IsNullOrEmpty(Comment)) sb.AppendLine("// {0}", Comment);
			sb.AppendLine("public enum {0}{1} {{", Group, !string.IsNullOrEmpty(Type) && Type == "bitmask" ? ": uint": "");
			sb.Indent();
			var enumHash = new HashSet<string>();
			foreach(var e1 in Enum) {
				var name = FixName(e1.Name);
				if(enumHash.Contains(name)) continue;
				enumHash.Add(name);
				if(!string.IsNullOrEmpty(e1.Comment)) sb.AppendLine("// {0}", e1.Comment);
				sb.AppendLine("{0} = {1},", name, e1.Value);
			}
			sb.Outdent();
			sb.AppendLine("}");
		}
	}

	[XmlRoot(ElementName="unused")]
	public class Unused {
		[XmlAttribute(AttributeName="start")]
		public string Start { get; set; }
		[XmlAttribute(AttributeName="end")]
		public string End { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlAttribute(AttributeName="vendor")]
		public string Vendor { get; set; }
	}

	[XmlRoot(ElementName="proto")]
	public class Proto {
        [XmlText()]
        public string ReturnTypeText;

		[XmlElement(ElementName="name")]
		public string Name { get; set; }
		[XmlElement(ElementName="ptype")]
		public string Ptype { get; set; }
		[XmlAttribute(AttributeName="group")]
		public string Group { get; set; }
		public string ReturnType;
		public bool MarshalString;
	}

	[XmlRoot(ElementName="param")]
	public class Param {
        [XmlText()]
        public List<string> TypeDecorators = new List<string>();

		[XmlElement(ElementName="ptype")]
		public string Ptype { get; set; }
		[XmlElement(ElementName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="group")]
		public string Group { get; set; }
		[XmlAttribute(AttributeName="len")]
		public string Len { get; set; }
		public string Translated;
		public bool NeedsFixed;
		public string EnumTranslated;
		public bool HasEnum;
	}

	[XmlRoot(ElementName="glx")]
	public class Glx {
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName="opcode")]
		public string Opcode { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
	}

	[XmlRoot(ElementName="command")]
	public class Command {
		[XmlElement(ElementName="proto")]
		public Proto Proto { get; set; }
		[XmlElement(ElementName="param")]
		public List<Param> Param { get; set; }
		[XmlElement(ElementName="glx")]
		public List<Glx> Glx { get; set; }
		[XmlElement(ElementName="alias")]
		public Alias Alias { get; set; }
		[XmlElement(ElementName="vecequiv")]
		public Vecequiv Vecequiv { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }

		public void FixParams(Registry reg) {
			if(Param != null) {
				foreach(var param in Param) {
					param.Name = Senzible.CleanUpName(param.Name);
					param.Translated = Senzible.GetReturnType(param);
					param.NeedsFixed = param.Translated.EndsWith("*");
					param.HasEnum = !string.IsNullOrEmpty(param.Group) && reg.Enums.Any(x => x.Group == param.Group);
					param.EnumTranslated = !param.HasEnum ? param.Translated : ("Enums." + param.Group + (param.NeedsFixed ? "*" : ""));
				}
			}			
		}

		public void BuildDelegate(IndentedStringBuilder sb) {
			sb.AppendLine("internal {0}delegate {1} {2}({3});", 
				Param != null && Param.Any(x => x.NeedsFixed) ? "unsafe " : "",
				Proto.ReturnType, 
				Proto.Name,
				Param != null ? string.Join(", ", Param.Select(x => x.Translated + " " + x.Name)): "");
			
		}

		public void BuildPointer(IndentedStringBuilder sb) {
			sb.AppendLine("internal static Delegates.{0} {0} = null;", Proto.Name);
		}

		public void BuildWrappers(IndentedStringBuilder sb) {
			sb.AppendLine("/// <summary>");
			if(!string.IsNullOrEmpty(Comment)) {
				sb.AppendLine("/// {0}", Comment);
			} else {
				sb.AppendLine("/// ");
			}
			sb.AppendLine("/// </summary>");
			foreach(var param in Param) {
				sb.AppendLine("/// <param name=\"{0}\"></param>", param.Name);
			}
			sb.AppendLine("public {0}static {1} {2}({3}) => {4}Pointers.{2}({5}){6};", 
				Param != null && Param.Any(x => x.NeedsFixed) ? "unsafe " : "",
				Proto.MarshalString ? "string" : Proto.ReturnType, 
				Proto.Name,
				Param != null ? string.Join(", ", Param.Select(x => x.Translated + " " + x.Name)): "",
				Proto.MarshalString ? "PtrToStringUTF8(" : "",
				Param != null ? string.Join(", ", Param.Select(x => x.Name)): "",
				Proto.MarshalString ? ")" : ""
				);
			if(Param.Any(x => x.HasEnum)) {
				sb.AppendLine("");
				sb.AppendLine("public {0}static {1} {2}({3}) => {4}Pointers.{2}({5}){6};", 
					Param != null && Param.Any(x => x.NeedsFixed) ? "unsafe " : "",
					Proto.MarshalString ? "string" : Proto.ReturnType, 
					Proto.Name,
					Param != null ? string.Join(", ", Param.Select(x => x.EnumTranslated + " " + x.Name)): "",
					Proto.MarshalString ? "PtrToStringUTF8(" : "",
					Param != null ? string.Join(", ", Param.Select(x => (string.IsNullOrEmpty(x.Group) ? "" : "(" + x.Translated + ")") + x.Name)): "",
					Proto.MarshalString ? ")" : ""
					);
			}
			if (Param != null && Param.Any(x => x.NeedsFixed)) {
				sb.AppendLine("");
				sb.AppendLine("/// <summary>");
				if(!string.IsNullOrEmpty(Comment)) {
					sb.AppendLine("/// {0}", Comment);
				} else {
					sb.AppendLine("/// ");
				}
				sb.AppendLine("/// </summary>");
				foreach(var param in Param) {
					sb.AppendLine("/// <param name=\"{0}\"></param>", param.Name);
				}
				sb.AppendLine("public unsafe static {0} {1}({2}) {{", 
					Proto.ReturnType,
					Proto.Name,
					string.Join(", ", Param.Select(x => (x.NeedsFixed ? x.Translated.Substring(0, x.Translated.Length - 1) +"[]" : x.Translated) + " " + x.Name))
				);
				sb.Indent();
				foreach(var param in Param)
				{
					if(param.NeedsFixed) {
						sb.AppendLine("fixed({0} {1}_ = &{1}[0])", param.Translated, param.Name);
					}
				}
				sb.Indent();
				sb.AppendLine("{0}Pointers.{1}({2});", 
					Proto.ReturnType == "void" ? "" : "return ",
					Proto.Name, 
					string.Join(", ", Param.Select(x => x.NeedsFixed ? (x.Name + "_") : x.Name))
				);
				sb.Outdent();
				sb.Outdent();
				sb.AppendLine("}");

				if(Param.Any(x => x.HasEnum)) {
					sb.AppendLine("");
					sb.AppendLine("public unsafe static {0} {1}({2}) {{", 
						Proto.ReturnType,
						Proto.Name,
						string.Join(", ", Param.Select(x => (x.NeedsFixed ? x.EnumTranslated.Substring(0, x.EnumTranslated.Length - 1) +"[]" : x.EnumTranslated) + " " + x.Name))
					);
					sb.Indent();
					foreach(var param in Param)
					{
						if(param.NeedsFixed) {
							sb.AppendLine("fixed({0} {1}_ = &{1}[0])", param.EnumTranslated, param.Name);
						}
					}
					sb.Indent();
					sb.AppendLine("{0}{1}({2});", 
						Proto.ReturnType == "void" ? "" : "return ",
						Proto.Name, 
						string.Join(", ", Param.Select(x => x.NeedsFixed ? (x.Name + "_") : x.Name))
					);
					sb.Outdent();
					sb.Outdent();
					sb.AppendLine("}");
				}
			}
		}

		public static void BuildLoader(IndentedStringBuilder sb, string name) {
			sb.AppendLine("if(Pointers.{0} == null) if((proc = loadProc(\"{0}\")) != IntPtr.Zero) Pointers.{0} = (Delegates.{0})Marshal.GetDelegateForFunctionPointer(proc, typeof(Delegates.{0}));", name);
		}

		public void BuildLoader(IndentedStringBuilder sb) {
			BuildLoader(sb, Name);
		}		
	}

	[XmlRoot(ElementName="alias")]
	public class Alias {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="vecequiv")]
	public class Vecequiv {
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName="commands")]
	public class Commands {
		[XmlElement(ElementName="command")]
		public List<Command> Command { get; set; }
		[XmlAttribute(AttributeName="namespace")]
		public string Namespace { get; set; }

		public void Build(IndentedStringBuilder delegates, IndentedStringBuilder pointers, IndentedStringBuilder wrappers, Registry reg, HashSet<string> commands) {
			var wrapperStart = true;
			foreach(var c in Command) {
				c.Proto.ReturnType = Senzible.GetReturnType(c.Proto);
				c.FixParams(reg);
				c.BuildDelegate(delegates);
				c.BuildPointer(pointers);
				if(!wrapperStart) {
					wrappers.AppendLine("");
				} else {
					wrapperStart = false;
				}
				c.BuildWrappers(wrappers);
				commands.Add(c.Proto.Name);
			}
		}
	}

	[XmlRoot(ElementName="require")]
	public class Require {
		[XmlElement(ElementName="type")]
		public List<Type> Type { get; set; }
		[XmlElement(ElementName="enum")]
		public List<Enum> Enum { get; set; }
		[XmlElement(ElementName="command")]
		public List<Command> Command { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlAttribute(AttributeName="profile")]
		public string Profile { get; set; }
		[XmlAttribute(AttributeName="api")]
		public string Api { get; set; }

		public void BuildLoader(IndentedStringBuilder sb, HashSet<string> commands) {
			if(Command == null || Command.Count == 0) return;
			foreach(var cmd in Command) {
				cmd.BuildLoader(sb);
				commands.Remove(cmd.Name);
			}			
		}
	}

	[XmlRoot(ElementName="feature")]
	public class Feature {
		[XmlElement(ElementName="require")]
		public List<Require> Require { get; set; }
		[XmlAttribute(AttributeName="api")]
		public string Api { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="number")]
		public string Number { get; set; }
		[XmlElement(ElementName="remove")]
		public List<Remove> Remove { get; set; }

		public void BuildDeclaration(IndentedStringBuilder sb) {
			sb.AppendLine("public static bool {0} {{get; internal set;}} = false;", Name);
		}

		public void BuildAssign(IndentedStringBuilder sb) {
			sb.AppendLine("if(version >= {0}) Features.{1} = true;", Number.Replace(".", ""), Name);
		}

		public void BuildLoader(IndentedStringBuilder sb, HashSet<string> commands) {
			if(Require == null || !Require.Any(x => x.Command != null && x.Command.Count > 0)) return;
			sb.AppendLine("if(Features.{0}) {{", Name);
			sb.Indent();
			foreach(var require in Require) {
				require.BuildLoader(sb, commands);
			}
			if(Remove != null && Remove.Any(x => x.Command != null && x.Command.Count > 0)) {
				sb.AppendLine("if(disableRemovedProcs) {");
				sb.Indent();
				foreach(var remove in Remove.Where(x => x.Command != null && x.Command.Count > 0)) {
					remove.BuildLoader(sb);
				}
				sb.Outdent();
				sb.AppendLine("}");
			}
			sb.Outdent();
			sb.AppendLine("}");
		}
	}

	[XmlRoot(ElementName="remove")]
	public class Remove {
		[XmlElement(ElementName="command")]
		public List<Command> Command { get; set; }
		[XmlAttribute(AttributeName="profile")]
		public string Profile { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }
		[XmlElement(ElementName="enum")]
		public List<Enum> Enum { get; set; }

		public void BuildLoader(IndentedStringBuilder sb) {
			if(Command == null || Command.Count == 0) return;
			foreach(var cmd in Command) {
				sb.AppendLine("Pointers.{0} = null;", cmd.Name);
			}			
		}
	}

	[XmlRoot(ElementName="extension")]
	public class Extension {
		[XmlElement(ElementName="require")]
		public List<Require> Require { get; set; }
		[XmlAttribute(AttributeName="name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName="supported")]
		public string Supported { get; set; }
		[XmlAttribute(AttributeName="comment")]
		public string Comment { get; set; }

		public void BuildLoader(IndentedStringBuilder sb, HashSet<string> commands) {
			if(Require == null) return;
			sb.AppendLine("if(Features.ExtensionsGPU.Contains(\"{0}\")) {{", Name);
			sb.Indent();
			foreach(var req in Require) {
				req.BuildLoader(sb, commands);
			}
			sb.Outdent();
			sb.AppendLine("}");
		}
	}

	[XmlRoot(ElementName="extensions")]
	public class Extensions {
		[XmlElement(ElementName="extension")]
		public List<Extension> Extension { get; set; }

		public void BuildLoader(IndentedStringBuilder sb, string api, HashSet<string> commands) {
            foreach(var ext in Extension.Where(x => x.Supported == api && x.Require != null && x.Require.Any(y => y.Command != null && y.Command.Count > 0))) {
                ext.BuildLoader(sb, commands);
            }
		}
	}

	[XmlRoot(ElementName="registry")]
	public class Registry {
		[XmlElement(ElementName="comment")]
		public string Comment { get; set; }
		[XmlElement(ElementName="types")]
		public Types Types { get; set; }
		[XmlElement(ElementName="groups")]
		public Groups Groups { get; set; }
		[XmlElement(ElementName="enums")]
		public List<Enums> Enums { get; set; }
		[XmlElement(ElementName="commands")]
		public Commands Commands { get; set; }
		[XmlElement(ElementName="feature")]
		public List<Feature> Feature { get; set; }
		[XmlElement(ElementName="extensions")]
		public Extensions Extensions { get; set; }
	}
}
