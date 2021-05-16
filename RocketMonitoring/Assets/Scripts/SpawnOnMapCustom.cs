using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;

// vectors have only 1 value for now, 0 index only

public class SpawnOnMapCustom : MonoBehaviour
{
	[SerializeField]
	AbstractMap _map;

	[SerializeField]
	[Geocode]
	string[] _locationStrings;
	Vector2d[] _locations;
	// for smooth locations changes 
	Vector2d[] _diffValues;

	[SerializeField]
	float _spawnScale = 1f;

	[SerializeField]
	GameObject _markerPrefab;

	List<GameObject> _spawnedObjects;

	[Header("Added Parameters")]
	[SerializeField] Camera mapCamera;
	float baseScaleFactor = 50f;
	[SerializeField] float baseSetPeriod = 0.5f;
	float timeRemaining = 0f;


	void Start()
	{
		_locations = new Vector2d[_locationStrings.Length];
		_diffValues = new Vector2d[_locationStrings.Length];

		_spawnedObjects = new List<GameObject>();
		for (int i = 0; i < _locationStrings.Length; i++)
		{
			var locationString = _locationStrings[i];
			_locations[i] = Conversions.StringToLatLon(locationString);
			var instance = Instantiate(_markerPrefab);
			instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);
			instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
			_spawnedObjects.Add(instance);
		}
	}

	private void Update()
	{
        // set base positions every 0.5 secs
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = baseSetPeriod;
            SetBasePosition();
        }

		// set new base location smoothly
		_locations[0] += (_diffValues[0] * Time.deltaTime / baseSetPeriod);

        // set proper scale for markers on map, 
        float currentScale = mapCamera.transform.position.y / baseScaleFactor;
		_spawnScale = currentScale;


		// set location for markers
		int count = _spawnedObjects.Count;
		for (int i = 0; i < count; i++)
		{
			var spawnedObject = _spawnedObjects[i];
			var location = _locations[i];
			spawnedObject.transform.localPosition = _map.GeoToWorldPosition(location, true);
			spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
		}
	}

	// assign random lat long periodically
	void SetBasePosition()
    {
		// 37.05706, 35.36111, around this point
		// create location string and set to the locations
		var latitude = Random.Range(37.05681f, 37.05731f).ToString().Replace(',', '.');
		var longitude = Random.Range(35.36076f, 35.36146f).ToString().Replace(',', '.');

		Vector2d nextLocation = Conversions.StringToLatLon(latitude + "," + longitude);
		_diffValues[0] = nextLocation - _locations[0];
	}
}
