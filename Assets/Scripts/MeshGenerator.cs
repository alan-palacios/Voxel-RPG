using UnityEngine;
using System.Collections;

public static class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail) {

		int width = heightMap.GetLength (0)-1;
		int height = heightMap.GetLength (1)-1;

		float topLeftX = (width ) / -2f;
 		float topLeftZ = (height) / -2f;

		int meshSimplificationIncrement= (levelOfDetail==0)?1:levelOfDetail*2;

		int meshWidth=width*2/meshSimplificationIncrement + 1;

		MeshData meshData = new MeshData (meshWidth);

		int vertexIndex = 0;

		for (int x=0; x<width; x+=meshSimplificationIncrement) {
			for (int y = 0; y < width; y+=meshSimplificationIncrement) {

				float xWoutIncrement=(topLeftX+x);
				float xWithIncrement=(topLeftX+x+meshSimplificationIncrement);

				float yWoutIncrement=(topLeftZ+y);
				float yWithIncrement=(topLeftZ+y+meshSimplificationIncrement);
				//terreno
				if ( y==0) {
					if(x==0){
						meshData.vertices [vertexIndex] = new Vector3 ( xWoutIncrement, heightMap [x, y], yWoutIncrement );//down left
						//meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);
					}

					meshData.vertices [vertexIndex+1] = new Vector3 ( xWithIncrement, heightMap [x, y], yWoutIncrement );//down right
					//meshData.uvs [vertexIndex+1] = new Vector2 (  x/ (float)width, y / (float)height);

				}

				//over triangles
					if (x==0) {
						meshData.vertices [vertexIndex+meshWidth] = new Vector3 ( xWoutIncrement, heightMap [x, y], yWithIncrement);//up left
						//meshData.uvs [vertexIndex+meshWidth] = new Vector2 ( x / (float)width,y/ (float)height);
					}

					meshData.vertices [vertexIndex+meshWidth+1] = new Vector3 (xWithIncrement, heightMap [x, y], yWithIncrement);//up right
					//meshData.uvs [vertexIndex+meshWidth+1] = new Vector2 ( x/ (float)width, y / (float)height);

					meshData.AddTriangle (vertexIndex +meshWidth, vertexIndex+1, vertexIndex );
					meshData.AddTriangle (vertexIndex + 1, vertexIndex+meshWidth, vertexIndex+meshWidth+1);

				//side triangles
					meshData.vertices [vertexIndex+2] = new Vector3 (xWithIncrement, heightMap [x+meshSimplificationIncrement, y] , yWoutIncrement);// side-r down right
					//meshData.uvs [vertexIndex+2] = new Vector2 ( (x+meshSimplificationIncrement)/ (float)width, y / (float)height);

					meshData.vertices [vertexIndex+meshWidth+2] = new Vector3 ( xWithIncrement, heightMap [x+meshSimplificationIncrement, y],yWithIncrement);//side-r up right
					//meshData.uvs [vertexIndex+meshWidth+2] = new Vector2 ( (x+meshSimplificationIncrement) / (float)width, y / (float)height);

					meshData.AddTriangle (vertexIndex+meshWidth+1, vertexIndex+2, vertexIndex+1 );
					meshData.AddTriangle (vertexIndex+2, vertexIndex+meshWidth+1, vertexIndex+meshWidth+ 2);

				//actualizar a la siguiente fila de vectores de enfrente
				vertexIndex+=meshWidth*2;

				//front triangles
					meshData.vertices [vertexIndex] = new Vector3 ( xWoutIncrement, heightMap [x, y+meshSimplificationIncrement], yWithIncrement );//front up left
					//meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, (y+meshSimplificationIncrement)/ (float)height);

					meshData.vertices [vertexIndex+1] = new Vector3 ( xWithIncrement, heightMap [x, y+meshSimplificationIncrement], yWithIncrement);//front up right
					//meshData.uvs [vertexIndex+1] = new Vector2 ( x / (float)width, (y+meshSimplificationIncrement)/ (float)height);

					meshData.AddTriangle (vertexIndex, vertexIndex-meshWidth + 1, vertexIndex-meshWidth );
					meshData.AddTriangle (vertexIndex-meshWidth + 1, vertexIndex, vertexIndex +1);


				if( y == ( width - meshSimplificationIncrement) ){
					vertexIndex=( (x/meshSimplificationIncrement)*2)+2;
				}


			}

		}
		meshData.FlatShading();

