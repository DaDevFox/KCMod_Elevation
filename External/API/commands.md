**Data Structures**:
*ElevationTileData (struct)*:
```cs
int x
int z
int elevationTier
```
*RefreshTileData (struct)*
```cs
int x
int z
bool force
```
**Commands:**
*Elevation*
```cs
// Updates the visual meshes and objects on each tile of all tiles in the world; if forceUpdate is true, will force an update even if there has been no change to the tile's elevation (updates color)
Refresh(bool forceUpdate) : void

// Updates the visual meshes and objects on a single in the world
RefreshTile(RefreshTileData data) : void

// Sets the elevaiton tier of a tile at the position (data.x, data.z) to data.elevationTier
Set(ElevationTileData data) : void
// Gets the elevation tier of a tile at the given position
Get(Vector2 position) : int
```