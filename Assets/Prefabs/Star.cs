using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Star : PoolableObject
{
	[SerializeField] Color _startColor = Color.white;
	[SerializeField] Color _lockedColor = Color.white;
	[SerializeField] float _endTransitionDuration = 0.5f;
	[SerializeField] private Image _starImage = null;
	public Image StarImage => _starImage;

	private Color _endColor = Color.white;

	public void Init(Color endColor, Vector3 position)
	{
		_endColor = endColor;
		_starImage.color = _startColor;
		transform.localPosition = position;
	}

	public void SetAsLockedColor()
	{
		_starImage.color = _lockedColor;
	}

	public void SetAsNormalColor()
	{
		_starImage.color = _startColor;
	}

	public void TransitionToEndState()
	{
		_starImage.DOColor(_endColor, _endTransitionDuration);
	}
}
