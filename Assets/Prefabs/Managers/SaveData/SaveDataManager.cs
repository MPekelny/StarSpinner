using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
	private const string LEVEL_COMPLETED_SUFFIX = "_lvl_completed";
	private const string PUZZLE_STATIC_DATA_SUFFIX = "_static_data";
	private const string PUZZLE_DYNAMIC_DATA_SUFFIX = "_dynamic_data";

	public bool IsLevelCompleted(string levelId) => PlayerPrefs.HasKey($"{levelId}{LEVEL_COMPLETED_SUFFIX}");
	public bool PuzzleStaticDataExistsForLevel(string levelId) => PlayerPrefs.HasKey($"{levelId}{PUZZLE_STATIC_DATA_SUFFIX}");
	public bool PuzzleDynamicDataExistsForLevel(string levelId) => PlayerPrefs.HasKey($"{levelId}{PUZZLE_DYNAMIC_DATA_SUFFIX}");

	public void SaveLevelCompleted(string levelId)
	{
		PlayerPrefs.SetInt($"{levelId}{LEVEL_COMPLETED_SUFFIX}", 1);
		PlayerPrefs.Save();
	}

	public void RemoveLevelCompled(string levelId)
	{
		string key = $"{levelId}{LEVEL_COMPLETED_SUFFIX}";
		if (PlayerPrefs.HasKey(key))
		{
			PlayerPrefs.DeleteKey(key);
			PlayerPrefs.Save();
		}
	}

	public string GetPuzzleStaticDataForLevel(string levelId)
	{
		if (!PuzzleStaticDataExistsForLevel(levelId))
		{
			return null;
		}

		return PlayerPrefs.GetString($"{levelId}{PUZZLE_STATIC_DATA_SUFFIX}");
	}

	public void SavePuzzleStaticDataForLevel(string levelId, string staticDataJson)
	{
		PlayerPrefs.SetString($"{levelId}{PUZZLE_STATIC_DATA_SUFFIX}", staticDataJson);
		PlayerPrefs.Save();
	}

	public string GetPuzzleDynamicDataForLevel(string levelId)
	{
		if (!PuzzleDynamicDataExistsForLevel(levelId))
		{
			return null;
		}

		return PlayerPrefs.GetString($"{levelId}{PUZZLE_DYNAMIC_DATA_SUFFIX}");
	}

	public void SavePuzzleDynamicDataForLevel(string levelId, string dynamicDataJson)
	{
		PlayerPrefs.SetString($"{levelId}{PUZZLE_DYNAMIC_DATA_SUFFIX}", dynamicDataJson);
		PlayerPrefs.Save();
	}

	public void RemovePuzzleSaveDataForLevel(string levelId)
	{
		if (PuzzleStaticDataExistsForLevel(levelId))
		{
			PlayerPrefs.DeleteKey($"{levelId}{PUZZLE_STATIC_DATA_SUFFIX}");
		}

		if (PuzzleDynamicDataExistsForLevel(levelId))
		{
			PlayerPrefs.DeleteKey($"{levelId}{PUZZLE_DYNAMIC_DATA_SUFFIX}");
		}

		PlayerPrefs.Save();
	}

	public void ClearAllSaveData()
	{
		PlayerPrefs.DeleteAll();
	}
}
