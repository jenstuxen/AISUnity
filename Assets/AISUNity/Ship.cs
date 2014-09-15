using UnityEngine;
using System.Collections;
using UnitySlippyMap;
 

public class Ship : Marker {

	public double speed;
	public float rotation;

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
