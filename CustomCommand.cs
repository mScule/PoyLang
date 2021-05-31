using UnityEngine;

namespace PoyLang
{
    // Use this class as parent class when creating a CustomCommand
    public class CustomCommand : MonoBehaviour
    {
        public string key { get; }

        public string description { get; }

        public CustomCommand(string key, string[] description)
        {

            this.key = key;
            this.description = "\n\n";

            foreach (string line in description)
                this.description += "  " + line + '\n';
        }

        public virtual string Command(string[] parameters) { return ""; }

        protected bool IsParamEmpty(string[] parameters, int index)
        {
            if (parameters[index] == null ||
                parameters[index] != null && parameters[index] == "")
                return true;
            return false;
        }

        protected void DemandParam(string[] parameters, int index, string errMsg)
        {
            if (parameters[index] != null && parameters[index] != "")
                return;
            Error.Throw(ErrorOrigin.CustomCommand, errMsg);
        }

        protected string Return(int num)
        {
            return num + "";
        }

        protected string Return(float num)
        {
            return num + "";
        }

        protected float Float(string str, string errMsg)
        {
            float result = 0.0f;

            if (float.TryParse(str, out result))
                return result;
            else
                Error.Throw(ErrorOrigin.CustomCommand, errMsg);
            return 0.0f;
        }
    }
}
