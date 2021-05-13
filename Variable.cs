namespace PoyLang
{
    public enum VariableType { String, Float }

    public class Variable
    {
        public VariableType type { get; set; }
        public string value { get; set; }

        public Variable(VariableType type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }
}
