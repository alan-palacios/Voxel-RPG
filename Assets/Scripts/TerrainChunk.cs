using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainChunk {

          public event System.Action<TerrainChunk, bool> onVisibilityChanged;
          const float colliderGenerationDistanceTreshold= 5f;

          public Vector2 coord;

          GameObject meshObject;
          Vector2 sampleCentre;
          Bounds bounds;

          MeshRenderer meshRenderer;
          MeshFilter meshFilter;
          MeshCollider meshCollider;

          float maxViewDst;

          LODInfo[]  detailLevels;
          LODMesh[] lodMeshes;
          int colliderLODIndex;

          public HeightMap heightMap {get; set;}
          bool heightMapReceived;
          int previousLODIndex = -1;
          bool hasSetCollider;

           HeightMapSettings heightMapSettings ;
          MeshSettings meshSettings;
          ObjectPlacingList objectPlacingList;
          GameObject [][] objetos;
          GameObject waterObj;

          Transform viewer;

          BiomeData [,] nearbyBiomes;


          public TerrainChunk(Vector2 coord, BiomesList biomesList, LODInfo[]  detailLevels, int colliderLODIndex, Transform parent, Transform viewer) {


                    nearbyBiomes = HeightMapGenerator.GenerateBiomeMap(2, 2, biomesList, coord);
                    BiomeData biomeData = nearbyBiomes[0,0];

                    this.coord = coord;
                    this.detailLevels=detailLevels;
                    this.colliderLODIndex= colliderLODIndex;
                    this.heightMapSettings= biomeData.heightMapSettings;
                    this.meshSettings=biomeData.meshSettings;
                    this.objectPlacingList = biomeData.objectPlacingList;

                    this.viewer=viewer;
                    sampleCentre = coord * meshSettings.chunkSize;//mapGenerator.biomeData.meshSettings.meshScale;
                    Vector2 position = coord*meshSettings.chunkSize;//new Vector3(sampleCentre.x,0,sampleCentre.y);
                    bounds = new Bounds(position,Vector2.one * meshSettings.chunkSize);



                    meshObject = new GameObject("Terrain Chunk");
                    meshRenderer = meshObject.AddComponent<MeshRenderer>();
                    meshFilter = meshObject.AddComponent<MeshFilter>();
                    meshCollider = meshObject.AddComponent<MeshCollider>();
                    meshRenderer.material=biomeData.textureData.material;


                    meshObject.transform.position = new Vector3(position.x,0, position.y);//position*mapGenerator.biomeData.meshSettings.meshScale;
                    meshObject.transform.parent = parent;
                    //meshObject.transform.localScale = Vector3.one*mapGenerator.biomeData.meshSettings.meshScale;

                    SetVisible(false);

                    lodMeshes = new LODMesh[detailLevels.Length];
                    for (int i = 0; i < detailLevels.Length; i++) {
                              lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                              lodMeshes[i].updateCallback+=UpdateTerrainChunk;
                              if (i==colliderLODIndex) {
                                        lodMeshes[i].updateCallback+=UpdateCollisionMesh;
                              }
                    }

                    maxViewDst = detailLevels[detailLevels.Length-1].visibleDstThreshold;

          }

          public void Load(BiomesList biomesList){
                    ThreadedDataRequested.RequestData(
                    () => HeightMapGenerator.GenerateHeightMap(nearbyBiomes[0,0], sampleCentre, biomesList),
                    OnHeightMapReceived);
          }

          void OnHeightMapReceived(object heightMapObject) {
                    ThreadedDataRequested.RemoveThread();
                    this.heightMap = (HeightMap)heightMapObject;
                    heightMapReceived = true;
                    /*if (GetTypeOfCorner()!=0) {
                              meshRenderer.material.SetInt("typeOfCorner", GetTypeOfCorner());
                              meshRenderer.material.SetFloat("chunkPosX", meshObject.transform.position.x);
                              meshRenderer.material.SetFloat("chunkPosZ", meshObject.transform.position.z);
                              meshRenderer.material.SetFloat("width", meshSettings.chunkSize);
                              switch (GetTypeOfCorner()) {
                                        case 1:
                                        setMaterialProperties(nearbyBiomes[1,0].textureData, nearbyBiomes[1,0].heightMapSettings,"R");
                                        break;
                                        case 5:
                                        setMaterialProperties(nearbyBiomes[0,1].textureData, nearbyBiomes[0,1].heightMapSettings, "F");
                                        break;
                                        case 6:
                                        setMaterialProperties(nearbyBiomes[1,0].textureData, nearbyBiomes[1,0].heightMapSettings, "R");
                                        break;
                              }
                    }*/
                    UpdateTerrainChunk ();
          }

          void setMaterialProperties(TextureData textureData, HeightMapSettings heightMapSettings, string side){
                    meshRenderer.material.SetInt("layerCount"+side, textureData.layers.Length);
                    meshRenderer.material.SetColorArray ("baseColours"+side, textureData.layers.Select(x => x.tint).ToArray());
                    meshRenderer.material.SetFloatArray ("baseStartHeights"+side, textureData.layers.Select(x => x.startHeight).ToArray());
                    meshRenderer.material.SetFloatArray ("baseBlends"+side, textureData.layers.Select(x => x.blendStrength).ToArray());
                    meshRenderer.material.SetFloatArray ("baseColourStrength"+side, textureData.layers.Select(x => x.tintStrength).ToArray());
                    meshRenderer.material.SetFloat("minHeight"+side, heightMapSettings.minHeight);
                    meshRenderer.material.SetFloat("maxHeight"+side, heightMapSettings.maxHeight);
          }

          Vector2 viewerPosition{
                    get{
                              return new Vector2(viewer.position.x, viewer.position.z);
                    }
          }

          public void UpdateTerrainChunk() {
                    if (heightMapReceived) {
                              float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
                              bool wasVisible= IsVisible();
                              bool visible = viewerDstFromNearestEdge <= maxViewDst;



                              if (visible) {
                                        int lodIndex = 0;

                                        for (int i = 0; i < detailLevels.Length - 1; i++) {
                                                  if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
                                                            lodIndex = i + 1;
                                                  } else {
                                                            break;
                                                  }
                                        }

                                        if (lodIndex != previousLODIndex) {
                                                  LODMesh lodMesh = lodMeshes [lodIndex];
                                                  if (lodMesh.hasMesh) {
                                                            previousLODIndex = lodIndex;
                                                            meshFilter.mesh = lodMesh.mesh;
                                                            if (lodMesh.lod==0) {
                                                                      ObjectGenerator.GenerateObjectsInGame(ref objetos, ref waterObj, objectPlacingList, heightMap, meshSettings.chunkSize,  meshObject.transform, coord);
                                                            }else{
                                                                      ObjectGenerator.DeleteObjectsInGame(ref objetos, ref waterObj, objectPlacingList.objectsSettings.Length);
                                                            }
                                                  } else if (!lodMesh.hasRequestedMesh) {
                                                            lodMesh.RequestMesh (heightMap, meshSettings);
                                                  }
                                        }

                              }

                              if (wasVisible!=visible) {
                                        SetVisible (visible);
                                        if (onVisibilityChanged!=null) {
                                                  onVisibilityChanged (this, visible);
                                        }
                              }

                    }
          }

          public void UpdateCollisionMesh(){
                    if (!hasSetCollider) {
                              float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                              if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold) {
                                        if (!lodMeshes[colliderLODIndex].hasRequestedMeshCollider) {
                                                  lodMeshes[colliderLODIndex].RequestMeshCollision(heightMap, meshSettings);
                                        }
                              }

                              if(sqrDstFromViewerToEdge <= colliderGenerationDistanceTreshold*colliderGenerationDistanceTreshold){

                                        if (lodMeshes[colliderLODIndex].hasMeshCollider) {

                                                  meshCollider.sharedMesh=lodMeshes[colliderLODIndex].meshCollider;
                                                  hasSetCollider=true;
                                        }
                              }
                    }
          }

          public void SetVisible(bool visible) {
                    meshObject.SetActive (visible);
          }

          public bool IsVisible() {
                    return meshObject.activeSelf;
          }

          public int GetTypeOfCorner(){
                    return heightMap.typeOfCorner;
          }
          public void SetMaterialShader(string path){
                    meshRenderer.material.shader = Shader.Find(path);
          }

}

