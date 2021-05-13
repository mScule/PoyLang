namespace PoyLang
{
    public enum TokenType
    {
        // data
        Number, String, Varname,

        // +, -
        Addition, Subtraction,

        // *, /
        Multiplication, Division,

        Assignment, // for assignment        (=)
        Ampersand,  // for string operations (&)

        // +=, -=, *=, /=, &=
        AssAdd, AssSub, AssMul, AssDiv, AssAmp,

        // ++, --
        Increment, Decrement,

        // (, )
        ParentheseOpen, ParentheseClose,

        Semicolon, // for ending statement (;)

        // @, :
        CustomCommand, ConsoleCommand,

        // ,
        Comma,

        NotDefined, // for tokens that is created in parser
        EndOfFile
    }

    public class Token
    {
        public TokenType type { get; }
        public string value { get; }

        public Location location { get; }

        public Token(TokenType type, string value, Location location)
        {
            this.type = type;
            this.value = value;
            this.location = location;
        }

        public Token(Token token)
        {
            this.type = token.type;
            this.value = token.value;
            this.location = token.location;
        }

        public override string ToString()
        {
            return Print.Item("Token", Print.SubContent(new string[] { Print.Value("Type", type.ToString()), Print.Value("Value", value) }));
        }
    }
}
