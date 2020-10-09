using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiRedirector.Models
{
    [Table("pair_tracker")]
    public partial class PairTracker
    {
        public PairTracker()
        {
            Group = new HashSet<Group>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("pair1")]
        public int Pair1 { get; set; }
        [Column("pair2")]
        public int Pair2 { get; set; }

        [ForeignKey(nameof(Pair1))]
        [InverseProperty(nameof(Code.PairTrackerPair1Navigation))]
        public virtual Code Pair1Navigation { get; set; }
        [ForeignKey(nameof(Pair2))]
        [InverseProperty(nameof(Code.PairTrackerPair2Navigation))]
        public virtual Code Pair2Navigation { get; set; }
        [InverseProperty("IdPairTrackerNavigation")]
        public virtual ICollection<Group> Group { get; set; }
    }
}
