using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.DTOs
{
    public class CourseDetails
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public SimpleInstructor Instructor { get; set; }
    }

    public class SimpleInstructor
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }

}
