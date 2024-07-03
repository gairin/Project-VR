using System.Collections.Generic;
using UnityEngine;

public static class LocationJSONReader
{
    public static List<LocationProximity.Location> ReadLocationDataFromJSON()
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("LocationData");
        if (jsonTextAsset == null)
        {
            Debug.LogError("Não foi possível carregar o arquivo JSON.");
            return new List<LocationProximity.Location>();
        }

        LocationListWrapper wrapper = JsonUtility.FromJson<LocationListWrapper>(jsonTextAsset.text);
        return wrapper.location;
    }

    [System.Serializable]
    public class LocationListWrapper
    {
        public List<LocationProximity.Location> location;
    }
}
