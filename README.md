# Dota Hack Launcher on WPF (MVVM) for demo purposes
### **[IMPORTANT!]**
#### **1. A huge part of code has been removed (as well as git history) to make the project usable for demo purposes**
#### **2. To have a debug working properly disable comment Resource.Embedder package and enable it back before Release build**

### Dependecies:
* **Stylet** - tiny framework to have a good MVVM implementation [Sources](https://github.com/canton7)
* **Costura.Fody** - for embedding all dependencies into a single .exe file
* **Goji** - dynamic UI translation
* **MaterialDesignThemes** - the name describes itself
* **Newtonsoft.Json** - for JSON stuff
* **PropertyChanged.Fody** - to have auto notfications on [PropertyChanged](https://github.com/canton7/Stylet/wiki/PropertyChangedBase)
* **Resource.Embedder** - to embedd all Resource files (translation strings in my case)
* **Serilog.Exceptions, Serilog.Sinks.Async, Serilog.Sinks.File** - for logging stuff
