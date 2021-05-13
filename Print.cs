namespace PoyLang
{
    public static class Print
    {
        public static string Alert(string origin, string message)
        {
            return origin + "! \"" + message + "\".";
        }

        public static string Message(string sender, string message)
        {
            return sender + ": " + message;
        }

        public static string Item(string type, string content)
        {
            return type + ": [" + content + "]";
        }

        public static string Value(string type, string value)
        {
            return type + '(' + value + ')';
        }

        public static string SubContent(string[] items)
        {
            string subcontent = "";

            for (int i = 0; i < items.Length; i++)
            {
                if (i != 0)
                    subcontent += ", ";

                subcontent += items[i];
            }

            return subcontent;
        }
    }
}
