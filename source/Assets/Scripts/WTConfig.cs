using UnityEngine;
using System.Collections;

public static class WTConfig {
	public const float tileSize = 64;
	public const float gravity = -1500;
	public const float bounceConstant = 0.5f;
	public const float minBounceDist = 3f;
	public const float frictionConstant = 5f;

	public static Vector2 maxVelocity = new Vector2(400, 475);
	public static Vector2 playerDrag = new Vector2(50, 0);
	public static Vector2 objectDrag = new Vector2(1000, 0);
}
