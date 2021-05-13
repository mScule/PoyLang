using System.Collections.Generic;

namespace PoyLang
{
    public class Interpreter
    {
        private Parser parser;
        private Node tree;

        private Dictionary<string, Variable> varList;
        private Dictionary<string, CustomCommand> customCommands;

        private string docPath = @"Assets\Garudec\Poy\Doc\";
        private string libPath = @"Assets\Garudec\Libraries\";

        private string mainOut; // Main output string, that output shows
        private string secondaryOut; // Secondary output string, that contains input info

        public Interpreter
        (
            Parser parser,
            Dictionary<string, Variable> varList,
            Dictionary<string, CustomCommand> customCommands
        )
        {
            this.parser = parser;
            tree = this.parser.Parse();

            this.varList = varList;
            this.customCommands = customCommands;
        }

        // Helper methods
        private float Float(Variable number, Location location)
        {
            if (number.type == VariableType.Float)
            {
                string floatNum = "";

                foreach (char c in number.value)
                {
                    if (c == '.')
                        floatNum += ',';
                    else
                        floatNum += c;
                }

                return float.Parse(floatNum) + 0.0f;
            }

            else
            {
                Error.Throw(ErrorOrigin.Interpreter, "variable type needs to be number", location);
                return 0.0f;
            }
        }

        private float Float(string number, Location location)
        {
            string floatNum = "";

            foreach (char c in number)
            {
                if (c == '.')
                    floatNum += ',';
                else
                    floatNum += c;
            }

            return float.Parse(floatNum) + 0.0f;
        }

        private bool IsNumber(string input)
        {
            if (input != null)
            {
                foreach (char c in input)
                {
                    if (c == '.' || c == ',' || c >= '0' && c <= '9')
                        continue;
                    else
                        return false;
                }
                return true;
            }
            return false;
        }

        private Variable TryGetVariable(string name, Location location)
        {
            if (varList.ContainsKey(name))
                return varList[name];
            else
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    "Variable \"" + name + "\" doesn't exist",
                    location
                );
            return null;
        }

        // Visit methods
        private string StatementList(Node node)
        {
            string outputStream = "";

            foreach (Node child in node.children)
                outputStream += Visit(child);

            return outputStream;
        }

        private string Value(Node node)
        {
            switch (node.content.type)
            {
                case TokenType.Number:
                case TokenType.String:
                    return node.content.value;

                case TokenType.Varname:
                    return TryGetVariable(node.content.value, node.content.location).value;
            }
            return "";
        }

        private string StringOperation(Node node)
        {
            int strElement1 = 0, strElement2 = 1;
            return Visit(node.children[strElement1]) + Visit(node.children[strElement2]);
        }

        private string BinaryOperation(Node node)
        {
            int value1 = 0, value2 = 1;

            switch (node.content.type)
            {
                case TokenType.Addition:
                    return
                        Float(Visit(node.children[value1]), node.children[value1].content.location)
                        +
                        Float(Visit(node.children[value2]), node.children[value2].content.location) + "";

                case TokenType.Subtraction:
                    return
                        Float(Visit(node.children[value1]), node.children[value1].content.location)
                        -
                        Float(Visit(node.children[value2]), node.children[value2].content.location) + "";

                case TokenType.Multiplication:
                    return
                        Float(Visit(node.children[value1]), node.children[value1].content.location)
                        *
                        Float(Visit(node.children[value2]), node.children[value2].content.location) + "";

                case TokenType.Division:
                    return
                        Float(Visit(node.children[value1]), node.children[value1].content.location)
                        /
                        Float(Visit(node.children[value2]), node.children[value2].content.location) + "";
            }

            return "";
        }

        private string UnaryOperation(Node node)
        {
            int value = 0;

            switch (node.content.type)
            {
                case TokenType.Addition:
                    return +Float(Visit(node.children[value]), node.children[value].content.location) + "";

                case TokenType.Subtraction:
                    return -Float(Visit(node.children[value]), node.children[value].content.location) + "";
            }

            return "";
        }

