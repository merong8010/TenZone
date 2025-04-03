/// <summary>
/// 게임 매니저나 데이터 관리 시스템 등 Unity 객체가 필요 없는 순수 C# 클래스에서 사용 가능
/// </summary>
/// <typeparam name="T"></typeparam>
public class StaticSingleton<T> where T : new()
{
    private static readonly object _lock = new object();
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}