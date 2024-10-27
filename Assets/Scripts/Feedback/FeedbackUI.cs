using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class FeedbackUI : MonoBehaviour
{
    // Panel de si terminó la actividad
    public Button termineButton;
    public Button noEmpeceButton;
    public Button empeceNoTermineButton;

    // Panel de razones (si no terminó la actividad)
    public GameObject panelRazones;
    public Button verguenzaButton;
    public Button olvidoButton;
    public Button miedoButton;
    public Button causasExternasButton;

    // Emociones
    public Slider felicidadSlider;
    public Slider tristezaSlider;
    public Slider enfadoSlider;
    public Slider ansiedadSlider;

    // Checklist de opciones
    public Toggle toggleOpcion1;
    public Toggle toggleOpcion2;
    public Toggle toggleOpcion3;
    public Toggle toggleOpcion4;

    //input checklist
    [SerializeField] public TMP_Text personalizedInput;

    // Panel de emociones y checklist
    public GameObject panelEmociones;

    private static double timeSpent;
    private static string activityName;
    private static string activityStat;

    public FeedbackMessage completeFeedbackMessage;

    void Start()
    {
        // Asociamos los botones del primer panel
        termineButton.onClick.AddListener(() => SetActivityStatus("Terminé"));
        noEmpeceButton.onClick.AddListener(() => SetActivityStatus("No empecé"));
        empeceNoTermineButton.onClick.AddListener(() => SetActivityStatus("Empecé pero no terminé"));

        // Asociamos los botones del panel de razones
        verguenzaButton.onClick.AddListener(() => SetReasonForNotFinishing("Me dio vergüenza"));
        olvidoButton.onClick.AddListener(() => SetReasonForNotFinishing("Se me olvidó cómo se hacía"));
        miedoButton.onClick.AddListener(() => SetReasonForNotFinishing("Me dio miedo"));
        causasExternasButton.onClick.AddListener(() => SetReasonForNotFinishing("Causas externas"));
    }

    public static void SetActivityData(string name, double time)
    {
        activityName = name;
        timeSpent = time;
    }

    // Se ejecuta cuando seleccionan si terminaron o no la actividad
    void SetActivityStatus(string status)
    {
        completeFeedbackMessage.activityStatus = status;
        activityStat = status;
        if (status == "Terminé")
        {
            // Si terminó la actividad, pasa directo al panel de emociones y checklist
            panelEmociones.SetActive(true);
        }
        else
        {
            // Si no terminó, activa el panel de razones
            panelRazones.SetActive(true);
        }
    }

    // Se ejecuta cuando seleccionan una razón por no haber terminado
    public void SetReasonForNotFinishing(string reason)
    {
        completeFeedbackMessage.reasonForNotFinishing = reason;

        // Después de seleccionar la razón, mostrar el panel de emociones y checklist
        panelRazones.SetActive(false);
        panelEmociones.SetActive(true);
    }

    // Recopilamos las emociones del panel de emociones
    void CollectEmotionsFromUI()
    {
        Emotion[] emotions = new Emotion[4];

        emotions[0] = new Emotion { emotionName = "Felicidad", intensity = felicidadSlider.value };
        emotions[1] = new Emotion { emotionName = "Tristeza", intensity = tristezaSlider.value };
        emotions[2] = new Emotion { emotionName = "Enfado", intensity = enfadoSlider.value };
        emotions[3] = new Emotion { emotionName = "Ansiedad", intensity = ansiedadSlider.value };

        completeFeedbackMessage.emotions = emotions;
    }

    // Recopilamos el checklist del panel de emociones
    void CollectChecklistFromUI()
    {
        List<string> checklistResponses = new List<string>();

        if (toggleOpcion1.isOn) checklistResponses.Add("no supe hablar con mi compañero");
        if (toggleOpcion2.isOn) checklistResponses.Add("me dio miedo que se rieran de mi");
        if (toggleOpcion3.isOn) checklistResponses.Add("me puse nervioso porque falta de habilidad en concreto");
        if (toggleOpcion4.isOn) checklistResponses.Add("me molestó la parte sensorial");

        // Añadir respuesta personalizada si el input no está vacío
        if (!string.IsNullOrEmpty(personalizedInput.text))
        {
            checklistResponses.Add(personalizedInput.text);
        }

        completeFeedbackMessage.checklistResponses = checklistResponses.ToArray();
    }

    public void personlizedChecklist()
    {
        // Añade la respuesta personalizada directamente desde el InputField
        string response = personalizedInput.text;

    }

    // Este método se ejecuta para enviar todo el feedback
    public void SendFeedback()
    {
        string playerName = PlayerPrefs.GetString("nombreJugador", "JugadorDesconocido");

        // Añadir marca de tiempo (fecha y hora actual)
        completeFeedbackMessage.feedbackTimestamp = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

        completeFeedbackMessage.playerName = playerName;
        completeFeedbackMessage.activityName = activityName;
        completeFeedbackMessage.timeSpent = timeSpent;

        CollectEmotionsFromUI();
        CollectChecklistFromUI();

        if(timeSpent <= 600 && (activityStat == "Terminé"))
        {
            PlayerPrefs.SetInt("UnderTime", 1);
        }

        if (SubmitFeedbackServerRpc.Instance != null)
        {
            Debug.Log("Enviando feedback completo: " + JsonUtility.ToJson(completeFeedbackMessage, true));
            SubmitFeedbackServerRpc.Instance.SendFeedbackServerRpc(completeFeedbackMessage, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            Debug.LogError("SubmitFeedbackServerRpc.Instance is null. Make sure it is initialized properly.");
        }
    }

    public void EndFeedback()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            EventUIManager eventManager = canvas.GetComponent<EventUIManager>();
            if (eventManager != null)
            {
                if(activityStat == "Terminé")
                {
                    eventManager.ScanEvent();
                }
                else
                {
                    eventManager.panels[4].SetActive(true);
                }
            }
        }
    }
}

