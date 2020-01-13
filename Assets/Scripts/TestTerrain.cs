using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTerrain : MonoBehaviour
{
    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    Mesh mesh;
    Vector3 [] vertex;

    void Start()
    {
              HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(49, 49, heightMapSettings,Vector2.zero);
              mesh = this.GetComponent<MeshFilter>().mesh;
             mesh.vertices = MeshGenerator.GenerateTestTerrainMesh(heightMap.values, meshSettings, 0 );
             mesh.RecalculateBounds();
    }

}
