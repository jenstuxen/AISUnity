using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnitySlippyMap;
using SimpleJSON;

public class Ship : Marker
{

		private ShipLoader parent;

		public ShipLoader Parent { get { return parent; } set { parent = value; } }

		private double speed;

		public double Speed {
				get { return speed; }
				set { speed = value; }
		}

		private double cog = -1;

		public double Cog {
				get { return cog; }
				set { cog = value; }
		}

		private double sog = -1;

		public double Sog {
				get { return sog; }
				set { sog = value; }
		}

		private int trueHeading = -1;

		public int TrueHeading {
				get { return trueHeading; }
				set { trueHeading = value; }
		}

		private int dimStern;

		public int DimStern {
				get { return dimStern; }
				set { dimStern = value; }
		}

		private int dimStarboard;

		public int DimStarboard {
				get { return dimStarboard; }
				set { dimStarboard = value; }
		}

		private int dimBow;

		public int DimBow {
				get { return dimBow; }
				set { dimBow = value; }
		}

		private int dimPort;

		public int DimPort {
				get { return dimPort; }
				set { dimPort = value; }
		}

		public string shipName = "N/A";

		public string ShipName {
				get { return shipName; }
				set { shipName = value; }
		}

		private string country = "N/A";

		public string Country {
				get { return country; }
				set { country = value; }
		}

		private string destination = "N/A";

		public string Destination {
				get { return destination; }
				set { destination = value; }
		}

		private string callsign = "N/A";

		public string Callsign {
				get { return callsign; }
				set { callsign = value; }
		}

		private string sourceCountry = "N/A";

		public string SourceCountry {
				get { return sourceCountry; }
				set { sourceCountry = value; }
		}

		private string imoNo = "N/A";

		public string ImoNo {
				get { return imoNo; }
				set { imoNo = value; }		
		}

		public int Length {
				get { return DimStern + DimBow; }
		}

		public int Width {
				get { return DimPort + DimStarboard; }
		}

		private float steps = 0.01f;

		public void UpdatePosIfNotExist (double[] pos, double timestamp)
		{
				PNT pnt = new PNT (pos, timestamp);
		
				if (this.isValidPosition (pos [1], pos [0]) && History.Contains (pnt)) {
						return;
				} else {
						//CoordinatesWGS84 = pos;

						if (timestamp > 0) {
								this.addPNT (pos, timestamp);
								framesBetweenUpdate = frameCounter;
								frameCounter = 1;
						}
				}
		}

		public void UpdateMetadata (JSONNode vessel)
		{
				//set if not null
				if (vessel ["dimBow"] != null) 
						this.dimBow = vessel ["dimBow"].AsInt;
				
				if (vessel ["dimStern"] != null) 
						this.dimBow = vessel ["dimStern"].AsInt;
						
				if (vessel ["dimStaboard"] != null) 
						this.dimBow = vessel ["dimStarboard"].AsInt;
				
				if (vessel ["dimPort"] != null) 
						this.dimBow = vessel ["dimPort"].AsInt;
				
				if (vessel ["name"] != null) 
						this.ShipName = (string)vessel ["name"];
				
				if (vessel ["sog"] != null) 
						this.Sog = vessel ["sog"].AsDouble;
													
				if (vessel ["cog"] != null) 
						this.Cog = vessel ["cog"].AsDouble;
															
				if (vessel ["sog"] != null) 
						this.TrueHeading = vessel ["trueHeading"].AsInt;
		}

		public class PNT : System.IEquatable<PNT>
		{
				public PNT (double[] pos, double timestamp)
				{
						this.lon = (float)pos [0];
						this.lat = (float)pos [1];
						this.timestamp = timestamp;
				}

				public bool Equals (PNT other)
				{
						return ((this.lat == other.lat && this.lon == other.lon) || this.timestamp == other.timestamp || Mathf.Abs ((float)this.timestamp - (float)other.timestamp) < 1500.0f || Mathf.Abs ((float)this.lat - (float)other.lat) < 0.0001f || Mathf.Abs ((float)this.lon - (float)other.lon) < 0.0001f);
				}

				public float lon;
				public float lat;
				public double timestamp;
		}

		private Queue<PNT> history = new Queue<PNT> ();

		public Queue<PNT> History {
				get { return history; }
				set { history = value;}		
		}

		public bool isValidPosition (double lat, double lon)
		{
				return lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180.0 && lat != 0.0 && lon != 0.0;
		}

