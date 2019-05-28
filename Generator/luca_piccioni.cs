// Copyright (C) 2015 Luca Piccioni
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;

namespace Generator
{
    public static class luca_piccioni
    {
        //copied from https://github.com/luca-piccioni/OpenGL.Net/blob/master/BindingsGen/GLSpecs/Enumerant.cs
        public static string ConstantDeclaration(string Name, string Value)
        {
            if (Value == null)
                return string.Empty;

            string type, value = Value;

            // Remove trailing \r
            value = value.TrimEnd('\r');
            // Remove parenthesis
            if (value.StartsWith("(") && value.EndsWith(")"))
                value = value.Substring(1, value.Length - 2);

            if (value.StartsWith("0x")) {
                if ((value.Length > 10) || (value.EndsWith("ull"))) {		// 0xXXXXXXXXXXull
                    // Remove ull suffix
                    value = value.Substring(0, value.Length - 3);
                    type = "ulong";
                } else if (Regex.IsMatch(value, @"0x\w{8}") && (Name.Contains("_BIT") || Name.Contains("_MASK"))) {
                    type = "uint";
                } else if (Regex.IsMatch(value, @"0x\w{8}") && Name.StartsWith("GL_SWAP_")) {
                    type = "uint";
                } else if (Regex.IsMatch(value, @"0x[8F]\w{7}")) {
                    type = "uint";
                } else {													// 0xXXXX
                    type = "int";
                }
            } else if (value.EndsWith("u")) {
                type = "uint";
            } else if (value.EndsWith("f")) {
                type = "float";
            } else if (value.StartsWith("\"")) {
                type = "string";
            } else {
                type = "int";
            }

            Match castMatch;

            if ((castMatch = Regex.Match(value, @"\(\([\w\d_]+\)\(?(?<value>(\+|\-)?[\d\.]+)\)?\)")).Success) {
                value = castMatch.Groups["value"].Value;
            }

            if ((castMatch = Regex.Match(value, @"\([\w\d_]+\)\(?(?<value>(\+|\-)?[\d\.]+f?)\)?")).Success) {
                value = castMatch.Groups["value"].Value;
            }

            return (string.Format("public const {0} {1} = {2};", type, Name, value));
        }
    }
}