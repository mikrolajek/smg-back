using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("code")]
    public partial class Code
    {
        public Code()
        {
            Group = new HashSet<Group>();
            PairTrackerPair1Navigation = new HashSet<PairTracker>();
            PairTrackerPair2Navigation = new HashSet<PairTracker>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("type")]
        public string Type { get; set; }
        [Required]
        [Column("uid")]
        public string Uid { get; set; }
        [Column("taken")]
        public bool Taken { get; set; }

        [InverseProperty("IdCodeNavigation")]
        public virtual ICollection<Group> Group { get; set; }
        [InverseProperty(nameof(PairTracker.Pair1Navigation))]
        public virtual ICollection<PairTracker> PairTrackerPair1Navigation { get; set; }
        [InverseProperty(nameof(PairTracker.Pair2Navigation))]
        public virtual ICollection<PairTracker> PairTrackerPair2Navigation { get; set; }
    }
}
