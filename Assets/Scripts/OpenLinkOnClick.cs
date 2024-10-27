using UnityEngine;

public class OpenLinkOnClick : MonoBehaviour
{
    string url = "https://gestion2.urjc.es/horarios/";
    public void OpenLink()
    {
        Application.OpenURL(url);
    }
}