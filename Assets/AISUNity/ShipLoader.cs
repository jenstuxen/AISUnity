using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnitySlippyMap;
using System;



public class ShipLoader : MonoBehaviour {
	public enum shipTypes{UNDEFINED, WIG, PILOT, SAR, TUG, PORT_TENDER, ANTI_POLLUTION, LAW_ENFORCEMENT, MEDICAL, FISHING, TOWING, TOWING_LONG_WIDE, DREDGING, DIVING, MILITARY, SAILING, PLEASURE, HSC, PASSENGER, CARGO, TANKER, SHIPS_ACCORDING_TO_RR, UNKNOWN};

	public GameObject[] gos;
	public Map map;
	public Texture	MarkerTexture;


	JSONNode jsonShips;
	AisViewClient av;
	//private TestMap testMap

	IEnumerable enumerable;
	IEnumerator enumerator;

	Coroutine op;

	private Boolean isDirty = true;
	public Boolean IsDirty
	{
		get { return isDirty; }
		set { isDirty = value; }
	}

	private double[] drawnPos;
	public double[] DrawnPos
	{
		get { return drawnPos; }
		set { drawnPos = value; } 
	}

	private float timePassed = 0.0f;
	public float TimePassed
	{
		get { return timePassed; }
		set { timePassed = value; } 
	}

	private Boolean doneSpawning = true;
	public Boolean DoneSpawning
	{
		get { return doneSpawning;}
		set { doneSpawning = value; }
	}



	// Use this for initialization
	void Start () {
		av = new AisViewClient ("aisview");

		map = GameObject.Find("Test").GetComponent<TestMap>().map;
		// create some test 2D markers

		IsDirty = true;
		DoneSpawning = true;
		DrawnPos = new double[]{-900,-900};

	}
	

	// Update is called once per frame
	void Update () 
	{
		if (IsDirty && DoneSpawning) 
		{
			IsDirty = false;
			DoneSpawning = false;
			map.RemoveAllMarkers();


			DrawnPos = map.CenterWGS84;
			//Debug.Log(map.CenterWGS84[0]);
			//Debug.Log (map.CenterWGS84[1]);

			StartCoroutine("ShipSpawningCoRoutine");
		}

	}

	IEnumerator ShipSpawningCoRoutine()
	{
			Debug.Log("DIRTY");
			double[] bbox = new double[]{map.CenterWGS84[1]-2.0,map.CenterWGS84[0]-2.0,map.CenterWGS84[1]+2.0,map.CenterWGS84[0]+2.0};
			jsonShips = av.vessel_list(bbox[0],bbox[1],bbox[2],bbox[3]);
			yield return null;
			
			foreach (JSONNode vessel in jsonShips["vesselList"]["vessels"].Childs)
			{
				try
				{
					var lon = vessel[1].AsDouble;
					var lat = vessel[2].AsDouble;
					var rot = vessel[0].AsFloat;
					var shipID = vessel[6];
					var shipType = vessel[4].AsInt;
					Debug.Log(shipType);
					if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180) {
					GameObject ship = Instantiate(gos[shipType]) as GameObject;
						Ship newShip = map.CreateMarker<Ship>(shipID, new double[2] { lat,lon  }, ship) as Ship;
						newShip.speed = 0;
						newShip.rotation = rot;
					}
				}
				catch(System.NullReferenceException )
				{
					
				}

				yield return null;
			}


			DoneSpawning = true;	
			Debug.Log ("DIRTY DONE");

	}



	void OnGUI () {
		if (Event.current.type == EventType.MouseUp) IsDirty = true;
	}
}
