using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetValidator : MonoBehaviour
{
    [Header("Target Configuration")]
    [SerializeField] private string targetName;

    private QuestionManager questionManager;
    private bool isTracking = false;

    void Start()
    {
        if (string.IsNullOrEmpty(targetName))
        {
            targetName = gameObject.name;
        }

        questionManager = FindFirstObjectByType<QuestionManager>();
        if (questionManager != null)
        {
            questionManager.RegisterTargetValidator(this);
        }
    }

    public void SetQuestionManager(QuestionManager manager)
    {
        questionManager = manager;
    }

    public void OnTargetFound()
    {
        OnTrackingFound();
    }

    public void OnTargetLost()
    {
        OnTrackingLost();
    }

    public void Saludar()
    {
        OnTrackingFound();
    }

    public void LimpiarTexto()
    {
        OnTrackingLost();
    }

    private void OnTrackingFound()
    {
        if (!isTracking)
        {
            isTracking = true;
            Debug.Log($"Target detectado: {targetName}");

            if (questionManager != null)
            {
                questionManager.OnTargetDetected(targetName);
            }
        }
    }

    private void OnTrackingLost()
    {
        if (isTracking)
        {
            isTracking = false;
            Debug.Log($"Target perdido: {targetName}");
        }
    }

    public void SetTargetName(string newName)
    {
        targetName = newName;
    }

    public string GetTargetName()
    {
        return targetName;
    }

    public bool IsTracking()
    {
        return isTracking;
    }
}
