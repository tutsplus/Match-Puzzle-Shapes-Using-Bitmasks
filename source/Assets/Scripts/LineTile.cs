using UnityEngine;
using System.Collections;

// the different tile types
public enum LineTileType {
	Nub,
	Line,
	Corner,
	Threeway,
	Cross,
	MAX
}

// i'm using this instead of exact degrees since the tiles should only have four distinct rotations
public enum RotationType {
	Rotation0,
	Rotation90,
	Rotation180,
	Rotation270,
	MAX
}

// this is basically a Vector2 except with ints instead of floats
public struct TileIndex {
	public int xIndex;
	public int yIndex;
	
	public TileIndex(int xIndex, int yIndex) {
		this.xIndex = xIndex;
		this.yIndex = yIndex;
	}
}

public enum TileConnectionType {
	// a mismatch
	Invalid,

	// the tiles don't directly connect,
	// but not because of an unmatched edge
	ValidWithOpenSide,

	// the tiles directly connect
	ValidWithSolidMatch
}

public class LineTile : FContainer {
	// here are the bits i assigned each side of the tile:
	// ===== 1 =====
	// |           |
	// |           |
	// 8           2
	// |           |
	// |           |
	// ===== 4 =====

	// 1 == 0001 in binary
	// 2 == 0010 in binary
	// 4 == 0100 in binary
	// 8 == 1000 in binary

	public const int kBitmaskNone   = 0;
	public const int kBitmaskTop    = 1;
	public const int kBitmaskRight  = 2;
	public const int kBitmaskBottom = 4;
	public const int kBitmaskLeft   = 8;

	public FSprite sprite;										// the sprite representation of the tile
	public LineTileType lineTileType {get; private set;}		// the type of the tile
	public RotationType rotationType {get; private set;}		// the rotation of the tile
	public int bitmask {get; private set;}						// the bitmask that represents the tile with its rotation
	public TileIndex tileIndex = new TileIndex();				// the tile's location on the board

	public LineTile(LineTileType lineTileType, RotationType rotationType) {
		this.lineTileType = lineTileType;
		this.rotationType = rotationType;

		// set up sprite
		switch (lineTileType) {
		case LineTileType.Nub:
			sprite = new FSprite("lineTileNub");
			break;
		case LineTileType.Line:
			sprite = new FSprite("lineTileLine");
			break;
		case LineTileType.Corner:
			sprite = new FSprite("lineTileCorner");
			break;
		case LineTileType.Threeway:
			sprite = new FSprite("lineTileThreeway");
			break;
		case LineTileType.Cross:
			sprite = new FSprite("lineTileCross");
			break;
		default:
			throw new FutileException("invalid line tile type");
		}

		AddChild(sprite);

		// set up sprite rotation
		switch (rotationType) {
		case RotationType.Rotation0:
			sprite.rotation = 0;
			break;
		case RotationType.Rotation90:
			sprite.rotation = 90;
			break;
		case RotationType.Rotation180:
			sprite.rotation = 180;
			break;
		case RotationType.Rotation270:
			sprite.rotation = 270;
			break;
		default:
			throw new FutileException("invalid rotation type");
		}

		// set up bitmask by doing bitwise OR with each side that is included in the shape

		// so for example, a tile that has all four sides solid (e.g. the cross tile) would be
		// 1 | 2 | 4 | 8 = 15, which is the same as 0001 | 0010 | 0100 | 1000 = 1111 in binary

		if (lineTileType == LineTileType.Nub) {
			if (rotationType == RotationType.Rotation0) 	bitmask = kBitmaskTop;
			if (rotationType == RotationType.Rotation90) 	bitmask = kBitmaskRight;
			if (rotationType == RotationType.Rotation180)	bitmask = kBitmaskBottom;
			if (rotationType == RotationType.Rotation270) 	bitmask = kBitmaskLeft;
		}

		if (lineTileType == LineTileType.Line) {
			if (rotationType == RotationType.Rotation0 || rotationType == RotationType.Rotation180)		bitmask = kBitmaskTop | kBitmaskBottom;
			if (rotationType == RotationType.Rotation90 || rotationType == RotationType.Rotation270) 	bitmask = kBitmaskRight | kBitmaskLeft;
		}

		if (lineTileType == LineTileType.Corner) {
			if (rotationType == RotationType.Rotation0) 	bitmask = kBitmaskTop | kBitmaskRight;
			if (rotationType == RotationType.Rotation90) 	bitmask = kBitmaskRight | kBitmaskBottom;
			if (rotationType == RotationType.Rotation180) 	bitmask = kBitmaskBottom | kBitmaskLeft;
			if (rotationType == RotationType.Rotation270) 	bitmask = kBitmaskLeft | kBitmaskTop;
		}

		if (lineTileType == LineTileType.Threeway) {
			if (rotationType == RotationType.Rotation0) 	bitmask = kBitmaskTop | kBitmaskRight | kBitmaskBottom;
			if (rotationType == RotationType.Rotation90)	bitmask = kBitmaskRight | kBitmaskBottom | kBitmaskLeft;
			if (rotationType == RotationType.Rotation180)	bitmask = kBitmaskBottom | kBitmaskLeft | kBitmaskTop;
			if (rotationType == RotationType.Rotation270) 	bitmask = kBitmaskLeft | kBitmaskTop | kBitmaskRight;
		}

		if (lineTileType == LineTileType.Cross) {
			bitmask = kBitmaskTop | kBitmaskRight | kBitmaskBottom | kBitmaskLeft;
		}
	}
}



























