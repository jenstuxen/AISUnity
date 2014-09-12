using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnitySlippyMap;


public class ShipLoader : MonoBehaviour {

	private TextAsset ships;
	GameObject go;
	GameObject markgerGO;
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
		go = Tile.CreateTileTemplate(Tile.AnchorPoint.BottomCenter).gameObject;
		go.renderer.material.mainTexture = MarkerTexture;
		go.renderer.material.renderQueue = 4001;
		go.transform.localScale = new Vector3(0.70588235294118f, 1.0f, 1.0f);
		go.transform.localScale /= 7.0f;

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
	

		GameObject markerGO = Instantiate(go) as GameObject;
		var lon = vessel[1].AsDouble;
		var lat = vessel[2].AsDouble;


		if (lat < 90.0 && lat > -90.0 && lon < 180.0 && lon > -180) {
			markerGO = Instantiate(go) as GameObject;;
			map.CreateMarker<Marker>(vessel, new double[2] { lat,lon  }, markerGO);
		}

	}
}
