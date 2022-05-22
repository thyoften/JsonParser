using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    public class JsonTokenizer
    {
        private string _json;

        private readonly char[] _skipChars = { '\n', '\t', (char)13, (char)10 };
        private readonly char[] _doubleChars = { '.', 'e', 'E', '+', '-' };


        public JsonTokenizer(string json)
        {
            _json = json;
        }

        public string[] GetTokens(bool array = false)
        {
            int open_brackets = _json.Where(x => x == '{').Count();
            int close_brackets = _json.Where(x => x == '}').Count();

            if (open_brackets != close_brackets)
                throw new Exception("Unbalanced brackets!");

            List<string> tokens = new List<string>();

            string buffer = "";
            int brackets_nestedness = 0; //Used for getting the right amount of [] or {} if nested

            for (int i = 0; i < _json.Length; i++)
            {
                if (_skipChars.Contains(_json[i]) || _json[i] == ' ')
                    continue;
                else if(!array && _json[i] == '{')
                {
                    tokens.Add(_json[i].ToString());
                }
                else if (!array && _json[i] == '}')
                {
                    tokens.Add(_json[i].ToString());
                }
                else if (_json[i] == ':')
                {
                    tokens.Add(_json[i].ToString());
                }
                else if (_json[i] == ',' && !array)
                {
                    tokens.Add(_json[i].ToString());
                }
                else if (_json[i] == '\"')
                {
                    i++;
                    while (i < _json.Length && _json[i] != '\"')
                    {
                        if (i + 1 < _json.Length && (_json[i] == '\\' && _json[i + 1] == '\"')) //do not stop at escaped strings
                        {
                            buffer += '\\';
                            buffer += '\"';
                            i += 2;
                        }
                        else
                        {
                            buffer += _json[i];
                            i++;
                        }
                    }

                    tokens.Add('\"' + buffer + '\"');

                    buffer = "";
                }
                else if (_json[i] == '[' || _json[i] == '{')
                {
                    buffer += _json[i].ToString();
                    brackets_nestedness++;
                    i++;
                    while (i < _json.Length)
                    {
                        if (!_skipChars.Contains(_json[i]))
                            buffer += _json[i];
                        if (array)
                        {
                            if (_json[i] == '[' || _json[i] == '{')
                                brackets_nestedness++;
                            else if (_json[i] == ']' || _json[i] == '}')
                                brackets_nestedness--;

                            i++;

                            if (brackets_nestedness == 0) //We are done!
                                break;
                        } else
                        {
                            if (_json[i] == '[')
                                brackets_nestedness++;
                            else if (_json[i] == ']')
                                brackets_nestedness--;

                            i++;

                            if (brackets_nestedness == 0) //We are done!
                                break;
                        }
                    }

                    tokens.Add(buffer);
                    buffer = "";
                }
                else if (_json[i] == 't')
                {
                    i += 3;
                    tokens.Add("true");
                    buffer = "";
                }
                else if (_json[i] == 'f')
                {
                    i += 4;
                    tokens.Add("false");
                    buffer = "";
                }
                else if (_json[i] == 'n')
                {
                    i += 3;
                    tokens.Add("null");
                    buffer = "";
                }
                else if (Char.IsDigit(_json[i]))
                {
                    while (i < _json.Length && (Char.IsDigit(_json[i]) || _doubleChars.Contains(_json[i])))
                    {
                        buffer += _json[i];
                        i++;
                    }

                    tokens.Add(buffer);

                    buffer = "";
                }
                else
                    throw new Exception("Invalid character: " + _json[i] + " at " + i);
            }

            return tokens.ToArray();
        }
    }
}
