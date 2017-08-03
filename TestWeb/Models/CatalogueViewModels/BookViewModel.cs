﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trainee.Business.DAL.Entities;
using Trainee.Catalogue.DAL.Entities;

namespace Eshop2.Models.CatalogueViewModels
{
    public class BookViewModel
    {
        public string Name { get; set; }

        public string CategoryName { get; set; } //or Category

        public Author Author { get; set; }
        public string ProductFormat { get; set; }
        public int Rating { get; set; }
        public string Annotation { get; set; }
        public string ProductText { get; set; }
        public string PicAddress { get; set; }

        public string State { get; set; }
        public decimal Price { get; set; }

        public string Language { get; set; }
        public int PageCount { get; set; }
        public int Year { get; set; }

        public string Publisher { get; set; }
        public string ISBN { get; set; }
        public string EAN { get; set; }

        public List<Review> Reviews { get; set; }

    }
}
