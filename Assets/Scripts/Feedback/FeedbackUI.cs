using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class FeedbackUI : MonoBehaviour
{
    public Button siButton;
    public Button verguenzaButton;
    public Button olvidoButton;
    public Button miedoButton;
    public Button causasExternasButton;

    private static double timeSpent;
    private static string activityName;

    void Start()
    {
        siButton.onClick.AddListener(() => SendFeedback("Sí"));
        verguenzaButton.onClick.AddListener(() => SendFeedback("Me dio vergüenza"));
        olvidoButton.onClick.AddListener(() => SendFeedback("Se me olvidó como se hacía"));
        miedoButton.onClick.AddListener(() => SendFeedback("Me dio miedo"));
        causasExternasButton.onClick.AddListener(() => SendFeedback("Causas externas"));
    }

    public static void SetActivityData(string name, double time)
    {
        activityName = name;
        timeSpent = time;
    }

    void SendFeedback(string feedback)
    {
        FeedbackMessage feedbackMessage = new FeedbackMessage
        {
            feedback = feedback,
            activityName = activityName,
            timeSpent = timeSpent
        };

        if (SubmitFeedbackServerRpc.Instance != null)
        {
            Debug.Log("Enviando feedback: " + JsonUtility.ToJson(feedbackMessage, true));
            SubmitFeedbackServerRpc.Instance.SendFeedbackServerRpc(feedbackMessage);
        }
        else
        {
            Debug.LogError("SubmitFeedbackServerRpc.Instance is null. Make sure it is initialized properly.");
        }
    }
}
