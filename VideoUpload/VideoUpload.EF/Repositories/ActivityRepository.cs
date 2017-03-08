﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoUpload.Core.Entities;
using VideoUpload.Core.Repositories;

namespace VideoUpload.EF.Repositories
{
    internal class ActivityRepository : Repository<Activity>, IActivityRepository
    {
        public ActivityRepository(DbContext context) : base(context)
        {
        }
    }
}
