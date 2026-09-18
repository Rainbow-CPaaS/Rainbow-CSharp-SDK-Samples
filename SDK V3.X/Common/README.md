![Rainbow](../../logo_rainbow.png)
 
# Rainbow CSharp SDK examples - v3.x - Rainbow.Example.Common

This project is used as common library to all examples for SDK V3.X

It permits to centralise:
- same packages
	- package reference to **Rainbow.CSharp.SDK**
	- package reference to **NLog.Extensions.Logging** to use Nlog as backend log provider
	- or (optionally) several package references to  **Serilog** (see file Common.csproj for details )
- same objects:
	- Account
	- ConsoleAbstraction
	- Credentials
	- ExeSettings
	- LogConfigurator
	- ServerConfig
	- UserConfig

