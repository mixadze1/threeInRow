using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardGenerate : MonoBehaviour
{

	[SerializeField] private List<Sprite> gameCharacters = new List<Sprite>();
	[SerializeField] private Sprite charactersBad;
	[SerializeField] private Sprite newCharactersBad;
	[SerializeField] private GameObject tile;
	[SerializeField] private GameObject gameObject;
	[SerializeField] private int xSize, ySize;
	[SerializeField] private int characterBadSize = 3;

	private GameObject[,] tiles;
	private GameObject[,] badTiles;

	private bool isShifting;
	public bool IsShifting => isShifting;

	public static BoardGenerate instance;
    void Start()
	{
		instance = GetComponent<BoardGenerate>();
		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
		CreateBoard(offset.x, offset.y);
		CreateBoardBadCharacter(offset.x, offset.y);
	}

	private void CreateBoard(float xOffset, float yOffset)
	{
		tiles = new GameObject[xSize, ySize];
		float startX = transform.position.x;
		float startY = transform.position.y;

		Sprite[] previousLeft = new Sprite[ySize];
		Sprite previousBelow = null;

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
				newTile.transform.parent = transform;
				List<Sprite> possibleCharacters = new List<Sprite>();
				possibleCharacters.AddRange(gameCharacters);
				possibleCharacters.Remove(previousLeft[y]);
				possibleCharacters.Remove(previousBelow);
				Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
				newTile.GetComponent<SpriteRenderer>().sprite = newSprite;
				previousLeft[y] = newSprite;
				previousBelow = newSprite;
				tiles[x, y] = newTile;
			}
		}
	}
	private void CreateBoardBadCharacter(float xOffset, float yOffset)
	{
		badTiles = new GameObject[xSize, ySize];

		float startX = transform.position.x;
		float startY = transform.position.y;

		for (int i = 0; i < characterBadSize; i++)
		{
			float x = Random.Range(0, xSize);
			float y = Random.Range(0, ySize);
			
			GameObject badNewTile = Instantiate(gameObject, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
			badTiles[(int)x, (int)y] = badNewTile;
			badNewTile.layer = 2;
			badNewTile.GetComponent<SpriteRenderer>().sortingOrder = 2;
			badNewTile.GetComponent<SpriteRenderer>().sprite = charactersBad;
		}
	}
	public void RandomCharactersUseOldCharacters()
	{
		for (int x = 0; x < xSize - 1; x++)
		{
			for (int y = 0; y < ySize - 1; y++)
			{
				tiles[x, y].GetComponent<SpriteRenderer>().sprite = tiles[x + 1, y + 1].GetComponent<SpriteRenderer>().sprite;
			}
		}
	}
	public IEnumerator FindNullTiles()
	{
		int countScoreTwo = 0;
		for (int x = 0; x < xSize; x++)
		{
			for (int y = ySize - 1; y >= 0; y--)
			{
				if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					countScoreTwo++;
					   yield return StartCoroutine(ShiftTilesUp(x, y, countScoreTwo));
					if (countScoreTwo == 2)
					{ 
						countScoreTwo = 0;
					}
					break;
				}
			}
		}

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				tiles[x, y].GetComponent<Tile>().ClearAllMatches();
			}
		}
	}

	private IEnumerator ShiftTilesUp(int x, int yStart, int countScoreTwo, float shiftDelay = .1f)
	{
		isShifting = true;
		List<SpriteRenderer> renders = new List<SpriteRenderer>();
		int nullCount = 0;
		SearchBadCharacters(x, yStart);
		for (int y = yStart; y >= 0; y--)
		{
			SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
			if (render.sprite == null)
			{
				nullCount++;
			}
			renders.Add(render);
		}
		CalculateScore(nullCount, countScoreTwo);
		if (countScoreTwo == 2)
		{
			countScoreTwo = 0;
		}
		for (int i = 0; i < nullCount; i++)
		{
			yield return new WaitForSeconds(shiftDelay);
			Debug.Log("tut");
			Debug.Log(renders.Count);
			if (renders.Count <= 1)
            {
				renders[0].sprite = null;
				renders[0].sprite = GetNewSprite(x, yStart);
				Debug.Log(renders[0]);
			}
            else
			{
				for (int countMassiveRender = 0; countMassiveRender < renders.Count - 1; countMassiveRender++)
				{
					renders[countMassiveRender].sprite = renders[countMassiveRender + 1].sprite;
					renders[countMassiveRender + 1].sprite = GetNewSprite(x, yStart - 1);
					Debug.Log(renders[countMassiveRender]);
				}
			}
		}
		isShifting = false;
	}

	private void CalculateScore(int nullCount, int countScoreTwo)
	{
		if (nullCount == 1 && countScoreTwo < 2)
        {
			GUIManager.instance.Score += 5;
		}
		else
		{
			for (int i = 0; i < nullCount - 1; i++)
			{
				GUIManager.instance.Score += 5;
			}
		}
	}

	private void SearchBadCharacters(int x, int yStart)
	{
		for (int y = ySize - 1; y >= 0; y--)
		{
			if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
			{
				if (badTiles[x, y] == null)
					break;

				badTiles[x, y].GetComponent<SpriteRenderer>().sprite = newCharactersBad;
				badTiles[x, y].GetComponent<SpriteRenderer>().sortingOrder = 2;
			}
		}
	}

	private Sprite GetNewSprite(int x, int y)
	{
		List<Sprite> valuesCharacter = new List<Sprite>();
		valuesCharacter.AddRange(gameCharacters);
        if (x < xSize - 1)
        {
            valuesCharacter.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
        }
		return valuesCharacter[Random.Range(0, valuesCharacter.Count)];
	}
}
