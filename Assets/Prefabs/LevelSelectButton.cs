using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : PoolableObject
{
	public enum LevelProgressState
	{
		Normal,
		InProgress,
		Solved
	}

	[SerializeField] private TMPro.TextMeshProUGUI _numberText = null;
	[SerializeField] private Image _buttonBacking = null;
	[SerializeField] private Color _unsolvedColor = Color.white;
	[SerializeField] private Color _solvedColor = Color.green;
	[SerializeField] private Color _inProgressColor = Color.blue;

	private LevelSelectScreen _parentReference = null;
	private int _levelIndex = 0;

	public void Init(LevelSelectScreen parentRef, int levelIndex, LevelProgressState stateOfLevel)
	{
		_parentReference = parentRef;
		_levelIndex = levelIndex;
		SetColorForState(stateOfLevel);
		_numberText.text = (_levelIndex + 1).ToString();
	}

	public void OnLevelButtonPressed()
	{
		_parentReference.LevelButtonPressed(_levelIndex);
	}

	private void SetColorForState(LevelProgressState state)
	{
		switch (state)
		{
			case LevelProgressState.Solved:
				_buttonBacking.color = _solvedColor;
				break;
			case LevelProgressState.InProgress:
				_buttonBacking.color = _inProgressColor;
				break;
			case LevelProgressState.Normal:
			default:
				_buttonBacking.color = _unsolvedColor;
				break;
		}
	}
}
