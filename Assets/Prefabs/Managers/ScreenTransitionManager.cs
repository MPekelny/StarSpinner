using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenTransitionManager : MonoBehaviour
{
	[SerializeField] private Image _screenOverlay = null;
	[SerializeField] private float _transitionFadeTime = 0.2f;

	public void Awake()
	{
		// Since the transition manager has an extra canvas on it and we really only need it during the transition,
		// set the whole thing to not active while not in use so there is not any extra canvas draw stuff going on.
		gameObject.SetActive(false);
	}

	public void TransitionScreen(string sceneName)
	{
		gameObject.SetActive(true);
		_screenOverlay.color = new Color(0f, 0f, 0f, 0f);
		_screenOverlay.DOFade(1f, _transitionFadeTime).OnComplete(() =>
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.LoadScene(sceneName);
		});
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		_screenOverlay.DOFade(0f, _transitionFadeTime).OnComplete(() => 
		{
			gameObject.SetActive(false);
		});
	}
}
