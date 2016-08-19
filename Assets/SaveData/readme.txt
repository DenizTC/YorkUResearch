Product : Save Data Package
Studio : Arkham Interactive
Date : September 17th, 2013
Version : 1.0
Email : support@arkhaminteractive.com

How to use:
	1) Call 'SaveData.Load(string filePath)' or 'SaveData.LoadFromStreamingAssets(string fileName)' from code to load data from an existing file.
	2) Get or set saved data using SaveData[string index] indexer, or SaveData.GetValue() overloads.
	3) Call 'SaveData.Save()' to save data either to a specified path or to the streaming assets path

 - All saved objects MUST have [System.Serializable] attribute
 - All saved fields MUST be public
 - Components CANNOT be saved