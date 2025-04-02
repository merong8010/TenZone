using System;
using System.Collections.Generic;
using UnityEngine;

public class NormalDistributionGraph : MonoBehaviour
{
    private Dictionary<int, int> histogram = new Dictionary<int, int>();
    private int sampleSize = 1000;
    private int minValue = 0;
    private int maxValue = 10;
    private int barWidth = 40; // 막대 너비
    private int maxBarHeight = 200; // 최대 막대 높이

    void Start()
    {
        GenerateHistogram();
    }

    void GenerateHistogram()
    {
        System.Random random = new System.Random();
        double mean = 5.0;
        double stdDev = 2.0;

        // 초기화
        histogram.Clear();
        for (int i = minValue; i <= maxValue; i++)
        {
            histogram[i] = 0;
        }

        // 난수 생성 및 히스토그램 업데이트
        for (int i = 0; i < sampleSize; i++)
        {
            int value = GenerateNormalRandomInt(random, mean, stdDev);
            if (value >= minValue && value <= maxValue)
            {
                histogram[value]++;
            }
        }
    }

    int GenerateNormalRandomInt(System.Random rand, double mean, double stdDev)
    {
        // Box-Muller 변환을 사용하여 정규분포 난수 생성
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        double normalValue = mean + stdDev * randStdNormal;

        return (int)Math.Round(normalValue);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), "정규분포를 따르는 난수 히스토그램");

        int xOffset = 50; // 그래프의 X 위치 조정
        int yOffset = 250; // 그래프의 Y 위치 (높이 조정)

        int maxCount = 1;
        foreach (var count in histogram.Values)
        {
            if (count > maxCount) maxCount = count;
        }

        int barIndex = 0;
        foreach (var kvp in histogram)
        {
            int xPosition = xOffset + (barIndex * (barWidth + 5));
            int barHeight = (int)((float)kvp.Value / maxCount * maxBarHeight); // 비율 맞추기
            int yPosition = yOffset - barHeight;

            GUI.Box(new Rect(xPosition, yPosition, barWidth, barHeight), "");
            GUI.Label(new Rect(xPosition, yOffset + 5, barWidth, 20), kvp.Key.ToString()); // X축 값 표시
            barIndex++;
        }
    }
}
