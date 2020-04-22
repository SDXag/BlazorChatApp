using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorChatApp.Shared.Models
{
    public class Message
    {
        public string SourceUserName { get; set; }
        public string TargetUserName { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
    }
}