        private string CommandStatement(Node node)
        {
            int command = 0;

            switch (node.children[command].type)
            {
                case NodeType.ConsoleCommand:

                    switch (node.children[command].content.value)
                    {
                        // Core commands
                        case "OUT":
                            Out(node);
                            break;

                        case "EDIT":
                            Edit(node);
                            break;

                        case "WHEN":
                            return When(node);

                        case "RUN":
                            return Run(node);

                        case "TIMES":
                            return Times(node);

                        case "LIBRARY":
                            return Library(node);

                        case "TASK":
                            return Task(node);

                        case "DELETE":
                            Delete(node);
                            break;

                        case "DELETE_ALL":
                            DeleteAll();
                            break;

                        // Listing commands
                        case "LIST_VARIABLES":
                            return ListVariables();

                        case "LIST_LIBRARIES":
                            string[] libs = System.IO.Directory.GetFileSystemEntries(libPath,"*.mcl");
                            mainOut += "\n$Y**LIBRARIES**$W\n\n";

                            foreach (string lib in libs)
                                mainOut += "$M*$W " + lib.Remove(0, libPath.Length) + "\n\n";
                            
                            break;

                        case "LIST_CONSOLE_COMMANDS":
                            mainOut += ReadFile("list_console_commands.txt");
                            break;

                        case "LIST_CUSTOM_COMMANDS":
                            mainOut += ListCustomCommands();
                            break;

                        // Help and documentation commands
                        case "HELP":
                            mainOut += ReadFile("help.txt");
                            break;

                        case "DOC_GENERAL":
                            mainOut += ReadFile("general.txt");
                            break;

                        case "DOC_COMMANDS":
                            mainOut += ReadFile("commands.txt");
                            break;

                        case "DOC_CONSOLE_COMMANDS":
                            mainOut += ReadFile("console_commands.txt");
                            break;

                        case "DOC_CUSTOM_COMMANDS":
                            DocCustomCommands();
                            break;

                        case "DOC_VARIABLES":
                            mainOut += ReadFile("variables.txt");
                            break;

                        case "DOC_DATATYPES":
                            mainOut += ReadFile("datatypes.txt");
                            break;

                        case "DOC_OPERATIONS":
                            mainOut += ReadFile("operations.txt");
                            break;

                        case "DOC_OUTPUT":
                            mainOut += ReadFile("output.txt");
                            break;

                        default:
                            Error.Throw(
                                ErrorOrigin.Interpreter,
                                "Console command " + node.children[command].content.value + " is not supported, " +
                                "see list of supported commands with :list_console_commands;", node.content.location);
                            break;
                    }
                    break;

                case NodeType.CustomCommand:
                    return CustomCommand(node);
            }

            return "";
        }

