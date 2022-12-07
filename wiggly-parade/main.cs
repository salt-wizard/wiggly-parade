using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Twitch.Common.Models;
using Streamer.bot.Common.Events;

namespace WigglyParade
{
#pragma warning disable CA1822
    /************************************************************************
     * COPY AND PASTE BELOW CLASS INTO STREAMER.BOT 
     ************************************************************************/
    public class CPHInline {
        public void Init()
        {
            // place your init code here
        }

        public void Dispose()
        {
            // place your dispose code here
        }

        public bool Execute()
        {
            //foreach (var arg in args)
            //{
            //    CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
            //    if ((arg.Key).Equals("emotes"))
            //    {
            //        List<Emote> emoteList = (List<Emote>)arg.Value;
            //        List<Emote> newEmoteList = new List<Emote>();

            //        emoteList.ForEach(delegate (Emote emote)
            //        {

            //            newEmoteList.Add(emote);
            //        });

            //        CPH.SetGlobalVar("wigglyList", newEmoteList, true);
            //    }
            //}

            return true;
        }

        public string ObjToString(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
        }
    }

    /************************************************************************
     * END OF INLINE SOLUTION
     ************************************************************************/
#pragma warning restore CA1822
}