using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class NetworkVisualizer : MonoBehaviour
{
	public Genome brain; // the neural network to visualize
	public GameObject nodePrefab;
	public GameObject smallNodePrefab;
	public GameObject connectionPrefab;
	private float startX = -9.5f; // starting x-coordinate for the visualization
	private float startY = -4.5f; // starting y-coordinate for the visualization
	private float width = 5.5f; // width of the visualization area
	private float height = 7.5f; // height of the visualization area
	private List<string> inputLabels;
	private List<string> outputLabels = new List<string> { "Move Left", "Move Right", "Stay Still", "Shoot"};

	private Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();
	private List<GameObject> connectionObjects = new List<GameObject>();

	public void BuildNetwork(Genome brain)
	{
		this.brain = brain;
		// Destroy existing visualization if any
		foreach (var node in nodeObjects.Values)
			Destroy(node);
		foreach (var conn in connectionObjects)
			Destroy(conn);

		nodeObjects.Clear();
		connectionObjects.Clear();

		CalculateNodePositions();
		DrawConnections();
	}

	private void CalculateNodePositions()
	{
		for (int i = 0; i < brain.layers; i++)
		{
			List<Node> nodesInLayer = brain.nodes.FindAll(node => node.layer == i);
			float x = startX + ((i + 1.0f) * width) / (brain.layers + 1.0f);

			List<Node> displayNodes;
			if (i == 0) {
				// Skip the first two nodes, and then select every third node starting from the third node
				displayNodes = nodesInLayer.Where((node, index) => index >= 2 && (index - 2) % 3 == 0).ToList();
			} else {
				displayNodes = nodesInLayer;
			}

			for (int j = 0; j < nodesInLayer.Count; j++)
			{
				float y = startY + (height - ((j + 1.0f) * height) / (nodesInLayer.Count + 1.0f));

				GameObject nodeObj;
				if (i == 0) {
					nodeObj = Instantiate(smallNodePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
				} else {
					nodeObj = Instantiate(nodePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
				}
				nodeObjects[nodesInLayer[j].id] = nodeObj;

				// Find each TextMeshPro component by name
				TextMeshPro idTextComponent = nodeObj.transform.Find("NodeID").GetComponent<TextMeshPro>();
				// TextMeshPro inputTextComponent = nodeObj.transform.Find("InputLabel").GetComponent<TextMeshPro>();
				TextMeshPro outputTextComponent = nodeObj.transform.Find("OutputLabel").GetComponent<TextMeshPro>();

				if (idTextComponent != null)
				{
					// idTextComponent.text = nodesInLayer[j].id.ToString(); // Set node ID
				}

				if (outputTextComponent != null)
				{
					if (i == brain.layers - 1 && j < outputLabels.Count) // Output layer
					{
						outputTextComponent.text = outputLabels[j];
					}
				}
			}
		}
	}

	private void DrawConnections()
	{
		foreach (Connection connection in brain.connections)
		{
			if (nodeObjects.ContainsKey(connection.fromNode.id) && nodeObjects.ContainsKey(connection.toNode.id))
			{
				GameObject lineObj = Instantiate(connectionPrefab, this.transform);
				LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

				Vector3 fromPos = nodeObjects[connection.fromNode.id].transform.position;
				Vector3 toPos = nodeObjects[connection.toNode.id].transform.position;

				lineRenderer.SetPositions(new Vector3[] { fromPos, toPos });

				Color color = connection.weight > 0 ? Color.red : Color.blue;
				color.a = connection.enabled ? 1.0f : 0.4f;
				lineRenderer.startColor = color;
				lineRenderer.endColor = color;

				lineRenderer.startWidth = 0.02f;
				lineRenderer.endWidth = 0.02f;

				connectionObjects.Add(lineObj);
			}
		}
	}
}
