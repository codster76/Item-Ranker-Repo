using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.UI;

public class ItemSort : MonoBehaviour
{	
	public TextAsset items; // holds the full text file
	List<string> allItemList = new List<string>();
	
	List<string> ranked = new List<string>();
	
	public bool roundStarted = false;
	//public bool pickStarted = false;
	//public bool eliminatedStarted = false;
	
	public Toggle[] buttons;
	public Button confirmButton;
	
	public Text itemsLeft;
	private int itemCount = 0; // number of items left to rank in the current round
	private int itemTotalRound = 0; // total number of items in the current round
	public Text itemsRanked;
	
	public Text rankingsDisplay;
	public GameObject rankingsPanel;
	public Text showRankingsButton;
	//public GameObject pickedButton;
	//public GameObject eliminatedButton;
	
	//public int stackCount;
	
    // Start is called before the first frame update
    void Start()
    {
		string[] itemArray = items.text.Split('\n');
		
		foreach(string word in itemArray)
		{
			allItemList.Add(word);
		}
		
		CreateFile(); // creates the rankings file
		
		StartCoroutine(nextRoundIterative(allItemList));
    }

    // Update is called once per frame
    void Update()
    {		
		foreach(Toggle x in buttons)
		{
			if(x.isOn)
			{
				confirmButton.interactable = true;
				break;
			}
			
			confirmButton.interactable = false;
		}
		
		itemsLeft.text = "Items Left This Round: " + itemCount + "/" + itemTotalRound;
		itemsRanked.text = "Items Ranked: " + ranked.Count;
    }
	
	public void startNextRound()
	{
		roundStarted = true;
	}
	
	public IEnumerator nextRoundIterative(List<string> items)
	{
		// assume item list isn't initially empty
		
		Stack<List<string>> itemStack = new Stack<List<string>>();
		itemStack.Push(items);
		
		while(itemStack.Count > 0)
		{
			Debug.Log("Loop");
			List<string> itemList = new List<string>();
			itemList = itemStack.Pop();
			itemTotalRound = itemList.Count;
			
			// if the list is exactly 1 long, add it to the ranked list and break
			if(itemList.Count == 1)
			{
				ranked.Add(itemList[0]);
				AddText("\n" + ranked.Count + ". " + itemList[0]);
				Debug.Log(itemList[0] + " added");
				continue;
			}
			
			// reset all buttons
			foreach(Toggle x in buttons)
			{
				x.gameObject.SetActive(true);
				x.isOn = false;
			}
			
			List<string> picked = new List<string>();
			List<string> eliminated = new List<string>();
			
			// go through and let the user pick items, creating their picked and eliminated lists
			while(itemList.Count >= buttons.Length)
			{
				roundStarted = false;
				itemCount = itemList.Count;
				
				// pick out and remove items from the toCheck list to display on buttons.
				foreach(Toggle x in buttons)
				{
					x.isOn = false; // reset button presses
					int index = Random.Range(0,itemList.Count);
					x.transform.Find("Label").GetComponent<Text>().text = itemList[index];
					itemList.RemoveAt(index);
				}
				
				yield return new WaitUntil(() => roundStarted == true);
				
				// After pressing confirm, the items get placed into their lists.
				foreach(Toggle x in buttons)
				{
					if(x.isOn && x.gameObject.activeSelf)
					{
						picked.Add(x.transform.Find("Label").GetComponent<Text>().text);
						Debug.Log(x.transform.Find("Label").GetComponent<Text>().text + " added to picked list");
					}
					else if (!x.isOn && x.gameObject.activeSelf)
					{
						eliminated.Add(x.transform.Find("Label").GetComponent<Text>().text);
						Debug.Log(x.transform.Find("Label").GetComponent<Text>().text + " added to eliminated list");
					}
				}
			}
			
			roundStarted = false;
			
			// reset all buttons
			foreach(Toggle x in buttons)
			{
				x.gameObject.SetActive(true);
				x.isOn = false;
			}
			
			// there will always be fewer items to check than buttons by this point
			if(itemList.Count > 0)
			{
				int numLeft = buttons.Length - itemList.Count;
				itemCount = itemList.Count;
				
				for(int i = 0;i<buttons.Length;i++)
				{
					if(i < itemList.Count)
					{
						buttons[i].transform.Find("Label").GetComponent<Text>().text = itemList[i];
					}
					else
					{
						buttons[i].gameObject.SetActive(false);
					}
				}
				
				yield return new WaitUntil(() => roundStarted == true);
			}
			
			// After pressing confirm, the items get placed into their lists.
			foreach(Toggle x in buttons)
			{
				if(x.isOn && x.gameObject.activeSelf)
				{
					picked.Add(x.transform.Find("Label").GetComponent<Text>().text);
					Debug.Log(x.transform.Find("Label").GetComponent<Text>().text + " added to picked list");
				}
				else if (!x.isOn && x.gameObject.activeSelf)
				{
					eliminated.Add(x.transform.Find("Label").GetComponent<Text>().text);
					Debug.Log(x.transform.Find("Label").GetComponent<Text>().text + " added to eliminated list");
				}
			}
			
			// add eliminated first, THEN picked so picked will always be pulled off the stack first
			if(eliminated.Count > 0)
			{
				itemStack.Push(eliminated);
			}
			if(picked.Count > 0)
			{
				itemStack.Push(picked);
			}
		}
	}
	
