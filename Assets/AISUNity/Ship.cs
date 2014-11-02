using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnitySlippyMap;
 
public class Ship : Marker
{

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

		public class PNT : System.IEquatable<PNT> {
			public PNT(double[] pos, double timestamp) {
				this.lon = (float)pos[0];
				this.lat = (float)pos[1];
				this.timestamp = timestamp;
			}

			public bool Equals(PNT other) {
				return ((this.lat == other.lat && this.lon == other.lon) || this.timestamp == other.timestamp);
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

		public bool isValidPosition(double lat, double lon) {
			return lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180.0 && lat != 0.0 && lon != 0.0;
		}

		public void addPNT(double[] pos, double timestamp) {
			if (!isValidPosition(pos[1],pos[0]) || timestamp < 1414078500095 || History.Contains (new PNT(pos,timestamp)))
				return;

			if (History.Count >= 50 || (History.Count >= 2 && (timestamp - History.Peek().timestamp > 120000))) {
				History.Dequeue();
			}

			History.Enqueue(new PNT(pos,timestamp));
		}

		public double[] calcDxDy() {
			
			float dX = 0;
			float dY = 0;
			float dT = 1;
	
			try {		
				if (History.Count >= 2) {
					PNT[] arr = History.ToArray();
					

					dT = (float)(arr[arr.Length-1].timestamp - arr[0].timestamp);
					
					if (dT > 2000 && dT < 120000) {
						//translate into 1st quadrant
						dX = (1000+arr[arr.Length-1].lon - (1000+arr[0].lon));					
						dY = (1000+arr[arr.Length-1].lat - (1000+arr[0].lat));

						//Debug.Log(arr[0].timestamp);	
					// 0/1 = 0, no delta x or y
					} else {
						dT = 1;
					}

					
				}
			} catch (System.Exception) {
			}


			return new double[] {dX/dT,dY/dT};
		}


		void Start ()
		{
				this.gameObject.transform.localScale = new Vector3 (0.10f, .10f, 0.10f);
		}
		// Update is called once per frame
		new void Update ()
		{
				float rotInRad = (float)Cog/10.0f * Mathf.Deg2Rad;

				double[] deltaPos = calcDxDy ();


				if (Map.RoundedMetersPerPixel > 1) {
						this.gameObject.transform.localScale = new Vector3 (0.4f * Map.HalfMapScale, .4f * Map.HalfMapScale, 0.4f * Map.HalfMapScale);

						if (Width > 0 && Length > 0) {
							//Debug.Log ("DIMMMM!!!");
							//this.gameObject.transform.localScale = new Vector3((float)Width,(float)Length,15.0f);
							this.gameObject.transform.localScale = new Vector3 ((float)Width*0.01f * Map.HalfMapScale, (float)Length*.01f * Map.HalfMapScale, 0.4f * Map.HalfMapScale);
						}
				}
				
				transform.localEulerAngles = new Vector3 (0f, (float)Cog/10.0f, 0f);
				
				double newLon = (deltaPos [0]*Time.deltaTime*1000) + CoordinatesWGS84 [0];
				double newLat = (deltaPos [1]*Time.deltaTime*1000) + CoordinatesWGS84 [1];
				
				if (isValidPosition(newLat,newLon)) {
					CoordinatesWGS84 = new double[] {newLon,newLat};
				}
				
				base.Reposition ();
		}

		void FixedUpdate ()
		{
		}
}
