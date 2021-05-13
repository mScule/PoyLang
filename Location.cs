namespace PoyLang
{
    public class Location
    {
        public int line { get; }
        public int character { get; }

        public Location(int line, int character)
        {
            this.line = line;
            this.character = character;
        }

        public Location(Location location)
        {
            this.line = location.line;
            this.character = location.character;
        }

        public override string ToString()
        {
            return Print.Item("Location", Print.SubContent(new string[] { "ln " + line, "ch " + character }));
        }
    }
}
