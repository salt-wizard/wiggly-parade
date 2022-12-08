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

public class CPHInline
{

    public void Init()
    {
        CPH.LogDebug("================= BEGIN Init() =================");
        
        // Create a blank emote list and time of initialize
        CPH.SetGlobalVar("wigglyList", new List<Emote>(), true);
        CPH.SetGlobalVar("paradeResetSeconds", DateTime.Now, true);

        CPH.LogDebug("================= END Init() =================");
    }

    public void Dispose()
    {
        // place your dispose code here
    }

    public bool Execute()
    {
        CPH.LogDebug("================= BEGIN Execute() =================");
        /*
        // Print each argument being provided
        foreach (var arg in args)
        {
            CPH.LogVerbose($"LogVars :: {arg.Key} = {arg.Value}");
        }
        */

        // Get the comma separated string and put it into a list
        List<string> wigglyList = ((string)args["allowedWigglies"]).Split(',').ToList();
        int i = 0;
        CPH.LogDebug("Allowed Wigglies...");
        wigglyList.ForEach(wiggly => CPH.LogDebug($"{i++} :: {wiggly}"));

        // Pull back the list of emotes used in the message
        List<Emote> emoteList = (List<Emote>)args["emotes"];

        // Go through each emote and validate it is a wiggly
        // Wiggly emotes are put into another list
        List<Emote> newEmoteList = new();
        emoteList.ForEach(delegate (Emote emote)
        {
            CPH.LogVerbose($"Evaluating emote {emote.Name}");
            if (wigglyList.Contains(emote.Name))
            {
                CPH.LogVerbose($"Adding emote {emote.Name}...");
                newEmoteList.Add((Emote)emote);
            }
        });

        // Pull back global list
        CPH.LogDebug("Pulling back global list...");
        List<Emote>? globalList = CPH.GetGlobalVar<List<Emote>>("wigglyList", true);
        CPH.LogDebug("Returned list");

        // Add emotes to global list
        globalList.AddRange(newEmoteList);
        CPH.LogDebug("Updated Emote List:");
        globalList.ForEach(emote => CPH.LogDebug($"{emote.Name}"));
        
        // Set global list
        CPH.SetGlobalVar("wigglyList", globalList, true);

        // Get elapsed time between emotes being put in
        TimeSpan interval = (DateTime.Now) - (CPH.GetGlobalVar<DateTime>("paradeResetSeconds", true));
        CPH.LogDebug($"Elapsed time :: {interval.TotalSeconds}");
        if (interval.TotalSeconds >= (Int64)args["paradeResetSeconds"])
        {
            // Reset global list + time
            CPH.LogDebug($"Exceeded limit of {(Int64)args["paradeResetSeconds"]} seconds. Resetting timer.");
            CPH.SetGlobalVar("paradeResetSeconds", DateTime.Now, true);
            CPH.SetGlobalVar("wigglyList", new List<Emote>(), true);
        }

        // If the list is greater than the maximum wiggly.....release it
        if (globalList.Count >= (Int64)args["wigglyThreshold"])
        {
            CPH.LogDebug("Wiggly threshold reached!!!");

            /**
             * 
             * TODO - INSERT WIGGLY PARADE CODE HERE!!!
             * 
             **/


            // Reset global list 
            CPH.SetGlobalVar("wigglyList", new List<Emote>(), true);
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