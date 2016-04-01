using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GelDesk.UI
{
    public class TreeViewController : ViewController<TreeView>
    {
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            View = new TreeView();
        }
        protected override void OnLoad(RpcContext rpc)
        {
            View.Name = Name;
            View.Indent = 14;
            View.SetDockStyle(ObjectData);
            LoadTreeNodes();
            
        }
        TreeNode CreateTreeNodeFromJObject(JObject data)
        {
            if (data == null)
                return null;

            var id = (string)data["id"];
            var text = (string)data["text"] ?? id ?? "";
            id = id ?? text;

            var node = new TreeNode(text);
            node.Name = id;
            
            var subTreeNodes = CreateTreeNodes(data);
            if (subTreeNodes != null)
                node.Nodes.AddRange(subTreeNodes);

            return node;
        }
        TreeNode CreateTreeNodeFromJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.None:
                    break;
                case JTokenType.Object:
                    return CreateTreeNodeFromJObject((JObject)token);
                case JTokenType.Array:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Integer:
                case JTokenType.Float:
                    break;
                case JTokenType.String:
                    return CreateTreeNodeFromString((string)token);
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    break;
            }
            throw new InvalidOperationException("Invalid TreeNode data type: {0}".FormatSafe(token.Type));
        }
        TreeNode CreateTreeNodeFromString(string text)
        {
            return new TreeNode(text) { Name = text };
        }
        TreeNode[] CreateTreeNodes(JObject data, string key = "nodes")
        {
            var token = data[key];
            if (token == null || token.Type != JTokenType.Array)
                return null;
            var tokens = (JArray)token;
            if (tokens.Count == 0)
                return null;
            return tokens.Select(CreateTreeNodeFromJToken).ToArray();
        }
        void LoadTreeNodes()
        {
            var treeNodes = CreateTreeNodes(ObjectData);
            if (treeNodes == null)
                return;
            View.Nodes.AddRange(treeNodes);
        }
    }
}
