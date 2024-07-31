// holds the next connection number/id which is used for creating new connections
public class PlayerNameManager
{
	private static PlayerNameManager _instance;
	public static PlayerNameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PlayerNameManager();
			}
			return _instance;
		}
	}

	public int nextPlayerNameNum = 1;

	// private constructor to prevent instance creation outside of the class
	private PlayerNameManager()
	{
	}
}
