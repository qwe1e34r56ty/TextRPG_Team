using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TextRPGTemplate.Context
{
    [Serializable]

    public  class QuestData
    {

        public string key { get; set; }
        public string questitem {  get; set; }
        public int questfigure {  get; set; }
        public string npc { get; set; }
        public string text { get; set; }
        public bool clearquest { get; set; }
        public int dropitemcount { get; set; }
        public bool rewardReceived { get; set; }

        public QuestData(string key, string questitem, int questfigure, string npc, string text, bool clearquest, int dropitemcount, bool rewardReceived)
        {
            this.key = key;
            this.questitem = questitem;
            this.questfigure = questfigure;
            this.npc = npc;
            this.text = text;
            this.clearquest = clearquest;
            this.dropitemcount = dropitemcount;
            this.rewardReceived = rewardReceived;
        }

        public QuestData()
        {

        }

    }
}
