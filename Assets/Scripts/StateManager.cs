using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    [Header("Task Statistics")]
    public int totalTasksCreated = 0;
    public int completedTasks = 0;
    public int waitingTasks = 0;

    public int lightTaskCount = 0;
    public int mediumTaskCount = 0;
    public int heavyTaskCount = 0;

    public float totalWaitingTime = 0f;

    [Header("Nurse Statistics")]
    public float totalDistanceTraveled = 0f;

    private void Awake()
    {
        Instance = this;
    }

    // 创建任务时调用
    public void RegisterTaskCreated(TaskType taskType)
    {
        totalTasksCreated++;
        waitingTasks++;

        switch (taskType)
        {
            case TaskType.Light:
                lightTaskCount++;
                break;
            case TaskType.Medium:
                mediumTaskCount++;
                break;
            case TaskType.Heavy:
                heavyTaskCount++;
                break;
        }
    }

    // 当任务开始被护士处理时调用
    public void RegisterTaskStarted()
    {
        waitingTasks = Mathf.Max(0, waitingTasks - 1);
    }

    public void RegisterTaskCompleted(float waitingTime)
    {
        completedTasks++;
        totalWaitingTime += waitingTime;
    }

    public float GetAverageWaitingTime()
    {
        if (completedTasks == 0) return 0f;
        return totalWaitingTime / completedTasks;
    }

    public void AddDistance(float distance)
    {
        totalDistanceTraveled += distance;
    }

    // 重置所有统计数据
    public void ResetStats()
    {
        totalTasksCreated = 0;
        completedTasks = 0;
        waitingTasks = 0;

        lightTaskCount = 0;
        mediumTaskCount = 0;
        heavyTaskCount = 0;

        totalWaitingTime = 0f;
        totalDistanceTraveled = 0f;
    }
}