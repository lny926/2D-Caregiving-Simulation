using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time Settings")]
    public float timeScale = 60f;   // 1秒 = 1分钟（游戏内）

    private float currentTime = 0f; // 当天秒数，0 = 00:00
    private float totalSimulatedSeconds = 0f; // 累计仿真秒数，不会每天归零

    private void Awake()
    {
        Instance = this;
    }

    public float DeltaSimSeconds { get; private set; }

    void Update()
    {
        float deltaSimTime = Time.deltaTime * timeScale;

        currentTime += deltaSimTime;
        totalSimulatedSeconds += deltaSimTime;

        if (currentTime >= 86400f)
        {
            currentTime -= 86400f;
        }
    }

    public float GetDeltaSimSeconds()
    {
        return DeltaSimSeconds;
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
        int day = GetDayCount();
        return "Day " + day + " " + GetHour().ToString("00") + ":" + GetMinute().ToString("00");
    }

    public int GetDayCount()
    {
        return Mathf.FloorToInt(totalSimulatedSeconds / 86400f) + 1;
    }

    public float GetCurrentTimeSeconds()
    {
        return currentTime;
    }

    public float GetTotalSimulatedSeconds()
    {
        return totalSimulatedSeconds;
    }

    public bool IsInTimeRange(int startHour, int endHour)
    {
        int hour = GetHour();

        if (startHour <= endHour)
            return hour >= startHour && hour < endHour;
        else
            return hour >= startHour || hour < endHour;
    }

    public void ResetTimeToMidnight()
    {
        currentTime = 0f;
        totalSimulatedSeconds = 0f;
    }

    public void SetStartTime(int hour, int minute)
    {
        currentTime = hour * 3600f + minute * 60f;

        // 实验从这个时间点开始，但累计仿真时间从0开始算
        totalSimulatedSeconds = 0f;
    }

    public void SetTimeScale(float newTimeScale)
    {
        timeScale = newTimeScale;
    }

}