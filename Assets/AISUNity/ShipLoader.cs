using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnitySlippyMap;


public class ShipLoader : MonoBehaviour {

	private TextAsset ships;
	public GameObject go;
	JSONNode jsonShips;
	public Map map;
	public Texture	MarkerTexture;
	//private TestMap testMap

	IEnumerable enumerable;
	IEnumerator enumerator;


	// Use this for initialization
	void Start () {

		map = GameObject.Find("Test").GetComponent<TestMap>().map;
		// create some test 2D markers

		ships = Resources.Load("vessel_list") as TextAsset;
		string txt=ships.text;
		jsonShips = JSON.Parse(txt);
		enumerable = jsonShips["vesselList"]["vessels"].Childs as IEnumerable;
		enumerator = enumerable.GetEnumerator();



	}
	
	// Update is called once per frame
	void Update () {

		JSONNode vessel = enumerator.Current as JSONNode;
		enumerator.MoveNext();
		try{
		var lon = vessel[1].AsDouble;
		var lat = vessel[2].AsDouble;
		var rot = vessel[0].AsFloat;
		var shipID = vessel[6];
		if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180) {
			GameObject ship = Instantiate(go) as GameObject;


			Ship newShip = map.CreateMarker<Ship>(shipID, new double[2] { lat,lon  }, ship) as Ship;
			newShip.speed = 1;
			newShip.rotation = rot;
			}
		}
		catch(System.NullReferenceException e)
		{
		
		}



	}
}
