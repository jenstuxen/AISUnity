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

	private List<WebResponse> connections = new List<WebResponse>();
	public List<WebResponse> Connections
	{
	get { return connections; } 
	set { connections = value; }
	}
	


	private IEnumerator<JSONNode> latest;
	public IEnumerator<JSONNode> Latest
	{
		get { return latest; } 
		set { latest = value; }
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

	public IEnumerator<JSONNode> Stream(String parameters)
	{
		Debug.Log ("Starting new Web Request");
		Uri uri = new Uri (BaseUri, "/stream/json/"+parameters);
		WebRequest wb = request (uri);
		wb.Timeout = 1000;

		WebResponse wr = wb.GetResponse ();
		Connections.Add (wr);
		
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

		Debug.Log("/stream loop ended");
		yield return null;
	}

	public void terminateConnections()
	{
		List<WebResponse> all = Connections;
		Connections = new List<WebResponse> ();
		foreach(WebResponse con in all)
		{

			try {
				con.GetResponseStream().Close();
				con.Close();
				
			} 
			catch (ObjectDisposedException)
			{
				Debug.Log("already disposed exception");
			}

		}
		all.Clear ();

		Debug.Log ("closed all connections");
	}


	public IEnumerator<JSONNode> Stream(double[] bbox)
	{
		return Stream (bbox [0], bbox [1], bbox [2], bbox [3]);
	}


	public IEnumerator<JSONNode> Stream(double topLat, double topLon, double botLat, double botLon)
	{
		string parameters = "?filter=t.pos within bbox(" + topLat + "," + topLon + "," + botLat + "," + botLon+")";

		return Stream (parameters);
	}

	public JSONNode Packets(string parameters) {
		return requestJSON ("/packets" + parameters);
	}

	public JSONNode packets(double topLat, double topLon, double botLat, double botLon) {
		string parameters = "?box=" + topLat + "," + topLon + "," + botLat + "," + botLon;
		return Packets (parameters);
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

