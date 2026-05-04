using UnityEngine;
using System.Collections.Generic;

public class RoomRoutineTask : MonoBehaviour
{
    [Header("Room Info")]
    public string roomID;

    [Header("Medication UI")]
    public SpriteRenderer medicationRenderer;
    public Color inactiveColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);
    public Color activeColor = Color.cyan;

    [Header("Medication Task State")]
    public bool hasMedicationTask = false;
    public bool isBeingHandled = false;

    [Header("Medication Duration")]
    public float medicationDuration = 0f;

    [Header("Path Settings")]
    public List<Transform> goPath = new List<Transform>();

    private void Start()
    {
        SetMedicationInactive();
    }

    public void ActivateMedicationTask()
    {
        if (hasMedicationTask)
        {
            Debug.Log("Medication task already exists in room: " + roomID);
            return;
        }

        hasMedicationTask = true;
        isBeingHandled = false;

        medicationDuration = GetRandomMedicationDuration();

        SetMedicationActive();

        Debug.Log("Medication task activated: " + roomID +
                  " | Duration: " + medicationDuration.ToString("F2"));
    }

    public void StartHandling()
    {
        isBeingHandled = true;
    }

    public void CompleteMedicationTask()
    {
        hasMedicationTask = false;
        isBeingHandled = false;
        medicationDuration = 0f;

        SetMedicationInactive();

        Debug.Log("Medication task completed: " + roomID);
    }

    private float GetRandomMedicationDuration()
    {
        float randomMinutes = Random.Range(2f, 5f);

        if (TimeManager.Instance != null)
        {
            return randomMinutes * 60f / TimeManager.Instance.timeScale;
        }

        return randomMinutes * 60f;
    }

    private void SetMedicationActive()
    {
        if (medicationRenderer != null)
        {
            medicationRenderer.color = activeColor;
        }
    }

    private void SetMedicationInactive()
    {
        if (medicationRenderer != null)
        {
            medicationRenderer.color = inactiveColor;
        }
    }

    public bool IsAvailableForRoutine()
    {
        return hasMedicationTask && !isBeingHandled;
    }

    public void ResetRoutineTask()
    {
        hasMedicationTask = false;
        isBeingHandled = false;
        medicationDuration = 0f;

        SetMedicationInactive();
    }

}