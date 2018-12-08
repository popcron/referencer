# Referencer
Used for getting a reference to all assets in the project.

## Requirements
- .NET Framework 3.5

## Installation
Add the .dll file to the Plugins folder.

If using 2018.3.x, you can add a new entry to the manifest.json file in your Packages folder:
```json
"com.popcron.referencer": "https://github.com/popcron/referencer.git#unity"
```

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

## FAQ
- **What namespace?** Popcron.Referencer
- **It loads everything?** Yes
- **Can I use it at runtime?** Yes
- **Is it optimized?** Yes
