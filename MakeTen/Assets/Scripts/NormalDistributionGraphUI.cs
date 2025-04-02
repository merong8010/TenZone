using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NormalDistributionGraphUI : MonoBehaviour
{
    public GameObject barPrefab;
    public Transform graphContainer;
    public LineRenderer lineRenderer;
    public TMP_Text titleText;

    public Slider meanSlider;
    public Slider stdDevSlider;
    public TMP_Text meanValueText;
    public TMP_Text stdDevValueText;

    private Dictionary<int, int> histogram = new Dictionary<int, int>();
    private int sampleSize = 1000;
    private int minValue = 0;
    private int maxValue = 10;
    private int maxBarHeight = 300;
    private int curveResolution = 100;

    private Dictionary<int, RectTransform> barTransforms = new Dictionary<int, RectTransform>();

    void Start()
    {
        meanSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
        stdDevSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });

        OnSliderValueChanged();
    }

    void OnSliderValueChanged()
    {
        double mean = meanSlider.value;
        double stdDev = stdDevSlider.value;

        meanValueText.text = mean.ToString("F1");
        stdDevValueText.text = stdDev.ToString("F1");

        GenerateHistogram(mean, stdDev);
        DrawGraph();
        GenerateNormalCurve(mean, stdDev);
    }

    void GenerateHistogram(double mean, double stdDev)
    {
        System.Random random = new System.Random();
        histogram.Clear();

        for (int i = minValue; i <= maxValue; i++)
        {
            histogram[i] = 0;
        }

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
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        double normalValue = mean + stdDev * randStdNormal;

        return (int)Math.Round(normalValue);
    }

    void DrawGraph()
    {
        if (titleText != null)
        {
            titleText.text = "정규분포 곡선 + 히스토그램 (애니메이션 적용)";
        }

        int maxCount = 1;
        foreach (var count in histogram.Values)
        {
            if (count > maxCount) maxCount = count;
        }

        float barSpacing = 60f;
        int barIndex = 0;

        foreach (var kvp in histogram)
        {
            int value = kvp.Key;
            int count = kvp.Value;
            float heightRatio = (float)count / maxCount;
            float barHeight = heightRatio * maxBarHeight;

            if (!barTransforms.ContainsKey(value))
            {
                GameObject bar = Instantiate(barPrefab, graphContainer);
                RectTransform barTransform = bar.GetComponent<RectTransform>();
                barTransform.anchoredPosition = new Vector2(barIndex * barSpacing, 0);
                barTransform.sizeDelta = new Vector2(40, 0);
                barTransforms[value] = barTransform;
            }

            StartCoroutine(AnimateBarHeight(barTransforms[value], barHeight));

            barIndex++;
        }
    }

    IEnumerator AnimateBarHeight(RectTransform barTransform, float targetHeight)
    {
        float duration = 0.5f;
        float time = 0;
        float startHeight = barTransform.sizeDelta.y;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newHeight = Mathf.Lerp(startHeight, targetHeight, time / duration);
            barTransform.sizeDelta = new Vector2(40, newHeight);
            yield return null;
        }

        barTransform.sizeDelta = new Vector2(40, targetHeight);
    }

    void GenerateNormalCurve(double mean, double stdDev)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = curveResolution;
        float graphWidth = (maxValue - minValue) * 60f;
        float step = graphWidth / (curveResolution - 1);

        float maxY = 1f;
        Vector3[] curvePoints = new Vector3[curveResolution];

        for (int i = 0; i < curveResolution; i++)
        {
            float x = minValue + (i / (float)curveResolution) * (maxValue - minValue);
            float y = NormalDistribution(x, mean, stdDev);

            if (y > maxY) maxY = y;
        }

        for (int i = 0; i < curveResolution; i++)
        {
            float x = minValue + (i / (float)curveResolution) * (maxValue - minValue);
            float y = NormalDistribution(x, mean, stdDev) / maxY * maxBarHeight;

            curvePoints[i] = new Vector3((x - minValue) * 60f, y, 0);
        }

        StartCoroutine(AnimateCurve(curvePoints));
    }

    IEnumerator AnimateCurve(Vector3[] targetPoints)
    {
        float duration = 0.5f;
        float time = 0;
        Vector3[] startPoints = new Vector3[lineRenderer.positionCount];

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            startPoints[i] = lineRenderer.GetPosition(i);
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Vector3.Lerp(startPoints[i], targetPoints[i], time / duration));
            }
            yield return null;
        }

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, targetPoints[i]);
        }
    }

    float NormalDistribution(float x, double mean, double stdDev)
    {
        double exponent = -Math.Pow(x - mean, 2) / (2 * Math.Pow(stdDev, 2));
        return (float)(Math.Exp(exponent) / (stdDev * Math.Sqrt(2 * Math.PI)));
    }
}
