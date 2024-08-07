using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class LocationManager : MonoBehaviour
{
    public static LocationProximity.Location currentLocation;
    public TextMeshProUGUI locationText;
    public float proximityRadius = 1000.0f; // Raio de proximidade em metros
    private List<LocationProximity.Location> locations;
    private bool locationServiceEnabled = false;
    private bool isRunningOnPC = false;
    private string currentLoadedScene = ""; // Variável para armazenar a cena atualmente carregada

    void Start()
    {
        locations = LocationJSONReader.ReadLocationDataFromJSON();

        // Verificar se está rodando no editor Unity (PC)
        isRunningOnPC = Application.isEditor && !Application.isMobilePlatform;

        // Iniciar serviço de localização ou usar coordenadas fixas
        if (isRunningOnPC)
        {
            // Rodando no editor, utilizar coordenadas fixas
            currentLocation = GetClosestLocationFixed();
            UpdateUI();
        }
        else
        {
            // Rodando em dispositivo móvel, iniciar serviço de localização
            StartCoroutine(StartLocationService());
        }
    }

    IEnumerator StartLocationService()
    {
        // Solicitar permissão de localização
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(1);
        }

        // Primeiro, verifique se a localização é ativada
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Serviço de localização não está habilitado pelo usuário.");
            UseFixedCoordinates();
            yield break;
        }

        // Inicie a obtenção da localização
        Input.location.Start();

        // Aguarde até que a localização seja inicializada
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Se não foi possível inicializar a localização em 20 segundos, aborte
        if (maxWait <= 0)
        {
            Debug.Log("Tempo de inicialização do serviço de localização excedido.");
            UseFixedCoordinates();
            yield break;
        }

        // Verifique se obtivemos uma conexão válida
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Não foi possível determinar a localização do dispositivo.");
            UseFixedCoordinates();
            yield break;
        }
        else
        {
            // Serviço de localização está habilitado
            locationServiceEnabled = true;
        }
    }

    void Update()
    {
        if (!locationServiceEnabled || isRunningOnPC)
        {
            // Se o serviço de localização não estiver habilitado ou estiver rodando no editor (PC), use coordenadas fixas
            return;
        }

        // Pegar as coordenadas atuais do usuário
        float currentLatitude = Input.location.lastData.latitude;
        float currentLongitude = Input.location.lastData.longitude;

        LocationProximity.Coordinates coord = new LocationProximity.Coordinates { latitude = currentLatitude, longitude = currentLongitude };

        currentLocation = ReturnClosestLocation(coord);
        UpdateUI();
    }

    private LocationProximity.Location GetClosestLocationFixed()
    {
        // Coordenadas fixas para uso no editor Unity (PC)
        float fixedLatitude = -27.5838f;
        float fixedLongitude = -54.7771f;

        LocationProximity.Coordinates fixedCoord = new LocationProximity.Coordinates { latitude = fixedLatitude, longitude = fixedLongitude };

        return ReturnClosestLocation(fixedCoord);
    }

    private LocationProximity.Location ReturnClosestLocation(LocationProximity.Coordinates coord)
    {
        LocationProximity.Location closestLocation = locations[0];
        float lowestDistance = float.MaxValue;

        foreach (var loc in locations)
        {
            float currentDistance = CalculateDistance(
                loc.coordinates.latitude, loc.coordinates.longitude, coord.latitude, coord.longitude
            );

            if (currentDistance < lowestDistance)
            {
                lowestDistance = currentDistance;
                closestLocation = loc;
            }
        }

        return closestLocation;
    }

    private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371e3f; // Raio da Terra em metros
        float φ1 = lat1 * Mathf.Deg2Rad;
        float φ2 = lat2 * Mathf.Deg2Rad;
        float Δφ = (lat2 - lat1) * Mathf.Deg2Rad;
        float Δλ = (lon2 - lon1) * Mathf.Deg2Rad;

        float a = Mathf.Sin(Δφ / 2) * Mathf.Sin(Δφ / 2) +
                  Mathf.Cos(φ1) * Mathf.Cos(φ2) *
                  Mathf.Sin(Δλ / 2) * Mathf.Sin(Δλ / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        float distance = R * c;
        return distance;
    }

    private void UpdateUI()
    {
        if (currentLocation != null)
        {
            locationText.text = "Região: " + currentLocation.locationName;

            // Verifica o nome da localização para carregar a cena correspondente
            string locationName = currentLocation.locationName.ToLower();
            string targetScene = "";

            if (locationName.Contains("rio"))
            {
                // Cena do Rio
                targetScene = "RiverScene";
            }
            else if (locationName.Contains("laguna") || locationName.Contains("lago"))
            {
                // Cena do Lago
                targetScene = "LakeScene";
            }
            else if (locationName.Contains("praia") || locationName.Contains("beach"))
            {
                // Cena da Praia
                targetScene = "BeachScene";
            }

            // Carregar a cena apenas se não estiver carregada
            if (!string.IsNullOrEmpty(targetScene) && SceneManager.GetActiveScene().name != targetScene)
            {
                SceneManager.LoadScene(targetScene);
                currentLoadedScene = targetScene;
            }
        }
    }

    private void UseFixedCoordinates()
    {
        // Coordenadas fixas para simulação quando o serviço de localização não está habilitado
        // Escolha as coordenadas fixas apropriadas para o seu caso
        currentLocation = new LocationProximity.Location
        {
            locationName = "Coordenadas Fixas",
            type = "Fixas",
            region = "Fixas",
            fish = "Fixas",
            coordinates = new LocationProximity.Coordinates
            {
                latitude = -29.7871f,
                longitude = -50.0800f
            }
        };
    }
}


