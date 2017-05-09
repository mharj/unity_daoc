using UnityEngine;
using System.Collections;

public class SceneryLoad : MonoBehaviour {
	void Awake() 
	{

	}

	// Use this for initialization
	void Start () {
		GameObject board = new GameObject();
		board.name = "zone000";
		ZoneLoader test = new ZoneLoader(0);
		GameObject zone000 = Terrain.CreateTerrainGameObject(test.terrainData);
		Terrain.activeTerrain.basemapDistance = 90500;

//		PcxLoader terrain = new PcxLoader("Assets/zones/zone000/terrain.pcx");
		Debug.Log("Hello World!");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
