using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.IO;

public class SubmitFeedbackServerRpc : NetworkBehaviour
{
    public static SubmitFeedbackServerRpc Instance;

    private void Awake()
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

    [ServerRpc(RequireOwnership = false)]
    public void SendFeedbackServerRpc(FeedbackMessage feedbackMessage)
    {
        if (IsServer)
        {
            Debug.Log("Feedback recibido: " + feedbackMessage.feedback);
            Debug.Log("Nombre de la actividad: " + feedbackMessage.activityName);
            Debug.Log("Tiempo empleado: " + feedbackMessage.timeSpent + " segundos");
            SaveFeedback(feedbackMessage);
        }
    }

    void SaveFeedback(FeedbackMessage feedbackMessage)
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, "feedback.json");
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
public struct FeedbackMessage : INetworkSerializable
{
    public string feedback;
    public string activityName;
    public double timeSpent;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref feedback);
        serializer.SerializeValue(ref activityName);
        serializer.SerializeValue(ref timeSpent);
    }
}
