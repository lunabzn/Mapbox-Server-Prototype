using UnityEngine;
using UnityEngine.UI;

public class ButtonTTSManager : MonoBehaviour
{
    public Sprite altavozSprite;  // Sprite del altavoz
    public Sprite pausaSprite;    // Sprite de pausa

    private Image buttonImage;    
    private bool isAltavoz = true;  // Estado actual del bot�n

    void Start()
    {        
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = altavozSprite;
    }

    // M�todo para cambiar entre los sprites
    public void ToggleSprite()
    {
        if (isAltavoz)
        {
            buttonImage.sprite = pausaSprite;
        }
        else
        {
            buttonImage.sprite = altavozSprite;
        }
        
        isAltavoz = !isAltavoz;
    }
}
