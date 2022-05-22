using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonParser
{
    public class JsonReader
    {
        private string _json = "";

        private enum JsonParserStates
        {
            EXP_OPEN_BRACKET, EXP_CLOSE_BRACKET, EXP_DEF, EXP_STRING, EXP_COMMA
        }

        public JsonReader(string json)
        {
            _json = json;
        }

        private bool IsInteger(string n)
        {
            return n.All(Char.IsDigit);
        }

        private bool IsDouble(string n)
        {
            return !n.Any(x => x == '{' || x == '}') && Regex.IsMatch(n, @"[0-9]+\.[0-9]+") || Regex.IsMatch(n, @"[-]?[0-9]+(\.[0-9]+)?[Ee][+-][0-9]+");
        }

        private List<object> ParseJsonArray(string arrayToken)
        {
            arrayToken = arrayToken.Substring(1, arrayToken.Length - 2); //remove []s
            arrayToken = arrayToken.TrimStart(' ').TrimEnd(' ');

            List<object> arrayData = new List<object>();

            var tokens = new JsonTokenizer(arrayToken).GetTokens(true);

            foreach (var token in tokens)
            {
                if (IsInteger(token))
                {
                    arrayData.Add(int.Parse(token, CultureInfo.InvariantCulture));
                }
                else if (IsDouble(token))
                {
                    arrayData.Add(double.Parse(token, CultureInfo.InvariantCulture));
                }
                else if (token.StartsWith("["))
                {
                    arrayData.Add(ParseJsonArray(token));
                }
                else if (token.StartsWith("{"))
                {
                    arrayData.Add(new JsonReader(token).Read());
                }
                else if (new char[] { ' ', ',', '\n', '\t', (char)13, (char)10 }.Contains(token[0]))
                    continue;
            }

            return arrayData;
        }

        public JsonNode Read()
        {
            JsonParserStates state = JsonParserStates.EXP_OPEN_BRACKET;

            JsonNode document = new JsonNode("/", "");

            Stack<JsonNode> stack = new Stack<JsonNode>();
            Stack<string> seminodes = new Stack<string>();

            stack.Push(document);


            var tokens = new JsonTokenizer(_json).GetTokens();

            for (int i = 0; i < tokens.Length; i++)
            {
                
                if (tokens[i] == "{")
                {
                    if (state == JsonParserStates.EXP_OPEN_BRACKET)
                        state = JsonParserStates.EXP_STRING;
                    else
                        throw new Exception("Unexpected { at " + i);
                }
                else if (tokens[i][0] == '\"' && state == JsonParserStates.EXP_STRING)
                {
                    seminodes.Push(tokens[i].Trim('\"'));
                    state = JsonParserStates.EXP_DEF;
                }
                else if (tokens[i] == ":" && state == JsonParserStates.EXP_DEF)
                {
                    if (i + 1 < tokens.Length && tokens[i + 1].StartsWith("\""))
                    {
                        var new_data = (seminodes.Pop(), tokens[i + 1].Substring(1, tokens[i + 1].Length - 2));

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new_data);

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                    else if (i + 1 < tokens.Length && tokens[i + 1].StartsWith("["))
                    {
                        var new_data = (seminodes.Pop(), ParseJsonArray(tokens[i + 1]));

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new_data);

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                    else if (i + 1 < tokens.Length && tokens[i + 1] == "{")
                    {
                        //Make a partial node only
                        //The actual subnode will be linked when we reach a }
                        //Could use a recursive approach but meh
                        JsonNode new_node = new JsonNode(seminodes.Pop().Trim('\"'), "");
                        stack.Push(new_node);
                        state = JsonParserStates.EXP_OPEN_BRACKET;
                    }
                    else if (i + 1 < tokens.Length && tokens[i + 1] == "true")
                    {
                        var new_data = (seminodes.Pop(), true);

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new_data);

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                    else if (i + 1 < tokens.Length && tokens[i + 1] == "false")
                    {
                        var new_data = (seminodes.Pop(), false);

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new_data);

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                    else if (i + 1 < tokens.Length && tokens[i + 1] == "null")
                    {
                        //var new_data = (seminodes.Pop(), null);

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new JsonNode(seminodes.Pop(), null));

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                    else if (i + 1 < tokens.Length && IsInteger(tokens[i + 1]))
                    {
                        var new_data = (seminodes.Pop(), int.Parse(tokens[i + 1], CultureInfo.InvariantCulture));

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new_data);

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                    else if (i + 1 < tokens.Length && IsDouble(tokens[i + 1]))
                    {
                        var new_data = (seminodes.Pop(), double.Parse(tokens[i + 1], CultureInfo.InvariantCulture));

                        var parent = stack.Pop();

                        parent.AddChildNode(parent, new_data);

                        stack.Push(parent);

                        i++;

                        state = JsonParserStates.EXP_COMMA;
                    }
                }
                else if (tokens[i] == ",")
                {
                    if (state == JsonParserStates.EXP_COMMA)
                        state = JsonParserStates.EXP_STRING;
                    else
                        throw new Exception("Unexpected comma at " + i);
                }
                else if (tokens[i] == "}") {
                    if (state == JsonParserStates.EXP_COMMA || state == JsonParserStates.EXP_CLOSE_BRACKET || state == JsonParserStates.EXP_STRING)
                        //if we expect a comma but find a } it probably means the document is finished
                        if (i == tokens.Length - 1)
                            break;
                        else
                        { //Bind the children nodes to a parent
                            var child = stack.Pop();
                            var parent = stack.Pop();
                            parent.AddChildNode(parent, child);
                            stack.Push(parent);
                        }
                    else
                        throw new Exception("Unexpected } at " + i);
                }
            }

            //Fix depth, useful for printing
            document.Visit(document, x =>
            {
                if (!x.IsRoot)
                    x.SetDepth(x.GetParent().GetDepth() + 1);
            });

            return document;
        }

    }
}
