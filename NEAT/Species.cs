using System.Collections.Generic;
using UnityEngine;

public class Species
{
	public List<Player> players; // players in this species
	public float bestFitness = 0;
	public float averageFitness = 0; // average fitness of this species
	public int staleness = 0; // how many generations with no improvement
	public Genome bestBrain;

	// coefficients for testing compatibility on whether to add to species or not
	public float excessCoeff = 1;
	public float weightDiffCoeff = 0.5f;
	public float compatibilityThreshold = 3;

	public Species(Player player)
	{
		players = new List<Player>();
		if (player != null)
		{
			players.Add(player);
			bestFitness = player.fitness;
			bestBrain = player.brain.Clone();
		}
	}

	// checks how similar a player is to the players of this species by comparing weights
	public bool IsSameSpecies(Genome genome)
	{
		float compatibility;
		int excessAndDisjoint = GetExcessAndDisjoint(genome, bestBrain); // get the number of excess and disjoint genes
		float averageWeightDiff = AverageWeightDiff(genome, bestBrain); // average weight difference

		int largeGenomerNormalizer = genome.connections.Count - 20;
		if (largeGenomerNormalizer < 1) largeGenomerNormalizer = 1;

		// compatibility formula
		compatibility =
			(excessCoeff * excessAndDisjoint) / largeGenomerNormalizer +
			weightDiffCoeff * averageWeightDiff;

		return compatibilityThreshold > compatibility;
	}

	// returns the number of excess and disjoint connections
	public int GetExcessAndDisjoint(Genome brain1, Genome brain2)
	{
		int matching = 0;
		foreach (var conn1 in brain1.connections)
		{
			foreach (var conn2 in brain2.connections)
			{
				if (conn1.innovationNumber == conn2.innovationNumber)
				{
					matching++;
					break;
				}
			}
		}
		return brain1.connections.Count + brain2.connections.Count - 2 * matching;
	}

	// returns the average weight difference between matching connections
	public float AverageWeightDiff(Genome brain1, Genome brain2)
	{
		if (brain1.connections.Count == 0 || brain2.connections.Count == 0) return 0;

		int matching = 0;
		float totalDifference = 0;
		foreach (var conn1 in brain1.connections)
		{
			foreach (var conn2 in brain2.connections)
			{
				if (conn1.innovationNumber == conn2.innovationNumber)
				{
					matching++;
					totalDifference += Mathf.Abs(conn1.weight - conn2.weight);
					break;
				}
			}
		}
		return matching == 0 ? 100 : totalDifference / matching;
	}

	public void AddToSpecies(Player player)
	{
		players.Add(player);
	}

	// sort players from highest to lowest fitness
	public void SortSpecies()
	{
		players.Sort((p1, p2) => p2.fitness.CompareTo(p1.fitness));

		if (players.Count == 0)
		{
			staleness = 200;
			return;
		}

		if (players[0].fitness > bestFitness)
		{
			staleness = 0;
			bestFitness = players[0].fitness;
			bestBrain = players[0].brain.Clone();
		}
		else
		{
			staleness++;
		}
	}

	// kill the weakest 50% of the species
	public void KillWeakest()
	{
		if (players.Count > 2)
		{
			players.RemoveRange(players.Count / 2, players.Count - players.Count / 2);
		}
	}

	// fitness sharing
	public void FitnessSharing()
	{
		foreach (Player player in players)
		{
			player.fitness /= players.Count;
		}
	}

	// calculate the average fitness
	public void SetAverageFitness()
	{
		float totalFitness = 0;
		foreach (Player player in players)
		{
			totalFitness += player.fitness;
		}
		averageFitness = totalFitness / players.Count;
	}

	// select a player based on fitness
	public Player SelectPlayer()
	{
		float fitnessSum = 0;
		foreach (Player player in players)
		{
			fitnessSum += player.fitness;
		}

		float rand = Random.value * fitnessSum;
		float runningSum = 0;

		foreach (Player player in players)
		{
			runningSum += player.fitness;
			if (runningSum > rand)
			{
				return player;
			}
		}

		return players[0]; // fallback
	}

	// create a new child
	public Player Reproduce(List<ConnectionHistory> innovationHistory, int name, GameObject parentContainer)
	{
		Player child;

		if (Random.value < 0.25) // 25% chance of no crossover
		{
			child = SelectPlayer().Clone(name, parentContainer);
		}
		else
		{
			Player parent1 = SelectPlayer();
			Player parent2 = SelectPlayer();

			child = parent1.fitness < parent2.fitness ? parent2.Crossover(parent1, name, parentContainer) : parent1.Crossover(parent2, name, parentContainer);
		}

		child.brain.Mutate(innovationHistory);
		return child;
	}
}
