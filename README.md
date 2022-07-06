# SnapmakerControl
Application that can be used to control your Snapmaker (or any GCode compatible machine over serial) using a gamepad.

Currently it's only possible to connect to printers over a serial connection. The code is largely ready for WiFi support, but the Snapmaker API still needs to be integrated properly.

Please note that this project is just a little side project and not something I intend to actively maintain. Code contributions are always appreciated.

Can be built using Visual Studio Community 2022. 

## Usage
```
.\SnapmakerControl.exe --help
SnapmakerControl 1.0.0
Copyright (C) 2022 SnapmakerControl

  -s, --serial    Set COM port of the printer if connecting over serial. Example: "COM4".

  --help          Display this help screen.

  --version       Display version information.
```

```
.\SnapmakerControl.exe -s "COM4"
```
Make sure a gamepad is connected when the application is started. Currently the first detected controller will be used. There is no way (yet) to specify which gamepad in case multiple are connected.
