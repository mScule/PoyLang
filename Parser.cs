using System.Collections.Generic;

namespace PoyLang
{
    public class Parser
    {
        private Tokenizer tokenizer;
        private Token currentToken;

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            currentToken = this.tokenizer.GetNextToken();
        }

        // Match with specific error message
        private void Match(TokenType type, string message)
        {
            if (currentToken.type != type)
                Error.Throw(ErrorOrigin.Parser, message, currentToken.location);
            else
                currentToken = tokenizer.GetNextToken();
        }

        // Match with generic error message
        private void Match(TokenType type)
        {
            if (currentToken.type != type)
                Error.Throw(ErrorOrigin.Parser,
                    "Waited for type " + type + ". " +
                    "Given type " + currentToken.type,

                    currentToken.location
                );
            else
                currentToken = tokenizer.GetNextToken();
        }

        // Factor : ParentheseOpen Expression ParentheseClose
        //        | Addition
        //        | Subtraction
        //        | Number
        //        | Varname
        //        | CustomCommand
        //        | ConsoleCommand

        private Node Factor()
        {
            Node factor = null;

            switch (currentToken.type)
            {
                case TokenType.ParentheseOpen:
                    Match(TokenType.ParentheseOpen, "");
                    factor = Expression();
                    Match(TokenType.ParentheseClose, "Waited for closing parenthese");
                    break;

                case TokenType.Addition:
                    Token addition = new Token(currentToken);
                    Match(TokenType.Addition);

                    factor = new Node(
                        NodeType.UnaryOperation,
                        new Token(addition),
                        new Node[] { Factor() });
                    break;

                case TokenType.Subtraction:
                    Token subtraction = new Token(currentToken);
                    Match(TokenType.Subtraction);

                    factor = new Node(
                        NodeType.UnaryOperation,
                        new Token(subtraction),
                        new Node[] { Factor() });
                    break;

                case TokenType.Number:
                    Token number = new Token(currentToken);
                    Match(TokenType.Number);

                    factor = new Node(NodeType.Value, new Token(number), null);
                    break;

                case TokenType.Varname:
                    Token varName = new Token(currentToken);
                    Match(TokenType.Varname);

                    factor = new Node(NodeType.Value, new Token(varName), null);
                    break;

                case TokenType.CustomCommand:
                    Token customCommand = new Token(currentToken);
                    Match(TokenType.CustomCommand);

                    Node command = new Node(NodeType.CustomCommand, customCommand, null);
                    Node parameters = Parameters();

                    factor = new Node
                    (
                        NodeType.CommandStatement,
                        new Token(TokenType.NotDefined, null, currentToken.location),
                        new Node[] { command, parameters }
                    );
                    break;

                case TokenType.ConsoleCommand:
                    Token consoleCommand = new Token(currentToken);
                    Match(TokenType.ConsoleCommand);

                    Node cmd = new Node(NodeType.ConsoleCommand, consoleCommand, null);
                    Node param = Parameters();

                    factor = new Node
                    (
                        NodeType.CommandStatement,
                        new Token(TokenType.NotDefined, null, currentToken.location),
                        new Node[] { cmd, param }
                    );
                    break;

                default:
                    factor = Empty();
                    break;
            }
            return factor;
        }

        // Term : Factor ((Multiplication|Division) Factor)*
        private Node Term()
        {
            Node factor = Factor();

            while (currentToken.type == TokenType.Multiplication ||
                currentToken.type == TokenType.Division)
            {
                Token opr = new Token(currentToken);

                switch (currentToken.type)
                {
                    case TokenType.Multiplication:
                        Match(TokenType.Multiplication);
                        factor = new Node(
                            NodeType.BinaryOperation,
                            opr,
                            new Node[] { factor, Factor() });
                        break;

                    case TokenType.Division:
                        Match(TokenType.Division);
                        factor = new Node(
                            NodeType.BinaryOperation,
                            opr,
                            new Node[] { factor, Factor() });
                        break;
                }
            }
            return factor;
        }

        // Expression : Term ((Addition|Subtraction) Term)*
        private Node Expression()
        {
            Node term = Term();

            while (currentToken.type == TokenType.Addition ||
                currentToken.type == TokenType.Subtraction)
            {
                Token opr = new Token(currentToken);

                switch (currentToken.type)
                {
                    case TokenType.Addition:
                        Match(TokenType.Addition);
                        term = new Node(
                            NodeType.BinaryOperation,
                            opr,
                            new Node[] { term, Term() });
                        break;

                    case TokenType.Subtraction:
                        Match(TokenType.Subtraction);
                        term = new Node(
                            NodeType.BinaryOperation,
                            opr,
                            new Node[] { term, Term() });
                        break;
                }
            }
            return term;
        }

        // StringElement : String | Varname | Expression
        private Node StringElement()
        {
            Token stringElement = new Token(currentToken);

            switch (currentToken.type)
            {
                case TokenType.String:
                    Match(TokenType.String);
                    return new Node(NodeType.Value, stringElement, null);

                default:
                    return Expression();
            }
        }

        // StringOperation : StringElement (Ampersand StringElement)*
        private Node StringOperation()
        {
            Node stringElement = StringElement();

            while (currentToken.type == TokenType.Ampersand)
            {
                Token stringOperation = new Token(currentToken);
                Match(TokenType.Ampersand);

                stringElement = new Node(
                    NodeType.StringOperation,
                    stringOperation,
                    new Node[] { stringElement, StringElement() });
            }

            return stringElement;
        }

