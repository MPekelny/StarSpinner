using SimpleJSON;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PuzzleDynamicSaveData
{
	private const string HINT_SPINNER_KEY = "hint_spinner";
	private const string SPINNER_ROTATIONS_KEY = "spinner_rotations";
	private const string SPINNER_TOUCH_ROTATIONS_KEY = "spinner_touch_rotations";

	public List<float> SpinnerRotations { get; set; } = new List<float>();
	public List<float> SpinnerTouchObjectRotations { get; set; } = new List<float>();
	public int HintLockedSpinner { get; set; } = -1;

	/// <summary>
	/// Takes the save data and turns it into a json formatted string.
	/// </summary>
	public string WriteToJsonString()
	{
		JSONNode node = new JSONObject();
		node[HINT_SPINNER_KEY] = HintLockedSpinner;
		foreach (float rotation in SpinnerRotations)
		{
			node[SPINNER_ROTATIONS_KEY][-1] = rotation;
		}

		foreach (float rotation in SpinnerTouchObjectRotations)
		{
			node[SPINNER_TOUCH_ROTATIONS_KEY][-1] = rotation;
		}

		StringBuilder builder = new StringBuilder();
		node.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
		return builder.ToString();
	}

	/// <summary>
	/// Creates the save data from a json formatted string.
	/// </summary>
	public void ReadFromJsonString(string json)
	{
		ResetData();

		JSONNode node = JSONNode.Parse(json);
		HintLockedSpinner = node[HINT_SPINNER_KEY].AsInt;
		JSONArray rotationsArray = node[SPINNER_ROTATIONS_KEY].AsArray;
		for (int i = 0; i < rotationsArray.Count; i++)
		{
			SpinnerRotations.Add(rotationsArray[i].AsFloat);
		}

		JSONArray objectRotationsArray = node[SPINNER_TOUCH_ROTATIONS_KEY].AsArray;
		for (int i = 0; i < objectRotationsArray.Count; i++)
		{
			SpinnerTouchObjectRotations.Add(objectRotationsArray[i].AsFloat);
		}
	}

	/// <summary>
	/// Given a number of spinners, make sure the number of rotations in the save data match that number.
	/// </summary>
	/// <param name="correctNumSpinners"></param>
	/// <returns></returns>
	public bool EnsureCorrectNumberOfSpinners(int correctNumSpinners)
	{
		if (SpinnerRotations.Count > correctNumSpinners)
		{
			Debug.Log("There are more spinners in dynamic save data than expected, removing extras to match.");
			SpinnerRotations.RemoveRange(correctNumSpinners, SpinnerRotations.Count - correctNumSpinners);
			SpinnerTouchObjectRotations.RemoveRange(correctNumSpinners, SpinnerTouchObjectRotations.Count - correctNumSpinners);
			return false;
		}
		else if (SpinnerRotations.Count < correctNumSpinners)
		{
			Debug.Log("There are fewer spinners in dynamic save data than expected, adding additional items to match.");
			int numToAdd = correctNumSpinners - SpinnerRotations.Count;
			for (int i = 0; i < numToAdd; i++)
			{
				float rRot = UnityEngine.Random.Range(0f, 360f);
				float rObjRot = UnityEngine.Random.Range(0f, 360f);
				SpinnerRotations.Add(rRot);
				SpinnerTouchObjectRotations.Add(rObjRot);
			}

			return false;
		}

		return true;
	}

	public void ResetData()
	{
		SpinnerRotations.Clear();
		SpinnerTouchObjectRotations.Clear();
		HintLockedSpinner = -1;
	}
}
