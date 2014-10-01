using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalAbyss
{
    //The game will trigger other logic when another gamestate is active
    //e.g. the players position does not have to be updated in the menu
    public enum GameState
    {
        ElementChoice, //When the player chooses their element
        Menu, //Show the menu
        Explorering, //For the actual gameplay
        Exit, //Closes the program immidiatly when this gamestate is active
        Dead, //When the player dies
        Credits, //Credits and licenses are shown
    }
    //All the available elements (easier to read)
    public enum Element
    {
        earth,
        water,
        fire,
        air,
    }
    //So 2 enemies can't hit eachother (easier to read)
    public enum Owner
    {
        player,
        enemy,
    }
    //Where to get the comments (easier to read)
    public enum GetComments
    {
        file,
        online,
    }
}