	void CreateFile()
	{
		string path = Application.dataPath + "/Ranked.txt";
		
		File.WriteAllText(path, "Rankings:");
	}
	
	void AddText(string textToAdd)
	{
		string path = Application.dataPath + "/Ranked.txt";
		
		File.AppendAllText(path, textToAdd);
	}
	
	public void ShowRankings()
	{
		if(rankingsPanel.activeSelf)
		{
			rankingsPanel.SetActive(false);
			showRankingsButton.text = "Show Rankings";
		}
		else
		{
			rankingsPanel.SetActive(true);
			string rankingsString = "";
			for(int i = 0;i<ranked.Count;i++)
			{
				rankingsString += i+1 + ". " +ranked[i] + ", ";
			}
			rankingsDisplay.text = rankingsString;
			showRankingsButton.text = "Hide Rankings";
		}
	}
	
	/*public void startPicked()
	{
		pickStarted = true;
	}
	
	public void startEliminated()
	{
		eliminatedStarted = true;
	}
	
	// this will run every time the confirm button is pressed
	// should be recursive and run on any list passed in
	public IEnumerator nextRound(List<string> toCheck)
	{
		stackCount++;
		Debug.Log(stackCount);
		
		pickStarted = false;
		eliminatedStarted = false;
		
		pickedButton.SetActive(false);
		eliminatedButton.SetActive(false);
		confirmButton.SetActive(true);
		
		foreach(string x in ranked)
		{
			Debug.Log(x);
		}
		
		List<string> picked = new List<string>();
		List<string> eliminated = new List<string>();
		
		// reset all buttons
		foreach(Toggle x in buttons)
		{
			x.gameObject.SetActive(true);
			x.isOn = false;
		}
		
		// exit condition
		// if only one item exists on the chosen list, then 
		if(toCheck.Count == 1)
		{
			ranked.Add(toCheck[0]);
			yield break;
		}
		
		while(toCheck.Count >= buttons.Length)
		{
			//Debug.Log("Picked: " + picked.Count);
			//Debug.Log("Eliminated: " + eliminated.Count);
			
			roundStarted = false;
			
			// pick out and remove items from the toCheck list to display on buttons.
			foreach(Toggle x in buttons)
			{
				x.isOn = false; // reset button presses
				int index = Random.Range(0,toCheck.Count);
				x.transform.Find("Label").GetComponent<Text>().text = toCheck[index];
				toCheck.RemoveAt(index);
			}
			yield return new WaitUntil(() => roundStarted == true);
			
			// After pressing confirm, the items get placed into their lists.
			foreach(Toggle x in buttons)
			{
				if(x.isOn)
				{
					picked.Add(x.transform.Find("Label").GetComponent<Text>().text);
				}
				else
				{
					eliminated.Add(x.transform.Find("Label").GetComponent<Text>().text);
				}
			}
		}
		
		// there will always be fewer items to check than buttons by this point
		if(toCheck.Count > 0)
		{
			int numLeft = buttons.Length - toCheck.Count;
			for(int i = 0;i<buttons.Length;i++)
			{
				if(i < toCheck.Count)
				{
					buttons[i].transform.Find("Label").GetComponent<Text>().text = toCheck[i];
				}
				else
				{
					buttons[i].gameObject.SetActive(false);
				}
			}
		}
		
		yield return new WaitUntil(() => roundStarted == true);
		
		// After pressing confirm, the items get placed into their lists.
		foreach(Toggle x in buttons)
		{
			if(x.isOn)
			{
				picked.Add(x.transform.Find("Label").GetComponent<Text>().text);
			}
			else
			{
				eliminated.Add(x.transform.Find("Label").GetComponent<Text>().text);
			}
		}
		
		
		// The issue here is that every single coroutine is running at once every time you go down a level of recursion. Every one of them is listening for roundStarted and every time they see it, every one will create more. I need some way to pause them as you go down the recursion stack.
		
		// Now the problem is slightly fixed for the first round, but the picked button hangs around because there are multiple coroutines sitting at the this point in the code and repeatedly enabling the picked code
		
		pickedButton.SetActive(true);
		eliminatedButton.SetActive(false);
		confirmButton.SetActive(false);
		
		yield return new WaitUntil(() => pickStarted == true);
		// after the toCheck list is exhausted, recur with the picked list
		picked = picked.Distinct().ToList();
		StartCoroutine(nextRound(picked));
		
		pickedButton.SetActive(false);
		eliminatedButton.SetActive(true);
		confirmButton.SetActive(false);
		
		yield return new WaitUntil(() => eliminatedStarted == true);
		// after breaking out a recursion stack, run recur with the eliminated list
		eliminated = eliminated.Distinct().ToList();
		StartCoroutine(nextRound(eliminated));
	}*/
	
	// sort item dictionary by value
	public Dictionary<string, int> sort(Dictionary<string, int> toSort)
	{
		Dictionary<string, int> sortedDict = new Dictionary<string, int>();
		var sorted = from pair in toSort orderby pair.Value ascending select pair;
		
		foreach(KeyValuePair<string, int> kvp in sorted)
		{
			sortedDict.Add(kvp.Key, kvp.Value);
		}
		
		return sortedDict;
	}
}
