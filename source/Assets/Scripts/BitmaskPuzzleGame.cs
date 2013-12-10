using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// when an arrow is pressed, it will contain this data to figure out what to do with the board
public struct ArrowData {
	public Direction direction;
	public int index;

	public ArrowData(Direction direction, int index) {
		this.direction = direction;
		this.index = index;
	}
}

// to help us determine which arrow was pressed
public enum Direction {
	Up,
	Right,
	Down,
	Left
}

public class BitmaskPuzzleGame : AbstractPage, FMultiTouchableInterface {
	public LineTile[][] tileMap;										// contains all the map's tiles
	public List<MatchGroup> matchGroups = new List<MatchGroup>();		// contains all the groups of connected tiles

	private Color flashingColor = new Color(1.0f, 0.1f, 0.2f);			// the current color of a matchgroup that is deemed to be "closed"
	private float _flashingColorPercentage = 1;							// this will contantly go up and down and multiply the flashingColor
	private bool matchGroupsAreDirty = true;							// when a row/column is shifted, this is set to true so HandleUpdate knows to refresh
	private float minBoardMargin = 75;									// amount of extra space on the sides/top/bottom of the board
	private float tileSize = 30;										// size of tiles in points
	private int tileMapWidth;											// how many tiles wide the board is
	private int tileMapHeight;											// how many tiles high the board is
	private FContainer boardContainer;									// just the visual container that holds everything

	public BitmaskPuzzleGame () {
		tileMapWidth = (int)((Futile.screen.width - minBoardMargin) / tileSize);
		tileMapHeight = (int)((Futile.screen.height - minBoardMargin) / tileSize);

		InitTileMap();
		InitArrows();
		InitFlashingColorForCompletedLines();
	}

	private void InitTileMap() {
		// setup overall container and place it in the middle of the screen
		boardContainer = new FContainer();
		boardContainer.x = Futile.screen.halfWidth - tileMapWidth * tileSize / 2f;
		boardContainer.y = Futile.screen.halfHeight - tileMapHeight * tileSize / 2f;
		AddChild(boardContainer);

		// create the blank background tiles
		for (int i = 0; i < tileMapWidth; i++) {
			for (int j = 0; j < tileMapHeight; j++) {
				FSprite backgroundTile = new FSprite("WhiteBox");
				backgroundTile.width = backgroundTile.height = tileSize;
				backgroundTile.color = new Color(0.2f, 0.2f, 0.2f);
				backgroundTile.x = (i + 0.5f) * tileSize;
				backgroundTile.y = (j + 0.5f) * tileSize;
				boardContainer.AddChild(backgroundTile);
			}
		}

		// create random tiles and fill the board with them
		tileMap = new LineTile[tileMapWidth][];
		for (int i = 0; i < tileMapWidth; i++) {
			tileMap[i] = new LineTile[tileMapHeight];
			for (int j = 0; j < tileMapHeight; j++) {
				LineTileType randomTileType = (LineTileType)Random.Range(0, (int)LineTileType.MAX);
				RotationType randomRotationType = (RotationType)Random.Range(0, (int)RotationType.MAX);
				LineTile newTile = new LineTile(randomTileType, randomRotationType);
				newTile.tileIndex.xIndex = i;
				newTile.tileIndex.yIndex = j;
				newTile.sprite.width = newTile.sprite.height = tileSize;
				tileMap[i][j] = newTile;
				newTile.x = (i + 0.5f) * tileSize;
				newTile.y = (j + 0.5f) * tileSize;
				boardContainer.AddChild(newTile);
			}
		}

		// create the tile outlines
		for (int i = 0; i < tileMapWidth; i++) {
			for (int j = 0; j < tileMapHeight; j++) {
				FSprite backgroundTile = new FSprite("lineTileOutline");
				backgroundTile.width = backgroundTile.height = tileSize;
				backgroundTile.color = new Color(0.2f, 0.2f, 0.2f);
				backgroundTile.x = (i + 0.5f) * tileSize;
				backgroundTile.y = (j + 0.5f) * tileSize;
				boardContainer.AddChild(backgroundTile);
			}
		}
	}

