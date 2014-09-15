using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnitySlippyMap;
using System;



public class ShipLoader : MonoBehaviour {
	
	public GameObject go;
	public Map map;
	public Texture	MarkerTexture;


	JSONNode jsonShips;
	AisViewClient av;
	//private TestMap testMap

	IEnumerable enumerable;
	IEnumerator enumerator;

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


	// Use this for initialization
	void Start () {
		av = new AisViewClient ();

		map = GameObject.Find("Test").GetComponent<TestMap>().map;
		// create some test 2D markers

		IsDirty = true;
		DrawnPos = new double[]{-900,-900};

	}
	

	// Update is called once per frame
	void Update () 
	{
		TimePassed += Time.deltaTime;

		if ((Math.Abs (DrawnPos [0] - map.CenterWGS84 [0]) > 0.5 | Math.Abs (DrawnPos [1] - map.CenterWGS84 [1]) > 0.5) & TimePassed > 1)
		{
			IsDirty = true;
			TimePassed=0;
		}


		if (IsDirty) 
		{


			DrawnPos = map.CenterWGS84;
			Debug.Log(map.CenterWGS84[0]);
			Debug.Log (map.CenterWGS84[1]);
			IsDirty = false;
			double[] bbox = new double[]{map.CenterWGS84[1]-2.0,map.CenterWGS84[0]-2.0,map.CenterWGS84[1]+2.0,map.CenterWGS84[0]+2.0};

			map.RemoveAllMarkers();

			

			jsonShips = av.vessel_list(bbox[0],bbox[1],bbox[2],bbox[3]);

			foreach (JSONNode vessel in jsonShips["vesselList"]["vessels"].Childs)
			{

				try
				{
					var lon = vessel[1].AsDouble;
					var lat = vessel[2].AsDouble;
					var rot = vessel[0].AsFloat;
					var shipID = vessel[6];
					if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180) {
						Debug.Log("shippppp");
						GameObject ship = Instantiate(go) as GameObject;
						Ship newShip = map.CreateMarker<Ship>(shipID, new double[2] { lat,lon  }, ship) as Ship;
						newShip.speed = 0;
						newShip.rotation = rot;
					}
				}
				catch(System.NullReferenceException e)
				{
					
				}
			}

		}


	}
}
