using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {


	const float viewerMoveThresholdForChunkUpdate = 50f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public BiomesList biomesList;
	public int[,] biomeMap;

	public HeightMapSettings wildSettings;
	public HeightMapSettings humiditySettings;

	public Transform viewer;
	public bool randPos;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	int chunksVisibleInViewDst;
	public float chunkSize=48;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	Color testColor;
	void Start() {

		/*foreach (BiomeData biomeData in biomesList.biomes) {
			biomeData.textureData.ApplyToMaterial();
		          biomeData.textureData.UpdateMeshHeights( biomeData.heightMapSettings.minHeight, biomeData.heightMapSettings.maxHeight);
		}*/

		if (randPos) {
			wildSettings.noiseSettings.seed = Random.Range(0,300);
			humiditySettings.noiseSettings.seed = Random.Range(0,300);

		}

		float maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;

		//chunkSize = biomesList.biomes[0].meshSettings.chunkSize;//-1
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

		UpdateVisibleChunks();

		StartCoroutine(DisplayPlayer());
	}

	IEnumerator DisplayPlayer(){
		yield return new WaitForSeconds(0);

		float yHeight = terrainChunkDictionary [new Vector2(0,0)].heightMap.values[ (int)chunkSize/2,  (int)chunkSize/2];
		viewer.position = new Vector3(0, yHeight+0.3f , 1);
	}

	void Update() {
		//testColor+=new Color(0.0F, 0.0F, 0.01F, 0.0F);
		//biomesList.biomes[0].textureData.material.SetColor("testColor", testColor );

		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);//mapGenerator.biomeData.meshSettings.meshScale;

		if (viewerPosition!=viewerPositionOld) {
			foreach (TerrainChunk chunk  in visibleTerrainChunks) {
				chunk.UpdateCollisionMesh();
			}
		}
		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks ();
		}
	}

	 void UpdateVisibleChunks() {
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

		for (int i = visibleTerrainChunks.Count-1 ; i >=0 ; i--) {
			alreadyUpdatedChunkCoords.Add(visibleTerrainChunks [i].coord);
			visibleTerrainChunks [i].UpdateTerrainChunk();
		}

		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
					if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
						terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();

					} else {
						//int xBiomeMapCord=(biomeMap.GetLength(0)/2)+(int)viewedChunkCoord.x-1;
						//int yBiomeMapCord=(biomeMap.GetLength(1)/2)+(int)viewedChunkCoord.y-1;

						TerrainChunk newChunk = new TerrainChunk (viewedChunkCoord, biomesList, detailLevels, colliderLODIndex,  transform, viewer);
						terrainChunkDictionary.Add (viewedChunkCoord, newChunk);
						newChunk.onVisibilityChanged +=onTerrainChunkVisibilityChanged;
						newChunk.Load( biomesList);
					}
				}

			}

		}
	}

		void onTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible){
			if (isVisible) {
				visibleTerrainChunks.Add (chunk);
			}else {
				visibleTerrainChunks.Remove (chunk);
			}
		}

	}



[System.Serializable]
public struct LODInfo {
	public int lod;
	public float visibleDstThreshold;

	public float sqrVisibleDstThreshold{
		get{
			return visibleDstThreshold*visibleDstThreshold;
		}
	}
}
