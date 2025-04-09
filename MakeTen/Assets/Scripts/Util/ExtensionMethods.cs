using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static bool IsInside(this UnityEngine.RectTransform rect, Transform target)
    {
        Vector3[] rectCorners = new Vector3[4];
        rect.GetWorldCorners(rectCorners); // 사각형의 월드 좌표 가져오기

        Vector3 targetPos = target.position; // 오브젝트의 월드 위치

        // 사각형 내부에 있는지 체크
        if (targetPos.x >= rectCorners[0].x && targetPos.x <= rectCorners[2].x &&
            targetPos.y >= rectCorners[0].y && targetPos.y <= rectCorners[2].y)
        {
            return true; // 포함됨
        }
        return false; // 포함되지 않음
    }

    public static int[] Shuffle(this int[] array)
    {
        System.Random rand = new System.Random(); // 난수 생성기

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rand.Next(0, i + 1); // 0부터 i까지 랜덤 인덱스 선택
            (array[i], array[j]) = (array[j], array[i]); // Swap
        }
        return array;
    }

    public static string ToTimeText(this int time)
    {
        return TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
    }

    public static string ToTimeText(this long time)
    {
        return time.LongToDateTime().ToString(@"yyyy-MM-dd H\:mm\:ss");
        //return TimeSpan.FromSeconds(time).ToString(@"yyyy-MM-dd H\:mm\:ss");
    }

    public static string ToTimeText(this DateTime time)
    {
        return time.ToString(@"yyyy-MM-dd H\:mm\:ss");
        //return TimeSpan.FromSeconds(time).ToString(@"yyyy-MM-dd H\:mm\:ss");
    }

    public static long ToTick(this DateTime dateTime)
    {
        return (dateTime.Ticks - 621355968000000000) / 10000000;
    }

    public static DateTime LongToDateTime(this long val)
    {
        return new DateTime(val * 10000000 + 621355968000000000);
    }

    public static string ToDateText(this DateTime dateTime)
    {
        return dateTime.ToString(@"yyyy-MM-dd");
    }
}

