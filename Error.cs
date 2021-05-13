using System;

namespace PoyLang
{
    public enum ErrorOrigin { Tokenizer, Parser, Interpreter }

    public static class Error
    {
        public static void Throw(ErrorOrigin origin, String message, Location location)
        {
            throw new Exception(
                Print.Alert(
                    "Error",
                    Print.SubContent(new string[] {
                        Print.Message( origin.ToString(), message ),
                        location.ToString() }
                    )
                )
            );
        }
    }
}
