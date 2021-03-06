using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("group")]
    public partial class Group
    {
        public Group()
        {
            Log = new HashSet<Log>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("id_product")]
        public int IdProduct { get; set; }
        [Column("id_code")]
        public int IdCode { get; set; }
        [Column("id_link")]
        public int IdLink { get; set; }
        [Column("from_date")]
        public DateTime FromDate { get; set; }
        [Column("to_date")]
        public DateTime? ToDate { get; set; }
        [Column("id_location")]
        public int IdLocation { get; set; }
        [Column("id_pair_tracker")]
        public int? IdPairTracker { get; set; }

        [ForeignKey(nameof(IdCode))]
        [InverseProperty(nameof(Code.Group))]
        public virtual Code IdCodeNavigation { get; set; }
        [ForeignKey(nameof(IdLink))]
        [InverseProperty(nameof(Link.Group))]
        public virtual Link IdLinkNavigation { get; set; }
        [ForeignKey(nameof(IdLocation))]
        [InverseProperty(nameof(Location.Group))]
        public virtual Location IdLocationNavigation { get; set; }
        [ForeignKey(nameof(IdPairTracker))]
        [InverseProperty(nameof(PairTracker.Group))]
        public virtual PairTracker IdPairTrackerNavigation { get; set; }
        [ForeignKey(nameof(IdProduct))]
        [InverseProperty(nameof(Product.Group))]
        public virtual Product IdProductNavigation { get; set; }
        [InverseProperty("IdGroupNavigation")]
        public virtual ICollection<Log> Log { get; set; }
    }
}
