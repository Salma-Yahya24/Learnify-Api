using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Learnify.Entities.Models;

public class Gender
{
    [Key]
    public int GenderId { get; set; }  // يتم تمثيل الـ Gender باستخدام Id

    [Required]
    [StringLength(50)]
    public string GenderName { get; set; } = string.Empty; // اسم الـ Gender (مثل Male, Female)

    // يمكن إضافة علاقة عكسية إذا كنت ترغب في استخدام Gender في جداول أخرى مثل User
    public ICollection<User> Users { get; set; } = new List<User>();
}
