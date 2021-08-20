using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;

public enum ZoomAction{
	ZoomIn,
	ZoomOut
}

public class SpawnOnMapCustom : MonoBehaviour
{
	public static SpawnOnMapCustom instance;

	[SerializeField]
	AbstractMap _map;

	[SerializeField]
	[Geocode]
	string[] _locationStrings;
	Vector2d[] _locations;

	[SerializeField]
	float _spawnScale = 1f;

	[SerializeField]
	GameObject[] _markerPrefabs;

	List<GameObject> _spawnedObjects;

	[Header("Map Camera Parameters")]
	[SerializeField] Camera _mapCamera;
	[SerializeField] float _initialCameraSetTime = 2f;
	Vector3 _cameraTargetPos;
	Vector3 _cameraDiff;
	bool _setCameraToBase = false;
	bool _setCameraTarget = false;
	public float currentScale = 1f;

	[Header("Marker Parameters")]
	[SerializeField] float _yPosition = 10f;
	[SerializeField] float _markerScale = 1f;
	float _baseScaleFactor = 50f;
	Vector2d[] _diffValues;
	float _locationSetPeriod = 1f; // matches with data obtain period

	Vector2d nextBasePosition;
	Vector2d nextRocketPosition;
	Vector2d nextPayLoadPosition;
	Vector2d initialDiffBase, initialDiffRocket, initialDiffPayLoad;

	const float _WIDTH = 51.32f;
	const float _HEIGHT = 45.92f;
	float _prevDragX = 0f;
	float _prevDragZ = 0f;
	float _currentDragX = 0f;
	float _currentDragZ = 0f;

	[Header("Scroll Wheel Sensitivity")]
	[SerializeField] float sensitivityValue = 10f;

