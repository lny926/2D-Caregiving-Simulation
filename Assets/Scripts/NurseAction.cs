using System.Collections.Generic;
using UnityEngine;

public class NurseAction : MonoBehaviour
{
    public float moveSpeed = 5f;

    // 当前状态
    public NurseState currentState = NurseState.Idle;

    // 当前路径
    public List<Transform> currentPath = new List<Transform>();
    private int pathIndex = 0;

    [Header("Return Anchor")]
    public Transform returnAnchor;

    // 默认工作时间（仅备用）
    public float workDuration = 3f;
    private float workTimer = 0f;

    // 返回路径
    private List<Transform> returnPath = new List<Transform>();

    // 当前正在处理的房间
    private RoomTask currentRoomTask;

    private Vector3 lastPosition;

    private Vector3 initialPosition;

    // 判断护工是否空闲
    public bool IsIdle()
    {
        return currentState == NurseState.Idle;
    }

    void Start()
    {
        lastPosition = transform.position;

        // 记录护士初始位置（护士站初始点）
        initialPosition = transform.position;
        lastPosition = transform.position;

    }

    void Update()
    {
        switch (currentState)
        {
            case NurseState.MovingToRoom:
            case NurseState.Returning:
                MoveAlongPath();
                break;

            case NurseState.Working:
                DoWork();
                break;

            case NurseState.Idle:
                break;
        }

        // 统计移动距离
        float frameDistance = Vector3.Distance(transform.position, lastPosition);
        if (frameDistance > 0f && StatsManager.Instance != null)
        {
            StatsManager.Instance.AddDistance(frameDistance);
        }
        lastPosition = transform.position;
    }

    // 获取护士自己的初始站位
    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    // 设置任务路径
    public void SetTaskPath(List<Transform> goPath, List<Transform> backPath)
    {
        currentPath = new List<Transform>(goPath);
        returnPath = new List<Transform>(backPath);
        pathIndex = 0;
        currentState = NurseState.MovingToRoom;
    }

    // 分配任务给护工
    public void AssignTask(RoomTask roomTask, List<Transform> goPath, List<Transform> backPath)
    {
        currentRoomTask = roomTask;

        // 复制房间提供的公共返回路径
        List<Transform> finalBackPath = new List<Transform>(backPath);

        // 动态创建一个只属于当前护士的最终回位点
        GameObject tempReturnPoint = new GameObject(name + "_ReturnPoint");
        tempReturnPoint.transform.position = initialPosition;

        // 把自己的站位点补到返回路径最后
        finalBackPath.Add(tempReturnPoint.transform);

        SetTaskPath(goPath, finalBackPath);

        Debug.Log("护工开始前往房间: " + roomTask.roomID);
    }

    // 沿路径移动
    private void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Transform targetPoint = currentPath[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            transform.position = targetPoint.position;
            pathIndex++;

            if (pathIndex >= currentPath.Count)
            {
                if (currentState == NurseState.MovingToRoom)
                {
                    currentState = NurseState.Working;

                    if (currentRoomTask != null)
                    {
                        // 标记：这个任务已经开始被处理
                        currentRoomTask.isBeingHandled = true;

                        // 一旦开始处理，就不再算 waiting task
                        if (StatsManager.Instance != null)
                        {
                            StatsManager.Instance.RegisterTaskStarted();
                        }

                        workTimer = currentRoomTask.taskDuration;
                        UnityEngine.Debug.Log("到达房间，开始工作: " + currentRoomTask.roomID +
                                              " | 类型: " + currentRoomTask.currentTaskType +
                                              " | 工作时长: " + workTimer);
                    }
                    else
                    {
                        workTimer = workDuration;
                    }
                }
                else if (currentState == NurseState.Returning)
                {
                    Debug.Log("护工已返回护士站");

                    currentState = NurseState.Idle;
                    currentPath.Clear();
                    returnPath.Clear();
                    pathIndex = 0;

                    if (currentRoomTask != null)
                    {
                        currentRoomTask = null;
                    }

                    Debug.Log("护工现在空闲，准备请求下一个任务");

                    if (TaskManager.Instance != null)
                    {
                        TaskManager.Instance.TryAssignNextTask();
                    }
                    else
                    {
                        Debug.LogError("TaskManager.Instance 是空的！");
                    }
                }
            }
        }
    }

    // 模拟工作
    private void DoWork()
    {
        workTimer -= Time.deltaTime;

        if (workTimer <= 0f)
        {
            // 工作完成后，立刻清除房间任务状态
            if (currentRoomTask != null)
            {
                Debug.Log("完成房间任务: " + currentRoomTask.roomID);
                currentRoomTask.CompleteTask();
            }

            // 然后开始返回护士站
            currentPath = new List<Transform>(returnPath);
            pathIndex = 0;
            currentState = NurseState.Returning;
            Debug.Log("工作完成，开始返回护士站");
        }
    }

    // 重置护工状态，并直接回到自己的初始站位
    public void ResetNurse()
    {
        transform.position = initialPosition;

        if (returnAnchor != null)
        {
            returnAnchor.position = initialPosition;
        }

        currentState = NurseState.Idle;
        currentPath.Clear();
        pathIndex = 0;
        returnPath.Clear();
        workTimer = 0f;
        currentRoomTask = null;
        lastPosition = initialPosition;

        Debug.Log("Nurse reset to initial station position.");
    }

    // 由外部设置护士的初始站位
    public void SetInitialPosition(Vector3 pos)
    {
        initialPosition = pos;
        transform.position = pos;
        lastPosition = pos;

        if (returnAnchor == null)
        {
            GameObject anchorObj = new GameObject(name + "_ReturnAnchor");
            returnAnchor = anchorObj.transform;
        }

        returnAnchor.position = pos;
    }
}