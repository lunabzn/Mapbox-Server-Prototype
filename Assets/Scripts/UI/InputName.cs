using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputName : MonoBehaviour
{
    [SerializeField] private TMP_Text inputText;

    public void guardarNombre()
    {
        PlayerPrefs.SetString("nombreJugador", inputText.text);
        PlayerPrefs.Save();
        Debug.Log(PlayerPrefs.GetString("nombreJugador"));
    }

    public void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("nombreJugador");
    }
}
