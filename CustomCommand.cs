using UnityEngine;

namespace PoyLang
{
    // Use this class as parent class when creating a CustomCommand
    public class CustomCommand : MonoBehaviour
    {
        public string key { get; }

        public string description { get; }

        public CustomCommand(string key, string[] description) {

            this.key = key;
            this.description = "\n\n";

            foreach (string line in description)
                this.description += "  " + line + '\n';
        }

        public virtual string Command(string[] parameters) { return ""; }
    }
}
