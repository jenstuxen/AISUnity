using UnityEngine;
using System.Collections;
using UnitySlippyMap;
 

public class Ship : Marker {

	private double speed;
	public double Speed
	{
		get { return speed; }
		set { speed = value; }
	}
		

	private float rotation;
	public float Rotation;
	{
		get { return rotation; }
		set { rotation = value; }
	}

	private double dimStern;
	public double DimStern;
	{
		get { return dimStern; }
		set { rotation = value; }
	}

	private double dimStarboard;
	public double DimStarboard;
	{
		get { return dimStarboard; }
		set { dimStarboard = value; }
	}

	private double dimBow;
	public double DimBow;
	{
		get { return dimBow; }
		set { dimBow = value; }
	}

	public double DimPort;
	{
		get { return dimPort; }
		set { dimPort = value; }
	}

	private string name = "N/A";
	public string Name
	{
		get { return name; }
		set { name = value; }

	}

	private string country = "N/A";

	private string destionation = "N/A";

	private string callsign = "N/A";

	private string imoNo = "N/A";

	private string sourceCountry = "N/A"

	


	void Start () {
		this.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
	}
	// Update is called once per frame
	void Update () {
		float rotInRad = rotation*Mathf.Deg2Rad;
		
		double dX = Mathf.Sin(rotInRad)*speed;
		double dY = Mathf.Cos(rotInRad)*speed;
		transform.localEulerAngles = new Vector3(0f, rotation, 0f);
		CoordinatesEPSG900913 = new double[] {dX + CoordinatesEPSG900913[0], dY + CoordinatesEPSG900913[1]};

		base.Reposition();
	
	}

	void FixedUpdate(){

	}
}
