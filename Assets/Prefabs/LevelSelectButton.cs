using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : PoolableObject
{
	[SerializeField] private TMPro.TextMeshProUGUI _numberText = null;

	private LevelSelectScreen _parentReference = null;
	private int _levelIndex = 0;

	public void Init(LevelSelectScreen parentRef, int levelIndex)
	{
		_parentReference = parentRef;
		_levelIndex = levelIndex;
		_numberText.text = (_levelIndex + 1).ToString();
	}

	public void OnLevelButtonPressed()
	{
		_parentReference.LevelButtonPressed(_levelIndex);
	}
}
