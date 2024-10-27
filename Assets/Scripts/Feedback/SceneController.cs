using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    private DateTime activityStartTime;
    private bool isTimerRunning = false;
    public string currentActivityName;

    // Diccionario para almacenar el mapeo de IDs y nombres de eventos
    private Dictionary<int, string> eventDictionary;
    // Lista de eventos completados
    private HashSet<int> completedEvents;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para que no se destruya al cambiar de escena 
            
            // Inicialización del diccionario con los eventos y sus nombres
            eventDictionary = new Dictionary<int, string>
            {
                { 1, "EventoCharla" },
                { 2, "EventoAulario 1" },
                { 3, "EventoCafeteria" },
                { 4, "EventoSecretaría" },
                { 5, "EventoPingPong" },
                { 6, "EventoBiblioteca" },
                { 7, "EventoBiblioteca" }
            };

            // Inicializar la lista de eventos completados
            completedEvents = new HashSet<int>();
            // Cargar eventos completados desde PlayerPrefs
            LoadCompletedEvents();

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
            int eventID = GetEventID(scene.name);
            currentActivityName = scene.name;
            Debug.Log("estamos en actividad");
            StartTimer();
        }
        else if (scene.name == "FeedbackScene")
        {
            Debug.Log("salimos de actividad");
            StopTimer();
        }
        else if (scene.name == "Location-basedGame")
        {
            ResetTimer();
        }
    }

    // Marcar un evento como completado
    public void MarkEventAsCompleted(int eventID)
    {
        if (!completedEvents.Contains(eventID))
        {
            completedEvents.Add(eventID);
        }
        SaveCompletedEvents();
    }

    // Verificar si un evento está completado
    public bool IsEventCompleted(int eventID)
    {
        Debug.Log("IsEventCompleted llamado con eventID: " + eventID);

        if (completedEvents == null)
        {
            Debug.LogError("completedEvents es null en SceneController");
            return false;
        }

        bool isCompleted = completedEvents.Contains(eventID);
        Debug.Log("¿Evento completado?: " + isCompleted);
        return isCompleted;
    }

    // Método para obtener el nombre del evento a partir de su ID
    public string GetEventName(int eventID)
    {
        if (eventDictionary.TryGetValue(eventID, out string eventName))
        {
            return eventName;
        }
        return "Evento desconocido";
    }

    // Método para obtener el ID del evento a partir del nombre de la escena
    public int GetEventID(string sceneName)
    {
        // Aquí podrías ajustar esta lógica si los nombres de escena no siguen un patrón claro
        foreach (var entry in eventDictionary)
        {
            if (sceneName.Contains(entry.Value.Replace(" ", string.Empty))) // Por ejemplo, compara la escena sin espacios
            {
                return entry.Key;
            }
        }
        return -1; // Si no se encuentra el evento
    }

    // Guardar los eventos completados en PlayerPrefs
    private void SaveCompletedEvents()
    {
        foreach (int eventID in completedEvents)
        {
            PlayerPrefs.SetInt("EventCompleted_" + eventID, 1); // 1 significa completado
        }
        PlayerPrefs.Save(); // Asegura que los datos se guarden
    }

    // Cargar los eventos completados desde PlayerPrefs
    private void LoadCompletedEvents()
    {
        foreach (int eventID in eventDictionary.Keys)
        {
            if (PlayerPrefs.GetInt("EventCompleted_" + eventID, 0) == 1) // 1 significa completado
            {
                completedEvents.Add(eventID);
            }
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
