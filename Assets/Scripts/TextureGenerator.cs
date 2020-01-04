using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colourMap);
		texture.Apply ();
		return texture;
	}


	public static Texture2D TextureFromHeightMap(HeightMap heightMap) {
		int width = heightMap.values.GetLength (0);
		int height = heightMap.values.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue,heightMap.maxValue, heightMap.values [x, y]));
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

	public static Texture2D TextureFromBiomeMap(BiomeData[,] biomesMap) {
		int width = biomesMap.GetLength (0);
		int height = biomesMap.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				if (x<width-1 && y<height-1) {
					colourMap [y * width + x] = biomesMap[x,y].color;
				}else {
					colourMap [y * width + x] = (biomesMap[x,y].color+Color.black)/2;
				}


			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

	/*public static Texture2D TextureFromBiomeMap(int[,] biomeMap, BiomeData [] biomes) {
		int width = biomeMap.GetLength (0);
		int height = biomeMap.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				if (x<width-1 && y<height-1) {
					colourMap [y * width + x] = (biomes[ biomeMap[x,y] ].color+biomes[ biomeMap[x+1,y+1] ].color)/2;
				}else {
					colourMap [y * width + x] = (biomes[ biomeMap[x,y] ].color+Color.black)/2;
				}


			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}*/

}
