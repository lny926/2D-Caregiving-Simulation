using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time Settings")]
    public float timeScale = 60f;   // 1秒 = 1分钟（游戏内）

    private float currentTime = 0f; // 游戏内秒数，0 = 00:00

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        currentTime += Time.deltaTime * timeScale;

        if (currentTime >= 86400f)
        {
            currentTime = 0f;
        }
    }

    public int GetHour()
    {
        return Mathf.FloorToInt(currentTime / 3600f);
    }

    public int GetMinute()
    {
        return Mathf.FloorToInt((currentTime % 3600f) / 60f);
    }

    public string GetFormattedTime()
    {
        return GetHour().ToString("00") + ":" + GetMinute().ToString("00");
    }

    public bool IsInTimeRange(int startHour, int endHour)
    {
        int hour = GetHour();

        if (startHour <= endHour)
            return hour >= startHour && hour < endHour;
        else
            return hour >= startHour || hour < endHour;
    }

    // 重置时间到 00:00
    public void ResetTimeToMidnight()
    {
        currentTime = 0f;
    }
}