	// this creates all the arrows around the board and sets them up to be able to send their data when pressed and released
	private void InitArrows() {
		for (int i = 0; i < tileMapWidth; i++) {
			FButton arrowButtonUp = new FButton("arrow", "arrow", "click");
			arrowButtonUp.SetPosition((i + 0.5f) * tileSize, (tileMapHeight + 0.5f) * tileSize);
			arrowButtonUp.SetColors(Color.white, Color.red);
			arrowButtonUp.data = new ArrowData(Direction.Up, i);
			arrowButtonUp.SignalRelease += ArrowButtonReleased;
			boardContainer.AddChild(arrowButtonUp);

			FButton arrowButtonDown = new FButton("arrow", "arrow", "click");
			arrowButtonDown.SetPosition((i + 0.5f) * tileSize, -0.5f * tileSize);
			arrowButtonDown.SetColors(Color.white, Color.red);
			arrowButtonDown.data = new ArrowData(Direction.Down, i);
			arrowButtonDown.SignalRelease += ArrowButtonReleased;
			arrowButtonDown.rotation = 180;
			boardContainer.AddChild(arrowButtonDown);
		}

		for (int j = 0; j < tileMapHeight; j++) {
			FButton arrowButtonRight = new FButton("arrow", "arrow", "click");
			arrowButtonRight.SetPosition((tileMapWidth + 0.5f) * tileSize, (j + 0.5f) * tileSize);
			arrowButtonRight.SetColors(Color.white, Color.red);
			arrowButtonRight.data = new ArrowData(Direction.Right, j);
			arrowButtonRight.SignalRelease += ArrowButtonReleased;
			arrowButtonRight.rotation = 90;
			boardContainer.AddChild(arrowButtonRight);

			FButton arrowButtonLeft = new FButton("arrow", "arrow", "click");
			arrowButtonLeft.SetPosition(-0.5f * tileSize, (j + 0.5f) * tileSize);
			arrowButtonLeft.SetColors(Color.white, Color.red);
			arrowButtonLeft.data = new ArrowData(Direction.Left, j);
			arrowButtonLeft.SignalRelease += ArrowButtonReleased;
			arrowButtonLeft.rotation = 270;
			boardContainer.AddChild(arrowButtonLeft);
		}
	}

	// sets up the tweens so the flashingColor will actually flash bright/dark
	private void InitFlashingColorForCompletedLines() {
		Tween colorDown = new Tween(this, 0.2f, new TweenConfig().floatProp("flashingColorPercentage", 0.5f));
		Tween colorUp = new Tween(this, 0.2f, new TweenConfig().floatProp("flashingColorPercentage", 1.0f));
		TweenChain chain = new TweenChain();
		chain.setIterations(-1);
		chain.append(colorDown).append(colorUp);
		Go.addTween(chain);
		chain.play();
	}

	// when the tweens change this property, it will automatically set the flashingColor accordingly
	public float flashingColorPercentage {
		get {return _flashingColorPercentage;}
		set {
			_flashingColorPercentage = value;

			flashingColor = new Color(1.0f * flashingColorPercentage, 0.1f * flashingColorPercentage, 0.2f * flashingColorPercentage);
		}
	}

	public void HandleUpdate() {
		// if we need to refresh the board, update the match groups and set all the non-closed matchgroup tiles to white
		if (matchGroupsAreDirty) {
			UpdateMatches();

			foreach (MatchGroup matchGroup in matchGroups) {
				if (matchGroup.isClosed) continue;

				matchGroup.SetTileColor(Color.white);
			}
		}

		// every frame, update the color of any closed matchgroup tiles to be flashing
		foreach (MatchGroup matchGroup in matchGroups) {
			if (matchGroup.isClosed) matchGroup.SetTileColor(flashingColor);
		}
	}

