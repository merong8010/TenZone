// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("CYqEi7sJioGJCYqKiyzX0RwCN68WzftI40Yjcj+c5fUrtAh4cJ2OLf+0H6+sJpkKr9vWEdXZ37BQ9TYPuwmKqbuGjYKhDcMNfIaKioqOi4j0bm87CfMlVDCww9HigOXn7sax9teePHYoAnikBJgLykloOOrRBDrfwfsNFDSTV4GhdgeK+ZXXLEq45uI8ReUySPezrQv5K1+O9Yj3MVG93Ek62N1CcGAL/zdrGnn9fldhL/JOTevxmyD/SINF3VQewVHkZ65z5rGOGGOdwR3PFPwo8+755hsvSUvY/aTKzBEmvRh4Smyt8FXY6TE1tBYhhQYOR23dRlRxQr2Jl4H1KMbYnnS7EgN0u78fGBSLC+uMhVoZ4vjgRUm1x1Vcwo069omIiouK");
        private static int[] order = new int[] { 1,10,6,10,9,10,11,8,13,13,11,12,13,13,14 };
        private static int key = 139;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
