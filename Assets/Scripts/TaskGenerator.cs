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

    [Header("Generation Settings")]
    public float spawnInterval = 5f;

    [Header("Task Type Weights")]
    public int lightWeight = 50;
    public int mediumWeight = 30;
    public int heavyWeight = 20;
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

    void Update()
    {
        if (!autoGenerate) return;
        if (TimeManager.Instance == null) return;
        if (TaskManager.Instance == null) return;

        TimeWindowTaskSettings currentWindow = GetCurrentWindowSettings();
        float interval = currentWindow.spawnInterval;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            TryGenerateTask(currentWindow);
        }
    }

    // 重置任务生成器（清空计时器，避免刚Reset就刷任务）
    public void ResetGenerator()
    {
        timer = 0f;
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