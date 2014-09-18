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

			DrawnPos = map.CenterWGS84;
			//Debug.Log(map.CenterWGS84[0]);
			//Debug.Log (map.CenterWGS84[1]);

			StartCoroutine("ShipSpawningCoRoutine");
		}

	}

	IEnumerator ShipSpawningCoRoutine()
	{
			
			double[] bbox = new double[]{map.CenterWGS84[1]-.25,map.CenterWGS84[0]-.25,map.CenterWGS84[1]+.25,map.CenterWGS84[0]+.25};
			Debug.Log("BBOX WEB START   "+bbox[0]+" "+bbox[1]+" "+bbox[2]+" "+bbox[3]);
			jsonShips = av.vessel_list(bbox[0],bbox[1],bbox[2],bbox[3]);
			yield return null;
			Debug.Log("BBOX WEB END     "+bbox[0]+" "+bbox[1]+" "+bbox[2]+" "+bbox[3]);
			Debug.Log("BBOX SPAWN START "+bbox[0]+" "+bbox[1]+" "+bbox[2]+" "+bbox[3]);
			foreach (JSONNode vessel in jsonShips["vesselList"]["vessels"].Childs)
			{
				try
				{
					var lon = vessel[1].AsDouble;
					var lat = vessel[2].AsDouble;
					var rot = vessel[0].AsFloat;
					int shipID = vessel[6].AsInt;
					var shipType = vessel[4].AsInt;
					if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180) {
						Ship shipMarker = null;
						if (map.Markers.ContainsKey(shipID))
						{
							Marker m = map.Markers[shipID];
							if (m.GetType().IsAssignableFrom(shipMarker.GetType()))
						    {
								shipMarker = (Ship) map.Markers[shipID];
								shipMarker.CoordinatesWGS84 = new double[2] {lat,lon};
								shipMarker.rotation = rot;
							}
						}
						else
						{
								GameObject ship = Instantiate(gos[shipType]) as GameObject;
								shipMarker = map.CreateMarker<Ship>(shipID, new double[2] { lat,lon  }, ship) as Ship;
								shipMarker.speed = 0;
								shipMarker.rotation = rot;
						}
					}
				}
				catch(System.NullReferenceException )
				{
					
				}

				yield return null;
			}
			Debug.Log("BBOX SPAWN END  "+bbox[0]+" "+bbox[1]+" "+bbox[2]+" "+bbox[3]);
			Debug.Log ("SHIPS: " + map.Markers.Count);

			DoneSpawning = true;	
			

	}

	IEnumerator ShipStreamReader()
	{
		//to be implemented
		return null;
	}



	void OnGUI () {
		if (Event.current.type == EventType.MouseUp) IsDirty = true;
	}
}