class LODMesh {

          public Mesh mesh;
          public Mesh meshCollider;
          public bool hasRequestedMesh;
          public bool hasMesh;
          public bool hasRequestedMeshCollider;
          public bool hasMeshCollider;
          public int lod;
          public event System.Action updateCallback;

          public LODMesh(int lod) {
                    this.lod = lod;
          }

          public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
                    hasRequestedMesh = true;
                    ThreadedDataRequested.RequestData(
                    () => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod),
                    OnMeshDataReceived
                    );

                    //mapGenerator.RequestMeshData (heightMap, lod, OnMeshDataReceived);
          }

          void OnMeshDataReceived(object meshDataObject) {
                    ThreadedDataRequested.RemoveThread();
                    mesh = ((MeshData)meshDataObject).CreateMesh ();
                    hasMesh = true;
                    updateCallback ();
          }

          public void RequestMeshCollision(HeightMap heightMap, MeshSettings meshSettings) {
                    hasRequestedMeshCollider = true;
                    ThreadedDataRequested.RequestData(
                    () => MeshGenerator.GenerateColliderMesh(heightMap.values, meshSettings, lod),
                    OnMeshColliderReceived
                    );

          }

          void OnMeshColliderReceived( object collisionMeshObject) {
                    ThreadedDataRequested.RemoveThread();
                    meshCollider=((MeshForCollision)collisionMeshObject).CreateMesh();
                    hasMeshCollider=true;
                    updateCallback ();
          }
}
