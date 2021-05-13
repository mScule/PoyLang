using System.Collections.Generic;

namespace PoyLang
{
    public enum NodeType
    {
        Value,                  // number, string, or varname

        CommandStatement,

        ConsoleCommand,         // :ConsoleCommand (caseinsensitive)
        CustomCommand,          // @CustomCommand  (caseinsensitive)

        Parameters,

        // operations
        AssignmentOperation,
        BinaryOperation,
        UnaryOperation,
        StringOperation,
        IncrementOperation,
        DecrementOperation,

        StatementList,
        Empty
    }

    public class Node
    {
        public NodeType type { get; }
        public Token content { get; }
        public Node[] children { get; }

        public Node(NodeType type, Token content, Node[] children)
        {
            this.type = type;
            this.content = content;
            this.children = children;
        }

        public override string ToString()
        {
            List<string> stringChildren = new List<string>();

            if (children != null)
                foreach (Node child in children)
                    stringChildren.Add(Print.Value("child", child.type.ToString()));
            else
                stringChildren.Add("no children");

            return Print.Item("Node",
                Print.SubContent(new string[] {
                    Print.Value("Type", type.ToString()),
                    Print.Value("Content", content.ToString()),
                    Print.Value("Children", Print.SubContent(stringChildren.ToArray()))
                })
            );
        }
    }
}