	// cam boundary offset values, initial value at scale 1
	// world position, diff values from camera center
	float camBoundLeft_Base = -31.4f;
	float camBoundRight_Base = 31.2f;
	float camBoundDown_Base = -28.3f;
	float camBoundUp_Base = 27.8f;
	float scaleOffSetX_Base = 6f;
	float scaleOffSetY_Base = 3f;
	float camBoundLeft_Current, camBoundRight_Current;
	float camBoundDown_Current, camBoundUp_Current;
	float scaleOffSetX_Current;
	float scaleOffSetY_Current;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
	}

	void Start()
	{
		// get data from EntryManager
		_locationSetPeriod = EntryManager.dataObtainPeriod;

		// set proper scale for markers on map, 
		currentScale = _mapCamera.transform.position.y / _baseScaleFactor;
		_spawnScale = currentScale * _markerScale;

		_locations = new Vector2d[_locationStrings.Length];
		_diffValues = new Vector2d[_locationStrings.Length];

		_spawnedObjects = new List<GameObject>();
		for (int i = 0; i < _locationStrings.Length; i++)
		{
			var locationString = _locationStrings[i];
			_locations[i] = Conversions.StringToLatLon(locationString);
			var instance = Instantiate(_markerPrefabs[i]);

			Vector3 rawPosition = _map.GeoToWorldPosition(_locations[i], false);
			instance.transform.localPosition = new Vector3(rawPosition.x, _yPosition, rawPosition.z);
			instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
			_spawnedObjects.Add(instance);
		}

		// Set camera bounds for missing pointers
		camBoundLeft_Current = camBoundLeft_Base;
		camBoundRight_Current = camBoundRight_Base;
		camBoundDown_Current = camBoundDown_Base;
		camBoundUp_Current = camBoundUp_Base;
		scaleOffSetX_Current = scaleOffSetX_Base;
		scaleOffSetY_Current = scaleOffSetY_Base;
	}

	private void Update()
	{
		// SET INITIAL CAMERA POSITION ONCE -------------------------------------------------------------
		if(_setCameraToBase == false && _setCameraTarget == true)
        {
			_mapCamera.transform.position += (_cameraDiff * Time.deltaTime / _initialCameraSetTime);
			float currentX = _mapCamera.transform.position.x;
			float currentZ = _mapCamera.transform.position.z;
			if( (((_cameraTargetPos.x - currentX) * _cameraDiff.x) <= 0f) &&
				(((_cameraTargetPos.z - currentZ) * _cameraDiff.z) <= 0f) )
            {
				// stop here
				_mapCamera.transform.position = _cameraTargetPos;
				_setCameraToBase = true;
				LogManager.instance.SendMessageToLog("Map is loaded");
			}
        }
		// ------------------------------------------------------------------------------------------------

		// get scroll wheel data and assign to map camera y -----------------------------------------------
		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if(scrollWheel < 0f && DraggingMap.isMouseInRegion)
        {
			if (_mapCamera.transform.position.y < 400f)
				_mapCamera.transform.position += new Vector3(0f, sensitivityValue, 0f);
		}
		if(scrollWheel > 0f && DraggingMap.isMouseInRegion)
        {
			if (_mapCamera.transform.position.y > 50f)
				_mapCamera.transform.position -= new Vector3(0f, sensitivityValue, 0f);

		}
		// ------------------------------------------------------------------------------------------------

		// current camera scale
		currentScale = _mapCamera.transform.position.y / _baseScaleFactor;
		_spawnScale = currentScale * _markerScale;

		// MOVE CAMERA WITH MOUSE DRAG ---------------------------------------------------------------------
		if (DraggingMap.isDragging)
        {
			_prevDragX = _currentDragX;
			_currentDragX = DraggingMap.dragX;
			float diff1 = _prevDragX - _currentDragX;
			if(_prevDragX != 0f && _currentDragX != 0f)
				_mapCamera.transform.position += new Vector3(diff1 * currentScale * _WIDTH, 0f, 0f);

			_prevDragZ = _currentDragZ;
			_currentDragZ = DraggingMap.dragZ;
			float diff2 = _prevDragZ - _currentDragZ;
			if(_prevDragZ != 0f && _currentDragZ != 0f)
				_mapCamera.transform.position += new Vector3(0f, 0f, diff2 * currentScale * _HEIGHT);
		}
		else
        {
			_prevDragX = 0f;
			_currentDragX = 0f;
			_prevDragZ = 0f;
			_currentDragZ = 0f;
        }
		// -------------------------------------------------------------------------------------------------

		// update cam boundaries, left, right, down and up, offset values
		camBoundLeft_Current = camBoundLeft_Base * currentScale;
		camBoundRight_Current = camBoundRight_Base * currentScale;
		camBoundDown_Current = camBoundDown_Base * currentScale;
		camBoundUp_Current = camBoundUp_Base * currentScale;
		scaleOffSetX_Current = scaleOffSetX_Base * currentScale;
		scaleOffSetY_Current = scaleOffSetY_Base * currentScale;

		// location adding doesn't check if it reaches the target point, check it out
		// BASE POSITION SMOOTH UPDATE
		_locations[0] += (_diffValues[0] * Time.deltaTime / _locationSetPeriod);
		initialDiffBase -= (_diffValues[0] * Time.deltaTime / _locationSetPeriod);
		if((initialDiffBase.x * _diffValues[0].x <= 0f) && (initialDiffBase.y * _diffValues[0].y <= 0f))
        {
			_locations[0] = nextBasePosition;
        }

		// ROCKET POSITION SMOOTH UPDATE
		_locations[1] += (_diffValues[1] * Time.deltaTime / _locationSetPeriod);
		initialDiffRocket -= (_diffValues[1] * Time.deltaTime / _locationSetPeriod);
		if ((initialDiffRocket.x * _diffValues[1].x <= 0f) && (initialDiffRocket.y * _diffValues[1].y <= 0f))
		{
			_locations[1] = nextRocketPosition;
		}

		// PAYLOAD POSITION SMOOTH UPDATE
		_locations[2] += (_diffValues[2] * Time.deltaTime / _locationSetPeriod);
		initialDiffPayLoad -= (_diffValues[2] * Time.deltaTime / _locationSetPeriod);
		if ((initialDiffPayLoad.x * _diffValues[2].x <= 0f) && (initialDiffPayLoad.y * _diffValues[2].y <= 0f))
		{
			_locations[2] = nextPayLoadPosition;
		}

		//	SET MARKER LOCATIONS -------------------------------------------------------------------------------
		int count = _spawnedObjects.Count;
		for (int i = 0; i < count; i++)
		{
			var spawnedObject = _spawnedObjects[i];
			var location = _locations[i];

			Vector3 rawPosition = _map.GeoToWorldPosition(location, false);
			spawnedObject.transform.localPosition = new Vector3(rawPosition.x, _yPosition, rawPosition.z);
			spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);

			// check if position is outside of minimap
			// base
			if(i==0)
            {
				Vector3 baseOffset = spawnedObject.transform.position - _mapCamera.transform.position;
				Vector2 dir = OverBoundDirection(baseOffset);
				DraggingMap.basePointerOn = (dir != Vector2.zero) ? true : false;
				if (DraggingMap.basePointerOn)
				{
					DraggingMap.baseOutsideDir = dir;
					DraggingMap.baseOutsideScale = PointerScale(baseOffset, dir);
				}
			}
			// rocket
			if(i==1)
            {
				Vector3 rocketOffset = spawnedObject.transform.position - _mapCamera.transform.position;
				Vector2 dir = OverBoundDirection(rocketOffset);
				DraggingMap.rocketPointerOn = (dir != Vector2.zero) ? true : false;
				if(DraggingMap.rocketPointerOn)
                {
					DraggingMap.rocketOutsideDir = dir;
					DraggingMap.rocketOutsideScale = PointerScale(rocketOffset, dir);
				}
			}
			// payload
			if (i == 2)
			{
				Vector3 payLoadOffset = spawnedObject.transform.position - _mapCamera.transform.position;
				Vector2 dir = OverBoundDirection(payLoadOffset);
				DraggingMap.payloadPointerOn = (dir != Vector2.zero) ? true : false;
				if (DraggingMap.payloadPointerOn)
				{
					DraggingMap.payloadOutsideDir = dir;
					DraggingMap.payloadOutsideScale = PointerScale(payLoadOffset, dir);
				}
			}
		}
		//	---------------------------------------------------------------------------------------------------------


	}

	// assign random lat long periodically
	public void SetBasePosition(string locString)
    {
		// assign location difference
		nextBasePosition = Conversions.StringToLatLon(locString);
		_diffValues[0] = nextBasePosition - _locations[0];
		initialDiffBase = nextBasePosition - _locations[0];

		// take base position in world position
		Vector3 baseWorldPos = _map.GeoToWorldPosition(nextBasePosition, false);

		// set camera target and initial positions
		if (_setCameraTarget == false)
        {
			_setCameraTarget = true;
			_cameraTargetPos = new Vector3(baseWorldPos.x, _baseScaleFactor, baseWorldPos.z);
			Vector3 camInit = new Vector3(_mapCamera.transform.position.x, _baseScaleFactor, _mapCamera.transform.position.z);
			_cameraDiff = _cameraTargetPos - camInit;
        }
	}

	public void SetRocketPosition(string locString)
    {
		// assign location difference
		nextRocketPosition = Conversions.StringToLatLon(locString);
		_diffValues[1] = nextRocketPosition - _locations[1];
		initialDiffRocket = nextRocketPosition - _locations[1];
	}

	public void SetPayLoadPosition(string locString)
	{
		// assign location difference
		nextPayLoadPosition = Conversions.StringToLatLon(locString);
		_diffValues[2] = nextPayLoadPosition - _locations[2];
		initialDiffPayLoad = nextPayLoadPosition - _locations[2];
	}


	// output Vector2 (x, y)
	private Vector2 OverBoundDirection(Vector3 checkPosition)
    {
		float xDir = 0f;
		float yDir = 0f;
		xDir = (checkPosition.x <= camBoundLeft_Current) ? -1f : xDir;
		xDir = (checkPosition.x >= camBoundRight_Current) ? 1f : xDir;
		yDir = (checkPosition.z >= camBoundUp_Current) ? 1f : yDir;
		yDir = (checkPosition.z <= camBoundDown_Current) ? -1f : yDir;
		return new Vector2(xDir, yDir);
    }

	private float PointerScale(Vector3 pointerPos, Vector2 pointerDir)
    {
		float bound1 = 0f;
		float bound2 = 0f;
		float scale = 0f;
		string state = "";

		if ( (Mathf.Abs(pointerDir.x) + Mathf.Abs(pointerDir.y)) == 1f)
        {
			bound1 = (pointerDir.x == 0f) ? camBoundLeft_Current : camBoundDown_Current;
			bound2 = (pointerDir.x == 0f) ? camBoundRight_Current : camBoundUp_Current;

			if (pointerDir.x == 0f)
            {
				state = "horizontal";
				bound1 += scaleOffSetX_Current;
				bound2 -= scaleOffSetX_Current;
			}
			else
            {
				state = "vertical";
				bound1 += scaleOffSetY_Current;
				bound2 -= scaleOffSetY_Current;
			}
		}

		float range = bound2 - bound1;
		if(state == "horizontal")
        {
			float pointX = Mathf.Clamp(pointerPos.x, bound1, bound2);
			pointX = pointX - bound1;
			scale = Mathf.Abs(pointX / range);
        }
		else if(state == "vertical")
        {
			float pointZ = Mathf.Clamp(pointerPos.z, bound1, bound2);
			pointZ = pointZ - bound1;
			scale = Mathf.Abs(pointZ / range);
        }

		scale = Mathf.Clamp(scale, 0f, 1f);
		return scale;
	}
}





