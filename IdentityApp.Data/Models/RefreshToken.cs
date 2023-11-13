using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApp.Data.Models
{
    public class RefreshToken
    {
        #region scalar
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Token { get; set; }

        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        public DateTime DateExpiresUtc { get; set; }

        public bool IsExpired => DateTime.UtcNow >= DateExpiresUtc; //Token bitiş tarihi Bugünün tarihinden büyük ya da eşit ise süresi dolmuştur;değilse dolmamıştır.

        public bool IsActive => !IsActive;

        #endregion

        #region navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        #endregion

    }
}
