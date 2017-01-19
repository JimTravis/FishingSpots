using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingSpots
{
    static class StaticUtils
    {
        static string [] _sayings = {
                              "Gone fishin'",
                              "Never a bad day to fish",
                              "It was a lovely little fish" + Environment.NewLine + "and it went wherever I did go",
                              "Teach a man to fish" + Environment.NewLine + "and he eats for a lifetime",
                              "Fishing is the sport" + Environment.NewLine + "of drowning worms",
                              "A bad day of fishing is" + Environment.NewLine + "better than a good day of work",
                              "The gods do not deduct" + Environment.NewLine + "from man's alloted span the" + Environment.NewLine + "hours spent in fishing",
                              "All men are equal before fish",
                              "Don't tell fish stories" + Environment.NewLine + "where the people know you",
                              "Big fish swim at the bottom",
                              "If fishing is interfering" + Environment.NewLine + "with your business" + Environment.NewLine + "give up your business",
                              "Here fishy, fishy, fishy", 
                              "Oh where did my fishy go?",
                              "Go find fish",
                              "The fish are trembling",
                              "The two best times to fish" + Environment.NewLine + "is when it's rainin'" + Environment.NewLine + "and when it ain't",
                              "Calling fishing a hobby is" + Environment.NewLine + "like calling brain surgery a job",
                              "It takes a thinker to catch" + Environment.NewLine + "the big one, hook, line and sinker",
                              "Nothing makes a fish bigger" + Environment.NewLine + "than almost being caught",
                              "Teach a man to fish and he will" + Environment.NewLine + "sit in a boat and drink beer all day",
                              "The fishing is always better" + Environment.NewLine + "on the other side of the lake",
                              "Ain't you due for a fishin' trip?"
                              };

        public static string GetRandomSaying()
        {
            return _sayings[new Random().Next(_sayings.Length-1)];
        }
    }
}
