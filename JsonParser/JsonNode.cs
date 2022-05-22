using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonParser
{
    [Serializable]
    public class JsonNode : TreeNode<(string key, object value)>
    {
        public JsonNode(string key, object value) : base((key, value))
        { }
    }
}
