﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Trainee.Catalogue.DAL.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public string PicAddress { get; set; }
        //Referenced properties
        public Category Parent { get; set; }
        public List<Category> Children { get; set; }
        public List<ProductBase> Products { get; set; }
    }
}
