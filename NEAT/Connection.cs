using UnityEngine;
using System.Collections.Generic;

public class Connection 
{
	public Node fromNode;
	public Node toNode;
	public float weight;
	public bool enabled;
	public int innovationNumber;

	public Connection(Node fromNode, Node toNode, float weight, int innovationNumber)
	{
		this.fromNode = fromNode;
		this.toNode = toNode;
		this.weight = weight;
		enabled = true;
		this.innovationNumber = innovationNumber;
	}

	// 10% chance of a large mutation and 90% chance of a small mutation
	public void MutateWeight()
	{
		if (Random.value < 0.1f)
		{
			weight = Random.value * 2f - 1f; // random between -1 and 1
		}
		else
		{
			weight += GaussianRandom() / 10.0f;

			if (weight > 1f) weight = 1f;
			if (weight < -1f) weight = -1f;
		}
	}

	// Create a copy of the connection
	public Connection Clone(Node fromNode, Node toNode)
	{
		Connection clone = new Connection(fromNode, toNode, weight, innovationNumber);
		clone.enabled = enabled;
		return clone;
	}

	// Helper method to generate a Gaussian distributed random number
	private float GaussianRandom()
	{
		// Using Box-Muller transform to generate a Gaussian distributed random number
		float u1 = 1.0f - Random.value; // uniform(0,1] random doubles
		float u2 = 1.0f - Random.value;
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log((float)u1)) *
							Mathf.Sin(2.0f * Mathf.PI * (float)u2); // random normal(0,1)
		return randStdNormal;
	}
}