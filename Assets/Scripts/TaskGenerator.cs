using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeWindowTaskSettings
{
    [Header("Window Name")]
    public string windowName;

    [Header("Time Range")]
    public int startHour;
    public int endHour;

    [Header("Random Spawn Interval")]
    public float minSpawnInterval = 8f;
    public float maxSpawnInterval = 12f;

    [Header("Task Type Weights")]
    public int lightWeight = 50;
    public int mediumWeight = 30;
    public int heavyWeight = 20;

    [Header("Task Count Weights Per Burst")]
    public int oneTaskWeight = 50;
    public int twoTaskWeight = 30;
    public int threeTaskWeight = 15;
    public int fourTaskWeight = 5;
}

public class TaskGenerator : MonoBehaviour
{
    [Header("All Rooms In Scene")]
    public List<RoomTask> allRooms = new List<RoomTask>();

    [Header("Enable Auto Generation")]
    public bool autoGenerate = true;

    [Header("Off-Peak / Normal Settings")]
    public TimeWindowTaskSettings normalSettings;

    [Header("Morning / Midday / Evening Settings")]
    public TimeWindowTaskSettings morningSettings;
    public TimeWindowTaskSettings middaySettings;
    public TimeWindowTaskSettings eveningSettings;

    private float timer = 0f;
    private float nextSpawnInterval = 5f;

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (!autoGenerate) return;
        if (TimeManager.Instance == null) return;
        if (TaskManager.Instance == null) return;

        timer += Time.deltaTime;

        if (timer >= nextSpawnInterval)
        {
            timer = 0f;

            TimeWindowTaskSettings currentWindow = GetCurrentWindowSettings();

            int taskCount = GetWeightedTaskCount(currentWindow);

            for (int i = 0; i < taskCount; i++)
            {
                TryGenerateTask(currentWindow);
            }

            ScheduleNextSpawn();
        }
    }

    // 重置任务生成器（清空计时器，避免刚Reset就刷任务）
    public void ResetGenerator()
    {
        timer = 0f;
        ScheduleNextSpawn();
    }

    // 获取当前时间段配置
    private TimeWindowTaskSettings GetCurrentWindowSettings()
    {
        if (TimeManager.Instance.IsInTimeRange(morningSettings.startHour, morningSettings.endHour))
        {
            return morningSettings;
        }

        if (TimeManager.Instance.IsInTimeRange(middaySettings.startHour, middaySettings.endHour))
        {
            return middaySettings;
        }

        if (TimeManager.Instance.IsInTimeRange(eveningSettings.startHour, eveningSettings.endHour))
        {
            return eveningSettings;
        }

        // 不在三段高峰里，就用平时配置
        return normalSettings;
    }

    // 尝试自动生成任务
    private void TryGenerateTask(TimeWindowTaskSettings settings)
    {
        List<RoomTask> availableRooms = GetAvailableRooms();

        if (availableRooms.Count == 0)
        {
            Debug.Log("No available rooms for auto task generation.");
            return;
        }

        RoomTask selectedRoom = availableRooms[Random.Range(0, availableRooms.Count)];
        TaskType taskType = GetWeightedTaskType(settings);

        TaskManager.Instance.TryCreateTask(selectedRoom, taskType);

        Debug.Log("Auto-generated task at room: " + selectedRoom.roomID +
                  " | Type: " + taskType +
                  " | Window: " + settings.windowName);
    }

    // 获取当前无任务房间
    private List<RoomTask> GetAvailableRooms()
    {
        List<RoomTask> available = new List<RoomTask>();

        foreach (RoomTask room in allRooms)
        {
            if (room != null && !room.hasTask)
            {
                available.Add(room);
            }
        }

        return available;
    }

    // 按权重随机任务类型
    private TaskType GetWeightedTaskType(TimeWindowTaskSettings settings)
    {
        int totalWeight = settings.lightWeight + settings.mediumWeight + settings.heavyWeight;

        if (totalWeight <= 0)
        {
            return TaskType.Light;
        }

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < settings.lightWeight)
        {
            return TaskType.Light;
        }
        else if (randomValue < settings.lightWeight + settings.mediumWeight)
        {
            return TaskType.Medium;
        }
        else
        {
            return TaskType.Heavy;
        }
    }

    private void ScheduleNextSpawn()
    {
        TimeWindowTaskSettings currentWindow = GetCurrentWindowSettings();

        if (currentWindow == null)
        {
            nextSpawnInterval = 8f;
            return;
        }

        float min = Mathf.Min(currentWindow.minSpawnInterval, currentWindow.maxSpawnInterval);
        float max = Mathf.Max(currentWindow.minSpawnInterval, currentWindow.maxSpawnInterval);

        nextSpawnInterval = Random.Range(min, max);
    }

    private int GetWeightedTaskCount(TimeWindowTaskSettings settings)
    {
        int totalWeight =
            settings.oneTaskWeight +
            settings.twoTaskWeight +
            settings.threeTaskWeight +
            settings.fourTaskWeight;

        if (totalWeight <= 0)
        {
            return 1;
        }

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < settings.oneTaskWeight)
        {
            return 1;
        }
        else if (randomValue < settings.oneTaskWeight + settings.twoTaskWeight)
        {
            return 2;
        }
        else if (randomValue < settings.oneTaskWeight + settings.twoTaskWeight + settings.threeTaskWeight)
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }

    // 获取当前时间窗口名称，给UI显示用
    public string GetCurrentWindowName()
    {
        if (TimeManager.Instance == null)
        {
            return "Unknown";
        }

        TimeWindowTaskSettings currentWindow = GetCurrentWindowSettings();

        if (currentWindow != null && !string.IsNullOrEmpty(currentWindow.windowName))
        {
            return currentWindow.windowName;
        }

        return "Unknown";
    }
}