        private void AssignmentStatement(Node node)
        {
            int id = 0, value = 1;

            // Number assignment statements
            void NumberAssignment() // =
            {
                VariableType type;
                string givenValue = Visit(node.children[value]);

                if (IsNumber(givenValue))
                    type = VariableType.Float;
                else
                    type = VariableType.String;

                varList[node.children[id].content.value].value = givenValue;
                varList[node.children[id].content.value].type = type;
            }

            void NumberAssignmentAmpersand() // &=
            {
                varList[node.children[id].content.value].value =
                    varList[node.children[id].content.value].value + Visit(node.children[value]);

                varList[node.children[id].content.value].type = VariableType.String;
            }

            void NumberAssignmentAddition() // +=
            {
                varList[node.children[id].content.value].value =
                    Float(varList[node.children[id].content.value].value, node.children[id].content.location)
                    +
                    Float(Visit(node.children[value]), node.children[value].content.location) + "";
            }

            void NumberAssignmentSubtraction() // -=
            {
                varList[node.children[id].content.value].value =
                    Float(varList[node.children[id].content.value].value, node.children[id].content.location)
                    -
                    Float(Visit(node.children[value]), node.children[value].content.location) + "";
            }

            void NumberAssignmentMultiplication() // *=
            {
                varList[node.children[id].content.value].value =
                    Float(varList[node.children[id].content.value].value, node.children[id].content.location)
                    *
                    Float(Visit(node.children[value]), node.children[value].content.location) + "";
            }

            void NumberAssignmentDivision() // /= 
            {
                varList[node.children[id].content.value].value =
                    Float(varList[node.children[id].content.value].value, node.children[id].content.location)
                    /
                    Float(Visit(node.children[value]), node.children[value].content.location) + "";
            }

            // String assignment statements

            void StringAssignment() // =
            {
                VariableType type;
                string givenValue = Visit(node.children[value]);

                if (IsNumber(givenValue))
                    type = VariableType.Float;
                else
                    type = VariableType.String;

                varList[node.children[id].content.value].value = givenValue;
                varList[node.children[id].content.value].type = type;
            }

            void StringAssignmentAmpersand() // &=
            {
                varList[node.children[id].content.value].value =
                    varList[node.children[id].content.value].value + Visit(node.children[value]);

                varList[node.children[id].content.value].type = VariableType.String;
            }

            // If variable already exists
            if (varList.ContainsKey(node.children[id].content.value))
            {
                switch (varList[node.children[id].content.value].type)
                {
                    case VariableType.Float:

                        switch (node.content.type)
                        {
                            case TokenType.Assignment:
                                NumberAssignment();
                                break;

                            case TokenType.AssAmp:
                                NumberAssignmentAmpersand();
                                break;

                            case TokenType.AssAdd:
                                NumberAssignmentAddition();
                                break;

                            case TokenType.AssSub:
                                NumberAssignmentSubtraction();
                                break;

                            case TokenType.AssMul:
                                NumberAssignmentMultiplication();
                                break;

                            case TokenType.AssDiv:
                                NumberAssignmentDivision();
                                break;
                        }
                        break;

                    case VariableType.String:

                        switch (node.content.type)
                        {
                            case TokenType.Assignment:
                                StringAssignment();
                                break;

                            case TokenType.AssAmp:
                                StringAssignmentAmpersand();
                                break;
                        }
                        break;
                }
            }

            else // Declares new variable
            {
                switch (node.content.type)
                {
                    case TokenType.Assignment:
                        VariableType type;

                        string givenValue = Visit(node.children[value]);

                        if (IsNumber(givenValue))
                            type = VariableType.Float;
                        else
                            type = VariableType.String;

                        varList.Add
                            (node.children[id].content.value,
                            new Variable(type, givenValue));

                        break;

                    default:
                        Error.Throw
                            (ErrorOrigin.Interpreter,
                            "You must init the variable with some value" +
                            "in order to use other types of assignment operators",
                            node.content.location);
                        break;
                }
            }
        }

        private void IncrementOperation(Node node)
        {
            int id = 0;

            TryGetVariable(node.children[id].content.value, node.children[id].content.location).value =
                Float(varList[node.children[id].content.value], node.children[id].content.location) + 1 + "";
        }

        private void DecrementOperation(Node node)
        {
            int id = 0;

            TryGetVariable(node.children[id].content.value, node.children[id].content.location).value =
                Float(varList[node.children[id].content.value], node.children[id].content.location) - 1 + "";
        }

        // Console commands (Core commands)
        private string When(Node node)
        {
            int parameters = 1;

            string a = Visit(node.children[parameters].children[0]);
            string comparisonOperator = Visit(node.children[parameters].children[1]);
            string b = Visit(node.children[parameters].children[2]);

            string mainCode = Visit(node.children[parameters].children[3]);

            string secondaryCode = "";

            if (node.children[parameters].children.Length > 4)
                secondaryCode = Visit(node.children[parameters].children[4]);

            bool runCode = false;

            switch (comparisonOperator)
            {
                case "==":
                    if (a == b)
                        runCode = true;
                    break;

                case "<":
                    if (Float(a, node.content.location) < Float(b, node.content.location))
                        runCode = true;
                    break;

                case ">":
                    if (Float(a, node.content.location) > Float(b, node.content.location))
                        runCode = true;
                    break;

                case "<=":
                    if (Float(a, node.content.location) <= Float(b, node.content.location))
                        runCode = true;
                    break;

                case ">=":
                    if (Float(a, node.content.location) >= Float(b, node.content.location))
                        runCode = true;
                    break;

                case "!=":
                    if (a != b)
                        runCode = true;
                    break;
            }

            // Condition is true. Run given code
            if (runCode)
            {
                Tokenizer tokenizer = new Tokenizer(mainCode);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                string result = interpreter.Interprete()[0];
                mainOut += result;
                return result;
            }

            // Condition is false. Run secondary code if it exists
            else if (!runCode && secondaryCode != "")
            {
                Tokenizer tokenizer = new Tokenizer(secondaryCode);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                string result = interpreter.Interprete()[0];
                mainOut += result;
                return result;
            }
            return "";
        }

