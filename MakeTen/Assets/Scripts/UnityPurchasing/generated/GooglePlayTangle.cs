// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("RMfPhqwch5Wwg3xIVkA06QcZX7V6yEtoekdMQ2DMAsy9R0tLS09KST513m5t51jLbhoX0BQYHnGRNPfONa+u+sgy5JXxcQIQI0EkJi8HcDf9hCTziTZybMo46p5PNEk28JB8HRZf/bfpw7llxVnKC4ip+SsQxfsejCowWuE+iUKEHJXfAJAlpm+yJ3AAOszV9VKWQGC3xks4VBbti3knI3rTwrV6ft7Z1UrKKk1Em9gjOSGEyEtFSnrIS0BIyEtLSu0WEN3D9m6I+xkcg7Ghyj72qtu4PL+WoO4zj0/ZolwA3A7VPekyLzgn2u6Iihk81ww6iSKH4rP+XSQ06nXJubFcT+xlCw3Q53zZuYutbDGUGSjw9HXX4Ih0BpSdA0z7N0hJS0pL");
        private static int[] order = new int[] { 4,4,6,9,8,9,7,11,9,9,13,11,13,13,14 };
        private static int key = 74;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
