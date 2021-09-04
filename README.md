
## Jump King Coop Mod

### How it works

It is fairly straightforward, you just start your game and others can join to you. 
Every player is in their "own" instance, meaning that the this mod **will not** affect your save states. 
Other players are only visuals and they won't interact with your instance!

**Use it at your own risk!**

### Installation
1. Make a backup copy of **MonoGame.Framework.dll**
2. Download the latest release archive (.zip) from https://github.com/sajmonks/JumpKingCoop/releases
3. Extract the content of the archive to your game directory (usually at C:\Program Files (x86)\Steam\steamapps\common\Jump King)
4. If you are the Server, make sure your IP address and Port are accessible through the internet. If not follow NAT/Port Forwarding
tutorials on the internet. Also, check your firewall configuration
5. Follow the configuration instructions below
6. Start the game and load your save/start a new game

### Additional key bindings
**Key S** - Press to switch between spectated players

**Key N** - To toggle player names

### Configuration
Browse to the Jump King\Content\mods\JumpKingCoop.xml
 - Enabled - Specifies if the mod is enabled or not 
 - Server - True = Server, False = Client 
 - IpAddress - For the client, it must be an actual IPv4 address accessible through the internet
 - Port - Server port
 - Nickname - Name of the player in the game
 - SessionPassword - Must be the same on the client and the server to be able to connect
 - MaxPlayer - (**Optional for the server**) Option for the server, to limit the max amount of players

Example configuration server:
```
<NetworkConfig>
  <Enabled>true</Enabled>
  <Server>true</Server>
  <IpAddress>127.0.0.1</IpAddress>
  <Port>6656</Port>
  <Nickname>sajmonks</Nickname>
  <SessionPassword>123</SessionPassword>
  <MaxPlayers>16</MaxPlayers>
</NetworkConfig>
```
Example configuration client
```
<NetworkConfig>
  <Enabled>true</Enabled>
  <Server>false</Server>
  <IpAddress>85.101.101.50</IpAddress>
  <Port>6656</Port>
  <Nickname>sajmonks's Friend</Nickname>
  <SessionPassword>123</SessionPassword>
  <MaxPlayers>16</MaxPlayers>
</NetworkConfig>
```
### Development - Dependencies
1. Harmony (https://github.com/pardeike/Harmony)

### Development - Setting up environment
1. GameStartup1 and GameStartup2 are placeholders to the startup projects which I use to debug the server/client.
2. Edit JumpKingCoop.csproj with a text editor and modify the destination directory of the game (References and Post-Scripts).
3. Code!

### Development - Mod hooking
1. Edit the code of any library loaded with the game.
2. Place the following code
```
try
{
	MethodInfo method = Assembly.Load(File.ReadAllBytes("Content/mods/JumpKingCoop.dll")).GetType("JumpKingCoop.JumpKingCoopEntry").GetMethod("Init", BindingFlags.Static | BindingFlags.Public);
	if (method != null)
	{
		method.Invoke(null, null);
	}
}
catch
{
	File.WriteAllText("modloader.txt", "Failed to inject JumpKingCoop\n");
}
```
3. Suggestion MonoGame.Framework.dll, class Game, execute at the end of function Initialize.

### TODOS
1. Add the integration with Steamworks API so that the player can join the other player via steam overlay.
2. Further net code optimizations.
