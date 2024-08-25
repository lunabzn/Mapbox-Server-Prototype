using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    private DateTime activityStartTime;
    private bool isTimerRunning = false;
    private string currentActivityName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para que no se destruya al cambiar de escena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("Evento"))
        {
            currentActivityName = scene.name;
            Debug.Log("estamos en actividad");
            StartTimer();
        }
        else if (scene.name == "FeedbackScene")
        {
            Debug.Log("salimos en actividad");
            StopTimer();
        }
        else if (scene.name == "Location-basedGame")
        {
            ResetTimer();
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void StartTimer()
    {
        activityStartTime = DateTime.Now;
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        if (isTimerRunning)
        {
            TimeSpan timeSpent = DateTime.Now - activityStartTime;
            isTimerRunning = false;
            FeedbackUI.SetActivityData(currentActivityName, timeSpent.TotalSeconds);
            Debug.Log("Timer stopped. Activity: " + currentActivityName);
            Debug.Log("Time spent: " + timeSpent.TotalSeconds + " seconds");
        }
    }

    public void ResetTimer()
    {
        isTimerRunning = false;
        activityStartTime = DateTime.MinValue;
    }

    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }
}
