using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRTest.Models
{
    public class ExquisiteCorpse
    {
        public string RoomId { get; set; }
        public List<string> Participants { get; set; }
        public bool HasStarted { get; set; }

        public ExquisiteCorpse()
        {
            Participants = new List<string>();
            HasStarted = false;
        }
    }
}