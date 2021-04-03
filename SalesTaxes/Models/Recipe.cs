using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SalesTaxes.Models
{
    public class Recipe
    {
        public Recipe()
        {

        }

        [DataMember(Name = "Id")]
        public Guid Id { get; set; }

        [DataMember(Name = "Item")]
        public Item Item { get; set; }

        [DataMember(Name = "ListItems")]

        public List<Item> ListItems { get; set; }


    }
}
