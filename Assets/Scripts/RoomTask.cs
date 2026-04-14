using System.Collections.Generic;
using UnityEngine;

public class RoomTask : MonoBehaviour
{
    [Header("房间信息")]
    public string roomID;

    [Header("房间颜色显示")]
    public SpriteRenderer roomRenderer;
    public Color normalColor = Color.white;
    public Color lightTaskColor = Color.green;
    public Color mediumTaskColor = Color.yellow;
    public Color heavyTaskColor = Color.red;

    [Header("路径设置")]
    // 去程路径：手动拖拽
    public List<Transform> goPath = new List<Transform>();

    // 是否自动生成返回路径
    public bool autoGenerateReturnPath = true;

    // 自动生成时，是否移除房间点
    public bool removeRoomPointFromReturn = true;

    // 手动返回路径
    public List<Transform> manualReturnPath = new List<Transform>();

    // 护士站点
    public Transform nurseStationPoint;

    [Header("任务状态")]
    public bool hasTask = false;

    // 当前任务类型
    public TaskType currentTaskType;

    // 当前任务持续时间
    public float taskDuration = 0f;

    [Header("升级机制")]
    // 升级阈值（秒），Inspector 可调
    public float escalationThreshold = 30f;

    // 当前任务已经等待了多久
    public float waitingTime = 0f;

    // 当前任务是否正在被处理
    public bool isBeingHandled = false;

    private void Start()
    {
        SetNormalColor();
    }

    private void Update()
    {
        // 只有“有任务且还没被处理”时，才继续等待、变色、升级
        if (hasTask && !isBeingHandled)
        {
            waitingTime += Time.deltaTime;

            // 每帧更新颜色渐变
            UpdateTaskColorByProgress();

            // 检查是否需要升级
            CheckEscalation();
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("点击房间: " + roomID);

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.TryCreateTask(this);
        }
    }

    // 创建随机任务（保留给手动点击用）
    public void CreateTask()
    {
        hasTask = true;
        isBeingHandled = false;
        waitingTime = 0f;

        int randomValue = Random.Range(0, 100);

        if (randomValue < 50)
        {
            SetTaskType(TaskType.Light);
        }
        else if (randomValue < 80)
        {
            SetTaskType(TaskType.Medium);
        }
        else
        {
            SetTaskType(TaskType.Heavy);
        }

        Debug.Log("Room task created: " + roomID +
                  " | Type: " + currentTaskType +
                  " | Duration: " + taskDuration);
    }

    // 创建指定类型任务（给自动生成器用）
    public void CreateTask(TaskType forcedType)
    {
        hasTask = true;
        isBeingHandled = false;
        waitingTime = 0f;

        SetTaskType(forcedType);

        Debug.Log("Room task created: " + roomID +
                  " | Type: " + currentTaskType +
                  " | Duration: " + taskDuration);
    }

    // 根据任务类型设置持续时间和颜色
    private void SetTaskType(TaskType type)
    {
        currentTaskType = type;

        switch (type)
        {
            case TaskType.Light:
                taskDuration = 5f;
                ApplyCurrentTaskBaseColor();
                break;

            case TaskType.Medium:
                taskDuration = 10f;
                ApplyCurrentTaskBaseColor();
                break;

            case TaskType.Heavy:
                taskDuration = 15f;
                ApplyCurrentTaskBaseColor();
                break;
        }
    }

    public void CompleteTask()
    {
        float finalWaitingTime = waitingTime;

        hasTask = false;
        isBeingHandled = false;
        taskDuration = 0f;
        waitingTime = 0f;
        SetNormalColor();

        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.RegisterTaskCompleted(finalWaitingTime);
        }

        Debug.Log("Room task completed: " + roomID);
    }

    // 根据当前任务类型设置基础颜色
    private void ApplyCurrentTaskBaseColor()
    {
        if (roomRenderer == null) return;

        switch (currentTaskType)
        {
            case TaskType.Light:
                roomRenderer.color = lightTaskColor;
                break;
            case TaskType.Medium:
                roomRenderer.color = mediumTaskColor;
                break;
            case TaskType.Heavy:
                roomRenderer.color = heavyTaskColor;
                break;
        }
    }

    // 更新颜色渐变
    private void UpdateTaskColorByProgress()
    {
        if (roomRenderer == null) return;

        // 进度 0~1
        float progress = Mathf.Clamp01(waitingTime / escalationThreshold);

        if (currentTaskType == TaskType.Light)
        {
            // 绿色 -> 黄色
            roomRenderer.color = Color.Lerp(lightTaskColor, mediumTaskColor, progress);
        }
        else if (currentTaskType == TaskType.Medium)
        {
            // 黄色 -> 红色
            roomRenderer.color = Color.Lerp(mediumTaskColor, heavyTaskColor, progress);
        }
        else if (currentTaskType == TaskType.Heavy)
        {
            // Heavy 保持红色
            roomRenderer.color = heavyTaskColor;
        }
    }

    // 检查是否升级
    private void CheckEscalation()
    {
        if (waitingTime < escalationThreshold) return;

        if (currentTaskType == TaskType.Light)
        {
            currentTaskType = TaskType.Medium;
            taskDuration = 10f;
            waitingTime = 0f; // 重置等待时间，开始下一轮 Medium -> Heavy 的倒计时

            Debug.Log("任务升级: " + roomID + " Light -> Medium");
        }
        else if (currentTaskType == TaskType.Medium)
        {
            currentTaskType = TaskType.Heavy;
            taskDuration = 15f;
            waitingTime = 0f; // Heavy 不再升级，但重置也没问题

            Debug.Log("任务升级: " + roomID + " Medium -> Heavy");
        }
        else if (currentTaskType == TaskType.Heavy)
        {
            // Heavy 不再升级
            waitingTime = escalationThreshold;
        }

        // 升级后立刻更新基础颜色
        ApplyCurrentTaskBaseColor();
    }

    // 恢复正常颜色
    public void SetNormalColor()
    {
        if (roomRenderer != null)
        {
            roomRenderer.color = normalColor;
        }
    }

    // 获取返回路径
    public List<Transform> GetReturnPath()
    {
        List<Transform> finalReturnPath = new List<Transform>();

        if (autoGenerateReturnPath)
        {
            finalReturnPath = new List<Transform>(goPath);
            finalReturnPath.Reverse();

            if (removeRoomPointFromReturn && finalReturnPath.Count > 0)
            {
                finalReturnPath.RemoveAt(0);
            }
        }
        else
        {
            finalReturnPath = new List<Transform>(manualReturnPath);
        }

        return finalReturnPath;
    }

    // 强制重置房间任务状态
    public void ResetRoomTask()
    {
        hasTask = false;
        isBeingHandled = false;
        waitingTime = 0f;
        taskDuration = 0f;

        SetNormalColor();
    }
}