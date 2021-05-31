using System;
using System.Collections.Generic;

namespace PoyLang
{
    public class Poy
    {
        private Dictionary<string, CustomCommand> customCommands;
        private Dictionary<string, Variable> varList;

        private string outputOut, outputIn;

        public Poy(Dictionary<string, CustomCommand> customCommands)
        {
            this.customCommands = customCommands;
            this.varList = new Dictionary<string, Variable>();

            outputOut =
                Print.Message(
                    "Poy",
                    "Programming tOY\n" +

                    // Version
                    "Version: " + "0.0.2\n" +

                    // Welcome message
                    "To get started: write \"$M:help$W;\"\n" +
                    "To get list of avaliable commands write:\n" +
                    "\n$M:doc_console_commands$W; &" +
                    "\n$M:doc_custom_commands$W;" + "\n\n" +

                    // Patch notes
                    "Patch Notes\n" +
                    "+ Numbers should now work as intented\n" +
                    "+ Documentation is now up to date\n"
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
                outputIn = output[1];
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
