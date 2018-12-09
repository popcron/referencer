# Referencer
Used for getting a reference to all assets in the project. Which automatically listens for file changes and ensures that all registered assets can be referenced at runtime without the need of manually creating lists and fields to keep track.

This was created so I could reference all of my assets via code with a single line. I'm lazy.

## Requirements
- .NET Framework 3.5

## Installation
If using 2018.3.x, you can add a new entry to the manifest.json file in your Packages folder:
```json
"com.popcron.referencer": "https://github.com/popcron/referencer.git"
```

On first fresh loads, you might have to load all assets using the `Popcron/Referencer/Load all` menu item.

## Example
```cs
using UnityEngine;
using Popcron.Referencer;

public class Player : MonoBehaviour
{
    private Sprite playerSprite;
    private Sprite gunSprite;
    
    private void Awake()
    {
        //load using an absolute path
        playerSprite = Referencer.Get<Sprite>("Art/Sprites/Player.psd/Player_0");
        
        //load using just the name, if there are duplicates it will find the first one
        gunSprite = Referencer.Get<Sprite>("GunSprite");
        
        //returns all scriptable objects of type Gun
        List<Gun> allGuns = Referencer.GetAll<Gun>();
        //List<Gun> allGuns = Referencer.GetAll(typeof(Gun));
        
        //gets the original path of the sprite asset
        string path = Referencer.GetPath(gunSprite);
        
        //returns a random asset from all guns
        Gun randomGun = Referencer.GetRandom<Gun>();
        
        //returns a gun with ID 12
        Gun pistol = Referencer.Get<Gun>(12);
    }
}
```

## Loaders
This package contains loaders for the follwing types:
- AudioClip
- Font
- Material
- GameObject (Prefabs)
- ScriptableObject
- Sprite

To create a custom loader, you can inherit from the `AssetLoader` type. For an example, see the provided loader classes in the Loaders folder.

**ScriptableObject IDs**

In addition to the scriptable object loader, it will also detect if the type has an ID property or field. If it does, the loader will recognize it and register it for each item. This allows you to retrieve the asset using its ID.

```cs
//get a gun scriptable object asset that has an ID of 12
int id = 12;
Gun gun = References.Get<Gun>(id);
```

## Settings
The `Settings` class has a static property called `Current`. This property holds the settings that are used by the loader. If provided with a custom value it will use that one instead of the defaults. If given null, it will use defaults.

The class contains fields for where the reference file should be stored, default is `Assets/References.asset`. As well as an array for which folders and extensions should be ignored.

## FAQ
- **What namespace?** Popcron.Referencer
- **It loads everything?** Everything that it was asked to
- **Can I use it at runtime?** Yes
- **Can I make my own custom loader?** Yes
- **Is it optimized?** For speed, yes
- **How can I ignore folder X?** Provide a value to `Settings.Current` with your own ignoreFolders value
