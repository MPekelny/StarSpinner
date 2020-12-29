using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
	[NonSerialized] public string SCREEN_NAME = "TitleScreen";

	[SerializeField] private GameObject _titleStarContainer = null;
	[SerializeField] private PuzzleData _titlePuzzle = null;
	[SerializeField] private TMPro.TextMeshProUGUI _tapToContinueText = null;

	private List<GameObject> _autoSpinners = new List<GameObject>();
	private List<Star> _stars = new List<Star>();
	private Sequence _starSpinnSequenceReference = null;

	public void Start()
	{
		GameManager.Instance.AudioManager.PlayBGM("menu_bgm", 0.5f);

		_tapToContinueText.text = GameManager.Instance.StringManager.GetStringForKey("title_tap_to_continue");

		for (int i = 0; i < _titlePuzzle.NumSpinners; i++)
		{
			GameObject spinner = new GameObject($"Spinner{i + 1}");
			spinner.transform.SetParent(_titleStarContainer.transform);
			spinner.transform.localPosition = Vector3.zero;
			_autoSpinners.Add(spinner);
		}

		for (int i = 0; i < _titlePuzzle.StarDatas.Length; i++)
		{
			int indexToUse = i % _autoSpinners.Count;
			Transform transformToUse = _autoSpinners[indexToUse].transform;

			Star star = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("Star", transformToUse).GetComponent<Star>();
			star.Init(_titlePuzzle.StarDatas[i].FinalColor, _titlePuzzle.StarDatas[i].Position);
			_stars.Add(star);
		}

		StartCoroutine(DelayNextSpin());
	}

	private void SpinStars()
	{
		float rotationAmount = 360f;
		_starSpinnSequenceReference = DOTween.Sequence();
		foreach (GameObject autoSpin in _autoSpinners)
		{
			Tween tween = autoSpin.transform.DORotate(new Vector3(0f, 0f, rotationAmount), 3f);
			tween.SetEase(Ease.InOutCubic);
			tween.SetRelative();
			_starSpinnSequenceReference.Insert(0f, tween);

			rotationAmount *= -1f;
		}

		_starSpinnSequenceReference.OnComplete(() => { StartCoroutine(DelayNextSpin()); });
	}

	private IEnumerator DelayNextSpin()
	{
		yield return new WaitForSeconds(1.5f);

		SpinStars();
	}

	public void OnTappedScreen()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect("button_pressed");
		_starSpinnSequenceReference.Kill();
		foreach (Star star in _stars)
		{
			star.ReturnToPool();
		}

		_stars.Clear();

		GameManager.Instance.ScreenTransitionManager.TransitionScreen(LevelSelectScreen.SCREEN_NAME);
	}
}
