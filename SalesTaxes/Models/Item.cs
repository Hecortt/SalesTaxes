using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SalesTaxes.Models
{
    public class Item
    {
        public Item()
        {

        }

        [DataMember(Name = "Id")]
        public Guid Id { get; set; }

        [DataMember(Name = "Qty")]
        public int Qty { get; set; }

        [DataMember(Name = "Name")]
        public string Name { get; set; }

        [DataMember(Name = "Price")]
        public double Price { get; set; }

        [DataMember(Name = "Imported")]
        public bool Imported { get; set; }

        [DataMember(Name = "TaxImported")]
        public double TaxImported { get; set; }

        [DataMember(Name = "TaxGeneral")]
        public double TaxGeneral { get; set; }

        [DataMember(Name = "CompletedName")]
        public string CompletedName { get; set; }

        /// <summary>
        /// TRUE: Form Item
        /// FALSE: Single Line Item
        /// </summary>
        [DataMember(Name = "TypeInput")]
        public bool TypeInput { get; set; }

    }
}
