using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSync
{
    internal class Product
    {
		public long productId { get; set; }
		public string productName { get; set; }
		public string description { get; set; }
		public string brand { get; set; }
		public string weight { get; set; }
		public string category { get; set; }
		public string subCategory { get; set; }
		public int pack { get; set; }
		public string searchIndex { get; set; }
		public string gtinUnit { get; set; }
		public string gtinPack { get; set; }
		public double price { get; set; }
		public int stock { get; set; }
		public string origin { get; set; }
		public bool discount { get; set; }
		public double tax { get; set; }
		public DateTime lastModified { get; set; }



	}
}
