﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Trainee.Business.DAL.Entities
{
    internal class ProductRating
    {
        public int ProductId { get; set; }
        public decimal? AverageRating { get; set; }
    }
}