![sup](https://cdn.discordapp.com/attachments/461266635383111680/688440552580317236/unknown.png "What it looks like")

# Referencer
This is a simple utility that lets you get an asset in the project by its name without setting anything up, both in editor and runtime.

## Requirements
- Git

## Installation
If using 2018.3.x, you can add a new entry to the manifest.json file in your Packages folder:
```json
"com.popcron.referencer": "https://github.com/popcron/referencer.git"
```

## Example
```cs
//load using the full path
Sprite playerSprite = Refs.Get<Sprite>("Art/Sprites/Player.psd/Player_0");

//or load using just the name
playerSprite = Refs.Get<Sprite>("Player_0");

//load using just the name, if there are duplicates it will find the first one
GameObject playerPrefab = Refs.Get<GameObject>("Player");

//returns all scriptable objects of type Gun
List<Gun> allGuns = Refs.GetAll<Gun>();

//gets the original path of the sprite asset
string path = Refs.GetPath(gunSprite);

//returns a random gun
Gun randomGun = Refs.GetRandom<Gun>();

//for loops through every asset loaded
foreach (Reference reference in Refs.Assets)
{
    Debug.Log($"loaded in {reference.Object} at path {reference.Path}");
}
```

## Types of assets
This utility will load all assets of these types in:
- AudioClips
- Fonts
- Materials
- GameObjects (Prefabs)
- Meshes
- ScriptableObjects
- Sprite and Sprite sub assets
- Textures
- Shaders
- TextAsset

**Assets with IDs**
Included is the ScriptableObject loader, which will load all found types into the references. It will also check if the type has an `ID` property or `id` field. This gives the reference an extra piece of information for the loaded object, which will allow you to load the asset using its ID:
```cs
//get a gun scriptable object asset that has an ID of 12
int id = 12;
Gun gun = Refs.Get<Gun>(id);
```

## Git ignore
So the references database is stored at `Assets/References.asset`, and this should be ignored in source control because this asset is always regenerated.

## FAQ
- **What namespace?** Popcron.Referencer
- **It loads everything?** Yes
- **Is it optimized?** For speed, yes