/*
		// CAMERA ZOOM ##############################################################################
		// get zoom level
		double distanceLat = Mathd.Abs(_locations[1].x - _locations[0].x);
		double distanceLong = Mathd.Abs(_locations[1].y - _locations[0].y);
		int divideLat = (int) (distanceLat / _verticalBound) + 1;
		int divideLong = (int) (distanceLong / _horizontalBound) + 1;
		_zoomTargetIndex = divideLat;
		if (divideLong > divideLat)
			_zoomTargetIndex = divideLong;
		// get camera Y target and current values
		float camYTarget = _zoomTargetIndex * _baseScaleFactor;
		float currentYValue = _mapCamera.transform.position.y;
		
		if(Mathf.Abs(camYTarget-currentYValue)>= _verticalBound && _setCamYDiff == false)
        {
			_setCamYDiff = true;
			_camYDiff = camYTarget - currentYValue;
		}
		// set new y value for map camera
		if( ((camYTarget - currentYValue) * _camYDiff) <= 0f)
        {
			// set target to current value to make it precise
			//Vector3 valueToSet = _mapCamera.transform.position;
			//valueToSet.y = camYTarget;
			//_mapCamera.transform.position = valueToSet;
			_camYDiff = 0f;
			_setCamYDiff = false;
		}
        else
			_mapCamera.transform.position += new Vector3(0f, _camYDiff * Time.deltaTime / _zoomSetTime, 0f);
		// ################################################################################################### 
 
*/


