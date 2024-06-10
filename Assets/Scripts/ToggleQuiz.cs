using UnityEngine;
using UnityEngine.UI;

public class ToggleQuiz : MonoBehaviour
{
    public Toggle[] toggles;
    public Text scoreText; // Referencia al texto donde mostrar la puntuación
    public int previousScore;

    void Start()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].onValueChanged.AddListener(delegate { ToggleValueChanged(); });
        }

        // Recuperar la puntuación anterior y mostrarla al iniciar la escena
        int previousScore = PlayerPrefs.GetInt("Score", 0);
        scoreText.text = "Puntuación: " + previousScore;
    }

    void Awake()
    {
        previousScore = 0;
    }

    void ToggleValueChanged()
    {
        int score = 0;

        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                score++;
            }
        }

        // Mostrar la puntuación en el texto
        scoreText.text = "Puntuación: " + score;

        // Sumar la nueva puntuación a la puntuación anterior y guardarla en PlayerPrefs
        previousScore = PlayerPrefs.GetInt("Score", 0);
        int newScore = previousScore + score;
        PlayerPrefs.SetInt("Score", newScore);
    }

    
}