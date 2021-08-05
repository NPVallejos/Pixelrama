using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using GuamboCollections;

// Transparent Tile
// - Sprite Renderer (Component)
// - Layer = default
// =================================================
// Regular Tile
// - Sprite Renderer (Component)
// - BoxCollider2D (Component)
// - Layer = Obstacle

public class SwitchTileManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile activatedTile;
    public Tile deactivatedTile;
    public List<Pair<Vector3, bool>> switchTiles;

    public void ToggleSwitchTiles() {
        for (int i = 0; i < switchTiles.Count; ++i) {
            Vector3Int tilePosition = tilemap.WorldToCell(switchTiles[i].first);
            if (switchTiles[i].second == true) {
                // Deactivate Tile
                tilemap.SetTile(tilePosition, deactivatedTile);
                tilemap.SetColliderType(tilePosition, Tile.ColliderType.None);
                tilemap.RefreshTile(tilePosition);
                switchTiles[i].second = false;
            }
            else {
                // Activate Tile
                tilemap.SetTile(tilePosition, activatedTile);
                tilemap.SetColliderType(tilePosition, Tile.ColliderType.Sprite);
                tilemap.RefreshTile(tilePosition);
                switchTiles[i].second = true;
            }
        }
    }
}
