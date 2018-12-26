/*
	STLMenuItem.cs
	Created by Carl Emil Carlsen.
	Copyright 2015 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class STLMenuItem : ScriptableObject
{   
	
	[ MenuItem( "File/Export/Selected Mesh(es)/STL (binary)" ) ]
	public static void ExportBinarySTL(){ ExportSTL( true ); }
	
	
	[ MenuItem( "File/Export/Selected Mesh(es)/STL (text)" ) ]
	public static void ExportTextSTL(){ ExportSTL( false ); }


	static void ExportSTL( bool isBinary )
	{
		// get selected meshes //
		Mesh[] meshes;
		Matrix4x4[] matrices;
		STL.GetMeshesAndMatrixes( Selection.gameObjects, out meshes, out matrices );
		
		// display dialog if nothing no meshes are selected //
		if( meshes.Length == 0 ){
			EditorUtility.DisplayDialog( "Nothing to export", "Select one or more GameObjects with MeshFilter or SkinnedMeshRenderer components attached.", "Close" );
			return;
		}
		
		// display dialog to get save path //
		string filePath = EditorUtility.SaveFilePanel( "Save STL file", DefaultDirectory(), DeafultFileName(), "stl" );
		
		// export //
		if( isBinary ) STL.ExportBinary( meshes, matrices, filePath );
		else STL.ExportText( meshes, matrices, filePath );
		
		// display feedback //
		string meshesPlural = meshes.Length == 1 ? "mesh" : "meshes";
		EditorUtility.DisplayDialog( "STL export complete", "Exported " + meshes.Length + " Unity " + meshesPlural + " combined in a STL file.", "Close" );
	}


	static string DeafultFileName()
	{
		string defaultName = DateTimeCode();
		if( SceneManager.GetActiveScene().name != "" ) defaultName = SceneManager.GetActiveScene().name + " " + defaultName;
		return defaultName;
	}
	
	
	static string DefaultDirectory()
	{
		string defaultDirectory = "";
		if( Application.platform == RuntimePlatform.OSXEditor ){
			defaultDirectory = System.Environment.GetEnvironmentVariable( "HOME" ) + "/Desktop";
		} else {
			defaultDirectory = System.Environment.GetFolderPath( System.Environment.SpecialFolder.Desktop );
		}
		return defaultDirectory;
	}
	
	
	static string DateTimeCode()
	{
		System.DateTime now = System.DateTime.Now;
		return now.ToString("yy") + now.ToString("MM") + now.ToString("dd") + "_" + now.ToString("hh") + now.ToString("mm") + now.ToString("ss");
	}
}