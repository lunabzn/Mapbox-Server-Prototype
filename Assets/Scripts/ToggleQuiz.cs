using UnityEngine;
using UnityEngine.UI;

public class ToggleQuiz : MonoBehaviour
{
    public Toggle[] toggles;
    public Text scoreText; // Referencia al texto donde mostrar la puntuaci�n
    public int previousScore;

    void Start()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].onValueChanged.AddListener(delegate { ToggleValueChanged(); });
        }

        // Recuperar la puntuaci�n anterior y mostrarla al iniciar la escena
        int previousScore = PlayerPrefs.GetInt("Score", 0);
        scoreText.text = "Puntuaci�n: " + previousScore;
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

        // Mostrar la puntuaci�n en el texto
        scoreText.text = "Puntuaci�n: " + score;

        // Sumar la nueva puntuaci�n a la puntuaci�n anterior y guardarla en PlayerPrefs
        previousScore = PlayerPrefs.GetInt("Score", 0);
        int newScore = previousScore + score;
        PlayerPrefs.SetInt("Score", newScore);
    }

    
}