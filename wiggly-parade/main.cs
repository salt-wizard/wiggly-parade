#pragma warning disable CA1822, CA1050
using CPHNameSpace;                // Mimic CPH for method references
using static CPHNameSpace.CPHArgs; // Mimic arguments for inline CPH


/************************************************************************
* COPY AND PASTE BELOW CLASS INTO STREAMER.BOT 
* DO NOT COPY ANYTHING OUTSIDE THE BLOCK COMMENT
************************************************************************/
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Twitch.Common.Models;
using Newtonsoft.Json.Linq;

public class CPHInline
{

    public void Init()
    {
        CPH.LogDebug("================= BEGIN Init() =================");
        string? @paradeConfiguration = CPH.GetGlobalVar<string>("paradeConfiguration", true); // paradeConfiguration path
        string? obsScene = CPH.GetGlobalVar<string>("obsScene", true);
        string? obsSource = CPH.GetGlobalVar<string>("obsSource", true);
        string? obsGroupSource = CPH.GetGlobalVar<string>("obsGroupSource", true);


        // Create a blank emote list and time of initialize
        CPH.SetGlobalVar("emoteList", new List<Emote>(), true);
        CPH.SetGlobalVar("paradeResetSeconds", DateTime.Now, true);

        // Determines if a parade is running
        CPH.SetGlobalVar("paradeOngoing", false, true);


        JObject jsonObject = new JObject(
            new JProperty("duration", "0s"),
            new JProperty("delay", "1s"),
            new JProperty("emotes", new JArray())
        );

        // Write configuration change to file
        String content = "var extConfig = " + @ObjToString(jsonObject);
        File.WriteAllText(@paradeConfiguration, content);

        // Refresh OBS source
        var obsJson = JsonConvert.SerializeObject(new { inputName = obsSource, propertyName = "refreshnocache" });
        CPH.ObsSendRaw("PressInputPropertiesButton", obsJson, 0);
        CPH.ObsSetSourceVisibility(obsScene, obsGroupSource, false, 0);

        CPH.LogDebug("================= END Init() =================");
    }

