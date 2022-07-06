using Gateway.Helper;

namespace Gateway.Models.Base
{
    public class Invite : ICloneable<Invite>
    {
        public string Text { get; set; }
        public bool IsUsed { get; set; }

        public Invite Clone()
        {
            return new Invite() { Text = this.Text, IsUsed = this.IsUsed };
        }
    }
}
