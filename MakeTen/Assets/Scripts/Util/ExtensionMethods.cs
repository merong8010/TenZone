using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

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

    public static bool IsInside(this RectTransform rect, RectTransform target)
    {
        //Vector3[] rectCorners = new Vector3[4];
        //rect.GetWorldCorners(rectCorners); // 사각형의 월드 좌표 가져오기

        //Vector3[] targetCorners = new Vector3[4];
        //target.GetWorldCorners(targetCorners); // 사각형의 월드 좌표 가져오기

        //if (IsContain(rectCorners, targetCorners[0]) || IsContain(rectCorners, targetCorners[1]) || IsContain(rectCorners, targetCorners[2]) || IsContain(rectCorners, targetCorners[3]) ||
        //    IsContain(targetCorners, rectCorners[0]) || IsContain(targetCorners, rectCorners[1]) || IsContain(targetCorners, rectCorners[2]) || IsContain(targetCorners, rectCorners[3]))
        //    return true;

        //return false; // 포함되지 않음

        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];

        rect.GetWorldCorners(aCorners);
        target.GetWorldCorners(bCorners);

        // A의 최소/최대 영역
        float aLeft = aCorners[0].x;
        float aRight = aCorners[2].x;
        float aBottom = aCorners[0].y;
        float aTop = aCorners[2].y;

        // B의 최소/최대 영역
        float bLeft = bCorners[0].x;
        float bRight = bCorners[2].x;
        float bBottom = bCorners[0].y;
        float bTop = bCorners[2].y;

        // 겹치는 영역이 존재하는지 검사
        //bool isOverlapping =
        //    aLeft < bRight && aRight > bLeft &&
        //    aBottom < bTop && aTop > bBottom;

        //return isOverlapping;
        return aLeft < bRight && aRight > bLeft &&
            aBottom < bTop && aTop > bBottom;
    }
    private static bool IsContain(Vector3[] corners, Vector3 pos)
    {
        return (pos.x >= corners[0].x && pos.x <= corners[2].x &&
            pos.y >= corners[0].y && pos.y <= corners[2].y);
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

    public static Block.Data[] Shuffle(this Block.Data[] array)
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

    public static string ToHourTimeText(this int time)
    {
        return TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
    }

    public static string ToDateTimeText(this int time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);

        // 총 시간이 24시간 이상인 경우 (즉, 하루 이상인 경우)
        if (timeSpan.TotalHours >= 24)
        {
            // "날짜.시간:분:초" 형식으로 포맷합니다.
            // d: 날짜
            // \.: '.'을 문자 그대로 사용하기 위한 이스케이프
            return timeSpan.ToString(@"d\.\ hh\:mm\:ss");
        }
        else // 하루 미만인 경우
        {
            // 기존과 같이 "시간:분:초" 형식으로 포맷합니다.
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        //return TimeSpan.FromSeconds(time).ToString(@"dd - hh\:mm\:ss");
    }

    public static string MilliSecondsToTimeText(this int ticks)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(ticks/10000);
        return $"{time.Minutes:D2}:{time.Seconds:D2}.{(int)(time.Milliseconds / 10f):D2}";
    }

    public static string ToTimeText(this long time)
    {
        return time.LongToDateTime().ToString(@"yyyy-MM-dd H\:mm\:ss");
        //return TimeSpan.FromSeconds(time).ToString(@"yyyy-MM-dd H\:mm\:ss");
    }

    public static string ToTimeText(this DateTime time)
    {
        return time.ToString(@"yyyy-MM-dd H\:mm\:ss");
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

    public static string ToProgressText(this int current, int max)
    {
        return new StringBuilder().AppendFormat("{0} / {1}", current, max).ToString();
    }

    public static string ToLevelText(this int level)
    {
        return new StringBuilder().AppendFormat("Lv.{0}", level).ToString();
    }

    public static bool IsSameDate(this DateTime date, DateTime other)
    {
        return date.Year == other.Year && date.Month == other.Month && date.Date == other.Date;
    }

    public static bool IsSameWeek(this DateTime date, DateTime other)
    {
        return date.AddDays(-(int)date.DayOfWeek).IsSameDate(other.AddDays(-(int)other.DayOfWeek));
    }

    public static bool IsSameMonth(this DateTime date, DateTime other)
    {
        return date.Year == other.Year && date.Month == other.Month;
    }

    public static int RemainTimeNextDay(this DateTime date)
    {
        return (int)(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).AddDays(1) - date).TotalSeconds;
    }

    public static int RemainTimeNextWeek(this DateTime date)
    {
        return (int)(new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).AddDays(7-(int)date.DayOfWeek) - date).TotalSeconds;
    }

    public static int RemainTimeNextMonth(this DateTime date)
    {
        return (int)(new DateTime(date.Year, date.Month, 0, 0, 0, 0).AddMonths(1) - date).TotalSeconds;
    }

    private static readonly Regex emailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return emailRegex.IsMatch(email);
    }

    const int rateRange = 1000000;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rate">0.00~1.00</param>
    /// <returns></returns>
    public static bool IsSuccess(this float rate)
    {
        int thisRate = Mathf.FloorToInt(rate * rateRange);
        int ran = UnityEngine.Random.Range(0, rateRange);
        return thisRate > ran;
    }
}

