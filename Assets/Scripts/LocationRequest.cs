using UnityEngine;
using UnityEngine.Android;

public class RequestLocationPermissionScript : MonoBehaviour
{
    internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    internal void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // No destruir este GameObject al cargar nuevas escenas.
    }

    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            // El usuario autorizó el uso de la ubicación.
        }
        else
        {
            bool useCallbacks = true;
            if (!useCallbacks)
            {
                // No tenemos permiso para usar la ubicación.
                // Solicitar permiso o proceder sin la funcionalidad habilitada.
                Permission.RequestUserPermission(Permission.FineLocation);
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
                Permission.RequestUserPermission(Permission.FineLocation, callbacks);
            }
        }
    }
}
