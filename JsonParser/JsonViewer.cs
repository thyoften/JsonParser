using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    public class JsonViewer
    { 
        //Quick and dirty tree viewer
        public void ViewJson(JsonNode node)
        {
            node.Visit(node, x =>
            {
                string data_string = "";
                if (x.IsRoot)
                    data_string = "/";
                else
                {
                    if (x.GetData().value == null)
                    {
                        data_string = x.GetData().key + " : null";
                    }
                    else if (x.GetData().value is string)
                    {
                        if (x.GetData().value.ToString() == "")
                            data_string = x.GetData().key;
                        else
                            data_string = x.GetData().key + " : " + x.GetData().value;
                    }
                    else if (x.GetData().value is int)
                    {
                        data_string = x.GetData().key + " : " + (int)x.GetData().value;
                    }
                    else if (x.GetData().value is double)
                    {
                        data_string = x.GetData().key + " : " + (double)x.GetData().value;
                    }
                    else if (x.GetData().value is List<object>)
                    {
                        data_string = x.GetData().key + " : [";
                        foreach (object data in (x.GetData().value as List<object>))
                        {
                            //TODO: fix these ugly ifs
                            if (!(data is List<object>) && !(data is JsonNode))
                                data_string += data.ToString() + ", ";
                            else
                            {
                                if (data is List<object>)
                                {
                                    data_string += "[";
                                    foreach (var item in (data as List<object>))
                                    {
                                        data_string += item.ToString() + ", ";
                                    }
                                    data_string = data_string.TrimEnd(',', ' ');
                                    data_string += "], ";
                                }
                                else if (data is JsonNode)
                                {
                                    data_string += "{\n";
                                    (data as JsonNode).Visit(data as JsonNode, sub =>
                                    {
                                        if (!sub.IsRoot)
                                            data_string += "{" + sub.GetData().key + " : " + sub.GetData().value + "},\n";
                                    });
                                    data_string = data_string.TrimEnd(',', ' ');
                                    data_string += "}, ";
                                }
                            }
                        }
                        data_string = data_string.TrimEnd(',', ' ') + "]";

                    }
                    else if (x.GetData().value is bool)
                    {
                        data_string = x.GetData().key + " : " + ((bool)x.GetData().value).ToString();
                    }
                    else
                    {
                        data_string = x.GetData().key + " : <unrecognized object> (" + x.GetData().value.GetType().Name + ")";
                    }
                }
                Console.WriteLine("{0}{1}", new string(' ', x.GetDepth() * 2), data_string);
            });
        }

    }
}