		return meshData;
	}

	public static MeshForCollision GenerateColliderMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail) {

		int width = heightMap.GetLength (0)-1;
		int height = heightMap.GetLength (1)-1;

		float topLeftX = (width ) / -2f;
 		float topLeftZ = (height) / -2f;

		int meshSimplificationIncrement= (levelOfDetail==0)?1:levelOfDetail*2;

		MeshForCollision meshCollision = new MeshForCollision (width/meshSimplificationIncrement+1);

		int colIndex=0;

		for (int x=0; x<width; x+=meshSimplificationIncrement) {
			for (int y = 0; y < width; y+=meshSimplificationIncrement) {

				float xWithIncrement=topLeftX+x+meshSimplificationIncrement/2f;
				float xIncrementOneFive=topLeftX+x+1.5f*meshSimplificationIncrement;

				float yWithIncrement=topLeftZ+y+meshSimplificationIncrement/2f;
				float yIncrementOneFive=topLeftZ+y+1.5f*meshSimplificationIncrement;

				//colision
				meshCollision.vertices [colIndex] = new Vector3 ( xWithIncrement , heightMap [x, y],  yWithIncrement );
				meshCollision.AddTriangle (colIndex+1, colIndex + width/meshSimplificationIncrement + 1, colIndex);
				meshCollision.AddTriangle (colIndex + width/meshSimplificationIncrement + 1, colIndex+1, colIndex + width/meshSimplificationIncrement + 2);

				if( x == ( width - meshSimplificationIncrement) ){
					meshCollision.vertices [colIndex+width/meshSimplificationIncrement+1] = new Vector3 (xIncrementOneFive , heightMap [x+meshSimplificationIncrement, y],  yWithIncrement );
				}

				if( y == ( width - meshSimplificationIncrement) ){
					colIndex++;
					meshCollision.vertices [colIndex] = new Vector3 ( xWithIncrement, heightMap [x, y+meshSimplificationIncrement], yIncrementOneFive  );
				}

				if( x == ( width - meshSimplificationIncrement) && y == ( width - meshSimplificationIncrement) ){
					meshCollision.vertices [colIndex+width/meshSimplificationIncrement+1] = new Vector3 ( xIncrementOneFive, heightMap [x+meshSimplificationIncrement, y+meshSimplificationIncrement],  yIncrementOneFive );
				}

				colIndex++;
			}

		}
		return meshCollision;
	}
}

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	//public Vector2[] uvs;

	int triangleIndex;

	public MeshData( int meshWidth) {
		vertices = new Vector3[meshWidth* meshWidth];
		//uvs = new Vector2[meshWidth* meshWidth];

		int noiseWidth= (meshWidth-1)/2;
		triangles = new int[ 18*noiseWidth*noiseWidth];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}


	public void FlatShading() {
		Vector3[] flatShadedVertices = new Vector3[triangles.Length];
		//Vector2[] flatShadedUvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++) {
			flatShadedVertices [i] = vertices [triangles [i]];
			//flatShadedUvs [i] = uvs [triangles [i]];
			triangles [i] = i;
		}

		vertices = flatShadedVertices;
		//uvs = flatShadedUvs;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		//mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}

}


public class MeshForCollision {
	public Vector3[] vertices;
	public int[] triangles;

	int triangleIndex;

	public MeshForCollision( int meshWidth) {
		vertices = new Vector3[meshWidth* meshWidth];
		triangles = new int[ (meshWidth-1)*(meshWidth-1)*6 ];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		return mesh;
	}

}
