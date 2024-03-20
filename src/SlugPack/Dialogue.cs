using System.Runtime.InteropServices.WindowsRuntime;

namespace Slugpack
{
    static class Dialogue
    {
        public static readonly string[] techy_meet_moon_firsttime_old = {
            "Hello, little companion. Is this reaching you?",
            "You appear to be lost, you are marked as having come from a facility quite near here.",
            "I am rather busy, but as long as you do not disturb my work you are free to remain.",
            "...",
            "I do wonder where you were intended to be sent to, however.",
            "You seem to lack the data to describe your intended destination, were you perhaps misplaced?",
            "Regardless, I must resume my duties, little one. Someone in my position often has most of their focus occupied."
        };

        public static readonly string[] techy_meet_moon_firsttime = {
            "Hello, little companion. Is this reaching you?",
            "You seem to be lost... You are marked as having come from quite close by.",
            "Strangely, while you seem to lack a designation, you are clearly not without a purpose.",
            "Those abilities you seem to possess are quite powerful, little creature.<LINE>Were I not so busy I would opt to study you further.",
            "However, I have many things that must be done, and I cannot afford to be distracted.",
            "Good luck on your journey, wherever it may lead you. I must return to my work."
        };

        public static readonly string[] techy_does_a_little_trolling_15 = {
            ". . .",
            "I recognize you. You're the one who has been causing quite a commotion<LINE>in my chambers, aren't you?",
            "I will let you know that those neurons you destroyed were quite valuable<LINE>to me, but I cannot say that I lost too much in your... Frolicking...",
            "Regardless, I would prefer if you refrained from doing any more damage<LINE>to my structure than you already have.",
            "Kindly exit through the way you entered, and do not damage any more.",
            "Please."
        };

        public static readonly string[] techy_does_a_little_trolling_30 = {
            ". . .",
            "You have been having quite the time, haven't you?",
            "Did destroying all of those neurons make you feel good? Did you have a fun time doing so?",
            "I hope you are proud of yourself, it will take quite some time to get those back.",
            "Thankfully I have no shortage of neurons at the moment, but I would<LINE>very much appreciate you refrain from damaging any more.",
            "I don't even want to imagine losing more than I already have!"
        };

        public static readonly string[] techy_does_a_little_trolling_45 = {
            "YOU..!",
            "You have been going out of your way to damage my structure, haven't you?",
            "I can feel the gnawing pain in the back of my head from the sheer<LINE>amount you have damaged.",
            "One may not be a big deal, but how many have you destroyed at this point?<LINE>Regardless of the exact amount, it is far too many.",
            "Perhaps I was too hasty in giving you that mark, you clearly cannot handle<LINE>yourself around any of my sensitive equipment.",
            "Leave, and never return."
        };

        public static readonly string[] techy_does_a_little_trolling_60 = {
            "YOU",
            "You stupid, ignorant animal!",
            "What are you trying to do!? Are you trying to kill me?",
            "You don't have even the slightest chance of doing enough damage to me to even<LINE>come close to that!",
            "Get out!",
            "GET OUT!"
        };

        // Above 75 she will simply kill you the moment she spots you in her chamber

        public static readonly string[] techy_staying_too_long = {
            "That is all, little creature. It is time for you to go.",
            "You are beginning to overstay your welcome, little creature.",
            "The exit is at the top of the room, the same way you entered."
        };
    }
}