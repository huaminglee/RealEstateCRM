﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OUDAL
{
    public class Project
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        [DisplayName("编码")]
        public string Code { get; set; }
    }
}
