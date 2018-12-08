# Referencer
Used for getting a reference to all assets in the project. Which automatically listens for file changes and ensures that all registered assets can be referenced at runtime without the need of manually creating lists and fields to keep track.

## Requirements
- .NET Framework 3.5

## Installation
If using 2018.3.x, you can add a new entry to the manifest.json file in your Packages folder:
```json
"com.popcron.referencer": "https://github.com/popcron/referencer.git#unity"
```

On first fresh loads, you might have to load all assets using the `Popcron/Referencer/Load all` menu item.

## Example
```cs
using UnityEngine;
using Popcron.Referencer;

public class Player : MonoBehaviour
{
    private Sprite playerSprite;

    private void Awake()
    {
        playerSprite = Referencer.Get<Sprite>("Art/Sprites/Player.psd/Player_0");
    }
}
```

## Settings
The `Settings` class has a static property called `Current`. This property holds the settings that are used by the loader. If provided with a custom value it will use that one instead of the defaults. If given null, it will use defaults.

The class contains fields for where the reference file should be stored, default is `Assets/References.asset`. As well as an array for which folders and extensions to ignore.

## FAQ
- **What namespace?** Popcron.Referencer
- **It loads everything?** Yes
- **Can I use it at runtime?** Yes
- **Is it optimized?** For speed, yeah
- **How can I ignore folder X?** Provide a value to `Settings.Current` with your own ignoreFolders value
