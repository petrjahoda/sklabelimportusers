using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace linaplastimportusers {
    [Table("user")]
    public class user {
        [Key] public int OID { get; set; }
        [Required] public string Login { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
        public string Rfid { get; set; }
        [Required] public int UserRoleId { get; set; }
    }
}