	// go through the board and analyze all the tiles, looking for matches
	private void UpdateMatches() {
		// match groups are being updated so they're no longer dirty
		matchGroupsAreDirty = false;

		// since sliding columns/rows can mess up everything, we need to get rid of the old match groups and start over.
		// keep in mind there's probably a way to use the algorithm where we don't have to get rid of all the matches and
		// start over every time (say, just update the matches that are disrupted by a shift), but that can come later if
		// you need to improve performance
		foreach (MatchGroup matchGroup in matchGroups) matchGroup.Destroy();
		matchGroups.Clear();

		// we'll start analyzing the board from the bottom left tile. the current base tile will be the one
		// that we are currently starting from and building match groups off of.
		LineTile currentBaseTile = tileMap[0][0];

		List<LineTile> tileSurrounders; // variable that will store surrounding tiles of various base tiles
		List<LineTile> checkedTiles = new List<LineTile>(); // we'll store base tiles here once they've been analyzed so we don't reanalyze them
		MatchGroup currentMatchGroup; // the match group we're analyzing that includes the current base tile

		// loop continuously through the board, making match groups until there are no more tiles to make match groups from
		while (currentBaseTile != null) {
			// create a new match group, add the current base tile as its first tile
			currentMatchGroup = new MatchGroup();
			currentMatchGroup.tiles.Add(currentBaseTile);

			// loop through the tiles starting on the current base tile, analyze their connections, find a new base tile,
			// and loop again, and so on until you find no more possible connections any of the tiles in the match group
			bool stillWorkingOnMatchGroup = true;

			while (stillWorkingOnMatchGroup) {
				// populate the tileSurrounders list with all the tiles surrounding the current base tile
				tileSurrounders = GetTilesSurroundingTile(currentBaseTile);

				// iterate through all the surrounding tiles and check if their solid sides are aligned with the base tile's solid sides
				foreach (LineTile surroundingTile in tileSurrounders) {
					TileConnectionType connectionType = TileConnectionTypeBetweenTiles(currentBaseTile, surroundingTile);

					// if there's a solid match, add the surrounder to the match group.
					// if there's a mismatch, the matchgroup is not a perfect "closed" match group.
					// if there's a mismatch because of an open side of the base tile, that doesn't actually matter
					// since there's not a solid side being cut off (this is called TileConnectionType.ValidWithOpenSide)
					if (connectionType == TileConnectionType.ValidWithSolidMatch) currentMatchGroup.tiles.Add(surroundingTile);
					else if (TileConnectionTypeBetweenTiles(currentBaseTile, surroundingTile) == TileConnectionType.Invalid) currentMatchGroup.isClosed = false;
				}

				// if the base tile has a closed/solid side that touches the edge of the board, the match group can't be closed
				if (((currentBaseTile.bitmask & LineTile.kBitmaskTop) != 0 && currentBaseTile.tileIndex.yIndex == tileMapHeight - 1) ||
				    ((currentBaseTile.bitmask & LineTile.kBitmaskRight) != 0 && currentBaseTile.tileIndex.xIndex == tileMapWidth - 1) ||
				    ((currentBaseTile.bitmask & LineTile.kBitmaskBottom) != 0 && currentBaseTile.tileIndex.yIndex == 0) ||
				    ((currentBaseTile.bitmask & LineTile.kBitmaskLeft) != 0 && currentBaseTile.tileIndex.xIndex == 0)) currentMatchGroup.isClosed = false;

				// add our base tile to an array so we don't check it again later
				if (!checkedTiles.Contains(currentBaseTile)) checkedTiles.Add(currentBaseTile);

				// find a new base tile that we've added to the match gropu but haven't analyzed yet
				for (int i = 0; i < currentMatchGroup.tiles.Count; i++) {
					LineTile tile = currentMatchGroup.tiles[i];

					// if the checkedTiles array has the tile in it already, check to see if we're on the last
					// tile in the match group. if we are, then there are no more base tile possibilities so
					// done with the match group. if checkedTiles DOESN'T have a tile in the array, it means
					// that tile is in the match group but hasn't been analyzed yet, so we need to set it as
					// the next base tile.
					if (checkedTiles.Contains(tile)) {
						if (i == currentMatchGroup.tiles.Count - 1) {
							stillWorkingOnMatchGroup = false;
							matchGroups.Add(currentMatchGroup);
						}
					}
					else {
						currentBaseTile = tile;
						break;
					}
				}
			}

			// we're done with a match group, so now we need to find a new un-analyzed tile that's
			// not in any match groups to start a new one off of. so we'll set currentBaseTile to
			// null then see if we can find a new one.
			currentBaseTile = null;

			for (int i = 0; i < tileMapWidth; i++) {
				for (int j = 0; j < tileMapHeight; j++) {
					LineTile newTile = tileMap[i][j];

					if (!TileIsAlreadyInMatchGroup(newTile)) {
						currentBaseTile = newTile;
						break;
					}
				}
				if (currentBaseTile != null) break;
			}
		}
	}

