using UnityEngine;

public static class Util
{
    //private static float mean = 5f;
    //private static float stdDev = 3f;

    //public static int GetRandomTen(this Random rand)
    //{
    //    double u1 = 1.0 - rand.NextDouble();
    //    double u2 = 1.0 - rand.NextDouble();
    //    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    //    double normalValue = mean + stdDev * randStdNormal;

    //    return (int)Math.Round(normalValue);
    //}

    public static int GenerateGaussianRandom(float mean, float stdDev)
    {
        float u1 = 1.0f - UnityEngine.Random.value; // 0~1 사이 난수 (0 포함 X)
        float u2 = 1.0f - UnityEngine.Random.value;

        float randStdNormal = UnityEngine.Mathf.Sqrt(-2.0f * UnityEngine.Mathf.Log(u1)) * UnityEngine.Mathf.Sin(2.0f * UnityEngine.Mathf.PI * u2);
        float randNormal = mean + stdDev * randStdNormal;

        // 1~9 범위를 벗어나지 않도록 보정
        return UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(randNormal), 1, 9);
    }
}
