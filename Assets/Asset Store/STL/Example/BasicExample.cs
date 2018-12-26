/*
	Part of STL, Asset Store product.
*/

using UnityEngine;
using System.Collections;

namespace STLExamples
{
	public class BasicExample : MonoBehaviour
	{
		const int objectCount = 100;

		GameObject[] objects;


		void Start()
		{
			GenerateNewObjects();

		}


		public void GenerateNewObjects()
		{
			if( objects != null ){
				for( int i=0; i<objects.Length; i++ ) Destroy( objects[i] );
			}
			objects = new GameObject[objectCount];
			for( int i=0; i<objects.Length; i++ ){
				objects[i] = GameObject.CreatePrimitive( PrimitiveType.Sphere );
				objects[i].transform.parent = transform;
				objects[i].transform.localScale = Vector3.one * Random.Range( 0.1f, 1f );
				objects[i].transform.position = Random.insideUnitSphere * 2;
			}
		}
		
		
		public void ExportToBinarySTL()
		{
			string filePath = DefaultDirectory() + "/stl_example_binary.stl";
			STL.ExportBinary( objects, filePath );
			Debug.Log( "Exported " + objectCount + " objects to binary STL file." + System.Environment.NewLine + filePath );
		}


		public void ExportToTextSTL()
		{
			string filePath = DefaultDirectory() + "/stl_example_text.stl";
			STL.ExportText( objects, filePath );
			Debug.Log( "Exported " + objectCount + " objects to text based STL file." + System.Environment.NewLine + filePath );
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
	}
}