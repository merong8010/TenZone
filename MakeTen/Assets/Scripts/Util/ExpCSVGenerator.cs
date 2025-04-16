using System.Text;
using System.IO;
using UnityEngine;

public class ExpCSVGenerator : MonoBehaviour
{
    public int maxLevel = 100;

    void Start()
    {
        string csv = GenerateCSV(maxLevel);
        string path = Application.dataPath + "/ExpTable.csv";
        File.WriteAllText(path, csv, Encoding.UTF8);
        Debug.Log($"✅ 경험치 테이블 CSV 생성 완료!\n→ {path}");
    }

    string GenerateCSV(int maxLevel)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Level,RequiredExp,CumulativeExp");

        int total = 0;
        for (int level = 1; level <= maxLevel; level++)
        {
            int required = GetHybridExp(level);
            total += required;
            sb.AppendLine($"{level},{required},{total}");
        }

        return sb.ToString();
    }

    int GetHybridExp(int level)
    {
        if (level <= 10)
            return 100 * level;
        else if (level <= 50)
            return Mathf.FloorToInt(1000 + Mathf.Pow(level - 10, 2.2f));
        else
            return Mathf.FloorToInt(5000 + 50 * Mathf.Pow(level - 50, 2.8f));
    }
}