        // AssignmentStatement : Varname Assignment StringExpression
        //                     | Varname AssAdd     StringExpression
        //                     | VarName AssSub     StringExpression
        //                     | VarName AssMul     StringExpression
        //                     | VarName AssDiv     StringExpression
        //                     | VarName AssAmp     StringExpression
        //                     | VarName Increment
        //                     | VarName Decrement
        private Node AssignmentStatement()
        {
            Node varName = new Node(NodeType.Value, currentToken, null);
            Match(TokenType.Varname);

            Token assOpr = new Token(currentToken);

            switch (assOpr.type)
            {
                case TokenType.Assignment:
                    Match(TokenType.Assignment);
                    varName = new Node(
                        NodeType.AssignmentOperation, 
                        assOpr, 
                        new Node[] { varName, StringOperation() });
                    break;

                case TokenType.AssAdd:
                    Match(TokenType.AssAdd);
                    varName = new Node(
                        NodeType.AssignmentOperation, 
                        assOpr, 
                        new Node[] { varName, StringOperation() });
                    break;

                case TokenType.AssSub:
                    Match(TokenType.AssSub);
                    varName = new Node(
                        NodeType.AssignmentOperation, 
                        assOpr, 
                        new Node[] { varName, StringOperation() });
                    break;

                case TokenType.AssMul:
                    Match(TokenType.AssMul);
                    varName = new Node(
                        NodeType.AssignmentOperation, 
                        assOpr, 
                        new Node[] { varName, StringOperation() });
                    break;

                case TokenType.AssDiv:
                    Match(TokenType.AssDiv);
                    varName = new Node(
                        NodeType.AssignmentOperation, 
                        assOpr, 
                        new Node[] { varName, StringOperation() });
                    break;

                case TokenType.AssAmp:
                    Match(TokenType.AssAmp);
                    varName = new Node(
                        NodeType.AssignmentOperation, 
                        assOpr, 
                        new Node[] { varName, StringOperation() });
                    break;

                case TokenType.Increment:
                    Match(TokenType.Increment);
                    return new Node(
                        NodeType.IncrementOperation, 
                        assOpr, 
                        new Node[] { varName });

                case TokenType.Decrement:
                    Match(TokenType.Decrement);
                    return new Node(
                        NodeType.DecrementOperation, 
                        assOpr, 
                        new Node[] { varName });

                default:
                    Error.Throw(
                        ErrorOrigin.Parser,
                        "Illegal operator " + currentToken.type + ". " +
                        "Supported assignment operators: " +
                        "++, --, +=, -=, *=, /=, &=, and =",
                        currentToken.location);
                    break;
            }
            return varName;
        }

        // Command : ConsoleCommand | CustomCommand
        private Node Command()
        {
            Token command = new Token(currentToken);

            switch (currentToken.type)
            {
                case TokenType.CustomCommand:
                    Match(TokenType.CustomCommand);
                    return new Node(NodeType.CustomCommand, command, null);

                case TokenType.ConsoleCommand:
                    Match(TokenType.ConsoleCommand);
                    return new Node(NodeType.ConsoleCommand, command, null);

                default:
                    Error.Throw(
                        ErrorOrigin.Parser,
                        "Command statement needs to start with: " +
                        "(console), or @ (custom) flags",
                        currentToken.location);
                    return null;
            }
        }

        // Parameter : (StringExpression)*
        private Node Parameters()
        {
            List<Node> children = new List<Node>();

            children.Add(StringOperation());

            while (currentToken.type == TokenType.Comma)
            {
                Match(TokenType.Comma);
                children.Add(StringOperation());
            }

            return new Node(
                NodeType.Parameters,
                new Token(TokenType.NotDefined,null, currentToken.location),
                children.ToArray());
        }

        // CommandStatement : Command Parameter
        private Node CommandStatement()
        {
            Node command = Command();

            Node parameters = Parameters();

            return new Node(
                NodeType.CommandStatement, 
                new Token(TokenType.NotDefined, null, currentToken.location), 
                new Node[] { command, parameters });
        }

        private Node Empty()
        {
            return new Node(
                NodeType.Empty, 
                new Token(TokenType.NotDefined, null, currentToken.location), 
                null);
        }

        // Statement : AssignmentStatement | CommandStatement | Empty
        private Node Statement()
        {
            switch (currentToken.type)
            {
                case TokenType.Varname:
                    return AssignmentStatement();

                case TokenType.CustomCommand:
                case TokenType.ConsoleCommand:
                    return CommandStatement();

                default:
                    return Empty();
            }
        }

        // StatementList : Statement (SemiColon Statement)*
        private Node StatementList()
        {
            List<Node> statements = new List<Node>();

            statements.Add(Statement());

            while (currentToken.type == TokenType.Semicolon)
            {
                Match(TokenType.Semicolon);
                statements.Add(Statement());
            }

            return new Node(
                NodeType.StatementList, 
                new Token(TokenType.NotDefined, "", currentToken.location), 
                statements.ToArray());
        }

        public Node Parse()
        {
            return StatementList();
        }
    }
}