/*		BASE POSITION SET CODE 
 *		
			// CAMERA TO BASE LOCATION
		Vector3 tempPos = _map.GeoToWorldPosition(nextLocationBase, false);
		Vector3 nextCamPos = new Vector3(tempPos.x, _mapCamera.transform.position.y, tempPos.z);
		_cameraMoveDiff = nextCamPos - _mapCamera.transform.position;

		// ADD ZOOM ACTION DEPENDING ON THE CAMERA CENTER NOT BASE LOCATION
		double distanceLat = Mathd.Abs(_locations[1].x - _locations[0].x);
		double distanceLong = Mathd.Abs(_locations[1].y - _locations[0].y);
		int divideLat = (int)(distanceLat / _verticalBound) + 1;
		int divideLong = (int)(distanceLong / _horizontalBound) + 1;
		int zoomTargetIndex = divideLat;
		if (divideLong > divideLat)
			zoomTargetIndex = divideLong;
		int zoomDiff = zoomTargetIndex - _zoomLevel;
		for(int i=0; i<(Mathd.Abs(zoomDiff)); i++)
        {
			if (zoomDiff > 0)
				_zoomActions.Add(ZoomAction.ZoomOut);
			else if (zoomDiff < 0)
				_zoomActions.Add(ZoomAction.ZoomIn);	
        }
		_zoomLevel = zoomTargetIndex;


		CODE USED TO BE IN UPDATE

		// FOLLOW BASE MARKER
		_mapCamera.transform.position += (_cameraMoveDiff * Time.deltaTime / _cameraMovePeriod);


		// APPLY CAMERA ZOOM ACTIONS ######################################################################
		if(_zoomActions.Count > 0)
        {
			if(actionStarted == false)
            {
				actionStarted = true;
				initialYPos = _mapCamera.transform.position.y;
			}	
			
			ZoomAction currentAction = _zoomActions[0];
			int dir = 1;

			if (currentAction == ZoomAction.ZoomOut)
				dir = 1;
			else
				dir = -1;

			_mapCamera.transform.position += new Vector3(0f, dir * _baseScaleFactor * Time.deltaTime / _zoomSetTime, 0f);
			float targetValue = initialYPos + dir * _baseScaleFactor;
			float currentValue = _mapCamera.transform.position.y;
			if (((targetValue - currentValue) * dir) < 0f)
            {
				_mapCamera.transform.position = new Vector3(_mapCamera.transform.position.x, targetValue, 
						_mapCamera.transform.position.z);
				actionStarted = false;
				_zoomActions.RemoveAt(0);
			}
		}
		//	############################################################################################################
*/