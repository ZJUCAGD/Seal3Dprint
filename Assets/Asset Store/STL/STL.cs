/*
	AUTHOR
	=============
	Created by Carl Emil Carlsen.
	Copyright 2015 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk


	LICENSE
	=======
	This is a Unity Asset Store product.
	https://www.assetstore.unity3d.com/en/#!/content/3397
	

	VERSION
	=============
	1.4
	

	DESCRIPTION
	=============
	
	STL exports unity meshes to the STL file format which is widely used for 
	rapid prototyping and computer-aided manufacturing (3D printing).
	
	Use the methods ExportBinary() to export a binary STL file and ExportText() to export
	a text based STL file.
	
	
	CHANGES
	=======

		1.4  (Dec 9, 2015)
			– Fixed warning introduced in Unity 5.3

		1.3  (Sep 18, 2015)
			– BEWARE this update will break previous code!
			– Added a runtime example.
			– Added support for SkinnedMeshRenderers.
			– Changed the naming of methods.
			– Removed script reference in the top of the script.
			– Added inline documentation.

		1.2  (Jul 09, 2014)
			– Fixed an out of memory issue with ExportText() when exporting extremely large meshes.
			– Removed a warning message that was displayed when cancelling an export in the editor.
			
		1.1  
			– Fixed a float formating bug in exported text based STL files.
			– Fixed a missing end statement in exported text based STL files.
			
		1.0 initial version

*/	

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class STL
{

	/// <summary>
	/// Exports all meshes found in MeshFilter and SkinnedMeshRenderer components attached to the supplied game objects (and their children) to a binary stl file at specified file path.
	/// </summary>
	/// <param name="gameObjects">Game objects.</param>
	/// <param name="filePath">File path.</param>
	public static void ExportBinary( GameObject[] gameObjects, string filePath )
	{
		Mesh[] meshes;
		Matrix4x4[] matrices;
		GetMeshesAndMatrixes( gameObjects, out meshes, out matrices );
		ExportBinary( meshes, matrices, filePath );
	}


	/// <summary>
	/// Exports all meshes found in supplied MeshFilters to a binary stl file at specified file path.
	/// </summary>
	/// <param name="gameObjects">An array of MeshFilter objects.</param>
	/// <param name="filePath">File path.</param>
	public static void ExportBinary( MeshFilter[] filters, string filePath )
	{
		Mesh[] meshes;
		Matrix4x4[] matrices;
		GetMeshesAndMatrixes( filters, out meshes, out matrices );
		ExportBinary( meshes, matrices, filePath );
	}


	/// <summary>
	/// Exports all meshes found in supplied MeshFilters to a binary stl file at specified file path.
	/// </summary>
	/// <param name="gameObjects">An array of SkinnedMeshRenderer objects.</param>
	/// <param name="filePath">File path.</param>
	public static void ExportBinary( SkinnedMeshRenderer[] skins, string filePath )
	{
		Mesh[] meshes;
		Matrix4x4[] matrices;
		GetMeshesAndMatrixes( skins, out meshes, out matrices );
		ExportBinary( meshes, matrices, filePath );
	}


	/// <summary>
	/// Exports a mesh with matrix transformation to a binary stl file at specified file path.
	/// </summary>
	/// <param name="mesh">Mesh.</param>
	/// <param name="matrix">Matrix.</param>
	/// <param name="filePath">File path.</param>
	public static void ExportBinary( Mesh mesh, Matrix4x4 matrix, string filePath ){ ExportBinary( new Mesh[]{ mesh }, new Matrix4x4[]{ matrix }, filePath ); }
	

	/// <summary>
	/// Exports meshes with matrix transformations to a binary stl file at specified file path.
	/// </summary>
	/// <param name="meshes">Meshes.</param>
	/// <param name="matrices">Matrices.</param>
	/// <param name="filePath">File Path.</param>
	public static void ExportBinary( Mesh[] meshes, Matrix4x4[] matrices, string filePath )
	{
		if( meshes.Length != matrices.Length ){
			Debug.LogError( "Mesh array length and matrix array length must match." );
			return;
		}

		try
		{
			using( BinaryWriter writer = new BinaryWriter( File.Open( filePath, FileMode.Create ) ) )
			{
				// write header
				writer.Write( new char[ 80 ] );
				
				// count all triangles and write
				int triangleIndexCount = 0;
				foreach( Mesh mesh in meshes ) {
					for( int s = 0; s < mesh.subMeshCount; s++ ){
						triangleIndexCount += mesh.GetTriangles( s ).Length;
					}
				}
				uint triangleCount = (uint) ( triangleIndexCount / 3 );
				writer.Write( triangleCount );
				
				// for each mesh filter ...
				int i;
				short attribute = 0;
				Vector3 u, v, normal;
				int[] triangles;
				Vector3[] vertices;
				for( int m=0; m<meshes.Length; m++ )
				{
					// get vertices and tranform them
					vertices = meshes[m].vertices;
					for( int vx = 0; vx < vertices.Length; vx++ ){
						vertices[ vx ] = matrices[m].MultiplyPoint( vertices[ vx ] );
					}
					
					// for each sub mesh ...
					for( int s = 0; s < meshes[m].subMeshCount; s++ )
					{
						// get trianlges
						triangles = meshes[m].GetTriangles( s );
						
						// for each triangle ...
						for( int t = 0; t < triangles.Length; t += 3 )
						{
							// calculate and write normal
							u = vertices[ triangles[ t+1 ] ] - vertices[ triangles[ t ] ];
							v = vertices[ triangles[ t+2 ] ] - vertices[ triangles[ t ] ];
							normal = new Vector3( u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x );
							for( i = 0; i < 3; i++ ) writer.Write( normal[ i ] );
							
							// write vertices
							for( i = 0; i < 3; i++ ) writer.Write( vertices[ triangles[ t ] ][i] );
							for( i = 0; i < 3; i++ ) writer.Write( vertices[ triangles[ t+1 ] ][i] );
							for( i = 0; i < 3; i++ ) writer.Write( vertices[ triangles[ t+2 ] ][i] );
							
							// write attribute byte count
							writer.Write( attribute );
						}
					}
				}
				
				// the end
				writer.Close();
			}
		}
		catch( System.Exception e ){
			Debug.LogWarning( "FAILED exporting STL object at : " + filePath + "\n" + e );
		}
	}


	/// <summary>
	/// Exports all meshes found in MeshFilter and SkinnedMeshRenderer components attached to the supplied game objects (or their children) to a text based stl file at specified file path.
	/// </summary>
	/// <param name="gameObjects">Game objects.</param>
	/// <param name="filePath">File path.</param>
	public static void ExportText( GameObject[] gameObjects, string filePath )
	{
		Mesh[] meshes;
		Matrix4x4[] matrices;
		STL.GetMeshesAndMatrixes( gameObjects, out meshes, out matrices );
		ExportText( meshes, matrices, filePath );
	}


	/// <summary>
	/// Exports a mesh with matrix transformation to a binary text based file at specified file path.
	/// </summary>
	/// <param name="mesh">Mesh.</param>
	/// <param name="matrix">Matrix.</param>
	/// <param name="filePath">File path.</param>
	public static void ExportText( Mesh mesh, Matrix4x4 matrix, string filePath ){ ExportBinary( new Mesh[]{ mesh }, new Matrix4x4[]{ matrix }, filePath ); }
	

	/// <summary>
	/// Exports meshes with matrix transformations to a text based stl file at specified file path.
	/// </summary>
	/// <param name="meshes">Meshes.</param>
	/// <param name="matrices">Matrices.</param>
	/// <param name="filePath">File Path.</param>
	public static void ExportText( Mesh[] meshes, Matrix4x4[] matrices, string filePath )
	{
		if( meshes.Length != matrices.Length ){
			Debug.LogError( "Mesh array length and matrix array length must match." );
			return;
		}
		try
		{
			bool append = false;
			using( StreamWriter sw = new StreamWriter( filePath, append ) ) 
			{
				// write header to disk
				sw.WriteLine( "Solid Unity Mesh" );
				
				// for each mesh filter ...
				Vector3 u, v, normal;
				int[] triangles;
				Vector3[] vertices;
				System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" );
				for( int m=0; m<meshes.Length; m++ )
				{
					// a new string builder for each mesh to avoid out of memory errors
					StringBuilder sb = new StringBuilder();
					
					// get vertices and tranform them
					vertices = meshes[m].vertices;
					for( int vx = 0; vx < vertices.Length; vx++ ){
						vertices[ vx ] = matrices[m].MultiplyPoint( vertices[ vx ] );
					}
					
					// for each sub mesh ...
					for( int s = 0; s < meshes[m].subMeshCount; s++ )
					{
						// get trianlges
						triangles = meshes[m].GetTriangles( s );
						
						// for each triangle ...
						for( int t = 0; t < triangles.Length; t += 3 )
						{
							// calculate and write normal
							u = vertices[ triangles[ t+1 ] ] - vertices[ triangles[ t ] ];
							v = vertices[ triangles[ t+2 ] ] - vertices[ triangles[ t ] ];
							normal = new Vector3( u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x );
							sb.AppendLine( "facet normal " + normal.x.ToString("e",ci) + " " + normal.y.ToString("e",ci) + " " + normal.z.ToString("e",ci) );
							
							// begin triangle
							sb.AppendLine( "outer loop" );
							
							// write vertices
							sb.AppendLine( "vertex " + vertices[ triangles[ t ] ].x.ToString("e",ci) + " " + vertices[ triangles[ t ] ].y.ToString("e",ci) + " " + vertices[ triangles[ t ] ].z.ToString("e",ci) );
							sb.AppendLine( "vertex " + vertices[ triangles[ t+1 ] ].x.ToString("e",ci) + " " + vertices[ triangles[ t+1 ] ].y.ToString("e",ci) + " " + vertices[ triangles[ t+1 ] ].z.ToString("e",ci) );
							sb.AppendLine( "vertex " + vertices[ triangles[ t+2 ] ].x.ToString("e",ci) + " " + vertices[ triangles[ t+2 ] ].y.ToString("e",ci) + " " + vertices[ triangles[ t+2 ] ].z.ToString("e",ci) );
							
							// end triangle
							sb.AppendLine( "endloop" );
							sb.AppendLine( "endfacet" );
						}
					}
					
					// write string builder memory to the disk
					sw.Write( sb.ToString() );
				}
				
				// write ending to disk and close writer
				sw.WriteLine( "endsolid Unity Mesh" );
				sw.Close();
			}
		}
		catch( System.Exception e ){
			Debug.LogWarning( "FAILED exporting wavefront obj at : " + filePath + "\n" + e );
		}
	}


	public static void GetMeshesAndMatrixes( GameObject[] objects, out Mesh[] meshes, out Matrix4x4[] matrices )
	{
		List<Mesh> meshList = new List<Mesh>();
		List<Matrix4x4> matrixList = new List<Matrix4x4>();
		for( int g = 0; g < objects.Length; g++ ){
			MeshFilter[] filters = objects[g].GetComponentsInChildren<MeshFilter>();
			for( int f=0; f<filters.Length; f++ ){
				if( filters[f] != null ){
					meshList.Add( filters[f].sharedMesh );
					matrixList.Add( filters[f].transform.localToWorldMatrix );
				}
			}
			SkinnedMeshRenderer[] skins = objects[g].GetComponentsInChildren<SkinnedMeshRenderer>();
			for( int s=0; s<skins.Length; s++ ){
				if( skins[s] != null ){
					meshList.Add( skins[s].sharedMesh );
					matrixList.Add( skins[s].transform.localToWorldMatrix );
				}
			}
		}
		meshes = meshList.ToArray();
		matrices = matrixList.ToArray();
	}


	public static void GetMeshesAndMatrixes( MeshFilter[] filters, out Mesh[] meshes, out Matrix4x4[] matrices )
	{
		List<Mesh> meshList = new List<Mesh>();
		List<Matrix4x4> matrixList = new List<Matrix4x4>();
		for( int f=0; f<filters.Length; f++ ){
			if( filters[f] != null ){
				meshList.Add( filters[f].sharedMesh );
				matrixList.Add( filters[f].transform.localToWorldMatrix );
			}
		}
		meshes = meshList.ToArray();
		matrices = matrixList.ToArray();
	}


	public static void GetMeshesAndMatrixes( SkinnedMeshRenderer[] skins, out Mesh[] meshes, out Matrix4x4[] matrices )
	{
		List<Mesh> meshList = new List<Mesh>();
		List<Matrix4x4> matrixList = new List<Matrix4x4>();
		for( int s=0; s<skins.Length; s++ ){
			if( skins[s] != null ){
				meshList.Add( skins[s].sharedMesh );
				matrixList.Add( skins[s].transform.localToWorldMatrix );
			}
		}
		meshes = meshList.ToArray();
		matrices = matrixList.ToArray();
	}
	
	
	static string DateTimeCode(){
		return System.DateTime.Now.ToString("yy") + System.DateTime.Now.ToString("MM") + System.DateTime.Now.ToString("dd") + "_" + System.DateTime.Now.ToString("hh") + System.DateTime.Now.ToString("mm") + System.DateTime.Now.ToString("ss");
	}
}