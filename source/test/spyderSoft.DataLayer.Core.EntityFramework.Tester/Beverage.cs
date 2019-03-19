using spyderSoft.DataLayer.Core;
using spyderSoft.DataLayer.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spyderSoft.DataLayer.Core.EntityFramework.Tester
{
    /// <summary>
    /// Class Beverage.
    /// </summary>
    /// <seealso cref="DataItem" />
    /// TODO Edit XML Comment Template for Beverage
    [Table("beverages")]
    public class Beverage : DataItem
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        /// TODO Edit XML Comment Template for Id
        [Key]
        [ColumnName("id")]
        public override long Id { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        /// TODO Edit XML Comment Template for BeverageKey
        [ColumnName("beverage_key")]
        public string BeverageKey { get; set; }

        /// <summary>
        /// Gets or sets the wo key.
        /// </summary>
        /// <value>The wo key.</value>
        [ColumnName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// TODO Edit XML Comment Template for Description
        [ColumnName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the external information.
        /// </summary>
        /// <value>The external information.</value>
        /// TODO Edit XML Comment Template for ExternalInfo
        [ColumnName("external_info_url")]
        public string ExternalInfo { get; set; }

        /// <summary>
        /// Gets or sets the date_created.
        /// </summary>
        /// <value>The date_created.</value>
        [ColumnName("date_created")]
        public DateTimeOffset DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the date_updated.
        /// </summary>
        /// <value>The date_updated.</value>
        [ColumnName("date_updated")]
        public DateTimeOffset DateUpdated { get; set; }
    }
}
