using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PickemApp.Models;

namespace PickemApp
{
    public static class Auth
    {
        private const string UserKey = "PickemApp.Auth.UserKey";

        public static Player User
        {
            get
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    return null;
                }

                var user = HttpContext.Current.Items[UserKey] as Player;

                if (user == null)
                {
                    using (PickemDBContext db = new PickemDBContext())
                    {
                        user = db.Players.FirstOrDefault(p => p.Username == HttpContext.Current.User.Identity.Name);
                    }
                    if (user == null)
                    {
                        return null;
                    }

                    HttpContext.Current.Items.Add(UserKey, user);
                }

                return user;
            }
        }
    }
}