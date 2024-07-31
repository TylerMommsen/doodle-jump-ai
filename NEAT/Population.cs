using System.Collections.Generic;
using UnityEngine;

public class Population : MonoBehaviour
{
	public List<Player> population = new List<Player>();
	public GameObject playerPrefab;
	public GameObject tempPopObject;
	public int popSize;

	public Genome bestPlayerBrain = null;
	public int totalAlive = 0;

	private List<Species> species = new List<Species>();
	private List<ConnectionHistory> innovationHistory = new List<ConnectionHistory>();

	// create a population of players
	public void InitializePopulation(int popSize)
	{
		this.popSize = popSize;
		for (int i = 0; i < popSize; i++)
		{
			GameObject playerObject = Instantiate(playerPrefab, transform);
			playerObject.name = PlayerNameManager.Instance.nextPlayerNameNum.ToString();
			PlayerNameManager.Instance.nextPlayerNameNum++;

			Player newPlayer = playerObject.GetComponent<Player>();
			newPlayer.brain.Mutate(innovationHistory);
			// newPlayer.brain.CreateInitialConnections(innovationHistory);
			newPlayer.brain.GenerateNetwork();
			population.Add(newPlayer);
		}
	}

	// update logic for players
	public void UpdatePlayers()
	{
		foreach (Player player in population)
		{
			if (player.isAlive)
			{
				player.Look(); // get inputs for brain
				player.Think(); // use outputs from neural network
				player.UpdatePlayer(); // update player rotation and time variables
			}
		}
	}

	// check if all players are dead
	public bool AllDead()
	{
		foreach (Player player in population)
		{
			if (player.isAlive) return false;
		}
		return true;
	}

	// pretty understandable
	void SetBestPlayer()
	{
		float highestFitness = 0;
		foreach (Player player in population)
		{
			if (player.fitness > highestFitness)
			{
				bestPlayerBrain = player.brain.Clone();
				highestFitness = player.fitness;
			}
		}
	}

	// get the total average fitness of all species together
	float GetAverageFitnessSum()
	{
		float averageSum = 0;
		foreach (Species s in species)
		{
			averageSum += s.averageFitness;
		}
		return averageSum;
	}

	// reset players after each generation
	public void Reset() {
		foreach (Player player in population) {
			player.Reset();
		}
	}

	// called when all players die
	public void NaturalSelection()
	{
		Speciate(); // sort players into different species
		CalculateFitness(); // calculate all players fitness
		SortSpecies(); // sort all players within a species and the species array itself by fitness
		KillWeakest(); // Wipe out the bottom 50% of each species
		SetBestPlayer();
		KillStaleSpecies(); // remove species that have not improved for 12 gens
		KillExtinctSpecies(); // kill species that can't reproduce
		NextGeneration(); // reproduce players for next generation
	}

	// separate players into different species based on the weights of their connections
	void Speciate()
	{
		foreach (Species s in species)
		{
			s.players.Clear();
		}

		foreach (Player currPlayer in population)
		{
			bool addToSpecies = false;
			foreach (Species currSpecies in species)
			{
				if (currSpecies.IsSameSpecies(currPlayer.brain))
				{
					currSpecies.AddToSpecies(currPlayer);
					addToSpecies = true;
					break;
				}
			}

			if (!addToSpecies)
			{
				species.Add(new Species(currPlayer));
			}
		}
	}

	// simple stuff
	void CalculateFitness()
	{
		foreach (Player player in population)
		{
			player.CalculateFitness();
		}
	}

	// sorts the players within a species and the species array itself by fitness
	void SortSpecies()
	{
		foreach (Species s in species)
		{
			s.SortSpecies();
		}

		// sort the species array by their best players
		List<Species> sortedSpecies = new List<Species>();
		while (species.Count > 0)
		{
			int maxIndex = 0;
			for (int i = 1; i < species.Count; i++)
			{
				if (species[i].bestFitness > species[maxIndex].bestFitness)
				{
					maxIndex = i;
				}
			}
			sortedSpecies.Add(species[maxIndex]);
			species.RemoveAt(maxIndex);
		}
		species = sortedSpecies;
	}

	// wipe out the weakest 50% of the population of every species
	void KillWeakest()
	{
		foreach (Species s in species)
		{
			s.KillWeakest();
			s.FitnessSharing();
			s.SetAverageFitness();
		}
	}

	// remove species that have not improved for 12 generations
	void KillStaleSpecies()
	{
		for (int i = 2; i < species.Count; i++)
		{
			if (species[i].staleness >= 15)
			{
				species.RemoveAt(i);
				i--;
			}
		}
	}

	// if the species won't even be allocated 1 child for the next gen, then kill it
	void KillExtinctSpecies()
	{
		float averageSum = GetAverageFitnessSum();
		if (averageSum == 0) return; // Prevent division by zero

		for (int i = 1; i < species.Count; i++)
		{
			if (species[i].averageFitness / averageSum * popSize < 1)
			{
				species.RemoveAt(i);
				i--;
			}
		}
	}


	// create the next gen of players! The best of the best
	void NextGeneration()
	{
		PlayerNameManager.Instance.nextPlayerNameNum = 1;
		float averageSum = GetAverageFitnessSum();
		if (averageSum == 0) {
			Debug.LogError("Average fitness sum is zero, which could cause division by zero.");
			return;
		}

		List<Player> children = new List<Player>();

		if (species.Count == 0) {
			Debug.LogError("No species available to reproduce.");
			return;
		}

		foreach (Species s in species)
		{
			// clone the best player in each species
			GameObject bestPlayerInSpecies = Instantiate(playerPrefab, tempPopObject.transform);
			bestPlayerInSpecies.name = PlayerNameManager.Instance.nextPlayerNameNum.ToString();
			Player bestPlayerInSpeciesScript = bestPlayerInSpecies.GetComponent<Player>();
			bestPlayerInSpeciesScript.brain = s.bestBrain.Clone();
			bestPlayerInSpeciesScript.brain.GenerateNetwork();
			children.Add(bestPlayerInSpeciesScript);
			PlayerNameManager.Instance.nextPlayerNameNum++;

			// children.Add(s.champion.Clone(PlayerNameManager.Instance.nextPlayerNameNum, tempPopObject));
			int childrenPerSpecies = Mathf.FloorToInt(s.averageFitness / averageSum * popSize) - 1;
			// int childrenPerSpecies = Mathf.FloorToInt((s.averageFitness / averageSum) * popSize);

			for (int j = 0; j < childrenPerSpecies; j++)
			{
				if (children.Count == popSize) break;

				children.Add(s.Reproduce(innovationHistory, PlayerNameManager.Instance.nextPlayerNameNum, tempPopObject));
				PlayerNameManager.Instance.nextPlayerNameNum++;
			}
		}

		// fill remaining slots
		while (children.Count < popSize) {
			children.Add(species[0].Reproduce(innovationHistory, PlayerNameManager.Instance.nextPlayerNameNum, tempPopObject));
			PlayerNameManager.Instance.nextPlayerNameNum++;
		}

		// clear original population
		foreach (Transform child in transform) {
			Destroy(child.gameObject);
		}
		population.Clear();

		// add new population
		foreach (Player child in children) {
			child.transform.SetParent(transform);
			child.Reset();
		}
		population.AddRange(children);

		foreach (Player p in population)
		{
			p.brain.GenerateNetwork();
		}

		PlayerNameManager.Instance.nextPlayerNameNum = 1;
	}
}
