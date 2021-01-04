using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : PoolableObject
{
	public enum LevelProgressState
	{
		Normal,
		InProgress,
		Solved,
		SolvedAndInProgress
	}

	[SerializeField] private TMPro.TextMeshProUGUI _upperText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _numberText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
	[SerializeField] private Image _buttonBacking = null;
	[SerializeField] private Image _solutionImage = null;
	[SerializeField] private Color _unsolvedColor = Color.white;
	[SerializeField] private Color _solvedColor = Color.green;
	[SerializeField] private Color _inProgressColor = Color.blue;

	private LevelSelectScreen _parentReference = null;
	private int _levelIndex = 0;
	private PuzzleData _dataUsedForButton = null;

	public void Init(LevelSelectScreen parentRef, PuzzleData puzzleData, int levelIndex)
	{
		_parentReference = parentRef;
		_dataUsedForButton = puzzleData;
		_levelIndex = levelIndex;
		SetupButtonVisuals();
	}

	public void OnLevelButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);
		_parentReference.LevelButtonPressed(_levelIndex);
	}

	private void SetupButtonVisuals()
	{
		LevelProgressState state = GetButtonState();

		SetColorForButton(state);
		SetUpperText(state);
		SetNumberText(state);
		SetLevelNameText(state);
		SetSolvedImage(state);
	}

	private void SetUpperText(LevelProgressState state)
	{
		_upperText.gameObject.SetActive(true);
		if (state == LevelProgressState.InProgress || state == LevelProgressState.SolvedAndInProgress)
		{
			_upperText.text = GameManager.Instance.StringManager.GetStringForKey("select_button_in_progress");
		}
		else if (state != LevelProgressState.Solved)
		{
			_upperText.text = GameManager.Instance.StringManager.GetStringForKey("select_button_level");
		}
		else
		{
			_upperText.gameObject.SetActive(false);
		}
	}

	private void SetNumberText(LevelProgressState state)
	{
		if (state == LevelProgressState.Solved || state == LevelProgressState.SolvedAndInProgress)
		{
			_numberText.gameObject.SetActive(false);
		}
		else
		{
			_numberText.gameObject.SetActive(true);
			_numberText.text = (_levelIndex + 1).ToString();
		}
	}

	private void SetLevelNameText(LevelProgressState state)
	{
		if (state == LevelProgressState.Solved || state == LevelProgressState.SolvedAndInProgress)
		{
			_nameText.gameObject.SetActive(true);
			_nameText.text = _dataUsedForButton.PuzzleName;
		}
		else
		{
			_nameText.gameObject.SetActive(false);
		}
	}

	private void SetSolvedImage(LevelProgressState state)
	{
		if (state == LevelProgressState.Solved || state == LevelProgressState.SolvedAndInProgress)
		{
			_solutionImage.gameObject.SetActive(true);
			_solutionImage.sprite = _dataUsedForButton.PuzzleSolvedSprite;
		}
		else
		{
			_solutionImage.gameObject.SetActive(false);
		}
	}

	private LevelProgressState GetButtonState()
	{
		bool isSolved = GameManager.Instance.SaveDataManager.IsLevelCompleted(_dataUsedForButton.PuzzleUniqueId);
		bool isInProgress = GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_dataUsedForButton.PuzzleUniqueId);
		LevelProgressState state = LevelProgressState.Normal;
		if (isInProgress && isSolved)
		{
			state = LevelProgressState.SolvedAndInProgress;
		}
		else if (isInProgress)
		{
			state = LevelProgressState.InProgress;
		}
		else if (isSolved)
		{
			state = LevelProgressState.Solved;
		}

		return state;
	}

	private void SetColorForButton(LevelProgressState state)
	{
		switch (state)
		{
			case LevelProgressState.Solved:
				_buttonBacking.color = _solvedColor;
				break;
			case LevelProgressState.InProgress:
			case LevelProgressState.SolvedAndInProgress:
				_buttonBacking.color = _inProgressColor;
				break;
			case LevelProgressState.Normal:
			default:
				_buttonBacking.color = _unsolvedColor;
				break;
		}
	}
}
