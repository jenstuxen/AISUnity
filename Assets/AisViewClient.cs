using UnityEngine;
using System.Collections;
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
		protected Uri baseUri = new Uri(Environment.GetEnvironmentVariable("AISVIEW_URI"));
		protected String username = Environment.GetEnvironmentVariable("AISVIEW_USERNAME");
		protected String password = Environment.GetEnvironmentVariable("AISVIEW_PASSWORD");
		


		public JSONNode vessel_list(float topLat, float topLon, float botLat, float botLon) {
		String topLatS = "topLat="+topLat.ToString ("R"); 
		String topLonS = "topLon="+topLon.ToString ("R");
		String botLatS = "botLat="+botLat.ToString ("R");
		String botLonS = "botLon="+botLon.ToString ("R");

		String param = topLatS + "&" + topLon + "&" + botLat + "&" + botLon;

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
			Uri uri = new Uri (baseUri, name);
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
			String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
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

