using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;
//using UnityEditor.PackageManager;

public class SubmitFeedbackServerRpc : NetworkBehaviour
{
    public static SubmitFeedbackServerRpc Instance;
    private static int unknownPlayerCounter = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendFeedbackServerRpc(FeedbackMessage feedbackMessage, ulong clientId)
    {
        if (IsServer)
        {
            // Si el nombre es "JugadorDesconocido", asignamos un número único
            if (feedbackMessage.playerName == "JugadorDesconocido")
            {
                feedbackMessage.playerName = AssignUniqueUnknownPlayerName();
                // Enviamos el nuevo nombre al cliente para que lo guarde en PlayerPrefs
                UpdatePlayerNameClientRpc(feedbackMessage.playerName, clientId);
            }

            // Mostrar los datos recibidos en la consola para depuración
            Debug.Log("Feedback recibido:");
            Debug.Log("Nombre del jugador: " + feedbackMessage.playerName);
            Debug.Log("Nombre de la actividad: " + feedbackMessage.activityName);
            Debug.Log("Tiempo empleado: " + feedbackMessage.timeSpent + " segundos");
            Debug.Log("Estado de la actividad: " + feedbackMessage.activityStatus);
            Debug.Log("Razón para no terminar: " + feedbackMessage.reasonForNotFinishing);

            // Mostrar emociones recibidas
            if (feedbackMessage.emotions != null)
            {
                foreach (var emotion in feedbackMessage.emotions)
                {
                    Debug.Log("Emoción: " + emotion.emotionName + ", Intensidad: " + emotion.intensity);
                }
            }

            // Mostrar respuestas del checklist
            if (feedbackMessage.checklistResponses != null)
            {
                Debug.Log("Respuestas del checklist:");
                foreach (var response in feedbackMessage.checklistResponses)
                {
                    Debug.Log("Checklist item: " + response);
                }
            }

            // Guardar el feedback en un archivo
            SaveFeedback(feedbackMessage);
        }
    }
    private string AssignUniqueUnknownPlayerName()
    {
        string uniqueName = "JugadorDesconocido" + unknownPlayerCounter;
        unknownPlayerCounter++; // Incrementar el contador global para el próximo jugador desconocido
        return uniqueName;
    }

    // Esta ClientRpc enviará el nombre asignado de vuelta al cliente para que lo guarde
    [ClientRpc]
    private void UpdatePlayerNameClientRpc(string newPlayerName, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Guardar el nuevo nombre en PlayerPrefs
            PlayerPrefs.SetString("nombreJugador", newPlayerName);
            PlayerPrefs.Save();
            Debug.Log("Nombre del jugador actualizado a: " + newPlayerName);
        }
    }

    void SaveFeedback(FeedbackMessage feedbackMessage)
    {
        try
        {
            // Genera un nombre de archivo único para cada jugador
            string fileName = $"{feedbackMessage.playerName}_feedback.json";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            Debug.Log("Ruta de almacenamiento: " + filePath);
            FeedbackList feedbackList = new FeedbackList();

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Debug.Log("Contenido actual del JSON: " + json);
                feedbackList = JsonUtility.FromJson<FeedbackList>(json);
            }
            else
            {
                Debug.Log("Archivo JSON no existe, creando uno nuevo.");
            }

            // Añadir feedback recibido a la lista
            feedbackList.feedbacks.Add(feedbackMessage);
            Debug.Log("Feedback añadido: " + JsonUtility.ToJson(feedbackMessage, true));

            // Serializar la lista actualizada a JSON
            string updatedJson = JsonUtility.ToJson(feedbackList, true);
            Debug.Log("Nuevo contenido del JSON: " + updatedJson);

            // Escribir el JSON actualizado en el archivo
            File.WriteAllText(filePath, updatedJson);
            Debug.Log("Feedback guardado correctamente en " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al guardar el feedback: " + ex.Message);
        }
    }
}

[System.Serializable]
public class FeedbackList
{
    public List<FeedbackMessage> feedbacks = new List<FeedbackMessage>();
}

[System.Serializable]
public class FeedbackMessage : INetworkSerializable
{
    public string playerName;
    public string activityName;
    public double timeSpent;

    // Respuestas del primer panel
    public string activityStatus;  // Terminada, no terminada, no empezada

    // Respuestas del segundo panel (si no la terminó o no la empezó)
    public string reasonForNotFinishing;  // Por qué no la terminó
    public Emotion[] emotions = new Emotion[0];  // Inicializa como array vacío por defecto

    // Array de checklist respuestas
    public string[] checklistResponses = new string[0];  // Inicializa como array vacío por defecto

    // Campo de fecha y hora para cada feedback
    public string feedbackTimestamp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref feedbackTimestamp);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref activityName);
        serializer.SerializeValue(ref timeSpent);
        serializer.SerializeValue(ref activityStatus);
        serializer.SerializeValue(ref reasonForNotFinishing);

        // Serializar el array de emociones
        int emotionCount = emotions.Length;
        serializer.SerializeValue(ref emotionCount);
        if (serializer.IsReader)
        {
            emotions = new Emotion[emotionCount];
        }
        for (int i = 0; i < emotionCount; i++)
        {
            if (serializer.IsReader)
            {
                emotions[i] = new Emotion(); // Inicializa el objeto Emotion si es necesario
            }
            serializer.SerializeValue(ref emotions[i].emotionName);
            serializer.SerializeValue(ref emotions[i].intensity);
        }

        // Serializar el array de checklist
        int checklistCount = checklistResponses.Length;
        serializer.SerializeValue(ref checklistCount);
        if (serializer.IsReader)
        {
            checklistResponses = new string[checklistCount];
        }
        for (int i = 0; i < checklistCount; i++)
        {
            serializer.SerializeValue(ref checklistResponses[i]);
        }

    }

}

[System.Serializable]
public class Emotion
{
    public string emotionName;
    public float intensity;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref emotionName);
        serializer.SerializeValue(ref intensity);
    }
}