        private string Run(Node node)
        {
            int parameters = 1;

            string runOut = "";

            foreach (Node parameter in node.children[parameters].children)
            {
                string input = Visit(parameter);

                Tokenizer tokenizer = new Tokenizer(input);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                runOut += interpreter.Interprete()[0];
            }

            mainOut += runOut;
            return runOut;
        }

        private string Times(Node node)
        {
            int parameters = 1;

            int times = (int)

                Float
                (
                    Visit(node.children[parameters].children[0]),
                    node.children[parameters].children[0].content.location
                );

            string loopOutput = "";
            string loopInput = Visit(node.children[parameters].children[1]);

            if (times < 0)
                times -= -times;

            while (times >= 0)
            {
                Tokenizer tokenizer = new Tokenizer(loopInput);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                loopOutput += interpreter.Interprete()[0];
                times--;
            }
            mainOut += loopOutput;
            return loopOutput;
        }

        private void Out(Node node)
        {
            foreach (Node parameter in node.children[1].children)
                mainOut += Visit(parameter);
        }

        private void Edit(Node node)
        {
            int parameters = 1;

            foreach (Node variable in node.children[parameters].children)
            {
                secondaryOut +=
                    "\n\n#" + variable.content.value + " = {\n" +
                    TryGetVariable(variable.content.value, variable.content.location).value + "\n};\n";
            }
        }

        private string Library(Node node)
        {
            int parameters = 1;

            string libraries = "";

            int i = 0;

            try
            {
                foreach (Node path in node.children[parameters].children)
                {
                    string[] file = System.IO.File.ReadAllLines(libPath + path.content.value);

                    foreach (string line in file)
                        libraries += line + '\n';

                    i++;
                }
            }

            catch
            {
                Error.Throw
                (
                    ErrorOrigin.Interpreter,
                    "path " + node.children[parameters].children[i].content.value + " doesn't exist",
                    node.children[parameters].children[i].content.location
                );
            }

            Tokenizer tokenizer = new Tokenizer(libraries);
            Parser parser = new Parser(tokenizer);
            Interpreter interpreter = new Interpreter(parser, varList, customCommands);

            string[] result = interpreter.Interprete();

            mainOut = result[0];
            secondaryOut = result[1];

            return result[0];
        }

        private string Task(Node node)
        {
            const char parameterPlaceHolder = '%';

            int parameters = 1;

            string varValue = TryGetVariable
            (
                node.children[parameters].children[0].content.value,
                node.children[parameters].children[0].content.location
            ).value;

            string subProcess = "";
            bool escape = false;
            int i = 1;

            foreach(char c in varValue)
            {
                if (escape)
                {
                    switch(c)
                    {
                        case '\\':
                            subProcess += '\\';
                            break;
                        case parameterPlaceHolder:
                            subProcess += parameterPlaceHolder;
                            break;
                    }
                    escape = false;
                }
                if (c == '\\')
                {
                    escape = true;
                    continue;
                }
                if (c == parameterPlaceHolder)
                {
                    subProcess += '{' + Visit(node.children[parameters].children[i]) + '}';
                    i++;
                    continue;
                }

                else
                    subProcess += c;
            }

            // Interpreting sub process
            Dictionary<string, Variable> closure = new Dictionary<string, Variable>();

            foreach(KeyValuePair<string, Variable> variable in varList)
                closure.Add(variable.Key, variable.Value);

            Tokenizer tokenizer = new Tokenizer(subProcess);
            Parser parser = new Parser(tokenizer);
            Interpreter interpreter = new Interpreter(parser, closure, customCommands);

            string[] iteration = interpreter.Interprete();

            return iteration[0];
        }

