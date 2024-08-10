# ZeroFemurBreaker Plugin
**ZeroFemurBreaker** is an Exiled plugin for SCP: Secret Laboratory which adds the ability (by owning a custom object that acts as a button) to be able to click it via [ALT] which will kill SCP-106 after playing an audio. It also adds a mechanism to keep SCP-106's cell in lockdown until all 3 SCP-079 generators are engaged (thus useful if the custom object acting as Femur Breaker is in SCP-106's cell).

## Installation
Download the latest release of the plugin from the Releases page.
Place the downloaded ZeroFemurBreaker.dll file in the Plugins folder of your SCP: Secret Laboratory server.

## Configuration
The plugin provides the following configurable options:

- `IsEnabled` (default: `true`): Specifies whether the ZeroFemurBreaker plugin is enabled or disabled.
- `Debug` (default: `false`): Enables or disables debug mode for logging.
- `femur_breaker_activated_hint` (default: "The Femur Breaker has already been activated."): The hint that appears on the screen when the Femur Breaker has already been activated if you try to reactivate it in the same round.
- `scp_106_not_alive_hint` (default: "SCP-106 is not out of containment."): The hint that appears on the screen when you try to activate the Femur Breaker if 106 is not alive.ù
- `audio_file_path` (default: "C:\\Users\\Administrator\\AppData\\Roaming\\EXILED\\Configs\\FemurBreakerSound.ogg"): The path to the audio file that is played upon activation of the Femur Breaker.
- `fake_player_name` (default: "Femur Breaker"): The name of the player you will see speaking (on the right side of the screen) while the audio is playing.
- `game_object_name` (default: "FemurBrakerButton"): The name of the game object around which the collider will be created for the plugin to understand when you are looking at it (the button, to be clear).
  
To configure these options, open the config-(YOUR PORT).yml file located in the Configs folder and modify the values as desired.

## Commands
Nope.

## Permissions
Nope again.

## License
This plugin is licensed under the MIT License.

## Credits
Plugin created by [Raffymimi](https://github.com/Raffymimii)

## Support
If you encounter any issues or have any questions, feel free to create an issue on the GitHub repository.
