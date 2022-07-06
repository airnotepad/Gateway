using Gateway.Helper;
using Gateway.Models.Base;

namespace Gateway.Services
{
    public class InviteService
    {
        private List<Invite> invites = new List<Invite>();
        private RWLock locker = new RWLock();

        public InviteService()
        {

        }

        public List<Invite> GetAll()
        {
            using (locker.ReadLock())
            {
                return invites.CloneList();
            }
        }

        public bool AddNew(string text)
        {
            using (locker.WriteLock())
            {
                if (invites.Any(x => x.Text == text)) throw new Exception("Invite with such text already exists");
                invites.Add(new Invite() { Text = text, IsUsed = false });
                return true;
            }
        }

        public bool Delete(string text)
        {
            using (locker.WriteLock())
            {
                invites.RemoveAt(invites.FindIndex(x => x.Text == text));
                return true;
            }
        }

        public bool IsOk(string text)
        {
            using (locker.WriteLock())
            {
                if (invites.Any(x => x.Text == text && !x.IsUsed))
                {
                    invites[invites.FindIndex(x => x.Text == text && !x.IsUsed)].IsUsed = true;
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
