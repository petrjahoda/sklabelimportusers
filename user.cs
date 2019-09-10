using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace sklabelimportusers {
    [Table("user")]
    public class user {
        [Key] public int OID { get; set; }
        [Required] public string Login { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
        public string Rfid { get; set; }
        public string Barcode { get; set; }
        public string Pin { get; set; }
        [Required] public int UserRoleId { get; set; }
    }
}