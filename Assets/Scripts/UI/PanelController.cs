using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    public GameObject[] panels; // Array con todos los paneles de la escena
    public GameObject firstPanel; // Panel activo al inicio

    void Start()
    {
        ResetPanels();
    }

    // Este m�todo desactiva todos los paneles excepto el primero
    void ResetPanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        firstPanel.SetActive(true); 
    }

    // M�todo para regresar a la escena inicial y resetear los paneles
    public void ReturnToInitialScene()
    {
        SceneManager.LoadScene("Location-basedGame");
    }
}
