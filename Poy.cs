using System;
using System.Collections.Generic;

namespace PoyLang
{
    public class Poy 
    {
        private Dictionary<string, CustomCommand> customCommands;
        private Dictionary<string, Variable> varList;

        private string outputOut;
        private string outputIn;

        public Poy(Dictionary<string, CustomCommand> customCommands)
        {
            this.customCommands = customCommands;
            this.varList = new Dictionary<string, Variable>();

            outputOut =
                Print.Message(
                    Print.Value("Garudec", "Game Runtime Debug Console"),

                    Print.SubContent(
                    new string[] {
                        // Version name
                        Print.Value("Version", "0.0.1"),

                        // Welcome message
                        Print.Message("To get started", "write \"$M:help$W;\"") + "\n\n" +
                        Print.Message("To get command list", "write \"$M:doc_console_commands$W; & $M:doc_custom_commands$W;\"") + "\n\n" +
                        
                        // Patch notes
                        "First version of garudec, and poy. 0.0.1"
                       }
                )
            );
        }

        public void Interprete(string input)
        {
            try
            {
                Tokenizer tokenizer = new Tokenizer(input);
                Parser parser = new Parser(tokenizer);
                Interpreter interpreter = new Interpreter(parser, varList, customCommands);

                string[] output = interpreter.Interprete();

                outputOut = output[0];
                outputIn =  output[1];
            }

            catch (Exception e)
            {
                outputOut = "$R" + e.Message + "$W";
            }
        }

        public string GetOutputOut()
        {
            return outputOut;
        }

        public string GetOutputIn()
        {
            string output = outputIn;
            outputIn = null;
            return output;
        }

        public bool IsOutputInNull()
        {
            if (outputIn == null)
                return true;
            else
                return false;
        }
    }
}
