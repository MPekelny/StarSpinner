using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance = null;
	public static GameManager Instance => _instance;

	[SerializeField] private ObjectPoolManager _objectPoolManagerPrefab = null;
	[SerializeField] private ScreenTransitionManager _screenTransitionManagerPrefab = null;

	private ObjectPoolManager _objectPoolManager = null;
	public ObjectPoolManager ObjectPoolManager => _objectPoolManager;

	private ScreenTransitionManager _screenTransitionManager = null;
	public ScreenTransitionManager ScreenTransitionManager => _screenTransitionManager;

	public PuzzleData ActivePuzzle { get; set; }

	public void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
			CreateManagers();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void CreateManagers()
	{
		_objectPoolManager = Instantiate(_objectPoolManagerPrefab, transform);
		_screenTransitionManager = Instantiate(_screenTransitionManagerPrefab, transform);
	}
}
