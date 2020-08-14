using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace constants
{
    public static class network
    {
        public static string serverAddress = "localhost";
        public static int port = 100;
    }
    public static class map
    {
        public static int
            height = 30,
            width = 50,
            foodGrow = 1,
            tps = 10,
            initialInvincibility = 30,
            megaFoodGrow = 5,
            invincibilityDuration = 30,
            slowDuration = 40,
            foodChance = 20,
            megaFoodChance = 120,
            invincibilityChance = 100,
            stoneChance = 50,
            slowChance = 60,
            reverseChance = 150;

        public static List<Color> colors = new List<Color> { Color.Yellow, Color.Red, Color.Green, Color.Blue };
        public static string[] snakeNames = { "Yellow", "Red", "Green", "Blue" };
    }
}
