using UnityEngine;
using System.Collections;

public class Resize : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		ResizeMeshToUnit (this.gameObject);
	}

	void ResizeMeshToUnit(GameObject t) {
		MeshFilter mf = t.GetComponent<MeshFilter>();
		if (mf == null)
			return;
		Mesh mesh = mf.sharedMesh;
		
		//***Set this to renderer bounds instead of mesh bounds***
		Bounds bounds = t.renderer.bounds;
		
		float size = bounds.size.x;
		if (size < bounds.size.y)
			size = bounds.size.y;
		if (size < bounds.size.z)
			size = bounds.size.z;
		
		if (Mathf.Abs(1.0f - size) < 0.01f) {
			Debug.Log ("Already unit size");
			return;
		}
		
		float scale = 1.0f / size;
		
		Vector3[] verts = mesh.vertices;
		
		for (int i = 0; i < verts.Length; i++) {
			verts[i] = verts[i] * scale;
		}
		
		mesh.vertices = verts;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}
}