        private void Delete(Node node)
        {
            int parameters = 1;

            foreach (Node variable in node.children[parameters].children)
                varList.Remove(variable.content.value);
        }

        private void DeleteAll()
        {
            List<string> varNames = new List<string>();

            foreach (KeyValuePair<string, Variable> variable in varList)
                varNames.Add(variable.Key);

            foreach (string varName in varNames)
                varList.Remove(varName);
        }

        // Console commands (Listing commands)
        private string ListVariables()
        {
            string variableList = "";

            if (varList.Count != 0)
                foreach (KeyValuePair<string, Variable> variable in varList)
                    variableList += variable.Key + " | " + variable.Value.type.ToString() + 
                        " (" + variable.Value.value + ")\n\n";
            else
                variableList = "empty";

            mainOut += variableList;
            return variableList;
        }

        private string ListCustomCommands()
        {
            string customCommandList = "\n$Y**LIST OF CUSTOM COMMANDS**$W\n\n";

            if (customCommands.Count > 0)
                foreach (KeyValuePair<string, CustomCommand> customCommand in customCommands)
                    customCommandList += "$R*$W " + customCommand.Key + "\n\n";
            else
                customCommandList += "none";

            return customCommandList;
        }

        // Console commands (Help and documetation commands)
        private void DocCustomCommands()
        {
            string customCommandDoc = "\n$Y**CUSTOM COMMANDS**$W\n\n";

            if (customCommands.Count != 0)
                foreach (KeyValuePair<string, CustomCommand> command in customCommands)
                    customCommandDoc += "$M" + command.Key + "$W" + command.Value.description + "\n\n";
            else
                customCommandDoc += "$Rnone$W";

            mainOut += customCommandDoc + "\nPrevious $M:doc_console_commands$W; Next $M:doc_variables$W;" + "\n";
        }

        // Custom command related
        private string CustomCommand(Node node)
        {
            int command = 0, parameters = 1;

            // CustomCommand List contains command with given key
            if (customCommands.ContainsKey(node.children[command].content.value))
            {
                List<string> visitedChildren = new List<string>();

                // Parameters given
                if (node.children[parameters] != null)
                {
                    foreach (Node child in node.children[parameters].children)
                        visitedChildren.Add(Visit(child));

                    return customCommands[node.children[command].content.value].Command(visitedChildren.ToArray());
                }
            }

            // CustomCommand List doesn't contain command with given key
            else
                Error.Throw(
                    ErrorOrigin.Interpreter,
                    "Custom command " + node.children[command].content.value + " is not supported, " +
                    "see list of supported custom commands with :list_custom_commands;", node.content.location);

            return "";
        }

        private string ReadFile(string path) { return System.IO.File.ReadAllText(docPath + path); }

        // Reads through AST
        private string Visit(Node node)
        {
            switch (node.type)
            {
                case NodeType.Value:
                    return Value(node);

                case NodeType.CommandStatement:
                    return CommandStatement(node);

                case NodeType.AssignmentOperation:
                    AssignmentStatement(node);
                    break;

                case NodeType.BinaryOperation:
                    return BinaryOperation(node);

                case NodeType.UnaryOperation:
                    return UnaryOperation(node);

                case NodeType.StringOperation:
                    return StringOperation(node);

                case NodeType.IncrementOperation:
                    IncrementOperation(node);
                    break;

                case NodeType.DecrementOperation:
                    DecrementOperation(node);
                    break;

                case NodeType.StatementList:
                    return StatementList(node);

                case NodeType.Empty:
                    return "";
            }
            return null;
        }

        public string[] Interprete()
        {
            Visit(tree);
            return new string[] { mainOut, secondaryOut };
        }
    }
}
