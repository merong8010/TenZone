// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("zU5GDyWVDhw5CvXB38m9YI6Q1jzzQcLh887FyulFi0U0zsLCwsbDwMZQK9WJVYdctGC7prGuU2cBA5C1ibNFXHzbH8npPk/Csd2fZALwrqpehbMAqw5rOnfUrb1j/EAwONXGZbf8V+fkbtFC55OeWZ2Rl/gYvX5HAXKQlQo4KEO3fyNSMbU2HylnugZ0Da16AL/75UOxYxfGvcC/eRn1lOyChFlu9VAwAiTluB2QoXl9/F5pQcLMw/NBwsnBQcLCw2SfmVRKf+ef1nQ+YEow7EzQQ4IBIHCimUxyl/NaSzzz91dQXMNDo8TNElGqsKgNBaO502i3AMsNlRxWiRmsL+Y7rvm8JidzQbttHHj4i5mqyK2vpo75vgH9jx0UisVyvsHAwsPC");
        private static int[] order = new int[] { 4,4,2,11,10,6,13,8,12,10,11,13,12,13,14 };
        private static int key = 195;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
