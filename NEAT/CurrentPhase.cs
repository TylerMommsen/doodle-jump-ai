// holds the current 'training' phase for the ai in order to be used by the genome to optimize which inputs to use
public class CurrentPhase
{
	private static CurrentPhase _instance;
	public static CurrentPhase Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new CurrentPhase();
			}
			return _instance;
		}
	}

	public bool isSpawningMonsters = false;
	public bool isSpawningLotsOfMonsters = false;
	public bool isSpawningItems = false;

	// private constructor to prevent instance creation outside of the class
	private CurrentPhase()
	{
	}
}
