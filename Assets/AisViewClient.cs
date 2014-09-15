using UnityEngine;
using System.Collections;
using System.Net;
using System;
/*
 * Implements the REST services of AisView (http://github.com/dmadk/AisView) 
 * 
 */
public class AisViewClient : MonoBehaviour
{
		protected Uri baseUri = new Uri(Environment.GetEnvironmentVariable("AISVIEW_URI"));
		protected String username = Environment.GetEnvironmentVariable("AISVIEW_USERNAME");
		protected String password = Environment.GetEnvironmentVariable("AISVIEW_PASSWORD");
		

		// Use this for initialization
		void Start ()
		{
			Uri uri = new Uri (baseUri, "/aisview/rest/vessel_list");

			WebRequest wb = WebRequest.Create(uri);
			authenticate (wb);	
			Debug.Log (wb.GetResponse ().ToString ());

		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}


		TextAsset ping() 
		{
			return null;
			//System.Net.HttpWebRequest wb = new System.Net.HttpWebRequest ();
		}

		void authenticate(WebRequest wb) 
		{
			String encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
			wb.Headers.Add("Authorization", "Basic " + encoded);

		}
}

