using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.IO;
using SimpleJSON;
/*
 * Implements the REST services of AisView (http://github.com/dmadk/AisView) 
 * 
 */
public class AisViewClient
{
	private Uri baseUri;
	public Uri BaseUri
	{
	get { return baseUri; } 
	set { baseUri = value; }
	}

	private String username;
	public string Username
	{
	get { return username; } 
	set { username = value; }
	}

	private String password;
	public string Password
	{
	get { return password; } 
	set { password = value; }
	}


	private List<WebRequest> connections = new List<WebRequest>();
	public List<WebRequest> Connections
	{
	get { return connections; } 
	set { connections = value; }
	}

	public AisViewClient(String json) 
	{
		TextAsset txtA = Resources.Load (json) as TextAsset;
		string txt = txtA.text;
		JSONNode config = JSON.Parse (txt);

		BaseUri = new Uri(config ["baseUri"]);
		Username = config ["username"];
		Password = config ["password"];

	}

	public AisViewClient() 
	{
		BaseUri = new Uri(Environment.GetEnvironmentVariable("AISVIEW_URI"));
		Username = Environment.GetEnvironmentVariable ("AISVIEW_USERNAME");
		Password = Environment.GetEnvironmentVariable("AISVIEW_PASSWORD");

	}

	public IEnumerable<JSONNode> Stream(String parameters)
	{
		
		Uri uri = new Uri (BaseUri, "/stream/json/"+parameters);
		WebRequest wb = request (uri);

		Connections.Add (wb);

		WebResponse wr = wb.GetResponse ();
		
		StreamReader reader = new StreamReader (wr.GetResponseStream());



		while (!reader.EndOfStream) 
		{
			JSONNode json = null;

			try
			{
				json = JSON.Parse(reader.ReadLine());
			}
			catch (Exception e)
			{
				Debug.Log("ERROR IN JSON ARRAY");
				Debug.Log(e.Message);
			}

			yield return json;
		}

		reader.Close ();
		wr.Close();
		Debug.Log ("/stream closed");
	}

	public void terminateConnections()
	{
		List<WebRequest> all = Connections;
		Connections = new List<WebRequest> ();
		foreach(WebRequest con in all)
		{

			con.GetResponse().GetResponseStream().Close();
			con.GetResponse().Close();

		}
		all.Clear ();

		Debug.Log ("closed all connections");
	}


	public IEnumerable<JSONNode> Stream(double[] bbox)
	{
		return Stream (bbox [0], bbox [1], bbox [2], bbox [3]);
	}


	public IEnumerable<JSONNode> Stream(double topLat, double topLon, double botLat, double botLon)
	{
		string parameters = "?filter=t.pos within bbox(" + topLat + "," + topLon + "," + botLat + "," + botLon+")";

		return Stream (parameters);
	}

	public JSONNode Vessel_target_details(int mmsi) 
	{
		String details = requestString ("/vessel_target_details?id=" + mmsi);
		return JSON.Parse (details);
	}


	public JSONNode Packets(string parameters) {
		return requestJSON ("/packets" + parameters);
	}

	public JSONNode packets(double topLat, double topLon, double botLat, double botLon) {
		string parameters = "?box=" + topLat + "," + topLon + "," + botLat + "," + botLon;
		return Packets (parameters);
	}



	public JSONNode Vessel_list(double topLat, double topLon, double botLat, double botLon) 
	{
		String topLatS = "topLat="+topLat.ToString ("R"); 
		String topLonS = "topLon="+topLon.ToString ("R");
		String botLatS = "botLat="+botLat.ToString ("R");
		String botLonS = "botLon="+botLon.ToString ("R");

		String param = topLatS + "&" + topLonS + "&" + botLatS + "&" + botLonS;

		return requestJSON ("/vessel_list?"+param);
		}

		public JSONNode vessel_list() {
		return requestJSON ("/vessel_list");
		}

		public Boolean ping() 
		{
			return requestString("/ping").Contains("pong");
		}

		public string requestString (WebRequest wb)
		{
			WebResponse webResponse = wb.GetResponse ();
			StreamReader reader = new StreamReader (webResponse.GetResponseStream ());
			string response = reader.ReadToEnd ();
			reader.Close ();
			webResponse.Close ();
			return response;
		}

		public string requestString (String name)
		{
			Uri uri = new Uri (BaseUri, name);
			WebRequest wb = request (uri);
			return requestString (wb);
		}

		public JSONNode requestJSON(String name)
		{
			string data = requestString (name);
			JSONNode jsonData = JSON.Parse(data);
			return jsonData;
		}
		

		private void authenticate(WebRequest wb) 
		{
			String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Username + ":" + Password));
			wb.Headers.Add("Authorization", "Basic " + encoded);

		}

		private WebRequest request(Uri uri)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			WebRequest wb = WebRequest.Create(uri);
			authenticate (wb);
			return wb;

		}
	
}

