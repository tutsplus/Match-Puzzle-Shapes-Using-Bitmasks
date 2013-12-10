using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// a simple class to store groups of tiles and do things to them without writing foreach functions a bunch of times

public class MatchGroup {
	public List<LineTile> tiles;
	public bool isClosed = true;

	public MatchGroup() {
		tiles = new List<LineTile>();
	}

	public void SetTileColor(Color color) {
		foreach (LineTile tile in tiles) tile.sprite.color = color;
	}

	public void Destroy() {
		tiles.Clear();
	}
}
