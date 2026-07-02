using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebAppTest.Data;

namespace WebAppTest.DTOs
{
	public class TagCheckBox 
	{
		public SelectList TagFromDb {  get; set; }
		public int SelectedTagId { get; set; }
	}
}
