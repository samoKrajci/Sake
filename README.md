# Sake
*A project for programming class (year 1, term 2) at Charles University, Prague.*


Sake is an arcade-like multiplayer game, that uses monogame C# library. Last snake slithering wins.

## How to run the game

In order to run the game, server must be also running. 

### Server

To run server, you have to build and run *server* project. There are 3 commands in the *server* program:

* **lobby**: puts server in a lobby state, enabling users to connect. Max 4 players are supported.
* **game**: starts the game
* **quit**: terminates the program

### Game

First you have to configure IP address of the server in the *Settings.settings* file in the *Sake* project (use `"localhost"` for localhost)
To run the game, build and run *Sake* project. Then wait for the server to star the game. 
**Do not terminate the program until the game ends, the server will crash** (a feature, not a bug ( ° ͜ʖ °) ).

Use right and left arrow to turn.

*It is not recommended to publish the game yet, any sort of configuration is missing, so in order to change server IP address or game paramateres, you have to change the code*
