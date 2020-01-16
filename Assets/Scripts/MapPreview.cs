using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	GameObject [][] objetos;
	GameObject water;

	public enum DrawMode {NoiseMap, Mesh, BiomeMap, DeleteChilds};
	public DrawMode drawMode;

	public BiomeData biomeData;

	public BiomesList biomesList;


	[Range(0,6)]
	public int editorPreviewLOD;
	public bool autoUpdate;

	//public float meshScale =10f;
	
	int noiseSize ;
	//public float chunkSize=48;

	public void DrawMapInEditor() {
		noiseSize= biomesList.meshSettings.noiseSize;
		biomeData.textureData.ApplyToMaterial();
		biomeData.textureData.UpdateMeshHeights( biomeData.heightMapSettings.minHeight, biomeData.heightMapSettings.maxHeight);

		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(noiseSize, noiseSize, biomeData.heightMapSettings,Vector2.zero);


		if (drawMode == DrawMode.NoiseMap) {
			DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
		} else if (drawMode == DrawMode.BiomeMap) {
			BiomeData[,] biomesMap = HeightMapGenerator.GenerateBiomeMap(100, 100, biomesList,Vector2.zero);
			DrawTexture (TextureGenerator.TextureFromBiomeMap (biomesMap));
		}else if (drawMode == DrawMode.Mesh) {
			DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values, biomesList.meshSettings, editorPreviewLOD), heightMap);
		} else if (drawMode == DrawMode.DeleteChilds) {
			foreach (Transform child in meshRenderer.gameObject.transform) {
			     GameObject.DestroyImmediate(child.gameObject);
			 }
		}
	}

	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 0, texture.height)/200f;

		textureRender.gameObject.SetActive(true);
		meshFilter.gameObject.SetActive(false);

		ObjectGenerator.DeleteObjects(ref objetos, ref water, biomeData.objectPlacingList.objectsSettings.Length);
	}

	public void DrawMesh(MeshData meshData, HeightMap heightMap) {
		meshFilter.sharedMesh = meshData.CreateMesh ();

		ObjectGenerator.DeleteObjects(ref objetos, ref water, biomeData.objectPlacingList.objectsSettings.Length);
		ObjectGenerator.GenerateObjects(ref objetos, ref water, biomeData.objectPlacingList, heightMap, biomesList.meshSettings.chunkSize,  meshFilter.transform );

		textureRender.gameObject.SetActive(false);
		meshFilter.gameObject.SetActive(true);
	}

	void OnValuesUpdated(){
		if (!Application.isPlaying) {
			DrawMapInEditor();
		}

	}

	void OnTextureValuesUpdated(){
		meshRenderer.material = biomeData.textureData.material;
		biomeData.textureData.ApplyToMaterial();

	}



	public void OnValidate(){
		if (biomesList.meshSettings!=null) {
			biomesList.meshSettings.OnValuesUpdated-=OnValuesUpdated;
			biomesList.meshSettings.OnValuesUpdated+=OnValuesUpdated;
		}

		if (biomeData.heightMapSettings!=null) {
			//biomesList.meshSettings.OnValuesUpdated-=OnValuesUpdated;
			biomeData.heightMapSettings.OnValuesUpdated+=OnValuesUpdated;
		}

		if (biomeData.textureData!=null) {
			biomeData.textureData.OnValuesUpdated-=OnTextureValuesUpdated;
			biomeData.textureData.OnValuesUpdated+=OnTextureValuesUpdated;
		}

		if (biomeData.objectPlacingList!=null) {
			biomeData.objectPlacingList.OnValuesUpdated-=OnValuesUpdated;
			biomeData.objectPlacingList.OnValuesUpdated+=OnValuesUpdated;
		}

		if (biomesList!=null) {
			biomesList.OnValuesUpdated+=OnValuesUpdated;
		}

		if (biomesList.wildSettings!=null) {
			biomesList.wildSettings.OnValuesUpdated+=OnValuesUpdated;
		}
		if (biomesList.humiditySettings!=null) {
			biomesList.humiditySettings.OnValuesUpdated+=OnValuesUpdated;
		}

	}

}
