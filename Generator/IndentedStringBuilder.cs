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

using System.Text;

namespace Generator
{
    public class IndentedStringBuilder
    {
        private StringBuilder _stringBuilder;
        private string _currentIndentation;

        public IndentedStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        public void Indent()
        {
            _currentIndentation = _currentIndentation + "    ";
        }

        public void Outdent()
        {
            if (_currentIndentation.Length > 0)
                _currentIndentation = _currentIndentation.Substring(4);
        }

        public void AppendLine(string value)
        {
            _stringBuilder.AppendLine(_currentIndentation + value);
        }

        public void AppendLine(string format, params object[] args) {
            _stringBuilder.AppendLine(_currentIndentation + string.Format(format, args));
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}