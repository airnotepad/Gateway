using Gateway.Helper;
using Gateway.Models.Base;
using Newtonsoft.Json;

namespace Gateway.Services
{
    public class OnlineUsersService
    {
        private List<OnlineUser> online;
        private RWLock locker = new RWLock();

        public OnlineUsersService()
        {
            online = new List<OnlineUser>();
        }

        public IReadOnlyCollection<OnlineUser> GetOnline()
        {
            using (locker.ReadLock())
                return online.CloneToReadOnly();
        }

        public void AddUserOnline(bool isAuth, string username, string action, string ip, string method)
        {
            using (locker.WriteLock())
            {
                if (isAuth)
                {
                    // auth, seach by username
                    if (online.Exists(x => x.Username == username))
                    {
                        //update data
                        int index = online.FindIndex(x => x.Username == username);

                        online[index].LastDate = DateTime.Now;
                        online[index].Action = action;
                        online[index].Ip = ip;
                        online[index].Method = method;
                    }
                    else
                    {
                        //add new
                        online.Add(new OnlineUser()
                        {
                            Username = username,
                            LastDate = Date.Now,
                            Action = action,
                            Ip = ip,
                            Method = method
                        });
                    }
                }
                else
                {
                    //non auth, seach by ip
                    if (online.Exists(x => x.Ip == ip))
                    {
                        //update data
                        int index = online.FindIndex(x => x.Ip == ip);

                        online[index].LastDate = DateTime.Now;
                        online[index].Action = action;
                        online[index].Username = "Неизвестный";
                        online[index].Method = method;
                    }
                    else
                    {
                        //add new
                        online.Add(new OnlineUser()
                        {
                            Username = "Неизвестный",
                            LastDate = Date.Now,
                            Action = action,
                            Ip = ip,
                            Method = method
                        });
                    }
                }

                online.RemoveAll(x => Date.Now.AddDays(-1) >= x.LastDate && x.Username == "Неизвестный");
            }
        }
    }
}
