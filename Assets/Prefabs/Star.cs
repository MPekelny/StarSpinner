using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Star : PoolableObject
{
	private enum StarState
	{
		Active,
		Locked,
		End
	}

	[SerializeField] private Color _activeColorLow = Color.white;
	[SerializeField] private Color _activeColorHigh = Color.white;
	[SerializeField] private float _activeColorTransitionTime = 0.5f;
	[SerializeField] private float _activeScaleLow = 0.75f;
	[SerializeField] private float _activeScaleHigh = 1.25f;
	[SerializeField] private float _activeScaleTransitionTime = 0.3f;

	[SerializeField] private Color _lockedColor = Color.white;
	[SerializeField] private float _endTransitionDuration = 0.5f;
	[SerializeField] private Image _starImage = null;

	public Image StarImage => _starImage;

	private Color _endColor = Color.white;
	private StarState _currentState = StarState.Active;
	private bool _activeScaleMovingToHigh = true;
	private float _activeScalePulseTime = 0f;
	private bool _activeColorMovingToHigh = true;
	private float _activeColorPulseTime = 0f;

	public void Init(Color endColor, Vector3 position)
	{
		_endColor = endColor;
		transform.localPosition = position;

		SwitchToActiveState();
	}

	public void SwitchToActiveState()
	{
		_currentState = StarState.Active;
		_activeColorMovingToHigh = true;
		_activeColorPulseTime = Random.Range(0f, _activeColorTransitionTime);
		_activeScaleMovingToHigh = true;
		_activeScalePulseTime = Random.Range(0f, _activeScaleTransitionTime);

		SetColorScaleFromTime();
	}

	public void SwitchToLockedState()
	{
		_currentState = StarState.Locked;
		_starImage.color = _lockedColor;
		transform.localScale = Vector3.one;
	}

	public void SwitchToEndState()
	{
		_currentState = StarState.End;
		_starImage.DOColor(_endColor, _endTransitionDuration);
		transform.localScale = Vector3.one;
	}

	private void Update()
	{
		if (_currentState == StarState.Active)
		{
			if (_activeColorMovingToHigh)
			{
				_activeColorPulseTime += Time.deltaTime;
				if (_activeColorPulseTime >= _activeColorTransitionTime)
				{
					_activeColorPulseTime = _activeColorTransitionTime;
					_activeColorMovingToHigh = false;
				}
			}
			else
			{
				_activeColorPulseTime -= Time.deltaTime;
				if (_activeColorPulseTime <= 0f)
				{
					_activeColorPulseTime = 0f;
					_activeColorMovingToHigh = true;
				}
			}

			if (_activeScaleMovingToHigh)
			{
				_activeScalePulseTime += Time.deltaTime;
				if (_activeScalePulseTime >= _activeScaleTransitionTime)
				{
					_activeScalePulseTime = _activeScaleTransitionTime;
					_activeScaleMovingToHigh = false;
				}
			}
			else
			{
				_activeScalePulseTime -= Time.deltaTime;
				if (_activeScalePulseTime <= 0f)
				{
					_activeScalePulseTime = 0f;
					_activeScaleMovingToHigh = true;
				}
			}

			SetColorScaleFromTime();
		}
	}

	private void SetColorScaleFromTime()
	{
		float scaleToUse = Mathf.Lerp(_activeScaleLow, _activeScaleHigh, _activeScalePulseTime / _activeScaleTransitionTime);
		transform.localScale = new Vector3(scaleToUse, scaleToUse, 1f);

		Color colorToUse = Color.Lerp(_activeColorLow, _activeColorHigh, _activeColorPulseTime / _activeColorTransitionTime);
		_starImage.color = colorToUse;
	}
}
