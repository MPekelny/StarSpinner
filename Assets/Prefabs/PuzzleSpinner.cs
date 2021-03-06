﻿using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleSpinner : PoolableObject
{
	[SerializeField] private float _transitionDuration = 0.5f;
	[SerializeField] private Selectable _touchablePart = null;
	[SerializeField] private Image _spinnerObject = null;
	public Image SpinnerObject => _spinnerObject;
	public float TransitionDuration => _transitionDuration;

	private Action<PuzzleSpinner> _checkOverlapCallback = null;
	private Action _checkSolvedCallback = null;
	private EventTrigger _trigger = null;
	private Star[] _referenceStars = null;
	private bool _locked = false;

	public void Init(Action<PuzzleSpinner> checkOverlapCallback, Action checkSolvedCallback, Color spinnerColor, Sprite spinnerSprite)
	{
		_checkOverlapCallback = checkOverlapCallback;
		_checkSolvedCallback = checkSolvedCallback;
		_trigger = _spinnerObject.GetComponent<EventTrigger>();

		_touchablePart.interactable = true;

		_spinnerObject.color = spinnerColor;
		_spinnerObject.sprite = spinnerSprite;
		_spinnerObject.transform.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, 359f));

		SetupTouchEvents();
	}

	public void SpinRandomly(float rangeMin, float rangeMax)
	{
		transform.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(rangeMin, rangeMax));
	}

	public void SetRotations(float mainRotation, float objectRotation)
	{
		transform.eulerAngles = new Vector3(0f, 0f, mainRotation);
		_spinnerObject.transform.localEulerAngles = new Vector3(0f, 0f, objectRotation);
	}

	public float GetSpinnerObjectRotation() => _spinnerObject.transform.localEulerAngles.z;

	public void FindStarChildren()
	{
		_referenceStars = GetComponentsInChildren<Star>();
	}

	public void SetToHintState()
	{
		transform.localEulerAngles = transform.localEulerAngles.With(nZ: 0f);
		_spinnerObject.color = _spinnerObject.color.With(nA: 0.4f);
		_touchablePart.interactable = false;
		_locked = true;

		foreach (Star star in _referenceStars)
		{
			star.SwitchToLockedState();
		}
	}

	public void TransitionToEndState(Action transitionComplete)
	{
		_touchablePart.interactable = false;

		Sequence transitionSequence = DOTween.Sequence();
		transitionSequence.Insert(0f, _spinnerObject.DOFade(0f, _transitionDuration));
		transitionSequence.Insert(0f, transform.DORotate(Vector3.zero, _transitionDuration));
		transitionSequence.OnComplete(() => { transitionComplete?.Invoke(); });
	}

	public override void ReturnToPoolCleanup()
	{
		_touchablePart.interactable = true;
		_referenceStars = null;
		_locked = false;
	}

	private void SetupTouchEvents()
	{
		EventTrigger.Entry evt = new EventTrigger.Entry();
		evt.eventID = EventTriggerType.BeginDrag;
		evt.callback.AddListener((data) => { OnDragStart((PointerEventData)data); });
		_trigger.triggers.Add(evt);

		EventTrigger.Entry evt2 = new EventTrigger.Entry();
		evt2.eventID = EventTriggerType.EndDrag;
		evt2.callback.AddListener((data) => { OnDragEnd((PointerEventData)data); });
		_trigger.triggers.Add(evt2);

		EventTrigger.Entry evt3 = new EventTrigger.Entry();
		evt3.eventID = EventTriggerType.Drag;
		evt3.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
		_trigger.triggers.Add(evt3);
	}

	private void OnDragStart(PointerEventData data)
	{

	}

	private void OnDrag(PointerEventData data)
	{
		if (!_locked)
		{
			Vector2 fromLine = data.position - (Vector2)transform.position;
			Vector2 toLine = new Vector2(0, 1);

			float angle = Vector2.Angle(fromLine, toLine);

			Vector3 cross = Vector3.Cross(fromLine, toLine);
			if (cross.z > 0f)
			{
				angle = 360f - angle;
			}

			transform.eulerAngles = new Vector3(0f, 0f, angle - _spinnerObject.transform.localEulerAngles.z);
		}
	}

	private void OnDragEnd(PointerEventData data)
	{
		if (!_locked)
		{
			_checkOverlapCallback?.Invoke(this);
			_checkSolvedCallback?.Invoke();
		}
	}
}
