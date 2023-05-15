using Panama.Security.Interfaces;

namespace Panama.Security
{
    public class Base10Encryptor : IBase10Encryptor
    {
        private const string _list = "0123456789abcdefghijklmnopqrstuvwxyz";

        public string ToString(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("input", value, "input cannot be negative");

            char[] clistarr = _list.ToCharArray();
            var result = new Stack<char>();
            while (value != 0)
            {
                result.Push(clistarr[value % 36]);
                value /= 36;
            }
            return new string(result.ToArray());
        }
        public long FromString(string encrypted)
        {
            var reversed = encrypted.ToLower().Reverse();
            long result = 0;
            int pos = 0;
            foreach (char c in reversed)
            {
                result += _list.IndexOf(c) * (long)Math.Pow(36, pos);
                pos++;
            }
            return result;
        }
    }
}
