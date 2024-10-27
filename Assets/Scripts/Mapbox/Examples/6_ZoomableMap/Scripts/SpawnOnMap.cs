namespace Mapbox.Examples
{
    using UnityEngine;
    using Mapbox.Utils;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.MeshGeneration.Factories;
    using Mapbox.Unity.Utilities;
    using System.Collections.Generic;

    public class SpawnOnMap : MonoBehaviour
    {
        [SerializeField]
        AbstractMap _map;

        [SerializeField]
        [Geocode]
        string[] _locationStrings;
        Vector2d[] _locations;

        [SerializeField]
        float _spawnScale = 100f;

        [SerializeField] GameObject _markerPrefab; 
        [SerializeField] GameObject _firtsMarkerPrefab;

        List<GameObject> _spawnedObjects;
        public bool firstInteract = false;

        void Start()
        {
            _locations = new Vector2d[_locationStrings.Length];
            _spawnedObjects = new List<GameObject>();
            InstantiateFirstPin();

            if(PlayerPrefs.GetInt("primerEvento") == 1) { UnlockPins(); }

            LoadCompletedPins();
        }

        private void Update()
        {
            int count = _spawnedObjects.Count;
            for (int i = 0; i < count; i++)
            {
                var spawnedObject = _spawnedObjects[i];
                var location = _locations[i];
                spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
                spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            }
        }

        void InstantiateFirstPin()
        {
            var locationString = _locationStrings[0];
            _locations[0] = Conversions.StringToLatLon(locationString);
            var instance = Instantiate(_firtsMarkerPrefab);
            //copia localizacion de eventos en la variable de Event Controller
            instance.GetComponent<EventController>().eventLoc = _locations[0];
            instance.GetComponent<EventController>().eventID = 1;
            instance.transform.localPosition = _map.GeoToWorldPosition(_locations[0], true);
            instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            _spawnedObjects.Add(instance);
        }

        public void OnFirstPinInteract()
        {
            firstInteract = true;
            PlayerPrefs.SetInt("primerEvento", 1);
            UnlockPins();
        }

        void UnlockPins()
        {
            // Asegurarse de que SceneController esté disponible antes de usarlo
            if (SceneController.Instance == null)
            {
                Debug.LogError("SceneController no está disponible. Asegúrate de que esté en la escena.");
                return;
            }

            for (int i = 1; i < _locationStrings.Length; i++)
            {
                var locationString = _locationStrings[i];
                _locations[i] = Conversions.StringToLatLon(locationString);
                var instance = Instantiate(_markerPrefab);
                //copia localizacion de eventos en la variable de Event Controller
                instance.GetComponent<EventController>().eventLoc = _locations[i];
                instance.GetComponent<EventController>().eventID = i + 1;
                instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
                instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);

                // Comprobar si el evento está completado
                if (SceneController.Instance.IsEventCompleted(i) || PlayerPrefs.GetInt($"Event_{i}_Completed") == 1)
                {
                    ChangePinColor(instance);
                }

                _spawnedObjects.Add(instance);
            }

        }

        void ChangePinColor(GameObject pin)
        {
            var renderer = pin.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green; // Cambiar el color del pin a verde
            }
        }

        void LoadCompletedPins()
        {
            for (int i = 1; i < _locationStrings.Length; i++)
            {
                // Si el evento está guardado como completado, cambiar el color del pin
                if (PlayerPrefs.GetInt($"Event_{i}_Completed") == 1)
                {
                    ChangePinColor(_spawnedObjects[i]); 
                }
            }
        }

    }
}