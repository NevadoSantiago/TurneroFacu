﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Grupo1.AgendaDeTurnos.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }
}
