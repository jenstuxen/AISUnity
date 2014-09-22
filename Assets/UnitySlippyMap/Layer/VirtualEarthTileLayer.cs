﻿// 
//  VirtualEarthTileLayer.cs
//  
//  Author:
//       Jonathan Derrough <jonathan.derrough@gmail.com>
//  
//  Copyright (c) 2012 Jonathan Derrough
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

#define DEBUG_LOG

using System;
using System.IO;
using System.Xml.Serialization;

using UnityEngine;

using System.Globalization;
using Microsoft.MapPoint;

namespace UnitySlippyMap
{

// <summary>
// A class representing a VirtualEarth tile layer.
// </summary>
public class VirtualEarthTileLayer : WebTileLayer
{
    // http://msdn.microsoft.com/en-us/library/ff701712.aspx
    // http://msdn.microsoft.com/en-us/library/ff701716.aspx

    #region Private members & properties

    /// <summary>
    /// Set it to true to notify the VirtualEarthTileLayer to reload the metadata.
    /// </summary>
    private bool hostnameChanged = false;
    /// <summary>
    /// The host.
    /// </summary>
    private string hostname = "dev.virtualearth.net";
    public string           Hostname
    {
        get { return hostname; }
        set
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value == String.Empty)
                throw new ArgumentException("value cannot be empty");
            hostnameChanged = true;
            hostname = value;
        }
    }

    /// <summary>
    /// Set it if you are using a proxy (mandatory with Unity3D webplayer since http://dev.virtualearth.net/crossdomain.xml is not supported for some reason).
    /// </summary>
    private string proxyURL = null;
    public string ProxyURL { get { return proxyURL; } set { proxyURL = value; } }

    /// <summary>
    /// Set it to true to notify the VirtualEarthTileLayer to reload the metadata.
    /// </summary>
    private bool            metadataRequestURIChanged = false;
    /// <summary>
    /// The request URI for the metada.
    /// </summary>
		private string          metadataRequestURI = "/REST/V1/Imagery/Metadata/Aerial/?mapVersion=v1&output=xml&key=";
    public string           MetadataURL
    {
        get { return metadataRequestURI; }
        set
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value == String.Empty)
                throw new ArgumentException("value cannot be empty");
            metadataRequestURIChanged = true;
            metadataRequestURI = value;
        } 
    }

    /// <summary>
    /// Set it to true to notify the VirtualEarthTileLayer to reload the metadata.
    /// </summary>
    private bool keyChanged = false;
    /// <summary>
    /// The authentication key to VirtualEarth service.
    /// </summary>
    private string          key = String.Empty;
    public string           Key
    {
        get { return key; }
        set
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (value == String.Empty)
                throw new ArgumentException("value cannot be empty");
            keyChanged = true;
            key = value; 
        } 
    }

    private WWW             loader;

    /// <summary>
    /// Set to true when the VirtualEarthTileLayer is parsing the metadata.
    /// </summary>
    private bool            isParsingMetadata = false;

    #endregion

    #region MonoBehaviour implementation

    private new void Awake()
    {
        base.Awake();
        minZoom = 1;
        maxZoom = 23;
    }

    private void Update()
    {
        if ((hostnameChanged || keyChanged || metadataRequestURIChanged) && loader == null)
        {
            if (metadataRequestURI != null && metadataRequestURI != String.Empty
                && key != null && key != String.Empty)
            {
                string url = "http://" + hostname + "/" + metadataRequestURI + key;
                if (proxyURL != null)
                    url = (proxyURL.StartsWith("http://") ? "" : "http://") + proxyURL + (proxyURL.EndsWith("?") ? "" : "?") + "url=" + WWW.EscapeURL(url);

#if DEBUG_LOG
                Debug.Log("DEBUG: VirtualEarthTileLayer.Update: launching metadata request on: " + url);
#endif

                loader = new WWW(url);
            }
            else
                loader = null;

            hostnameChanged = false;
            keyChanged = false;
            metadataRequestURIChanged = false;
            isReadyToBeQueried = false;
        }
        else if (loader != null && loader.isDone)
        {
            if (loader.error != null || loader.text.Contains("404 Not Found"))
            {
#if DEBUG_LOG
				Debug.LogError("ERROR: VirtualEarthTileLayer.Update: loader [" + loader.url + "] error: " + loader.error);// + "(" + loader.text + ")");
#endif
                loader = null;
                return;
            }
            else
            {
                if (isParsingMetadata == false)
                {
#if DEBUG_LOG
                    Debug.Log("DEBUG: VirtualEarthTileLayer.Update: metadata response:\n" + loader.text);
#endif

                    byte[] bytes = loader.bytes;

                    isParsingMetadata = true;

                    UnityThreadHelper.CreateThread(() =>
                    {
                        UnitySlippyMap.VirtualEarth.Metadata metadata = null;
                        try
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(UnitySlippyMap.VirtualEarth.Metadata), "http://schemas.microsoft.com/search/local/ws/rest/v1");
                            metadata = xs.Deserialize(new MemoryStream(bytes)) as UnitySlippyMap.VirtualEarth.Metadata;

                            baseURL = (metadata.ResourceSets[0].Resources[0] as UnitySlippyMap.VirtualEarth.ImageryMetadata).ImageUrl.Replace("{culture}", CultureInfo.CurrentCulture.ToString());
                        }
                        catch (
                            Exception
#if DEBUG_LOG
                             e
#endif
                            )
                        {
#if DEBUG_LOG
                            Debug.LogError("ERROR: VirtualEarthTileLayer.Update: metadata deserialization exception:\n" + e.Source + " : " + e.InnerException + "\n" + e.Message + "\n" + e.StackTrace);
#endif
                        }

                        UnityThreadHelper.Dispatcher.Dispatch(() =>
                        {
#if DEBUG_LOG
                            Debug.Log("DEBUG: VirtualEarthTileLayer.Update: ImageUrl: " + (metadata.ResourceSets[0].Resources[0] as UnitySlippyMap.VirtualEarth.ImageryMetadata).ImageUrl);
#endif

                            isReadyToBeQueried = true;

                            loader = null;

                            isParsingMetadata = false;

                            if (needsToBeUpdatedWhenReady)
                            {
                                UpdateContent();
                                needsToBeUpdatedWhenReady = false;
                            }
                        });
                    });
                }
            }
        }
    }

    #endregion

    #region TileLayer implementation

    protected override void GetTileCountPerAxis(out int tileCountOnX, out int tileCountOnY)
    {
        tileCountOnX = tileCountOnY = (int)Mathf.Pow(2, Map.RoundedZoom);
    }

    protected override void GetCenterTile(int tileCountOnX, int tileCountOnY, out int tileX, out int tileY, out float offsetX, out float offsetZ)
    {
        int[] tileCoordinates = GeoHelpers.WGS84ToTile(Map.CenterWGS84[0], Map.CenterWGS84[1], Map.RoundedZoom);
        double[] centerTile = GeoHelpers.TileToWGS84(tileCoordinates[0], tileCoordinates[1], Map.RoundedZoom);
        double[] centerTileMeters = Map.WGS84ToEPSG900913Transform.Transform(centerTile); //GeoHelpers.WGS84ToMeters(centerTile[0], centerTile[1]);

        tileX = tileCoordinates[0];
        tileY = tileCoordinates[1];
        offsetX = Map.RoundedHalfMapScale / 2.0f - (float)(Map.CenterEPSG900913[0] - centerTileMeters[0]) * Map.RoundedScaleMultiplier;
        offsetZ = -Map.RoundedHalfMapScale / 2.0f - (float)(Map.CenterEPSG900913[1] - centerTileMeters[1]) * Map.RoundedScaleMultiplier;
    }

    protected override bool GetNeighbourTile(int tileX, int tileY, float offsetX, float offsetZ, int tileCountOnX, int tileCountOnY, NeighbourTileDirection dir, out int nTileX, out int nTileY, out float nOffsetX, out float nOffsetZ)
    {
        bool ret = false;
        nTileX = 0;
        nTileY = 0;
        nOffsetX = 0.0f;
        nOffsetZ = 0.0f;

        switch (dir)
        {
            case NeighbourTileDirection.South:
                if ((tileY + 1) < tileCountOnY)
                {
                    nTileX = tileX;
                    nTileY = tileY + 1;
                    nOffsetX = offsetX;
                    nOffsetZ = offsetZ - Map.RoundedHalfMapScale;
                    ret = true;
                }
                break;

            case NeighbourTileDirection.North:
                if (tileY > 0)
                {
                    nTileX = tileX;
                    nTileY = tileY - 1;
                    nOffsetX = offsetX;
                    nOffsetZ = offsetZ + Map.RoundedHalfMapScale;
                    ret = true;
                }
                break;

            case NeighbourTileDirection.East:
                nTileX = tileX + 1;
                nTileY = tileY;
                nOffsetX = offsetX + Map.RoundedHalfMapScale;
                nOffsetZ = offsetZ;
                ret = true;
                break;

            case NeighbourTileDirection.West:
                nTileX = tileX - 1;
                nTileY = tileY;
                nOffsetX = offsetX - Map.RoundedHalfMapScale;
                nOffsetZ = offsetZ;
                ret = true;
                break;
        }


        return ret;
    }
	
	#endregion
	
	#region WebTileLayer implementation
	
    protected override string GetTileURL(int tileX, int tileY, int roundedZoom)
    {
        string quadKey = TileSystem.TileXYToQuadKey(tileX, tileY, roundedZoom);
        string url = baseURL.Replace("{quadkey}", quadKey).Replace("{subdomain}", "t0");
        if (proxyURL != null)
            url = (proxyURL.StartsWith("http://") ? "" : "http://") + proxyURL + (proxyURL.EndsWith("?") ? "" : "?") + "key=" + key + "&url=" + WWW.EscapeURL(url);
        return url;
    }
	
    #endregion
}

}