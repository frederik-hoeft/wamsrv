using System;
using System.Collections.Generic;
using System.Text;

namespace wamsrv
{
    public class Event
    {
        public readonly EventInfo EventInfo;
        public readonly string UserId;
        public readonly int Id;
        public Event(EventInfo eventInfo, string userid, int id)
        {
            EventInfo = eventInfo;
            UserId = userid;
            Id = id;
        }
    }
}
