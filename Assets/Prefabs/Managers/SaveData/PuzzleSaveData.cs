using SimpleJSON;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PuzzleSaveData
{
	// Save data for puzzles is divided into two parts, static and dynamic.
	// Static data is the data that only needs to be set once per level, the list of spinners each star got attached to as well as how many spinners and stars were in the puzzle at the time the level was entered so that
	//    if the puzzle was changed since it was saved, it can regenerate the level rather than trying to use data that will not work correctly. This way that data does not need to be repeatedly be resaved when other data changes.
	// Dynamic data is the data that needs to be changed relatively frequently, namely the rotation values of the spinners and which spinner was hint locked.
	public PuzzleStaticSaveData StaticData { get; private set; } = new PuzzleStaticSaveData();
	public PuzzleDynamicSaveData DynamicData { get; private set; } = new PuzzleDynamicSaveData();

	public void ResetAllData()
	{
		StaticData.ResetData();
		DynamicData.ResetData();
	}
}
