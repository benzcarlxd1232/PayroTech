using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayroTech.Models.Entities;

public class Branch : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string BranchName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Province { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string ContactNumber { get; set; } = string.Empty;
    
    public bool IsMainBranch { get; set; } = false;
    
    // Hide BaseEntity.IsActive with a branch-specific flag as intended
    public new bool IsActive { get; set; } = true;
    
    // Foreign Key
    public int CompanyId { get; set; }
    
    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; } = null!;
    
    // Navigation
    public virtual ICollection<ApplicationUser> Employees { get; set; } = new List<ApplicationUser>();
}
