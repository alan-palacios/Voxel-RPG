using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;

	public enum DrawMode {NoiseMap, Mesh, BiomeMap};
	public DrawMode drawMode;

	public BiomeData biomeData;

	public BiomesList biomesList;


	[Range(0,6)]
	public int editorPreviewLOD;
	public bool autoUpdate;

	//public float meshScale =10f;
	public int noiseSize = 49;
	//public float chunkSize=48;

	public void DrawMapInEditor() {
		biomeData.textureData.ApplyToMaterial();
		biomeData.textureData.UpdateMeshHeights( biomeData.heightMapSettings.minHeight, biomeData.heightMapSettings.maxHeight);

		HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(noiseSize, noiseSize, biomeData.heightMapSettings,Vector2.zero);


		if (drawMode == DrawMode.NoiseMap) {
			DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
		} else if (drawMode == DrawMode.Mesh) {
			DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values, biomeData.meshSettings, editorPreviewLOD));
		} else if (drawMode == DrawMode.BiomeMap) {
			BiomeData[,] biomesMap = HeightMapGenerator.GenerateBiomeMap(100, 100, biomesList,Vector2.zero);
			DrawTexture (TextureGenerator.TextureFromBiomeMap (biomesMap));
		}
	}

	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 0, texture.height)/200f;

		textureRender.gameObject.SetActive(true);
		meshFilter.gameObject.SetActive(false);
	}

	public void DrawMesh(MeshData meshData) {
		meshFilter.sharedMesh = meshData.CreateMesh ();
		//meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator> ().biomeData.meshSettings.meshScale;

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
		if (biomeData.meshSettings!=null) {
			biomeData.meshSettings.OnValuesUpdated-=OnValuesUpdated;
			biomeData.meshSettings.OnValuesUpdated+=OnValuesUpdated;
		}

		if (biomeData.heightMapSettings!=null) {
			biomeData.meshSettings.OnValuesUpdated-=OnValuesUpdated;
			biomeData.heightMapSettings.OnValuesUpdated+=OnValuesUpdated;
		}

		if (biomeData.textureData!=null) {
			biomeData.textureData.OnValuesUpdated-=OnTextureValuesUpdated;
			biomeData.textureData.OnValuesUpdated+=OnTextureValuesUpdated;
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
