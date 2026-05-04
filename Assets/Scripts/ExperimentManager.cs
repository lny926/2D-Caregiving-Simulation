using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance;

    [Header("Experiment Control")]
    public bool autoStartOnPlay = false;
    public bool isExperimentRunning = false;

    [Header("Start Time Settings")]
    [Range(0, 23)]
    public int startHour = 8;

    [Range(0, 59)]
    public int startMinute = 0;

    [Header("Duration Settings")]
    public int experimentDays = 3;

    [Header("Time Speed Settings")]
    public float experimentTimeScale = 300f; // 300 = 1秒现实时间等于5分钟模拟时间

    [Header("Random Seed Settings")]
    public bool useRandomSeed = true;
    public int randomSeed = 12345;

    [Header("Dispatch Mode Settings")]
    public DispatchMode experimentMode = DispatchMode.FCFS;

    [Header("References")]
    public TaskManager taskManager;

    private float experimentDurationSeconds = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartExperiment();
        }
    }

    private void Update()
    {
        if (!isExperimentRunning) return;
        if (TimeManager.Instance == null) return;

        float elapsed = TimeManager.Instance.GetTotalSimulatedSeconds();

        if (elapsed >= experimentDurationSeconds)
        {
            StopExperiment();
        }
    }


    //  按钮调用：Start

    public void StartExperimentFromButton()
    {
        if (isExperimentRunning)
        {
            Debug.Log("Experiment already running.");
            return;
        }

        StartExperiment();
    }


    //  按钮调用：Stop

    public void StopExperimentFromButton()
    {
        StopExperiment();
    }


    //  核心：开始实验

    public void StartExperiment()
    {
        //  恢复 Unity 时间（防止之前暂停）
        Time.timeScale = 1f;

        Debug.Log("Starting experiment...");

        if (taskManager == null)
        {
            taskManager = TaskManager.Instance;
        }

        // 1. 设置随机种子
        if (useRandomSeed)
        {
            Random.InitState(randomSeed);
            Debug.Log("Experiment seed set to: " + randomSeed);
        }

        // 2. 重置系统
        if (taskManager != null)
        {
            taskManager.ResetSimulation();
        }

        // 3. 设置开始时间 & 加速
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetStartTime(startHour, startMinute);
            TimeManager.Instance.SetTimeScale(experimentTimeScale);
        }

        // 4. 设置调度模式
        if (taskManager != null)
        {
            taskManager.currentMode = experimentMode;
            taskManager.pendingMode = experimentMode;
        }

        // 5. 设置实验时长（秒）
        experimentDurationSeconds = experimentDays * 24f * 60f * 60f;

        isExperimentRunning = true;

        Debug.Log(">>> EXPERIMENT MODE ACTIVE <<<");

        Debug.Log("Experiment started | Mode: " + experimentMode +
                  " | Start Time: " + startHour.ToString("00") + ":" + startMinute.ToString("00") +
                  " | Days: " + experimentDays +
                  " | TimeScale: " + experimentTimeScale);
    }


    //  核心：停止实验

    public void StopExperiment()
    {
        if (!isExperimentRunning)
        {
            Debug.Log("Experiment is not running.");
            return;
        }

        isExperimentRunning = false;

        // 暂停整个 Unity 仿真
        Time.timeScale = 0f;

        Debug.Log("Experiment stopped.");

        PrintExperimentSummary();
    }


    // 输出实验结果

    private void PrintExperimentSummary()
    {
        if (StatsManager.Instance == null)
        {
            Debug.LogWarning("No StatsManager found.");
            return;
        }

        StatsManager stats = StatsManager.Instance;

        Debug.Log(
            "===== Experiment Summary =====\n" +
            "Mode: " + experimentMode + "\n" +
            "Seed: " + randomSeed + "\n" +
            "Start Time: " + startHour.ToString("00") + ":" + startMinute.ToString("00") + "\n" +
            "Duration Days: " + experimentDays + "\n\n" +

            "Total Tasks: " + stats.totalTasksCreated + "\n" +
            "Completed Tasks: " + stats.completedTasks + "\n" +
            "Routine Created: " + stats.routineTaskCreated + "\n" +
            "Routine Completed: " + stats.routineTaskCompleted + "\n" +
            "Completion Rate: " + (stats.GetCompletionRate() * 100f).ToString("F1") + "%\n\n" +

            "Average Waiting Time: " + stats.GetAverageWaitingTime().ToString("F2") + "\n" +
            "Max Waiting Time: " + stats.maxWaitingTime.ToString("F2") + "\n" +
            "P95 Waiting Time: " + stats.GetP95WaitingTime().ToString("F2") + "\n\n" +

            "Escalations: " + stats.escalationCount + "\n" +
            "Light -> Medium: " + stats.lightToMediumEscalation + "\n" +
            "Medium -> Heavy: " + stats.mediumToHeavyEscalation + "\n" +
            "Heavy Secondary: " + stats.heavySecondaryCallCount + "\n\n" +

            "Total Distance: " + stats.totalDistanceTraveled.ToString("F2")
        );
    }
}