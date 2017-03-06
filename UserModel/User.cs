using System;
using System.ComponentModel.DataAnnotations;

namespace TestTask.Models
{
    public class User : IEquatable<User>
    {
        public User() { }

        [Key]
        [StringLength(20)]
        public string NickName { get; set; }

        public string UserName { get; set; }

        #region IEquatable

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as User);
        }

        public bool Equals(User other)
        {
            if (other == null) return false;
            return NickName == other.NickName && UserName == other.UserName; 
        }
    
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + NickName.GetHashCode();
            hash = hash * 23 + UserName.GetHashCode();
            return hash;
        }

        #endregion  
    }
}
