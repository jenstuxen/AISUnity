using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SimpleJSON;
using UnitySlippyMap;
using System;
using System.Threading;




public class ShipLoader : MonoBehaviour {
	public enum shipTypes{UNDEFINED, WIG, PILOT, SAR, TUG, PORT_TENDER, ANTI_POLLUTION, LAW_ENFORCEMENT, MEDICAL, FISHING, TOWING, TOWING_LONG_WIDE, DREDGING, DIVING, MILITARY, SAILING, PLEASURE, HSC, PASSENGER, CARGO, TANKER, SHIPS_ACCORDING_TO_RR, UNKNOWN};

	public GameObject[] gos;
	public Map map;

	public Texture	MarkerTexture;
	public GameObject go;

	private int packetCount = -1;
	public int PacketCount
	{
		get { return packetCount; }
	}

	JSONNode latest = null;
	public JSONNode Latest
	{
		get { return latest; }
		set { latest = value; }
	}


	AisViewClient av;
	//private TestMap testMap
	
	IEnumerator<JSONNode> enumerator = null;
	public IEnumerator<JSONNode> Enumerator
	{
		get { return enumerator; }
		set { enumerator = value; }
	}

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

		IsDirty = false;
		DoneSpawning = true;
		DrawnPos = new double[]{-900,-900};
		StartCoroutine("UpdateShip");

		Thread newThread = new Thread (() => 
		{
			while(true)
			{
				if (av.Latest !=null && av.Latest.MoveNext ())
				{
					Latest = av.Latest.Current;
					//Debug.Log("changing latest");
				}
	
			}
		});

		newThread.Start();

	}
	

	// Update is called once per frame
	void Update () 
	{
		if (IsDirty) 
		{
			IsDirty = false;


			DrawnPos = map.CenterWGS84;
			//Debug.Log(map.CenterWGS84[0]);
			//Debug.Log (map.CenterWGS84[1]);

			CallWebService();

		}

		if (PacketCount % 10 == 0) 
		{
			Debug.Log("Total Packets: "+PacketCount+" Vessels: "+map.Markers.Keys.Count);

			//if (map.Markers.Keys.Count > 100) {
			//	map.RemoveAllMarkers();
			//}
		}


		UpdateShip ();



	}


	void UpdateShip()
	{
		JSONNode vessel = Latest;
		
		if (vessel != null) Latest = null;
		
		
		int mmsi = -9999;
		Double lon = -9999;
		Double lat = -9999;	
		Double rot = -9999;
		string shipType = "N/A";
		
		try 
		{
			if (vessel != null)
			{
				mmsi = vessel ["mmsi"].AsInt;
				lon = vessel["lon"].AsDouble;
				lat = vessel["lat"].AsDouble;
				rot = vessel["cog"].AsDouble/1000.0;
				shipType = vessel["shipType"];
			}
		} 
		catch (System.NullReferenceException) 
		{
			
		}
		
		if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180)
		{
			Ship shipMarker = null;
			if (map.Markers.ContainsKey(mmsi))
			{
				try 
				{
					shipMarker = (Ship) map.Markers[mmsi];
					shipMarker.CoordinatesWGS84 = new double[2] {lon,lat};
					shipMarker.Rotation = rot;
					
				} catch (System.NullReferenceException)
				{
				}
			}
			else
			{	
				//disabled while I test the stability of the packet stream
				//GameObject ship = Instantiate(gos[1]) as GameObject;
				GameObject ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
				//ship.AddComponent<Rigidbody>();
				
				shipMarker = map.CreateMarker<Ship>(mmsi, new double[2] { lon,lat  }, ship) as Ship;
				shipMarker.Speed = 0;
				shipMarker.Rotation = rot;
			}
		}
	}
	
	
	void CallWebService()
	{
		//av.terminateConnections();
		double[] bbox = new double[]{map.CenterWGS84[1]-0.25,map.CenterWGS84[0]-0.25,map.CenterWGS84[1]+0.25,map.CenterWGS84[0]+0.25};

		Thread b = new Thread (() => {
				Debug.Log ("New Thread Started");
				IEnumerator<JSONNode> myEnumerator = av.Stream (bbox);
				av.Latest = myEnumerator;

				while(true) {
					Thread.Sleep(1000);
				}

				Debug.Log("DECOMISSIONING Thread");
		});

		b.Start ();
		


	}

	void OnGUI () {
		if (Event.current.type == EventType.MouseUp) IsDirty = true;
	}
}
