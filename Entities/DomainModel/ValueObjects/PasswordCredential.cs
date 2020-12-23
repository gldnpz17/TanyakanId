﻿using DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ValueObjects
{
    public class PasswordCredential
    {
        public virtual Guid UserId { get; set; }
        public virtual User User { get; set; }
        public virtual string HashedPassword { get; set; }
        public virtual string PasswordSalt { get; set; }
        public virtual PasswordResetToken PasswordResetToken { get; set; }
    }
}
