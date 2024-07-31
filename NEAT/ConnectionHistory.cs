using System;
using System.Collections.Generic;

public class ConnectionHistory
{
	public int fromNode;
	public int toNode;
	public int innovationNumber;
	public List<int> innovationNumbers;

	public ConnectionHistory(int fromNode, int toNode, int innovationNumber, List<int> innovationNumbers)
	{
		this.fromNode = fromNode;
		this.toNode = toNode;
		this.innovationNumber = innovationNumber;
		this.innovationNumbers = new List<int>(innovationNumbers);
	}

	// returns whether the genome matches the original genome and the connection is between the same nodes
	public bool Matches(Genome genome, Node fromNode, Node toNode)
	{
		if (genome.connections.Count == innovationNumbers.Count)
		{
			if (fromNode.id == this.fromNode && toNode.id == this.toNode)
			{
				// check if all the innovation numbers match from the genome
				foreach (var connection in genome.connections)
				{
					if (!innovationNumbers.Contains(connection.innovationNumber))
					{
						return false;
					}
				}

				// if reached this far then the innovation numbers match the connections' innovation numbers and the connection is between the same nodes, so it does match
				return true;
			}
		}
		return false;
	}
}
