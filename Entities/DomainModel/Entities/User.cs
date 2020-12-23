﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel.ValueObjects;

namespace DomainModel.Entities
{
    public class User
    {
        public virtual Guid Id { get; set; }
        public virtual string Username { get; set; }
        public virtual IList<UserPrivilege> Privileges { get; set; } = new List<UserPrivilege>();
        public virtual Image ProfilePicture { get; set; }
        public virtual UserEmail Email { get; set; }
        public virtual DateTime BanLiftedDate { get; set; }
        public virtual PasswordCredential PasswordCredential { get; set; }
        public virtual IList<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
        public virtual IList<Article> Articles { get; set; } = new List<Article>();
    }
}