	// when an arrow is pressed and released, shift a column up/down or a row right/left
	public void ArrowButtonReleased(FButton button) {
		ArrowData arrowData = (ArrowData)button.data;

		if (arrowData.direction == Direction.Up || arrowData.direction == Direction.Down) {
			ShiftColumnInDirection(arrowData.index, arrowData.direction);
		}

		else if (arrowData.direction == Direction.Right || arrowData.direction == Direction.Left) {
			ShiftRowInDirection(arrowData.index, arrowData.direction);
		}
	}

	// shift the tiles in a column either up or down one (with wrapping)
	private void ShiftColumnInDirection(int columnIndex, Direction dir) {
		LineTile[] currentColumnArrangement = GetColumnTiles(columnIndex);
		int nextIndex;

		// move the tiles so they are in the correct spots in the tileMap array
		if (dir == Direction.Up) {
			for (int j = 0; j < tileMapHeight; j++) {
				nextIndex = (j + 1) % tileMapHeight;

				tileMap[columnIndex][nextIndex] = currentColumnArrangement[j];
				tileMap[columnIndex][nextIndex].tileIndex = new TileIndex(columnIndex, nextIndex);
			}
		}
		else if (dir == Direction.Down) {
			for (int j = 0; j < tileMapHeight; j++) {
				nextIndex = j - 1;
				if (nextIndex < 0) nextIndex += tileMapHeight;

				tileMap[columnIndex][nextIndex] = currentColumnArrangement[j];
				tileMap[columnIndex][nextIndex].tileIndex = new TileIndex(columnIndex, nextIndex);
			}
		}
		else throw new FutileException("can't shift column in direction: " + dir.ToString());

		// once the tileMap array is set up, actually visually move the tiles to their correct spots
		for (int j = 0; j < tileMapHeight; j++) {
			tileMap[columnIndex][j].y = (j + 0.5f) * tileSize;
		}

		matchGroupsAreDirty = true;
	}

	// shift the tiles in a row either right or left one (with wrapping)
	private void ShiftRowInDirection(int rowIndex, Direction dir) {
		LineTile[] currentRowArrangement = GetRowTiles(rowIndex);
		int nextIndex;

		// move the tiles so they are in the correct spots in the tileMap array
		if (dir == Direction.Right) {
			for (int i = 0; i < tileMapWidth; i++) {
				nextIndex = (i + 1) % tileMapWidth;

				tileMap[nextIndex][rowIndex] = currentRowArrangement[i];
				tileMap[nextIndex][rowIndex].tileIndex = new TileIndex(nextIndex, rowIndex);
			}
		}
		else if (dir == Direction.Left) {
			for (int i = 0; i < tileMapWidth; i++) {
				nextIndex = i - 1;
				if (nextIndex < 0) nextIndex += tileMapWidth;

				tileMap[nextIndex][rowIndex] = currentRowArrangement[i];
				tileMap[nextIndex][rowIndex].tileIndex = new TileIndex(nextIndex, rowIndex);
			}
		}
		else throw new FutileException("can't shift row in direction: " + dir.ToString());

		// once the tileMap array is set up, actually visually move the tiles to their correct spots
		for (int i = 0; i < tileMapWidth; i++) {
			tileMap[i][rowIndex].x = (i + 0.5f) * tileSize;
		}

		matchGroupsAreDirty = true;
	}

	// helper method to get all the tiles in a specific column
	private LineTile[] GetColumnTiles(int columnIndex) {
		if (columnIndex < 0 || columnIndex >= tileMapWidth) throw new FutileException("invalid column: " + columnIndex);

		LineTile[] columnTiles = new LineTile[tileMapHeight];

		for (int j = 0; j < tileMapHeight; j++) columnTiles[j] = tileMap[columnIndex][j];

		return columnTiles;
	}

