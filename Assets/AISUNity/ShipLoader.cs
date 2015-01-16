using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SimpleJSON;
using UnitySlippyMap;
using System;

public class ShipLoader : MonoBehaviour
{
		public enum shipTypes
		{
				UNDEFINED,
				WIG,
				PILOT,
				SAR,
				TUG,
				PORT_TENDER,
				ANTI_POLLUTION,
				LAW_ENFORCEMENT,
				MEDICAL,
				FISHING,
				TOWING,
				TOWING_LONG_WIDE,
				DREDGING,
				DIVING,
				MILITARY,
				SAILING,
				PLEASURE,
				HSC,
				PASSENGER,
				CARGO,
				TANKER,
				SHIPS_ACCORDING_TO_RR,
				UNKNOWN}
		;
		
		public GameObject defaultShipModel;
		public Map map;
		public Texture	MarkerTexture;
		private int packetCount = -1;

		public int PacketCount {
				get { return packetCount; }
		}
	
		private int frameCount = -1;

		public int FrameCount {
				get { return frameCount; }
		}

		List<JSONNode> buffer = new List<JSONNode> ();

		public List<JSONNode> Buffer {
				get { return buffer; }
				set { buffer = value; }
		}

		AisViewClient av;
		//private TestMap testMap
	
		IEnumerator<JSONNode> enumerator = null;

		public IEnumerator<JSONNode> Enumerator {
				get { return enumerator; }
				set { enumerator = value; }
		}

		private Boolean isDirty = true;

		public Boolean IsDirty {
				get { return isDirty; }
				set { isDirty = value; }
		}

		private DateTime epoch = new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
		private double accumulatedTimeDelta = 0;
		private double previousVesselTimeStamp;
		private double timeWarp = 1.0f;

		public Double TimeWarp {
				get { return timeWarp; }
				set { timeWarp = value; }
		}

		private double[] drawnPos;

		public double[] DrawnPos {
				get { return drawnPos; }
				set { drawnPos = value; } 
		}
	
		private Boolean doneSpawning = true;

		public Boolean DoneSpawning {
				get { return doneSpawning;}
				set { doneSpawning = value; }
		}

		// Use this for initialization
		void Start ()
		{
				defaultShipModel = (Resources.Load ("Ships/Steamer boat")) as GameObject;
				av = new AisViewClient ("aisview");

				map = GameObject.Find ("Test").GetComponent<TestMap> ().map;
				// create some test 2D markers

				IsDirty = false;
				DoneSpawning = true;
				DrawnPos = new double[]{-900,-900};
				//CallWebService (true);
				CallHistoricalReplay ();
		}

		// Update is called once per frame
		void Update ()
		{
				if (IsDirty) {
						IsDirty = false;
						DrawnPos = map.CenterWGS84;
						//Debug.Log(map.CenterWGS84[0]);
						//Debug.Log (map.CenterWGS84[1]);
						//CallWebService (true);
				}

				frameCount++;
				
				if (FrameCount % 100 == 0) {
						DateTime meh = epoch.AddMilliseconds (previousVesselTimeStamp).ToUniversalTime ();
						Debug.Log ("Vessel time is: " + meh.ToLongDateString () + " " + meh.ToLongTimeString ());
						//Debug.Log ("Total Packets: " + PacketCount + " Vessels: " + map.Markers.Keys.Count);
						//Debug.Log ("TimeWarp Factor: " + TimeWarp);
				}
		
				UpdateShips ();
		}

		void FixedUpdate ()
		{
				accumulatedTimeDelta += Time.deltaTime;
				List<JSONNode> buffer = Buffer;
				if (accumulatedTimeDelta > 0.5f && buffer.Count > 0) {
				
						double newest = buffer [0] ["timestamp"].AsDouble;
						TimeWarp = (newest - previousVesselTimeStamp) / (accumulatedTimeDelta * 1000);
						previousVesselTimeStamp = newest;
						accumulatedTimeDelta = 0;

				}
		}

