// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("21hWWWnbWFNb21hYWf4FA87Q5X2fOSNJ8i2aUZcPhswTgza1fKE0Y3YYHsP0b8qqmL5/IocKO+PnZsTzm+gKD5Cistkt5bnIqy+shbP9IJxp21h7aVRfUHPfEd+uVFhYWFxZWlfU3JW/D5SGo5BvW0VTJ/oUCkymacDRpmltzcrGWdk5XleIyzAqMpdcyrFPE88dxi76ITwrNMn9m5kKLya8venbIfeG4mIRAzBSNzU8FGMk7pc34JolYX/ZK/mNXCdaJeODbw4tZs19fvRL2H0JBMMHCw1igifk3cQfKZoxlPGg7U43J/lm2qqiT1z/BUzupPrQqnbWStkYm7rqOAPW6A0TKd/G5kGFU3Ok1VgrRwX+mGo0MJtnFYeOEF/oJFtaWFlY");
        private static int[] order = new int[] { 1,7,12,13,7,7,7,12,9,9,12,12,13,13,14 };
        private static int key = 89;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
