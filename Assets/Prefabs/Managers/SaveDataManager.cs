using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
	// For now at least I think save data can be handled fairly simply, just when each level is completed, save that level's id plus a suffix to the playerprefs.
	// Then, if that key exists in playerprefs at all, the game can know that that level has been completed. What I do for save data in the future may change, like when I add save data for incomplete levels,
	// but for now, should be good enough.
	private const string LEVEL_COMPLETED_SUFFIX = "_lvl_completed";

	public bool IsLevelCompleted(string levelId)
	{
		return PlayerPrefs.HasKey($"{levelId}{LEVEL_COMPLETED_SUFFIX}"); ;
	}

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

	public void ClearAllSaveData()
	{
		PlayerPrefs.DeleteAll();
	}
}
