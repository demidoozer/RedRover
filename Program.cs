using System.Text;

internal static class Program
{
    private record Node(string Label)
    {
        public List<Node> Children { get; set; } = [];
    }

    private class NodeParser
    {
        private string Text { get; set; } = "";
        private int Index { get; set; } = 0;
        private StringBuilder CurrentString { get; } = new StringBuilder();

        public char CurrentCharacter { 
            get 
            {
                return this.Text[this.Index];
            }
        }
        public bool IsDone { 
            get 
            {
                return this.Text.Length <= this.Index;
            }
        }
        public bool IsEmpty { 
            get 
            {
                return this.CurrentString.Length == 0;
            }
        }

        public NodeParser(string text)
        {
            this.Text = text;
        }

        public void Next()
        {
            this.CurrentString.Append(this.CurrentCharacter);
            this.Index++;
        }

        public void Skip()
        {
            this.Index++;
        }

        public Node GetNode()
        {
            var node = new Node(this.CurrentString.ToString().Trim());
            this.CurrentString.Clear();

            return node;
        }
        
    }

    private static void ParseNode(NodeParser parser, bool isAlphabetical, Node node)
    {
        while(!parser.IsDone)
        {
            if (parser.CurrentCharacter == ',')
            {
                node.Children.Add(parser.GetNode());
                parser.Skip();
            }
            else if(parser.CurrentCharacter == '(')
            {
                var parent = parser.GetNode();
                parser.Skip();
                ParseNode(parser, isAlphabetical, parent);
                node.Children.Add(parent);
            }
            else if(parser.CurrentCharacter == ')')
            {
                parser.Skip();

                if (parser.CurrentCharacter == ',')
                {
                    parser.Skip(); 
                }
                if (!parser.IsEmpty) {
                    node.Children.Add(parser.GetNode());
                }
                if (isAlphabetical)
                {
                    node.Children = node.Children.OrderBy(n => n.Label).ToList();
                }
                return;
            } 
            else
            {
                parser.Next();
            }
        }
         if (!parser.IsEmpty) {
            node.Children.Add(parser.GetNode());
        }
    }

    private static void WriteToConsole (List<Node> list, int depth = 0)
    {
        foreach(var node in list)
        {
            Console.WriteLine(new string(' ', depth * 4) + "- " + node.Label);
            WriteToConsole(node.Children, depth + 1);
        }
    }

    private static void Parse(string text, bool isAlphabetical)
    {
        var parser = new NodeParser(text[1..^1]);
        var node = new Node("");

        ParseNode(parser, isAlphabetical, node);

        if (isAlphabetical)
        {
            node.Children = node.Children.OrderBy(n => n.Label).ToList();
        }

        WriteToConsole(node.Children);
    }

    public static int Main(string[] args)
    {
        try
        {
            var isAlphabetical = args.Contains("-a");
            var input = args.Where(a => a != "-a").ToArray();

            // Make sure a string is passed in
            if (input.Length == 0 || input[0].Length == 0)
            {
                Console.WriteLine("Please pass a string.");
                return 0;
            }

            // Check for extra params
            if (input.Length > 1 || args.Length > 2)
            {
                Console.WriteLine("Only the alphabetical flag and a single string are acceptable params.");
                return 0;
            }

            // Check that the string is contained in parentheses
            string text = input[0];
            char first = text[0];
            char last = text[text.Length - 1];

            if (first != '('|| last != ')')
            {
                Console.WriteLine("String must start with '(' and end with ')'");
                return 0;
            }

            Parse(text, isAlphabetical);
            
            return 0;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine();
            return 1;
        }
    }
}