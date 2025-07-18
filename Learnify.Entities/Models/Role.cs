using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Entities.Models;

public class Role
{
    [Key]
    public int RoleId { get; set; }  // معرف الدور

    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = string.Empty;  // اسم الدور (مثل Admin, Instructor, Student)

    // يمكن إضافة علاقة عكسية إذا كنت ترغب في استخدام Role في جداول أخرى مثل User
    public ICollection<User> Users { get; set; } = new List<User>();
}