		public void addPNT (double[] pos, double timestamp)
		{
				if (!isValidPosition (pos [1], pos [0]) || History.Contains (new PNT (pos, timestamp)))
						return;

				if (History.Count >= 50) {
						History.Dequeue ();
				}
				steps = 0.0f;
				accumulatedTimeBetweenUpdates = 1.0f;
				History.Enqueue (new PNT (pos, timestamp));
		}

		private double[] predict (float mu)
		{	
				float X = 9999;
				float Y = 9999;
				float dNew = 9999;
	
	
				PNT[] segment = findRepresentativeSegment (History, 4, 200.0).ToArray ();
				if (segment.Length >= 4) {
						X = Utils.CubicInterpolate (segment [0].lon, segment [1].lon, segment [2].lon, segment [3].lon, mu);
						Y = Utils.CubicInterpolate (segment [0].lat, segment [1].lat, segment [2].lat, segment [3].lat, mu);
						dNew = Utils.CubicInterpolate ((float)segment [0].timestamp, (float)segment [1].timestamp, (float)segment [2].timestamp, (float)segment [3].timestamp, mu);
				} else if (History.Count >= 10) {
						PNT[] arr = History.ToArray ();
						PNT a = arr [arr.Length - 1];
						PNT b = arr [arr.Length - 10];
						dNew = Utils.CosineInterpolate ((float)a.timestamp, (float)b.timestamp, mu);
						X = Utils.CosineInterpolate (a.lon, b.lon, mu);					
						Y = Utils.CosineInterpolate (a.lat, b.lat, mu);
				}
				
				 
			
				return new double[] {X,Y,dNew};
		}
		
		
		private Queue<PNT> findRepresentativeSegment (Queue<PNT> h, int count, double distance)
		{
		
				Queue<PNT> result = new Queue<PNT> ();
				PNT[] copy = h.ToArray ();
			
			
				result.Enqueue (copy [copy.Length - 1]);
				PNT a = result.Peek ();
				for (int i= copy.Length-2; i>0; i--) {
						PNT b = copy [i];
						if (Utils.cartasianDistance (a.lat, a.lon, b.lat, b.lon) > distance) {
								result.Enqueue (b);
						}
						if (result.Count > count)
								return result;
				}
			
				return result;
		
		}
		
		private float averageActualTimeBetweenUpdates ()
		{
			
				if (History.Count > 2) {
						PNT[] arr = History.ToArray ();
						return (float)(arr [arr.Length - 1].timestamp - arr [0].timestamp) / (float)arr.Length;
				}
			
				return 9999999.0f;
		}
		
		private float accumulatedTimeBetweenUpdates = 0f;
		private int frameCounter = 1;
		private int framesBetweenUpdate = 1;

		void Start ()
		{
				this.gameObject.transform.localScale = new Vector3 (0.10f, .10f, 0.10f);
		}
		// Update is called once per frame
		void Update ()
		{
		
				if (ShipName != "HAMLET")
						return;
						
				accumulatedTimeBetweenUpdates += Time.deltaTime;
				float rotInRad = (float)Cog / 10.0f * Mathf.Deg2Rad;

				if (Map.RoundedMetersPerPixel > 1) {
						this.gameObject.transform.localScale = new Vector3 (0.8f * Map.HalfMapScale, .8f * Map.HalfMapScale, 0.8f * Map.HalfMapScale);

						if (Width > 0 && Length > 0) {
								//Debug.Log ("DIMMMM!!!");
								//this.gameObject.transform.localScale = new Vector3((float)Width,(float)Length,15.0f);
								this.gameObject.transform.localScale = new Vector3 ((float)Width * 0.02f * Map.HalfMapScale, (float)Length * .02f * Map.HalfMapScale, 0.8f * Map.HalfMapScale);
						}
				}
			
				transform.localEulerAngles = new Vector3 (0f, (float)Cog / 10.0f, 0f);

				steps = steps + (Time.deltaTime / accumulatedTimeBetweenUpdates);
				
				double[] deltaPos = predict (0.5f + steps);

				//Debug.Log (steps);
		
				//double[] deltaPos = calcDxDy();
				double newLon = (deltaPos [0]);
				double newLat = (deltaPos [1]);
			
				
				if (isValidPosition (newLat, newLon)) {
						CoordinatesWGS84 = new double[] {newLon,newLat};
				} 
			
				frameCounter++;
				base.Reposition ();
		}

		void FixedUpdate ()
		{
		}
}
