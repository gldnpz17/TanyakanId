﻿using DomainModel.Services;
using System;

namespace DomainModel.Entities
{
    public class AuthToken
    {
        public virtual User User { get; set; }
        public virtual string Token { get; set; }
        public virtual DateTime ExpireDate { get; set; }
        public virtual string UserAgent { get; set; }
        public virtual string IPAddress { get; set; }
        public virtual bool IsRemembered { get; set; }

        public virtual void Extend(IDateTimeService _dateTimeService)
        {
            if (IsRemembered)
            {
                ExpireDate = _dateTimeService.GetCurrentDateTime().AddDays(30);
            }
        }
    }
}
