using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance = null;
	public static GameManager Instance => _instance;

	[SerializeField] private ObjectPoolManager _objectPoolManagerPrefab = null;

	private ObjectPoolManager _objectPoolManager = null;
	public ObjectPoolManager ObjectPoolManager => _objectPoolManager;

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
	}
}
