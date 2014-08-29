﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTasks.Core.Models
{
    [DataContract()]
    public class BackupData
    {
        [DataMember(Name = "Version")]
        public string Version { get; set; }

        [DataMember(Name = "Created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "Settings", Order = 2)]
        public Settings Settings { get; set; }

        [DataMember(Name = "Tasks", Order = 3)]
        public TaskCollection Tasks { get; set; }
    }
}
