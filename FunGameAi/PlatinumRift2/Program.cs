using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int playerCount = int.Parse(inputs[0]); // the amount of players (always 2)
        int myId = int.Parse(inputs[1]); // my player ID (0 or 1)
        int zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
        int linkCount = int.Parse(inputs[3]); // the amount of links between all zones
        for (int i = 0; i < zoneCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int zoneId = int.Parse(inputs[0]); // this zone's ID (between 0 and zoneCount-1)
            int platinumSource = int.Parse(inputs[1]); // Because of the fog, will always be 0
        }
        for (int i = 0; i < linkCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int zone1 = int.Parse(inputs[0]);
            int zone2 = int.Parse(inputs[1]);
        }

        // game loop
        while (true)
        {
            int myPlatinum = int.Parse(Console.ReadLine()); // your available Platinum
            for (int i = 0; i < zoneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int zId = int.Parse(inputs[0]); // this zone's ID
                int ownerId = int.Parse(inputs[1]); // the player who owns this zone (-1 otherwise)
                int podsP0 = int.Parse(inputs[2]); // player 0's PODs on this zone
                int podsP1 = int.Parse(inputs[3]); // player 1's PODs on this zone
                int visible = int.Parse(inputs[4]); // 1 if one of your units can see this tile, else 0
                int platinum = int.Parse(inputs[5]); // the amount of Platinum this zone can provide (0 if hidden by fog)
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // first line for movement commands, second line no longer used (see the protocol in the statement for details)
            Console.WriteLine("WAIT");

            Console.WriteLine("WAIT");
        }
    }
}