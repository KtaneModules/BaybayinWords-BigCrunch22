using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BaybayinWords : MonoBehaviour
{
    public KMBombModule Module;
    public KMAudio Audio;
    
    public TextMesh Display;
    public KMSelectable[] Buttons;
    string[] wordsEnglish = new string[] { "Dove", "House", "Food", "Sleep", "Bridge", "Wood", "Sugar", "Rich", "Shoe", "Gold", "Brown", "Treasure", "Weak", "Guava", "Happy", "Thanks", "Captain", "Clap", "Eyes", "Dive", "Plant", "Medicine", "Island", "Hide", "Swim", "Fly", "Lightning", "Dinner", "Breakfast", "Lunch", "Clothing", "Copper", "Silver", "Dead", "Deaf", "Blind", "Lame", "Mute", "Stain", "Plate", "Knife", "Necklace", "Ring", "Stop", "Walk" };
    string[] wordsBaybayin = new string[] { "ᜃᜎᜉᜆᜒ", "ᜊᜑᜌ᜔", "ᜉᜄ᜔ᜃᜁᜈ᜔", "ᜆᜓᜎoᜄ᜔", "ᜆᜓᜎᜌ᜔", "ᜃᜑoᜌ᜔", "ᜀᜐᜓᜃᜎ᜔", "ᜋᜌᜋᜈ᜔", "ᜐᜉᜆoᜐ᜔", "ᜄᜒᜈ᜔ᜆo", "ᜃᜌᜓᜋᜅ᜔ᜄᜒ", "ᜃᜌᜋᜈᜈ᜔", "ᜋᜑᜒᜈ", "ᜊᜌᜊᜐ᜔", "ᜋᜐᜌ", "ᜐᜎᜋᜆ᜔", "ᜃᜉᜒᜆᜈ᜔", "ᜉᜎᜃ᜔ᜉᜃ᜔", "ᜋᜆ", "ᜐᜒᜐᜒᜇ᜔", "ᜑᜎᜋᜈ᜔", "ᜄᜋoᜆ᜔", "ᜁᜐ᜔ᜎ", "ᜆᜄo", "ᜎᜅoᜌ᜔", "ᜎᜒᜉᜇ᜔", "ᜃᜒᜇ᜔ᜎᜆ᜔", "ᜑᜉᜓᜈᜈ᜔", "ᜀᜄᜑᜈ᜔", "ᜆᜅ᜔ᜑᜎᜒᜀᜈ᜔", "ᜇᜋᜒᜆ᜔", "ᜆᜈ᜔ᜐo", "ᜉᜒᜎᜃ᜔", "ᜉᜆᜌ᜔", "ᜊᜒᜈ᜔ᜄᜒ", "ᜊᜓᜎᜄ᜔", "ᜉᜒᜎᜌ᜔", "ᜉᜒᜉᜒ", "ᜋᜈ᜔ᜆ᜔ᜐ", "ᜉ᜔ᜎᜆo", "ᜃᜓᜆ᜔ᜐᜒᜎ᜔ᜌo", "ᜃᜓᜏᜒᜈ᜔ᜆᜐ᜔", "ᜐᜒᜅ᜔ᜐᜒᜅ᜔", "ᜑᜒᜈ᜔ᜆo", "ᜎᜃᜇ᜔" };
    List<string> ModuleWords;
    int correctWordIndex;
    
    // Logging
    int ModuleId;
    static int ModuleIdCounter = 1;
    bool solved;
    
    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        for (int i = 0; i < 6; i++)
        {
            Buttons[i].OnInteract += PressButton(i);
        }
        correctWordIndex = UnityEngine.Random.Range(0, 6);
        ActivateModule();
    }
    
    void ActivateModule()
    {
        ModuleWords = wordsEnglish.ToList().Shuffle().Take(6).ToList();
        for (int i = 0; i < 6; i++)
        {
            Buttons[i].GetComponentInChildren<TextMesh>().text = ModuleWords[i];
        }
        
        Display.text = wordsBaybayin[Array.IndexOf(wordsEnglish, ModuleWords[correctWordIndex])];
        Debug.LogFormat("[Baybayin Words #{0}] The word displayed on the screen is {1}.", ModuleId, ModuleWords[correctWordIndex].ToLower());
    }
    
    KMSelectable.OnInteractHandler PressButton (int index)
    {
        return delegate
        {
            Buttons[index].AddInteractionPunch();
            if (!solved)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[index].transform);
                Debug.LogFormat("[Baybayin Words #{0}] You pressed the button labeled {1}.", ModuleId, ModuleWords[index].ToLower());
                if (index == correctWordIndex)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    Debug.LogFormat("[Baybayin Words #{0}] That was correct. Module solved.", ModuleId);
                    Display.text = "";
                    string[] Speech = {"Baybayin", "Words", "Module", "Has", "Been", "Solved"};
                    for (int i = 0; i < 6; i++)
                    {
                        Buttons[i].GetComponentInChildren<TextMesh>().text = Speech[i];
                    }
                    Module.HandlePass();
                    solved = true;
                }
                else
                {
                    Module.HandleStrike();
                    Debug.LogFormat("[Baybayin Words #{0}] That was incorrect. Strike!", ModuleId);
                    correctWordIndex = UnityEngine.Random.Range(0, 6);
                    ActivateModule();
                }
            }
            return false;
        };
    }
    
    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press a button on the module (in reading order), use the command !{0} press [1-6]";
    #pragma warning restore 414
    
    string[] ValidNumbers = {"1", "2", "3", "4", "5", "6"};
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        
        if (parameters.Length != 2)
        {
            yield return "sendtochaterror Invalid parameter length. The command was not processed.";
            yield break;
        }
            
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!parameters[1].EqualsAny(ValidNumbers))
            {
                yield return "sendtochaterror Invalid button position sent. The command was not processed.";
                yield break;
            }
            Buttons[Int32.Parse(parameters[1]) - 1].OnInteract();
        }
    }
    
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Buttons[correctWordIndex].OnInteract();           
    }
}
