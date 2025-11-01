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

    [Header("3D Model Feedback (Optional)")]
    [SerializeField] private GameObject correctModel3D;
    [SerializeField] private GameObject incorrectModel3D;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float modelDistance = 1.5f;

    private Question currentQuestion;
    private bool waitingForAnswer = false;
    private List<TargetValidator> targetValidators = new List<TargetValidator>();

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

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        if (questionPanel) questionPanel.SetActive(false);
        if (feedbackPanel) feedbackPanel.SetActive(false);

        if (correctModel3D) correctModel3D.SetActive(false);
        if (incorrectModel3D) incorrectModel3D.SetActive(false);

        PrepareModelsForWorldSpace();

        if (IsSystemReady())
        {
            StartCoroutine(StartFirstQuestionAfterDelay());
        }
    }

    private void PrepareModelsForWorldSpace()
    {
        if (correctModel3D != null)
        {
            if (correctModel3D.transform.parent != null && correctModel3D.transform.parent.GetComponent<RectTransform>() != null)
            {
                correctModel3D.transform.SetParent(null);
            }
        }

        if (incorrectModel3D != null)
        {
            if (incorrectModel3D.transform.parent != null && incorrectModel3D.transform.parent.GetComponent<RectTransform>() != null)
            {
                incorrectModel3D.transform.SetParent(null);
            }
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
        string feedbackMessage = isCorrect ? "Correct!" : $"Incorrect. That's a: {detectedTarget}";

        if (feedbackText)
        {
            feedbackText.text = feedbackMessage;
        }

        feedbackPanel.SetActive(true);
        questionPanel.SetActive(false);

        if (correctModel3D) correctModel3D.SetActive(false);
        if (incorrectModel3D) incorrectModel3D.SetActive(false);

        if (isCorrect)
        {
            if (correctModel3D)
            {
                PositionModelInFrontOfCamera(correctModel3D);
                correctModel3D.SetActive(true);
            }
        }
        else
        {
            if (incorrectModel3D)
            {
                PositionModelInFrontOfCamera(incorrectModel3D);
                incorrectModel3D.SetActive(true);
            }
        }
    }

    private void PositionModelInFrontOfCamera(GameObject model)
    {
        if (cameraTransform == null || model == null) return;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 targetPosition = cameraTransform.position + (cameraForward * modelDistance);

        model.transform.position = targetPosition;

        model.transform.LookAt(cameraTransform);
        model.transform.Rotate(0, 180, 0);
    }

    IEnumerator ShowNextQuestionAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);

        if (correctModel3D) correctModel3D.SetActive(false);
        if (incorrectModel3D) incorrectModel3D.SetActive(false);

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
