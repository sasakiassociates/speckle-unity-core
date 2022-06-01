# speckle-unity-core

An unoffical client connection for using speckle in unity. This project was started from the main [speckle-unity](https://github.com/specklesystems/speckle-unity) project, but has been moved into different repos for more modular package support with unity.    

This package features: 

- `SpeckleStream` - A scriptable object for storing and accessing speckle streams
- `Sender` + `Receiver` - the main operation objects that handle interacting with speckle and unity
- `ScriptableConverter` - A wrapper object for `ISpeckleConverter` that targets more unity specific stuff
- `ComponentConverter<>`- a modular scriptable object for customising how certain speckle objects convert into unity     
 

If you are looking for a more complete unity pacakge take a look at [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)
 
This pacakge can be installed with [OpenUpm](https://github.com/openupm/openupm-cli#installation) 

`openupm add com.speckle.core`


## Roadmap

All roadmap information is available in the main package repo, [speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)  


## Additional 
There are additional packages in active development.

- Supported objects and conversions 
[speckle-unity-objects](https://github.com/sasakiassociates/speckle-unity-objects)

- Core and Objects wrapped together with some additional GUI, probably the more ideal package to install 
[speckle-unity-connector](https://github.com/sasakiassociates/speckle-unity-connector)