		void UpdateShips ()
		{
				
				//where are my concurrent queues???
				int snapshotCount = Buffer.Count;
				List<JSONNode> vessels = Buffer;
				
				if (vessels.Count > 0) {
						Buffer = new List<JSONNode> ();
				} else {
						return;
				}

				JSONNode[] arr = vessels.ToArray ();

				int count = 0;
				while (count < arr.Length) {
						UpdateShip (arr [count]);
						count++;
						packetCount++;
				}
				
				if (count > 100)
						Debug.Log ("Packets in Buffer: " + count);
		}

		void UpdateShip (JSONNode vessel)
		{	
				int mmsi = -9999;
				Double lon = -9999;
				Double lat = -9999;	
				Double cog = -9999;
				string shipType = "N/A";
				double timestamp = 0;
		
				try {
						if (vessel != null) {
								mmsi = vessel ["mmsi"].AsInt;
								lon = vessel ["lon"].AsDouble;
								lat = vessel ["lat"].AsDouble;
								cog = vessel ["cog"].AsInt;
								timestamp = vessel ["timestamp"].AsDouble;
								shipType = vessel ["shipType"];
								
						}
				} catch (System.NullReferenceException) {
			
				}
				Ship shipMarker = null;
				if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180 && timestamp > 0) {
						if (map.Markers.ContainsKey (mmsi)) {
								try {
										shipMarker = (Ship)map.Markers [mmsi];
										shipMarker.UpdatePosIfNotExist (new double[] {lon, lat}, timestamp);	
										//Debug.Log ("Updated dynamic info");
								} catch (System.NullReferenceException) {
								}
						} else {	
								GameObject ship = Instantiate (defaultShipModel) as GameObject;
				
								//ship.AddComponent<Rigidbody>();
				
								shipMarker = map.CreateMarker<Ship> (mmsi, new double[2] { lon,lat  }, ship) as Ship;
								shipMarker.Parent = this;
						}
				}
				
				//update other info
				if (map.Markers.ContainsKey (mmsi)) {
						shipMarker = (Ship)map.Markers [mmsi];
						shipMarker.UpdateMetadata (vessel);
				}
		}

		void CallLiveView (Boolean full)
		{
				//av.terminateConnections();
				double[] bbox = new double[] {
						map.CenterWGS84 [1] - 0.5,
						map.CenterWGS84 [0] - 0.5,
						map.CenterWGS84 [1] + 0.5,
						map.CenterWGS84 [0] + 0.5
				};

				Thread b = new Thread (() => 
				{
						Debug.Log ("New Thread Started " + bbox [0]);
						IEnumerator<JSONNode> myEnumerator;
						if (full) {
								Debug.Log ("Initiating Full Update");
								myEnumerator = av.TrackerPackets (bbox);
						} else {

								myEnumerator = av.Stream (bbox);
						}


						
						av.Latest = myEnumerator;
						Thread.Sleep (2000);
						while (av.Latest != null && av.Latest.Equals(myEnumerator)) {
								if (av.Latest.MoveNext ()) {
										Buffer.Add (av.Latest.Current);
								} else {
										Debug.Log ("Initiating Stream Connection");
										CallLiveView (false);
										Thread.Sleep (2000);
								}
								
						}

						if (full)
								Debug.Log ("DECOMISSIONING FULL UPDATE " + bbox [0]);
						else
								Debug.Log ("DECOMISSIONING STREAM " + bbox [0]);	
				});

				b.Start ();
		}

		void CallHistoricalReplay ()
		{
				Thread b = new Thread (() =>
				{		
						//follow mmsi
						IEnumerator<JSONNode> enumerator = av.StoreStream ("?interval=2014-12-22T14:00:00Z/2015-01-05T18:10:00Z&box=56.075,12.599,56.012,12.700&mmsi=219622000&output=json");
						//follow area
						//IEnumerator<JSONNode> enumerator = av.StoreStream ("?interval=2014-12-22T14:00:00Z/2015-01-05T18:10:00Z&box=56.075,12.599,56.012,12.700&output=json");
			
						
						av.Latest = enumerator;
						Thread.Sleep (4000);
						while (av.Latest != null && av.Latest.Equals(enumerator)) {
								if (av.Latest.MoveNext ()) {
										Buffer.Add (av.Latest.Current);
								}
						}
				});

				b.Start ();
		}

		void OnGUI ()
		{
				if (Event.current.type == EventType.MouseUp)
						IsDirty = true;
		}
}
