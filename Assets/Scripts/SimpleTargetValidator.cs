using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTargetValidator : MonoBehaviour
{
    [Header("Target Configuration")]
    [SerializeField] private string targetName;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject correctAnswerEffect; 
    [SerializeField] private GameObject incorrectAnswerEffect;
    [SerializeField] private float effectDuration = 1f;
    
    private QuestionManager questionManager;
    private bool isTracking = false;
    
    void Start()
    {
        if (string.IsNullOrEmpty(targetName))
        {
            targetName = gameObject.name;
        }
        
        if (correctAnswerEffect) correctAnswerEffect.SetActive(false);
        if (incorrectAnswerEffect) incorrectAnswerEffect.SetActive(false);
        
        questionManager = FindFirstObjectByType<QuestionManager>();
        if (questionManager != null)
        {
            questionManager.RegisterTargetValidator(this);
        }
    }
    
    void Update()
    {
        bool currentlyActive = gameObject.activeInHierarchy;
        
        if (currentlyActive && !isTracking)
        {
            OnTrackingFound();
        }
        else if (!currentlyActive && isTracking)
        {
            OnTrackingLost();
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
    
    private void OnTrackingFound()
    {
        if (!isTracking)
        {
            isTracking = true;
            Debug.Log($"Target detectado: {targetName}");
            
            if (questionManager != null)
            {
                questionManager.OnTargetDetected(targetName);
                StartCoroutine(ShowFeedbackEffect());
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
    
    private IEnumerator ShowFeedbackEffect()
    {
        yield return null;
        
        if (questionManager != null)
        {
            Question currentQuestion = questionManager.GetCurrentQuestion();
            if (currentQuestion != null)
            {
                bool isCorrect = targetName.Equals(currentQuestion.correctAnswerTargetName, System.StringComparison.OrdinalIgnoreCase);
                
                GameObject effectToShow = isCorrect ? correctAnswerEffect : incorrectAnswerEffect;
                
                if (effectToShow != null)
                {
                    effectToShow.SetActive(true);
                    yield return new WaitForSeconds(effectDuration);
                    effectToShow.SetActive(false);
                }
            }
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
    
    [ContextMenu("Force Detection")]
    public void ForceDetection()
    {
        OnTrackingFound();
    }
}
