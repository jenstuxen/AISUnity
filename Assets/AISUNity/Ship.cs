using UnityEngine;
using System.Collections;
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

		void Start ()
		{
				this.gameObject.transform.localScale = new Vector3 (0.10f, .10f, 0.10f);
		}
		// Update is called once per frame
		new void Update ()
		{
				float rotInRad = (float)Cog/10.0f * Mathf.Deg2Rad;

				double dX = 0;
				double dY = 0;

				if (Sog > 0 && Cog > -1 && Sog < 1023) {
					//not representative of real world
					dX = Mathf.Sin (rotInRad) * (sog * 0.51 * Map.MetersPerPixel)/Map.RoundedHalfMapScale/20000;
					dY = Mathf.Cos (rotInRad) * (sog * 0.51 * Map.MetersPerPixel)/Map.RoundedHalfMapScale/20000;
				}
				
				if (Map.RoundedMetersPerPixel > 1) {
						this.gameObject.transform.localScale = new Vector3 (0.4f * Map.HalfMapScale, .4f * Map.HalfMapScale, 0.4f * Map.HalfMapScale);

						if (Width > 0 && Length > 0) {
							//Debug.Log ("DIMMMM!!!");
							//this.gameObject.transform.localScale = new Vector3((float)Width,(float)Length,15.0f);
							this.gameObject.transform.localScale = new Vector3 ((float)Width*0.01f * Map.HalfMapScale, (float)Length*.01f * Map.HalfMapScale, 0.4f * Map.HalfMapScale);
						}
				}
				
				transform.localEulerAngles = new Vector3 (0f, (float)Cog/10.0f, 0f);



				CoordinatesEPSG900913 = new double[] {
						dX + CoordinatesEPSG900913 [0],
						dY + CoordinatesEPSG900913 [1]
				};
				base.Reposition ();
		}

		void FixedUpdate ()
		{
		}
}
