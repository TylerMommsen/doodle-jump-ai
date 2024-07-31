// holds the next connection number/id which is used for creating new connections
public class InnovationManager
{
	private static InnovationManager _instance;
	public static InnovationManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new InnovationManager();
			}
			return _instance;
		}
	}

	public int nextConnectionNumber = 1000;

	// private constructor to prevent instance creation outside of the class
	private InnovationManager()
	{
	}
}
