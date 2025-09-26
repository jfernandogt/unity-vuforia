using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Question
{
    public string questionText;
    public string correctAnswerTargetName;
    public string[] possibleTargets;
}

public class QuestionManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject feedbackPanel;
    
    [Header("Question Settings")]
    [SerializeField] private Question[] questions;
    [SerializeField] private float feedbackDisplayTime = 2f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    
    private Question currentQuestion;
    private bool waitingForAnswer = false;
    private List<TargetValidator> targetValidators = new List<TargetValidator>();
    private List<SimpleTargetValidator> simpleTargetValidators = new List<SimpleTargetValidator>();
    
    public static QuestionManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        targetValidators.AddRange(FindObjectsByType<TargetValidator>(FindObjectsSortMode.None));
        
        foreach (var validator in targetValidators)
        {
            validator.SetQuestionManager(this);
        }
        
        if (questionPanel) questionPanel.SetActive(false);
        if (feedbackPanel) feedbackPanel.SetActive(false);
        
        if (IsSystemReady())
        {
            StartCoroutine(StartFirstQuestionAfterDelay());
        }
        else
        {
            Debug.Log("QuestionManager: Sistema no está completamente configurado. Esperando configuración manual.");
        }
    }
    
    private bool IsSystemReady()
    {
        return questionText != null && feedbackText != null && 
               questionPanel != null && feedbackPanel != null && 
               questions.Length > 0;
    }
    
    IEnumerator StartFirstQuestionAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        ShowRandomQuestion();
    }
    
    public void ShowRandomQuestion()
    {
        if (questions.Length == 0) return;
        
        currentQuestion = questions[Random.Range(0, questions.Length)];
        
        if (questionText)
        {
            questionText.text = currentQuestion.questionText;
        }
        
        if (questionPanel)
        {
            questionPanel.SetActive(true);
        }
        
        if (feedbackPanel)
        {
            feedbackPanel.SetActive(false);
        }
        
        waitingForAnswer = true;
        
        Debug.Log($"Pregunta mostrada: {currentQuestion.questionText}");
        Debug.Log($"Respuesta correcta: {currentQuestion.correctAnswerTargetName}");
    }
    
    public void OnTargetDetected(string targetName)
    {
        if (!waitingForAnswer || currentQuestion == null) return;
        
        bool isCorrect = targetName.Equals(currentQuestion.correctAnswerTargetName, System.StringComparison.OrdinalIgnoreCase);
        
        ShowFeedback(isCorrect, targetName);
        
        if (audioSource)
        {
            if (isCorrect && correctSound)
            {
                audioSource.PlayOneShot(correctSound);
            }
            else if (!isCorrect && incorrectSound)
            {
                audioSource.PlayOneShot(incorrectSound);
            }
        }
        
        waitingForAnswer = false;
        
        StartCoroutine(ShowNextQuestionAfterDelay());
    }
    
    void ShowFeedback(bool isCorrect, string detectedTarget)
    {
        string feedbackMessage;
        
        if (isCorrect)
        {
            feedbackMessage = "Correct!";
        }
        else
        {
            feedbackMessage = $"Incorrect. That's a: {detectedTarget}";
        }
        
        if (feedbackText)
        {
            feedbackText.text = feedbackMessage;
        }
        
        if (feedbackPanel)
        {
            feedbackPanel.SetActive(true);
        }
        
        if (questionPanel)
        {
            questionPanel.SetActive(false);
        }
        
        Debug.Log(feedbackMessage);
    }
    
    IEnumerator ShowNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);
        ShowRandomQuestion();
    }
    
    public void AddQuestion(Question newQuestion)
    {
        List<Question> questionList = new List<Question>(questions);
        questionList.Add(newQuestion);
        questions = questionList.ToArray();
    }
    
    public Question GetCurrentQuestion()
    {
        return currentQuestion;
    }
    
    public void RegisterTargetValidator(SimpleTargetValidator validator)
    {
        if (!simpleTargetValidators.Contains(validator))
        {
            simpleTargetValidators.Add(validator);
            validator.SetQuestionManager(this);
        }
    }
    
    public void RegisterTargetValidator(TargetValidator validator)
    {
        if (!targetValidators.Contains(validator))
        {
            targetValidators.Add(validator);
            validator.SetQuestionManager(this);
        }
    }
    
    [ContextMenu("Show Random Question")]
    public void ForceNewQuestion()
    {
        ShowRandomQuestion();
    }
}
