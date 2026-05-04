using System.Collections.Generic;
using UnityEngine;

public class NurseAction : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("State")]
    public NurseState currentState = NurseState.Idle;

    [Header("Current Path")]
    public List<Transform> currentPath = new List<Transform>();
    private int pathIndex = 0;

    [Header("Speed Display")]
    public float currentActualSpeed = 0f;

    [Header("Personal Station Points")]
    public Transform stationPoint;
    public Transform exitPoint;

    [Header("Work Settings")]
    public float workDuration = 3f;
    private float workTimer = 0f;

    private List<Transform> returnPath = new List<Transform>();

    private RoomTask currentRoomTask;
    private RoomTask currentLocationRoom;

    private Vector3 lastPosition;
    private Vector3 initialPosition;

    // 当前正在处理的 Routine Medication 任务
    private RoomRoutineTask currentRoutineTask;

    // 是否正在处理 Routine Task
    private bool isHandlingRoutineTask = false;

    [Header("Fatigue Settings")]
    [Range(0f, 1f)]
    public float fatigue = 0f;

    public float heavyTaskFatigueIncrease = 0.08f;
    public float fatigueRecoveryPerMinute = 0.002f;
    public float fatigueSpeedCoefficient = 0.5f;

    public bool IsIdle()
    {
        return currentState == NurseState.Idle;
    }

    public bool IsAvailable()
    {
        return currentState == NurseState.Idle && fatigue < 1f;
    }

    void Start()
    {
        if (stationPoint != null)
        {
            initialPosition = stationPoint.position;
            transform.position = initialPosition;
        }
        else
        {
            initialPosition = transform.position;
        }

        lastPosition = transform.position;
        currentActualSpeed = CalculateCurrentMoveSpeed();
    }

    void Update()
    {
        currentActualSpeed = CalculateCurrentMoveSpeed();

        switch (currentState)
        {
            case NurseState.MovingToRoom:
            case NurseState.Returning:
                MoveAlongPath();
                break;

            case NurseState.Working:
                DoWork();
                break;

            case NurseState.Resting:
                HandleResting();
                break;

            case NurseState.Idle:
                break;
        }

        UpdateFatigueRecovery();

        float frameDistance = Vector3.Distance(transform.position, lastPosition);
        if (frameDistance > 0f && StatsManager.Instance != null)
        {
            StatsManager.Instance.AddDistance(frameDistance);
        }

        lastPosition = transform.position;
    }

    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    public void SetTaskPath(List<Transform> goPath, List<Transform> backPath)
    {
        currentPath = new List<Transform>(goPath);
        returnPath = new List<Transform>(backPath);
        pathIndex = 0;
        currentState = NurseState.MovingToRoom;
    }

    public void AssignTask(RoomTask roomTask, List<Transform> goPath, List<Transform> backPath)
    {
        currentRoomTask = roomTask;

        List<Transform> finalGoPath = BuildDynamicGoPath(roomTask, goPath);
        List<Transform> finalBackPath = new List<Transform>(backPath);

        SetTaskPath(finalGoPath, finalBackPath);

        Debug.Log("护工开始前往房间: " + roomTask.roomID);
    }

    public void AssignRoutineTask(RoomRoutineTask routineTask)
    {
        currentRoutineTask = routineTask;
        isHandlingRoutineTask = true;

        if (currentRoutineTask != null)
        {
            currentRoutineTask.StartHandling();
        }

        List<Transform> routinePath = BuildRoutineMedicationPath(routineTask);

        currentPath = routinePath;
        returnPath.Clear();
        pathIndex = 0;
        currentState = NurseState.MovingToRoom;

        Debug.Log("护工开始处理 Medication Routine Task: " + routineTask.roomID);
    }

    private List<Transform> BuildRoutineMedicationPath(RoomRoutineTask routineTask)
    {
        List<Transform> path = new List<Transform>();

        // 1. 如果护士当前在某个房间，先沿当前房间路径反向回到公共走廊
        if (currentLocationRoom != null &&
            currentLocationRoom.goPath != null &&
            currentLocationRoom.goPath.Count > 0)
        {
            List<Transform> roomPath = currentLocationRoom.goPath;

            // 从当前房间点的前一个节点开始反向走
            for (int i = roomPath.Count - 2; i >= 0; i--)
            {
                if (roomPath[i] == null) continue;

                // 不走 BaseCenter，避免回站路径绕中心点
                if (roomPath[i].name == "BaseCenter")
                {
                    continue;
                }

                path.Add(roomPath[i]);
            }
        }

        bool isAtStation = currentLocationRoom == null;

        if (!isAtStation)
        {
            // 如果护士在房间/走廊外，先经过 exitPoint 回护士站
            if (exitPoint != null)
            {
                path.Add(exitPoint);
            }

            if (stationPoint != null)
            {
                path.Add(stationPoint);
            }
        }
        else
        {
            // 如果护士本来就在护士站，直接把 stationPoint 当作取药点
            if (stationPoint != null)
            {
                path.Add(stationPoint);
            }
        }

        // 取完药后，从自己的 exitPoint 出站
        if (exitPoint != null)
        {
            path.Add(exitPoint);
        }

        // 5. 前往目标房间，跳过 BaseCenter
        if (routineTask != null && routineTask.goPath != null)
        {
            foreach (Transform point in routineTask.goPath)
            {
                if (point == null) continue;

                if (point.name == "BaseCenter")
                {
                    continue;
                }

                path.Add(point);
            }
        }

        return path;
    }

    private List<Transform> BuildDynamicGoPath(RoomTask targetRoom, List<Transform> targetGoPath)
    {
        List<Transform> path = new List<Transform>();

        // 情况1：护士当前不在房间，说明从护士站出发
        // 路径：护士当前站位 -> 个人出口点 -> 房间公共路径
        if (currentLocationRoom == null ||
            currentLocationRoom.goPath == null ||
            currentLocationRoom.goPath.Count == 0)
        {
            if (exitPoint != null)
            {
                path.Add(exitPoint);
            }

            // 从护士站出发时，跳过 BaseCenter，避免出站又绕回中心点
            AddPathSkippingBaseCenter(path, targetGoPath);

            return path;
        }

        List<Transform> currentRoomPath = currentLocationRoom.goPath;
        List<Transform> targetRoomPath = targetGoPath;

        int lastCommonIndexCurrent = -1;
        int lastCommonIndexTarget = -1;

        // 找两个路径中最后一个共同节点
        for (int i = 0; i < currentRoomPath.Count; i++)
        {
            for (int j = 0; j < targetRoomPath.Count; j++)
            {
                if (currentRoomPath[i] == targetRoomPath[j])
                {
                    lastCommonIndexCurrent = i;
                    lastCommonIndexTarget = j;
                }
            }
        }

        if (lastCommonIndexCurrent == -1 || lastCommonIndexTarget == -1)
        {
            Debug.LogWarning("No common waypoint found. Fallback to target goPath.");

            if (exitPoint != null)
            {
                path.Add(exitPoint);
            }

            path.AddRange(targetGoPath);
            return path;
        }

        // 从当前房间倒退到最近共同节点
        for (int i = currentRoomPath.Count - 2; i >= lastCommonIndexCurrent; i--)
        {
            path.Add(currentRoomPath[i]);
        }

        // 从共同节点前往目标房间
        for (int i = lastCommonIndexTarget + 1; i < targetRoomPath.Count; i++)
        {
            path.Add(targetRoomPath[i]);
        }

        return path;
    }

    private void AddPathSkippingBaseCenter(List<Transform> resultPath, List<Transform> sourcePath)
    {
        foreach (Transform point in sourcePath)
        {
            if (point == null) continue;

            // 从护士站出发/回站时跳过 BaseCenter
            if (point.name == "BaseCenter")
            {
                continue;
            }

            resultPath.Add(point);
        }
    }

    private List<Transform> BuildReturnToStationPath()
    {
        List<Transform> path = new List<Transform>();

        // 如果护士当前在某个房间，沿该房间公共路径反向回走廊
        if (currentLocationRoom != null &&
            currentLocationRoom.goPath != null &&
            currentLocationRoom.goPath.Count > 0)
        {
            List<Transform> roomPath = currentLocationRoom.goPath;

            // 从房间前一个节点开始反向走
            for (int i = roomPath.Count - 2; i >= 0; i--)
            {
                if (roomPath[i] == null) continue;

                // 回护士站时也跳过 BaseCenter
                if (roomPath[i].name == "BaseCenter")
                {
                    continue;
                }

                path.Add(roomPath[i]);
            }
        }

        // 进入自己的出站/入站点
        if (exitPoint != null)
        {
            path.Add(exitPoint);
        }

        // 最后回到自己的站位点
        if (stationPoint != null)
        {
            path.Add(stationPoint);
        }
        else
        {
            // 兜底：如果没设 stationPoint，就回初始位置
            GameObject fallback = new GameObject(name + "_FallbackStationPoint");
            fallback.transform.position = initialPosition;
            path.Add(fallback.transform);
        }

        return path;
    }

    private float CalculateCurrentMoveSpeed()
    {
        float speedMultiplier = 1f - fatigue * fatigueSpeedCoefficient;
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.2f, 1f);

        return moveSpeed * speedMultiplier;
    }

    private float GetCurrentMoveSpeed()
    {
        currentActualSpeed = CalculateCurrentMoveSpeed();
        return currentActualSpeed;
    }

    private void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Transform targetPoint = currentPath[pathIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            GetCurrentMoveSpeed() * Time.deltaTime
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

                    // Routine Medication Task
                    if (isHandlingRoutineTask && currentRoutineTask != null)
                    {
                        if (StatsManager.Instance != null)
                        {
                            StatsManager.Instance.RegisterTaskStarted();
                        }

                        workTimer = currentRoutineTask.medicationDuration;

                        Debug.Log("到达房间，开始 Medication: " + currentRoutineTask.roomID +
                                  " | 工作时长: " + workTimer);
                    }
                    // Normal Room Task
                    else if (currentRoomTask != null)
                    {
                        currentRoomTask.isBeingHandled = true;

                        if (StatsManager.Instance != null)
                        {
                            StatsManager.Instance.RegisterTaskStarted();
                        }

                        workTimer = currentRoomTask.taskDuration;

                        Debug.Log("到达房间，开始工作: " + currentRoomTask.roomID +
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
                    Debug.Log("护工已返回自己的护士站位置");

                    currentPath.Clear();
                    returnPath.Clear();
                    pathIndex = 0;

                    currentLocationRoom = null;
                    currentRoomTask = null;

                    if (fatigue >= 1f)
                    {
                        currentState = NurseState.Resting;
                        Debug.Log(name + " is resting due to fatigue.");
                    }
                    else
                    {
                        currentState = NurseState.Idle;
                        Debug.Log(name + " is idle at nurse station.");

                        if (TaskManager.Instance != null)
                        {
                            TaskManager.Instance.TryAssignNextTask();
                        }
                    }
                }
            }
        }
    }

    private void DoWork()
    {
        workTimer -= Time.deltaTime;

        if (workTimer <= 0f)
        {
            // 只有当前任务本身就是 Medication，才完成 Medication
            if (isHandlingRoutineTask && currentRoutineTask != null)
            {
                Debug.Log("完成 Medication Routine Task: " + currentRoutineTask.roomID);

                currentRoutineTask.CompleteMedicationTask();

                if (StatsManager.Instance != null)
                {
                    StatsManager.Instance.RegisterRoutineTaskCompleted();
                }

                RoomTask roomTask = currentRoutineTask.GetComponent<RoomTask>();
                if (roomTask != null)
                {
                    currentLocationRoom = roomTask;
                }

                currentRoutineTask = null;
                isHandlingRoutineTask = false;

                AfterTaskFinished();
                return;
            }

            // 普通任务完成，只完成普通任务，不碰 Medication
            if (currentRoomTask != null)
            {
                if (currentRoomTask.currentTaskType == TaskType.Heavy)
                {
                    AddFatigue(heavyTaskFatigueIncrease);
                }

                Debug.Log("完成普通护理任务: " + currentRoomTask.roomID);

                currentLocationRoom = currentRoomTask;
                currentRoomTask.CompleteTask();
                currentRoomTask = null;
            }

            AfterTaskFinished();
        }
    }

    private void AfterTaskFinished()
    {
        // 疲劳满了，强制回护士站休息
        if (fatigue >= 1f)
        {
            currentPath = BuildReturnToStationPath();
            pathIndex = 0;
            currentState = NurseState.Returning;

            Debug.Log(name + " is exhausted, returning to station to rest.");
            return;
        }

        // 如果还有普通任务或 routine 任务，直接尝试接下一个
        if (TaskManager.Instance != null &&
            (TaskManager.Instance.GetPendingTaskCount() > 0 ||
             TaskManager.Instance.GetPendingRoutineTaskCount() > 0))
        {
            currentState = NurseState.Idle;
            currentPath.Clear();
            returnPath.Clear();
            pathIndex = 0;

            Debug.Log("任务完成，当前还有待处理任务，护士从当前位置继续接任务。");
            TaskManager.Instance.TryAssignNextTask();
        }
        else
        {
            // 没有任务，回自己的护士站站位
            currentPath = BuildReturnToStationPath();
            pathIndex = 0;
            currentState = NurseState.Returning;

            Debug.Log("任务完成，当前没有等待任务，护士返回自己的站位点。");
        }
    }

    private void AddFatigue(float amount)
    {
        fatigue += amount;
        fatigue = Mathf.Clamp01(fatigue);

        Debug.Log(name + " fatigue increased to: " + fatigue.ToString("F2"));

        if (fatigue >= 1f)
        {
            Debug.Log(name + " is exhausted! Will return to station after current task.");
        }
    }

    public void ResetNurse()
    {
        if (stationPoint != null)
        {
            initialPosition = stationPoint.position;
            transform.position = initialPosition;
        }
        else
        {
            transform.position = initialPosition;
        }

        currentState = NurseState.Idle;
        currentPath.Clear();
        returnPath.Clear();

        pathIndex = 0;
        workTimer = 0f;

        currentRoomTask = null;
        currentLocationRoom = null;
        currentRoutineTask = null;
        isHandlingRoutineTask = false;

        fatigue = 0f;
        lastPosition = transform.position;
        currentActualSpeed = CalculateCurrentMoveSpeed();

        Debug.Log("Nurse reset to station position.");
    }

    public void SetInitialPosition(Vector3 pos)
    {
        initialPosition = pos;
        transform.position = pos;
        lastPosition = pos;
        currentActualSpeed = CalculateCurrentMoveSpeed();
    }

    private void UpdateFatigueRecovery()
    {
        bool shouldRecover = false;

        if (currentState == NurseState.Idle)
        {
            shouldRecover = true;
        }

        if (currentState == NurseState.Working &&
            currentRoomTask != null &&
            currentRoomTask.currentTaskType == TaskType.Light)
        {
            shouldRecover = true;
        }

        if (!shouldRecover) return;

        float simulatedMinutes = GetSimulatedMinutesThisFrame();
        fatigue -= fatigueRecoveryPerMinute * simulatedMinutes;
        fatigue = Mathf.Clamp01(fatigue);
    }

    private float GetSimulatedMinutesThisFrame()
    {
        if (TimeManager.Instance != null)
        {
            return Time.deltaTime * TimeManager.Instance.timeScale / 60f;
        }

        return Time.deltaTime / 60f;
    }

    private void HandleResting()
    {
        float simulatedMinutes = GetSimulatedMinutesThisFrame();

        fatigue -= fatigueRecoveryPerMinute * simulatedMinutes;
        fatigue = Mathf.Clamp01(fatigue);

        if (fatigue <= 0.5f)
        {
            currentState = NurseState.Idle;
            Debug.Log(name + " has recovered and is back to work.");

            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.TryAssignNextTask();
            }
        }
    }
}