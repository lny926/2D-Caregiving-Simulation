using System.Collections.Generic;
using UnityEngine;

public class RoutineTaskManager : MonoBehaviour
{
    public static RoutineTaskManager Instance;

    [Header("All Routine Rooms")]
    public List<RoomRoutineTask> allRooms = new List<RoomRoutineTask>();

    [Header("Routine Settings")]
    public int roomsPerTrigger = 2;

    // 每2小时触发一次（模拟时间）
    public float routineIntervalSeconds = 7200f;

    // 下一次触发时间（累计模拟时间）
    private float nextTriggerTotalTime = 7200f;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (TimeManager.Instance == null) return;

        float totalTime = TimeManager.Instance.GetTotalSimulatedSeconds();

        if (totalTime >= nextTriggerTotalTime)
        {
            TriggerRoutineTasks();

            // 防止 timeScale 很大跳过多个周期
            while (totalTime >= nextTriggerTotalTime)
            {
                nextTriggerTotalTime += routineIntervalSeconds;
            }
        }
    }

    private void TriggerRoutineTasks()
    {
        List<RoomRoutineTask> availableRooms = GetAvailableRooms();

        if (availableRooms.Count == 0)
        {
            Debug.Log("No available rooms for routine task.");
            return;
        }

        int count = Mathf.Min(roomsPerTrigger, availableRooms.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableRooms.Count);

            RoomRoutineTask room = availableRooms[randomIndex];

            // 1. 激活房间 UI
            room.ActivateMedicationTask();

            // 2. 加入 TaskManager 队列
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.AddRoutineTask(room);
            }

            availableRooms.RemoveAt(randomIndex);
        }

        Debug.Log("Routine medication tasks triggered.");
    }

    private List<RoomRoutineTask> GetAvailableRooms()
    {
        List<RoomRoutineTask> result = new List<RoomRoutineTask>();

        foreach (RoomRoutineTask room in allRooms)
        {
            if (room != null && !room.hasMedicationTask)
            {
                result.Add(room);
            }
        }

        return result;
    }

    public void ResetRoutineTaskManager()
    {
        nextTriggerTotalTime = routineIntervalSeconds;
    }
}