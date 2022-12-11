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
        string @configPath = CPH.GetGlobalVar<string>("paradeConfiguration", true);
        File.WriteAllText(configPath, content);

        // Refresh OBS source
        var obsJson = JsonConvert.SerializeObject(new { inputName = CPH.GetGlobalVar<string>("obsSource", true), propertyName = "refreshnocache" });
        CPH.ObsSendRaw("PressInputPropertiesButton", obsJson, 0);
        CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsScene", true), CPH.GetGlobalVar<string>("obsGroupSource", true), false, 0);

        CPH.LogDebug("================= END Init() =================");
    }

    public void Dispose()
    {
        CPH.LogDebug("================= BEGIN Dispose() =================");

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
        string @configPath = CPH.GetGlobalVar<string>("paradeConfiguration", true);
        File.WriteAllText(configPath, content);

        // Refresh OBS source
        var obsJson = JsonConvert.SerializeObject(new { inputName = CPH.GetGlobalVar<string>("obsSource", true), propertyName = "refreshnocache" });
        CPH.ObsSendRaw("PressInputPropertiesButton", obsJson, 0);
        CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsScene", true), CPH.GetGlobalVar<string>("obsGroupSource", true), false, 0);

        CPH.LogDebug("================= END Dispose() =================");
    }

    public bool Execute()
    {
        CPH.LogDebug("================= BEGIN Execute() =================");
        // Print each argument being provided
        foreach (var arg in args)
        {
            CPH.LogDebug($"LogVars :: {arg.Key} = {arg.Value}");
        }

        CPH.LogDebug($"JSON :: {ObjToString(args)}");

        /*
         * Check to see if a parade is ongoing. If it is, then we must ignore the inputs from chat
         * and exit the Execute function early.
         */
        if(CPH.GetGlobalVar<Boolean>("paradeOngoing", true))
        {
            CPH.LogDebug("Ongoing parade...ignoring and exiting early.");
            return true;
        }


        // Get the comma separated string and put it into a list
        List<string> allowedEmoteList = ((string)args["allowedEmotes"]).Split(',').ToList();
        int i = 0;
        CPH.LogVerbose("Allowed Emotes...");
        allowedEmoteList.ForEach(chatEmote => CPH.LogVerbose($"{i++} :: {chatEmote}"));

        // Pull back the list of emotes used in the message
        List<Emote> emoteList = (List<Emote>)args["emotes"];

        // Reorder emote list by startIndex
        List<Emote> sortedList = emoteList.OrderBy(o => o.StartIndex).ToList();

        // Validate emotes. Emotes are put into a new separate list.
        List <Emote> newEmoteList = new();
        sortedList.ForEach(delegate (Emote emote)
        {
            CPH.LogVerbose($"Evaluating emote {emote.Name}");
            if (allowedEmoteList.Contains(emote.Name))
            {
                CPH.LogVerbose($"Adding emote {emote.Name}...");
                newEmoteList.Add((Emote)emote);
            }
        });

        // Pull back global list of emotes being used.
        CPH.LogVerbose("Pulling back global list...");
        List<Emote>? globalList = CPH.GetGlobalVar<List<Emote>>("emoteList", true);
        CPH.LogVerbose("Returned list");

        // Add new emotes to global list
        globalList.AddRange(newEmoteList);
        CPH.LogDebug("Updated Emote List:");
        globalList.ForEach(emote => CPH.LogDebug($"{emote.Name}"));

        // Set global list (persist it)
        CPH.SetGlobalVar("emoteList", globalList, true);

        // Get elapsed time between emotes being put in
        TimeSpan interval = (DateTime.Now) - (CPH.GetGlobalVar<DateTime>("paradeResetSeconds", true));
        CPH.LogDebug($"Elapsed time :: {interval.TotalSeconds}");
        if (interval.TotalSeconds >= (Int64)args["emoteResetSeconds"])
        {
            // Reset global list + time
            CPH.LogDebug($"Exceeded limit of {(Int64)args["emoteResetSeconds"]} seconds. Resetting timer.");
            CPH.SetGlobalVar("paradeResetSeconds", DateTime.Now, true);
            CPH.SetGlobalVar("emoteList", new List<Emote>(), true);
        }

        // If the list is greater than the maximum wiggly.....release it
        if (globalList.Count >= (Int64)args["emoteThreshold"])
        {
            CPH.LogDebug("Emote threshold reached!!!");

            // The parade is now running!
            CPH.SetGlobalVar("paradeOngoing", true, true);

            // Cut down the number of emotes being sent over...
            Int64 emoteCutoff = (Int64)args["emoteCutoff"];
            List<Emote> truncatedList = globalList.GetRange(0, (int)emoteCutoff);



            // Create the animation parameters for the browser source
            JArray emoteArray = new();
            truncatedList.ForEach(emote => {
                emoteArray.Add(new JObject(
                    new JProperty("name", emote.Name),
                    new JProperty("type", emote.Type),
                    new JProperty("startIndex", emote.StartIndex),
                    new JProperty("endIndex", emote.EndIndex),
                    new JProperty("imageUrl", emote.ImageUrl)
                ));
            });

            JObject jsonObject = new JObject(
                new JProperty("duration", (Int64)args["paradeDuration"] + "ms"),
                new JProperty("delay", (Int64)args["paradeDelay"] + "ms"),
                new JProperty("emotes", emoteArray)
            );

            // Write configuration change to file
            String content = "var extConfig = " + @ObjToString(jsonObject);
            string @configPath = CPH.GetGlobalVar<string>("paradeConfiguration", true);
            File.WriteAllText(configPath, content);

            // Refresh browser source and set visibility to true
            var obsJson = JsonConvert.SerializeObject(new { inputName = CPH.GetGlobalVar<string>("obsSource", true), propertyName = "refreshnocache" });
            CPH.ObsSendRaw("PressInputPropertiesButton", obsJson, 0);
            CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsScene", true), CPH.GetGlobalVar<string>("obsGroupSource", true), true, 0);


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

                CPH.ObsSetSourceVisibility(CPH.GetGlobalVar<string>("obsScene", true), CPH.GetGlobalVar<string>("obsGroupSource", true), false, 0);
                CPH.SetGlobalVar("paradeOngoing", false, true);
            }).Start();


            // Reset global list
            CPH.SetGlobalVar("emoteList", new List<Emote>(), true);
        }

        CPH.LogDebug("================= END Execute() =================");
        return true;
    }

    public string ObjToString(object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
    }
}
/************************************************************************
* COPY AND PASTE ABOVE CLASS INTO STREAMER.BOT
* DO NOT COPY ANYTHING OUTSIDE THE BLOCK COMMENT
************************************************************************/
#pragma warning restore CA1822, CA1050