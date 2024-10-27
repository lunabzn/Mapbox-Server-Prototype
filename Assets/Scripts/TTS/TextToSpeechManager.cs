using UnityEngine;
using System.Collections;
using UnityEngine.Android;

public class TextToSpeechManager : MonoBehaviour
{
    private AndroidJavaObject ttsObject;  // Objeto para manejar el TTS
    private bool isSpeaking = false;      // Estado para saber si el TTS está hablando

    void Start()
    {
        InitializeTextToSpeech();
    }

    // Método para inicializar el Text-to-Speech
    private void InitializeTextToSpeech()
    {
        // Verifica y solicita permiso de micrófono
        if (Application.platform == RuntimePlatform.Android && !Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        // Obtiene la actividad de Unity
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Crea el objeto TTS con un listener para saber cuándo está listo
            ttsObject = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new OnInitListener());
        }

    }

    // Clase interna para el listener de inicialización
    private class OnInitListener : AndroidJavaProxy
    {
        public OnInitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }

        // Este método es llamado cuando el Text-to-Speech está listo
        public void onInit(int status)
        {
            if (status == 0)  // TextToSpeech.SUCCESS
            {
                Debug.Log("Text-to-Speech inicializado correctamente");
            }
            else
            {
                Debug.LogError("Error al inicializar Text-to-Speech");
            }
        }
    }

    // Método para reproducir un texto
    public void Speak(string text)
    {
        if (ttsObject != null && !isSpeaking)
        {
            // Se cxrea un bundle vacio
            AndroidJavaObject bundle = new AndroidJavaObject("android.os.Bundle");

            // Metodo speak reproduce el texto
            ttsObject.Call<int>("speak", text, 0, bundle, null);
            isSpeaking = true; //TTS está hablando
        }
        else
        {
            Debug.LogError("El objeto Text-to-Speech no está inicializado o ya está hablando.");
        }
    }

    // Método para pausar el texto
    public void PauseSpeech()
    {
        if (ttsObject != null && isSpeaking)
        {
            ttsObject.Call<int>("stop");  // Detiene el TTS actual
            isSpeaking = false;      //Ha dejado de hablar
        }
        else
        {
            Debug.LogWarning("No hay ninguna reproducción de TTS en curso para pausar.");
        }
    }

    
}
