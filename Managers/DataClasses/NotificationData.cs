using System;
using System.Collections.Generic;
using System.Text;

namespace StayQL.Managers.DataClasses
{
    public class NotificationData
    {
        public enum Type
        {
            Failed = 0,
            Succeed = 1,
            Loading = 2
        }

        public string Message;
        public TimeSpan Duration;
        public Type nType;
        public NotificationData(Type Type, String Message, TimeSpan Duration)
        {
            this.Message = Message;
            this.Duration = Duration;
            this.nType = Type;
        }
    }
}