    public bool Execute()
    {
        CPH.LogDebug("================= BEGIN Execute() =================");
        // Print each argument being provided
        //CPH.LogDebug($"JSON :: {ObjToString(args)}");

        /*
         * Check to see if a parade is ongoing. If it is, then we must ignore the inputs from chat
         * and exit the Execute function early.
         */

        string? @paradeConfiguration = CPH.GetGlobalVar<string>("paradeConfiguration", true); // paradeConfiguration path
        string? @soundFile = CPH.GetGlobalVar<string>("soundFile", true);

        CPH.LogDebug($"Listing important variables...");

        Boolean? paradeOngoing = CPH.GetGlobalVar<Boolean>("paradeOngoing", true); // Is parade ongoing?
        CPH.LogDebug($"paradeOngoing :: {paradeOngoing}");

        List<Emote>? emoteList = (List<Emote>)args["emotes"]; // Emotes used in the message
        CPH.LogDebug($"message emotes :: {ObjToString(emoteList)}");

        List<Emote>? currentEmoteList = CPH.GetGlobalVar<List<Emote>>("emoteList", true); // Current parade list
        CPH.LogDebug($"saved emotes :: {ObjToString(currentEmoteList)}");

        Int64 emoteResetSeconds = (Int64)args["emoteResetSeconds"];
        CPH.LogDebug($"emoteResetSeconds :: {emoteResetSeconds}");

        Int64 emoteThreshold = CPH.GetGlobalVar<Int64>("emoteThreshold", true);
        CPH.LogDebug($"emoteThreshold :: {emoteThreshold}");

        DateTime paradeResetSeconds = CPH.GetGlobalVar<DateTime>("paradeResetSeconds", true);
        CPH.LogDebug($"paradeResetSeconds :: {ObjToString(paradeResetSeconds)}");

        string? obsScene = CPH.GetGlobalVar<string>("obsScene", true);
        CPH.LogDebug($"obsScene :: {ObjToString(obsScene)}");

        string? obsSource = CPH.GetGlobalVar<string>("obsSource", true);
        CPH.LogDebug($"obsSource :: {ObjToString(obsSource)}");

        string? obsGroupSource = CPH.GetGlobalVar<string>("obsGroupSource", true);
        CPH.LogDebug($"obsGroupSource :: {ObjToString(obsGroupSource)}");

        Int64 emoteCutoff = (Int64)args["emoteCutoff"];
        CPH.LogDebug($"emoteCutoff :: {emoteCutoff}");

        Int64 paradeDuration = (Int64)args["paradeDuration"];
        CPH.LogDebug($"paradeDuration :: {paradeDuration}");

        Int64 paradeDelay = (Int64)args["paradeDelay"];
        CPH.LogDebug($"paradeDelay :: {paradeDelay}");

        Int64 paradeResetDelay = CPH.GetGlobalVar<Int64>("paradeResetDelay", true);
        CPH.LogDebug($"paradeResetDelay :: {paradeResetDelay}");

        Int64 paradeVolume = CPH.GetGlobalVar<Int64>("paradeVolume", true);
        CPH.LogDebug($"paradeVolume :: {paradeVolume}");

        if ((bool)paradeOngoing)
        {
            CPH.LogDebug("Ongoing parade...ignoring and exiting early.");
            CPH.LogDebug("================= END Execute() =================");
            return true;
        }

        // Get the comma separated string and put it into a list
        List<string> allowedEmoteList = ((string)args["allowedEmotes"]).Split(',').ToList();
        int i = 0;
        CPH.LogVerbose("Allowed Emotes...");
        allowedEmoteList.ForEach(chatEmote => CPH.LogVerbose($"{i++} :: {chatEmote}"));

        // Reorder emote list by startIndex
        List<Emote> sortedList = emoteList.OrderBy(o => o.StartIndex).ToList();
        CPH.LogDebug($"Emote list from chat message resorted to the following :: {ObjToString(sortedList)}");

        // Validate emotes. Emotes are put into a new separate list.
        List<Emote> newEmoteList = new();
        sortedList.ForEach(delegate (Emote emote)
        {
            CPH.LogVerbose($"Evaluating emote {emote.Name}");
            if (allowedEmoteList.Contains(emote.Name))
            {
                CPH.LogVerbose($"Adding emote {emote.Name}...");
                newEmoteList.Add((Emote)emote);
            }
        });


        // Add new emotes to global list
        currentEmoteList.AddRange(newEmoteList);
        CPH.LogDebug("Updated Emote List:");
        currentEmoteList.ForEach(emote => CPH.LogDebug($"{emote.Name}"));

        // Set global list (persist it)
        CPH.SetGlobalVar("emoteList", currentEmoteList, true);

        // Get elapsed time between emotes being put in
        TimeSpan interval = (DateTime.Now) - (paradeResetSeconds);
        CPH.LogDebug($"Elapsed time :: {interval.TotalSeconds}");
        if (interval.TotalSeconds >= emoteResetSeconds)
        {
            // Reset global list + time
            CPH.LogDebug($"Exceeded limit of {emoteResetSeconds} seconds. Resetting timer.");
            CPH.SetGlobalVar("paradeResetSeconds", DateTime.Now, true);
            CPH.SetGlobalVar("emoteList", new List<Emote>(), true);
        }

        CPH.LogDebug($"Emote list before threshold check :: {currentEmoteList}");

        // If the list is greater than the maximum wiggly.....release it
        if (currentEmoteList.Count >= emoteThreshold)
        {
            CPH.LogDebug("Emote threshold reached!!!");
            CPH.LogDebug($"Emote list after crossing threshold :: {currentEmoteList}");

            // The parade is now running!
            CPH.LogDebug("paradeOngoing set to TRUE!");
            CPH.SetGlobalVar("paradeOngoing", true, true);

            // Cut down the number of emotes being sent over...
            //List<Emote> truncatedList = currentEmoteList.GetRange(0, (int)emoteCutoff);

            // Create the animation parameters for the browser source
            JArray emoteArray = new();
            currentEmoteList.ForEach(emote => {
                emoteArray.Add(new JObject(
                    new JProperty("name", emote.Name),
                    new JProperty("type", emote.Type),
                    new JProperty("startIndex", emote.StartIndex),
                    new JProperty("endIndex", emote.EndIndex),
                    new JProperty("imageUrl", emote.ImageUrl)
                ));
            });

            JObject jsonObject = new JObject(
                new JProperty("duration", paradeDuration + "ms"),
                new JProperty("delay", paradeDelay + "ms"),
                new JProperty("emotes", emoteArray)
            );

            CPH.LogDebug($"JSON object :: {jsonObject}");

            // Write configuration change to file
            String content = "var extConfig = " + @ObjToString(jsonObject);
            File.WriteAllText(@paradeConfiguration, content);
            CPH.LogDebug("Updated ext-config.js file!!!");

            // Refresh browser source and set visibility to true
            CPH.LogDebug("Starting parade in OBS!");
            var obsJson = JsonConvert.SerializeObject(new { inputName = obsSource, propertyName = "refreshnocache" });
            CPH.ObsSendRaw("PressInputPropertiesButton", obsJson, 0);
            CPH.ObsSetSourceVisibility(obsScene, obsGroupSource, true, 0);
            CPH.LogDebug("Parade started!!!");

            CPH.PlaySound(@soundFile, (long)paradeVolume, false);


            /*
             * Once the parade kicks off, a new thread is kicked off to handle resetting the parade back to an off state.
             * The duration the thread sleeps for before resetting the pardade is based off the delay + duration of the parade 
             * plus additional time the user specifies.
             */
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Int64 duration = (Int64)args["paradeDuration"];
                Int64 delay = (Int64)args["paradeDelay"];
                Int64 resetDelay = CPH.GetGlobalVar<Int64>("paradeResetDelay", true);
                System.Threading.Thread.Sleep((int)(duration + (int)delay + (int)resetDelay));
                CPH.LogDebug($"Sleep duration of {(int)(duration + (int)delay + (int)resetDelay)}s is over. Resetting parade.");

                CPH.ObsSetSourceVisibility(obsScene, obsGroupSource, false, 0);
                CPH.SetGlobalVar("paradeOngoing", false, true);
                CPH.LogDebug("Parade stopped.");
            }).Start();


            // Reset global list
            CPH.SetGlobalVar("emoteList", new List<Emote>(), true);
        }

        CPH.LogDebug("================= END Execute() =================");
        return true;
    }

    public string ObjToString(object obj)
    {
        if (obj == null)
        {
            return "null";
        }
        return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
    }
}
/************************************************************************
* COPY AND PASTE ABOVE CLASS INTO STREAMER.BOT
* DO NOT COPY ANYTHING OUTSIDE THE BLOCK COMMENT
************************************************************************/
#pragma warning restore CA1822, CA1050