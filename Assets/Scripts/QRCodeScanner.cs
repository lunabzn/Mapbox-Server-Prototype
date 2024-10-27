using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class QRCodeScanner : MonoBehaviour
{
    [SerializeField] private RawImage background;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private RectTransform scanZone;

    private bool isCamAvailable;
    private WebCamTexture cameraTexture;

    // Start is called before the first frame update
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        SetUpCamera();
    }

    // Update is called once per frame
    void Update()
    {
        CameraUpdate();
    }

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if(devices.Length == 0 ) 
        { 
            isCamAvailable = false;
            return;
        }
        for ( int i = 0; i < devices.Length; i++ )
        {
            if (devices[i].isFrontFacing == false )
            {
                cameraTexture = new WebCamTexture(devices[i].name,(int) scanZone.rect.width,(int) scanZone.rect.height);
            }
        }

        if(cameraTexture!=null)
        {
            cameraTexture.Play();
            background.texture = cameraTexture;
            isCamAvailable = true;
        }

    }

    private void CameraUpdate()
    {
        if(isCamAvailable==false) { return; }
        float ratio = (float) cameraTexture.width / (float) cameraTexture.height;
        aspectRatioFitter.aspectRatio = ratio;  //coge toda la escena no solo una parte del movil

        int orientation = -cameraTexture.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0,0,orientation);
    }

    public void OnClickScan()
    {
        Scan();
    }

    private void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(cameraTexture.GetPixels32(), cameraTexture.width, cameraTexture.height);
            if(result != null )
            {
                outputText.text = result.Text;
                
                // Si el escaneo es correcto, notificar al SceneController
                OnQRCodeScannedSuccessfully();
            }
            else
            {
                outputText.text = "FAILED TO READ QR CODE";
            }
        }
        catch
        {
            outputText.text = "FAILED TRY";
        }
    }

    private void OnQRCodeScannedSuccessfully()
    {      
        Debug.LogError("OnQRCodeScannedSuccessfully ejecutado correctamente");

        // Obtener el ID del evento actual desde el SceneController
        int eventID = SceneController.Instance.GetEventID(SceneController.Instance.currentActivityName);

        if (eventID != -1)
        {
            // Marcar el evento como completado en el SceneController
            SceneController.Instance.MarkEventAsCompleted(eventID);

            Debug.Log("Evento " + eventID + " completado.");
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            ScoreManager scoreManager = canvas.GetComponent<ScoreManager>();
            if (scoreManager != null)
            {
                // Llamar al método AddFullScore de ScoreManager
                scoreManager.AddFullScore();
            }
        }

        // Cargar la escena de feedback (o la escena que prefieras después del escaneo)
        SceneController.Instance.LoadScene("Location-basedGame");
    }


}
