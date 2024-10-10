Using HTC Vive Tracker 3.0 without a headset connected.
Working camera example: `Tracker.unity`

# SteamVR Modifications
SteamVR will not work out of the box without a headset, some modifications are required:
```
Null Driver HMD
    C:\Program Files (x86)\Steam\steamapps\common\SteamVR\drivers\null\resources\settings\default.vrsettings
        "enable": true,
        "windowWidth": 0,
        "windowHeight": 0,
    C:\Program Files (x86)\Steam\steamapps\common\SteamVR\resources\settings\default.vrsettings
        "requireHmd": false,
        "forcedDriver": "null",
        "activateMultipleDrivers": true,
Never turn off null HMD (optional, un-needed)
    C:\Program Files (x86)\Steam\steamapps\common\SteamVR\resources\settingsschema.vrsettings
        Settings_Power_TurnOffScreensTimeout
            {"value": "0", "label": "#SettingsTime_Never" },

HMD Tracking Pose Override (optional, telling SteamVR to use tracker for hmd camera position)
    C:\Program Files (x86)\Steam\config\steamvr.vrsettings
        "TrackingOverrides" : {
          "/devices/htc/vive_trackerLHR-686398D9" : "/user/head"
        },
```
