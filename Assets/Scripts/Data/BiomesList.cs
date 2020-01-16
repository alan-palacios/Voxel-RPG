using System.Collections;
using UnityEngine;


[System.Serializable]
public struct BiomeData
{
/*[CreateAssetMenu()]
public class BiomeData : UpdatableData
{*/
          public string nombre;
          public float humidityStartHeight;
          public float wildnessStartHeight;
          public Color color;
          public HeightMapSettings heightMapSettings;
          public TextureData textureData;
          public ObjectPlacingList objectPlacingList;

}


[CreateAssetMenu()]
public class BiomesList : UpdatableData
{
    public BiomeData [] biomes;
    public HeightMapSettings wildSettings;
    public HeightMapSettings humiditySettings;
    public MeshSettings meshSettings;
}
