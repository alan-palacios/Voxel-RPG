using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectGenerator
{
          public static void GenerateObjects(ref GameObject[][] objetos, ref GameObject water, ObjectPlacingList objectPlacingList,
                    HeightMap heightMap, float chunkSize,  Transform parent){
                    int longLista = objectPlacingList.objectsSettings.Length;
                    float[,] heightValues = heightMap.values;
                    if (longLista>0) {
                              objetos = new GameObject[longLista][];

                              for (int i=0; i<longLista; i++) {

                                        List <Vector2> originalPoints = PoissonDiscSampling.GeneratePoints(objectPlacingList.objectsSettings[i].radius,
                                                  new Vector2( chunkSize, chunkSize), objectPlacingList.objectsSettings[i].rejectionSamples );

                                        List<Vector2> points = new List<Vector2>();

                                        for (int x=0; x<originalPoints.Count; x++) {
                                                  float heightValue = heightValues[ (int)originalPoints[x].x, (int)originalPoints[x].y ];
                                                  if ( heightValue >= objectPlacingList.objectsSettings[i].startHeight && heightValue <= objectPlacingList.objectsSettings[i].endHeight ) {
                                                            points.Add(originalPoints[x]);
                                                  }
                                        }

                                        objetos[i]= new GameObject[points.Count];

                                        for (int j=0; j<objetos[i].Length; j++) {

                                                  int modelsCount = objectPlacingList.objectsSettings[i].modelos.Length;

                                                  GameObject objectPlaced = objectPlacingList.objectsSettings[i].modelos[Random.Range(0,modelsCount)];
                                                  MeshFilter viewedModelFilter = objectPlaced.GetComponent<MeshFilter>();

                                                  float xCoord = -chunkSize/2+points[j].x;
                                                  float yCoord = -chunkSize/2+points[j].y;
                                                  float heightCoord = heightValues[ (int)points[j].x, (int)points[j].y ] -
                                                                                          objectPlacingList.objectsSettings[i].offsetHeight;
                                                  //float heightCoord = heightValues[ (int)points[j].x, (int)points[j].y ] +
                                                  //                                        viewedModelFilter.sharedMesh.bounds.size.y*objectPlaced.transform.localScale.y/2 -
                                                  //                                        objectPlacingList.objectsSettings[i].offsetHeight;


                                                  Vector3 angles = new Vector3( 0, Random.Range(0, 36)*10, 0);
                                                  objetos[i][j] = GameObject.Instantiate(objectPlaced, new Vector3(  xCoord, heightCoord  ,yCoord)  , Quaternion.Euler(angles), parent ) as GameObject;

                                                  if (objectPlacingList.objectsSettings[i].randomMaterial) {
                                                            int materialCount = objectPlacingList.objectsSettings[i].materiales.Length;
                                                            Renderer objRenderer = objetos[i][j].GetComponent<Renderer>();
                                                            objRenderer.sharedMaterial= objectPlacingList.objectsSettings[i].materiales[Random.Range(0,materialCount)];
                                                  }
                                                  if (objectPlacingList.objectsSettings[i].randomScale) {
                                                            float newScale = Random.Range( objectPlacingList.objectsSettings[i].minScale, objectPlacingList.objectsSettings[i].maxScale );

                                                            objetos[i][j].transform.localScale = Vector3.one*Mathf.Floor (newScale / 0.1f)*0.1f;
                                                  }

                                        }
                              }
                    }

                    if (heightMap.minValue <= objectPlacingList.waterHeightPos - objectPlacingList.minDstWaterGround) {
                              water = GameObject.Instantiate(objectPlacingList.waterObj, new Vector3(  0, objectPlacingList.waterHeightPos  ,0)  , Quaternion.identity, parent) as GameObject;
                    }

          }

          public static void DeleteObjects( ref GameObject [][] objetos, ref GameObject water, int longObjetos ){
                    if ( objetos != null) {
                              if (longObjetos>0) {
                                        for (int i=0; i<objetos.Length; i++) {
                                                  for (int j=0; j<objetos[i].Length; j++) {
                                                            Object.DestroyImmediate(objetos[i][j]);
                                                  }
                                        }
                                        objetos=null;
                              }
                    }
                    if (water!=null) {
                              Object.DestroyImmediate(water);
                    }
          }

          public static void GenerateObjectsInGame(  GameObject[][] objetos, ObjectPlacingList objectPlacingList,
                    HeightMap heightMap, float chunkSize,  GameObject parentObj, Vector2 coord){

                              float[,] heightValues = heightMap.values;
                              int longLista = objectPlacingList.objectsSettings.Length;
                              if (longLista>0) {
                                        objetos = new GameObject[longLista][];

                                        for (int i=0; i<longLista; i++) {

                                                  List <Vector2> originalPoints = PoissonDiscSampling.GeneratePoints(objectPlacingList.objectsSettings[i].radius,
                                                  new Vector2( chunkSize, chunkSize), objectPlacingList.objectsSettings[i].rejectionSamples );

                                                  List<Vector2> points = new List<Vector2>();

                                                  for (int x=0; x<originalPoints.Count; x++) {
                                                            float heightValue = heightValues[ (int)originalPoints[x].x, (int)originalPoints[x].y ];
                                                            if ( heightValue >= objectPlacingList.objectsSettings[i].startHeight && heightValue <= objectPlacingList.objectsSettings[i].endHeight ) {
                                                                      points.Add(originalPoints[x]);
                                                            }
                                                  }

                                                  objetos[i]= new GameObject[points.Count];

                                                  for (int j=0; j<objetos[i].Length; j++) {
                                                            int modelsCount = objectPlacingList.objectsSettings[i].modelos.Length;

                                                            GameObject objectPlaced = objectPlacingList.objectsSettings[i].modelos[Random.Range(0,modelsCount)];
                                                            MeshFilter viewedModelFilter = objectPlaced.GetComponent<MeshFilter>();

                                                            float xCoord =  coord.x*chunkSize-chunkSize/2+points[j].x;
                                                            float yCoord = coord.y*chunkSize-chunkSize/2+points[j].y;
                                                            float heightCoord = heightValues[ (int)points[j].x, (int)points[j].y ] -
                                                            objectPlacingList.objectsSettings[i].offsetHeight;


                                                            Vector3 angles = new Vector3(0,0,0);
                                                            angles.y = Random.Range(0, 6)*60;

                                                            objetos[i][j] = GameObject.Instantiate(objectPlaced, new Vector3(  xCoord, heightCoord  ,yCoord)  , Quaternion.Euler(angles), parentObj.transform) as GameObject;

                                                            /*Vector3 newScale = objetos[i][j].transform.localScale;
                                                            newScale *= 1.6f;
                                                            objetos[i][j].transform.localScale = newScale;*/
                                                            if (objectPlacingList.objectsSettings[i].randomMaterial) {
                                                                      int materialCount = objectPlacingList.objectsSettings[i].materiales.Length;
                                                                      Renderer objRenderer = objetos[i][j].GetComponent<Renderer>();
                                                                      objRenderer.sharedMaterial= objectPlacingList.objectsSettings[i].materiales[Random.Range(0,materialCount)];
                                                            }
                                                            if (objectPlacingList.objectsSettings[i].randomScale) {
                                                                      float newScale = Random.Range( objectPlacingList.objectsSettings[i].minScale, objectPlacingList.objectsSettings[i].maxScale );

                                                                      objetos[i][j].transform.localScale = Vector3.one*Mathf.Floor (newScale / 0.1f)*0.1f;
                                                            }
                                                  }
                                        }
                              }


          }
          public static void GenerateWaterInGame( GameObject waterObj, ObjectPlacingList objectPlacingList,
                    HeightMap heightMap, float chunkSize,  GameObject parentObj, Vector2 coord){

                              if (objectPlacingList.waterObj!=null) {
                                        if (heightMap.minValue <= objectPlacingList.waterHeightPos - objectPlacingList.minDstWaterGround) {
                                                  waterObj = GameObject.Instantiate(objectPlacingList.waterObj,
                                                  new Vector3(  coord.x*chunkSize, objectPlacingList.waterHeightPos  , coord.y*chunkSize)  , Quaternion.identity, parentObj.transform) as GameObject;
                                        }
                              }

          }

          public static void DeleteObjectsInGame( ref GameObject [][] objetos, ref GameObject waterObj, int longObjetos ){
                    if ( !System.Object.ReferenceEquals(objetos, null)) {
                              if (longObjetos>0) {
                                        for (int i=0; i<objetos.Length; i++) {
                                                  for (int j=0; j<objetos[i].Length; j++) {
                                                            objetos[i][j].SetActive(false);
                                                  }
                                        }
                              }
                    }
                    if (!System.Object.ReferenceEquals(waterObj, null)) {
                              waterObj.SetActive(false);
                    }
          }

}