	// helper method to get all the tiles in a specific row
	private LineTile[] GetRowTiles(int rowIndex) {
		if (rowIndex < 0 || rowIndex >= tileMapHeight) throw new FutileException("invalid column: " + rowIndex);

		LineTile[] rowTiles = new LineTile[tileMapWidth];

		for (int i = 0; i < tileMapWidth; i++) rowTiles[i] = tileMap[i][rowIndex];

		return rowTiles;
	}

	// helper function to see if a tile already belongs to a match group
	private bool TileIsAlreadyInMatchGroup(LineTile tile) {
		bool tileIsAlreadyInMatchGroup = false;

		foreach (MatchGroup matchGroup in matchGroups) {
			if (matchGroup.tiles.Contains(tile)) {
				tileIsAlreadyInMatchGroup = true;
				break;
			}
		}

		return tileIsAlreadyInMatchGroup;
	}

	// there are three types of connections two tiles can have.
	// 1. ValidWithSolidMatch: this means the tiles are accurately matched with their solid sides connected
	// 2. ValidWithOpenSide: this means the baseTile has an open side touching the other tile, so it doesn't matter what the other tile is
	// 3. Invalid: this means the baseTile's solid side is matched with the other tile's open side, resulting in a mismatch
	private TileConnectionType TileConnectionTypeBetweenTiles(LineTile baseTile, LineTile otherTile) {
		int baseTileBitmaskSide = baseTile.bitmask; // the bitmask for the specific baseTile side that is touching the other tile
		int otherTileBitmaskSide = otherTile.bitmask; // the bitmask for the specific otherTile side that is touching the base tile

		// depending on which side of the base tile the other tile is on, bitwise & each side together with
		// the bitwise constant for that individual side. if the result is 0, then the side is open. otherwise,
		// the side is solid.
		if (otherTile.tileIndex.yIndex < baseTile.tileIndex.yIndex) {
			baseTileBitmaskSide &= LineTile.kBitmaskBottom;
			otherTileBitmaskSide &= LineTile.kBitmaskTop;
		}
		else if (otherTile.tileIndex.yIndex > baseTile.tileIndex.yIndex) {
			baseTileBitmaskSide &= LineTile.kBitmaskTop;
			otherTileBitmaskSide &= LineTile.kBitmaskBottom;
		}
		else if (otherTile.tileIndex.xIndex < baseTile.tileIndex.xIndex) {
			baseTileBitmaskSide &= LineTile.kBitmaskLeft;
			otherTileBitmaskSide &= LineTile.kBitmaskRight;
		}
		else if (otherTile.tileIndex.xIndex > baseTile.tileIndex.xIndex) {
			baseTileBitmaskSide &= LineTile.kBitmaskRight;
			otherTileBitmaskSide &= LineTile.kBitmaskLeft;
		}

		if (baseTileBitmaskSide == 0) return TileConnectionType.ValidWithOpenSide; // baseTile side touching otherTile is open
		else if (otherTileBitmaskSide != 0) return TileConnectionType.ValidWithSolidMatch; // baseTile side and otherTile side are solid and matched
		else return TileConnectionType.Invalid; // baseTile side is solid but otherTile side is open. mismatch!
	}

	// helper method to get all the tiles that are above/below/right/left of a specific tile
	private List<LineTile> GetTilesSurroundingTile(LineTile tile) {
		List<LineTile> surroundingTiles = new List<LineTile>();

		int xIndex = tile.tileIndex.xIndex;
		int yIndex = tile.tileIndex.yIndex;

		if (xIndex > 0) surroundingTiles.Add(tileMap[xIndex - 1][yIndex]);
		if (xIndex < tileMapWidth - 1) surroundingTiles.Add(tileMap[xIndex + 1][yIndex]);

		if (yIndex > 0) surroundingTiles.Add(tileMap[xIndex][yIndex - 1]);
		if (yIndex < tileMapHeight - 1) surroundingTiles.Add(tileMap[xIndex][yIndex + 1]);

		return surroundingTiles;
	}
	
	override public void Start() {	
		EnableMultiTouch();
		ListenForUpdate(HandleUpdate);
	}

	override public void Destroy() {	

	}

	public void HandleMultiTouch(FTouch[] touches) {
		foreach (FTouch touch in touches) {
			if (touch.phase == TouchPhase.Began) {
				// we don't need to handle touches at this point since the arrows do that themselves
			}
		}
	}
}




















