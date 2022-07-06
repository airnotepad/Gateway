using Gateway.Helper;

namespace Gateway.Models.Base
{
    public class OnlineUser : ICloneable<OnlineUser>
    {
        public string Username { get; set; }
        public DateTime LastDate { get; set; }
        public string Action { get; set; }
        public string Ip { get; set; }
        public string Method { get; set; }

        public OnlineUser Clone()
        {
            return new OnlineUser()
            {
                Username = this.Username,
                LastDate = this.LastDate,
                Action = this.Action,
                Ip = this.Ip,
                Method = this.Method
            };
        }
    }
}
