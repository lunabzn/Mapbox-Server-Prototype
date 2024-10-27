using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TTSPanelManager : MonoBehaviour
{
    public Sprite altavozSprite;  // Sprite del altavoz
    public Sprite pausaSprite;    // Sprite de pausa
    public TextToSpeechManager ttsManager; // Referencia al script que maneja el TTS
    public TextMeshProUGUI[] textos; // Arreglo de todos los textos en la escena

    public Image buttonImage;
    private bool isAltavoz = true;  // Estado actual del botón
    private string currentText = ""; // Texto que se está reproduciendo

    void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = altavozSprite; // Iniciar con el sprite de altavoz
    }
    void Update()
    {
        CheckActiveText(); // Verifica si el texto activo sigue visible
    }

    // Método para cambiar entre los sprites y controlar el TTS
    public void ToggleSpriteAndTTS()
    {
        if (isAltavoz)
        {
            // Cambia al sprite de pausa y comienza a reproducir el TTS
            buttonImage.sprite = pausaSprite;
            SpeakActiveText(); // Llama al método para hablar el texto activo
        }
        else
        {
            // Cambia al sprite de altavoz y pausa el TTS
            buttonImage.sprite = altavozSprite;
            ttsManager.PauseSpeech(); // Pausa el TTS
            currentText = ""; // Restablece el texto actual
        }

        isAltavoz = !isAltavoz;
    }

    // Método para recorrer los textos y hablar el primero que esté activo
    private void SpeakActiveText()
    {
        foreach (TextMeshProUGUI texto in textos)
        {
            if (texto.gameObject.activeInHierarchy) // Verifica si el objeto del texto está activo
            {
                string textToSpeak = texto.text;

                if (!string.IsNullOrEmpty(textToSpeak))
                {
                    // Si hay un texto en reproducción y el texto actual no es el mismo, pausa el TTS
                    if (!string.IsNullOrEmpty(currentText) && currentText != textToSpeak)
                    {
                        ttsManager.PauseSpeech();
                    }

                    // Asigna el texto actual y llama al TTS
                    currentText = textToSpeak;
                    ttsManager.Speak(textToSpeak); // Llama al Text-to-Speech para leer el texto
                    return; // Si ya encontró un texto activo, no sigue buscando más
                }
            }
        }

        Debug.LogWarning("No hay ningún texto activo para reproducir.");
    }

    // Método para pausar la lectura si el texto se desactiva
    public void CheckActiveText()
    {
        foreach (TextMeshProUGUI texto in textos)
        {
            if (!texto.gameObject.activeInHierarchy && texto.text == currentText)
            {
                ttsManager.PauseSpeech(); // Si el texto activo se desactiva, pausa el TTS
                buttonImage.sprite = altavozSprite;
                isAltavoz = true;
                currentText = ""; // Restablece el texto actual
                break; // Salir del bucle
            }
        }
    }
}
