using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : PoolableObject
{
	[SerializeField] private TMPro.TextMeshProUGUI _numberText = null;
	[SerializeField] private Image _buttonBacking = null;
	[SerializeField] private Color _unsolvedColor = Color.white;
	[SerializeField] private Color _solvedColor = Color.green;

	private LevelSelectScreen _parentReference = null;
	private int _levelIndex = 0;

	public void Init(LevelSelectScreen parentRef, int levelIndex, bool isSolved)
	{
		_parentReference = parentRef;
		_levelIndex = levelIndex;
		_buttonBacking.color = isSolved ? _solvedColor : _unsolvedColor;
		_numberText.text = (_levelIndex + 1).ToString();
	}

	public void OnLevelButtonPressed()
	{
		_parentReference.LevelButtonPressed(_levelIndex);
	}
}
